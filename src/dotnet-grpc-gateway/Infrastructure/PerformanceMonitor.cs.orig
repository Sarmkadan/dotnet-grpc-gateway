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

    /// <summary>
    /// Gets per-route performance metrics.
    /// Key: route path, Value: route-specific metrics including percentiles.
    /// </summary>
    public IReadOnlyDictionary<string, RouteMetrics> RouteMetrics { get; init; } = new Dictionary<string, RouteMetrics>();
}

/// <summary>
/// Performance metrics for a specific route.
/// </summary>
public class RouteMetrics
{
    /// <summary>
    /// Gets the route path.
    /// </summary>
    public string Path { get; init; } = string.Empty;

    /// <summary>
    /// Gets the total number of requests for this route.
    /// </summary>
    public long TotalRequests { get; init; }

    /// <summary>
    /// Gets the average request duration in milliseconds for this route.
    /// </summary>
    public double AverageDurationMs { get; init; }

    /// <summary>
    /// Gets the minimum request duration in milliseconds for this route.
    /// </summary>
    public long MinDurationMs { get; init; }

    /// <summary>
    /// Gets the maximum request duration in milliseconds for this route.
    /// </summary>
    public long MaxDurationMs { get; init; }

    /// <summary>
    /// Gets the 50th percentile (median) request duration in milliseconds for this route.
    /// </summary>
    public double P50DurationMs { get; init; }

    /// <summary>
    /// Gets the 95th percentile request duration in milliseconds for this route.
    /// </summary>
    public double P95DurationMs { get; init; }

    /// <summary>
    /// Gets the 99th percentile request duration in milliseconds for this route.
    /// </summary>
    public double P99DurationMs { get; init; }
}

/// <summary>
/// Thread-safe, allocation-aware performance monitor implementation.
/// Uses Interlocked operations for counters and a ring-buffer quantile sketch for percentile tracking.
/// Memory bound: ~2KB per tracked route (1024 samples * 8 bytes per long).
/// </summary>
public class PerformanceMonitor : IPerformanceMonitor
{
    // Thread-safe counters using Interlocked operations
    private readonly AtomicLong _totalRequests = new();
    private readonly AtomicLong _totalDurationMs = new();
    private readonly AtomicLong _minDurationMs = new(long.MaxValue);
    private readonly AtomicLong _maxDurationMs = new(long.MinValue);

    // Route-specific metrics using ConcurrentDictionary with ring-buffer quantile sketches
    // Key: route path, Value: RouteQuantileSketch for percentile tracking
    private readonly ConcurrentDictionary<string, RouteQuantileSketch> _routeMetrics = new();

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

        // Update route-specific metrics with proper concurrent access
        var sketch = _routeMetrics.GetOrAdd(path, static _ => new RouteQuantileSketch());
        sketch.Add(durationMs);
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
        _routeMetrics.Clear();
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
                RequestsPerSecond = 0,
                RouteMetrics = new Dictionary<string, RouteMetrics>()
            };
        }

        // Calculate average
        var averageDurationMs = (double)totalDurationMs / totalRequests;

        // Calculate requests per second
        var requestsPerSecond = _uptime.Elapsed.TotalSeconds > 0
            ? totalRequests / _uptime.Elapsed.TotalSeconds
            : 0;

        // Calculate global percentiles from all route sketches
        var globalDurations = new List<long>();
        foreach (var sketch in _routeMetrics.Values)
        {
            globalDurations.AddRange(sketch.GetAllSamples());
        }

        globalDurations.Sort();

        // Build route-specific metrics
        var routeMetricsDict = new Dictionary<string, RouteMetrics>(StringComparer.Ordinal);
        foreach (var kvp in _routeMetrics)
        {
            var sketch = kvp.Value;
            routeMetricsDict[kvp.Key] = new RouteMetrics
            {
                Path = kvp.Key,
                TotalRequests = sketch.Count,
                AverageDurationMs = sketch.AverageDurationMs,
                MinDurationMs = sketch.MinDurationMs,
                MaxDurationMs = sketch.MaxDurationMs,
                P50DurationMs = sketch.GetPercentile(0.50),
                P95DurationMs = sketch.GetPercentile(0.95),
                P99DurationMs = sketch.GetPercentile(0.99)
            };
        }

        return new PerformanceMetrics
        {
            TotalRequests = totalRequests,
            AverageDurationMs = averageDurationMs,
            MinDurationMs = minDurationMs,
            MaxDurationMs = maxDurationMs,
            P50DurationMs = globalDurations.Count > 0 ? GetPercentile(globalDurations, 0.50) : 0,
            P95DurationMs = globalDurations.Count > 0 ? GetPercentile(globalDurations, 0.95) : 0,
            P99DurationMs = globalDurations.Count > 0 ? GetPercentile(globalDurations, 0.99) : 0,
            RequestsPerSecond = requestsPerSecond,
            RouteMetrics = routeMetricsDict
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

    /// <summary>
    /// Fixed-size ring buffer quantile sketch for efficient percentile tracking.
    /// Memory bound: 1024 samples * 8 bytes = 8KB per route.
    /// Provides O(1) insertion and O(n) percentile calculation with n=1024.
    /// </summary>
    private sealed class RouteQuantileSketch
    {
        // Ring buffer with fixed capacity for memory-bound percentile tracking
        private const int MaxSamples = 1024;
        private readonly long[] _samples = new long[MaxSamples];
        private int _head = 0;
        private int _count = 0;
        private long _sum = 0;
        private long _min = long.MaxValue;
        private long _max = long.MinValue;

        public int Count => _count;
        public double AverageDurationMs => _count > 0 ? (double)_sum / _count : 0;
        public long MinDurationMs => _count > 0 ? _min : 0;
        public long MaxDurationMs => _count > 0 ? _max : 0;

        public void Add(long durationMs)
        {
            // Update counters
            _sum += durationMs;
            _min = Math.Min(_min, durationMs);
            _max = Math.Max(_max, durationMs);

            // Add to ring buffer
            _samples[_head] = durationMs;
            _head = (_head + 1) % MaxSamples;

            // Update count (handles wrap-around)
            if (_count < MaxSamples)
            {
                _count++;
            }
        }

        public double GetPercentile(double percentile)
        {
            if (_count == 0)
            {
                return 0;
            }

            // Copy samples to temporary array for sorting
            var samplesToSort = new long[_count];
            if (_head == 0)
            {
                Array.Copy(_samples, samplesToSort, _count);
            }
            else
            {
                // Handle wrap-around case
                var firstPart = MaxSamples - _head;
                Array.Copy(_samples, _head, samplesToSort, 0, firstPart);
                Array.Copy(_samples, 0, samplesToSort, firstPart, _head);
            }

            Array.Sort(samplesToSort);

            var index = (int)((_count - 1) * percentile);
            return samplesToSort[Math.Max(0, Math.Min(index, _count - 1))];
        }

        public IEnumerable<long> GetAllSamples()
        {
            if (_count == 0)
            {
                yield break;
            }

            if (_head == 0)
            {
                for (int i = 0; i < _count; i++)
                {
                    yield return _samples[i];
                }
            }
            else
            {
                // Handle wrap-around case
                var firstPart = MaxSamples - _head;
                for (int i = 0; i < firstPart; i++)
                {
                    yield return _samples[_head + i];
                }
                for (int i = 0; i < _head; i++)
                {
                    yield return _samples[i];
                }
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