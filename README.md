
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
