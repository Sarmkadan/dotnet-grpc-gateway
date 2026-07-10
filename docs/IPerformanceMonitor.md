# IPerformanceMonitor

The `IPerformanceMonitor` interface defines a contract for tracking and aggregating performance metrics for gRPC services within the `dotnet-grpc-gateway` environment. It provides capabilities to record individual request durations and retrieve summarized statistics, including latency percentiles and throughput, facilitating real-time monitoring and observability of gateway performance.

## API

### Properties

*   `long TotalRequests`
    Gets the total count of requests processed since the last reset.

*   `double AverageDurationMs`
    Gets the arithmetic mean of the durations of all recorded requests in milliseconds.

*   `long MinDurationMs`
    Gets the minimum duration recorded in milliseconds.

*   `long MaxDurationMs`
    Gets the maximum duration recorded in milliseconds.

*   `double P50DurationMs`
    Gets the 50th percentile (median) request duration in milliseconds.

*   `double P95DurationMs`
    Gets the 95th percentile request duration in milliseconds.

*   `double P99DurationMs`
    Gets the 99th percentile request duration in milliseconds.

*   `double RequestsPerSecond`
    Gets the average number of requests processed per second.

### Methods

*   `void RecordRequestDuration(TimeSpan duration)`
    Records the latency of a single completed request.

*   `Task<PerformanceMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)`
    Asynchronously retrieves a snapshot of current performance statistics.
    Returns: A `PerformanceMetrics` object containing the aggregated data.

*   `Task ResetAsync(CancellationToken cancellationToken = default)`
    Asynchronously resets all internal metrics counters and latency trackers to zero.

## Usage

### Example 1: Recording Request Latency in a Middleware

```csharp
public class PerformanceMiddleware
{
    private readonly IPerformanceMonitor _monitor;

    public PerformanceMiddleware(IPerformanceMonitor monitor)
    {
        _monitor = monitor;
    }

    public async Task InvokeAsync(HttpContext context, Func<Task> next)
    {
        var stopwatch = Stopwatch.StartNew();
        await next();
        stopwatch.Stop();

        _monitor.RecordRequestDuration(stopwatch.Elapsed);
    }
}
```

### Example 2: Exposing Metrics via an Endpoint

```csharp
public class MetricsController : ControllerBase
{
    private readonly IPerformanceMonitor _monitor;

    public MetricsController(IPerformanceMonitor monitor)
    {
        _monitor = monitor;
    }

    [HttpGet("metrics")]
    public async Task<ActionResult<PerformanceMetrics>> GetMetrics(CancellationToken ct)
    {
        var metrics = await _monitor.GetMetricsAsync(ct);
        return Ok(metrics);
    }
}
```

## Notes

*   **Thread-Safety**: Implementations of `IPerformanceMonitor` must be thread-safe, as `RecordRequestDuration` will typically be called concurrently by multiple request-handling threads.
*   **Latency Accuracy**: Durations should be measured with high-precision timers, such as `System.Diagnostics.Stopwatch`.
*   **Async Operations**: `GetMetricsAsync` and `ResetAsync` are designed to be asynchronous to accommodate potential overhead in calculating complex statistics (like high-percentile latencies) or interacting with external storage if needed.
*   **Performance Impact**: While intended for lightweight tracking, recording every single request in very high-throughput systems may introduce minor overhead. Ensure implementations are optimized for low-latency updates.
