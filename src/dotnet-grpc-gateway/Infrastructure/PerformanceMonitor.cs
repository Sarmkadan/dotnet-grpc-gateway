#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Collections.Immutable;

namespace DotNetGrpcGateway.Infrastructure;

/// <summary>
/// Performance monitoring service that tracks request times, throughput, and latency percentiles.
/// Used for real-time performance insights and SLA monitoring.
/// </summary>
public interface IPerformanceMonitor
{
    /// <summary>
    /// Records a request duration for the given path.
    /// </summary>
    /// <param name="path">The request path/route.</param>
    /// <param name="durationMs">The duration in milliseconds. Must be non-negative.</param>
    /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="path"/> is empty or whitespace.</exception>
    void RecordRequestDuration(string path, long durationMs);

    /// <summary>
    /// Gets a snapshot of current performance metrics.
    /// The returned metrics are immutable and thread-safe.
    /// </summary>
    /// <returns>A task containing the performance metrics snapshot.</returns>
    Task<PerformanceMetrics> GetMetricsAsync();

    /// <summary>
    /// Resets all recorded metrics to zero.
    /// </summary>
    /// <returns>A completed task.</returns>
    Task ResetAsync();
}

/// <summary>
/// Performance metrics data.
/// </summary>
public class PerformanceMetrics
{
    /// <summary>
    /// Gets the total number of requests recorded.
    /// </summary>
    public long TotalRequests { get; init; }

    /// <summary>
    /// Gets the average request duration in milliseconds.
    /// </summary>
    public double AverageDurationMs { get; init; }

    /// <summary>
    /// Gets the minimum request duration in milliseconds.
    /// </summary>
    public long MinDurationMs { get; init; }

    /// <summary>
    /// Gets the maximum request duration in milliseconds.
    /// </summary>
    public long MaxDurationMs { get; init; }

    /// <summary>
    /// Gets the 50th percentile (median) request duration in milliseconds.
    /// </summary>
    public double P50DurationMs { get; init; }

    /// <summary>
    /// Gets the 95th percentile request duration in milliseconds.
    /// </summary>
    public double P95DurationMs { get; init; }

    /// <summary>
    /// Gets the 99th percentile request duration in milliseconds.
    /// </summary>
    public double P99DurationMs { get; init; }

    /// <summary>
    /// Gets the current requests per second.
    /// </summary>
    public double RequestsPerSecond { get; init; }
}

/// <summary>
/// Thread-safe, allocation-aware performance monitor implementation.
/// Uses Interlocked operations for counters and immutable snapshots for metrics.
/// </summary>
public class PerformanceMonitor : IPerformanceMonitor
{
    // Thread-safe counters using Interlocked operations
    private readonly AtomicLong _totalRequests = new();
    private readonly AtomicLong _totalDurationMs = new();
    private readonly AtomicLong _minDurationMs = new(long.MaxValue);
    private readonly AtomicLong _maxDurationMs = new(long.MinValue);
    private readonly AtomicLong _routeCount = new();

    // Route-specific metrics using ConcurrentDictionary with immutable lists
    // Key: route path, Value: immutable array of durations (sorted for percentiles)
    private readonly ConcurrentDictionary<string, ImmutableArray<long>> _routeDurations = new();

    // Uptime tracking
    private readonly Stopwatch _uptime = Stopwatch.StartNew();

    public void RecordRequestDuration(string path, long durationMs)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (durationMs < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(durationMs), "Duration must be non-negative");
        }

        // Update global counters atomically
        _totalRequests.Increment();
        _totalDurationMs.Add(durationMs);
        _minDurationMs.Min(durationMs);
        _maxDurationMs.Max(durationMs);

        // Update route-specific metrics
        // Use a simple approach: get or create the array, then update atomically
        ImmutableArray<long> existingArray;
        do
        {
            if (_routeDurations.TryGetValue(path, out var existing))
            {
                existingArray = existing;
            }
            else
            {
                existingArray = ImmutableArray<long>.Empty;
            }

            var updatedArray = existingArray.Add(durationMs).Sort();
        }
        while (!_routeDurations.TryUpdate(path, existingArray.Add(durationMs).Sort(), existingArray));

        // Limit memory growth by removing oldest samples when exceeding threshold
        // This is done periodically rather than on every update for performance
        if (_routeDurations.Count > 0 && _totalRequests.Value % 1000 == 0)
        {
            TrimOldSamples();
        }
    }

    public async Task<PerformanceMetrics> GetMetricsAsync()
    {
        return await Task.FromResult(CreateMetricsSnapshot());
    }

    public async Task ResetAsync()
    {
        _totalRequests.Reset();
        _totalDurationMs.Reset();
        _minDurationMs.Reset(long.MaxValue);
        _maxDurationMs.Reset(long.MinValue);
        _routeCount.Reset();
        _routeDurations.Clear();
        _uptime.Restart();
        await Task.CompletedTask;
    }

    private PerformanceMetrics CreateMetricsSnapshot()
    {
        // Read counters atomically
        var totalRequests = _totalRequests.Value;
        var totalDurationMs = _totalDurationMs.Value;
        var minDurationMs = _minDurationMs.Value;
        var maxDurationMs = _maxDurationMs.Value;

        if (totalRequests == 0)
        {
            return new PerformanceMetrics
            {
                TotalRequests = 0,
                AverageDurationMs = 0,
                MinDurationMs = 0,
                MaxDurationMs = 0,
                P50DurationMs = 0,
                P95DurationMs = 0,
                P99DurationMs = 0,
                RequestsPerSecond = 0
            };
        }

        // Calculate average
        var averageDurationMs = (double)totalDurationMs / totalRequests;

        // Calculate requests per second
        var requestsPerSecond = _uptime.Elapsed.TotalSeconds > 0
            ? totalRequests / _uptime.Elapsed.TotalSeconds
            : 0;

        // Collect all durations from all routes for percentile calculation
        // This is the only allocation-heavy operation, but it's necessary for accurate percentiles
        var allDurationsBuilder = new List<long>();
        foreach (var durations in _routeDurations.Values)
        {
            allDurationsBuilder.AddRange(durations);
        }

        if (allDurationsBuilder.Count == 0)
        {
            return new PerformanceMetrics
            {
                TotalRequests = totalRequests,
                AverageDurationMs = averageDurationMs,
                MinDurationMs = minDurationMs,
                MaxDurationMs = maxDurationMs,
                P50DurationMs = 0,
                P95DurationMs = 0,
                P99DurationMs = 0,
                RequestsPerSecond = requestsPerSecond
            };
        }

        // Sort once for all percentiles
        allDurationsBuilder.Sort();

        return new PerformanceMetrics
        {
            TotalRequests = totalRequests,
            AverageDurationMs = averageDurationMs,
            MinDurationMs = minDurationMs,
            MaxDurationMs = maxDurationMs,
            P50DurationMs = GetPercentile(allDurationsBuilder, 0.50),
            P95DurationMs = GetPercentile(allDurationsBuilder, 0.95),
            P99DurationMs = GetPercentile(allDurationsBuilder, 0.99),
            RequestsPerSecond = requestsPerSecond
        };
    }

    private double GetPercentile(List<long> sortedValues, double percentile)
    {
        if (sortedValues.Count == 0)
        {
            return 0;
        }

        var index = (int)((sortedValues.Count - 1) * percentile);
        return sortedValues[Math.Max(0, Math.Min(index, sortedValues.Count - 1))];
    }

    private void TrimOldSamples()
    {
        // Remove oldest samples from each route to prevent unbounded memory growth
        // Keep only the most recent 1000 samples per route
        foreach (var key in _routeDurations.Keys.ToArray())
        {
            if (_routeDurations.TryGetValue(key, out var durations) && durations.Length > 1000)
            {
                // Keep the last 1000 samples (most recent)
                var newDurations = durations.AsSpan()[^1000..].ToImmutableArray();
                _routeDurations[key] = newDurations;
            }
        }
    }

    /// <summary>
    /// Atomic long implementation using Interlocked operations.
    /// Provides thread-safe increment, decrement, add, min, max, and read operations.
    /// </summary>
    private sealed class AtomicLong
    {
        private long _value;

        public AtomicLong(long initialValue = 0)
        {
            _value = initialValue;
        }

        public long Value => Interlocked.Read(ref _value);

        public void Increment() => Interlocked.Increment(ref _value);
        public void Add(long amount) => Interlocked.Add(ref _value, amount);
        public void Min(long value) => InterlockedExtensions.Min(ref _value, value);
        public void Max(long value) => InterlockedExtensions.Max(ref _value, value);
        public void Reset(long initialValue = 0) => Interlocked.Exchange(ref _value, initialValue);
    }
}

/// <summary>
/// Extension methods for Interlocked operations on long values.
/// </summary>
internal static class InterlockedExtensions
{
    /// <summary>
    /// Atomically sets the value to the minimum of the current value and the specified value.
    /// </summary>
    public static void Min(ref long location, long value)
    {
        long initialValue, newValue;
        do
        {
            initialValue = Interlocked.Read(ref location);
            newValue = Math.Min(initialValue, value);
        }
        while (initialValue != newValue && Interlocked.CompareExchange(ref location, newValue, initialValue) != initialValue);
    }

    /// <summary>
    /// Atomically sets the value to the maximum of the current value and the specified value.
    /// </summary>
    public static void Max(ref long location, long value)
    {
        long initialValue, newValue;
        do
        {
            initialValue = Interlocked.Read(ref location);
            newValue = Math.Max(initialValue, value);
        }
        while (initialValue != newValue && Interlocked.CompareExchange(ref location, newValue, initialValue) != initialValue);
    }
}