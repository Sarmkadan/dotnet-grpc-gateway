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