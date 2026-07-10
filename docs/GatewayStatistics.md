# GatewayStatistics

The `GatewayStatistics` class serves as a data transfer object representing a snapshot of operational metrics for the gRPC gateway at a specific point in time. It aggregates request volumes, performance latencies, connection states, service health status, and caching efficiency into a single immutable-like structure, typically used for monitoring dashboards, logging, or historical analysis of gateway throughput and reliability.

## API

### `Id`
```csharp
public int Id
```
A unique identifier assigned to this specific statistics record. This value is typically used for database indexing or correlating log entries with specific metric snapshots.

### `StatisticsDate`
```csharp
public DateTime StatisticsDate
```
The timestamp indicating when the metrics contained in this instance were captured. This defines the time window end-point for the aggregated data.

### `TotalRequestsProcessed`
```csharp
public long TotalRequestsProcessed
```
The cumulative count of all requests handled by the gateway during the reporting period. This value equals the sum of `SuccessfulRequests` and `FailedRequests`.

### `SuccessfulRequests`
```csharp
public long SuccessfulRequests
```
The count of requests that completed with a successful status code (typically HTTP 2xx or gRPC OK) within the reporting period.

### `FailedRequests`
```csharp
public long FailedRequests
```
The count of requests that resulted in an error, timeout, or non-success status code during the reporting period.

### `SuccessRate`
```csharp
public double SuccessRate
```
The ratio of successful requests to total requests, expressed as a percentage (0.0 to 100.0). This is a calculated field derived from `SuccessfulRequests` and `TotalRequestsProcessed`.

### `AverageResponseTimeMs`
```csharp
public double AverageResponseTimeMs
```
The arithmetic mean of response times for all processed requests, measured in milliseconds.

### `MinResponseTimeMs`
```csharp
public double MinResponseTimeMs
```
The shortest response time recorded among all requests in the reporting period, measured in milliseconds.

### `MaxResponseTimeMs`
```csharp
public double MaxResponseTimeMs
```
The longest response time recorded among all requests in the reporting period, measured in milliseconds.

### `TotalDataProcessedBytes`
```csharp
public long TotalDataProcessedBytes
```
The total volume of data transmitted through the gateway (including both request and payload sizes) during the reporting period, measured in bytes.

### `ActiveConnections`
```csharp
public int ActiveConnections
```
The number of client connections currently open and active at the moment the statistics were captured.

### `PeakConnections`
```csharp
public int PeakConnections
```
The highest number of concurrent connections observed during the reporting period.

### `RequestsByService`
```csharp
public Dictionary<string, long> RequestsByService
```
A mapping of gRPC service names to the number of requests received for each service. This allows for traffic distribution analysis across different backend services.

### `RequestsByMethod`
```csharp
public Dictionary<string, long> RequestsByMethod
```
A mapping of fully qualified method names (e.g., `/Package.Service/Method`) to the number of invocations received. This provides granular visibility into endpoint usage.

### `ErrorsByType`
```csharp
public Dictionary<string, int> ErrorsByType
```
A mapping of error categories or gRPC status codes (e.g., "Unavailable", "DeadlineExceeded") to the count of occurrences. This aids in identifying predominant failure modes.

### `HealthyServices`
```csharp
public int HealthyServices
```
The count of backend services currently passing health checks at the time of the snapshot.

### `UnhealthyServices`
```csharp
public int UnhealthyServices
```
The count of backend services currently failing health checks at the time of the snapshot.

### `TotalServices`
```csharp
public int TotalServices
```
The total number of services configured and monitored by the gateway, equal to the sum of `HealthyServices` and `UnhealthyServices`.

### `CacheHitRate`
```csharp
public double CacheHitRate
```
The percentage of requests served directly from the cache without invoking backend services, expressed as a value between 0.0 and 100.0.

### `CacheHits`
```csharp
public int CacheHits
```
The absolute number of requests resolved via cache during the reporting period.

## Usage

### Example 1: Logging Critical Metrics
The following example demonstrates how to extract key performance indicators from a `GatewayStatistics` instance for structured logging.

```csharp
public void LogGatewayHealth(GatewayStatistics stats)
{
    if (stats.SuccessRate < 95.0)
    {
        Console.WriteLine($"[ALERT] Success rate dropped to {stats.SuccessRate:F2}% at {stats.StatisticsDate}");
        Console.WriteLine($"Failed Requests: {stats.FailedRequests}");
        
        foreach (var error in stats.ErrorsByType)
        {
            Console.WriteLine($"  - {error.Key}: {error.Value} occurrences");
        }
    }
    else
    {
        Console.WriteLine($"Gateway healthy. Avg Response: {stats.AverageResponseTimeMs:F2}ms. Active Connections: {stats.ActiveConnections}");
    }
}
```

### Example 2: Analyzing Service Traffic Distribution
This example illustrates iterating over the `RequestsByService` dictionary to identify the most heavily utilized services.

```csharp
public void IdentifyTopServices(GatewayStatistics stats)
{
    var topServices = stats.RequestsByService
        .OrderByDescending(kvp => kvp.Value)
        .Take(5);

    Console.WriteLine($"Top 5 Services by Traffic ({stats.StatisticsDate:HH:mm:ss}):");
    
    foreach (var service in topServices)
    {
        double percentage = (double)service.Value / stats.TotalRequestsProcessed * 100;
        Console.WriteLine($"  {service.Key}: {service.Value} requests ({percentage:F1}%)");
    }
}
```

## Notes

*   **Thread Safety**: The `GatewayStatistics` class is designed as a data snapshot. While the instance itself does not perform internal synchronization, the collections (`Dictionary<string, long>`, `Dictionary<string, int>`) contained within it are not thread-safe for modification. However, since this type typically represents an immutable snapshot generated by a collector, consumers should treat these properties as read-only. If multiple threads need to iterate over the dictionaries simultaneously, external locking or conversion to a thread-safe collection is recommended if there is any risk of the underlying dictionary being modified after instantiation.
*   **Division by Zero**: Calculated fields such as `SuccessRate` and `CacheHitRate` should handle scenarios where `TotalRequestsProcessed` is zero. In such edge cases (e.g., gateway startup with no traffic), these values typically default to `0.0` or `100.0` depending on implementation specifics, but consumers should verify behavior when `TotalRequestsProcessed` is 0 to avoid logical errors in alerting systems.
*   **Dictionary Mutability**: The dictionaries (`RequestsByService`, `RequestsByMethod`, `ErrorsByType`) are returned as references. Modifying these collections directly will alter the state of the `GatewayStatistics` object. It is best practice to treat these as read-only or create a defensive copy if the data needs to be manipulated.
*   **Time Precision**: The `StatisticsDate` property uses `DateTime`. Care should be taken to ensure consistent time zone handling (UTC vs. Local) when aggregating multiple `GatewayStatistics` instances across different nodes or storage systems.
