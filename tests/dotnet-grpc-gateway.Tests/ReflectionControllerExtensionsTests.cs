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
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace DotNetGrpcGateway.Tests;

/// <summary>
/// Contains unit tests for <see cref="ReflectionControllerExtensions"/> covering health summary retrieval,
/// service counting and availability checks against a real <see cref="ReflectionController"/>.
/// </summary>
public class ReflectionControllerExtensionsTests
{
    [Fact]
    public async Task Extensions_NullController_ThrowsArgumentNullException()
    {
        Func<Task> health = () => ((ReflectionController)null!).GetHealthSummaryAsync(CancellationToken.None);
        Func<Task> count = () => ((ReflectionController)null!).GetAvailableServiceCountAsync(CancellationToken.None);
        Func<Task> any = () => ((ReflectionController)null!).IsAnyServiceAvailableAsync(CancellationToken.None);

        await health.Should().ThrowAsync<ArgumentNullException>().WithParameterName("controller");
        await count.Should().ThrowAsync<ArgumentNullException>().WithParameterName("controller");
        await any.Should().ThrowAsync<ArgumentNullException>().WithParameterName("controller");
    }

    [Fact]
    public async Task GetHealthSummaryAsync_PartialAvailability_ReturnsOkSummaryWithCounts()
    {
        // Arrange
        var sut = CreateSut(ServiceReturning(CreateInfos(total: 2, available: 1)));

        // Act
        var summary = await sut.GetHealthSummaryAsync(CancellationToken.None);

        // Assert
        summary.IsAvailable.Should().BeTrue();
        summary.AvailableServiceCount.Should().Be(1);
        summary.TotalServiceCount.Should().Be(2);
        summary.Message.Should().Be("1/2 service(s) have reflection enabled");
    }

    [Fact]
    public async Task GetHealthSummaryAsync_NoneAvailable_ReturnsUnavailableSummaryWithoutThrowing()
    {
        // Arrange
        var sut = CreateSut(ServiceReturning(CreateInfos(total: 1, available: 0)));

        // Act
        var summary = await sut.GetHealthSummaryAsync(CancellationToken.None);

        // Assert
        summary.IsAvailable.Should().BeFalse();
        summary.AvailableServiceCount.Should().Be(0);
        summary.TotalServiceCount.Should().Be(1);
    }

    [Fact]
    public async Task GetHealthSummaryAsync_ProbeThrows_ReturnsUnavailableSummaryWithExceptionMessage()
    {
        // Arrange
        var failing = new Mock<IReflectionService>();
        failing.Setup(s => s.GetAllReflectionInfoAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("registry offline"));
        var sut = CreateSut(failing.Object);

        // Act
        var summary = await sut.GetHealthSummaryAsync(CancellationToken.None);

        // Assert
        summary.IsAvailable.Should().BeFalse();
        summary.Message.Should().Be("registry offline");
        summary.CheckedAt.Should().NotBe(default);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(3, 0)]
    [InlineData(3, 1)]
    [InlineData(3, 3)]
    public async Task GetAvailableServiceCountAsync_Boundaries_MatchesAvailableCountAndAnyFlag(int total, int available)
    {
        // Arrange
        var sut = CreateSut(ServiceReturning(CreateInfos(total, available)));

        // Act
        var count = await sut.GetAvailableServiceCountAsync(CancellationToken.None);
        var any = await sut.IsAnyServiceAvailableAsync(CancellationToken.None);

        // Assert
        count.Should().Be(available);
        any.Should().Be(available > 0);
    }

    private static ReflectionController CreateSut(IReflectionService service) =>
        new(service, NullLogger<ReflectionController>.Instance);

    private static IReflectionService ServiceReturning(IReadOnlyList<ServiceReflectionInfo> infos)
    {
        var mock = new Mock<IReflectionService>();
        mock.Setup(s => s.GetAllReflectionInfoAsync(It.IsAny<CancellationToken>())).ReturnsAsync(infos);
        return mock.Object;
    }

    private static List<ServiceReflectionInfo> CreateInfos(int total, int available) =>
        Enumerable.Range(1, total)
            .Select(i => new ServiceReflectionInfo
            {
                ServiceId = i,
                ServiceName = $"Svc{i}",
                ServiceFullName = $"pkg.Svc{i}",
                IsAvailable = i <= available
            })
            .ToList();
}
