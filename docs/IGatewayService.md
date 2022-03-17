# IGatewayService

The `IGatewayService` interface defines the contract for managing gateway configuration, services, and routing in a gRPC gateway system. It provides asynchronous methods to inspect and modify the gateway's operational state, including service registration, route management, and health monitoring.

## API

### `Task<GatewayConfiguration> GetConfigurationAsync()`

Retrieves the current gateway configuration. The configuration includes settings for registered services, routes, and other operational parameters.

- **Return value**: A `Task<GatewayConfiguration>` that resolves to the current gateway configuration.
- **Exceptions**: May throw if the configuration cannot be read from persistent storage or if the gateway is in an inconsistent state.

---

### `Task<GatewayConfiguration> UpdateConfigurationAsync(GatewayConfiguration configuration)`

Updates the gateway configuration with the provided settings. The new configuration is persisted and applied immediately.

- **Parameters**:
  - `configuration`: The new `GatewayConfiguration` to apply.
- **Return value**: A `Task<GatewayConfiguration>` that resolves to the updated configuration after persistence.
- **Exceptions**:
  - Throws if the configuration is invalid (e.g., malformed routes or services).
  - Throws if persistence fails (e.g., disk write errors).

---

### `Task RegisterServiceAsync(GrpcService service)`

Registers a gRPC service with the gateway. The service becomes available for routing after successful registration.

- **Parameters**:
  - `service`: The `GrpcService` to register, including its address and metadata.
- **Return value**: A `Task` that completes when the service is registered.
- **Exceptions**:
  - Throws if the service address is unreachable or invalid.
  - Throws if the service is already registered.
  - Throws if registration fails due to system constraints (e.g., maximum service limit).

---

### `Task UnregisterServiceAsync(string serviceId)`

Unregisters a gRPC service from the gateway. The service is removed from routing and health checks.

- **Parameters**:
  - `serviceId`: The unique identifier of the service to unregister.
- **Return value**: A `Task` that completes when the service is unregistered.
- **Exceptions**:
  - Throws if the service does not exist.
  - Throws if unregistration fails (e.g., due to active connections).

---
### `Task<GrpcService> GetServiceAsync(string serviceId)`

Retrieves a registered gRPC service by its unique identifier.

- **Parameters**:
  - `serviceId`: The unique identifier of the service to retrieve.
- **Return value**: A `Task<GrpcService>` that resolves to the requested service, or `null` if not found.
- **Exceptions**: Throws if the service ID is malformed or if retrieval fails (e.g., storage corruption).

---
### `Task<List<GrpcService>> GetAllServicesAsync()`

Retrieves all registered gRPC services in the gateway.

- **Return value**: A `Task<List<GrpcService>>` containing all registered services. The list may be empty if no services are registered.
- **Exceptions**: Throws if retrieval fails (e.g., storage corruption).

---
### `Task<List<GrpcService>> GetHealthyServicesAsync()`

Retrieves all registered gRPC services that are currently healthy (i.e., responding to health checks).

- **Return value**: A `Task<List<GrpcService>>` containing healthy services. The list may be empty if no services are healthy.
- **Exceptions**: Throws if health checks cannot be performed (e.g., network failures).

---
### `Task<GatewayRoute> AddRouteAsync(GatewayRoute route)`

Adds a new route to the gateway configuration. The route defines how incoming requests are forwarded to registered services.

- **Parameters**:
  - `route`: The `GatewayRoute` to add, including path, service selection criteria, and other routing rules.
- **Return value**: A `Task<GatewayRoute>` that resolves to the added route after persistence.
- **Exceptions**:
  - Throws if the route is invalid (e.g., malformed path or missing service selector).
  - Throws if the route conflicts with an existing route.
  - Throws if persistence fails.

---
### `Task RemoveRouteAsync(string routeId)`

Removes a route from the gateway configuration. The route is no longer used for request forwarding.

- **Parameters**:
  - `routeId`: The unique identifier of the route to remove.
- **Return value**: A `Task` that completes when the route is removed.
- **Exceptions**:
  - Throws if the route does not exist.
  - Throws if removal fails (e.g., due to active usage).

---
### `Task<List<GatewayRoute>> GetAllRoutesAsync()`

Retrieves all routes defined in the gateway configuration.

- **Return value**: A `Task<List<GatewayRoute>>` containing all routes. The list may be empty if no routes are defined.
- **Exceptions**: Throws if retrieval fails (e.g., storage corruption).

## Usage

### Example 1: Registering a Service and Adding a Route
