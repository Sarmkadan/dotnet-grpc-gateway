#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Events;
using DotNetGrpcGateway.Infrastructure;
using DotNetGrpcGateway.Services;
using Xunit;

/// <summary>
/// Tests for the RouteManagementService class.
/// </summary>
namespace DotNetGrpcGateway.Tests;

public class RouteManagementServiceTests
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RouteManagementServiceTests"/> class.
    /// </summary>
    private readonly Mock<IRouteRepository> _routeRepo = new();
    private readonly Mock<IEventPublisher> _eventPublisher = new();
    private readonly Mock<ILogger<RouteManagementService>> _logger = new();

    /// <summary>
    /// Creates a new instance of the RouteManagementService class.
    /// </summary>
    /// <returns>A new instance of the RouteManagementService class.</returns>
    private RouteManagementService CreateSut() =>
        new(_routeRepo.Object, _eventPublisher.Object, _logger.Object);

    /// <summary>
    /// Builds a new GatewayRoute instance.
    /// </summary>
    /// <param name="id">The ID of the route.</param>
    /// <param name="pattern">The pattern of the route.</param>
    /// <param name="serviceId">The ID of the service.</param>
    /// <param name="priority">The priority of the route (default is 100).</param>
    /// <returns>A new GatewayRoute instance.</returns>
    private static GatewayRoute BuildRoute(int id, string pattern, int serviceId, int priority = 100) => new()
    {
        Id = id,
        Pattern = pattern,
        TargetServiceId = serviceId,
        Priority = priority,
        IsActive = true,
        RateLimitPerMinute = 100
    };

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task FindMatchingRouteAsync_NullOrEmptyPath_ReturnsNull(string? path)
    {
        var sut = CreateSut();

        var result = await sut.FindMatchingRouteAsync(path!);

        result.Should().BeNull();
        _routeRepo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task FindMatchingRouteAsync_MultipleWildcardMatches_ReturnsHighestPriorityActiveRoute()
    {
        var lowPriority = BuildRoute(1, "UserService.*", serviceId: 1, priority: 100);
        var highPriority = BuildRoute(2, "*.GetUser", serviceId: 2, priority: 300);
        var inactive = BuildRoute(3, "*", serviceId: 3, priority: 500);
        inactive.IsActive = false;
        _routeRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GatewayRoute> { lowPriority, inactive, highPriority });
        var sut = CreateSut();

        var result = await sut.FindMatchingRouteAsync("UserService.GetUser");

        result.Should().BeSameAs(highPriority);
    }

    [Fact]
    public async Task FindMatchingRouteAsync_RepositoryThrows_ReturnsNull()
    {
        _routeRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Repository unavailable"));
        var sut = CreateSut();

        var result = await sut.FindMatchingRouteAsync("UserService.GetUser");

        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that an empty pattern returns false.
    /// </summary>
    /// <returns>A task that represents the test.</returns>
    [Fact]
    public async Task ValidateRouteAsync_EmptyPattern_ReturnsFalse()
    {
        var route = BuildRoute(0, string.Empty, 1);
        route.Pattern = string.Empty;
        var sut = CreateSut();

        var result = await sut.ValidateRouteAsync(route);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that an empty pattern logs a warning.
    /// </summary>
    /// <returns>A task that represents the test.</returns>
    [Fact]
    public async Task ValidateRouteAsync_EmptyPattern_LogsWarning()
    {
        var route = BuildRoute(1, string.Empty, 1);
        var sut = CreateSut();

        await sut.ValidateRouteAsync(route);

        _logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Route validation failed - Pattern is empty (RouteId: 1)")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that a duplicate pattern in the repository returns false.
    /// </summary>
    /// <returns>A task that represents the test.</returns>
    [Fact]
    public async Task ValidateRouteAsync_DuplicatePatternInRepository_ReturnsFalse()
    {
        var existingRoute = BuildRoute(99, "UserService.GetUser", serviceId: 1);
        _routeRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<GatewayRoute> { existingRoute });

        var newRoute = BuildRoute(0, "UserService.GetUser", serviceId: 1);
        var sut = CreateSut();

        var result = await sut.ValidateRouteAsync(newRoute);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that a unique valid route returns true.
    /// </summary>
    /// <returns>A task that represents the test.</returns>
    [Fact]
    public async Task ValidateRouteAsync_UniqueValidRoute_ReturnsTrue()
    {
        _routeRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<GatewayRoute>());

        var route = BuildRoute(1, "OrderService.PlaceOrder", serviceId: 2);
        var sut = CreateSut();

        var result = await sut.ValidateRouteAsync(route);

        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that getting routes by service returns only matching routes.
    /// </summary>
    /// <returns>A task that represents the test.</returns>
    [Fact]
    public async Task GetRoutesByServiceAsync_MultipleServices_ReturnsOnlyMatchingRoutes()
    {
        var routes = new List<GatewayRoute>
        {
            BuildRoute(1, "UserService.GetUser", serviceId: 1, priority: 200),
            BuildRoute(2, "UserService.ListUsers", serviceId: 1, priority: 100),
            BuildRoute(3, "OrderService.PlaceOrder", serviceId: 2, priority: 150)
        };
        _routeRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(routes);

        var sut = CreateSut();
        var result = await sut.GetRoutesByServiceAsync(serviceId: 1);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.TargetServiceId == 1);
        result.Select(r => r.Priority).Should().Equal(200, 100);
    }

    [Fact]
    public async Task GetRoutesByServiceAsync_RepositoryThrows_ReturnsEmptyList()
    {
        _routeRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Repository unavailable"));
        var sut = CreateSut();

        var result = await sut.GetRoutesByServiceAsync(serviceId: 1);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_NullRouteRepository_ThrowsArgumentNullException()
    {
        var act = () => new RouteManagementService(null!, _eventPublisher.Object, _logger.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("routeRepository");
    }

    [Fact]
    public void Constructor_NullEventPublisher_ThrowsArgumentNullException()
    {
        var act = () => new RouteManagementService(_routeRepo.Object, null!, _logger.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("eventPublisher");
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new RouteManagementService(_routeRepo.Object, _eventPublisher.Object, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }
}
