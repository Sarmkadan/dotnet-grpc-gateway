# GatewayBenchmarksExtensions

The `GatewayBenchmarksExtensions` class provides a suite of static utility methods designed for benchmarking and validating the performance characteristics of the `dotnet-grpc-gateway` infrastructure. These methods enable standardized testing of specific gateway operations—such as route matching, service registration, and caching—under both nominal and edge-case conditions, facilitating performance regression analysis and throughput profiling.

## API

### FindMatchingRoute_WithNoParams
Executes a performance benchmark for the route matching logic when no query parameters are present in the request.
- **Parameters:** None.
- **Return Value:** `void`.
- **Throws:** None.

### GetFromCache_WithLargeData
Performs an asynchronous benchmark measuring the latency and throughput of retrieving large data payloads from the gateway cache.
- **Parameters:** None.
- **Return Value:** `Task`. Represents the asynchronous benchmark operation.
- **Throws:** May throw `System.TimeoutException` or `System.IO.IOException` if the cache operation exceeds configured limits.

### GetNextEndpoint_WithEmptyList
Validates and benchmarks the endpoint selection algorithm when the collection of available endpoints is empty.
- **Parameters:** None.
- **Return Value:** `void`.
- **Throws:** May throw `System.InvalidOperationException` if the selection logic requires at least one available endpoint.

### RegisterService_WithValidData
Executes an asynchronous benchmark for registering a new service with valid service metadata and configuration data.
- **Parameters:** None.
- **Return Value:** `Task`. Represents the asynchronous benchmark operation.
- **Throws:** May throw `System.ArgumentNullException` if the internal registration data is not properly initialized.

## Usage

```csharp
// Example 1: Executing a synchronous benchmark
GatewayBenchmarksExtensions.FindMatchingRoute_WithNoParams();
GatewayBenchmarksExtensions.GetNextEndpoint_WithEmptyList();
```

```csharp
// Example 2: Executing asynchronous benchmarks
public async Task RunGatewayBenchmarks()
{
    await GatewayBenchmarksExtensions.GetFromCache_WithLargeData();
    await GatewayBenchmarksExtensions.RegisterService_WithValidData();
}
```

## Notes

- **Thread-Safety:** These methods are designed to be invoked within isolated benchmark harnesses. While individual methods are marked as `static`, their thread-safety depends on the underlying gateway state. Concurrent execution of these methods against the same gateway instance may lead to non-deterministic performance results or race conditions.
- **Edge Cases:** Methods targeting empty collections (e.g., `GetNextEndpoint_WithEmptyList`) assume the caller intends to measure the system's robustness against empty state configurations; ensure the test environment is pre-configured to handle such states without unexpected application crashes.
- **Asynchronous Operations:** The `async` methods (`GetFromCache_WithLargeData`, `RegisterService_WithValidData`) do not enforce specific timeout policies by default. Implementers should ensure that the calling test harness applies appropriate timeout constraints to prevent deadlocks during benchmark execution.
