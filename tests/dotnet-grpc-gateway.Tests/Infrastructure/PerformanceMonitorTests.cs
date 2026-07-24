#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetGrpcGateway.Infrastructure;
using FluentAssertions;
using Xunit;

namespace DotNetGrpcGateway.Tests.Infrastructure;

/// <summary>
/// Comprehensive tests for PerformanceMonitor to ensure thread-safety, correctness,
/// and proper handling of edge cases and threshold boundaries.
/// </summary>
public class PerformanceMonitorTests
{
    private readonly IPerformanceMonitor _monitor = new PerformanceMonitor();

    [Fact]
    public void RecordRequestDuration_WithNullPath_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _monitor.RecordRequestDuration(null!, 100);

        // Assert
        act.Should().Throw<ArgumentNullException>("path cannot be null");
    }

    [Fact]
    public void RecordRequestDuration_WithEmptyPath_ThrowsArgumentException()
    {
        // Act
        var act = () => _monitor.RecordRequestDuration(string.Empty, 100);

        // Assert
        act.Should().Throw<ArgumentException>("path cannot be empty");
    }

    [Fact]
    public void RecordRequestDuration_WithWhitespacePath_ThrowsArgumentException()
    {
        // Act
        var act = () => _monitor.RecordRequestDuration("   ", 100);

        // Assert
        act.Should().Throw<ArgumentException>("path cannot be whitespace only");
    }

    [Fact]
    public void RecordRequestDuration_WithNegativeDuration_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => _monitor.RecordRequestDuration("/api/test", -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>("duration must be non-negative");
    }

    [Fact]
    public async Task GetMetricsAsync_WhenNoRequestsRecorded_ReturnsZeroMetrics()
    {
        // Act
        var metrics = await _monitor.GetMetricsAsync();

        // Assert
        metrics.Should().NotBeNull();
        metrics.TotalRequests.Should().Be(0);
        metrics.AverageDurationMs.Should().Be(0);
        metrics.MinDurationMs.Should().Be(0);
        metrics.MaxDurationMs.Should().Be(0);
        metrics.P50DurationMs.Should().Be(0);
        metrics.P95DurationMs.Should().Be(0);
        metrics.P99DurationMs.Should().Be(0);
        metrics.RequestsPerSecond.Should().Be(0);
        metrics.RouteMetrics.Should().BeEmpty();
    }

    [Fact]
    public async Task RecordRequestDuration_WithSingleRequest_UpdatesMetricsCorrectly()
    {
        // Arrange
        const string path = "/api/users";
        const long duration = 150;

        // Act
        _monitor.RecordRequestDuration(path, duration);
        var metrics = await _monitor.GetMetricsAsync();

        // Assert
        metrics.TotalRequests.Should().Be(1);
        metrics.AverageDurationMs.Should().Be(150);
        metrics.MinDurationMs.Should().Be(150);
        metrics.MaxDurationMs.Should().Be(150);
        metrics.P50DurationMs.Should().Be(150); // With 1 sample, P50 is that sample
        metrics.P95DurationMs.Should().Be(150); // With 1 sample, P95 is that sample
        metrics.P99DurationMs.Should().Be(150); // With 1 sample, P99 is that sample
        metrics.RouteMetrics.Should().HaveCount(1);

        var routeMetrics = metrics.RouteMetrics[path];
        routeMetrics.Should().NotBeNull();
        routeMetrics.Path.Should().Be(path);
        routeMetrics.TotalRequests.Should().Be(1);
        routeMetrics.AverageDurationMs.Should().Be(150);
        routeMetrics.MinDurationMs.Should().Be(150);
        routeMetrics.MaxDurationMs.Should().Be(150);
        routeMetrics.P50DurationMs.Should().Be(150);
        routeMetrics.P95DurationMs.Should().Be(150);
        routeMetrics.P99DurationMs.Should().Be(150);
    }

    [Fact]
    public async Task RecordRequestDuration_WithMultipleRequests_CalculatesAveragesCorrectly()
    {
        // Arrange
        var durations = new long[] { 100, 200, 300, 400, 500 };

        // Act
        foreach (var duration in durations)
        {
            _monitor.RecordRequestDuration("/api/test", duration);
        }

        var metrics = await _monitor.GetMetricsAsync();

        // Assert
        metrics.TotalRequests.Should().Be(5);
        metrics.AverageDurationMs.Should().BeApproximately(300, 0.01);
        metrics.MinDurationMs.Should().Be(100);
        metrics.MaxDurationMs.Should().Be(500);
    }

    [Fact]
    public async Task RecordRequestDuration_WithMultipleRoutes_TracksRouteSpecificMetrics()
    {
        // Arrange
        _monitor.RecordRequestDuration("/api/users", 100);
        _monitor.RecordRequestDuration("/api/users", 150);
        _monitor.RecordRequestDuration("/api/posts", 200);
        _monitor.RecordRequestDuration("/api/posts", 250);
        _monitor.RecordRequestDuration("/api/posts", 300);

        // Act
        var metrics = await _monitor.GetMetricsAsync();

        // Assert
        metrics.TotalRequests.Should().Be(5);
        metrics.RouteMetrics.Should().HaveCount(2);

        metrics.RouteMetrics.Should().ContainKey("/api/users");
        metrics.RouteMetrics["/api/users"].TotalRequests.Should().Be(2);
        metrics.RouteMetrics["/api/users"].AverageDurationMs.Should().BeApproximately(125, 0.01);

        metrics.RouteMetrics.Should().ContainKey("/api/posts");
        metrics.RouteMetrics["/api/posts"].TotalRequests.Should().Be(3);
        metrics.RouteMetrics["/api/posts"].AverageDurationMs.Should().BeApproximately(250, 0.01);
    }

    [Fact]
    public async Task RecordRequestDuration_WithConcurrentRequests_ThreadSafetyMaintained()
    {
        // Arrange
        const int threadCount = 10;
        const int requestsPerThread = 100;
        var tasks = new List<Task>();
        var random = new Random();

        // Act - spawn multiple threads recording requests concurrently
        for (var i = 0; i < threadCount; i++)
        {
            var threadId = i;
            tasks.Add(Task.Run(() =>
            {
                for (var j = 0; j < requestsPerThread; j++)
                {
                    var duration = random.Next(10, 500);
                    var path = $"/api/route-{threadId}";
                    _monitor.RecordRequestDuration(path, duration);
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        var metrics = await _monitor.GetMetricsAsync();
        metrics.TotalRequests.Should().Be(threadCount * requestsPerThread);
        metrics.MinDurationMs.Should().BeGreaterThanOrEqualTo(10);
        metrics.MaxDurationMs.Should().BeLessThanOrEqualTo(500);
        metrics.AverageDurationMs.Should().BeGreaterThanOrEqualTo(10).And.BeLessThanOrEqualTo(500);

        // Verify no corruption in route metrics
        var totalRouteRequests = metrics.RouteMetrics.Values.Sum(r => r.TotalRequests);
        totalRouteRequests.Should().Be(metrics.TotalRequests);
    }

    [Fact]
    public async Task RecordRequestDuration_WithManyParallelRequests_NoDataCorruption()
    {
        // Arrange - simulate high load with many parallel requests
        // Note: RouteQuantileSketch has a fixed buffer of 1024 samples per route
        const int totalRequests = 10000;
        var tasks = new Task[totalRequests];
        var random = new Random();

        // Act - all requests in parallel
        for (var i = 0; i < totalRequests; i++)
        {
            var duration = random.Next(1, 1000);
            tasks[i] = Task.Run(() => _monitor.RecordRequestDuration("/api/high-load", duration));
        }

        await Task.WhenAll(tasks);

        // Assert
        var metrics = await _monitor.GetMetricsAsync();
        // Ring buffer keeps only the most recent 1024 samples per route
        metrics.TotalRequests.Should().Be(totalRequests);
        metrics.MinDurationMs.Should().BeGreaterThanOrEqualTo(1);
        metrics.MaxDurationMs.Should().BeLessThanOrEqualTo(1000);
        metrics.AverageDurationMs.Should().BeGreaterThanOrEqualTo(1).And.BeLessThanOrEqualTo(1000);

        // Verify route metrics are consistent
        metrics.RouteMetrics.Should().ContainKey("/api/high-load");
        var routeMetrics = metrics.RouteMetrics["/api/high-load"];
        // Route buffer only keeps 1024 samples
        routeMetrics.TotalRequests.Should().BeLessThanOrEqualTo(1024);
        routeMetrics.AverageDurationMs.Should().BeApproximately(metrics.AverageDurationMs, 50.0); // Allow variance due to sampling
    }

    [Fact]
    public async Task GetMetricsAsync_WithBoundaryDurations_CalculatesPercentilesCorrectly()
    {
        // Arrange - use specific values to test percentile boundaries
        _monitor.RecordRequestDuration("/api/test", 50);   // Min
        _monitor.RecordRequestDuration("/api/test", 100);
        _monitor.RecordRequestDuration("/api/test", 150);
        _monitor.RecordRequestDuration("/api/test", 200);
        _monitor.RecordRequestDuration("/api/test", 250);
        _monitor.RecordRequestDuration("/api/test", 300);
        _monitor.RecordRequestDuration("/api/test", 350);
        _monitor.RecordRequestDuration("/api/test", 400);
        _monitor.RecordRequestDuration("/api/test", 450);
        _monitor.RecordRequestDuration("/api/test", 500); // Max

        // Act
        var metrics = await _monitor.GetMetricsAsync();

        // Assert - with 10 values, indices are:
        // P50 (50th percentile): index = (10-1)*0.5 = 4.5 -> 4 (0-based) -> value at index 4
        // P95 (95th percentile): index = (10-1)*0.95 = 8.55 -> 8 -> value at index 8
        // P99 (99th percentile): index = (10-1)*0.99 = 8.91 -> 8 -> value at index 8
        metrics.P50DurationMs.Should().Be(250);
        metrics.P95DurationMs.Should().Be(450);
        metrics.P99DurationMs.Should().Be(450);
    }

    [Fact]
    public async Task GetMetricsAsync_WithExactThresholdBoundary_HandlesCorrectly()
    {
        // Arrange - test values exactly at common thresholds
        _monitor.RecordRequestDuration("/api/test", 100);
        _monitor.RecordRequestDuration("/api/test", 200);
        _monitor.RecordRequestDuration("/api/test", 300);
        _monitor.RecordRequestDuration("/api/test", 400);
        _monitor.RecordRequestDuration("/api/test", 500);

        // Act
        var metrics = await _monitor.GetMetricsAsync();

        // Assert
        metrics.P50DurationMs.Should().Be(300); // Median of 5 values
        metrics.P95DurationMs.Should().Be(500); // 95th percentile rounds down to max
        metrics.P99DurationMs.Should().Be(500);
    }

    [Fact]
    public async Task GetMetricsAsync_WithOneUnitOverUnderThreshold_HandlesCorrectly()
    {
        // Arrange - test values just over and under thresholds
        _monitor.RecordRequestDuration("/api/test", 99);   // Just under 100
        _monitor.RecordRequestDuration("/api/test", 100);  // Exactly 100
        _monitor.RecordRequestDuration("/api/test", 101);  // Just over 100

        // Act
        var metrics = await _monitor.GetMetricsAsync();

        // Assert
        metrics.MinDurationMs.Should().Be(99);
        metrics.MaxDurationMs.Should().Be(101);
        metrics.AverageDurationMs.Should().BeApproximately(100, 0.01);
        metrics.P50DurationMs.Should().Be(100);
    }

    [Fact]
    public async Task ResetAsync_AfterRecordingMetrics_ClearsAllState()
    {
        // Arrange - record some metrics
        _monitor.RecordRequestDuration("/api/test", 100);
        _monitor.RecordRequestDuration("/api/test", 200);

        var metricsBefore = await _monitor.GetMetricsAsync();
        metricsBefore.TotalRequests.Should().Be(2);
        metricsBefore.RouteMetrics.Should().HaveCount(1);

        // Act - reset
        _monitor.ResetAsync().GetAwaiter().GetResult();

        // Assert - all metrics should be zero/empty
        var metricsAfter = await _monitor.GetMetricsAsync();
        metricsAfter.TotalRequests.Should().Be(0);
        metricsAfter.AverageDurationMs.Should().Be(0);
        metricsAfter.MinDurationMs.Should().Be(0);
        metricsAfter.MaxDurationMs.Should().Be(0);
        metricsAfter.P50DurationMs.Should().Be(0);
        metricsAfter.P95DurationMs.Should().Be(0);
        metricsAfter.P99DurationMs.Should().Be(0);
        metricsAfter.RequestsPerSecond.Should().Be(0);
        metricsAfter.RouteMetrics.Should().BeEmpty();
    }

    [Fact]
    public async Task ResetAsync_MultipleTimes_WorksCorrectly()
    {
        // Arrange - record metrics, reset, record more, reset again
        _monitor.RecordRequestDuration("/api/test", 100);
        _monitor.ResetAsync().GetAwaiter().GetResult();

        _monitor.RecordRequestDuration("/api/test", 200);
        _monitor.ResetAsync().GetAwaiter().GetResult();

        // Act
        var metrics = await _monitor.GetMetricsAsync();

        // Assert
        metrics.TotalRequests.Should().Be(0);
        metrics.RouteMetrics.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMetricsAsync_WithEmptyState_NoDivideByZeroErrors()
    {
        // Arrange - no metrics recorded

        // Act
        var metrics = await _monitor.GetMetricsAsync();

        // Assert - should not throw any exceptions and return valid zero metrics
        metrics.Should().NotBeNull();
        metrics.TotalRequests.Should().Be(0);

        // These should all be 0, not throw
        Action getMetrics = () => _ = metrics.AverageDurationMs;
        getMetrics.Should().NotThrow();

        Action getMin = () => _ = metrics.MinDurationMs;
        getMin.Should().NotThrow();

        Action getMax = () => _ = metrics.MaxDurationMs;
        getMax.Should().NotThrow();

        Action getP50 = () => _ = metrics.P50DurationMs;
        getP50.Should().NotThrow();

        Action getP95 = () => _ = metrics.P95DurationMs;
        getP95.Should().NotThrow();

        Action getP99 = () => _ = metrics.P99DurationMs;
        getP99.Should().NotThrow();
    }

    [Fact]
    public async Task RecordRequestDuration_WithDifferentPaths_CreatesSeparateRouteMetrics()
    {
        // Arrange
        _monitor.RecordRequestDuration("/api/users", 100);
        _monitor.RecordRequestDuration("/api/posts", 200);
        _monitor.RecordRequestDuration("/api/comments", 150);

        // Act
        var metrics = await _monitor.GetMetricsAsync();

        // Assert
        metrics.RouteMetrics.Should().HaveCount(3);
        metrics.RouteMetrics.Should().ContainKeys("/api/users", "/api/posts", "/api/comments");

        metrics.RouteMetrics["/api/users"].TotalRequests.Should().Be(1);
        metrics.RouteMetrics["/api/posts"].TotalRequests.Should().Be(1);
        metrics.RouteMetrics["/api/comments"].TotalRequests.Should().Be(1);
    }

    [Fact]
    public async Task GetMetricsAsync_AfterConcurrentResets_ThreadSafetyMaintained()
    {
        // Arrange - perform concurrent reset operations
        _monitor.RecordRequestDuration("/api/test", 100);

        var tasks = new List<Task>();
        for (var i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                _monitor.ResetAsync().GetAwaiter().GetResult();
                // Small delay to increase chance of interleaving
                Thread.Sleep(1);
                _monitor.RecordRequestDuration("/api/test", 200);
            }));
        }

        await Task.WhenAll(tasks);

        // Act
        var metrics = await _monitor.GetMetricsAsync();

        // Assert - should have metrics from the last operations
        metrics.TotalRequests.Should().BeGreaterThanOrEqualTo(1);
        metrics.MinDurationMs.Should().Be(200);
        metrics.MaxDurationMs.Should().Be(200);
    }

    [Fact]
    public async Task RecordRequestDuration_WithLargeDurations_HandlesCorrectly()
    {
        // Arrange - test with large duration values
        var largeDuration = TimeSpan.FromHours(1).TotalMilliseconds;
        _monitor.RecordRequestDuration("/api/long-running", (long)largeDuration);

        // Act
        var metrics = await _monitor.GetMetricsAsync();

        // Assert
        metrics.TotalRequests.Should().Be(1);
        metrics.AverageDurationMs.Should().Be(largeDuration);
        metrics.MinDurationMs.Should().Be((long)largeDuration);
        metrics.MaxDurationMs.Should().Be((long)largeDuration);
        metrics.RouteMetrics.Should().ContainKey("/api/long-running");
    }

    [Fact]
    public async Task GetMetricsAsync_WithMixedPathDurations_CalculatesGlobalPercentilesCorrectly()
    {
        // Arrange - mix durations across different paths
        // Path 1: [100, 200, 300, 400, 500]
        _monitor.RecordRequestDuration("/api/path1", 100);
        _monitor.RecordRequestDuration("/api/path1", 200);
        _monitor.RecordRequestDuration("/api/path1", 300);
        _monitor.RecordRequestDuration("/api/path1", 400);
        _monitor.RecordRequestDuration("/api/path1", 500);

        // Path 2: [150, 250, 350]
        _monitor.RecordRequestDuration("/api/path2", 150);
        _monitor.RecordRequestDuration("/api/path2", 250);
        _monitor.RecordRequestDuration("/api/path2", 350);

        // Act
        var metrics = await _monitor.GetMetricsAsync();

        // Assert - global percentiles combine all samples
        // Total: 8 samples sorted: [100, 150, 200, 250, 300, 350, 400, 500]
        // P50: index = (8-1)*0.5 = 3.5 -> 3 -> value at index 3 = 250
        // P95: index = (8-1)*0.95 = 6.65 -> 6 -> value at index 6 = 400
        metrics.TotalRequests.Should().Be(8);
        metrics.P50DurationMs.Should().Be(250);
        metrics.P95DurationMs.Should().Be(400);
        metrics.P99DurationMs.Should().Be(500);
    }

    [Fact]
    public async Task GetMetricsAsync_AfterMultipleResets_ReturnsConsistentZeroState()
    {
        // Arrange - perform multiple reset cycles
        for (var i = 0; i < 3; i++)
        {
            _monitor.RecordRequestDuration("/api/test", 100);
            var metrics = await _monitor.GetMetricsAsync();
            metrics.TotalRequests.Should().Be(i + 1);

            _monitor.ResetAsync().GetAwaiter().GetResult();
        }

        // Act - final metrics
        var finalMetrics = await _monitor.GetMetricsAsync();

        // Assert
        finalMetrics.TotalRequests.Should().Be(0);
        finalMetrics.RouteMetrics.Should().BeEmpty();
    }

    [Fact]
    public async Task RecordRequestDuration_WithSamePath_MergesMetricsCorrectly()
    {
        // Arrange - record multiple requests to same path
        const string path = "/api/consistent";
        var durations = new[] { 50, 100, 150, 200, 250 };

        foreach (var duration in durations)
        {
            _monitor.RecordRequestDuration(path, duration);
        }

        // Act
        var metrics = await _monitor.GetMetricsAsync();

        // Assert
        metrics.TotalRequests.Should().Be(5);
        metrics.RouteMetrics.Should().ContainKey(path);
        metrics.RouteMetrics[path].TotalRequests.Should().Be(5);
        metrics.RouteMetrics[path].AverageDurationMs.Should().BeApproximately(150, 0.01);
        metrics.RouteMetrics[path].MinDurationMs.Should().Be(50);
        metrics.RouteMetrics[path].MaxDurationMs.Should().Be(250);
    }
}
