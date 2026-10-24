#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Controllers;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace DotNetGrpcGateway.Tests;

/// <summary>
/// Contains unit tests for <see cref="ReflectionController"/> covering lookups, refresh and health endpoints.
/// </summary>
public class ReflectionControllerTests
{
    [Fact]
    public void Constructor_NullDependency_ThrowsArgumentNullException()
    {
        Action nullService = () => new ReflectionController(null!, NullLogger<ReflectionController>.Instance);
        nullService.Should().ThrowExactly<ArgumentNullException>().WithParameterName("reflectionService");

        Action nullLogger = () => new ReflectionController(new Mock<IReflectionService>().Object, null!);
        nullLogger.Should().ThrowExactly<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public async Task GetAllReflection_CachedServices_ReturnsOkWithSameSnapshot()
    {
        // Arrange
        var infos = new List<ServiceReflectionInfo> { CreateInfo(1, true), CreateInfo(2, false) };
        var sut = CreateSut(ServiceReturning(infos));

        var result = await sut.GetAllReflection(CancellationToken.None);
        result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(infos);
    }

    [Fact]
    public async Task GetServiceReflection_KnownAndUnknownIds_ReturnsOkOrNotFound()
    {
        // Arrange
        var info = CreateInfo(1, true);
        var mock = new Mock<IReflectionService>();
        mock.Setup(s => s.GetServiceReflectionAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(info);
        mock.Setup(s => s.GetServiceReflectionAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceReflectionInfo?)null);
        var sut = CreateSut(mock.Object);

        // Act
        var found = await sut.GetServiceReflection(1, CancellationToken.None);
        var missing = await sut.GetServiceReflection(99, CancellationToken.None);

        found.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(info);
        missing.Result.Should().BeOfType<NotFoundObjectResult>().Which.Value.Should().BeOfType<string>()
            .Which.Should().Contain("service 99");
    }

    [Fact]
    public async Task GetServiceMethods_ServiceWithAndWithoutMethods_ReturnsListOrNotFound()
    {
        // Arrange
        var methods = new List<ServiceMethodDescriptor>
        {
            new() { Name = "Get", RequestType = "pkg.Req", ResponseType = "pkg.Resp" }
        };
        var withMethods = CreateInfo(1, true, methods.ToArray());
        var noMethods = CreateInfo(2, true);
        var mock = new Mock<IReflectionService>();
        mock.Setup(s => s.GetServiceReflectionAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(withMethods);
        mock.Setup(s => s.GetServiceReflectionAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(noMethods);
        mock.Setup(s => s.GetServiceReflectionAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceReflectionInfo?)null);
        var sut = CreateSut(mock.Object);

        // Act
        var populated = await sut.GetServiceMethods(1, CancellationToken.None);
        var empty = await sut.GetServiceMethods(2, CancellationToken.None);
        var missing = await sut.GetServiceMethods(99, CancellationToken.None);

        populated.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(withMethods.Methods);
        empty.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeAssignableTo<List<ServiceMethodDescriptor>>()
            .Which.Should().BeEmpty();
        missing.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task RefreshServiceReflection_KnownAndUnknownIds_ReturnsOkOrNotFound()
    {
        // Arrange
        var refreshed = CreateInfo(1, true);
        var mock = new Mock<IReflectionService>();
        mock.Setup(s => s.RefreshServiceReflectionAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(refreshed);
        mock.Setup(s => s.RefreshServiceReflectionAsync(99, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException());
        var sut = CreateSut(mock.Object);

        // Act
        var ok = await sut.RefreshServiceReflection(1, CancellationToken.None);
        var missing = await sut.RefreshServiceReflection(99, CancellationToken.None);

        ok.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(refreshed);
        missing.Result.Should().BeOfType<NotFoundObjectResult>().Which.Value.Should().Be("Service 99 not found");
    }

    [Fact]
    public async Task RefreshAllReflections_ProbeOutcome_ReturnsAcceptedOr500()
    {
        var succeeding = new Mock<IReflectionService>();
        succeeding.Setup(s => s.RefreshAllReflectionsAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var failing = new Mock<IReflectionService>();
        failing.Setup(s => s.RefreshAllReflectionsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("probe failed"));

        var accepted = await CreateSut(succeeding.Object).RefreshAllReflections(CancellationToken.None);
        var error = await CreateSut(failing.Object).RefreshAllReflections(CancellationToken.None);

        accepted.Should().BeAssignableTo<IStatusCodeActionResult>().Which.StatusCode.Should().Be(202);
        error.Should().BeAssignableTo<IStatusCodeActionResult>().Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetReflectionHealth_PartialAvailabilityAndIdle_ReturnsOkWithCounts()
    {
        // Arrange
        var partial = CreateSut(ServiceReturning(new List<ServiceReflectionInfo> { CreateInfo(1, true), CreateInfo(2, false) }));
        var idle = CreateSut(ServiceReturning(new List<ServiceReflectionInfo>()));

        // Act
        var partialResult = await partial.GetReflectionHealth(CancellationToken.None);
        var idleResult = await idle.GetReflectionHealth(CancellationToken.None);

        // Assert
        var summary = partialResult.Result.Should().BeOfType<OkObjectResult>().Which.Value
            .Should().BeOfType<ReflectionHealthSummary>().Which;
        summary.IsAvailable.Should().BeTrue();
        summary.AvailableServiceCount.Should().Be(1);
        summary.TotalServiceCount.Should().Be(2);
        summary.Message.Should().Be("1/2 service(s) have reflection enabled");

        var idleSummary = idleResult.Result.Should().BeOfType<OkObjectResult>().Which.Value
            .Should().BeOfType<ReflectionHealthSummary>().Which;
        idleSummary.IsAvailable.Should().BeFalse();
        idleSummary.TotalServiceCount.Should().Be(0);
        idleSummary.Message.Should().Be("No services registered; reflection subsystem is idle");
    }

    [Fact]
    public async Task GetReflectionHealth_NoneAvailableOrProbeThrows_Returns503WithUnavailableSummary()
    {
        // Arrange
        var noneAvailable = CreateSut(ServiceReturning(new List<ServiceReflectionInfo> { CreateInfo(1, false) }));
        var throwing = new Mock<IReflectionService>();
        throwing.Setup(s => s.GetAllReflectionInfoAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("registry offline"));

        // Act
        var unavailable = await noneAvailable.GetReflectionHealth(CancellationToken.None);
        var failed = await CreateSut(throwing.Object).GetReflectionHealth(CancellationToken.None);

        // Assert
        var unavailableSummary = unavailable.Result.Should().BeAssignableTo<ObjectResult>().Which.Value
            .Should().BeOfType<ReflectionHealthSummary>().Which;
        unavailable.Result.Should().BeAssignableTo<IStatusCodeActionResult>().Which.StatusCode.Should().Be(503);
        unavailableSummary.IsAvailable.Should().BeFalse();
        unavailableSummary.AvailableServiceCount.Should().Be(0);

        failed.Result.Should().BeAssignableTo<IStatusCodeActionResult>().Which.StatusCode.Should().Be(503);
        failed.Result.Should().BeAssignableTo<ObjectResult>().Which.Value.Should().BeOfType<ReflectionHealthSummary>()
            .Which.Message.Should().Be("registry offline");
    }

    private static ReflectionController CreateSut(IReflectionService service) =>
        new(service, NullLogger<ReflectionController>.Instance);

    private static IReflectionService ServiceReturning(IReadOnlyList<ServiceReflectionInfo> infos)
    {
        var mock = new Mock<IReflectionService>();
        mock.Setup(s => s.GetAllReflectionInfoAsync(It.IsAny<CancellationToken>())).ReturnsAsync(infos);
        return mock.Object;
    }

    private static ServiceReflectionInfo CreateInfo(int serviceId, bool isAvailable, params ServiceMethodDescriptor[] methods) =>
        new()
        {
            ServiceId = serviceId,
            ServiceName = $"Svc{serviceId}",
            ServiceFullName = $"pkg.Svc{serviceId}",
            IsAvailable = isAvailable,
            Methods = methods.ToList()
        };
}
