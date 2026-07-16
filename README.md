// src/dotnet-grpc-gateway/README.md
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

## Architecture

For the big picture - project layout, middleware pipeline order, core abstractions,
persistence trade-offs and extension points - see
[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md). The rest of this file is per-type
API reference.

## DotnetGrpcGatewayOptions

`DotnetGrpcGatewayOptions` provides centralized configuration for the gRPC gateway, controlling network binding, reflection, metrics, health checks, request logging, and performance settings. It supports dependency injection and can be configured from configuration files using the `Gateway` section name.

### Example Usage

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using DotNetGrpcGateway.Options;

// 1. Configure from appsettings.json
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var services = new ServiceCollection();

// Bind configuration to options
services.Configure<DotnetGrpcGatewayOptions>(
    configuration.GetSection(DotnetGrpcGatewayOptions.SectionName));

// Or configure manually
services.Configure<DotnetGrpcGatewayOptions>(options =>
{
    options.ListenAddress = "0.0.0.0";
    options.Port = 8080;
    options.EnableReflection = true;
    options.EnableMetrics = true;
    options.MaxConcurrentConnections = 2000;
    options.RequestTimeoutMs = 60000;
    options.LogLevel = "Debug";
    options.EnableCompression = true;
    
    // Configure health checks
    options.HealthCheck.IntervalSeconds = 60;
    options.HealthCheck.TimeoutMs = 10000;
    options.HealthCheck.FailureThreshold = 5;
    
    // Configure metrics
    options.Metrics.EnableMetrics = true;
    options.Metrics.CollectionIntervalSeconds = 30;
    options.Metrics.RetentionDays = 60;
    
    // Configure request logging
    options.RequestLogging.Enabled = true;
    options.RequestLogging.Verbosity = RequestLoggingVerbosity.Verbose;
});

var serviceProvider = services.BuildServiceProvider();

// 2. Access configured options
var options = serviceProvider.GetRequiredService<IOptions<DotnetGrpcGatewayOptions>>().Value;

Console.WriteLine($"Gateway listening on {options.ListenAddress}:{options.Port}");
Console.WriteLine($"Reflection enabled: {options.EnableReflection}");
Console.WriteLine($"Metrics enabled: {options.EnableMetrics}");
Console.WriteLine($"Max concurrent connections: {options.MaxConcurrentConnections}");
Console.WriteLine($"Request timeout: {options.RequestTimeoutMs}ms");
Console.WriteLine($"Log level: {options.LogLevel}");
Console.WriteLine($"Compression enabled: {options.EnableCompression}");
Console.WriteLine($"Health check interval: {options.HealthCheck.IntervalSeconds}s");
Console.WriteLine($"Health check timeout: {options.HealthCheck.TimeoutMs}ms");
Console.WriteLine($"Health check threshold: {options.HealthCheck.FailureThreshold}");
Console.WriteLine($"Metrics collection interval: {options.Metrics.CollectionIntervalSeconds}s");
Console.WriteLine($"Metrics retention: {options.Metrics.RetentionDays} days");
Console.WriteLine($"Request logging enabled: {options.RequestLogging.Enabled}");
Console.WriteLine($"Request logging verbosity: {options.RequestLogging.Verbosity}");

// 3. Validate configuration
if (string.IsNullOrEmpty(options.ListenAddress))
{
    throw new InvalidOperationException("ListenAddress is required");
}

if (options.Port < 1 || options.Port > 65535)
{
    throw new InvalidOperationException("Port must be between 1 and 65535");
}
```

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

## IRequestLogService

`IRequestLogService` stores and provides queryable access to recent gateway request/response log entries. It maintains a fixed-capacity ring buffer of request logs that can be queried by method, status code, or time range. The service provides aggregate statistics including success rate, response times, and entry timestamps, making it ideal for monitoring, debugging, and performance analysis of gRPC gateway traffic.

### Example Usage

```csharp
using System;
using System.Linq;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static void Main()
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddLogging(configure => configure.AddConsole());
        services.AddSingleton<IRequestLogService, RequestLogService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var requestLogService = serviceProvider.GetRequiredService<IRequestLogService>();

        // 1. Log a successful request
        var successfulEntry = new RequestLogEntry
        {
            GrpcMethod = "UserService/GetUserById",
            StatusCode = 0, // OK
            DurationMs = 23.5,
            IsSuccess = true,
            RequestSizeBytes = 1024,
            ResponseSizeBytes = 2048,
            Timestamp = DateTime.UtcNow,
            ClientIpAddress = "192.168.1.100"
        };
        requestLogService.Append(successfulEntry);

        // 2. Log a failed request
        var failedEntry = new RequestLogEntry
        {
            GrpcMethod = "PaymentService/ProcessPayment",
            StatusCode = 3, // INVALID_ARGUMENT
            DurationMs = 156.8,
            IsSuccess = false,
            RequestSizeBytes = 512,
            ResponseSizeBytes = 128,
            Timestamp = DateTime.UtcNow.AddSeconds(-10),
            ClientIpAddress = "192.168.1.101",
            ErrorMessage = "Insufficient funds"
        };
        requestLogService.Append(failedEntry);

        // 3. Get recent log entries
        var recentEntries = requestLogService.GetRecent(limit: 10);
        Console.WriteLine($"Recent entries: {recentEntries.Count}");
        foreach (var entry in recentEntries.Take(3))
        {
            Console.WriteLine($" - {entry.Timestamp:HH:mm:ss} | {entry.GrpcMethod} | {entry.DurationMs}ms | {(entry.IsSuccess ? "SUCCESS" : "FAILED")}");
        }

        // 4. Search logs by method
        var userServiceEntries = requestLogService.Search(methodFilter: "UserService");
        Console.WriteLine($"\nUserService entries: {userServiceEntries.Count}");

        // 5. Get aggregate statistics
        var summary = requestLogService.GetSummary();
        Console.WriteLine($"\nStatistics:");
        Console.WriteLine($" - Total entries: {summary.TotalEntries}");
        Console.WriteLine($" - Success count: {summary.SuccessCount}");
        Console.WriteLine($" - Error count: {summary.ErrorCount}");
        Console.WriteLine($" - Success rate: {summary.SuccessRatePct:F1}%");
        Console.WriteLine($" - Average duration: {summary.AverageDurationMs:F1}ms");
        Console.WriteLine($" - Min duration: {summary.MinDurationMs}ms");
        Console.WriteLine($" - Max duration: {summary.MaxDurationMs}ms");
        Console.WriteLine($" - Oldest entry: {summary.OldestEntry?.ToString("yyyy-MM-dd HH:mm:ss")}");
        Console.WriteLine($" - Newest entry: {summary.NewestEntry?.ToString("yyyy-MM-dd HH:mm:ss")}");

        // 6. Search logs by status code
        var errorEntries = requestLogService.Search(statusCode: 3);
        Console.WriteLine($"\nError entries (status code 3): {errorEntries.Count}");

        // 7. Search logs by time range
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var recentSuccessfulEntries = requestLogService.Search(
            methodFilter: "UserService",
            from: oneHourAgo,
            limit: 50
        );
        Console.WriteLine($"\nRecent successful UserService entries: {recentSuccessfulEntries.Count(re => re.IsSuccess)}");

        // 8. Clear all logs
        requestLogService.Clear();
        Console.WriteLine($"\nAfter clearing - Total entries: {requestLogService.GetSummary().TotalEntries}");
    }
}
```

### Example Usage

```csharp
using System;
using System.Threading.Tasks;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(IServiceProvider serviceProvider)
    {
        var requestLogService = serviceProvider.GetRequiredService<IRequestLogService>();

        // Simulate logging requests from a gateway middleware
        var gatewayMiddleware = new GatewayMiddleware(requestLogService);
        
        // Process a successful request
        await gatewayMiddleware.ProcessRequestAsync("UserService", "GetUserById", 23.5, true);
        
        // Process a failed request
        await gatewayMiddleware.ProcessRequestAsync("PaymentService", "ProcessPayment", 156.8, false);
        
        // Get performance metrics
        var summary = requestLogService.GetSummary();
        Console.WriteLine($"Gateway performance - Success rate: {summary.SuccessRatePct:F1}%");
        Console.WriteLine($"Average response time: {summary.AverageDurationMs:F1}ms");
    }
}

class GatewayMiddleware
{
    private readonly IRequestLogService _requestLogService;
    
    public GatewayMiddleware(IRequestLogService requestLogService)
    {
        _requestLogService = requestLogService;
    }
    
    public async Task ProcessRequestAsync(string serviceName, string methodName, double durationMs, bool isSuccess)
    {
        var entry = new RequestLogEntry
        {
            GrpcMethod = $"{serviceName}/{methodName}",
            StatusCode = isSuccess ? 0 : 3,
            DurationMs = durationMs,
            IsSuccess = isSuccess,
            Timestamp = DateTime.UtcNow,
            ClientIpAddress = "192.168.1.100"
        };
        
        _requestLogService.Append(entry);
        
        if (!isSuccess)
        {
            Console.WriteLine($"Request failed: {serviceName}/{methodName} in {durationMs}ms");
        }
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

## IRequestLogService

`IRequestLogService` stores and provides queryable access to recent gateway request/response log entries. It maintains a fixed-capacity ring buffer of request logs that can be queried by method, status code, or time range. The service provides aggregate statistics including success rate, response times, and entry timestamps, making it ideal for monitoring, debugging, and performance analysis of gRPC gateway traffic.

### Example Usage

```csharp
using System;
using System.Linq;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static void Main()
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddLogging(configure => configure.AddConsole());
        services.AddSingleton<IRequestLogService, RequestLogService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var requestLogService = serviceProvider.GetRequiredService<IRequestLogService>();

        // 1. Log a successful request
        var successfulEntry = new RequestLogEntry
        {
            GrpcMethod = "UserService/GetUserById",
            StatusCode = 0, // OK
            DurationMs = 23.5,
            IsSuccess = true,
            RequestSizeBytes = 1024,
            ResponseSizeBytes = 2048,
            Timestamp = DateTime.UtcNow,
            ClientIpAddress = "192.168.1.100"
        };
        requestLogService.Append(successfulEntry);

        // 2. Log a failed request
        var failedEntry = new RequestLogEntry
        {
            GrpcMethod = "PaymentService/ProcessPayment",
            StatusCode = 3, // INVALID_ARGUMENT
            DurationMs = 156.8,
            IsSuccess = false,
            RequestSizeBytes = 512,
            ResponseSizeBytes = 128,
            Timestamp = DateTime.UtcNow.AddSeconds(-10),
            ClientIpAddress = "192.168.1.101",
            ErrorMessage = "Insufficient funds"
        };
        requestLogService.Append(failedEntry);

        // 3. Get recent log entries
        var recentEntries = requestLogService.GetRecent(limit: 10);
        Console.WriteLine($"Recent entries: {recentEntries.Count}");
        foreach (var entry in recentEntries.Take(3))
        {
            Console.WriteLine($" - {entry.Timestamp:HH:mm:ss} | {entry.GrpcMethod} | {entry.DurationMs}ms | {(entry.IsSuccess ? "SUCCESS" : "FAILED")}");
        }

        // 4. Search logs by method
        var userServiceEntries = requestLogService.Search(methodFilter: "UserService");
        Console.WriteLine($"\nUserService entries: {userServiceEntries.Count}");

        // 5. Get aggregate statistics
        var summary = requestLogService.GetSummary();
        Console.WriteLine($"\nStatistics:");
        Console.WriteLine($" - Total entries: {summary.TotalEntries}");
        Console.WriteLine($" - Success count: {summary.SuccessCount}");
        Console.WriteLine($" - Error count: {summary.ErrorCount}");
        Console.WriteLine($" - Success rate: {summary.SuccessRatePct:F1}%");
        Console.WriteLine($" - Average duration: {summary.AverageDurationMs:F1}ms");
        Console.WriteLine($" - Min duration: {summary.MinDurationMs}ms");
        Console.WriteLine($" - Max duration: {summary.MaxDurationMs}ms");
        Console.WriteLine($" - Oldest entry: {summary.OldestEntry?.ToString("yyyy-MM-dd HH:mm:ss")}");
        Console.WriteLine($" - Newest entry: {summary.NewestEntry?.ToString("yyyy-MM-dd HH:mm:ss")}");

        // 6. Search logs by status code
        var errorEntries = requestLogService.Search(statusCode: 3);
        Console.WriteLine($"\nError entries (status code 3): {errorEntries.Count}");

        // 7. Search logs by time range
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var recentSuccessfulEntries = requestLogService.Search(
            methodFilter: "UserService",
            from: oneHourAgo,
            limit: 50
        );
        Console.WriteLine($"\nRecent successful UserService entries: {recentSuccessfulEntries.Count(re => re.IsSuccess)}");

        // 8. Clear all logs
        requestLogService.Clear();
        Console.WriteLine($"\nAfter clearing - Total entries: {requestLogService.GetSummary().TotalEntries}");
    }
}
```

### Example Usage

```csharp
using System;
using System.Threading.Tasks;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(IServiceProvider serviceProvider)
    {
        var requestLogService = serviceProvider.GetRequiredService<IRequestLogService>();

        // Simulate logging requests from a gateway middleware
        var gatewayMiddleware = new GatewayMiddleware(requestLogService);
        
        // Process a successful request
        await gatewayMiddleware.ProcessRequestAsync("UserService", "GetUserById", 23.5, true);
        
        // Process a failed request
        await gatewayMiddleware.ProcessRequestAsync("PaymentService", "ProcessPayment", 156.8, false);
        
        // Get performance metrics
        var summary = requestLogService.GetSummary();
        Console.WriteLine($"Gateway performance - Success rate: {summary.SuccessRatePct:F1}%");
        Console.WriteLine($"Average response time: {summary.AverageDurationMs:F1}ms");
    }
}

class GatewayMiddleware
{
    private readonly IRequestLogService _requestLogService;
    
    public GatewayMiddleware(IRequestLogService requestLogService)
    {
        _requestLogService = requestLogService;
    }
    
    public async Task ProcessRequestAsync(string serviceName, string methodName, double durationMs, bool isSuccess)
    {
        var entry = new RequestLogEntry
        {
            GrpcMethod = $"{serviceName}/{methodName}",
            StatusCode = isSuccess ? 0 : 3,
            DurationMs = durationMs,
            IsSuccess = isSuccess,
            Timestamp = DateTime.UtcNow,
            ClientIpAddress = "192.168.1.100"
        };
        
        _requestLogService.Append(entry);
        
        if (!isSuccess)
        {
            Console.WriteLine($"Request failed: {serviceName}/{methodName} in {durationMs}ms");
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

## IRequestLogService

`IRequestLogService` stores and provides queryable access to recent gateway request/response log entries. It maintains a fixed-capacity ring buffer of request logs that can be queried by method, status code, or time range. The service provides aggregate statistics including success rate, response times, and entry timestamps, making it ideal for monitoring, debugging, and performance analysis of gRPC gateway traffic.

### Example Usage

```csharp
using System;
using System.Linq;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static void Main()
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddLogging(configure => configure.AddConsole());
        services.AddSingleton<IRequestLogService, RequestLogService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var requestLogService = serviceProvider.GetRequiredService<IRequestLogService>();

        // 1. Log a successful request
        var successfulEntry = new RequestLogEntry
        {
            GrpcMethod = "UserService/GetUserById",
            StatusCode = 0, // OK
            DurationMs = 23.5,
            IsSuccess = true,
            RequestSizeBytes = 1024,
            ResponseSizeBytes = 2048,
            Timestamp = DateTime.UtcNow,
            ClientIpAddress = "192.168.1.100"
        };
        requestLogService.Append(successfulEntry);

        // 2. Log a failed request
        var failedEntry = new RequestLogEntry
        {
            GrpcMethod = "PaymentService/ProcessPayment",
            StatusCode = 3, // INVALID_ARGUMENT
            DurationMs = 156.8,
            IsSuccess = false,
            RequestSizeBytes = 512,
            ResponseSizeBytes = 128,
            Timestamp = DateTime.UtcNow.AddSeconds(-10),
            ClientIpAddress = "192.168.1.101",
            ErrorMessage = "Insufficient funds"
        };
        requestLogService.Append(failedEntry);

        // 3. Get recent log entries
        var recentEntries = requestLogService.GetRecent(limit: 10);
        Console.WriteLine($"Recent entries: {recentEntries.Count}");
        foreach (var entry in recentEntries.Take(3))
        {
            Console.WriteLine($" - {entry.Timestamp:HH:mm:ss} | {entry.GrpcMethod} | {entry.DurationMs}ms | {(entry.IsSuccess ? "SUCCESS" : "FAILED")}");
        }

        // 4. Search logs by method
        var userServiceEntries = requestLogService.Search(methodFilter: "UserService");
        Console.WriteLine($"\nUserService entries: {userServiceEntries.Count}");

        // 5. Get aggregate statistics
        var summary = requestLogService.GetSummary();
        Console.WriteLine($"\nStatistics:");
        Console.WriteLine($" - Total entries: {summary.TotalEntries}");
        Console.WriteLine($" - Success count: {summary.SuccessCount}");
        Console.WriteLine($" - Error count: {summary.ErrorCount}");
        Console.WriteLine($" - Success rate: {summary.SuccessRatePct:F1}%");
        Console.WriteLine($" - Average duration: {summary.AverageDurationMs:F1}ms");
        Console.WriteLine($" - Min duration: {summary.MinDurationMs}ms");
        Console.WriteLine($" - Max duration: {summary.MaxDurationMs}ms");
        Console.WriteLine($" - Oldest entry: {summary.OldestEntry?.ToString("yyyy-MM-dd HH:mm:ss")}");
        Console.WriteLine($" - Newest entry: {summary.NewestEntry?.ToString("yyyy-MM-dd HH:mm:ss")}");

        // 6. Search logs by status code
        var errorEntries = requestLogService.Search(statusCode: 3);
        Console.WriteLine($"\nError entries (status code 3): {errorEntries.Count}");

        // 7. Search logs by time range
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var recentSuccessfulEntries = requestLogService.Search(
            methodFilter: "UserService",
            from: oneHourAgo,
            limit: 50
        );
        Console.WriteLine($"\nRecent successful UserService entries: {recentSuccessfulEntries.Count(re => re.IsSuccess)}");

        // 8. Clear all logs
        requestLogService.Clear();
        Console.WriteLine($"\nAfter clearing - Total entries: {requestLogService.GetSummary().TotalEntries}");
    }
}
```

### Example Usage

```csharp
using System;
using System.Threading.Tasks;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(IServiceProvider serviceProvider)
    {
        var requestLogService = serviceProvider.GetRequiredService<IRequestLogService>();

        // Simulate logging requests from a gateway middleware
        var gatewayMiddleware = new GatewayMiddleware(requestLogService);
        
        // Process a successful request
        await gatewayMiddleware.ProcessRequestAsync("UserService", "GetUserById", 23.5, true);
        
        // Process a failed request
        await gatewayMiddleware.ProcessRequestAsync("PaymentService", "ProcessPayment", 156.8, false);
        
        // Get performance metrics
        var summary = requestLogService.GetSummary();
        Console.WriteLine($"Gateway performance - Success rate: {summary.SuccessRatePct:F1}%");
        Console.WriteLine($"Average response time: {summary.AverageDurationMs:F1}ms");
    }
}

class GatewayMiddleware
{
    private readonly IRequestLogService _requestLogService;
    
    public GatewayMiddleware(IRequestLogService requestLogService)
    {
        _requestLogService = requestLogService;
    }
    
    public async Task ProcessRequestAsync(string serviceName, string methodName, double durationMs, bool isSuccess)
    {
        var entry = new RequestLogEntry
        {
            GrpcMethod = $"{serviceName}/{methodName}",
            StatusCode = isSuccess ? 0 : 3,
            DurationMs = durationMs,
            IsSuccess = isSuccess,
            Timestamp = DateTime.UtcNow,
            ClientIpAddress = "192.168.1.100"
        };
        
        _requestLogService.Append(entry);
        
        if (!isSuccess)
        {
            Console.WriteLine($"Request failed: {serviceName}/{methodName} in {durationMs}ms");
        }
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

## IRequestLogService

`IRequestLogService` stores and provides queryable access to recent gateway request/response log entries. It maintains a fixed-capacity ring buffer of request logs that can be queried by method, status code, or time range. The service provides aggregate statistics including success rate, response times, and entry timestamps, making it ideal for monitoring, debugging, and performance analysis of gRPC gateway traffic.

### Example Usage

```csharp
using System;
using System.Linq;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static void Main()
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddLogging(configure => configure.AddConsole());
        services.AddSingleton<IRequestLogService, RequestLogService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var requestLogService = serviceProvider.GetRequiredService<IRequestLogService>();

        // 1. Log a successful request
        var successfulEntry = new RequestLogEntry
        {
            GrpcMethod = "UserService/GetUserById",
            StatusCode = 0, // OK
            DurationMs = 23.5,
            IsSuccess = true,
            RequestSizeBytes = 1024,
            ResponseSizeBytes = 2048,
            Timestamp = DateTime.UtcNow,
            ClientIpAddress = "192.168.1.100"
        };
        requestLogService.Append(successfulEntry);

        // 2. Log a failed request
        var failedEntry = new RequestLogEntry
        {
            GrpcMethod = "PaymentService/ProcessPayment",
            StatusCode = 3, // INVALID_ARGUMENT
            DurationMs = 156.8,
            IsSuccess = false,
            RequestSizeBytes = 512,
            ResponseSizeBytes = 128,
            Timestamp = DateTime.UtcNow.AddSeconds(-10),
            ClientIpAddress = "192.168.1.101",
            ErrorMessage = "Insufficient funds"
        };
        requestLogService.Append(failedEntry);

        // 3. Get recent log entries
        var recentEntries = requestLogService.GetRecent(limit: 10);
        Console.WriteLine($"Recent entries: {recentEntries.Count}");
        foreach (var entry in recentEntries.Take(3))
        {
            Console.WriteLine($" - {entry.Timestamp:HH:mm:ss} | {entry.GrpcMethod} | {entry.DurationMs}ms | {(entry.IsSuccess ? "SUCCESS" : "FAILED")}");
        }

        // 4. Search logs by method
        var userServiceEntries = requestLogService.Search(methodFilter: "UserService");
        Console.WriteLine($"\nUserService entries: {userServiceEntries.Count}");

        // 5. Get aggregate statistics
        var summary = requestLogService.GetSummary();
        Console.WriteLine($"\nStatistics:");
        Console.WriteLine($" - Total entries: {summary.TotalEntries}");
        Console.WriteLine($" - Success count: {summary.SuccessCount}");
        Console.WriteLine($" - Error count: {summary.ErrorCount}");
        Console.WriteLine($" - Success rate: {summary.SuccessRatePct:F1}%");
        Console.WriteLine($" - Average duration: {summary.AverageDurationMs:F1}ms");
        Console.WriteLine($" - Min duration: {summary.MinDurationMs}ms");
        Console.WriteLine($" - Max duration: {summary.MaxDurationMs}ms");
        Console.WriteLine($" - Oldest entry: {summary.OldestEntry?.ToString("yyyy-MM-dd HH:mm:ss")}");
        Console.WriteLine($" - Newest entry: {summary.NewestEntry?.ToString("yyyy-MM-dd HH:mm:ss")}");

        // 6. Search logs by status code
        var errorEntries = requestLogService.Search(statusCode: 3);
        Console.WriteLine($"\nError entries (status code 3): {errorEntries.Count}");

        // 7. Search logs by time range
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var recentSuccessfulEntries = requestLogService.Search(
            methodFilter: "UserService",
            from: oneHourAgo,
            limit: 50
        );
        Console.WriteLine($"\nRecent successful UserService entries: {recentSuccessfulEntries.Count(re => re.IsSuccess)}");

        // 8. Clear all logs
        requestLogService.Clear();
        Console.WriteLine($"\nAfter clearing - Total entries: {requestLogService.GetSummary().TotalEntries}");
    }
}
```

### Example Usage

```csharp
using System;
using System.Threading.Tasks;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(IServiceProvider serviceProvider)
    {
        var requestLogService = serviceProvider.GetRequiredService<IRequestLogService>();

        // Simulate logging requests from a gateway middleware
        var gatewayMiddleware = new GatewayMiddleware(requestLogService);
        
        // Process a successful request
        await gatewayMiddleware.ProcessRequestAsync("UserService", "GetUserById", 23.5, true);
        
        // Process a failed request
        await gatewayMiddleware.ProcessRequestAsync("PaymentService", "ProcessPayment", 156.8, false);
        
        // Get performance metrics
        var summary = requestLogService.GetSummary();
        Console.WriteLine($"Gateway performance - Success rate: {summary.SuccessRatePct:F1}%");
        Console.WriteLine($"Average response time: {summary.AverageDurationMs:F1}ms");
    }
}

class GatewayMiddleware
{
    private readonly IRequestLogService _requestLogService;
    
    public GatewayMiddleware(IRequestLogService requestLogService)
    {
        _requestLogService = requestLogService;
    }
    
    public async Task ProcessRequestAsync(string serviceName, string methodName, double durationMs, bool isSuccess)
    {
        var entry = new RequestLogEntry
        {
            GrpcMethod = $"{serviceName}/{methodName}",
            StatusCode = isSuccess ? 0 : 3,
            DurationMs = durationMs,
            IsSuccess = isSuccess,
            Timestamp = DateTime.UtcNow,
            ClientIpAddress = "192.168.1.100"
        };
        
        _requestLogService.Append(entry);
        
        if (!isSuccess)
        {
            Console.WriteLine($"Request failed: {serviceName}/{methodName} in {durationMs}ms");
        }
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

## IRequestLogService

`IRequestLogService` stores and provides queryable access to recent gateway request/response log entries. It maintains a fixed-capacity ring buffer of request logs that can be queried by method, status code, or time range. The service provides aggregate statistics including success rate, response times, and entry timestamps, making it ideal for monitoring, debugging, and performance analysis of gRPC gateway traffic.

### Example Usage

```csharp
using System;
using System.Linq;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static void Main()
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddLogging(configure => configure.AddConsole());
        services.AddSingleton<IRequestLogService, RequestLogService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var requestLogService = serviceProvider.GetRequiredService<IRequestLogService>();

        // 1. Log a successful request
        var successfulEntry = new RequestLogEntry
        {
            GrpcMethod = "UserService/GetUserById",
            StatusCode = 0, // OK
            DurationMs = 23.5,
            IsSuccess = true,
            RequestSizeBytes = 1024,
            ResponseSizeBytes = 2048,
            Timestamp = DateTime.UtcNow,
            ClientIpAddress = "192.168.1.100"
        };
        requestLogService.Append(successfulEntry);

        // 2. Log a failed request
        var failedEntry = new RequestLogEntry
        {
            GrpcMethod = "PaymentService/ProcessPayment",
            StatusCode = 3, // INVALID_ARGUMENT
            DurationMs = 156.8,
            IsSuccess = false,
            RequestSizeBytes = 512,
            ResponseSizeBytes = 128,
            Timestamp = DateTime.UtcNow.AddSeconds(-10),
            ClientIpAddress = "192.168.1.101",
            ErrorMessage = "Insufficient funds"
        };
        requestLogService.Append(failedEntry);

        // 3. Get recent log entries
        var recentEntries = requestLogService.GetRecent(limit: 10);
        Console.WriteLine($"Recent entries: {recentEntries.Count}");
        foreach (var entry in recentEntries.Take(3))
        {
            Console.WriteLine($" - {entry.Timestamp:HH:mm:ss} | {entry.GrpcMethod} | {entry.DurationMs}ms | {(entry.IsSuccess ? "SUCCESS" : "FAILED")}");
        }

        // 4. Search logs by method
        var userServiceEntries = requestLogService.Search(methodFilter: "UserService");
        Console.WriteLine($"\nUserService entries: {userServiceEntries.Count}");

        // 5. Get aggregate statistics
        var summary = requestLogService.GetSummary();
        Console.WriteLine($"\nStatistics:");
        Console.WriteLine($" - Total entries: {summary.TotalEntries}");
        Console.WriteLine($" - Success count: {summary.SuccessCount}");
        Console.WriteLine($" - Error count: {summary.ErrorCount}");
        Console.WriteLine($" - Success rate: {summary.SuccessRatePct:F1}%");
        Console.WriteLine($" - Average duration: {summary.AverageDurationMs:F1}ms");
        Console.WriteLine($" - Min duration: {summary.MinDurationMs}ms");
        Console.WriteLine($" - Max duration: {summary.MaxDurationMs}ms");
        Console.WriteLine($" - Oldest entry: {summary.OldestEntry?.ToString("yyyy-MM-dd HH:mm:ss")}");
        Console.WriteLine($" - Newest entry: {summary.NewestEntry?.ToString("yyyy-MM-dd HH:mm:ss")}");

        // 6. Search logs by status code
        var errorEntries = requestLogService.Search(statusCode: 3);
        Console.WriteLine($"\nError entries (status code 3): {errorEntries.Count}");

        // 7. Search logs by time range
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var recentSuccessfulEntries = requestLogService.Search(
            methodFilter: "UserService",
            from: oneHourAgo,
            limit: 50
        );
        Console.WriteLine($"\nRecent successful UserService entries: {recentSuccessfulEntries.Count(re => re.IsSuccess)}");

        // 8. Clear all logs
        requestLogService.Clear();
        Console.WriteLine($"\nAfter clearing - Total entries: {requestLogService.GetSummary().TotalEntries}");
    }
}
```

### Example Usage

```csharp
using System;
using System.Threading.Tasks;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(IServiceProvider serviceProvider)
    {
        var requestLogService = serviceProvider.GetRequiredService<IRequestLogService>();

        // Simulate logging requests from a gateway middleware
        var gatewayMiddleware = new GatewayMiddleware(requestLogService);
        
        // Process a successful request
        await gatewayMiddleware.ProcessRequestAsync("UserService", "GetUserById", 23.5, true);
        
        // Process a failed request
        await gatewayMiddleware.ProcessRequestAsync("PaymentService", "ProcessPayment", 156.8, false);
        
        // Get performance metrics
        var summary = requestLogService.GetSummary();
        Console.WriteLine($"Gateway performance - Success rate: {summary.SuccessRatePct:F1}%");
        Console.WriteLine($"Average response time: {summary.AverageDurationMs:F1}ms");
    }
}

class GatewayMiddleware
{
    private readonly IRequestLogService _requestLogService;
    
    public GatewayMiddleware(IRequestLogService requestLogService)
    {
        _requestLogService = requestLogService;
    }
    
    public async Task ProcessRequestAsync(string serviceName, string methodName, double durationMs, bool isSuccess)
    {
        var entry = new RequestLogEntry
        {
            GrpcMethod = $"{serviceName}/{methodName}",
            StatusCode = isSuccess ? 0 : 3,
            DurationMs = durationMs,
            IsSuccess = isSuccess,
            Timestamp = DateTime.UtcNow,
            ClientIpAddress = "192.168.1.100"
        };
        
        _requestLogService.Append(entry);
        
        if (!isSuccess)
        {
            Console.WriteLine($"Request failed: {serviceName}/{methodName} in {durationMs}ms");
        }
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

## IRequestLogService

`IRequestLogService` stores and provides queryable access to recent gateway request/response log entries. It maintains a fixed-capacity ring buffer of request logs that can be queried by method, status code, or time range. The service provides aggregate statistics including success rate, response times, and entry timestamps, making it ideal for monitoring, debugging, and performance analysis of gRPC gateway traffic.

### Example Usage

```csharp
using System;
using System.Linq;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static void Main()
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddLogging(configure => configure.AddConsole());
        services.AddSingleton<IRequestLogService, RequestLogService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var requestLogService = serviceProvider.GetRequiredService<IRequestLogService>();

        // 1. Log a successful request
        var successfulEntry = new RequestLogEntry
        {
            GrpcMethod = "UserService/GetUserById",
            StatusCode = 0, // OK
            DurationMs = 23.5,
            IsSuccess = true,
            RequestSizeBytes = 1024,
            ResponseSizeBytes = 2048,
            Timestamp = DateTime.UtcNow,
            ClientIpAddress = "192.168.1.100"
        };
        requestLogService.Append(successfulEntry);

        // 2. Log a failed request
        var failedEntry = new RequestLogEntry
        {
            GrpcMethod = "PaymentService/ProcessPayment",
            StatusCode = 3, // INVALID_ARGUMENT
            DurationMs = 156.8,
            IsSuccess = false,
            RequestSizeBytes = 512,
            ResponseSizeBytes = 128,
            Timestamp = DateTime.UtcNow.AddSeconds(-10),
            ClientIpAddress = "192.168.1.101",
            ErrorMessage = "Insufficient funds"
        };
        requestLogService.Append(failedEntry);

        // 3. Get recent log entries
        var recentEntries = requestLogService.GetRecent(limit: 10);
        Console.WriteLine($"Recent entries: {recentEntries.Count}");
        foreach (var entry in recentEntries.Take(3))
        {
            Console.WriteLine($" - {entry.Timestamp:HH:mm:ss} | {entry.GrpcMethod} | {entry.DurationMs}ms | {(entry.IsSuccess ? "SUCCESS" : "FAILED")}");
        }

        // 4. Search logs by method
        var userServiceEntries = requestLogService.Search(methodFilter: "UserService");
        Console.WriteLine($"\nUserService entries: {userServiceEntries.Count}");

        // 5. Get aggregate statistics
        var summary = requestLogService.GetSummary();
        Console.WriteLine($"\nStatistics:");
        Console.WriteLine($" - Total entries: {summary.TotalEntries}");
        Console.WriteLine($" - Success count: {summary.SuccessCount}");
        Console.WriteLine($" - Error count: {summary.ErrorCount}");
        Console.WriteLine($" - Success rate: {summary.SuccessRatePct:F1}%");
        Console.WriteLine($" - Average duration: {summary.AverageDurationMs:F1}ms");
        Console.WriteLine($" - Min duration: {summary.MinDurationMs}ms");
        Console.WriteLine($" - Max duration: {summary.MaxDurationMs}ms");
        Console.WriteLine($" - Oldest entry: {summary.OldestEntry?.ToString("yyyy-MM-dd HH:mm:ss")}");
        Console.WriteLine($" - Newest entry: {summary.NewestEntry?.ToString("yyyy-MM-dd HH:mm:ss")}");

        // 6. Search logs by status code
        var errorEntries = requestLogService.Search(statusCode: 3);
        Console.WriteLine($"\nError entries (status code 3): {errorEntries.Count}");

        // 7. Search logs by time range
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var recentSuccessfulEntries = requestLogService.Search(
            methodFilter: "UserService",
            from: oneHourAgo,
            limit: 50
        );
        Console.WriteLine($"\nRecent successful UserService entries: {recentSuccessfulEntries.Count(re => re.IsSuccess)}");

        // 8. Clear all logs
        requestLogService.Clear();
        Console.WriteLine($"\nAfter clearing - Total entries: {requestLogService.GetSummary().TotalEntries}");
    }
}
```

### Example Usage

```csharp
using System;
using System.Threading.Tasks;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(IServiceProvider serviceProvider)
    {
        var requestLogService = serviceProvider.GetRequiredService<IRequestLogService>();

        // Simulate logging requests from a gateway middleware
        var gatewayMiddleware = new GatewayMiddleware(requestLogService);
        
        // Process a successful request
        await gatewayMiddleware.ProcessRequestAsync("UserService", "GetUserById", 23.5, true);
        
        // Process a failed request
        await gatewayMiddleware.ProcessRequestAsync("PaymentService", "ProcessPayment", 156.8, false);
        
        // Get performance metrics
        var summary = requestLogService.GetSummary();
        Console.WriteLine($"Gateway performance - Success rate: {summary.SuccessRatePct:F1}%");
        Console.WriteLine($"Average response time: {summary.AverageDurationMs:F1}ms");
    }
}

class GatewayMiddleware
{
    private readonly IRequestLogService _requestLogService;
    
    public GatewayMiddleware(IRequestLogService requestLogService)
    {
        _requestLogService = requestLogService;
    }
    
    public async Task ProcessRequestAsync(string serviceName, string methodName, double durationMs, bool isSuccess)
    {
        var entry = new RequestLogEntry
        {
            GrpcMethod = $"{serviceName}/{methodName}",
            StatusCode = isSuccess ? 0 : 3,
            DurationMs = durationMs,
            IsSuccess = isSuccess,
            Timestamp = DateTime.UtcNow,
            ClientIpAddress = "192.168.1.100"
        };
        
        _requestLogService.Append(entry);
        
        if (!isSuccess)
        {
            Console.WriteLine($"Request failed: {serviceName}/{methodName} in {durationMs}ms");
        }
    }
}
```

## MemoryCacheService

`MemoryCacheService` provides an in-memory caching implementation using `IMemoryCache`. It offers thread-safe caching operations with support for expiration, statistics tracking, and cache management. The service tracks cache hits/misses, maintains metadata about cached entries, and provides methods for checking cache existence, clearing the cache, and retrieving cache statistics.

### Example Usage

```csharp
using System;
using System.Threading.Tasks;
using DotNetGrpcGateway.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main()
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddLogging(configure => configure.AddConsole());
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();

        var serviceProvider = services.BuildServiceProvider();
        var cacheService = serviceProvider.GetRequiredService<ICacheService>();

        // 1. Set a value in cache with default 5-minute expiration
        await cacheService.SetAsync("user:123", new { Id = 123, Name = "John Doe", Email = "john@example.com" });
        Console.WriteLine("Value cached successfully");

        // 2. Get a value from cache
        var cachedUser = await cacheService.GetAsync<object>("user:123");
        if (cachedUser != null)
        {
            Console.WriteLine($"Cache hit! User: {cachedUser}");
        }
        else
        {
            Console.WriteLine("Cache miss - value not found");
        }

        // 3. Check if a key exists in cache
        var exists = await cacheService.ExistsAsync("user:123");
        Console.WriteLine($"Key 'user:123' exists: {exists}");

        // 4. Set a value with custom expiration (30 seconds)
        await cacheService.SetAsync("temp:data", "Temporary data", TimeSpan.FromSeconds(30));
        Console.WriteLine("Temporary value cached for 30 seconds");

        // 5. Remove a value from cache
        await cacheService.RemoveAsync("temp:data");
        Console.WriteLine("Temporary value removed from cache");

        // 6. Get cache statistics
        var stats = await cacheService.GetStatisticsAsync();
        Console.WriteLine($"\nCache Statistics:");
        Console.WriteLine($" - Entries: {stats.EntryCount}");
        Console.WriteLine($" - Hits: {stats.HitCount}");
        Console.WriteLine($" - Misses: {stats.MissCount}");
        Console.WriteLine($" - Approximate size: {stats.ApproximateSizeBytes} bytes");

        // 7. Clear the entire cache
        await cacheService.ClearAsync();
        Console.WriteLine("Cache cleared");
        
        var emptyStats = await cacheService.GetStatisticsAsync();
        Console.WriteLine($"After clear - Entries: {emptyStats.EntryCount}");
    }
}
```

## ICacheService

`ICacheService` is a cache abstraction interface that defines the contract for caching implementations. It provides a unified API for storing, retrieving, and managing cached data with support for expiration policies, statistics tracking, and cache invalidation. This interface enables the application to swap between different caching implementations (in-memory, distributed, etc.) without changing the business logic.

### Example Usage

```csharp
using System;
using System.Threading.Tasks;
using DotNetGrpcGateway.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main()
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddLogging(configure => configure.AddConsole());
        services.AddSingleton<ICacheService, MemoryCacheService>();

        var serviceProvider = services.BuildServiceProvider();
        var cacheService = serviceProvider.GetRequiredService<ICacheService>();

        // 1. Set a value in cache with default 5-minute expiration
        await cacheService.SetAsync("user:123", new { Id = 123, Name = "John Doe", Email = "john@example.com" });
        Console.WriteLine("Value cached successfully");

        // 2. Get a value from cache
        var cachedUser = await cacheService.GetAsync<object>("user:123");
        if (cachedUser != null)
        {
            Console.WriteLine($"Cache hit! User: {cachedUser}");
        }
        else
        {
            Console.WriteLine("Cache miss - value not found");
        }

        // 3. Check if a key exists in cache
        var exists = await cacheService.ExistsAsync("user:123");
        Console.WriteLine($"Key 'user:123' exists: {exists}");

        // 4. Set a value with custom expiration (30 seconds)
        await cacheService.SetAsync("temp:data", "Temporary data", TimeSpan.FromSeconds(30));
        Console.WriteLine("Temporary value cached for 30 seconds");

        // 5. Remove a value from cache
        await cacheService.RemoveAsync("temp:data");
        Console.WriteLine("Temporary value removed from cache");

        // 6. Get cache statistics
        var stats = await cacheService.GetStatisticsAsync();
        Console.WriteLine($"\nCache Statistics:");
        Console.WriteLine($" - Name: {cacheService.Name}");
        Console.WriteLine($" - Entry count: {stats.EntryCount}");
        Console.WriteLine($" - Hit count: {stats.HitCount}");
        Console.WriteLine($" - Miss count: {stats.MissCount}");
        Console.WriteLine($" - Hit rate: {stats.HitRate:P2}");
        Console.WriteLine($" - Approximate size: {stats.ApproximateSizeBytes} bytes");
        Console.WriteLine($" - Max size: {cacheService.MaxSize}");
        Console.WriteLine($" - Duration: {cacheService.Duration}");
        Console.WriteLine($" - Absolute expiration: {cacheService.AbsoluteExpiration}");
        Console.WriteLine($" - Sliding expiration: {cacheService.SlidingExpiration}");

        // 7. Clear the entire cache
        await cacheService.ClearAsync();
        Console.WriteLine("Cache cleared");
    }
}
```

## IReflectionService

`IReflectionService` provides gRPC Server Reflection support — discovers and caches the method descriptors of registered back-end services so callers can inspect the API surface at runtime without direct access to .proto source files. It enables runtime discovery of gRPC service methods, their request/response types, and streaming capabilities through HTTP-based Server Reflection endpoints.

### Example Usage

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(IServiceProvider serviceProvider)
    {
        var reflectionService = serviceProvider.GetRequiredService<IReflectionService>();

        // 1. Check if reflection is available
        Console.WriteLine($"Reflection available: {reflectionService.IsReflectionAvailable}");

        // 2. Get reflection info for a specific service
        var serviceReflection = await reflectionService.GetServiceReflectionAsync(10);
        if (serviceReflection != null)
        {
            Console.WriteLine($"Service: {serviceReflection.ServiceName}");
            Console.WriteLine($"Available: {serviceReflection.IsAvailable}");
            Console.WriteLine($"Reflected at: {serviceReflection.ReflectedAt}");
            Console.WriteLine($"Methods: {serviceReflection.MethodCount}");
            
            foreach (var method in serviceReflection.Methods)
            {
                Console.WriteLine($" - {method.Name}: {method.RequestType} -> {method.ResponseType}");
            }
        }

        // 3. Get reflection info for all services
        var allReflections = await reflectionService.GetAllReflectionInfoAsync();
        Console.WriteLine($"\nTotal services with reflection: {allReflections.Count(r => r.IsAvailable)}");
        
        // 4. Refresh reflection for a specific service
        var refreshed = await reflectionService.RefreshServiceReflectionAsync(10);
        Console.WriteLine($"\nRefreshed service reflection: {refreshed.ServiceName}");
        
        // 5. Refresh reflection for all services
        await reflectionService.RefreshAllReflectionsAsync();
        Console.WriteLine("All service reflections refreshed");
    }
}
```

## IRequestLogService

`IRequestLogService` stores and provides queryable access to recent gateway request/response log entries. It maintains a fixed-capacity ring buffer of request logs that can be queried by method, status code, or time range. The service provides aggregate statistics including success rate, response times, and entry timestamps, making it ideal for monitoring, debugging, and performance analysis of gRPC gateway traffic.

### Example Usage

```csharp
using System;
using System.Linq;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static void Main()
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddLogging(configure => configure.AddConsole());
        services.AddSingleton<IRequestLogService, RequestLogService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var requestLogService = serviceProvider.GetRequiredService<IRequestLogService>();

        // 1. Log a successful request
        var successfulEntry = new RequestLogEntry
        {
            GrpcMethod = "UserService/GetUserById",
            StatusCode = 0, // OK
            DurationMs = 23.5,
            IsSuccess = true,
            RequestSizeBytes = 1024,
            ResponseSizeBytes = 2048,
            Timestamp = DateTime.UtcNow,
            ClientIpAddress = "192.168.1.100"
        };
        requestLogService.Append(successfulEntry);

        // 2. Log a failed request
        var failedEntry = new RequestLogEntry
        {
            GrpcMethod = "PaymentService/ProcessPayment",
            StatusCode = 3, // INVALID_ARGUMENT
            DurationMs = 156.8,
            IsSuccess = false,
            RequestSizeBytes = 512,
            ResponseSizeBytes = 128,
            Timestamp = DateTime.UtcNow.AddSeconds(-10),
            ClientIpAddress = "192.168.1.101",
            ErrorMessage = "Insufficient funds"
        };
        requestLogService.Append(failedEntry);

        // 3. Get recent log entries
        var recentEntries = requestLogService.GetRecent(limit: 10);
        Console.WriteLine($"Recent entries: {recentEntries.Count}");
        foreach (var entry in recentEntries.Take(3))
        {
            Console.WriteLine($" - {entry.Timestamp:HH:mm:ss} | {entry.GrpcMethod} | {entry.DurationMs}ms | {(entry.IsSuccess ? "SUCCESS" : "FAILED")}");
        }

        // 4. Search logs by method
        var userServiceEntries = requestLogService.Search(methodFilter: "UserService");
        Console.WriteLine($"\nUserService entries: {userServiceEntries.Count}");

        // 5. Get aggregate statistics
        var summary = requestLogService.GetSummary();
        Console.WriteLine($"\nStatistics:");
        Console.WriteLine($" - Total entries: {summary.TotalEntries}");
        Console.WriteLine($" - Success count: {summary.SuccessCount}");
        Console.WriteLine($" - Error count: {summary.ErrorCount}");
        Console.WriteLine($" - Success rate: {summary.SuccessRatePct:F1}%");
        Console.WriteLine($" - Average duration: {summary.AverageDurationMs:F1}ms");
        Console.WriteLine($" - Min duration: {summary.MinDurationMs}ms");
        Console.WriteLine($" - Max duration: {summary.MaxDurationMs}ms");
        Console.WriteLine($" - Oldest entry: {summary.OldestEntry?.ToString("yyyy-MM-dd HH:mm:ss")}");
        Console.WriteLine($" - Newest entry: {summary.NewestEntry?.ToString("yyyy-MM-dd HH:mm:ss")}");

        // 6. Search logs by status code
        var errorEntries = requestLogService.Search(statusCode: 3);
        Console.WriteLine($"\nError entries (status code 3): {errorEntries.Count}");

        // 7. Search logs by time range
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var recentSuccessfulEntries = requestLogService.Search(
            methodFilter: "UserService",
            from: oneHourAgo,
            limit: 50
        );
        Console.WriteLine($"\nRecent successful UserService entries: {recentSuccessfulEntries.Count(re => re.IsSuccess)}");

        // 8. Clear all logs
        requestLogService.Clear();
        Console.WriteLine($"\nAfter clearing - Total entries: {requestLogService.GetSummary().TotalEntries}");
    }
}
```

### Example Usage

```csharp
using System;
using System.Threading.Tasks;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(IServiceProvider serviceProvider)
    {
        var requestLogService = serviceProvider.GetRequiredService<IRequestLogService>();

        // Simulate logging requests from a gateway middleware
        var gatewayMiddleware = new GatewayMiddleware(requestLogService);
        
        // Process a successful request
        await gatewayMiddleware.ProcessRequestAsync("UserService", "GetUserById", 23.5, true);
        
        // Process a failed request
        await gatewayMiddleware.ProcessRequestAsync("PaymentService", "ProcessPayment", 156.8, false);
        
        // Get performance metrics
        var summary = requestLogService.GetSummary();
        Console.WriteLine($"Gateway performance - Success rate: {summary.SuccessRatePct:F1}%");
        Console.WriteLine($"Average response time: {summary.AverageDurationMs:F1}ms");
    }
}

class GatewayMiddleware
{
    private readonly IRequestLogService _requestLogService;
    
    public GatewayMiddleware(IRequestLogService requestLogService)
    {
        _requestLogService = requestLogService;
    }
    
    public async Task ProcessRequestAsync(string serviceName, string methodName, double durationMs, bool isSuccess)
    {
        var entry = new RequestLogEntry
        {
            GrpcMethod = $"{serviceName}/{methodName}",
            StatusCode = isSuccess ? 0 : 3,
            DurationMs = durationMs,
            IsSuccess = isSuccess,
            Timestamp = DateTime.UtcNow,
            ClientIpAddress = "192.168.1.100"
        };
        
        _requestLogService.Append(entry);
        
        if (!isSuccess)
        {
            Console.WriteLine($"Request failed: {serviceName}/{methodName} in {durationMs}ms");
        }
    }
}
```

## ServiceCollectionExtensions

`ServiceCollectionExtensions` provides extension methods for configuring gateway services and their dependencies in the .NET dependency injection container. It includes methods to register core gateway services, configure gateway options from configuration files, and set up health checks for monitoring gateway components.

### Example Usage

```csharp
using System;
using DotNetGrpcGateway.Configuration;
using Microsoft.Extensions.DependencyInjection;

class Program
{
    static void Main()
    {
        // 1. Setup dependency injection with gateway services
        var services = new ServiceCollection();
        
        // Add all gateway services
        services.AddGatewayServices();
        
        // Configure gateway options from appsettings.json
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        services.AddGatewayConfiguration(configuration);
        
        // Add gateway health checks
        services.AddGatewayHealthChecks();
        
        // Add gRPC Server Reflection support
        services.AddGatewayReflection();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Resolve gateway services
        var gatewayService = serviceProvider.GetRequiredService<IGatewayService>();
        var serviceDiscovery = serviceProvider.GetRequiredService<IServiceDiscoveryService>();
        
        Console.WriteLine("Gateway services configured successfully!");
    }
}
```

## ILoadBalancerService

`ILoadBalancerService` manages endpoint pools for gRPC services and selects the next endpoint to use based on a configurable load balancing strategy. It provides methods for registering/deregistering endpoints, retrieving endpoint lists, updating health status, and recording request metrics. The service supports RoundRobin, Random, and LeastConnections strategies to distribute traffic across healthy endpoints.

### Example Usage

```csharp
using System;
using System.Linq;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static void Main()
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddLogging(configure => configure.AddConsole());
        services.AddSingleton<ILoadBalancerService, LoadBalancerService>();

        var serviceProvider = services.BuildServiceProvider();
        var loadBalancer = serviceProvider.GetRequiredService<ILoadBalancerService>();

        // 1. Register endpoints for a service
        var endpoint1 = new ServiceEndpoint {
            Id = 1,
            ServiceId = 10,
            Host = "backend1.example.com",
            Port = 5001,
            Weight = 2
        };

        var endpoint2 = new ServiceEndpoint {
            Id = 2,
            ServiceId = 10,
            Host = "backend2.example.com",
            Port = 5002,
            Weight = 1
        };

        loadBalancer.RegisterEndpoint(endpoint1);
        loadBalancer.RegisterEndpoint(endpoint2);

        // 2. Set load balancing strategy
        loadBalancer.Strategy = LoadBalancingStrategy.RoundRobin;
        Console.WriteLine($"Load balancing strategy: {loadBalancer.Strategy}");

        // 3. Get all registered endpoints
        var endpoints = loadBalancer.GetEndpoints(10);
        Console.WriteLine($"Registered endpoints: {endpoints.Count}");
        foreach (var ep in endpoints)
        {
            Console.WriteLine($" - Endpoint {ep.Id}: {ep.Host}:{ep.Port} (Healthy: {ep.IsHealthy})");
        }

        // 4. Get next endpoint using RoundRobin strategy
        var selectedEndpoint = loadBalancer.GetNextEndpoint(10);
        Console.WriteLine($"\nSelected endpoint: {selectedEndpoint?.Host}:{selectedEndpoint?.Port}");

        // 5. Update endpoint health status
        loadBalancer.UpdateEndpointHealth(10, 1, true);
        loadBalancer.UpdateEndpointHealth(10, 2, false);
        Console.WriteLine("Updated endpoint health status");

        // 6. Record request completion
        loadBalancer.RecordRequestCompleted(10, 1, 45.2, true);
        loadBalancer.RecordRequestCompleted(10, 2, 38.7, false);

        // 7. Switch to Random strategy
        loadBalancer.Strategy = LoadBalancingStrategy.Random;
        Console.WriteLine($"\nSwitched to strategy: {loadBalancer.Strategy}");

        // 8. Get next endpoint using Random strategy
        var randomEndpoint = loadBalancer.GetNextEndpoint(10);
        Console.WriteLine($"Randomly selected endpoint: {randomEndpoint?.Host}:{randomEndpoint?.Port}");

        // 9. Deregister an endpoint
        loadBalancer.DeregisterEndpoint(10, 2);
        Console.WriteLine("Deregistered endpoint 2");

        // 10. Verify remaining endpoints
        var remainingEndpoints = loadBalancer.GetEndpoints(10);
        Console.WriteLine($"Remaining endpoints: {remainingEndpoints.Count}");
    }
}
```

## IRequestLogService

`IRequestLogService` stores and provides queryable access to recent gateway request/response log entries. It maintains a fixed-capacity ring buffer of request logs that can be queried by method, status code, or time range. The service provides aggregate statistics including success rate, response times, and entry timestamps, making it ideal for monitoring, debugging, and performance analysis of gRPC gateway traffic.

### Example Usage

```csharp
using System;
using System.Linq;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static void Main()
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddLogging(configure => configure.AddConsole());
        services.AddSingleton<IRequestLogService, RequestLogService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var requestLogService = serviceProvider.GetRequiredService<IRequestLogService>();

        // 1. Log a successful request
        var successfulEntry = new RequestLogEntry
        {
            GrpcMethod = "UserService/GetUserById",
            StatusCode = 0, // OK
            DurationMs = 23.5,
            IsSuccess = true,
            RequestSizeBytes = 1024,
            ResponseSizeBytes = 2048,
            Timestamp = DateTime.UtcNow,
            ClientIpAddress = "192.168.1.100"
        };
        requestLogService.Append(successfulEntry);

        // 2. Log a failed request
        var failedEntry = new RequestLogEntry
        {
            GrpcMethod = "PaymentService/ProcessPayment",
            StatusCode = 3, // INVALID_ARGUMENT
            DurationMs = 156.8,
            IsSuccess = false,
            RequestSizeBytes = 512,
            ResponseSizeBytes = 128,
            Timestamp = DateTime.UtcNow.AddSeconds(-10),
            ClientIpAddress = "192.168.1.101",
            ErrorMessage = "Insufficient funds"
        };
        requestLogService.Append(failedEntry);

        // 3. Get recent log entries
        var recentEntries = requestLogService.GetRecent(limit: 10);
        Console.WriteLine($"Recent entries: {recentEntries.Count}");
        foreach (var entry in recentEntries.Take(3))
        {
            Console.WriteLine($" - {entry.Timestamp:HH:mm:ss} | {entry.GrpcMethod} | {entry.DurationMs}ms | {(entry.IsSuccess ? "SUCCESS" : "FAILED")}");
        }

        // 4. Search logs by method
        var userServiceEntries = requestLogService.Search(methodFilter: "UserService");
        Console.WriteLine($"\nUserService entries: {userServiceEntries.Count}");

        // 5. Get aggregate statistics
        var summary = requestLogService.GetSummary();
        Console.WriteLine($"\nStatistics:");
        Console.WriteLine($" - Total entries: {summary.TotalEntries}");
        Console.WriteLine($" - Success count: {summary.SuccessCount}");
        Console.WriteLine($" - Error count: {summary.ErrorCount}");
        Console.WriteLine($" - Success rate: {summary.SuccessRatePct:F1}%");
        Console.WriteLine($" - Average duration: {summary.AverageDurationMs:F1}ms");
        Console.WriteLine($" - Min duration: {summary.MinDurationMs}ms");
        Console.WriteLine($" - Max duration: {summary.MaxDurationMs}ms");
        Console.WriteLine($" - Oldest entry: {summary.OldestEntry?.ToString("yyyy-MM-dd HH:mm:ss")}");
        Console.WriteLine($" - Newest entry: {summary.NewestEntry?.ToString("yyyy-MM-dd HH:mm:ss")}");

        // 6. Search logs by status code
        var errorEntries = requestLogService.Search(statusCode: 3);
        Console.WriteLine($"\nError entries (status code 3): {errorEntries.Count}");

        // 7. Search logs by time range
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var recentSuccessfulEntries = requestLogService.Search(
            methodFilter: "UserService",
            from: oneHourAgo,
            limit: 50
        );
        Console.WriteLine($"\nRecent successful UserService entries: {recentSuccessfulEntries.Count(re => re.IsSuccess)}");

        // 8. Clear all logs
        requestLogService.Clear();
        Console.WriteLine($"\nAfter clearing - Total entries: {requestLogService.GetSummary().TotalEntries}");
    }
}
```

### Example Usage

```csharp
using System;
using System.Threading.Tasks;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(IServiceProvider serviceProvider)
    {
        var requestLogService = serviceProvider.GetRequiredService<IRequestLogService>();

        // Simulate logging requests from a gateway middleware
        var gatewayMiddleware = new GatewayMiddleware(requestLogService);
        
        // Process a successful request
        await gatewayMiddleware.ProcessRequestAsync("UserService", "GetUserById", 23.5, true);
        
        // Process a failed request
        await gatewayMiddleware.ProcessRequestAsync("PaymentService", "ProcessPayment", 156.8, false);
        
        // Get performance metrics
        var summary = requestLogService.GetSummary();
        Console.WriteLine($"Gateway performance - Success rate: {summary.SuccessRatePct:F1}%");
        Console.WriteLine($"Average response time: {summary.AverageDurationMs:F1}ms");
    }
}

class GatewayMiddleware
{
    private readonly IRequestLogService _requestLogService;
    
    public GatewayMiddleware(IRequestLogService requestLogService)
    {
        _requestLogService = requestLogService;
    }
    
    public async Task ProcessRequestAsync(string serviceName, string methodName, double durationMs, bool isSuccess)
    {
        var entry = new RequestLogEntry
        {
            GrpcMethod = $"{serviceName}/{methodName}",
            StatusCode = isSuccess ? 0 : 3,
            DurationMs = durationMs,
            IsSuccess = isSuccess,
            Timestamp = DateTime.UtcNow,
            ClientIpAddress = "192.168.1.100"
        };
        
        _requestLogService.Append(entry);
        
        if (!isSuccess)
        {
            Console.WriteLine($"Request failed: {serviceName}/{methodName} in {durationMs}ms");
        }
    }
}
```

## HttpUtility

`HttpUtility` provides HTTP-related utilities for request/response handling and header manipulation. It simplifies common HTTP operations like header parsing, content negotiation, and status code classification. The class includes methods for working with Accept headers, building authorization headers, extracting Bearer tokens, adding gRPC-Web specific headers, and determining content type categories.

### Example Usage

```csharp
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using DotNetGrpcGateway.Utilities;

class Program
{
    static void Main()
    {
        // 1. Get accepted content type from Accept header
        var acceptHeader = "application/json, text/xml";
        var contentType = HttpUtility.GetAcceptedContentType(acceptHeader);
        Console.WriteLine($"Accepted content type: {contentType}");
        
        // 2. Build authorization header
        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
        var authHeader = HttpUtility.BuildAuthorizationHeader(token);
        Console.WriteLine($"Authorization header: {authHeader}");
        
        // 3. Extract Bearer token
        var extractedToken = HttpUtility.ExtractBearerToken(authHeader);
        Console.WriteLine($"Extracted token: {extractedToken}");
        
        // 4. Add gRPC-Web headers to HttpRequestHeaders
        var httpClient = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/grpc");
        request.Headers.AddGrpcWebHeaders();
        Console.WriteLine("Added gRPC-Web headers");
        
        // 5. Check status code categories
        Console.WriteLine($"Status 200 is success: {HttpUtility.IsSuccessStatusCode(200)}");
        Console.WriteLine($"Status 404 is client error: {HttpUtility.IsClientError(404)}");
        Console.WriteLine($"Status 500 is server error: {HttpUtility.IsServerError(500)}");
        Console.WriteLine($"Status 200 category: {HttpUtility.GetStatusCodeCategory(200)}");
        
        // 6. Check content types
        Console.WriteLine($"application/json is JSON: {HttpUtility.IsJsonContentType("application/json")}");
        Console.WriteLine($"application/xml is XML: {HttpUtility.IsXmlContentType("application/xml")}");
        Console.WriteLine($"application/x-www-form-urlencoded is form: {HttpUtility.IsFormContentType("application/x-www-form-urlencoded")}");
    }
}
```

## IRouteManagementService

`IRouteManagementService` is responsible for advanced route management operations in the gRPC gateway. It handles retrieving routes by service, finding matching routes for incoming requests, identifying conflicting route patterns, and validating route configurations to ensure proper routing and prevent conflicts.

### Example Usage

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main()
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddLogging(configure => configure.AddConsole());
        services.AddSingleton<IRouteManagementService, RouteManagementService>();
        services.AddSingleton<IRouteRepository, RouteRepository>();
        services.AddSingleton<IEventPublisher, EventPublisher>();

        var serviceProvider = services.BuildServiceProvider();
        var routeManagementService = serviceProvider.GetRequiredService<IRouteManagementService>();

        // 1. Get all routes for a specific service
        var serviceRoutes = await routeManagementService.GetRoutesByServiceAsync(10);
        Console.WriteLine($"Found {serviceRoutes.Count} routes for service ID 10");
        foreach (var route in serviceRoutes)
        {
            Console.WriteLine($" - Route {route.Id}: {route.Pattern} (Priority: {route.Priority})");
        }

        // 2. Find a matching route for a specific path
        var matchingRoute = await routeManagementService.FindMatchingRouteAsync("/api/users/get");
        if (matchingRoute != null)
        {
            Console.WriteLine($"\nFound matching route for path '/api/users/get': {matchingRoute.Pattern}");
            Console.WriteLine($" - Target Service ID: {matchingRoute.TargetServiceId}");
            Console.WriteLine($" - Priority: {matchingRoute.Priority}");
        }
        else
        {
            Console.WriteLine("\nNo matching route found for path '/api/users/get'");
        }

        // 3. Find conflicting routes for a pattern
        var conflictingRoutes = await routeManagementService.GetConflictingRoutesAsync("/api/users/*");
        Console.WriteLine($"\nFound {conflictingRoutes.Count} conflicting routes for pattern '/api/users/*'");
        foreach (var route in conflictingRoutes)
        {
            Console.WriteLine($" - Route {route.Id}: {route.Pattern}");
        }

        // 4. Validate a route configuration
        var newRoute = new GatewayRoute
        {
            Id = 1,
            Name = "UserServiceRoute",
            Pattern = "/api/users/*",
            TargetServiceId = 10,
            Priority = 100,
            RateLimitPerMinute = 1000,
            EnableCaching = true,
            CacheDurationSeconds = 300
        };

        bool isValid = await routeManagementService.ValidateRouteAsync(newRoute);
        Console.WriteLine($"\nRoute validation result: {(isValid ? "Valid" : "Invalid")}");

        // 5. Validate a route with invalid configuration
        var invalidRoute = new GatewayRoute
        {
            Id = 2,
            Name = "InvalidRoute",
            Pattern = "", // Empty pattern - invalid
            TargetServiceId = 10,
            Priority = 10000 // Priority out of range - invalid
        };

        bool isInvalid = await routeManagementService.ValidateRouteAsync(invalidRoute);
        Console.WriteLine($"Invalid route validation result: {(isInvalid ? "Valid" : "Invalid")}");
    }
}
```

## IRequestLogService

`IRequestLogService` stores and provides queryable access to recent gateway request/response log entries. It maintains a fixed-capacity ring buffer of request logs that can be queried by method, status code, or time range. The service provides aggregate statistics including success rate, response times, and entry timestamps, making it ideal for monitoring, debugging, and performance analysis of gRPC gateway traffic.

### Example Usage

```csharp
using System;
using System.Linq;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static void Main()
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddLogging(configure => configure.AddConsole());
        services.AddSingleton<IRequestLogService, RequestLogService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var requestLogService = serviceProvider.GetRequiredService<IRequestLogService>();

        // 1. Log a successful request
        var successfulEntry = new RequestLogEntry
        {
            GrpcMethod = "UserService/GetUserById",
            StatusCode = 0, // OK
            DurationMs = 23.5,
            IsSuccess = true,
            RequestSizeBytes = 1024,
            ResponseSizeBytes = 2048,
            Timestamp = DateTime.UtcNow,
            ClientIpAddress = "192.168.1.100"
        };
        requestLogService.Append(successfulEntry);

        // 2. Log a failed request
        var failedEntry = new RequestLogEntry
        {
            GrpcMethod = "PaymentService/ProcessPayment",
            StatusCode = 3, // INVALID_ARGUMENT
            DurationMs = 156.8,
            IsSuccess = false,
            RequestSizeBytes = 512,
            ResponseSizeBytes = 128,
            Timestamp = DateTime.UtcNow.AddSeconds(-10),
            ClientIpAddress = "192.168.1.101",
            ErrorMessage = "Insufficient funds"
        };
        requestLogService.Append(failedEntry);

        // 3. Get recent log entries
        var recentEntries = requestLogService.GetRecent(limit: 10);
        Console.WriteLine($"Recent entries: {recentEntries.Count}");
        foreach (var entry in recentEntries.Take(3))
        {
            Console.WriteLine($" - {entry.Timestamp:HH:mm:ss} | {entry.GrpcMethod} | {entry.DurationMs}ms | {(entry.IsSuccess ? "SUCCESS" : "FAILED")}");
        }

        // 4. Search logs by method
        var userServiceEntries = requestLogService.Search(methodFilter: "UserService");
        Console.WriteLine($"\nUserService entries: {userServiceEntries.Count}");

        // 5. Get aggregate statistics
        var summary = requestLogService.GetSummary();
        Console.WriteLine($"\nStatistics:");
        Console.WriteLine($" - Total entries: {summary.TotalEntries}");
        Console.WriteLine($" - Success count: {summary.SuccessCount}");
        Console.WriteLine($" - Error count: {summary.ErrorCount}");
        Console.WriteLine($" - Success rate: {summary.SuccessRatePct:F1}%");
        Console.WriteLine($" - Average duration: {summary.AverageDurationMs:F1}ms");
        Console.WriteLine($" - Min duration: {summary.MinDurationMs}ms");
        Console.WriteLine($" - Max duration: {summary.MaxDurationMs}ms");
        Console.WriteLine($" - Oldest entry: {summary.OldestEntry?.ToString("yyyy-MM-dd HH:mm:ss")}");
        Console.WriteLine($" - Newest entry: {summary.NewestEntry?.ToString("yyyy-MM-dd HH:mm:ss")}");

        // 6. Search logs by status code
        var errorEntries = requestLogService.Search(statusCode: 3);
        Console.WriteLine($"\nError entries (status code 3): {errorEntries.Count}");

        // 7. Search logs by time range
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var recentSuccessfulEntries = requestLogService.Search(
            methodFilter: "UserService",
            from: oneHourAgo,
            limit: 50
        );
        Console.WriteLine($"\nRecent successful UserService entries: {recentSuccessfulEntries.Count(re => re.IsSuccess)}");

        // 8. Clear all logs
        requestLogService.Clear();
        Console.WriteLine($"\nAfter clearing - Total entries: {requestLogService.GetSummary().TotalEntries}");
    }
}
```

### Example Usage

```csharp
using System;
using System.Threading.Tasks;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(IServiceProvider serviceProvider)
    {
        var requestLogService = serviceProvider.GetRequiredService<IRequestLogService>();

        // Simulate logging requests from a gateway middleware
        var gatewayMiddleware = new GatewayMiddleware(requestLogService);
        
        // Process a successful request
        await gatewayMiddleware.ProcessRequestAsync("UserService", "GetUserById", 23.5, true);
        
        // Process a failed request
        await gatewayMiddleware.ProcessRequestAsync("PaymentService", "ProcessPayment", 156.8, false);
        
        // Get performance metrics
        var summary = requestLogService.GetSummary();
        Console.WriteLine($"Gateway performance - Success rate: {summary.SuccessRatePct:F1}%");
        Console.WriteLine($"Average response time: {summary.AverageDurationMs:F1}ms");
    }
}

class GatewayMiddleware
{
    private readonly IRequestLogService _requestLogService;
    
    public GatewayMiddleware(IRequestLogService requestLogService)
    {
        _requestLogService = requestLogService;
    }
    
    public async Task ProcessRequestAsync(string serviceName, string methodName, double durationMs, bool isSuccess)
    {
        var entry = new RequestLogEntry
        {
            GrpcMethod = $"{serviceName}/{methodName}",
            StatusCode = isSuccess ? 0 : 3,
            DurationMs = durationMs,
            IsSuccess = isSuccess,
            Timestamp = DateTime.UtcNow,
            ClientIpAddress = "192.168.1.100"
        };
        
        _requestLogService.Append(entry);
        
        if (!isSuccess)
        {
            Console.WriteLine($"Request failed: {serviceName}/{methodName} in {durationMs}ms");
        }
    }
}
```

### Example Usage

```csharp
using System;
using System.Linq;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static void Main()
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddLogging(configure => configure.AddConsole());
        services.AddSingleton<ILoadBalancerService, LoadBalancerService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var loadBalancer = serviceProvider.GetRequiredService<ILoadBalancerService>();

        // 1. Register endpoints for a service
        var endpoint1 = new ServiceEndpoint { 
            Id = 1, 
            ServiceId = 10, 
            Host = "backend1.example.com", 
            Port = 5001,
            Weight = 2
        };
        
        var endpoint2 = new ServiceEndpoint { 
            Id = 2, 
            ServiceId = 10, 
            Host = "backend2.example.com", 
            Port = 5002,
            Weight = 1
        };
        
        loadBalancer.RegisterEndpoint(endpoint1);
        loadBalancer.RegisterEndpoint(endpoint2);

        // 2. Set load balancing strategy
        loadBalancer.Strategy = LoadBalancingStrategy.RoundRobin;
        Console.WriteLine($"Load balancing strategy: {loadBalancer.Strategy}");

        // 3. Get all registered endpoints
        var endpoints = loadBalancer.GetEndpoints(10);
        Console.WriteLine($"Registered endpoints: {endpoints.Count}");
        foreach (var ep in endpoints)
        {
            Console.WriteLine($" - Endpoint {ep.Id}: {ep.Host}:{ep.Port} (Healthy: {ep.IsHealthy})");
        }

        // 4. Get next endpoint using RoundRobin strategy
        var selectedEndpoint = loadBalancer.GetNextEndpoint(10);
        Console.WriteLine($"\nSelected endpoint: {selectedEndpoint?.Host}:{selectedEndpoint?.Port}");

        // 5. Update endpoint health status
        loadBalancer.UpdateEndpointHealth(10, 1, true);
        loadBalancer.UpdateEndpointHealth(10, 2, false);
        Console.WriteLine("Updated endpoint health status");

        // 6. Record request completion
        loadBalancer.RecordRequestCompleted(10, 1, 45.2, true);
        loadBalancer.RecordRequestCompleted(10, 2, 38.7, false);

        // 7. Switch to Random strategy
        loadBalancer.Strategy = LoadBalancingStrategy.Random;
        Console.WriteLine($"\nSwitched to strategy: {loadBalancer.Strategy}");

        // 8. Get next endpoint using Random strategy
        var randomEndpoint = loadBalancer.GetNextEndpoint(10);
        Console.WriteLine($"Randomly selected endpoint: {randomEndpoint?.Host}:{randomEndpoint?.Port}");

        // 9. Deregister an endpoint
        loadBalancer.DeregisterEndpoint(10, 2);
        Console.WriteLine("Deregistered endpoint 2");

        // 10. Verify remaining endpoints
        var remainingEndpoints = loadBalancer.GetEndpoints(10);
        Console.WriteLine($"Remaining endpoints: {remainingEndpoints.Count}");
    }
}
```

## IRequestLogService

`IRequestLogService` stores and provides queryable access to recent gateway request/response log entries. It maintains a fixed-capacity ring buffer of request logs that can be queried by method, status code, or time range. The service provides aggregate statistics including success rate, response times, and entry timestamps, making it ideal for monitoring, debugging, and performance analysis of gRPC gateway traffic.

### Example Usage

```csharp
using System;
using System.Linq;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static void Main()
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddLogging(configure => configure.AddConsole());
        services.AddSingleton<IRequestLogService, RequestLogService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var requestLogService = serviceProvider.GetRequiredService<IRequestLogService>();

        // 1. Log a successful request
        var successfulEntry = new RequestLogEntry
        {
            GrpcMethod = "UserService/GetUserById",
            StatusCode = 0, // OK
            DurationMs = 23.5,
            IsSuccess = true,
            RequestSizeBytes = 1024,
            ResponseSizeBytes = 2048,
            Timestamp = DateTime.UtcNow,
            ClientIpAddress = "192.168.1.100"
        };
        requestLogService.Append(successfulEntry);

        // 2. Log a failed request
        var failedEntry = new RequestLogEntry
        {
            GrpcMethod = "PaymentService/ProcessPayment",
            StatusCode = 3, // INVALID_ARGUMENT
            DurationMs = 156.8,
            IsSuccess = false,
            RequestSizeBytes = 512,
            ResponseSizeBytes = 128,
            Timestamp = DateTime.UtcNow.AddSeconds(-10),
            ClientIpAddress = "192.168.1.101",
            ErrorMessage = "Insufficient funds"
        };
        requestLogService.Append(failedEntry);

        // 3. Get recent log entries
        var recentEntries = requestLogService.GetRecent(limit: 10);
        Console.WriteLine($"Recent entries: {recentEntries.Count}");
        foreach (var entry in recentEntries.Take(3))
        {
            Console.WriteLine($" - {entry.Timestamp:HH:mm:ss} | {entry.GrpcMethod} | {entry.DurationMs}ms | {(entry.IsSuccess ? "SUCCESS" : "FAILED")}");
        }

        // 4. Search logs by method
        var userServiceEntries = requestLogService.Search(methodFilter: "UserService");
        Console.WriteLine($"\nUserService entries: {userServiceEntries.Count}");

        // 5. Get aggregate statistics
        var summary = requestLogService.GetSummary();
        Console.WriteLine($"\nStatistics:");
        Console.WriteLine($" - Total entries: {summary.TotalEntries}");
        Console.WriteLine($" - Success count: {summary.SuccessCount}");
        Console.WriteLine($" - Error count: {summary.ErrorCount}");
        Console.WriteLine($" - Success rate: {summary.SuccessRatePct:F1}%");
        Console.WriteLine($" - Average duration: {summary.AverageDurationMs:F1}ms");
        Console.WriteLine($" - Min duration: {summary.MinDurationMs}ms");
        Console.WriteLine($" - Max duration: {summary.MaxDurationMs}ms");
        Console.WriteLine($" - Oldest entry: {summary.OldestEntry?.ToString("yyyy-MM-dd HH:mm:ss")}");
        Console.WriteLine($" - Newest entry: {summary.NewestEntry?.ToString("yyyy-MM-dd HH:mm:ss")}");

        // 6. Search logs by status code
        var errorEntries = requestLogService.Search(statusCode: 3);
        Console.WriteLine($"\nError entries (status code 3): {errorEntries.Count}");

        // 7. Search logs by time range
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var recentSuccessfulEntries = requestLogService.Search(
            methodFilter: "UserService",
            from: oneHourAgo,
            limit: 50
        );
        Console.WriteLine($"\nRecent successful UserService entries: {recentSuccessfulEntries.Count(re => re.IsSuccess)}");

        // 8. Clear all logs
        requestLogService.Clear();
        Console.WriteLine($"\nAfter clearing - Total entries: {requestLogService.GetSummary().TotalEntries}");
    }
}
```

### Example Usage

```csharp
using System;
using System.Threading.Tasks;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(IServiceProvider serviceProvider)
    {
        var requestLogService = serviceProvider.GetRequiredService<IRequestLogService>();

        // Simulate logging requests from a gateway middleware
        var gatewayMiddleware = new GatewayMiddleware(requestLogService);
        
        // Process a successful request
        await gatewayMiddleware.ProcessRequestAsync("UserService", "GetUserById", 23.5, true);
        
        // Process a failed request
        await gatewayMiddleware.ProcessRequestAsync("PaymentService", "ProcessPayment", 156.8, false);
        
        // Get performance metrics
        var summary = requestLogService.GetSummary();
        Console.WriteLine($"Gateway performance - Success rate: {summary.SuccessRatePct:F1}%");
        Console.WriteLine($"Average response time: {summary.AverageDurationMs:F1}ms");
    }
}

class GatewayMiddleware
{
    private readonly IRequestLogService _requestLogService;
    
    public GatewayMiddleware(IRequestLogService requestLogService)
    {
        _requestLogService = requestLogService;
    }
    
    public async Task ProcessRequestAsync(string serviceName, string methodName, double durationMs, bool isSuccess)
    {
        var entry = new RequestLogEntry
        {
            GrpcMethod = $"{serviceName}/{methodName}",
            StatusCode = isSuccess ? 0 : 3,
            DurationMs = durationMs,
            IsSuccess = isSuccess,
            Timestamp = DateTime.UtcNow,
            ClientIpAddress = "192.168.1.100"
        };
        
        _requestLogService.Append(entry);
        
        if (!isSuccess)
        {
            Console.WriteLine($"Request failed: {serviceName}/{methodName} in {durationMs}ms");
        }
    }
}
```

## ICircuitBreakerRegistry

`ICircuitBreakerRegistry` manages a registry of circuit breakers, one per registered service, providing lifecycle operations and aggregate status queries. It allows retrieving circuit breakers by service ID, creating them on-demand, resetting their state, and inspecting the state of all registered circuit breakers.

### Example Usage

```csharp
using System;
using System.Linq;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static void Main()
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddLogging(configure => configure.AddConsole());
        services.AddSingleton<ICircuitBreakerRegistry, CircuitBreakerRegistry>();

        var serviceProvider = services.BuildServiceProvider();
        var registry = serviceProvider.GetRequiredService<ICircuitBreakerRegistry>();

        // 1. Get or create a circuit breaker for a service
        var circuitBreaker = registry.GetOrCreate(10);
        Console.WriteLine($"Circuit breaker created for service ID: 10");

        // 2. Try to get an existing circuit breaker
        var existingBreaker = registry.TryGet(10);
        Console.WriteLine($"Existing breaker found: {existingBreaker != null}");

        // 3. Reset a circuit breaker to closed state
        registry.Reset(10);
        Console.WriteLine("Circuit breaker reset to closed state");

        // 4. Register endpoints and simulate failures to trip the breaker
        var endpoint1 = new ServiceEndpoint
        {
            Id = 1,
            ServiceId = 10,
            Host = "backend1.example.com",
            Port = 5001,
            Weight = 2
        };

        // Simulate failures to trip the circuit breaker
        for (int i = 0; i < 5; i++)
        {
            circuitBreaker.RecordFailure();
        }
        
        Console.WriteLine($"Circuit breaker state: {circuitBreaker.State.Status}");
        Console.WriteLine($"Failures in a row: {circuitBreaker.State.FailuresInRow}");

        // 5. Get all circuit breaker states
        var allStates = registry.GetAllStates();
        Console.WriteLine($"\nAll circuit breaker states ({allStates.Count}):");
        foreach (var kvp in allStates)
        {
            Console.WriteLine($" - Service {kvp.Key}: {kvp.Value.Status} ({kvp.Value.FailuresInRow} failures in a row)");
        }
    }
}
```

## IRequestLogService

`IRequestLogService` stores and provides queryable access to recent gateway request/response log entries. It maintains a fixed-capacity ring buffer of request logs that can be queried by method, status code, or time range. The service provides aggregate statistics including success rate, response times, and entry timestamps, making it ideal for monitoring, debugging, and performance analysis of gRPC gateway traffic.

### Example Usage

```csharp
using System;
using System.Linq;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static void Main()
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddLogging(configure => configure.AddConsole());
        services.AddSingleton<IRequestLogService, RequestLogService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var requestLogService = serviceProvider.GetRequiredService<IRequestLogService>();

        // 1. Log a successful request
        var successfulEntry = new RequestLogEntry
        {
            GrpcMethod = "UserService/GetUserById",
            StatusCode = 0, // OK
            DurationMs = 23.5,
            IsSuccess = true,
            RequestSizeBytes = 1024,
            ResponseSizeBytes = 2048,
            Timestamp = DateTime.UtcNow,
            ClientIpAddress = "192.168.1.100"
        };
        requestLogService.Append(successfulEntry);

        // 2. Log a failed request
        var failedEntry = new RequestLogEntry
        {
            GrpcMethod = "PaymentService/ProcessPayment",
            StatusCode = 3, // INVALID_ARGUMENT
            DurationMs = 156.8,
            IsSuccess = false,
            RequestSizeBytes = 512,
            ResponseSizeBytes = 128,
            Timestamp = DateTime.UtcNow.AddSeconds(-10),
            ClientIpAddress = "192.168.1.101",
            ErrorMessage = "Insufficient funds"
        };
        requestLogService.Append(failedEntry);

        // 3. Get recent log entries
        var recentEntries = requestLogService.GetRecent(limit: 10);
        Console.WriteLine($"Recent entries: {recentEntries.Count}");
        foreach (var entry in recentEntries.Take(3))
        {
            Console.WriteLine($" - {entry.Timestamp:HH:mm:ss} | {entry.GrpcMethod} | {entry.DurationMs}ms | {(entry.IsSuccess ? "SUCCESS" : "FAILED")}");
        }

        // 4. Search logs by method
        var userServiceEntries = requestLogService.Search(methodFilter: "UserService");
        Console.WriteLine($"\nUserService entries: {userServiceEntries.Count}");

        // 5. Get aggregate statistics
        var summary = requestLogService.GetSummary();
        Console.WriteLine($"\nStatistics:");
        Console.WriteLine($" - Total entries: {summary.TotalEntries}");
        Console.WriteLine($" - Success count: {summary.SuccessCount}");
        Console.WriteLine($" - Error count: {summary.ErrorCount}");
        Console.WriteLine($" - Success rate: {summary.SuccessRatePct:F1}%");
        Console.WriteLine($" - Average duration: {summary.AverageDurationMs:F1}ms");
        Console.WriteLine($" - Min duration: {summary.MinDurationMs}ms");
        Console.WriteLine($" - Max duration: {summary.MaxDurationMs}ms");
        Console.WriteLine($" - Oldest entry: {summary.OldestEntry?.ToString("yyyy-MM-dd HH:mm:ss")}");
        Console.WriteLine($" - Newest entry: {summary.NewestEntry?.ToString("yyyy-MM-dd HH:mm:ss")}");

        // 6. Search logs by status code
        var errorEntries = requestLogService.Search(statusCode: 3);
        Console.WriteLine($"\nError entries (status code 3): {errorEntries.Count}");

        // 7. Search logs by time range
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var recentSuccessfulEntries = requestLogService.Search(
            methodFilter: "UserService",
            from: oneHourAgo,
            limit: 50
        );
        Console.WriteLine($"\nRecent successful UserService entries: {recentSuccessfulEntries.Count(re => re.IsSuccess)}");

        // 8. Clear all logs
        requestLogService.Clear();
        Console.WriteLine($"\nAfter clearing - Total entries: {requestLogService.GetSummary().TotalEntries}");
    }
}
```

### Example Usage

```csharp
using System;
using System.Threading.Tasks;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(IServiceProvider serviceProvider)
    {
        var requestLogService = serviceProvider.GetRequiredService<IRequestLogService>();

        // Simulate logging requests from a gateway middleware
        var gatewayMiddleware = new GatewayMiddleware(requestLogService);
        
        // Process a successful request
        await gatewayMiddleware.ProcessRequestAsync("UserService", "GetUserById", 23.5, true);
        
        // Process a failed request
        await gatewayMiddleware.ProcessRequestAsync("PaymentService", "ProcessPayment", 156.8, false);
        
        // Get performance metrics
        var summary = requestLogService.GetSummary();
        Console.WriteLine($"Gateway performance - Success rate: {summary.SuccessRatePct:F1}%");
        Console.WriteLine($"Average response time: {summary.AverageDurationMs:F1}ms");
    }
}

class GatewayMiddleware
{
    private readonly IRequestLogService _requestLogService;
    
    public GatewayMiddleware(IRequestLogService requestLogService)
    {
        _requestLogService = requestLogService;
    }
    
    public async Task ProcessRequestAsync(string serviceName, string methodName, double durationMs, bool isSuccess)
    {
        var entry = new RequestLogEntry
        {
            GrpcMethod = $"{serviceName}/{methodName}",
            StatusCode = isSuccess ? 0 : 3,
            DurationMs = durationMs,
            IsSuccess = isSuccess,
            Timestamp = DateTime.UtcNow,
            ClientIpAddress = "192.168.1.100"
        };
        
        _requestLogService.Append(entry);
        
        if (!isSuccess)
        {
            Console.WriteLine($"Request failed: {serviceName}/{methodName} in {durationMs}ms");
        }
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

## IRequestLogService

`IRequestLogService` stores and provides queryable access to recent gateway request/response log entries. It maintains a fixed-capacity ring buffer of request logs that can be queried by method, status code, or time range. The service provides aggregate statistics including success rate, response times, and entry timestamps, making it ideal for monitoring, debugging, and performance analysis of gRPC gateway traffic.

### Example Usage

```csharp
using System;
using System.Linq;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static void Main()
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddLogging(configure => configure.AddConsole());
        services.AddSingleton<IRequestLogService, RequestLogService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var requestLogService = serviceProvider.GetRequiredService<IRequestLogService>();

        // 1. Log a successful request
        var successfulEntry = new RequestLogEntry
        {
            GrpcMethod = "UserService/GetUserById",
            StatusCode = 0, // OK
            DurationMs = 23.5,
            IsSuccess = true,
            RequestSizeBytes = 1024,
            ResponseSizeBytes = 2048,
            Timestamp = DateTime.UtcNow,
            ClientIpAddress = "192.168.1.100"
        };
        requestLogService.Append(successfulEntry);

        // 2. Log a failed request
        var failedEntry = new RequestLogEntry
        {
            GrpcMethod = "PaymentService/ProcessPayment",
            StatusCode = 3, // INVALID_ARGUMENT
            DurationMs = 156.8,
            IsSuccess = false,
            RequestSizeBytes = 512,
            ResponseSizeBytes = 128,
            Timestamp = DateTime.UtcNow.AddSeconds(-10),
            ClientIpAddress = "192.168.1.101",
            ErrorMessage = "Insufficient funds"
        };
        requestLogService.Append(failedEntry);

        // 3. Get recent log entries
        var recentEntries = requestLogService.GetRecent(limit: 10);
        Console.WriteLine($"Recent entries: {recentEntries.Count}");
        foreach (var entry in recentEntries.Take(3))
        {
            Console.WriteLine($" - {entry.Timestamp:HH:mm:ss} | {entry.GrpcMethod} | {entry.DurationMs}ms | {(entry.IsSuccess ? "SUCCESS" : "FAILED")}");
        }

        // 4. Search logs by method
        var userServiceEntries = requestLogService.Search(methodFilter: "UserService");
        Console.WriteLine($"\nUserService entries: {userServiceEntries.Count}");

        // 5. Get aggregate statistics
        var summary = requestLogService.GetSummary();
        Console.WriteLine($"\nStatistics:");
        Console.WriteLine($" - Total entries: {summary.TotalEntries}");
        Console.WriteLine($" - Success count: {summary.SuccessCount}");
        Console.WriteLine($" - Error count: {summary.ErrorCount}");
        Console.WriteLine($" - Success rate: {summary.SuccessRatePct:F1}%");
        Console.WriteLine($" - Average duration: {summary.AverageDurationMs:F1}ms");
        Console.WriteLine($" - Min duration: {summary.MinDurationMs}ms");
        Console.WriteLine($" - Max duration: {summary.MaxDurationMs}ms");
        Console.WriteLine($" - Oldest entry: {summary.OldestEntry?.ToString("yyyy-MM-dd HH:mm:ss")}");
        Console.WriteLine($" - Newest entry: {summary.NewestEntry?.ToString("yyyy-MM-dd HH:mm:ss")}");

        // 6. Search logs by status code
        var errorEntries = requestLogService.Search(statusCode: 3);
        Console.WriteLine($"\nError entries (status code 3): {errorEntries.Count}");

        // 7. Search logs by time range
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var recentSuccessfulEntries = requestLogService.Search(
            methodFilter: "UserService",
            from: oneHourAgo,
            limit: 50
        );
        Console.WriteLine($"\nRecent successful UserService entries: {recentSuccessfulEntries.Count(re => re.IsSuccess)}");

        // 8. Clear all logs
        requestLogService.Clear();
        Console.WriteLine($"\nAfter clearing - Total entries: {requestLogService.GetSummary().TotalEntries}");
    }
}
```

### Example Usage

```csharp
using System;
using System.Threading.Tasks;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(IServiceProvider serviceProvider)
    {
        var requestLogService = serviceProvider.GetRequiredService<IRequestLogService>();

        // Simulate logging requests from a gateway middleware
        var gatewayMiddleware = new GatewayMiddleware(requestLogService);
        
        // Process a successful request
        await gatewayMiddleware.ProcessRequestAsync("UserService", "GetUserById", 23.5, true);
        
        // Process a failed request
        await gatewayMiddleware.ProcessRequestAsync("PaymentService", "ProcessPayment", 156.8, false);
        
        // Get performance metrics
        var summary = requestLogService.GetSummary();
        Console.WriteLine($"Gateway performance - Success rate: {summary.SuccessRatePct:F1}%");
        Console.WriteLine($"Average response time: {summary.AverageDurationMs:F1}ms");
    }
}

class GatewayMiddleware
{
    private readonly IRequestLogService _requestLogService;
    
    public GatewayMiddleware(IRequestLogService requestLogService)
    {
        _requestLogService = requestLogService;
    }
    
    public async Task ProcessRequestAsync(string serviceName, string methodName, double durationMs, bool isSuccess)
    {
        var entry = new RequestLogEntry
        {
            GrpcMethod = $"{serviceName}/{methodName}",
            StatusCode = isSuccess ? 0 : 3,
            DurationMs = durationMs,
            IsSuccess = isSuccess,
            Timestamp = DateTime.UtcNow,
            ClientIpAddress = "192.168.1.100"
        };
        
        _requestLogService.Append(entry);
        
        if (!isSuccess)
        {
            Console.WriteLine($"Request failed: {serviceName}/{methodName} in {durationMs}ms");
        }
    }
}
```

## IValidationService

`IValidationService` provides validation capabilities for gateway configuration, gRPC services, routes, and authentication tokens. It validates gateway settings, service configurations, route definitions, IP addresses, and authentication/authorization tokens to ensure the gateway operates with valid and secure configurations.

### Example Usage

```csharp
using System;
using System.Threading.Tasks;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;

class Program
{
    static async Task Main(IServiceProvider serviceProvider)
    {
        var validationService = serviceProvider.GetRequiredService<IValidationService>();

        // 1. Validate gateway configuration
        var config = new GatewayConfiguration { Name = "MyGateway", Port = 5000 };
        validationService.ValidateGatewayConfiguration(config);
        Console.WriteLine("Gateway configuration validated successfully");

        // 2. Validate gRPC service
        var service = new GrpcService { Name = "UserService", Host = "localhost", Port = 5001 };
        validationService.ValidateGrpcService(service);
        Console.WriteLine("gRPC service validated successfully");

        // 3. Validate gateway route
        var route = new GatewayRoute { Pattern = "/api/users", TargetServiceId = 10, Priority = 1 };
        validationService.ValidateGatewayRoute(route);
        Console.WriteLine("Gateway route validated successfully");

        // 4. Validate request metric
        var metric = new RequestMetric { ServiceName = "UserService", MethodName = "GetUser", DurationMs = 45.2, ClientIpAddress = "192.168.1.100" };
        validationService.ValidateRequestMetric(metric);
        Console.WriteLine("Request metric validated successfully");

        // 5. Validate IP address
        var ipValid = validationService.ValidateIpAddress("192.168.1.100");
        Console.WriteLine($"IP address valid: {ipValid}");

        // 6. Validate authentication token
        var tokenValid = await validationService.ValidateAuthenticationTokenAsync("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0IiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c");
        Console.WriteLine($"Authentication token valid: {tokenValid}");

        // 7. Validate authorization
        var authValid = await validationService.ValidateAuthorizationAsync("user-123", 10);
        Console.WriteLine($"Authorization valid: {authValid}");
    }
}
```

## IRequestLogService

`IRequestLogService` stores and provides queryable access to recent gateway request/response log entries. It maintains a fixed-capacity ring buffer of request logs that can be queried by method, status code, or time range. The service provides aggregate statistics including success rate, response times, and entry timestamps, making it ideal for monitoring, debugging, and performance analysis of gRPC gateway traffic.

### Example Usage

```csharp
using System;
using System.Linq;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static void Main()
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddLogging(configure => configure.AddConsole());
        services.AddSingleton<IRequestLogService, RequestLogService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var requestLogService = serviceProvider.GetRequiredService<IRequestLogService>();

        // 1. Log a successful request
        var successfulEntry = new RequestLogEntry
        {
            GrpcMethod = "UserService/GetUserById",
            StatusCode = 0, // OK
            DurationMs = 23.5,
            IsSuccess = true,
            RequestSizeBytes = 1024,
            ResponseSizeBytes = 2048,
            Timestamp = DateTime.UtcNow,
            ClientIpAddress = "192.168.1.100"
        };
        requestLogService.Append(successfulEntry);

        // 2. Log a failed request
        var failedEntry = new RequestLogEntry
        {
            GrpcMethod = "PaymentService/ProcessPayment",
            StatusCode = 3, // INVALID_ARGUMENT
            DurationMs = 156.8,
            IsSuccess = false,
            RequestSizeBytes = 512,
            ResponseSizeBytes = 128,
            Timestamp = DateTime.UtcNow.AddSeconds(-10),
            ClientIpAddress = "192.168.1.101",
            ErrorMessage = "Insufficient funds"
        };
        requestLogService.Append(failedEntry);

        // 3. Get recent log entries
        var recentEntries = requestLogService.GetRecent(limit: 10);
        Console.WriteLine($"Recent entries: {recentEntries.Count}");
        foreach (var entry in recentEntries.Take(3))
        {
            Console.WriteLine($" - {entry.Timestamp:HH:mm:ss} | {entry.GrpcMethod} | {entry.DurationMs}ms | {(entry.IsSuccess ? "SUCCESS" : "FAILED")}");
        }

        // 4. Search logs by method
        var userServiceEntries = requestLogService.Search(methodFilter: "UserService");
        Console.WriteLine($"\nUserService entries: {userServiceEntries.Count}");

        // 5. Get aggregate statistics
        var summary = requestLogService.GetSummary();
        Console.WriteLine($"\nStatistics:");
        Console.WriteLine($" - Total entries: {summary.TotalEntries}");
        Console.WriteLine($" - Success count: {summary.SuccessCount}");
        Console.WriteLine($" - Error count: {summary.ErrorCount}");
        Console.WriteLine($" - Success rate: {summary.SuccessRatePct:F1}%");
        Console.WriteLine($" - Average duration: {summary.AverageDurationMs:F1}ms");
        Console.WriteLine($" - Min duration: {summary.MinDurationMs}ms");
        Console.WriteLine($" - Max duration: {summary.MaxDurationMs}ms");
        Console.WriteLine($" - Oldest entry: {summary.OldestEntry?.ToString("yyyy-MM-dd HH:mm:ss")}");
        Console.WriteLine($" - Newest entry: {summary.NewestEntry?.ToString("yyyy-MM-dd HH:mm:ss")}");

        // 6. Search logs by status code
        var errorEntries = requestLogService.Search(statusCode: 3);
        Console.WriteLine($"\nError entries (status code 3): {errorEntries.Count}");

        // 7. Search logs by time range
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var recentSuccessfulEntries = requestLogService.Search(
            methodFilter: "UserService",
            from: oneHourAgo,
            limit: 50
        );
        Console.WriteLine($"\nRecent successful UserService entries: {recentSuccessfulEntries.Count(re => re.IsSuccess)}");

        // 8. Clear all logs
        requestLogService.Clear();
        Console.WriteLine($"\nAfter clearing - Total entries: {requestLogService.GetSummary().TotalEntries}");
    }
}
```

### Example Usage

```csharp
using System;
using System.Threading.Tasks;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(IServiceProvider serviceProvider)
    {
        var requestLogService = serviceProvider.GetRequiredService<IRequestLogService>();

        // Simulate logging requests from a gateway middleware
        var gatewayMiddleware = new GatewayMiddleware(requestLogService);
        
        // Process a successful request
        await gatewayMiddleware.ProcessRequestAsync("UserService", "GetUserById", 23.5, true);
        
        // Process a failed request
        await gatewayMiddleware.ProcessRequestAsync("PaymentService", "ProcessPayment", 156.8, false);
        
        // Get performance metrics
        var summary = requestLogService.GetSummary();
        Console.WriteLine($"Gateway performance - Success rate: {summary.SuccessRatePct:F1}%");
        Console.WriteLine($"Average response time: {summary.AverageDurationMs:F1}ms");
    }
}

class GatewayMiddleware
{
    private readonly IRequestLogService _requestLogService;
    
    public GatewayMiddleware(IRequestLogService requestLogService)
    {
        _requestLogService = requestLogService;
    }
    
    public async Task ProcessRequestAsync(string serviceName, string methodName, double durationMs, bool isSuccess)
    {
        var entry = new RequestLogEntry
        {
            GrpcMethod = $"{serviceName}/{methodName}",
            StatusCode = isSuccess ? 0 : 3,
            DurationMs = durationMs,
            IsSuccess = isSuccess,
            Timestamp = DateTime.UtcNow,
            ClientIpAddress = "192.168.1.100"
        };
        
        _requestLogService.Append(entry);
        
        if (!isSuccess)
        {
            Console.WriteLine($"Request failed: {serviceName}/{methodName} in {durationMs}ms");
        }
    }
}
```

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

## IRequestLogService

`IRequestLogService` stores and provides queryable access to recent gateway request/response log entries. It maintains a fixed-capacity ring buffer of request logs that can be queried by method, status code, or time range. The service provides aggregate statistics including success rate, response times, and entry timestamps, making it ideal for monitoring, debugging, and performance analysis of gRPC gateway traffic.

### Example Usage

```csharp
using System;
using System.Linq;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static void Main()
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddLogging(configure => configure.AddConsole());
        services.AddSingleton<IRequestLogService, RequestLogService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var requestLogService = serviceProvider.GetRequiredService<IRequestLogService>();

        // 1. Log a successful request
        var successfulEntry = new RequestLogEntry
        {
            GrpcMethod = "UserService/GetUserById",
            StatusCode = 0, // OK
            DurationMs = 23.5,
            IsSuccess = true,
            RequestSizeBytes = 1024,
            ResponseSizeBytes = 2048,
            Timestamp = DateTime.UtcNow,
            ClientIpAddress = "192.168.1.100"
        };
        requestLogService.Append(successfulEntry);

        // 2. Log a failed request
        var failedEntry = new RequestLogEntry
        {
            GrpcMethod = "PaymentService/ProcessPayment",
            StatusCode = 3, // INVALID_ARGUMENT
            DurationMs = 156.8,
            IsSuccess = false,
            RequestSizeBytes = 512,
            ResponseSizeBytes = 128,
            Timestamp = DateTime.UtcNow.AddSeconds(-10),
            ClientIpAddress = "192.168.1.101",
            ErrorMessage = "Insufficient funds"
        };
        requestLogService.Append(failedEntry);

        // 3. Get recent log entries
        var recentEntries = requestLogService.GetRecent(limit: 10);
        Console.WriteLine($"Recent entries: {recentEntries.Count}");
        foreach (var entry in recentEntries.Take(3))
        {
            Console.WriteLine($" - {entry.Timestamp:HH:mm:ss} | {entry.GrpcMethod} | {entry.DurationMs}ms | {(entry.IsSuccess ? "SUCCESS" : "FAILED")}");
        }

        // 4. Search logs by method
        var userServiceEntries = requestLogService.Search(methodFilter: "UserService");
        Console.WriteLine($"\nUserService entries: {userServiceEntries.Count}");

        // 5. Get aggregate statistics
        var summary = requestLogService.GetSummary();
        Console.WriteLine($"\nStatistics:");
        Console.WriteLine($" - Total entries: {summary.TotalEntries}");
        Console.WriteLine($" - Success count: {summary.SuccessCount}");
        Console.WriteLine($" - Error count: {summary.ErrorCount}");
        Console.WriteLine($" - Success rate: {summary.SuccessRatePct:F1}%");
        Console.WriteLine($" - Average duration: {summary.AverageDurationMs:F1}ms");
        Console.WriteLine($" - Min duration: {summary.MinDurationMs}ms");
        Console.WriteLine($" - Max duration: {summary.MaxDurationMs}ms");
        Console.WriteLine($" - Oldest entry: {summary.OldestEntry?.ToString("yyyy-MM-dd HH:mm:ss")}");
        Console.WriteLine($" - Newest entry: {summary.NewestEntry?.ToString("yyyy-MM-dd HH:mm:ss")}");

        // 6. Search logs by status code
        var errorEntries = requestLogService.Search(statusCode: 3);
        Console.WriteLine($"\nError entries (status code 3): {errorEntries.Count}");

        // 7. Search logs by time range
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var recentSuccessfulEntries = requestLogService.Search(
            methodFilter: "UserService",
            from: oneHourAgo,
            limit: 50
        );
        Console.WriteLine($"\nRecent successful UserService entries: {recentSuccessfulEntries.Count(re => re.IsSuccess)}");

        // 8. Clear all logs
        requestLogService.Clear();
        Console.WriteLine($"\nAfter clearing - Total entries: {requestLogService.GetSummary().TotalEntries}");
    }
}
```

### Example Usage

```csharp
using System;
using System.Threading.Tasks;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(IServiceProvider serviceProvider)
    {
        var requestLogService = serviceProvider.GetRequiredService<IRequestLogService>();

        // Simulate logging requests from a gateway middleware
        var gatewayMiddleware = new GatewayMiddleware(requestLogService);
        
        // Process a successful request
        await gatewayMiddleware.ProcessRequestAsync("UserService", "GetUserById", 23.5, true);
        
        // Process a failed request
        await gatewayMiddleware.ProcessRequestAsync("PaymentService", "ProcessPayment", 156.8, false);
        
        // Get performance metrics
        var summary = requestLogService.GetSummary();
        Console.WriteLine($"Gateway performance - Success rate: {summary.SuccessRatePct:F1}%");
        Console.WriteLine($"Average response time: {summary.AverageDurationMs:F1}ms");
    }
}

class GatewayMiddleware
{
    private readonly IRequestLogService _requestLogService;
    
    public GatewayMiddleware(IRequestLogService requestLogService)
    {
        _requestLogService = requestLogService;
    }
    
    public async Task ProcessRequestAsync(string serviceName, string methodName, double durationMs, bool isSuccess)
    {
        var entry = new RequestLogEntry
        {
            GrpcMethod = $"{serviceName}/{methodName}",
            StatusCode = isSuccess ? 0 : 3,
            DurationMs = durationMs,
            IsSuccess = isSuccess,
            Timestamp = DateTime.UtcNow,
            ClientIpAddress = "192.168.1.100"
        };
        
        _requestLogService.Append(entry);
        
        if (!isSuccess)
        {
            Console.WriteLine($"Request failed: {serviceName}/{methodName} in {durationMs}ms");
        }
    }
}
```

