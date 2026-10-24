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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class CircuitBreakerControllerTests
{
    private readonly Mock<ICircuitBreakerRegistry> _registry = new();

    private CircuitBreakerController CreateSut() =>
        new(_registry.Object, NullLogger<CircuitBreakerController>.Instance);

    [Fact]
    public void Constructor_NullRegistry_ThrowsArgumentNullException()
    {
        Action act = () => new CircuitBreakerController(null!, NullLogger<CircuitBreakerController>.Instance);

        act.Should().Throw<ArgumentNullException>().WithParameterName("registry");
    }

    [Fact]
    public void GetAll_MixedStates_ReturnsStateNamesKeyedByServiceId()
    {
        _registry.Setup(r => r.GetAllStates()).Returns(new Dictionary<int, CircuitBreakerState>
        {
            [1] = CircuitBreakerState.Closed,
            [2] = CircuitBreakerState.Open,
            [3] = CircuitBreakerState.HalfOpen
        });

        var result = CreateSut().GetAll();

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new Dictionary<int, string>
            {
                [1] = "Closed",
                [2] = "Open",
                [3] = "HalfOpen"
            });
    }

    [Fact]
    public void GetAll_NoBreakersRegistered_ReturnsOkWithEmptyDictionary()
    {
        _registry.Setup(r => r.GetAllStates()).Returns(new Dictionary<int, CircuitBreakerState>());

        var result = CreateSut().GetAll();

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeAssignableTo<Dictionary<int, string>>()
            .Which.Should().BeEmpty();
    }

    [Fact]
    public void GetStatus_RegisteredService_ReturnsOkWithStateFailuresAndOpenedAt()
    {
        var openedAt = new DateTime(2026, 10, 1, 12, 30, 0, DateTimeKind.Utc);
        var breaker = new Mock<ICircuitBreaker>();
        breaker.SetupGet(b => b.State).Returns(CircuitBreakerState.HalfOpen);
        breaker.SetupGet(b => b.ConsecutiveFailures).Returns(3);
        breaker.SetupGet(b => b.OpenedAt).Returns(openedAt);
        _registry.Setup(r => r.TryGet(7)).Returns(breaker.Object);

        var result = CreateSut().GetStatus(7);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new
            {
                serviceId = 7,
                state = "HalfOpen",
                consecutiveFailures = 3,
                openedAt
            });
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void GetStatus_UnregisteredServiceId_ReturnsNotFoundWithMessage(int serviceId)
    {
        _registry.Setup(r => r.TryGet(serviceId)).Returns((ICircuitBreaker?)null);

        var result = CreateSut().GetStatus(serviceId);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().Be($"No circuit breaker registered for service {serviceId}");
    }

    [Fact]
    public void Reset_RegisteredService_ResetsBreakerAndReturnsClosedState()
    {
        var breaker = new Mock<ICircuitBreaker>();
        _registry.Setup(r => r.TryGet(5)).Returns(breaker.Object);

        var result = CreateSut().Reset(5);

        _registry.Verify(r => r.Reset(5), Times.Once);
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new { serviceId = 5, state = "Closed" });
    }

    [Fact]
    public void Reset_UnregisteredService_ReturnsNotFoundWithoutResetting()
    {
        _registry.Setup(r => r.TryGet(42)).Returns((ICircuitBreaker?)null);

        var result = CreateSut().Reset(42);

        _registry.Verify(r => r.Reset(It.IsAny<int>()), Times.Never);
        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().Be("No circuit breaker registered for service 42");
    }
}
