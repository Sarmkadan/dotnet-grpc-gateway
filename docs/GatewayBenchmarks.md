# GatewayBenchmarks

The `GatewayBenchmarks` class provides a suite of performance testing operations tailored for the `dotnet-grpc-gateway` infrastructure. It allows developers to benchmark critical pathways, including routing resolution, load balancing, caching mechanisms, service registration, and health monitoring, within a controlled environment such as BenchmarkDotNet. These methods are designed to exercise the core logic of the gateway components in isolation, enabling the measurement of execution time and resource allocation under various scenarios.

## API

*   **`Setup()`**: Initializes the necessary internal state, dependencies, and mock data required to execute the benchmark operations. Must be invoked prior to running any other benchmark method.
*   **`FindMatchingRoute_ExactMatch()`**: Simulates a routing lookup scenario where an exact match is found for the provided request.
*   **`FindMatchingRoute_WildcardMatch()`**: Simulates a routing lookup scenario that resolves using wildcard pattern matching.
*   **`FindMatchingRoute_NoMatch()`**: Simulates a routing lookup scenario where no registered routes match the request, exercising the failure path.
*   **`FindMatchingRoute_MultipleMatches()`**: Simulates a routing lookup scenario where multiple routes match, exercising the disambiguation or selection logic.
*   **`GetNextEndpoint()`**: Measures the performance of the load balancing algorithm when selecting the next available endpoint from a pool.
*   **`RegisterEndpoint()`**: Measures the overhead of adding a new endpoint to the service registry.
*   **`UpdateEndpointHealth()`**: Measures the performance of updating the health status of a registered endpoint.
*   **`GetFromCache_Miss()`**: Simulates an asynchronous cache lookup scenario that results in a cache miss.
*   **`GetFromCache_Hit()`**: Simulates an asynchronous cache lookup scenario that results in a successful cache hit.
*   **`SetInCache()`**: Measures the asynchronous overhead of adding or updating an entry in the cache.
*   **`RemoveFromCache()`**: Measures the asynchronous overhead of invalidating or removing an entry from the cache.
*   **`CacheStatistics()`**: Retrieves statistical data regarding cache performance, such as hit/miss ratios, asynchronously.
*   **`RecordRequestDuration()`**: Simulates the overhead of recording and processing request duration telemetry data.
*   **`GetAllRoutes()`**: Asynchronously retrieves the complete collection of registered routes from the router.
*   **`RegisterService()`**: Asynchronously measures the performance of registering a new gRPC service definition.
*   **`GetAllServices()`**: Asynchronously retrieves the complete collection of registered services.
*   **`GetHealthyServices()`**: Asynchronously retrieves only the subset of services currently reported as healthy.
*   **`PerformHealthCheck()`**: Asynchronously executes a health check operation across all configured services and endpoints.
*   **`GetAllServicesHealth()`**: Asynchronously retrieves the aggregated health status for all registered services.

## Usage

**Example 1: Benchmarking Routing Logic**
```csharp
[MemoryDiagnoser]
public class RoutingBenchmarks
{
    private GatewayBenchmarks _benchmarks;

    [GlobalSetup]
    public void Setup()
    {
        _benchmarks = new GatewayBenchmarks();
        _benchmarks.Setup();
    }

    [Benchmark]
    public void ExactMatch() => _benchmarks.FindMatchingRoute_ExactMatch();
}
```

**Example 2: Benchmarking Caching Operations**
```csharp
public class CacheBenchmarks
{
    private GatewayBenchmarks _benchmarks;

    [GlobalSetup]
    public void Setup()
    {
        _benchmarks = new GatewayBenchmarks();
        _benchmarks.Setup();
    }

    [Benchmark]
    public async Task CacheHit() => await _benchmarks.GetFromCache_Hit();
}
```

## Notes

*   **Lifecycle:** The `Setup()` method must be called before executing any other method. Failure to do so will result in `NullReferenceException` or similar state-related errors as the internal dependencies will not be initialized.
*   **Thread Safety:** The methods within `GatewayBenchmarks` are not designed for concurrent multi-threaded execution within the same instance. When used in benchmark frameworks like BenchmarkDotNet, instances are typically handled in a controlled, single-threaded context per benchmark iteration.
*   **Purpose:** These methods are intended solely for performance testing and benchmarking. They should not be utilized as part of the production application code, as they may bypass necessary validation or side effects present in the actual `dotnet-grpc-gateway` implementation.
