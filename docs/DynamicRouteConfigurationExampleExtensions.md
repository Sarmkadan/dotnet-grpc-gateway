# DynamicRouteConfigurationExampleExtensions

`DynamicRouteConfigurationExampleExtensions` provides a set of static extension methods for configuring and managing gRPC‑Gateway routes, together with instance properties that expose runtime metrics and configuration limits for a single configuration instance.

## API

### CreateMultipleRoutesAsync
```csharp
public static async Task CreateMultipleRoutesAsync(
    IEnumerable<RouteDefinition> routeDefinitions,
    IGrpcGatewayClient gatewayClient,
    CancellationToken cancellationToken = default)
```
Creates several routes on the gateway in parallel.  
- **routeDefinitions** – collection of route descriptions to create; must not be null nor contain null elements.  
- **gatewayClient** – client used to communicate with the gRPC‑Gateway administration endpoint; must not be null.  
- **cancellationToken** – optional token to cancel the operation.  

Returns a completed `Task` when all create requests have been sent and acknowledged.  
Throws `ArgumentNullException` if `routeDefinitions` or `gatewayClient` is null, `ArgumentException` if the collection contains a null element, `OperationCanceledException` if the token is triggered, and `RpcException` if the gateway returns an error status.

### UpdateMultipleRoutesAsync
```csharp
public static async Task UpdateMultipleRoutesAsync(
    IEnumerable<RouteDefinition> routeDefinitions,
    IGrpcGatewayClient gatewayClient,
    CancellationToken cancellationToken = default)
```
Updates existing routes on the gateway.  
- **routeDefinitions** – collection of routes with updated settings; must not be null nor contain null elements.  
- **gatewayClient** – client used for the administration calls; must not be null.  
- **cancellationToken** – optional cancellation token.  

Returns a `Task` that completes when all update requests have been processed.  
Throws the same exceptions as `CreateMultipleRoutesAsync`.

### DeleteMultipleRoutesAsync
```csharp
public static async Task DeleteMultipleRoutesAsync(
    IEnumerable<int> routeIds,
    IGrpcGatewayClient gatewayClient,
    CancellationToken cancellationToken = default)
```
Deletes routes identified by their IDs.  
- **routeIds** – sequence of route identifiers to delete; must not be null nor contain negative values.  
- **gatewayClient** – administration client; must not be null.  
- **cancellationToken** – optional cancellation token.  

Returns a `Task` completing after all delete requests have been handled.  
Throws `ArgumentNullException` for null arguments, `ArgumentException` for invalid IDs, `OperationCanceledException` on cancellation, and `RpcException` for gateway failures.

### AnalyzeRoutePerformanceAsync
```csharp
public static async Task<Dictionary<int, RoutePerformanceMetrics>> AnalyzeRoutePerformanceAsync(
    IEnumerable<int> routeIds,
    IGrpcGatewayClient gatewayClient,
    CancellationToken cancellationToken = default)
```
Retrieves performance metrics for the specified routes.  
- **routeIds** – IDs of the routes to query; must not be null.  
- **gatewayClient** – client used to query the gateway’s monitoring endpoint; must not be null.  
- **cancellationToken** – optional cancellation token.  

Returns a dictionary mapping each route ID to a `RoutePerformanceMetrics` instance containing latency, error rate, and other stats.  
Throws `ArgumentNullException` for null inputs, `OperationCanceledException` if cancelled, and `RpcException` for communication errors.

### GetGatewayUrl
```csharp
public static string GetGatewayUrl(IGrpcGatewayClient gatewayClient)
```
Extracts the base URL from a gateway client instance.  
- **gatewayClient** – the client whose underlying channel address is required; must not be null.  

Returns the URL as a string (e.g., `"https://gateway.example.com"`).  
Throws `ArgumentNullException` if `gatewayClient` is null.

### CreateRouteWithMetadataAsync
```csharp
public static async Task CreateRouteWithMetadataAsync(
    RouteDefinition routeDefinition,
    IDictionary<string, string> metadata,
    IGrpcGatewayClient gatewayClient,
    CancellationToken cancellationToken = default)
```
Creates a single route and attaches arbitrary metadata key/value pairs.  
- **routeDefinition** – description of the route to create; must not be null.  
- **metadata** – dictionary of metadata to associate with the route; may be null or empty.  
- **gatewayClient** – administration client; must not be null.  
- **cancellationToken** – optional cancellation token.  

Returns a `Task` completing when the create request succeeds.  
Throws `ArgumentNullException` for null `routeDefinition` or `gatewayClient`, `OperationCanceledException` on cancellation, and `RpcException` for gateway errors.

### RequestCount
```csharp
public int RequestCount { get; }
```
Gets the total number of requests processed by the configuration instance since its creation or last reset.  
No parameters; returns an `Int32`.  
Does not throw under normal operation.

### AverageLatencyMs
```csharp
public double AverageLatencyMs { get; }
```
Gets the average request latency in milliseconds observed by the instance.  
No parameters; returns a `Double` representing the mean latency.  
Does not throw.

### ErrorRate
```csharp
public double ErrorRate { get; }
```
Gets the proportion of requests that resulted in an error (value between 0.0 and 1.0).  
No parameters; returns a `Double`.  
Does not throw.

### CacheHitRate
```csharp
public double CacheHitRate { get; }
```
Gets the proportion of requests served from cache (value between 0.0 and 1.0).  
No parameters; returns a `Double`.  
Does not throw.

### MaxConcurrentRequests
```csharp
public int MaxConcurrentRequests { get; set; }
```
Gets or sets the maximum number of concurrent requests the instance will allow before applying back‑pressure or queuing.  
No parameters; returns an `Int32`.  
Setting the value to a negative number throws `ArgumentOutOfRangeException`. Changes take effect immediately for subsequent operations.

## Usage

### Example 1 – Bulk route creation
```csharp
using Grpc.Net.Client;
using DotnetGrpcGateway.Administration;
using DotnetGrpcGateway.Examples;

var channel = GrpcChannel.ForAddress("https://gateway.example.com");
var gatewayClient = new GrpcGatewayClient(channel);

var routes = new[]
{
    new RouteDefinition { Path = "/serviceA", Service = "ServiceA" },
    new RouteDefinition { Path = "/serviceB", Service = "ServiceB" }
};

await DynamicRouteConfigurationExampleExtensions.CreateMultipleRoutesAsync(
    routes,
    gatewayClient,
    CancellationToken.None);
```

### Example 2 – Monitoring and configuring a runtime instance
```csharp
var config = new DynamicRouteConfigurationExampleExtensions();
config.MaxConcurrentRequests = 20; // allow up to 20 parallel calls

// Simulate some work that updates the internal counters …
// (in a real scenario the instance would be updated by the gateway client)

Console.WriteLine($"Requests: {config.RequestCount}");
Console.WriteLine($"Avg latency: {config.AverageLatencyMs} ms");
Console.WriteLine($"Error rate: {config.ErrorRate:P2}");
Console.WriteLine($"Cache hit rate: {config.CacheHitRate:P2}");
```

## Notes
- All static extension methods are safe to call concurrently from multiple threads as long as the supplied `IGrpcGatewayClient` instance is thread‑safe (the generated gRPC client satisfies this requirement).  
- The instance properties (`RequestCount`, `AverageLatencyMs`, `ErrorRate`, `CacheHitRate`, `MaxConcurrentRequests`) are **not** synchronized; concurrent reads and writes from different threads may lead to stale or inconsistent values. If the instance is accessed from multiple threads, external synchronization (e.g., `lock` or `ConcurrentDictionary`) is required.  
- `MaxConcurrentRequests` should be configured before invoking any of the async methods that depend on throttling; changing it while operations are in flight only affects newly started operations.  
- Passing `null` for any required argument results in an `ArgumentNullException`; the methods do not attempt to derive default values.  
- Empty collections are permitted for the bulk methods; they complete immediately without making any gateway calls.  
- Cancellation tokens are honored; if cancellation is triggered before a request is sent, no network traffic is generated for that operation. If cancellation occurs after a request has been sent, the method will still await the server’s response unless the underlying channel aborts the call.  
- The `RoutePerformanceMetrics` type returned by `AnalyzeRoutePerformanceAsync` is assumed to contain fields such as `LatencyMs`, `ErrorRate`, and `CacheHitRate`; consumers should inspect those fields for detailed diagnostics.
