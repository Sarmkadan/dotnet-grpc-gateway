#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetGrpcGateway.Controllers;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class GatewayControllerExtensionsTests
{
    private static GatewayController CreateController(
        Mock<IGatewayService> gatewayServiceMock,
        Mock<IMetricsCollectionService> metricsServiceMock)
    {
        var loggerMock = new Mock<ILogger<GatewayController>>();
        return new GatewayController(gatewayServiceMock.Object, metricsServiceMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task GetServiceByName_NullController_ThrowsArgumentNullException()
    {
        GatewayController? controller = null;

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => GatewayControllerExtensions.GetServiceByName(controller!, "my-service"));
    }

    [Fact]
    public async Task GetServiceByName_ExistingService_ReturnsOkWithService()
    {
        var gatewayServiceMock = new Mock<IGatewayService>();
        var metricsServiceMock = new Mock<IMetricsCollectionService>();
        var service = new GrpcService
        {
            Id = 1,
            Name = "OrderService",
            ServiceFullName = "orders.OrderService",
            Host = "localhost"
        };
        gatewayServiceMock.Setup(s => s.GetAllServicesAsync())
            .ReturnsAsync(new List<GrpcService> { service });

        var controller = CreateController(gatewayServiceMock, metricsServiceMock);

        var result = await controller.GetServiceByName("orderservice");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsType<GrpcService>(okResult.Value);
        Assert.Equal("OrderService", returned.Name);
    }

    [Fact]
    public async Task GetServiceByName_ServiceNotFound_ReturnsNotFound()
    {
        var gatewayServiceMock = new Mock<IGatewayService>();
        var metricsServiceMock = new Mock<IMetricsCollectionService>();
        gatewayServiceMock.Setup(s => s.GetAllServicesAsync())
            .ReturnsAsync(new List<GrpcService>());

        var controller = CreateController(gatewayServiceMock, metricsServiceMock);

        var result = await controller.GetServiceByName("missing-service");

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GetServiceByName_InvalidServiceName_ReturnsBadRequest(string? serviceName)
    {
        var gatewayServiceMock = new Mock<IGatewayService>();
        var metricsServiceMock = new Mock<IMetricsCollectionService>();
        var controller = CreateController(gatewayServiceMock, metricsServiceMock);

        var result = await controller.GetServiceByName(serviceName!);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task IsServiceHealthy_ServiceIsHealthy_ReturnsOkTrue()
    {
        var gatewayServiceMock = new Mock<IGatewayService>();
        var metricsServiceMock = new Mock<IMetricsCollectionService>();
        var service = new GrpcService
        {
            Id = 2,
            Name = "PaymentService",
            ServiceFullName = "payments.PaymentService",
            Host = "localhost"
        };
        gatewayServiceMock.Setup(s => s.GetHealthyServicesAsync())
            .ReturnsAsync(new List<GrpcService> { service });

        var controller = CreateController(gatewayServiceMock, metricsServiceMock);

        var result = await controller.IsServiceHealthy("PaymentService");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.True((bool)okResult.Value!);
    }

    [Fact]
    public async Task IsServiceHealthy_ServiceNotHealthy_ReturnsOkFalse()
    {
        var gatewayServiceMock = new Mock<IGatewayService>();
        var metricsServiceMock = new Mock<IMetricsCollectionService>();
        gatewayServiceMock.Setup(s => s.GetHealthyServicesAsync())
            .ReturnsAsync(new List<GrpcService>());

        var controller = CreateController(gatewayServiceMock, metricsServiceMock);

        var result = await controller.IsServiceHealthy("UnknownService");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.False((bool)okResult.Value!);
    }

    [Fact]
    public async Task IsServiceHealthy_BlankServiceName_ReturnsBadRequest()
    {
        var gatewayServiceMock = new Mock<IGatewayService>();
        var metricsServiceMock = new Mock<IMetricsCollectionService>();
        var controller = CreateController(gatewayServiceMock, metricsServiceMock);

        var result = await controller.IsServiceHealthy("  ");

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetTodayStatisticsWithFilter_NoServiceNameFilter_ReturnsFullStatistics()
    {
        var gatewayServiceMock = new Mock<IGatewayService>();
        var metricsServiceMock = new Mock<IMetricsCollectionService>();
        var stats = new GatewayStatistics
        {
            Id = 1,
            StatisticsDate = DateTime.UtcNow.Date,
            RequestsByService = new Dictionary<string, long> { { "OrderService", 42 } }
        };
        metricsServiceMock.Setup(m => m.GetTodayStatisticsAsync()).ReturnsAsync(stats);

        var controller = CreateController(gatewayServiceMock, metricsServiceMock);

        var result = await controller.GetTodayStatisticsWithFilter();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsType<GatewayStatistics>(okResult.Value);
        Assert.Same(stats, returned);
    }

    [Fact]
    public async Task GetTodayStatisticsWithFilter_KnownServiceFilter_ReturnsFilteredStatistics()
    {
        var gatewayServiceMock = new Mock<IGatewayService>();
        var metricsServiceMock = new Mock<IMetricsCollectionService>();
        var stats = new GatewayStatistics
        {
            Id = 1,
            StatisticsDate = DateTime.UtcNow.Date,
            RequestsByService = new Dictionary<string, long>
            {
                { "OrderService", 42 },
                { "PaymentService", 7 }
            }
        };
        metricsServiceMock.Setup(m => m.GetTodayStatisticsAsync()).ReturnsAsync(stats);

        var controller = CreateController(gatewayServiceMock, metricsServiceMock);

        var result = await controller.GetTodayStatisticsWithFilter("OrderService");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var filtered = Assert.IsType<GatewayStatistics>(okResult.Value);
        Assert.Single(filtered.RequestsByService!);
        Assert.Equal(42, filtered.RequestsByService!["OrderService"]);
        Assert.Equal(42, filtered.TotalRequestsProcessed);
    }

    [Fact]
    public async Task GetTodayStatisticsWithFilter_UnknownServiceFilter_ReturnsNotFound()
    {
        var gatewayServiceMock = new Mock<IGatewayService>();
        var metricsServiceMock = new Mock<IMetricsCollectionService>();
        var stats = new GatewayStatistics
        {
            Id = 1,
            StatisticsDate = DateTime.UtcNow.Date,
            RequestsByService = new Dictionary<string, long> { { "OrderService", 42 } }
        };
        metricsServiceMock.Setup(m => m.GetTodayStatisticsAsync()).ReturnsAsync(stats);

        var controller = CreateController(gatewayServiceMock, metricsServiceMock);

        var result = await controller.GetTodayStatisticsWithFilter("MissingService");

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetSlowRequestsByService_ZeroThreshold_ReturnsBadRequest()
    {
        var gatewayServiceMock = new Mock<IGatewayService>();
        var metricsServiceMock = new Mock<IMetricsCollectionService>();
        var controller = CreateController(gatewayServiceMock, metricsServiceMock);

        var result = await controller.GetSlowRequestsByService(thresholdMs: 0);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetSlowRequestsByService_WithServiceFilter_ReturnsOnlyMatchingRequests()
    {
        var gatewayServiceMock = new Mock<IGatewayService>();
        var metricsServiceMock = new Mock<IMetricsCollectionService>();
        var metrics = new List<RequestMetric>
        {
            new RequestMetric { Id = 1, ServiceName = "OrderService", MethodName = "GetOrder", ClientIpAddress = "127.0.0.1" },
            new RequestMetric { Id = 2, ServiceName = "PaymentService", MethodName = "Charge", ClientIpAddress = "127.0.0.1" }
        };
        metricsServiceMock.Setup(m => m.GetSlowRequestsAsync(1500)).ReturnsAsync(metrics);

        var controller = CreateController(gatewayServiceMock, metricsServiceMock);

        var result = await controller.GetSlowRequestsByService(1500, "orderservice");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var filtered = Assert.IsType<List<RequestMetric>>(okResult.Value);
        Assert.Single(filtered);
        Assert.Equal("OrderService", filtered[0].ServiceName);
    }

    [Fact]
    public async Task GetSlowRequestsByService_EmptyResultSet_ReturnsOkEmptyList()
    {
        var gatewayServiceMock = new Mock<IGatewayService>();
        var metricsServiceMock = new Mock<IMetricsCollectionService>();
        metricsServiceMock.Setup(m => m.GetSlowRequestsAsync(It.IsAny<double>()))
            .ReturnsAsync(new List<RequestMetric>());

        var controller = CreateController(gatewayServiceMock, metricsServiceMock);

        var result = await controller.GetSlowRequestsByService();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsType<List<RequestMetric>>(okResult.Value);
        Assert.Empty(returned);
    }
}
