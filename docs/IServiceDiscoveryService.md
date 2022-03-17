# IServiceDiscoveryService

`IServiceDiscoveryService` is the central abstraction for monitoring and querying the health and availability of gRPC services within a `dotnet-grpc-gateway` deployment. It provides methods to perform on-demand health checks, retrieve cached health reports, update service health status programmatically, discover currently available services, and obtain a consolidated view of all service health states.

## API

### `ServiceDiscoveryService`

The concrete implementation type that realizes the `IServiceDiscoveryService` contract. It encapsulates the logic for health probing, status caching, and service enumeration.

### `async Task<ServiceHealthReport> PerformHealthCheckAsync`

Executes an immediate, on-demand health check against a target service and returns a detailed report.

- **Purpose:** To synchronously verify the health of a specific service at the moment of invocation, bypassing any cached state.
- **Parameters:** Implicitly requires identification of the target service (typically by name or endpoint, as configured in the implementation).
- **Return Value:** A `ServiceHealthReport` containing the outcome of the check, including status, latency, and any diagnostic information.
- **Throws:** May throw `TimeoutException` if the health probe exceeds its configured deadline, or `InvalidOperationException` if the service identity cannot be resolved.

### `async Task<ServiceHealthReport> GetLatestHealthReportAsync`

Retrieves the most recently stored health report for a service without triggering a new probe.

- **Purpose:** To access the last known health state efficiently, suitable for dashboards or lightweight status queries where real-time accuracy is not critical.
- **Parameters:** Implicitly requires identification of the target service.
- **Return Value:** The cached `ServiceHealthReport` instance. If no prior report exists, the returned report will indicate an unknown or default state.
- **Throws:** Typically does not throw; returns a neutral report when no data is available.

### `async Task UpdateServiceHealthAsync`

Programmatically updates the stored health status of a service, bypassing the normal probing mechanism.

- **Purpose:** Allows external systems or manual processes to inject a health status, such as during maintenance windows or when integrating with external monitoring tools.
- **Parameters:** Accepts a service identifier and the new `ServiceHealthStatus` (or a full report) to be recorded.
- **Return Value:** A `Task` that completes when the update has been persisted.
- **Throws:** May throw `ArgumentException` if the provided service identifier is invalid or unrecognized.

### `async Task<List<GrpcService>> DiscoverAvailableServicesAsync`

Enumerates all gRPC services currently registered and considered available for routing.

- **Purpose:** Provides dynamic service discovery for clients or middleware that need an up-to-date list of reachable endpoints.
- **Parameters:** None.
- **Return Value:** A `List<GrpcService>` where each entry describes a service’s identity, address, and metadata.
- **Throws:** May throw `TimeoutException` if the discovery backend (e.g., a registry) is unreachable.

### `async Task<Dictionary<int, ServiceHealthStatus>> GetAllServicesHealthAsync`

Returns a dictionary mapping service identifiers to their current health status.

- **Purpose:** Offers a compact, aggregated snapshot of the entire service mesh’s health for monitoring or decision-making logic.
- **Parameters:** None.
- **Return Value:** A `Dictionary<int, ServiceHealthStatus>` keyed by service ID. Services with no recorded status are omitted or reported with a default value.
- **Throws:** May throw `InvalidOperationException` if the internal health store is corrupted or unavailable.

## Usage

### Example 1: Performing a Health Check and Handling a Missing Report

```csharp
IServiceDiscoveryService discovery = new ServiceDiscoveryService(/* dependencies */);

try
{
    ServiceHealthReport report = await discovery.PerformHealthCheckAsync();
    Console.WriteLine($"Service status: {report.Status}, Latency: {report.LatencyMs}ms");
}
catch (TimeoutException)
{
    Console.WriteLine("Health check timed out; service may be unresponsive.");
}
```

### Example 2: Aggregating All Service Health States for a Dashboard

```csharp
IServiceDiscoveryService discovery = new ServiceDiscoveryService(/* dependencies */);

Dictionary<int, ServiceHealthStatus> allHealth = await discovery.GetAllServicesHealthAsync();

foreach (var kvp in allHealth)
{
    Console.WriteLine($"Service ID {kvp.Key}: {kvp.Value}");
}

// Optionally refresh a specific service's status proactively
await discovery.UpdateServiceHealthAsync(serviceId: 42, newStatus: ServiceHealthStatus.Healthy);
```

## Notes

- **Caching and Staleness:** `GetLatestHealthReportAsync` returns the last stored report, which may be stale if no recent `PerformHealthCheckAsync` or `UpdateServiceHealthAsync` call has occurred. Use `PerformHealthCheckAsync` when a guaranteed fresh result is required.
- **Thread Safety:** Implementations of `IServiceDiscoveryService` are expected to be thread-safe. Concurrent calls to `UpdateServiceHealthAsync` and `GetAllServicesHealthAsync` must not corrupt the internal health store or produce partial updates.
- **Empty Results:** `GetAllServicesHealthAsync` may return an empty dictionary if no services have been registered or probed. Callers should guard against empty collections.
- **Discovery Consistency:** The list returned by `DiscoverAvailableServicesAsync` reflects the state at the time of the call; it does not automatically update if services register or deregister afterward. Repeated calls are necessary for long-lived processes.
- **Error Propagation:** Network-related failures (timeouts, connection refused) surface as exceptions rather than error-typed return values. Wrap calls in try-catch blocks where resilience is required.
