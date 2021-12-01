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

namespace DotNetGrpcGateway.Tests;

public class RouteManagementServiceTests
{
    private readonly Mock<IRouteRepository> _routeRepo = new();
    private readonly Mock<IEventPublisher> _eventPublisher = new();
    private readonly Mock<ILogger<RouteManagementService>> _logger = new();

    private RouteManagementService CreateSut() =>
        new(_routeRepo.Object, _eventPublisher.Object, _logger.Object);

    private static GatewayRoute BuildRoute(int id, string pattern, int serviceId, int priority = 100) => new()
    {
        Id = id,
        Pattern = pattern,
        TargetServiceId = serviceId,
        Priority = priority,
        IsActive = true,
        RateLimitPerMinute = 100
    };

    [Fact]
    public async Task ValidateRouteAsync_EmptyPattern_ReturnsFalse()
    {
        var route = BuildRoute(0, string.Empty, 1);
        route.Pattern = string.Empty;
        var sut = CreateSut();

        var result = await sut.ValidateRouteAsync(route);

        result.Should().BeFalse();
    }

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
        result.First().Priority.Should().Be(200, "results must be ordered by priority descending");
    }
}
