// src/dotnet-grpc-gateway/README.md
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

## ServiceHealthReport

`ServiceHealthReport` represents health check results for a gRPC service, tracking consecutive success/failure patterns, response times, error details, and diagnostic information. It's used by the gateway's health monitoring system to determine service availability and trigger alerts.

### Example Usage

```csharp
using System;
using System.Collections.Generic;
using DotNetGrpcGateway.Domain;

class Program
{
    static void Main()
    {
        // Create a health report for a gRPC service
        var healthReport = new ServiceHealthReport
        {
            Id = 1,
            ServiceId = 10,
            IsHealthy = true,
            HealthStatus = "Healthy",
            ResponseTimeMs = 45,
            HttpStatusCode = 200,
            ErrorMessage = null,
            StackTrace = null,
            SuccessfulChecksInARow = 5,
            FailedChecksInARow = 0,
            TotalHealthChecks = 25,
            SuccessfulHealthChecks = 24,
            HealthCheckSuccessRate = 96.0,
            LastCheckAt = DateTime.UtcNow.AddMinutes(-2),
            NextCheckScheduledAt = DateTime.UtcNow.AddSeconds(30),
            HealthCheckEndpoint = "https://localhost:5001/health",
            ReportedAt = DateTime.UtcNow,
            DiagnosticMessages = new List<string> { "All checks passed" }
        };

        // Validate the report
        healthReport.Validate();
        Console.WriteLine($"Service {healthReport.ServiceId} health status: {healthReport.HealthStatus}");
        Console.WriteLine($"Success rate: {healthReport.HealthCheckSuccessRate:F1}%");
        Console.WriteLine($"Consecutive successes: {healthReport.SuccessfulChecksInARow}");

        // Record a new health check result
        healthReport.RecordCheckResult(
            success: true,
            responseTimeMs: 38,
            errorMessage: null
        );

        Console.WriteLine($"\nAfter recording check:");
        Console.WriteLine($"Healthy: {healthReport.IsHealthy}");
        Console.WriteLine($"Success rate: {healthReport.HealthCheckSuccessRate:F1}%");
        Console.WriteLine($"Total checks: {healthReport.TotalHealthChecks}");
        Console.WriteLine($"Consecutive successes: {healthReport.SuccessfulChecksInARow}");

        // Add diagnostic message
        healthReport.AddDiagnosticMessage("Service responding within SLA");
        Console.WriteLine($"\nDiagnostic messages: {string.Join(", ", healthReport.DiagnosticMessages)}");
    }
}
```

## ServiceReflectionInfo

The `ServiceReflectionInfo` class holds metadata about a gRPC service discovered through Server Reflection. It provides information about the service's methods, response times, and availability.

### Example Usage

```csharp
using System;
using System.Collections.Generic;
using DotNetGrpcGateway.Domain;

class Program
{
    static void Main()
    {
        var reflectionInfo = new ServiceReflectionInfo
        {
            ServiceId = 10,
            ServiceName = "MyService",
            ServiceFullName = "MyPackage.MyService",
            Methods = new List<ServiceMethodDescriptor>
            {
                new ServiceMethodDescriptor
                {
                    Name = "MyMethod",
                    RequestType = "MyPackage.MyRequest",
                    ResponseType = "MyPackage.MyResponse",
                    IsClientStreaming = false,
                    IsServerStreaming = false
                }
            },
            ReflectedAt = DateTime.UtcNow,
            IsAvailable = true
        };

        Console.WriteLine($"Service {reflectionInfo.ServiceName} ({reflectionInfo.ServiceFullName}) has {reflectionInfo.MethodCount} methods.");
        Console.WriteLine($"  - Available: {reflectionInfo.IsAvailable}");
        Console.WriteLine($"  - Reflected at: {reflectionInfo.ReflectedAt}");

        foreach (var method in reflectionInfo.Methods)
        {
            Console.WriteLine($"  - Method: {method.Name} ({method.RequestType} -> {method.ResponseType}) - {method.StreamingMode}");
        }
    }
}
```

## ServiceEndpoint

The `ServiceEndpoint` class represents a single addressable endpoint for a registered gRPC service. It tracks endpoint health, load balancing weights, and request statistics.

### Example Usage

```csharp
using DotNetGrpcGateway.Domain;

class Program
{
    static void Main()
    {
        var endpoint = new ServiceEndpoint
        {
            Id = 1,
            ServiceId = 10,
            Host = "localhost",
            Port = 5001,
            UseTls = true,
            Weight = 2
        };

        Console.WriteLine($"Endpoint URI: {endpoint.GetUri()}");
        Console.WriteLine($"Healthy: {endpoint.IsHealthy}");

        endpoint.RecordRequest(50.0, true);
        Console.WriteLine($"Total requests: {endpoint.TotalRequestsHandled}");
        Console.WriteLine($"Average response time: {endpoint.AverageResponseTimeMs}ms");

        endpoint.IsHealthy = false;
        Console.WriteLine($"Healthy: {endpoint.IsHealthy}");
    }
}
```

## IRouteResolutionService

`IRouteResolutionService` is responsible for mapping incoming gRPC requests to the appropriate target backend service based on configured routes. It manages route lookup, validation, and caching to ensure efficient request routing within the gateway.

### Example Usage

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;

class Program
{
    static async Task Main(IServiceProvider serviceProvider)
    {
        var routeResolutionService = serviceProvider.GetRequiredService<IRouteResolutionService>();

        // 1. Resolve a route for a request
        var route = await routeResolutionService.ResolveRouteAsync("MyService", "MyMethod");
        Console.WriteLine($"Resolved route to service ID: {route.TargetServiceId}");

        // 2. Resolve the actual target service details
        var targetService = await routeResolutionService.ResolveTargetServiceAsync(route.TargetServiceId);
        Console.WriteLine($"Target service: {targetService.Name} at {targetService.Host}:{targetService.Port}");

        // 3. Find a matching route (returns null if not found)
        var matchingRoute = await routeResolutionService.FindMatchingRouteAsync("MyService", "MyMethod");
        if (matchingRoute != null)
        {
            Console.WriteLine($"Matching route priority: {matchingRoute.Priority}");
        }

        // 4. Validate access to a specific route
        await routeResolutionService.ValidateRouteAccessAsync(route, "client-123");

        // 5. Manage cache
        Console.WriteLine($"Cached routes count: {routeResolutionService.GetCachedRouteCount()}");
        routeResolutionService.ClearRouteCache();
        Console.WriteLine("Route cache cleared.");
    }
}
```

## IGrpcClientFactory

`IGrpcClientFactory` is a factory for creating and caching HTTP clients for downstream gRPC service communication. It manages per-service client lifecycle, TLS configuration, and provides both unary and server-streaming invocation methods.

### Example Usage

```csharp
using System;
using System.Threading.Tasks;
using DotNetGrpcGateway.Domain;

class Program
{
    static async Task Main()
    {
        var factory = new GrpcClientFactory(new HttpClient(), new Logger<GrpcClientFactory>());
        var service = new GrpcService
        {
            Id = 10,
            Name = "MyService",
            ServiceFullName = "MyPackage.MyService",
            UseTls = true,
            Host = "localhost",
            Port = 5001
        };

        var client = factory.CreateHttpClient(service);
        var response = await client.GetAsync("/MyService/MyMethod");
        Console.WriteLine($"Response status: {response.StatusCode}");

        var result = await factory.InvokeAsync<int>(service, "MyMethod", new object(), default);
        Console.WriteLine($"Result: {result}");

        var stream = await factory.InvokeStreamingAsync(service, "MyMethod", new object(), default);
        Console.WriteLine($"Stream: {stream}");
    }
}
```

## IServiceDiscoveryService

`IServiceDiscoveryService` is responsible for discovering and monitoring the health of registered gRPC services. It provides methods for performing health checks, retrieving the latest health report, updating service health status, discovering available services, and retrieving the health status of all services.

### Example Usage

```csharp
using System;
using System.Threading.Tasks;
using DotNetGrpcGateway.Domain;

class Program
{
    static async Task Main()
    {
        var serviceDiscoveryService = new ServiceDiscoveryService(new UnitOfWork(), new Logger<ServiceDiscoveryService>());
        var serviceId = 10;

        // Perform a health check on the service
        var healthReport = await serviceDiscoveryService.PerformHealthCheckAsync(serviceId);
        Console.WriteLine($"Health report for service {serviceId}: {healthReport.HealthStatus}");

        // Get the latest health report for the service
        var latestHealthReport = await serviceDiscoveryService.GetLatestHealthReportAsync(serviceId);
        Console.WriteLine($"Latest health report for service {serviceId}: {latestHealthReport.HealthStatus}");

        // Update the health status of the service
        await serviceDiscoveryService.UpdateServiceHealthAsync(serviceId, true);
        Console.WriteLine($"Health status of service {serviceId} updated to: Healthy");

        // Get the health status of all services
        var allServicesHealth = await serviceDiscoveryService.GetAllServicesHealthAsync();
        Console.WriteLine($"Health status of all services: {string.Join(", ", allServicesHealth.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");
    }
}

## IGatewayService

`IGatewayService` is the core interface for managing the gateway's configuration, registering and unregistering gRPC services, and managing routing rules. It serves as the primary entry point for administrative operations on the gateway.

### Example Usage

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;

class Program
{
    static async Task Main(IGatewayService gatewayService)
    {
        // 1. Get current configuration
        var config = await gatewayService.GetConfigurationAsync();
        Console.WriteLine($"Gateway Name: {config.Name}");

        // 2. Register a new service
        var newService = new GrpcService { Name = "MyNewService", Host = "localhost", Port = 5005 };
        await gatewayService.RegisterServiceAsync(newService);

        // 3. Get all healthy services
        var healthyServices = await gatewayService.GetHealthyServicesAsync();
        Console.WriteLine($"Healthy services: {healthyServices.Count}");

        // 4. Add a new route
        var newRoute = new GatewayRoute { Name = "MyNewRoute", TargetServiceId = newService.Id, Path = "/api" };
        await gatewayService.AddRouteAsync(newRoute);

        // 5. Unregister a service
        await gatewayService.UnregisterServiceAsync(newService.Id);
    }
}
```

## IMetricsCollectionService

`IMetricsCollectionService` is responsible for collecting, aggregating, and analyzing metrics related to gateway requests and service performance. It provides methods for recording request metrics, retrieving statistics, identifying slow requests, and analyzing service usage patterns.

### Example Usage

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;

class Program
{
  static async Task Main(IServiceProvider serviceProvider)
  {
    var metricsService = serviceProvider.GetRequiredService<IMetricsCollectionService>();

    // 1. Record a request metric
    var metric = new RequestMetric
    {
      ServiceName = "UserService",
      MethodName = "GetUserById",
      DurationMs = 45.2,
      IsSuccessful = true,
      RequestSizeBytes = 1024,
      GrpcStatusCode = "OK",
      Timestamp = DateTime.UtcNow
    };
    await metricsService.RecordRequestMetricAsync(metric);

    // 2. Get today's statistics
    var todayStats = await metricsService.GetTodayStatisticsAsync();
    Console.WriteLine($"Today's requests: {todayStats.TotalRequests}");
    Console.WriteLine($"Success rate: {todayStats.SuccessRate:P}");
    Console.WriteLine($"Average response time: {todayStats.AverageResponseTimeMs}ms");

    // 3. Get statistics for a specific date
    var yesterdayStats = await metricsService.GetStatisticsAsync(DateTime.UtcNow.AddDays(-1));
    Console.WriteLine($"Yesterday's requests: {yesterdayStats.TotalRequests}");

    // 4. Get slow requests (threshold: 1000ms)
    var slowRequests = await metricsService.GetSlowRequestsAsync(1000);
    Console.WriteLine($"Slow requests (>1000ms): {slowRequests.Count}");

    // 5. Get requests per service
    var requestsPerService = await metricsService.GetRequestsPerServiceAsync();
    foreach (var kvp in requestsPerService)
    {
      Console.WriteLine($"{kvp.Key}: {kvp.Value} requests");
    }

    // 6. Get average response time
    var avgResponseTime = await metricsService.GetAverageResponseTimeAsync();
    Console.WriteLine($"Average response time: {avgResponseTime}ms");

    // 7. Update service metrics
    await metricsService.UpdateServiceMetricsAsync(1, 75.5, true);
  }
}
```

## IRequestMetricsAnalyzerService

`IRequestMetricsAnalyzerService` analyzes request metrics to identify patterns, trends, and potential issues. It provides methods for analyzing overall request patterns, evaluating individual endpoint health, and detecting anomalies in service behavior. This service helps identify performance bottlenecks, track service health, and generate actionable insights from gateway metrics.

### Example Usage

```csharp
using System;
using System.Threading.Tasks;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;

class Program
{
static async Task Main()
{
// Setup dependency injection
var services = new ServiceCollection();
services.AddSingleton<IRequestMetricsAnalyzerService, RequestMetricsAnalyzerService>();
services.AddSingleton<IMetricsCollectionService, MetricsCollectionService>();
services.AddLogging();

var serviceProvider = services.BuildServiceProvider();
var analyzerService = serviceProvider.GetRequiredService<IRequestMetricsAnalyzerService>();

// 1. Analyze overall request patterns
var patternAnalysis = await analyzerService.AnalyzeRequestPatternsAsync();
Console.WriteLine($"Total requests: {patternAnalysis.TotalRequests}");
Console.WriteLine($"Success rate: {patternAnalysis.SuccessRate:P}");
Console.WriteLine($"Average response time: {patternAnalysis.AverageResponseTime:F1}ms");
Console.WriteLine($"Most accessed endpoint: {patternAnalysis.MostAccessedEndpoint ?? "N/A"}");
Console.WriteLine($"Slowest endpoint: {patternAnalysis.SlowestEndpoint ?? "N/A"}");

// 2. Analyze specific endpoint health
var endpointHealth = await analyzerService.AnalyzeEndpointHealthAsync("UserService/GetUserById");
Console.WriteLine($"\nEndpoint: {endpointHealth.Endpoint}");
Console.WriteLine($"Health score: {endpointHealth.HealthScore:F1}/100");
Console.WriteLine($"Status: {endpointHealth.Status}");
Console.WriteLine($"Request count: {endpointHealth.RequestCount}");
Console.WriteLine($"Success rate: {endpointHealth.SuccessRate:P}");
Console.WriteLine($"Average response time: {endpointHealth.AverageResponseTime:F1}ms");

// 3. Detect anomalies
var anomalies = await analyzerService.DetectAnomaliesAsync();
Console.WriteLine($"\nDetected {anomalies.Count} anomalies:");
foreach (var alert in anomalies)
{
Console.WriteLine($" - [{alert.Severity}] {alert.AlertType}: {alert.Message}");
Console.WriteLine($"   Detected at: {alert.DetectedAt}");
}
}
}
```

