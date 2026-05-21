// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Infrastructure;
using FluentAssertions;

namespace DotNetGrpcGateway.Tests;

public class PerformanceMonitorTests
{
    private readonly PerformanceMonitor _sut = new();

    [Fact]
    public async Task GetMetricsAsync_NoRecordedRequests_ReturnsZeroMetrics()
    {
        var metrics = await _sut.GetMetricsAsync();
        metrics.TotalRequests.Should().Be(0);
        metrics.AverageDurationMs.Should().Be(0);
    }

    [Fact]
    public void RecordRequestDuration_NullPath_DoesNotRecord()
    {
        _sut.RecordRequestDuration(null!, 100);
        var metrics = _sut.GetMetricsAsync().Result;
        metrics.TotalRequests.Should().Be(0);
    }

    [Fact]
    public void RecordRequestDuration_EmptyPath_DoesNotRecord()
    {
        _sut.RecordRequestDuration("", 100);
        var metrics = _sut.GetMetricsAsync().Result;
        metrics.TotalRequests.Should().Be(0);
    }

    [Fact]
    public void RecordRequestDuration_NegativeDuration_DoesNotRecord()
    {
        _sut.RecordRequestDuration("/api/test", -1);
        var metrics = _sut.GetMetricsAsync().Result;
        metrics.TotalRequests.Should().Be(0);
    }

    [Fact]
    public async Task RecordRequestDuration_SingleRequest_CorrectMetrics()
    {
        _sut.RecordRequestDuration("/api/users", 150);
        var metrics = await _sut.GetMetricsAsync();

        metrics.TotalRequests.Should().Be(1);
        metrics.AverageDurationMs.Should().Be(150);
        metrics.MinDurationMs.Should().Be(150);
        metrics.MaxDurationMs.Should().Be(150);
    }

    [Fact]
    public async Task RecordRequestDuration_MultipleRequests_CalculatesCorrectAverage()
    {
        _sut.RecordRequestDuration("/api/users", 100);
        _sut.RecordRequestDuration("/api/users", 200);
        _sut.RecordRequestDuration("/api/users", 300);

        var metrics = await _sut.GetMetricsAsync();

        metrics.TotalRequests.Should().Be(3);
        metrics.AverageDurationMs.Should().Be(200);
        metrics.MinDurationMs.Should().Be(100);
        metrics.MaxDurationMs.Should().Be(300);
    }

    [Fact]
    public async Task RecordRequestDuration_DifferentPaths_AggregatesAllDurations()
    {
        _sut.RecordRequestDuration("/api/users", 100);
        _sut.RecordRequestDuration("/api/orders", 200);

        var metrics = await _sut.GetMetricsAsync();
        metrics.TotalRequests.Should().Be(2);
    }

    [Fact]
    public async Task GetMetricsAsync_CalculatesPercentiles()
    {
        // Record 100 requests with durations 1..100
        for (int i = 1; i <= 100; i++)
        {
            _sut.RecordRequestDuration("/api/test", i);
        }

        var metrics = await _sut.GetMetricsAsync();

        metrics.P50DurationMs.Should().BeGreaterThan(0);
        metrics.P95DurationMs.Should().BeGreaterThanOrEqualTo(metrics.P50DurationMs);
        metrics.P99DurationMs.Should().BeGreaterThanOrEqualTo(metrics.P95DurationMs);
    }

    [Fact]
    public async Task ResetAsync_ClearsAllMetrics()
    {
        _sut.RecordRequestDuration("/api/test", 100);
        _sut.RecordRequestDuration("/api/test", 200);

        await _sut.ResetAsync();
        var metrics = await _sut.GetMetricsAsync();

        metrics.TotalRequests.Should().Be(0);
    }

    [Fact]
    public async Task GetMetricsAsync_RequestsPerSecond_IsPositive()
    {
        _sut.RecordRequestDuration("/api/test", 50);
        Thread.Sleep(10); // ensure some time passes

        var metrics = await _sut.GetMetricsAsync();
        metrics.RequestsPerSecond.Should().BeGreaterThan(0);
    }

    [Fact]
    public void RecordRequestDuration_ZeroDuration_IsRecorded()
    {
        _sut.RecordRequestDuration("/api/fast", 0);
        var metrics = _sut.GetMetricsAsync().Result;
        metrics.TotalRequests.Should().Be(1);
        metrics.MinDurationMs.Should().Be(0);
    }
}
