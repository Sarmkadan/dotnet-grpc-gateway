# GatewayController

Provides HTTP endpoints for managing and monitoring a gRPC Gateway instance. Exposes configuration, service registration, routing, and metrics endpoints to allow external clients to inspect and control gateway behavior at runtime.

## API

### `GatewayController()`

Initializes a new instance of the `GatewayController` with default dependencies.

### `Task<ActionResult<GatewayConfiguration>> GetConfiguration()`

Retrieves the current gateway configuration.

- **Returns**: The active `GatewayConfiguration`.
- **Throws**: If the configuration cannot be read (e.g., due to I/O or serialization errors).

### `Task<ActionResult<GatewayConfiguration>> UpdateConfiguration()`

Updates the gateway configuration with the provided settings.

- **Returns**: The updated `GatewayConfiguration`.
- **Throws**: If the configuration is invalid, cannot be persisted, or triggers an internal restart.

### `Task<ActionResult<List<GrpcService>>> GetServices()`

Lists all registered gRPC services known to the gateway.

- **Returns**: A list of `GrpcService` objects describing each service.
- **Throws**: If service discovery fails or the registry is unavailable.

### `Task<ActionResult<List<GrpcService>>> GetHealthyServices()`

Lists all registered gRPC services that are currently healthy and accepting traffic.

- **Returns**: A list of healthy `GrpcService` objects.
- **Throws**: If health checks cannot be performed or the registry is unavailable.

### `Task<ActionResult<GrpcService>> GetService(string serviceName)`

Retrieves a specific registered gRPC service by name.

- **Parameters**:
  - `serviceName`: The name of the service to retrieve.
- **Returns**: The `GrpcService` matching the given name.
- **Throws**: `NotFound` if the service does not exist; otherwise, if the registry is unavailable.

### `Task<ActionResult> RegisterService(GrpcServiceRegistration registration)`

Registers a new gRPC service with the gateway.

- **Parameters**:
  - `registration`: The service metadata and endpoint to register.
- **Returns**: `NoContent` on success.
- **Throws**: `BadRequest` if the registration is invalid; otherwise, if registration fails due to conflicts or I/O errors.

### `Task<ActionResult> UnregisterService(string serviceName)`

Removes a previously registered gRPC service from the gateway.

- **Parameters**:
  - `serviceName`: The name of the service to remove.
- **Returns**: `NoContent` on success.
- **Throws**: `NotFound` if the service does not exist; otherwise, if unregistration fails.

### `Task<ActionResult<List<GatewayRoute>>> GetRoutes()`

Lists all configured routes managed by the gateway.

- **Returns**: A list of `GatewayRoute` objects.
- **Throws**: If the route store is unavailable or corrupted.

### `Task<ActionResult> AddRoute(GatewayRoute route)`

Adds a new route to the gateway configuration.

- **Parameters**:
  - `route`: The route definition to add.
- **Returns**: `NoContent` on success.
- **Throws**: `BadRequest` if the route is invalid; otherwise, if persistence fails.

### `Task<ActionResult> RemoveRoute(string routeName)`

Removes an existing route from the gateway configuration.

- **Parameters**:
  - `routeName`: The name of the route to remove.
- **Returns**: `NoContent` on success.
- **Throws**: `NotFound` if the route does not exist; otherwise, if removal fails.

### `Task<ActionResult<GatewayStatistics>> GetTodayStatistics()`

Retrieves aggregated statistics for the current day.

- **Returns**: A `GatewayStatistics` object summarizing today’s traffic.
- **Throws**: If metrics storage is unavailable or corrupted.

### `Task<ActionResult<GatewayStatistics>> GetStatistics()`

Retrieves aggregated statistics across all tracked time.

- **Returns**: A `GatewayStatistics` object summarizing overall traffic.
- **Throws**: If metrics storage is unavailable or corrupted.

### `Task<ActionResult<List<RequestMetric>>> GetSlowRequests(int thresholdMs)`

Lists request metrics that exceeded the specified response time threshold.

- **Parameters**:
  - `thresholdMs`: The minimum response time (in milliseconds) to consider.
- **Returns**: A list of `RequestMetric` objects exceeding the threshold.
- **Throws**: If metrics storage is unavailable or corrupted.

### `Task<ActionResult> GetAverageResponseTime()`

Calculates and returns the average response time across all requests.

- **Returns**: The average response time in milliseconds.
- **Throws**: If metrics storage is unavailable or no requests have been recorded.

## Usage
