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

## RequestMetricTests

`RequestMetricTests` is a comprehensive test class that validates the behavior of the `RequestMetric` class, which tracks and validates request metrics including validation rules, helper methods, and default state. The tests cover validation scenarios (empty/null required fields, negative values), slow request detection, error recording, retry tracking, cache status management, and default constructor initialization. Each test verifies that the metric class maintains data integrity and provides accurate information for monitoring and observability purposes.

### Example Usage

```csharp
using System;
using DotNetGrpcGateway.Domain;

class Program
{
static void Main()
{
// 1. Create and validate a fully populated request metric
var metric = new RequestMetric
{
ServiceName = "UserService",
MethodName = "GetUser",
ClientIpAddress = "192.168.1.1",
DurationMs = 150.5,
RequestSizeBytes = 1024,
ResponseSizeBytes = 2048,
HttpStatusCode = 200,
IsSuccessful = true
};

// Validate the metric - should not throw
metric.Validate();
Console.WriteLine("Metric validation passed!");
Console.WriteLine($"Service: {metric.ServiceName}, Method: {metric.MethodName}");
Console.WriteLine($"Duration: {metric.DurationMs}ms, Size: {metric.RequestSizeBytes} bytes");

// 2. Check if request is slow (threshold: 1000ms)
bool isSlow = metric.IsSlowRequest(slowThresholdMs: 1000);
Console.WriteLine($"Is slow request: {isSlow}");

// 3. Record an error
metric.RecordError("Connection timeout", "Timeout at line 42");
Console.WriteLine($"Error recorded: {metric.ErrorMessage}");
Console.WriteLine($"Successful: {metric.IsSuccessful}");

// 4. Record a retry
metric.RecordRetry();
Console.WriteLine($"Retry count: {metric.RetryCount}");
Console.WriteLine($"Was retried: {metric.WasRetried}");

// 5. Set cache status
metric.SetCacheStatus("HIT");
Console.WriteLine($"Cache status: {metric.CacheHitStatus}");

// 6. Create a new metric and check default values
var defaultMetric = new RequestMetric();
Console.WriteLine($"\nDefault metric values:");
Console.WriteLine($" - RequestId is not null/empty: {!string.IsNullOrEmpty(defaultMetric.RequestId)}");
Console.WriteLine($" - ServiceName is null: {defaultMetric.ServiceName == null}");
Console.WriteLine($" - MethodName is null: {defaultMetric.MethodName == null}");
Console.WriteLine($" - ClientIpAddress is null: {defaultMetric.ClientIpAddress == null}");
Console.WriteLine($" - DurationMs is 0: {defaultMetric.DurationMs == 0}");
Console.WriteLine($" - IsSuccessful is true: {defaultMetric.IsSuccessful}");
Console.WriteLine($" - WasRetried is false: {defaultMetric.WasRetried == false}");
Console.WriteLine($" - RetryCount is 0: {defaultMetric.RetryCount == 0}");
Console.WriteLine($" - CacheHitStatus is null: {defaultMetric.CacheHitStatus == null}");
}
```

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
Console.WriteLine($"Health check interval: {options.HealthCheck.IntervalSeconds}ss");
Console.WriteLine($"Health check timeout: {options.HealthCheck.TimeoutMs}ms");
Console.WriteLine($"Health check threshold: {options.HealthCheck.FailureThreshold}");
Console.WriteLine($"Metrics collection interval: {options.Metrics.CollectionIntervalSeconds}ss");
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

## ServiceHealthReportTests

`ServiceHealthReportTests` is a comprehensive test class that validates the behavior of the `ServiceHealthReport` class in the gRPC gateway's health monitoring system. It tests validation scenarios (invalid service IDs, negative success rates, null health status, negative response times), health check recording (successful checks, failed checks, multiple failures, success rate calculation), diagnostic message handling (adding messages, maintaining message limit), health status calculations (unhealthy detection after 3 failures, availability percentage), and default constructor behavior.

### Example Usage

```csharp
using DotNetGrpcGateway.Domain;
using Xunit;

public class ServiceHealthReportTestsExample
{
    [Fact]
    public void ExampleUsage()
    {
        // Create a new health report
        var report = new ServiceHealthReport
        {
            ServiceId = 1,
            IsHealthy = true,
            HealthStatus = "Healthy",
            ResponseTimeMs = 100,
            HttpStatusCode = 200,
            TotalHealthChecks = 10,
            SuccessfulHealthChecks = 10
        };

        // Validate the report - should not throw
        report.Validate();

        // Record a successful health check
        report.RecordCheckResult(success: true, responseTimeMs: 85);
        
        Assert.True(report.IsHealthy);
        Assert.Equal(11, report.TotalHealthChecks);
        Assert.Equal(11, report.SuccessfulHealthChecks);
        Assert.Equal(1, report.SuccessfulChecksInARow);
        Assert.Equal(0, report.FailedChecksInARow);

        // Record a failed health check
        report.RecordCheckResult(
            success: false,
            responseTimeMs: 200,
            errorMessage: "Connection refused"
        );
        
        Assert.Equal(12, report.TotalHealthChecks);
        Assert.Equal(11, report.SuccessfulHealthChecks);
        Assert.Equal(0, report.SuccessfulChecksInARow);
        Assert.Equal(1, report.FailedChecksInARow);
        Assert.Equal("Degraded", report.HealthStatus);

        // Add diagnostic messages
        report.AddDiagnosticMessage("Health check completed successfully");
        report.AddDiagnosticMessage("Response time within SLA");
        
        // Check availability percentage
        var availability = report.GetAvailabilityPercentage;
        Assert.Equal(91.67, availability, 2);
    }
}
```

## RequestLogServiceTests

`RequestLogServiceTests` is a comprehensive test class that validates the behavior of request logging functionality in the gRPC gateway. It tests various scenarios including successful requests, failed requests, slow requests, large payloads, cache hits/misses, and retry behavior. The tests ensure that log entries are created with appropriate log levels (INFO, WARN, ERROR) and contain the expected message patterns and metadata for different request outcomes.