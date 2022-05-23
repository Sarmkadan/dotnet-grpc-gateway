// ... (rest of the file remains the same)

## HealthChecksAndMonitoringExampleExtensions

The `HealthChecksAndMonitoringExampleExtensions` class provides extension methods for the `HealthChecksAndMonitoringExample` type. These methods enhance health check and monitoring functionality, allowing for detailed analysis of service health, performance metrics, and error tracking.

### Example Usage:

```csharp
public class Program
{
  public static async Task Main(string[] args)
  {
    var example = new HealthChecksAndMonitoringExample();

    // Check health status of multiple services
    var serviceIds = new[] { 1, 2, 3 };
    var healthStatus = await HealthChecksAndMonitoringExampleExtensions.CheckMultipleServicesHealthAsync(example, serviceIds);
    Console.WriteLine("Health Status:");
    foreach (var kvp in healthStatus)
    {
      Console.WriteLine($"Service {kvp.Key}: {(kvp.Value ? "Healthy" : "Unhealthy")}");
    }

    // Get a detailed health report
    var healthReport = await HealthChecksAndMonitoringExampleExtensions.GetHealthReportAsync(example);
    Console.WriteLine(healthReport);

    // Check if all services are healthy
    var allHealthy = await HealthChecksAndMonitoringExampleExtensions.AreAllServicesHealthyAsync(example, serviceIds);
    Console.WriteLine($"All services healthy: {allHealthy}");

    // Get detailed health status
    var detailedStatus = await HealthChecksAndMonitoringExampleExtensions.GetDetailedHealthStatusAsync(example);
    Console.WriteLine($"Uptime: {detailedStatus.Uptime}");
    Console.WriteLine($"Requests Processed: {detailedStatus.RequestsProcessed}");

    // Check health of a single service
    var serviceHealth = await HealthChecksAndMonitoringExampleExtensions.CheckServiceHealthAsync(example, 1);
    Console.WriteLine($"Service {serviceHealth.ServiceId} Health: {(serviceHealth.IsHealthy ? "Healthy" : "Unhealthy")}");
  }
}

// The types `HealthStatus` and `ServiceHealth` used in the example have the following properties:

* `HealthStatus`:
  - `Status`
  - `Uptime`
  - `RequestsProcessed`
  - `ActiveConnections`
  - `CacheHitRate`

* `ServiceHealth`:
  - `ServiceId`
  - `ServiceName`
  - `IsHealthy`
  - `ResponseTimeMs`
  - `LastCheckAt`
  - `ConsecutiveFailures`
  - `FailureThreshold`
```

## DynamicRouteConfigurationExampleExtensions

`DynamicRouteConfigurationExampleExtensions` adds a set of helper methods to the `DynamicRouteConfigurationExample` class for bulk route management and performance analysis. It lets you create, update, and delete multiple routes in one call, retrieve the gateway URL, create a route with rich metadata, and fetch per‑route performance metrics encapsulated in `RoutePerformanceMetrics`.

### Example Usage

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetGrpcGateway.Examples;

public class Program
{
    public static async Task Main(string[] args)
    {
        var example = new DynamicRouteConfigurationExample();

        // Create multiple routes
        var routes = new[]
        {
            new { pattern = "/api/v1/users", targetServiceId = 1 },
            new { pattern = "/api/v1/orders", targetServiceId = 2 }
        };
        await example.CreateMultipleRoutesAsync(routes);

        // Update multiple routes
        var updates = new[]
        {
            new { pattern = "/api/v1/users", targetServiceId = 1, priority = 10 },
            new { pattern = "/api/v1/orders", targetServiceId = 2, priority = 20 }
        };
        await example.UpdateMultipleRoutesAsync(updates);

        // Delete routes by IDs
        await example.DeleteMultipleRoutesAsync(new[] { 3, 4 });

        // Create a route with additional metadata
        await example.CreateRouteWithMetadataAsync(
            pattern: "/api/v1/products",
            targetServiceId: 3,
            priority: 5,
            rateLimitPerMinute: 500,
            enableCaching: true,
            description: "Products endpoint",
            tags: new[] { "product", "v1" });

        // Analyze performance of all routes
        var metrics = await example.AnalyzeRoutePerformanceAsync(durationMinutes: 10);
        foreach (var kvp in metrics)
        {
            var m = kvp.Value;
            Console.WriteLine(
                $"Route {kvp.Key}: " +
                $"Requests={m.RequestCount}, " +
                $"AvgLatency={m.AverageLatencyMs}ms, " +
                $"ErrorRate={m.ErrorRate:P}, " +
                $"CacheHitRate={m.CacheHitRate:P}, " +
                $"MaxConcurrent={m.MaxConcurrentRequests}");
        }

        // Retrieve the gateway URL
        var gatewayUrl = example.GetGatewayUrl();
        Console.WriteLine($"Gateway URL: {gatewayUrl}");
    }
}
```
