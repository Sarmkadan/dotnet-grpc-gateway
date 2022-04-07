# MetricsAndMonitoringExample

Provides a demonstration and operational interface for inspecting, analyzing, and managing gRPC gateway metrics including performance counters, request statistics, error distributions, and endpoint rankings.

## API

### `public MetricsAndMonitoringExample()`
Initializes a new instance of the metrics and monitoring example. Configures internal metric collectors and connects to the gateway's telemetry pipeline.

### `public async Task DisplayPerformanceMetricsAsync()`
Outputs current performance metrics including request latency percentiles (p50, p95, p99), throughput (requests/second), and active connection counts to the console.

**Returns:** A task that completes when metrics have been retrieved and displayed.

**Throws:** `InvalidOperationException` if the metrics collector is not initialized. `TaskCanceledException` if the operation is canceled.

### `public async Task DisplayTodayStatisticsAsync()`
Displays aggregated statistics for the current calendar day including total requests, successful requests, failed requests, and average response time.

**Returns:** A task that completes when statistics have been retrieved and displayed.

**Throws:** `InvalidOperationException` if the statistics store is unavailable. `TaskCanceledException` if the operation is canceled.

### `public async Task DisplaySlowRequestsAsync()`
Lists the slowest requests observed in the current measurement window, sorted by duration descending. Includes endpoint, method, duration, and timestamp for each entry.

**Returns:** A task that completes when the slow request report has been retrieved and displayed.

**Throws:** `InvalidOperationException` if the request tracker is not initialized. `TaskCanceledException` if the operation is canceled.

### `public async Task DisplayErrorDistributionAsync()`
Shows a breakdown of errors by type (status code, exception category) with occurrence counts and percentages for the current measurement window.

**Returns:** A task that completes when the error distribution has been retrieved and displayed.

**Throws:** `InvalidOperationException` if the error aggregator is not initialized. `TaskCanceledException` if the operation is canceled.

### `public async Task DisplayTopEndpointsAsync()`
Ranks endpoints by request volume, displaying the top consumers with request counts, error rates, and average latencies.

**Returns:** A task that completes when the endpoint ranking has been retrieved and displayed.

**Throws:** `InvalidOperationException` if the endpoint tracker is not initialized. `TaskCanceledException` if the operation is canceled.

### `public async Task ResetMetricsAsync()`
Clears all accumulated metrics, statistics, and tracking data, resetting collectors to initial state.

**Returns:** A task that completes when all metric stores have been reset.

**Throws:** `InvalidOperationException` if a reset operation is already in progress. `TaskCanceledException` if the operation is canceled.

### `public static async Task Main()`
Application entry point. Executes a demonstration sequence displaying all available metric views in order, then resets metrics.

**Returns:** A task that completes when the demonstration sequence finishes.

**Throws:** Any exception thrown by the individual display methods propagates out of Main.

## Usage

```csharp
var monitor = new MetricsAndMonitoringExample();
await monitor.DisplayPerformanceMetricsAsync();
await monitor.DisplayTodayStatisticsAsync();
await monitor.DisplaySlowRequestsAsync();
await monitor.DisplayErrorDistributionAsync();
await monitor.DisplayTopEndpointsAsync();
await monitor.ResetMetricsAsync();
```

```csharp
try
{
    var monitor = new MetricsAndMonitoringExample();
    
    await monitor.DisplayPerformanceMetricsAsync();
    
    if (DateTime.Now.Hour >= 17)
    {
        await monitor.DisplayTodayStatisticsAsync();
        await monitor.DisplayErrorDistributionAsync();
    }
    
    await monitor.DisplayTopEndpointsAsync();
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine($"Monitoring subsystem unavailable: {ex.Message}");
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("Monitoring operation was canceled");
}
```

## Notes

- All instance methods are not thread-safe; concurrent calls to display or reset methods may produce interleaved output or inconsistent metric snapshots. Synchronize externally if concurrent access is required.
- `ResetMetricsAsync` invalidates any in-progress metric reads; callers should ensure no other operations are executing before invoking reset.
- The measurement window for "today" statistics aligns with the system's local calendar day boundary, not a rolling 24-hour window.
- Slow request tracking retains a bounded history (typically top 100 by duration); older entries are evicted as new slower requests are observed.
- Error distribution aggregates by gRPC status code and .NET exception type; transient network errors and application-level failures are categorized separately.
- Top endpoints ranking uses request count as the primary sort key; endpoints with identical counts are secondarily sorted by error rate descending.
- The parameterless constructor may perform I/O to initialize metric collectors; consider lazy initialization in latency-sensitive startup paths.
