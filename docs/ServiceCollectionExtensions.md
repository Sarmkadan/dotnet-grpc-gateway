# ServiceCollectionExtensions

Provides extension methods for `IServiceCollection` to register gateway infrastructure services, configuration bindings, health checks, and gRPC server reflection in a `dotnet-grpc-gateway` application. Also exposes concrete health check types used internally by the gateway to verify connectivity and runtime status of downstream components.

## API

### `AddGatewayServices`
```csharp
public static IServiceCollection AddGatewayServices(this IServiceCollection services)
```
Registers the core gateway services required for request proxying, routing, and middleware execution. This includes transient and singleton dependencies that form the backbone of the gateway pipeline.

- **Parameters:** `services` — the `IServiceCollection` to augment.
- **Returns:** The same `IServiceCollection` for chaining.
- **Throws:** `ArgumentNullException` if `services` is `null`.

### `AddGatewayConfiguration`
```csharp
public static IServiceCollection AddGatewayConfiguration(this IServiceCollection services, IConfiguration configuration)
```
Binds the gateway configuration section from application settings and registers it into the DI container. The configuration object is typically consumed by other gateway services to determine upstream endpoints, timeouts, and routing rules.

- **Parameters:**
  - `services` — the `IServiceCollection` to augment.
  - `configuration` — an `IConfiguration` instance (usually from `WebApplicationBuilder.Configuration`).
- **Returns:** The same `IServiceCollection` for chaining.
- **Throws:** `ArgumentNullException` if either argument is `null`.

### `AddGatewayHealthChecks`
```csharp
public static IServiceCollection AddGatewayHealthChecks(this IServiceCollection services)
```
Adds the standard set of gateway health checks to the service collection, including checks for gateway internal state, gRPC reflection availability, and service discovery connectivity. These are intended to be used with ASP.NET Core’s health check middleware.

- **Parameters:** `services` — the `IServiceCollection` to augment.
- **Returns:** The same `IServiceCollection` for chaining.
- **Throws:** `ArgumentNullException` if `services` is `null`.

### `AddGatewayReflection`
```csharp
public static IServiceCollection AddGatewayReflection(this IServiceCollection services)
```
Registers gRPC server reflection services, enabling clients to discover available gRPC services and their methods at runtime. This is typically used for debugging and tooling support.

- **Parameters:** `services` — the `IServiceCollection` to augment.
- **Returns:** The same `IServiceCollection` for chaining.
- **Throws:** `ArgumentNullException` if `services` is `null`.

### `GatewayHealthCheck`
```csharp
public class GatewayHealthCheck : IHealthCheck
```
A health check that evaluates the gateway’s own operational status, such as whether the core pipeline is initialized and no critical internal faults are present.

#### `CheckHealthAsync`
```csharp
public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
```
Performs the gateway self-check asynchronously.

- **Parameters:**
  - `context` — the `HealthCheckContext` provided by the health check framework.
  - `cancellationToken` — a `CancellationToken` that can abort the check.
- **Returns:** `HealthCheckResult.Healthy()` when the gateway is operational; `HealthCheckResult.Unhealthy()` with a description when a critical fault is detected.
- **Throws:** No exceptions are thrown; all failures are captured in the returned `HealthCheckResult`.

### `ReflectionHealthCheck`
```csharp
public class ReflectionHealthCheck : IHealthCheck
```
A health check that verifies whether the gRPC server reflection service is reachable and responding correctly.

#### `CheckHealthAsync`
```csharp
public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
```
Probes the gRPC reflection endpoint asynchronously.

- **Parameters:**
  - `context` — the `HealthCheckContext` provided by the health check framework.
  - `cancellationToken` — a `CancellationToken` that can abort the probe.
- **Returns:** `HealthCheckResult.Healthy()` when reflection responds successfully; `HealthCheckResult.Unhealthy()` or `HealthCheckResult.Degraded()` on timeout, connection refusal, or invalid response.
- **Throws:** No exceptions are thrown; all failures are captured in the returned `HealthCheckResult`.

### `ServiceDiscoveryHealthCheck`
```csharp
public class ServiceDiscoveryHealthCheck : IHealthCheck
```
A health check that confirms the service discovery mechanism (e.g., Consul, DNS, or custom registry) is reachable and returning valid endpoint data.

#### `CheckHealthAsync`
```csharp
public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
```
Queries the service discovery backend asynchronously.

- **Parameters:**
  - `context` — the `HealthCheckContext` provided by the health check framework.
  - `cancellationToken` — a `CancellationToken` that can abort the query.
- **Returns:** `HealthCheckResult.Healthy()` when the discovery backend responds with valid data; `HealthCheckResult.Unhealthy()` or `HealthCheckResult.Degraded()` on connectivity failure, empty results, or stale data.
- **Throws:** No exceptions are thrown; all failures are captured in the returned `HealthCheckResult`.

## Usage

### Example 1: Minimal Gateway Startup
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddGatewayConfiguration(builder.Configuration)
    .AddGatewayServices()
    .AddGatewayHealthChecks()
    .AddGatewayReflection();

var app = builder.Build();

app.MapHealthChecks("/healthz");
app.MapGrpcReflectionService();

app.Run();
```

### Example 2: Custom Health Check Pipeline with Selective Checks
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddGatewayConfiguration(builder.Configuration)
    .AddGatewayServices();

// Register only specific health checks manually
builder.Services
    .AddHealthChecks()
    .AddCheck<GatewayHealthCheck>("gateway_self")
    .AddCheck<ServiceDiscoveryHealthCheck>("service_discovery");

var app = builder.Build();

app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    Predicate = check => check.Name.StartsWith("gateway") || check.Name == "service_discovery"
});

app.Run();
```

## Notes

- All extension methods return the same `IServiceCollection` instance, enabling fluent chaining. They do not modify any external state.
- `AddGatewayConfiguration` must be called before other gateway registration methods if those services depend on the bound configuration object. Failure to do so may result in runtime resolution errors.
- The health check classes (`GatewayHealthCheck`, `ReflectionHealthCheck`, `ServiceDiscoveryHealthCheck`) implement `IHealthCheck` and are designed to be registered either through `AddGatewayHealthChecks` or individually via `AddCheck<T>`.
- `CheckHealthAsync` implementations are expected to handle their own exceptions internally and never propagate them to the health check framework. Unhandled exceptions from these methods would be caught by the framework and reported as `HealthCheckResult.Unhealthy()`.
- Thread safety: The extension methods are safe to call during application startup on a single thread. The health check `CheckHealthAsync` methods may be invoked concurrently by the health check middleware; implementations must be thread-safe (e.g., using transient dependencies or synchronization where necessary).
- Cancellation tokens passed to `CheckHealthAsync` should be honored to avoid blocking health check endpoints indefinitely. Long-running checks should periodically test `cancellationToken.IsCancellationRequested`.
