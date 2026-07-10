# MetricsController

The `MetricsController` is an ASP.NET Core API controller responsible for exposing runtime performance and request metrics collected by the gRPC gateway. It provides endpoints to retrieve aggregated performance data, request-level statistics, slow-request identification, error breakdowns, per-endpoint analytics, and a mechanism to reset all in-memory metric accumulators.

## API

### `MetricsController`

Constructor. Initializes a new instance of the controller, typically resolved via dependency injection with the required metric-collection services and logger factory.

- **Parameters**: Injected via the ASP.NET Core DI container. Dependencies include the metric aggregation service and an `ILogger<MetricsController>`.
- **Returns**: (Constructor – no return value.)
- **Throws**: `ArgumentNullException` if any required dependency is `null`.

---

### `GetPerformanceMetrics`

```csharp
public async Task<ActionResult<PerformanceMetrics>> GetPerformanceMetrics()
```

Retrieves the current snapshot of overall gateway performance metrics, including throughput, average latency, and percentile latencies over the configured aggregation window.

- **Parameters**: None.
- **Returns**:  
  `ActionResult<PerformanceMetrics>` – `200 OK` with a `PerformanceMetrics` object containing aggregated counters and latency distributions. The object is never `null`; an empty metrics snapshot is returned if no data has been recorded yet.
- **Throws**:  
  `InvalidOperationException` if the underlying metric store is unavailable or has been disposed.

---

### `GetRequestMetrics`

```csharp
public async Task<ActionResult> GetRequestMetrics()
```

Returns a collection of recent individual request metrics, including method name, status code, latency, and timestamp. The response format is typically a JSON array.

- **Parameters**: None.
- **Returns**:  
  `ActionResult` – `200 OK` with a list of request metric records. Returns an empty array when no requests have been recorded.
- **Throws**:  
  `InvalidOperationException` if the metric store is unavailable.

---

### `GetSlowRequests`

```csharp
public async Task<ActionResult> GetSlowRequests()
```

Returns only those requests whose latency exceeds the configured slow-request threshold. Useful for identifying performance outliers.

- **Parameters**: None.
- **Returns**:  
  `ActionResult` – `200 OK` with a filtered list of slow-request metric records. An empty array is returned when no requests exceed the threshold.
- **Throws**:  
  `InvalidOperationException` if the metric store is unavailable.

---

### `GetErrorMetrics`

```csharp
public async Task<ActionResult> GetErrorMetrics()
```

Retrieves aggregated error counts grouped by gRPC status code or HTTP status code, along with the most recent error timestamps.

- **Parameters**: None.
- **Returns**:  
  `ActionResult` – `200 OK` with an error-metrics summary object. Returns zero counts when no errors have occurred.
- **Throws**:  
  `InvalidOperationException` if the metric store is unavailable.

---

### `GetEndpointStats`

```csharp
public async Task<ActionResult> GetEndpointStats()
```

Returns per-endpoint (per-method) statistics, including call counts, average latency, and error rates for each gRPC method exposed through the gateway.

- **Parameters**: None.
- **Returns**:  
  `ActionResult` – `200 OK` with a collection of endpoint statistics objects. An empty collection is returned when no endpoints have been invoked.
- **Throws**:  
  `InvalidOperationException` if the metric store is unavailable.

---

### `ResetMetrics`

```csharp
public async Task<ActionResult> ResetMetrics()
```

Resets all in-memory metric accumulators to their initial state. All counters, histograms, and recorded request lists are cleared.

- **Parameters**: None.
- **Returns**:  
  `ActionResult` – `200 OK` on successful reset. The response body is empty.
- **Throws**:  
  `InvalidOperationException` if the metric store is unavailable or cannot be reset due to an internal lock contention timeout.

## Usage

### Example 1: Retrieve performance metrics and check P99 latency

```csharp
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class MonitoringController : ControllerBase
{
    private readonly MetricsController _metrics;

    public MonitoringController(MetricsController metrics)
    {
        _metrics = metrics;
    }

    [HttpGet("health/performance")]
    public async Task<IActionResult> CheckPerformanceHealth()
    {
        var result = await _metrics.GetPerformanceMetrics();
        if (result.Result is OkObjectResult ok && ok.Value is PerformanceMetrics perf)
        {
            if (perf.P99LatencyMs > 500)
            {
                return UnprocessableEntity(new { Alert = "P99 latency exceeds 500ms" });
            }
            return Ok(new { Status = "Healthy", perf.AverageLatencyMs, perf.P99LatencyMs });
        }
        return StatusCode(500, "Unable to retrieve metrics");
    }
}
```

### Example 2: Reset metrics after a deployment or maintenance window

```csharp
[ApiController]
[Route("admin")]
public class AdminController : ControllerBase
{
    private readonly MetricsController _metrics;

    public AdminController(MetricsController metrics)
    {
        _metrics = metrics;
    }

    [HttpPost("metrics/reset")]
    public async Task<IActionResult> ResetAllMetrics()
    {
        // Optional authorization check omitted for brevity.
        await _metrics.ResetMetrics();
        return Ok(new { Message = "All metrics have been reset." });
    }
}
```

## Notes

- **Thread safety**: All public methods are asynchronous and delegate to a thread-safe metric store. Concurrent calls to any endpoint, including `ResetMetrics`, are safe; the reset operation acquires an exclusive lock internally and will block other metric reads or writes until completion.
- **Empty state**: When no traffic has been recorded, all retrieval methods return successful responses with zero counts or empty collections rather than `404 Not Found`. Clients should handle empty data gracefully.
- **Reset behavior**: `ResetMetrics` clears all accumulated data immediately. Any in-flight requests that complete after the reset will be recorded in the fresh accumulator state. There is no undo mechanism.
- **Disposed store**: If the underlying metric store has been disposed (e.g., during application shutdown), all methods throw `InvalidOperationException`. Callers should catch this exception and return an appropriate `503 Service Unavailable` response if the controller is used as a dependency in other controllers.
- **Performance considerations**: The `GetRequestMetrics` and `GetSlowRequests` endpoints may return large payloads under high traffic. Consider client-side pagination or time-range filtering if the default response size becomes problematic.
