#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class LoadBalancerServiceTests
{
    private readonly Mock<ILogger<LoadBalancerService>> _logger = new();

    private LoadBalancerService CreateSut() => new(_logger.Object);

    private static ServiceEndpoint BuildEndpoint(int id, int serviceId, string host = "localhost", int port = 5000) => new()
    {
        Id = id,
        ServiceId = serviceId,
        Host = host,
        Port = port,
        IsHealthy = true,
        Weight = 1
    };

    [Fact]
    public void GetNextEndpoint_NoEndpointsRegistered_ReturnsNull()
    {
        var sut = CreateSut();

        var result = sut.GetNextEndpoint(serviceId: 1);

        result.Should().BeNull();
    }

    [Fact]
    public void GetNextEndpoint_AllEndpointsUnhealthy_ReturnsNull()
    {
        var sut = CreateSut();
        var endpoint = BuildEndpoint(1, serviceId: 1);
        endpoint.IsHealthy = false;
        sut.RegisterEndpoint(endpoint);

        var result = sut.GetNextEndpoint(serviceId: 1);

        result.Should().BeNull();
    }

    [Fact]
    public void GetNextEndpoint_RoundRobin_CyclesThroughEndpoints()
    {
        var sut = CreateSut();
        sut.Strategy = LoadBalancingStrategy.RoundRobin;
        sut.RegisterEndpoint(BuildEndpoint(1, serviceId: 1, host: "host-a"));
        sut.RegisterEndpoint(BuildEndpoint(2, serviceId: 1, host: "host-b"));

        var first = sut.GetNextEndpoint(1);
        var second = sut.GetNextEndpoint(1);

        first.Should().NotBeNull();
        second.Should().NotBeNull();
        first!.Host.Should().NotBe(second!.Host, "round-robin should alternate between endpoints");
    }

    [Fact]
    public void GetNextEndpoint_LeastConnections_SelectsEndpointWithFewerConnections()
    {
        var sut = CreateSut();
        sut.Strategy = LoadBalancingStrategy.LeastConnections;

        var busy = BuildEndpoint(1, serviceId: 2, host: "busy");
        busy.ActiveConnections = 10;
        var idle = BuildEndpoint(2, serviceId: 2, host: "idle");
        idle.ActiveConnections = 0;

        sut.RegisterEndpoint(busy);
        sut.RegisterEndpoint(idle);

        var result = sut.GetNextEndpoint(serviceId: 2);

        result!.Host.Should().Be("idle");
    }

    [Fact]
    public void DeregisterEndpoint_RemovesEndpointFromPool()
    {
        var sut = CreateSut();
        sut.RegisterEndpoint(BuildEndpoint(1, serviceId: 3));
        sut.RegisterEndpoint(BuildEndpoint(2, serviceId: 3));

        sut.DeregisterEndpoint(serviceId: 3, endpointId: 1);
        var endpoints = sut.GetEndpoints(serviceId: 3);

        endpoints.Should().HaveCount(1);
        endpoints[0].Id.Should().Be(2);
    }

    [Fact]
    public void UpdateEndpointHealth_MarksEndpointUnhealthy_ExcludesFromSelection()
    {
        var sut = CreateSut();
        sut.RegisterEndpoint(BuildEndpoint(1, serviceId: 4));
        sut.UpdateEndpointHealth(serviceId: 4, endpointId: 1, isHealthy: false);

        var result = sut.GetNextEndpoint(serviceId: 4);

        result.Should().BeNull("unhealthy endpoints must not be selected");
    }
}
