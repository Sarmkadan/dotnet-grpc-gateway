#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using DotNetGrpcGateway.Domain;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class GatewayStatisticsTests
{
    [Fact]
    public void Validate_ValidStatistics_DoesNotThrow()
    {
        var stats = new GatewayStatistics
        {
            TotalRequestsProcessed = 1000,
            SuccessfulRequests = 950,
            FailedRequests = 50,
            SuccessRate = 95.0,
            AverageResponseTimeMs = 150.5,
            MinResponseTimeMs = 50,
            MaxResponseTimeMs = 500,
            TotalDataProcessedBytes = 1024 * 1024,
            ActiveConnections = 10,
            PeakConnections = 25
        };

        var act = () => stats.Validate();

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_NegativeTotalRequests_ThrowsInvalidOperationException()
    {
        var stats = new GatewayStatistics
        {
            TotalRequestsProcessed = -1
        };

        var act = () => stats.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Total requests cannot be negative*");
    }

    [Fact]
    public void Validate_NegativeSuccessfulRequests_ThrowsInvalidOperationException()
    {
        var stats = new GatewayStatistics
        {
            TotalRequestsProcessed = 100,
            SuccessfulRequests = -1
        };

        var act = () => stats.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Request counts cannot be negative*");
    }

    [Fact]
    public void Validate_NegativeFailedRequests_ThrowsInvalidOperationException()
    {
        var stats = new GatewayStatistics
        {
            TotalRequestsProcessed = 100,
            FailedRequests = -1
        };

        var act = () => stats.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Request counts cannot be negative*");
    }

    [Fact]
    public void Validate_SuccessRateBelowZero_ThrowsInvalidOperationException()
    {
        var stats = new GatewayStatistics
        {
            TotalRequestsProcessed = 100,
            SuccessfulRequests = 0,
            SuccessRate = -1.0
        };

        var act = () => stats.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Success rate must be between 0 and 100*");
    }

    [Fact]
    public void Validate_SuccessRateAbove100_ThrowsInvalidOperationException()
    {
        var stats = new GatewayStatistics
        {
            TotalRequestsProcessed = 100,
            SuccessfulRequests = 101,
            SuccessRate = 101.0
        };

        var act = () => stats.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Success rate must be between 0 and 100*");
    }

    [Fact]
    public void Validate_NegativeAverageResponseTime_ThrowsInvalidOperationException()
    {
        var stats = new GatewayStatistics
        {
            AverageResponseTimeMs = -10
        };

        var act = () => stats.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Average response time cannot be negative*");
    }

    [Fact]
    public void Validate_MaxLessThanMinResponseTime_ThrowsInvalidOperationException()
    {
        var stats = new GatewayStatistics
        {
            MinResponseTimeMs = 200,
            MaxResponseTimeMs = 100
        };

        var act = () => stats.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Max response time cannot be less than min*");
    }

    [Fact]
    public void RecordRequest_FirstRequest_SetsInitialValues()
    {
        var stats = new GatewayStatistics();

        stats.RecordRequest(success: true, responseTimeMs: 150.5, dataBytes: 1024);

        stats.TotalRequestsProcessed.Should().Be(1);
        stats.SuccessfulRequests.Should().Be(1);
        stats.FailedRequests.Should().Be(0);
        stats.SuccessRate.Should().Be(100.0);
        stats.AverageResponseTimeMs.Should().Be(150.5);
        stats.MinResponseTimeMs.Should().Be(150.5);
        stats.MaxResponseTimeMs.Should().Be(150.5);
        stats.TotalDataProcessedBytes.Should().Be(1024);
    }

    [Fact]
    public void RecordRequest_MultipleRequests_CalculatesCorrectAverages()
    {
        var stats = new GatewayStatistics();

        stats.RecordRequest(success: true, responseTimeMs: 100, dataBytes: 1024);
        stats.RecordRequest(success: true, responseTimeMs: 200, dataBytes: 2048);
        stats.RecordRequest(success: false, responseTimeMs: 300, dataBytes: 512);

        stats.TotalRequestsProcessed.Should().Be(3);
        stats.SuccessfulRequests.Should().Be(2);
        stats.FailedRequests.Should().Be(1);
        stats.SuccessRate.Should().BeApproximately(66.67, 0.01);
        stats.AverageResponseTimeMs.Should().BeApproximately(200.0, 0.01);
        stats.MinResponseTimeMs.Should().Be(100);
        stats.MaxResponseTimeMs.Should().Be(300);
        stats.TotalDataProcessedBytes.Should().Be(3680);
    }

    [Fact]
    public void RecordRequest_ZeroRequests_DoesNotCalculateAverage()
    {
        var stats = new GatewayStatistics();

        stats.RecordRequest(success: true, responseTimeMs: 0, dataBytes: 0);

        stats.AverageResponseTimeMs.Should().Be(0);
        stats.MinResponseTimeMs.Should().Be(0);
        stats.MaxResponseTimeMs.Should().Be(0);
    }

    [Fact]
    public void RecordServiceRequest_AddsToServiceDictionary()
    {
        var stats = new GatewayStatistics();

        stats.RecordServiceRequest("UserService");
        stats.RecordServiceRequest("UserService");
        stats.RecordServiceRequest("OrderService");

        stats.RequestsByService.Should().HaveCount(2);
        stats.RequestsByService["UserService"].Should().Be(2);
        stats.RequestsByService["OrderService"].Should().Be(1);
    }

    [Fact]
    public void RecordMethodCall_AddsToMethodDictionary()
    {
        var stats = new GatewayStatistics();

        stats.RecordMethodCall("GetUser");
        stats.RecordMethodCall("GetUser");
        stats.RecordMethodCall("CreateOrder");

        stats.RequestsByMethod.Should().HaveCount(2);
        stats.RequestsByMethod["GetUser"].Should().Be(2);
        stats.RequestsByMethod["CreateOrder"].Should().Be(1);
    }

    [Fact]
    public void RecordError_AddsToErrorDictionary()
    {
        var stats = new GatewayStatistics();

        stats.RecordError("TimeoutError");
        stats.RecordError("TimeoutError");
        stats.RecordError("ConnectionError");

        stats.ErrorsByType.Should().HaveCount(2);
        stats.ErrorsByType["TimeoutError"].Should().Be(2);
        stats.ErrorsByType["ConnectionError"].Should().Be(1);
    }

    [Fact]
    public void RecordCacheHit_UpdatesCacheMetrics()
    {
        var stats = new GatewayStatistics();

        stats.RecordCacheHit(isHit: true);
        stats.RecordCacheHit(isHit: false);
        stats.RecordCacheHit(isHit: true);

        stats.CacheHits.Should().Be(2);
        stats.CacheMisses.Should().Be(1);
        stats.CacheHitRate.Should().BeApproximately(66.67, 0.01);
    }

    [Fact]
    public void RecordCacheHit_NoRequests_CalculatesZeroRate()
    {
        var stats = new GatewayStatistics();

        stats.RecordCacheHit(isHit: true);

        stats.CacheHitRate.Should().Be(100.0);
    }

    [Fact]
    public void UpdateServiceHealth_SetsHealthCounts()
    {
        var stats = new GatewayStatistics();

        stats.UpdateServiceHealth(healthyCount: 8, unhealthyCount: 2);

        stats.HealthyServices.Should().Be(8);
        stats.UnhealthyServices.Should().Be(2);
        stats.TotalServices.Should().Be(10);
    }

    [Fact]
    public void DefaultConstructor_SetsDefaultValues()
    {
        var stats = new GatewayStatistics();

        stats.Id.Should().Be(0);
        stats.StatisticsDate.Should().Be(DateTime.UtcNow.Date);
        stats.TotalRequestsProcessed.Should().Be(0);
        stats.SuccessfulRequests.Should().Be(0);
        stats.FailedRequests.Should().Be(0);
        stats.SuccessRate.Should().Be(0.0);
        stats.AverageResponseTimeMs.Should().Be(0.0);
        stats.MinResponseTimeMs.Should().Be(0.0);
        stats.MaxResponseTimeMs.Should().Be(0.0);
        stats.TotalDataProcessedBytes.Should().Be(0);
        stats.ActiveConnections.Should().Be(0);
        stats.PeakConnections.Should().Be(0);
        stats.RequestsByService.Should().BeEmpty();
        stats.RequestsByMethod.Should().BeEmpty();
        stats.ErrorsByType.Should().BeEmpty();
        stats.HealthyServices.Should().Be(0);
        stats.UnhealthyServices.Should().Be(0);
        stats.TotalServices.Should().Be(0);
        stats.CacheHitRate.Should().Be(0.0);
        stats.CacheHits.Should().Be(0);
        stats.CacheMisses.Should().Be(0);
        stats.RecordedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        stats.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
