// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Diagnostics;

namespace DotNetGrpcGateway.Infrastructure;

/// <summary>
/// Performance monitoring service that tracks request times, throughput, and latency percentiles.
/// Used for real-time performance insights and SLA monitoring.
/// </summary>
public interface IPerformanceMonitor
{
    void RecordRequestDuration(string path, long durationMs);
    Task<PerformanceMetrics> GetMetricsAsync();
    Task ResetAsync();
}

/// <summary>
/// Performance metrics data.
/// </summary>
public class PerformanceMetrics
{
    public long TotalRequests { get; set; }
    public double AverageDurationMs { get; set; }
    public long MinDurationMs { get; set; }
    public long MaxDurationMs { get; set; }
    public double P50DurationMs { get; set; } // Median
    public double P95DurationMs { get; set; }
    public double P99DurationMs { get; set; }
    public double RequestsPerSecond { get; set; }
}

/// <summary>
/// In-memory performance monitor implementation.
/// </summary>
public class PerformanceMonitor : IPerformanceMonitor
{
    private readonly ConcurrentDictionary<string, List<long>> _durations = new();
    private readonly Stopwatch _uptime = Stopwatch.StartNew();
    private long _totalRequests = 0;

    public void RecordRequestDuration(string path, long durationMs)
    {
        if (string.IsNullOrEmpty(path) || durationMs < 0)
            return;

        _durations.AddOrUpdate(path,
            new List<long> { durationMs },
            (_, list) =>
            {
                list.Add(durationMs);
                // Keep only recent samples to prevent memory growth
                if (list.Count > 10000)
                    list.RemoveRange(0, 1000);
                return list;
            });

        Interlocked.Increment(ref _totalRequests);
    }

    public async Task<PerformanceMetrics> GetMetricsAsync()
    {
        return await Task.FromResult(CalculateMetrics());
    }

    public async Task ResetAsync()
    {
        _durations.Clear();
        _totalRequests = 0;
        _uptime.Restart();
        await Task.CompletedTask;
    }

    private PerformanceMetrics CalculateMetrics()
    {
        if (_totalRequests == 0)
            return new PerformanceMetrics();

        var allDurations = new List<long>();
        foreach (var durations in _durations.Values)
        {
            allDurations.AddRange(durations);
        }

        if (allDurations.Count == 0)
            return new PerformanceMetrics();

        allDurations.Sort();

        return new PerformanceMetrics
        {
            TotalRequests = _totalRequests,
            AverageDurationMs = allDurations.Average(),
            MinDurationMs = allDurations.First(),
            MaxDurationMs = allDurations.Last(),
            P50DurationMs = GetPercentile(allDurations, 0.50),
            P95DurationMs = GetPercentile(allDurations, 0.95),
            P99DurationMs = GetPercentile(allDurations, 0.99),
            RequestsPerSecond = _uptime.Elapsed.TotalSeconds > 0
                ? _totalRequests / _uptime.Elapsed.TotalSeconds
                : 0
        };
    }

    private double GetPercentile(List<long> sortedValues, double percentile)
    {
        if (sortedValues.Count == 0)
            return 0;

        var index = (int)((sortedValues.Count - 1) * percentile);
        return sortedValues[Math.Max(0, Math.Min(index, sortedValues.Count - 1))];
    }
}
