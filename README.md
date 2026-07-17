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

## CircuitBreakerTests

`CircuitBreakerTests` is a comprehensive test class that validates the behavior of the `CircuitBreaker` class, which implements the circuit breaker pattern to prevent cascading failures in distributed systems. The tests cover state transitions (closed → open → half-open → closed), failure threshold tracking, request allowance control, and circuit reset functionality. Each test verifies that the circuit breaker correctly manages service availability based on failure patterns.

### Example Usage

```csharp
using DotNetGrpcGateway.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;

// 1. Create a circuit breaker with configuration
var loggerMock = new Mock<ILogger<CircuitBreaker>>();
var options = new CircuitBreakerOptions
{
    FailureThreshold = 3,      // Open circuit after 3 consecutive failures
    OpenDuration = TimeSpan.FromSeconds(30),  // Circuit stays open for 30 seconds
    HalfOpenSuccessThreshold = 2  // Close circuit after 2 successful requests in half-open state
};

var circuitBreaker = new CircuitBreaker(
    serviceId: 1,  // Unique identifier for the service being monitored
    options,
    loggerMock.Object
);

Console.WriteLine($"Initial state: {circuitBreaker.State}");  // Closed
Console.WriteLine($"Can make requests: {circuitBreaker.AllowRequest()}");  // True

// 2. Simulate failures below threshold - circuit remains closed
circuitBreaker.RecordFailure();
circuitBreaker.RecordFailure();
Console.WriteLine($"After 2 failures - State: {circuitBreaker.State}, Can request: {circuitBreaker.AllowRequest()}");  // Closed, True

// 3. Reach failure threshold - circuit opens
circuitBreaker.RecordFailure();
Console.WriteLine($"After 3rd failure - State: {circuitBreaker.State}, Can request: {circuitBreaker.AllowRequest()}");  // Open, False

// 4. Wait for circuit to transition to half-open (after OpenDuration elapses)
// In production, this happens automatically via timer; for testing we can simulate:
circuitBreaker.RecordSuccess();  // This would transition to HalfOpen state
Console.WriteLine($"After success - State: {circuitBreaker.State}");  // HalfOpen

// 5. Circuit is half-open - allow limited requests
if (circuitBreaker.AllowRequest())
{
    Console.WriteLine("Making request in half-open state...");
    circuitBreaker.RecordSuccess();  // First success in half-open
    circuitBreaker.RecordSuccess();  // Second success - closes circuit
    Console.WriteLine($"After 2 successes - State: {circuitBreaker.State}");  // Closed
}

// 6. If failure occurs in half-open, circuit re-opens
circuitBreaker.RecordFailure();
Console.WriteLine($"After failure in half-open - State: {circuitBreaker.State}");  // Open

// 7. Reset circuit breaker (manually close and clear counters)
circuitBreaker.Reset();
Console.WriteLine($"After reset - State: {circuitBreaker.State}, Failures: {circuitBreaker.ConsecutiveFailures}");  // Closed, 0
```

## BasicServiceRegistrationExample

`BasicServiceRegistrationExample` demonstrates how to register a gRPC service with the gateway using the REST API. It covers the complete lifecycle from service registration to health checking and unregistration, providing a practical starting point for integrating services with the gRPC gateway infrastructure.

### Example Usage

```csharp
using DotNetGrpcGateway.Examples;

// 1. Create an instance of the basic service registration example
var example = new BasicServiceRegistrationExample();

Console.WriteLine("=== Basic Service Registration Example ===\n");

// 2. Register a new gRPC service with the gateway
Console.WriteLine("Step 1: Registering UserService...");
await example.RegisterServiceAsync();
await Task.Delay(1000);

// 3. Verify the service was registered by retrieving all services
Console.WriteLine("\nStep 2: Verifying service registration...");
await example.VerifyServiceRegistrationAsync();
await Task.Delay(1000);

// 4. Check health status of the registered service
Console.WriteLine("\nStep 3: Checking service health (service ID: 1)...");
await example.CheckServiceHealthAsync(1);
await Task.Delay(1000);

// 5. Unregister the service from the gateway (optional)
// await example.UnregisterServiceAsync(1);

Console.WriteLine("\n=== Example complete ===");
```

## HealthChecksAndMonitoringExample

`HealthChecksAndMonitoringExample` demonstrates comprehensive health checking and monitoring capabilities of the gRPC gateway. It provides methods to check gateway health status, monitor individual service health, perform readiness and liveness probes (for Kubernetes environments), and poll health status over time. This example is essential for production monitoring, load balancer integration, and ensuring service reliability.

### Example Usage

```csharp
using DotNetGrpcGateway.Examples;

// 1. Create an instance of the health monitoring example
var example = new HealthChecksAndMonitoringExample();

Console.WriteLine("=== Health Checks & Monitoring Example ===");

// 2. Check basic gateway health status
Console.WriteLine("\nStep 1: Checking gateway health...");
await example.CheckGatewayHealthAsync();

// 3. Get detailed health information including uptime and metrics
await example.DisplayDetailedHealthStatusAsync();

// 4. Check health status of all registered services
await example.DisplayAllServicesHealthAsync();

// 5. Get detailed health information for a specific service
await example.DisplayServiceHealthDetailAsync(1);

// 6. Check readiness probe (for load balancers)
await example.CheckReadinessAsync();

// 7. Check liveness probe (for Kubernetes)
await example.CheckLivenessAsync();

// 8. Poll health status over a period of time for monitoring
await example.PollHealthStatusAsync(intervalSeconds: 5, durationSeconds: 30);
```

## AuthenticationAndSecurityExample

`AuthenticationAndSecurityExample` demonstrates authentication and security features of the gRPC gateway, including token-based authentication, route protection, tiered access control, and security validation. This example shows how to create authentication tokens, protect routes with authentication requirements, configure rate limits for different user tiers, and test security enforcement.

### Example Usage

```csharp
using DotNetGrpcGateway.Examples;

// 1. Create an instance of the authentication and security example
var example = new AuthenticationAndSecurityExample();

Console.WriteLine("=== Authentication & Security Example ===");

// 2. Create authentication tokens for different applications
Console.WriteLine("\nStep 1: Creating authentication tokens...");
var clientToken = await example.CreateAuthenticationTokenAsync(
    "client-token-12345",
    "Client Application"
);
var adminToken = await example.CreateAuthenticationTokenAsync(
    "admin-token-67890",
    "Admin User"
);

// 3. Create protected routes that require authentication
Console.WriteLine("\nStep 2: Creating protected routes...");
await example.CreateProtectedRouteAsync(
    serviceId: 1,
    pattern: "admin.*",
    description: "Admin operations"
);
await example.CreateProtectedRouteAsync(
    serviceId: 1,
    pattern: "api.secure.*",
    description: "Secure API endpoints"
);

// 4. Configure tiered access with different rate limits
Console.WriteLine("\nStep 3: Configuring tiered access...");
await example.ConfigureTieredAccessAsync(serviceId: 1);

// 5. Display all registered tokens
Console.WriteLine("\nStep 4: Displaying tokens...");
await example.DisplayTokensAsync();

// 6. Test that protected endpoints block unauthorized access
Console.WriteLine("\nStep 5: Testing security...");
await example.TryUnauthorizedAccessAsync("http://localhost:5000/admin/endpoint");

// 7. Make an authenticated request to a protected endpoint
if (clientToken is not null)
{
    Console.WriteLine("\nStep 6: Making authenticated request...");
    await example.MakeAuthenticatedRequestAsync(
        clientToken,
        "http://localhost:5000/api/secure/data"
    );
}
```

## DynamicRouteConfigurationExample

`DynamicRouteConfigurationExample` demonstrates how to configure routes dynamically in the gRPC gateway, including pattern-based routing, priority-based matching, rate limiting, and response caching. This example shows how to create routes with different patterns and priorities, display all configured routes, test route matching for specific requests, and detect potential route conflicts.

### Example Usage

```csharp
using DotNetGrpcGateway.Examples;

// 1. Create an instance of the dynamic route configuration example
var example = new DynamicRouteConfigurationExample();

Console.WriteLine("=== Dynamic Route Configuration Example ===\n");

// 2. Configure routes with different patterns and priorities
Console.WriteLine("Step 1: Creating routes...");
await example.ConfigureRoutesAsync(1);
await Task.Delay(1000);

// 3. Display all configured routes
Console.WriteLine("\nStep 2: Displaying all routes...");
await example.DisplayAllRoutesAsync();
await Task.Delay(1000);

// 4. Test route matching for a specific service/method
Console.WriteLine("\nStep 3: Testing route matching...");
await example.TestRouteMatchingAsync("user.UserService", "GetUser");
await Task.Delay(1000);

// 5. Detect potential route conflicts
Console.WriteLine("\nStep 4: Detecting potential conflicts...");
await example.DetectConflictsAsync();
```

## MetricsAndMonitoringExample

`MetricsAndMonitoringExample` demonstrates how to collect and analyze comprehensive metrics from the gRPC gateway, including performance statistics, slow request detection, error tracking, and endpoint analysis. This example is essential for monitoring gateway health, identifying performance bottlenecks, and making data-driven optimization decisions.

### Example Usage

```csharp
using DotNetGrpcGateway.Examples;

// 1. Create an instance of the metrics and monitoring example
var example = new MetricsAndMonitoringExample();

Console.WriteLine("=== Metrics & Monitoring Example ===");

// 2. Display overall performance metrics including throughput, latency percentiles, and error rates
Console.WriteLine("\nStep 1: Retrieving performance metrics...");
await example.DisplayPerformanceMetricsAsync();

// 3. Display today's statistics including total requests, success/failure counts, and error rates
Console.WriteLine("\nStep 2: Retrieving today's statistics...");
await example.DisplayTodayStatisticsAsync();

// 4. Display slow requests exceeding a latency threshold (default: 1000ms)
Console.WriteLine("\nStep 3: Retrieving slow requests...");
await example.DisplaySlowRequestsAsync(thresholdMs: 500);

// 5. Display error distribution by HTTP status code
Console.WriteLine("\nStep 4: Retrieving error distribution...");
await example.DisplayErrorDistributionAsync();

// 6. Display top endpoints by request volume
Console.WriteLine("\nStep 5: Retrieving top endpoints...");
await example.DisplayTopEndpointsAsync();

// 7. Reset all metrics (careful: this clears all data)
Console.WriteLine("\nStep 6: Resetting metrics...");
await example.ResetMetricsAsync();
```

## StructuredLoggerValidation

`StructuredLoggerValidation` provides validation helpers for structured logging method parameters in the gRPC gateway. It validates input parameters before they are passed to logging methods to ensure they meet expected criteria and prevent invalid log entries. The class offers validation methods that return lists of validation problems, boolean check methods for quick validation, and ensure methods that throw exceptions on invalid parameters.

### Example Usage

```csharp
using DotNetGrpcGateway.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;

// 1. Create a mock logger
var loggerMock = new Mock<ILogger>();

// 2. Validate request start parameters
var requestStartProblems = StructuredLoggerValidation.ValidateLogRequestStart(
    loggerMock.Object,
    requestId: "req-12345",
    path: "/api/users/get",
    method: "GET",
    clientIp: "192.168.1.100"
);

if (requestStartProblems.Count == 0)
{
    Console.WriteLine("Request start parameters are valid!");
}
else
{
    Console.WriteLine("Validation problems found:");
    foreach (var problem in requestStartProblems)
    {
        Console.WriteLine($"- {problem}");
    }
}

// 3. Validate request complete parameters
var requestCompleteProblems = StructuredLoggerValidation.ValidateLogRequestComplete(
    loggerMock.Object,
    requestId: "req-12345",
    path: "/api/users/get",
    statusCode: 200,
    durationMs: 150
);

if (StructuredLoggerValidation.IsValidLogRequestComplete(
    loggerMock.Object,
    "req-12345",
    "/api/users/get",
    200,
    150))
{
    Console.WriteLine("Request complete parameters are valid!");
}

// 4. Validate service discovery parameters
var serviceDiscoveryProblems = StructuredLoggerValidation.ValidateLogServiceDiscovery(
    loggerMock.Object,
    serviceId: 1,
    serviceName: "UserService",
    healthy: true
);

if (serviceDiscoveryProblems.Count == 0)
{
    Console.WriteLine("Service discovery parameters are valid!");
}

// 5. Validate cache operation parameters
var cacheProblems = StructuredLoggerValidation.ValidateLogCacheOperation(
    loggerMock.Object,
    operation: "Get",
    key: "user:123",
    hit: true
);

if (StructuredLoggerValidation.IsValidLogCacheOperation(
    loggerMock.Object,
    "Get",
    "user:123",
    true))
{
    Console.WriteLine("Cache operation parameters are valid!");
}

// 6. Validate route resolution parameters
var routeProblems = StructuredLoggerValidation.ValidateLogRouteResolution(
    loggerMock.Object,
    path: "/api/users/get",
    routePattern: "api/users/{id}",
    targetServiceId: 1
);

if (StructuredLoggerValidation.IsValidLogRouteResolution(
    loggerMock.Object,
    "/api/users/get",
    "api/users/{id}",
    1))
{
    Console.WriteLine("Route resolution parameters are valid!");
}

// 7. Validate rate limit parameters
var rateLimitProblems = StructuredLoggerValidation.ValidateLogRateLimit(
    loggerMock.Object,
    clientIp: "192.168.1.100",
    path: "/api/users/get",
    limit: 100
);

if (StructuredLoggerValidation.IsValidLogRateLimit(
    loggerMock.Object,
    "192.168.1.100",
    "/api/users/get",
    100))
{
    Console.WriteLine("Rate limit parameters are valid!");
}

// 8. Validate authentication parameters
var authProblems = StructuredLoggerValidation.ValidateLogAuthentication(
    loggerMock.Object,
    userId: "user-456",
    success: true,
    reason: null
);

if (StructuredLoggerValidation.IsValidLogAuthentication(
    loggerMock.Object,
    "user-456",
    true))
{
    Console.WriteLine("Authentication parameters are valid!");
}

// 9. Validate critical error parameters
var errorProblems = StructuredLoggerValidation.ValidateLogCriticalError(
    loggerMock.Object,
    ex: new InvalidOperationException("Database connection failed"),
    context: "DatabaseService.Connect",
    additionalData: new Dictionary<string, object>
    {
        ["database"] = "users_db",
        ["attempt"] = 3
    }
);

if (StructuredLoggerValidation.IsValidLogCriticalError(
    loggerMock.Object,
    new InvalidOperationException("Database connection failed"),
    "DatabaseService.Connect"))
{
    Console.WriteLine("Critical error parameters are valid!");
}

// 10. Validate performance metrics parameters
var perfProblems = StructuredLoggerValidation.ValidateLogPerformanceMetrics(
    loggerMock.Object,
    operation: "GetUserById",
    durationMs: 45,
    itemCount: 1
);

if (StructuredLoggerValidation.IsValidLogPerformanceMetrics(
    loggerMock.Object,
    "GetUserById",
    45,
    1))
{
    Console.WriteLine("Performance metrics parameters are valid!");
}

// 11. Use Ensure methods to throw exceptions on invalid parameters
try
{
    StructuredLoggerValidation.EnsureValidLogRequestStart(
        loggerMock.Object,
        requestId: "req-12345",
        path: "/api/users/get",
        method: "GET",
        clientIp: "192.168.1.100"
    );
    Console.WriteLine("All parameters validated successfully!");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}
```

## DateTimeUtilityValidation

`DateTimeUtilityValidation` provides utility methods for validating and working with DateTime values in the gRPC gateway. It includes validation methods that return lists of validation problems, boolean check methods for quick validation, and ensure methods that throw exceptions on invalid DateTime parameters. The class is particularly useful for validating request timestamps, business hours constraints, and date range boundaries.

### Example Usage

```csharp
using DotNetGrpcGateway.Utilities;
using System;

// 1. Validate a DateTime value
var validationProblems = DateTimeUtilityValidation.Validate(
    dateTime: DateTime.UtcNow,
    propertyName: "RequestTimestamp",
    minValue: DateTime.UtcNow.AddMinutes(-5),
    maxValue: DateTime.UtcNow.AddMinutes(5)
);

if (validationProblems.Count == 0)
{
    Console.WriteLine("DateTime is valid!");
}
else
{
    Console.WriteLine("Validation problems found:");
    foreach (var problem in validationProblems)
    {
        Console.WriteLine($"- {problem}");
    }
}

// 2. Quick validation check
if (DateTimeUtilityValidation.IsValid(
    dateTime: DateTime.UtcNow,
    minValue: DateTime.UtcNow.AddDays(-1),
    maxValue: DateTime.UtcNow.AddDays(1)))
{
    Console.WriteLine("DateTime is within acceptable range!");
}

// 3. Validate for business hours (9 AM to 6 PM)
var businessHoursProblems = DateTimeUtilityValidation.ValidateForBusinessHours(
    dateTime: DateTime.UtcNow,
    propertyName: "AppointmentTime"
);

if (businessHoursProblems.Count == 0)
{
    Console.WriteLine("DateTime is within business hours!");
}
else
{
    Console.WriteLine("Not within business hours:");
    foreach (var problem in businessHoursProblems)
    {
        Console.WriteLine($"- {problem}");
    }
}

// 4. Validate a nullable DateTime
var nullableValidationProblems = DateTimeUtilityValidation.Validate(
    dateTime: null,
    propertyName: "OptionalTimestamp",
    allowNull: true,
    minValue: DateTime.UtcNow.AddDays(-7),
    maxValue: DateTime.UtcNow.AddDays(7)
);

if (nullableValidationProblems.Count == 0)
{
    Console.WriteLine("Nullable DateTime is valid!");
}

// 5. Use Ensure methods to throw exceptions on invalid parameters
try
{
    DateTimeUtilityValidation.EnsureValid(
        dateTime: DateTime.UtcNow.AddDays(-10),
        propertyName: "StartDate",
        minValue: DateTime.UtcNow.AddDays(-5),
        maxValue: DateTime.UtcNow.AddDays(5)
    );
    Console.WriteLine("All parameters validated successfully!");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}

// 6. Validate for business hours with Ensure
try
{
    DateTimeUtilityValidation.EnsureValidForBusinessHours(
        dateTime: new DateTime(2024, 3, 15, 14, 30, 0), // 2:30 PM
        propertyName: "MeetingTime"
    );
    Console.WriteLine("Meeting time is within business hours!");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Meeting time validation failed: {ex.Message}");
}
```

## RequestLogServiceTests

`RequestLogServiceTests` is a comprehensive test class that validates the behavior of request logging functionality in the gRPC gateway. It tests various scenarios including successful requests, failed requests, slow requests, large payloads, cache hits/misses, and retry behavior. The tests ensure that log entries are created with appropriate log levels (INFO, WARN, ERROR) and contain the expected message patterns and metadata for different request outcomes.