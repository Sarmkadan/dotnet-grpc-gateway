#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetGrpcGateway.Controllers;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class GatewayControllerTests
{
    private readonly Mock<IGatewayService> _gateway = new();
    private readonly Mock<IMetricsCollectionService> _metrics = new();

    private GatewayController CreateSut() =>
        new(_gateway.Object, _metrics.Object, NullLogger<GatewayController>.Instance);

    [Fact]
    public void Constructor_NullDependency_ThrowsArgumentNullException()
    {
        var logger = NullLogger<GatewayController>.Instance;
        Action nullGateway = () => new GatewayController(null!, _metrics.Object, logger);
        Action nullMetrics = () => new GatewayController(_gateway.Object, null!, logger);
        Action nullLogger = () => new GatewayController(_gateway.Object, _metrics.Object, null!);

        nullGateway.Should().Throw<ArgumentNullException>().WithParameterName("gatewayService");
        nullMetrics.Should().Throw<ArgumentNullException>().WithParameterName("metricsService");
        nullLogger.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public async Task GetAndUpdateConfiguration_ServiceResults_ReturnOkWithResult()
    {
        var current = new GatewayConfiguration { Name = "current" };
        var updated = new GatewayConfiguration { Name = "updated" };
        _gateway.Setup(s => s.GetConfigurationAsync()).ReturnsAsync(current);
        _gateway.Setup(s => s.UpdateConfigurationAsync(updated)).ReturnsAsync(updated);
        var sut = CreateSut();

        var get = await sut.GetConfiguration();
        var put = await sut.UpdateConfiguration(updated);

        get.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(current);
        put.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(updated);
    }

    [Fact]
    public async Task NullBodies_UpdateConfigurationRegisterServiceAndAddRoute_ReturnBadRequestWithoutCallingService()
    {
        var sut = CreateSut();

        var config = await sut.UpdateConfiguration(null!);
        var service = await sut.RegisterService(null!);
        var route = await sut.AddRoute(null!);

        config.Result.Should().BeOfType<BadRequestObjectResult>();
        service.Should().BeOfType<BadRequestObjectResult>();
        route.Should().BeOfType<BadRequestObjectResult>();
        _gateway.Verify(s => s.UpdateConfigurationAsync(It.IsAny<GatewayConfiguration>()), Times.Never);
        _gateway.Verify(s => s.RegisterServiceAsync(It.IsAny<GrpcService>()), Times.Never);
        _gateway.Verify(s => s.AddRouteAsync(It.IsAny<GatewayRoute>()), Times.Never);
    }

    [Fact]
    public async Task ListEndpoints_EmptyHealthyAndRoutes_ReturnOkWithExpectedCollections()
    {
        _gateway.Setup(s => s.GetAllServicesAsync()).ReturnsAsync(new List<GrpcService> { new() { Name = "orders" } });
        _gateway.Setup(s => s.GetHealthyServicesAsync()).ReturnsAsync(new List<GrpcService>());
        _gateway.Setup(s => s.GetAllRoutesAsync()).ReturnsAsync(new List<GatewayRoute>());
        var sut = CreateSut();

        var services = await sut.GetServices();
        var healthy = await sut.GetHealthyServices();
        var routes = await sut.GetRoutes();

        services.Result.Should().BeOfType<OkObjectResult>().Which.Value
            .Should().BeOfType<List<GrpcService>>().Which.Should().ContainSingle();
        healthy.Result.Should().BeOfType<OkObjectResult>().Which.Value
            .Should().BeOfType<List<GrpcService>>().Which.Should().BeEmpty();
        routes.Result.Should().BeOfType<OkObjectResult>().Which.Value
            .Should().BeOfType<List<GatewayRoute>>().Which.Should().BeEmpty();
    }

    [Fact]
    public async Task GetService_ExistingAndUnknownId_ReturnsOkOrNotFound()
    {
        var orders = new GrpcService { Id = 1, Name = "orders" };
        _gateway.Setup(s => s.GetServiceAsync(1)).ReturnsAsync(orders);
        _gateway.Setup(s => s.GetServiceAsync(42)).ThrowsAsync(new KeyNotFoundException());
        var sut = CreateSut();

        var found = await sut.GetService(1);
        var missing = await sut.GetService(42);

        found.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(orders);
        missing.Result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().Be("Service with ID 42 not found");
    }

    [Fact]
    public async Task RegisterServiceAndAddRoute_ValidInput_ReturnsCreatedWithLocation()
    {
        var service = new GrpcService { Id = 5, Name = "orders" };
        var route = new GatewayRoute { Pattern = "/orders.*", TargetServiceId = 5 };
        _gateway.Setup(s => s.AddRouteAsync(route))
            .ReturnsAsync(new GatewayRoute { Id = 9, Pattern = route.Pattern, TargetServiceId = 5 });
        var sut = CreateSut();

        var registered = await sut.RegisterService(service);
        var added = await sut.AddRoute(route);

        registered.Should().BeOfType<CreatedResult>().Which.Location.Should().Be("/api/gateway/services/5");
        added.Should().BeOfType<CreatedResult>().Which.Location.Should().Be("/api/gateway/routes/9");
    }

    [Fact]
    public async Task RegisterService_DuplicateName_ReturnsBadRequestWithServiceMessage()
    {
        _gateway.Setup(s => s.RegisterServiceAsync(It.IsAny<GrpcService>()))
            .ThrowsAsync(new InvalidOperationException("Service 'orders' already registered"));
        var sut = CreateSut();

        var result = await sut.RegisterService(new GrpcService { Name = "orders" });

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Service 'orders' already registered");
    }

    [Fact]
    public async Task UnregisterService_SuccessAndFailure_ReturnsNoContentOrNotFound()
    {
        _gateway.Setup(s => s.UnregisterServiceAsync(7))
            .ThrowsAsync(new InvalidOperationException("Service 7 is missing"));
        var sut = CreateSut();

        var removed = await sut.UnregisterService(3);
        var failed = await sut.UnregisterService(7);

        removed.Should().BeOfType<NoContentResult>();
        failed.Should().BeOfType<NotFoundObjectResult>().Which.Value.Should().Be("Service 7 is missing");
    }
}
