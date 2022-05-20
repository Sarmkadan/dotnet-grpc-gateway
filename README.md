## DotnetGrpcGatewayOptionsExtensions

The `DotnetGrpcGatewayOptionsExtensions` class provides a set of extension methods for configuring `DotnetGrpcGatewayOptions` instances. These methods allow for easy customization of gateway settings, such as listening addresses, ports, and health check configurations.

### Example Usage:

```csharp
var options = new DotnetGrpcGatewayOptions();

options
  .UseLocalhost()
  .UsePort(50051)
  .DisableReflection()
  .ConfigureHealthCheck(hc => hc.FailureThreshold = 3)
  .SetLogLevel("Information")
  .SetRequestLoggingVerbosity(RequestLoggingVerbosity.Verbose)
  .SetRequestLoggingEnabled(true);
```

## MetricsControllerExtensions

The `MetricsControllerExtensions` class provides extension methods for the `MetricsController` that enhance metrics reporting with additional context, filtering, and calculated statistics. These methods allow for detailed performance analysis, endpoint-specific metrics, error tracking with time-based context, and health indicators based on request patterns.

### Example Usage:

```csharp
var controller = new MetricsController();

// Get performance metrics with latency histogram
var performanceMetrics = await controller.GetPerformanceMetricsWithDetails(
  includeHistogram: true,
  histogramBucketSize: 50);

// Get endpoint-specific performance metrics with percentiles
var endpointMetrics = await controller.GetEndpointPerformanceMetrics(
  endpointName: "/api/users",
  includePercentiles: true);

// Get error metrics from the last 14 days with top 20 error codes
var errorMetrics = await controller.GetErrorMetricsWithContext(
  daysBack: 14,
  topN: 20);

// Get request metrics with health indicators from the last 30 days
var requestMetrics = await controller.GetRequestMetricsWithHealth(
  daysBack: 30,
  healthyThreshold: 1500);
```

### LatencyBucket Properties

The `LatencyBucket` class returned by `GetPerformanceMetricsWithDetails` contains:

- **Range**: `string` - The label describing the bucket range (e.g., "0-50ms", "50-100ms")
- **BucketStart**: `int` - The start value of the bucket in milliseconds
- **BucketEnd**: `int` - The end value of the bucket in milliseconds
- **Count**: `int` - The number of requests in this bucket
- **Percentage**: `double` - The percentage of total requests in this bucket

## GatewayBenchmarks

The `GatewayBenchmarks` class provides a suite of benchmarks for testing the performance and throughput of the gRPC gateway services. It measures various operations including route resolution, load balancing, caching, service management, and performance monitoring.

### Example Usage:

```csharp
public class Program
{
  public static void Main(string[] args)
  {
    var benchmarks = new GatewayBenchmarks();
    benchmarks.Setup();

    // Run individual benchmarks
    benchmarks.FindMatchingRoute_ExactMatch();
    benchmarks.GetNextEndpoint();
    benchmarks.RegisterEndpoint();

    // Run async benchmarks
    benchmarks.GetFromCache_Miss().Wait();
    benchmarks.GetAllRoutes().Wait();
    benchmarks.RegisterService().Wait();
  }
}
```

This example demonstrates how to create an instance of `GatewayBenchmarks`, perform setup, and run individual benchmarks. You can execute the benchmarks using the `BenchmarkRunner` class provided by BenchmarkDotNet.

### Benchmark Categories

The `GatewayBenchmarks` class includes benchmarks categorized into the following areas:

* RouteResolution
* LoadBalancing
* Caching
* PerformanceMonitor
* RouteManagement
* ServiceManagement
* ServiceDiscovery
* GrpcClient
* Throughput

Each benchmark provides insights into the performance characteristics of the gRPC gateway under various scenarios.

## RequestMetricTestsExtensions

The `RequestMetricTestsExtensions` class provides extension methods for creating test instances of `RequestMetric` for unit testing scenarios. These methods simplify the creation of various metric types including valid metrics, error scenarios, slow requests, retry tracking, and performance sequences, making test code more readable and maintainable.

### Example Usage:

```csharp
using DotNetGrpcGateway.Tests;
using DotNetGrpcGateway.Domain;

// Create a valid metric with default values
var validMetric = RequestMetricTestsExtensions.CreateValidMetric(null, "UserService", "GetUserById", "192.168.1.100");

// Create a collection of metrics with varying durations for performance testing
var durationSequence = RequestMetricTestsExtensions.CreateDurationSequence(null, 5, 0, 100).ToList();

// Create a slow request metric for SLA testing
var slowMetric = RequestMetricTestsExtensions.CreateSlowRequest(null, 1500);

// Create an error metric for testing error handling
var errorMetric = RequestMetricTestsExtensions.CreateErrorMetric(null, "Database connection failed");

// Create a metric with retry tracking for circuit breaker testing
var retryMetric = RequestMetricTestsExtensions.CreateRetryMetric(null, 3);

// Create a metric with negative duration for validation testing
var negativeDurationMetric = RequestMetricTestsExtensions.CreateNegativeDurationMetric(null, -50);

// Create an invalid metric with empty service name for validation testing
var invalidServiceMetric = RequestMetricTestsExtensions.CreateInvalidServiceNameMetric(null);
```

```csharp
using DotNetGrpcGateway.Tests;

// Repeat a character multiple times
string repeatedChars = 'x'.Repeat(5); // "xxxxx"


// Check if a string contains only alphabetic characters
bool isAlphabetic = "HelloWorld".IsAlphabetic(); // true
bool isNotAlphabetic = "Hello123".IsAlphabetic(); // false

// Check if a string contains only numeric characters
bool isNumeric = "12345".IsNumeric(); // true
bool isNotNumeric = "123abc".IsNumeric(); // false

// Count lines in a multi-line string
int lineCount = "Line1\nLine2\nLine3".CountLines(); // 3

// Remove all whitespace from a string
string noWhitespace = "  Hello   World  ".RemoveWhitespace(); // "HelloWorld"
```

## IGatewayRepository

The `IGatewayRepository` interface defines a contract for persisting and retrieving `GatewayConfiguration` entities. It supports CRUD operations, querying all or active configurations, and counting the total number of configurations. The concrete implementation, `GatewayRepository`, uses an in-memory store but can be swapped for a database‑backed implementation.

### Example Usage

```csharp
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Infrastructure;
using System.Collections.Generic;
using System.Threading.Tasks;

public class RepositoryDemo
{
  private readonly IGatewayRepository _repo;

  public RepositoryDemo(IGatewayRepository repo)
  {
    _repo = repo;
  }

  public async Task DemoAsync()
  {
    // Create a new configuration
    var config = new GatewayConfiguration
    {
      Name = "MyGateway",
      IsActive = true
    };
    var created = await _repo.CreateAsync(config);

    // Retrieve by ID
    var byId = await _repo.GetByIdAsync(created.Id);

    // Get all configurations
    List<GatewayConfiguration> all = await _repo.GetAllAsync();

    // Get active configurations
    List<GatewayConfiguration> active = await _repo.GetActiveAsync();

    // Update configuration
    byId.IsActive = false;
    await _repo.UpdateAsync(byId);

    // Count configurations
    int count = await _repo.CountAsync();

    // Delete configuration
    await _repo.DeleteAsync(byId.Id);
  }
}
```

This example demonstrates typical usage of the repository interface, including creation, retrieval, update, deletion, and counting of gateway configurations. The repository can be injected via dependency injection in an ASP.NET Core application.

## ICircuitBreaker

The `ICircuitBreaker` interface defines a contract for a circuit breaker that prevents cascading failures by rejecting calls when a service is known to be unhealthy. It provides methods for recording successes and failures, allowing or rejecting requests, and resetting the circuit.

### Example Usage:

```csharp
public class Service
{
  private readonly ICircuitBreaker _circuitBreaker;

  public Service(ICircuitBreaker circuitBreaker)
  {
    _circuitBreaker = circuitBreaker;
  }

  public async Task CallServiceAsync()
  {
    if (_circuitBreaker.AllowRequest())
    {
      try
      {
        // Call the service
        await CallService();
        _circuitBreaker.RecordSuccess();
      }
      catch (Exception ex)
      {
        _circuitBreaker.RecordFailure();
        throw;
      }
    }
    else
    {
      // Reject the request
      throw new CircuitBreakerException("Service is currently unavailable");
    }
  }
}
```

This example demonstrates how to use the `ICircuitBreaker` interface to prevent cascading failures by rejecting calls when a service is known to be unhealthy. The circuit breaker can be injected via dependency injection in an ASP.NET Core application.