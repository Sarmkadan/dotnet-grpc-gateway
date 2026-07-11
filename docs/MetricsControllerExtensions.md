# MetricsControllerExtensions
The `MetricsControllerExtensions` class provides a set of extension methods for working with metrics in a .NET gRPC gateway application. It offers various methods for retrieving performance metrics, error metrics, and request metrics, allowing developers to easily integrate metrics collection and analysis into their applications.

## API
The `MetricsControllerExtensions` class includes the following public members:
* `GetPerformanceMetricsWithDetails`: Retrieves performance metrics with detailed information. Returns a `Task<ActionResult<PerformanceMetrics>>`.
* `GetEndpointPerformanceMetrics`: Retrieves performance metrics for a specific endpoint. Returns a `Task<IActionResult>`.
* `GetErrorMetricsWithContext`: Retrieves error metrics with contextual information. Returns a `Task<IActionResult>`.
* `GetRequestMetricsWithHealth`: Retrieves request metrics with health information. Returns a `Task<IActionResult>`.
* `Range`: A string property representing the range of metrics.
* `BucketStart`: An integer property representing the start of the bucket.
* `BucketEnd`: An integer property representing the end of the bucket.
* `Count`: An integer property representing the count of metrics.
* `Percentage`: A double property representing the percentage of metrics.

## Usage
Here are two examples of using the `MetricsControllerExtensions` class:
```csharp
// Example 1: Retrieving performance metrics with details
var performanceMetrics = await MetricsControllerExtensions.GetPerformanceMetricsWithDetails();
Console.WriteLine($"Performance Metrics: {performanceMetrics.Value}");

// Example 2: Retrieving error metrics with context
var errorMetrics = await MetricsControllerExtensions.GetErrorMetricsWithContext();
Console.WriteLine($"Error Metrics: {errorMetrics}");
```

## Notes
When using the `MetricsControllerExtensions` class, note the following:
* The `GetPerformanceMetricsWithDetails`, `GetEndpointPerformanceMetrics`, `GetErrorMetricsWithContext`, and `GetRequestMetricsWithHealth` methods are asynchronous and may throw exceptions if the underlying metrics collection fails.
* The `Range`, `BucketStart`, `BucketEnd`, `Count`, and `Percentage` properties are used to filter and analyze the collected metrics.
* The class is designed to be thread-safe, allowing multiple concurrent requests to retrieve metrics without interfering with each other.
* Edge cases, such as empty or null metric collections, should be handled accordingly by the application using this class.
