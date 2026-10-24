#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using DotNetGrpcGateway.Controllers;
using DotNetGrpcGateway.Infrastructure;
using DotNetGrpcGateway.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace DotNetGrpcGateway.Tests;

/// <summary>
/// Contains unit tests for <see cref="CircuitBreakerControllerExtensions"/> covering status, metrics,
/// reset and faulted-breaker queries against a mocked circuit breaker registry.
/// </summary>
public class CircuitBreakerControllerExtensionsTests
{
    private readonly Mock<ICircuitBreakerRegistry> _registry = new();

    private CircuitBreakerController CreateSut()
    {
        // The extensions resolve the registry from HttpContext.RequestServices, so the controller needs a real context.
        var services = new ServiceCollection().AddSingleton(_registry.Object).BuildServiceProvider();
        return new CircuitBreakerController(_registry.Object, NullLogger<CircuitBreakerController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { RequestServices = services } }
        };
    }

    private static Mock<ICircuitBreaker> CreateBreaker(CircuitBreakerState state, int failures = 0, DateTime? openedAt = null)
    {
        var breaker = new Mock<ICircuitBreaker>();
        breaker.SetupGet(b => b.State).Returns(state);
        breaker.SetupGet(b => b.ConsecutiveFailures).Returns(failures);
        breaker.SetupGet(b => b.OpenedAt).Returns(openedAt);
        return breaker;
    }

    private static IReadOnlyDictionary<int, object> ValueOf(ActionResult<IReadOnlyDictionary<int, object>> result) =>
        (IReadOnlyDictionary<int, object>)result.Result.Should().BeOfType<OkObjectResult>().Which.Value!;

    [Fact]
    public void GetStatuses_MixedRegisteredAndUnregisteredIds_ReturnsOnlyRegisteredStatuses()
    {
        var openedAt = new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);
        _registry.Setup(r => r.TryGet(1)).Returns(CreateBreaker(CircuitBreakerState.Open, 5, openedAt).Object);
        _registry.Setup(r => r.TryGet(2)).Returns((ICircuitBreaker?)null);

        var result = ValueOf(CreateSut().GetStatuses(new[] { 1, 2 }));

        result.Should().HaveCount(1);
        result[1].Should().BeOfType<CircuitBreakerControllerExtensions.CircuitBreakerStatus>().Which.Should().BeEquivalentTo(
            new CircuitBreakerControllerExtensions.CircuitBreakerStatus
            {
                ServiceId = 1,
                State = "Open",
                ConsecutiveFailures = 5,
                OpenedAt = openedAt
            });
    }

    [Fact]
    public void GetStatuses_EmptyServiceIds_ReturnsEmptyDictionaryWithoutQueryingRegistry()
    {
        var result = ValueOf(CreateSut().GetStatuses(Array.Empty<int>()));

        result.Should().BeEmpty();
        _registry.Verify(r => r.TryGet(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void GetAllWithMetrics_MixedStates_FlagsOnlyHalfOpenBreakers()
    {
        _registry.Setup(r => r.GetAllStates()).Returns(new Dictionary<int, CircuitBreakerState>
        {
            [1] = CircuitBreakerState.Closed,
            [2] = CircuitBreakerState.HalfOpen
        });
        _registry.Setup(r => r.TryGet(1)).Returns(CreateBreaker(CircuitBreakerState.Closed).Object);
        _registry.Setup(r => r.TryGet(2)).Returns(CreateBreaker(CircuitBreakerState.HalfOpen, 2).Object);

        var result = ValueOf(CreateSut().GetAllWithMetrics());

        result.Should().HaveCount(2);
        result[1].Should().BeOfType<CircuitBreakerControllerExtensions.CircuitBreakerStatusWithMetrics>()
            .Which.IsHalfOpen.Should().BeFalse();
        result[2].Should().BeOfType<CircuitBreakerControllerExtensions.CircuitBreakerStatusWithMetrics>().Which.Should().BeEquivalentTo(
            new CircuitBreakerControllerExtensions.CircuitBreakerStatusWithMetrics
            {
                ServiceId = 2,
                State = "HalfOpen",
                ConsecutiveFailures = 2,
                IsHalfOpen = true
            });
    }

    [Fact]
    public void ResetMultiple_MixedRegistration_ResetsRegisteredAndReportsErrorForUnknown()
    {
        _registry.Setup(r => r.TryGet(1)).Returns(CreateBreaker(CircuitBreakerState.Open).Object);
        _registry.Setup(r => r.TryGet(2)).Returns((ICircuitBreaker?)null);

        var result = ValueOf(CreateSut().ResetMultiple(new[] { 1, 2 }));

        _registry.Verify(r => r.Reset(1), Times.Once);
        _registry.Verify(r => r.Reset(2), Times.Never);
        result[1].Should().BeOfType<CircuitBreakerControllerExtensions.CircuitBreakerResetResult>()
            .Which.State.Should().Be("Closed");
        result[2].Should().BeOfType<CircuitBreakerControllerExtensions.CircuitBreakerErrorResult>()
            .Which.Error.Should().Be("No circuit breaker registered for service 2");
    }

    [Fact]
    public void GetFaulted_MixedStates_ReturnsOnlyOpenBreakers()
    {
        var openedAt = new DateTime(2026, 10, 2, 8, 0, 0, DateTimeKind.Utc);
        _registry.Setup(r => r.GetAllStates()).Returns(new Dictionary<int, CircuitBreakerState>
        {
            [1] = CircuitBreakerState.Closed,
            [2] = CircuitBreakerState.Open,
            [3] = CircuitBreakerState.HalfOpen
        });
        _registry.Setup(r => r.TryGet(1)).Returns(CreateBreaker(CircuitBreakerState.Closed).Object);
        _registry.Setup(r => r.TryGet(2)).Returns(CreateBreaker(CircuitBreakerState.Open, 4, openedAt).Object);
        _registry.Setup(r => r.TryGet(3)).Returns(CreateBreaker(CircuitBreakerState.HalfOpen).Object);

        var result = ValueOf(CreateSut().GetFaulted());

        result.Should().HaveCount(1);
        result[2].Should().BeOfType<CircuitBreakerControllerExtensions.CircuitBreakerStatus>().Which.Should().BeEquivalentTo(
            new CircuitBreakerControllerExtensions.CircuitBreakerStatus
            {
                ServiceId = 2,
                State = "Open",
                ConsecutiveFailures = 4,
                OpenedAt = openedAt
            });
    }

    [Fact]
    public void GetAllWithMetricsAndGetFaulted_NoBreakersRegistered_ReturnEmptyDictionaries()
    {
        _registry.Setup(r => r.GetAllStates()).Returns(new Dictionary<int, CircuitBreakerState>());

        ValueOf(CreateSut().GetAllWithMetrics()).Should().BeEmpty();
        ValueOf(CreateSut().GetFaulted()).Should().BeEmpty();
    }

    [Fact]
    public void Methods_NullArguments_ThrowArgumentNullException()
    {
        var sut = CreateSut();
        CircuitBreakerController? nullController = null;

        Action nullControllerStatuses = () => nullController!.GetStatuses(new[] { 1 });
        Action nullControllerMetrics = () => nullController!.GetAllWithMetrics();
        Action nullControllerReset = () => nullController!.ResetMultiple(new[] { 1 });
        Action nullControllerFaulted = () => nullController!.GetFaulted();
        Action nullStatusIds = () => sut.GetStatuses(null!);
        Action nullResetIds = () => sut.ResetMultiple(null!);

        nullControllerStatuses.Should().Throw<ArgumentNullException>().WithParameterName("controller");
        nullControllerMetrics.Should().Throw<ArgumentNullException>().WithParameterName("controller");
        nullControllerReset.Should().Throw<ArgumentNullException>().WithParameterName("controller");
        nullControllerFaulted.Should().Throw<ArgumentNullException>().WithParameterName("controller");
        nullStatusIds.Should().Throw<ArgumentNullException>().WithParameterName("serviceIds");
        nullResetIds.Should().Throw<ArgumentNullException>().WithParameterName("serviceIds");
    }
}
