# GatewayStatisticsExtensions

Provides static helper methods for querying runtime statistics collected by the gRPC‑Gateway. The methods aggregate error counts, request volumes per service, processed data size, and overall health status, allowing monitoring tools or application code to inspect the gateway’s behavior without exposing internal data structures.

## API

### GetTotalErrors
- **Purpose:** Returns the cumulative number of errors that have occurred in the gateway since it started.
- **Parameters:** None.
- **Return Value:** `int` – the total error count.
- **Exceptions:** Throws `InvalidOperationException` if the statistics subsystem has not been initialized or has been disposed.

### GetTopServicesByRequestCount
- **Purpose:** Returns a list of services ordered by descending request count, enabling identification of the most‑used endpoints.
- **Parameters:** None.
- **Return Value:** `List<KeyValuePair<string, long>>` where the key is the service name and the value is the number of requests processed for that service. Returns an empty list when no request data is available.
- **Exceptions:** Throws `InvalidOperationException` if the statistics subsystem is not ready.

### GetFormattedDataProcessed
- **Purpose:** Provides a human‑readable representation of the total amount of data (payloads) processed by the gateway.
- **Parameters:** None.
- **Return Value:** `string` – e.g., `"3.45 MB"` or `"0 KB"` when no data has been processed.
- **Exceptions:** Throws `InvalidOperationException` if data‑processing metrics are unavailable.

### IsGatewayHealthy
- **Purpose:** Indicates whether the gateway meets health criteria (e.g., error rate below a threshold and latency within acceptable bounds).
- **Parameters:** None.
- **Return Value:** `bool` – `true` if the gateway is considered healthy; otherwise `false`.
- **Exceptions:** Throws `InvalidOperationException` when health cannot be evaluated due to missing metrics.

## Usage

```csharp
using DotNetGrpcGateway; // namespace containing GatewayStatisticsExtensions

// Example 1: Periodic logging of gateway statistics
var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
await foreach (var _ in timer.WaitForNextTickAsync())
{
    int errors = GatewayStatisticsExtensions.GetTotalErrors;
    var topServices = GatewayStatisticsExtensions.GetTopServicesByRequestCount;
    string data = GatewayStatisticsExtensions.GetFormattedDataProcessed;
    bool healthy = GatewayStatisticsExtensions.IsGatewayHealthy;

    Console.WriteLine(
        $"[Gateway Stats] Errors: {errors}, Data: {data}, Healthy: {healthy}");
    foreach (var kvp in topServices.Take(5))
    {
        Console.WriteLine($"  Service {kvp.Key}: {kvp.Value} requests");
    }
}
```

```csharp
using DotNetGrpcGateway;

// Example 2: Health‑check endpoint for an orchestration system
public bool CheckGatewayHealth()
{
    try
    {
        return GatewayStatisticsExtensions.IsGatewayHealthy;
    }
    catch (InvalidOperationException)
    {
        // Treat inability to determine health as unhealthy
        return false;
    }
}
```

## Notes
- All methods rely on internal state that is updated by the gateway’s instrumentation layer. If the gateway has not started collecting metrics or has been shut down, the methods will throw `InvalidOperationException`.
- The returned list from `GetTopServicesByRequestCount` is a snapshot; concurrent modifications to the underlying counters will not affect the list once it has been returned.
- The class is thread‑safe for concurrent reads; internal counters are assumed to be synchronized by the gateway’s metrics implementation. However, callers should not depend on the ordering of items in the list beyond the guaranteed descending request count.
- No method allocates or modifies observable gateway state; they are purely observational.
