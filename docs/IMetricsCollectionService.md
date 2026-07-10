# IMetricsCollectionService

IMetricsCollectionService provides an asynchronous interface for collecting, storing, and querying telemetry data related to gRPC gateway requests. It enables recording individual request metrics and retrieving aggregated statistics such as totals, averages, and per‑service breakdowns.

## API

### MetricsCollectionService()
Initializes a new instance of the metrics collection service. No parameters are required.

### RecordRequestMetricAsync
**Purpose:** Persists a single request metric for later aggregation.  
**Parameters:** As defined in the interface (typically a `RequestMetric` instance and an optional `CancellationToken`).  
**Return Value:** A `Task` representing the asynchronous operation.  
**Throws:**  
- `OperationCanceledException` if the supplied cancellation token is triggered.  
- `IOException` if an error occurs while writing to the underlying storage.  

### GetTodayStatisticsAsync
**Purpose:** Retrieves aggregated statistics for all requests processed on the current calendar day.  
**Parameters:** As defined in the interface (typically an optional `CancellationToken`).  
**Return Value:** A `Task<GatewayStatistics>` containing today’s request totals, error counts, and latency summaries.  
**Throws:**  
- `OperationCanceledException` on cancellation.  
- `InvalidOperationException` if statistics for today have not yet been initialized.  

### GetStatisticsAsync
**Purpose:** Retrieves aggregated statistics for a specified time interval.  
**Parameters:** As defined in the interface (typically a start `DateTimeOffset`, an end `DateTimeOffset`, and an optional `CancellationToken`).  
**Return Value:** A `Task<GatewayStatistics>` representing the aggregated data for the interval.  
**Throws:**  
- `OperationCanceledException` on cancellation.  
- `ArgumentOutOfRangeException` if the start time is after the end time.  
- `IOException` if reading the persisted data fails.  

### GetSlowRequestsAsync
**Purpose:** Returns a list of requests whose duration exceeded a given threshold.  
**Parameters:** As defined in the interface (typically a threshold in milliseconds, an optional maximum result count, and a `CancellationToken`).  
**Return Value:** A `Task<List<RequestMetric>>` containing the matching request metrics.  
**Throws:**  
- `OperationCanceledException` on cancellation.  
- `ArgumentException` if the threshold is less than or equal to zero.  

### GetRequestsPerServiceAsync
**Purpose:** Provides a breakdown of request counts grouped by service name.  
**Parameters:** As defined in the interface (typically an optional `CancellationToken`).  
**Return Value:** A `Task<Dictionary<string,int>>` where the key is the service name and the value is the number of requests recorded for that service.  
**Throws:**  
- `OperationCanceledException` on cancellation.  

### GetAverageResponseTimeAsync
**Purpose:** Calculates the average response time for a given service (or across all services if no service is specified).  
**Parameters:** As defined in the interface (typically an optional service name string and a `CancellationToken`).  
**Return Value:** A `Task<double>` representing the average latency in milliseconds.  
**Throws:**  
- `OperationCanceledException` on cancellation.  
- `InvalidOperationException` if no request data exists for the requested scope.  

### UpdateServiceMetricsAsync
**Purpose:** Applies external metric updates to the internal store (e.g., bulk imports or reconciliations).  
**Parameters:** As defined in the interface (typically a collection of metric updates and an optional `CancellationToken`).  
**Return Value:** A `Task` representing the asynchronous operation.  
**Throws:**  
- `OperationCanceledException` on cancellation.  
- `ArgumentNullException` if the supplied updates collection is null.  

## Usage

```csharp
// Example 1: Record a single request metric
var metric = new RequestMetric
{
    ServiceName = "OrderService",
    MethodName = "CreateOrder",
    Timestamp   = DateTime.UtcNow,
    DurationMs  = 125
};
await metricsService.RecordRequestMetricAsync(metric);
```

```csharp
// Example 2: Retrieve today's statistics and the average response time for a service
var todayStats = await metricsService.GetTodayStatisticsAsync();
var avgLatency = await metricsService.GetAverageResponseTimeAsync(serviceName: "OrderService");

Console.WriteLine($"Today: {todayStats.TotalRequests} requests, {todayStats.ErrorCount} errors");
Console.WriteLine($"Average response time for OrderService: {avgLatency:F1} ms");
```

## Notes

- All interface methods are designed to be thread‑safe; concurrent calls are permitted and the implementation should synchronize internal state appropriately.  
- Callers should not assume any particular ordering of effects across threads; each method operates on a snapshot of the current state.  
- The service may buffer metrics internally before persisting them; the exact flush timing is implementation‑specific and not exposed via this interface.  
- Supplying a `CancellationToken` is recommended to allow graceful shutdown or timeout scenarios; if none is provided, the methods behave as if `CancellationToken.None` was used.  
- I/O‑related exceptions (e.g., `IOException`) propagate directly; callers should handle them according to their error‑handling policy.  
- The `Get*` methods return point‑in‑time snapshots; subsequent updates may change the results of later calls.  
- Implementations should validate arguments and throw appropriate exceptions (`ArgumentNullException`, `ArgumentOutOfRangeException`, etc.) before performing any asynchronous work.
