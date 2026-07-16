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

        // Discover available services
        var availableServices = await serviceDiscoveryService.DiscoverAvailableServicesAsync();
        Console.WriteLine($"Available services: {string.Join(", ", availableServices.Select(s => s.Name))}");

        // Get the health status of all services
        var allServicesHealth = await serviceDiscoveryService.GetAllServicesHealthAsync();
        Console.WriteLine($"Health status of all services: {string.Join(", ", allServicesHealth.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");
    }
}
```

