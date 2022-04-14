# GatewayControllerExtensions

Extension methods for ASP.NET Core controllers that provide access to gRPC Gateway service functionality, including service discovery, health checks, and request metrics.

## API

### `GetServiceByName`
Retrieves a gRPC service instance by its name.

**Parameters:**
- `controller`: The controller instance.
- `serviceName`: The name of the gRPC service to locate.

**Returns:**
- `Task<ActionResult<GrpcService>>`: An `ActionResult` containing the `GrpcService` instance if found, or `NotFound` if the service does not exist.

**Exceptions:**
- Throws `ArgumentNullException` if `serviceName` is `null`.

---

### `IsServiceHealthy`
Checks the health status of a gRPC service.

**Parameters:**
- `controller`: The controller instance.
- `serviceName`: The name of the gRPC service to check.

**Returns:**
- `Task<ActionResult<bool>>`: An `ActionResult` containing `true` if the service is healthy, or `false` if unhealthy or not found.

**Exceptions:**
- Throws `ArgumentNullException` if `serviceName` is `null`.

---
### `GetTodayStatisticsWithFilter`
Retrieves aggregated statistics for the current day, optionally filtered by service name.

**Parameters:**
- `controller`: The controller instance.
- `serviceName`: Optional name of the service to filter statistics by. If `null`, statistics for all services are returned.

**Returns:**
- `Task<ActionResult<GatewayStatistics>>`: An `ActionResult` containing a `GatewayStatistics` object with request counts, error rates, and latency metrics for the specified service or all services.

**Exceptions:**
- Throws `ArgumentNullException` if `serviceName` is `null` (only when explicitly provided as `null`).

---
### `GetSlowRequestsByService`
Retrieves a list of the slowest requests for a specified service.

**Parameters:**
- `controller`: The controller instance.
- `serviceName`: The name of the gRPC service to query.
- `thresholdMs`: Minimum request duration (in milliseconds) to be considered slow. Requests faster than this are excluded.

**Returns:**
- `Task<ActionResult<List<RequestMetric>>>`: An `ActionResult` containing a list of `RequestMetric` objects representing slow requests, ordered by duration descending.

**Exceptions:**
- Throws `ArgumentNullException` if `serviceName` is `null`.
- Throws `ArgumentOutOfRangeException` if `thresholdMs` is negative.

## Usage

```csharp
// Example 1: Check service health
[HttpGet("health/{serviceName}")]
public async Task<IActionResult> CheckHealth(string serviceName)
{
    var result = await this.IsServiceHealthy(serviceName);
    return result.Result;
}

// Example 2: Retrieve slow requests for a service
[HttpGet("slow-requests/{serviceName}")]
public async Task<IActionResult> GetSlowRequests(string serviceName, [FromQuery] int thresholdMs = 500)
{
    var result = await this.GetSlowRequestsByService(serviceName, thresholdMs);
    return result.Result;
}
```

## Notes

- All methods are thread-safe and may be called concurrently from multiple controller instances.
- If a service name does not exist, methods return `NotFound` rather than throwing.
- Statistics methods (`GetTodayStatisticsWithFilter`, `GetSlowRequestsByService`) may return partial data if metrics are still being aggregated.
- Health checks are best-effort and may report stale status under high load.
