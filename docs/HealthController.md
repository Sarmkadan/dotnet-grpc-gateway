# HealthController

The `HealthController` is an ASP.NET Core controller that provides health monitoring endpoints for a gRPC gateway service. It exposes endpoints to check the overall health status of the service, individual service components, and readiness/liveness probes commonly used in container orchestration systems.

## API

### `HealthController`
The controller class that provides health monitoring endpoints. Inherits from `ControllerBase` and is decorated with `[ApiController]` and `[Route]` attributes for RESTful routing.

### `public async Task<ActionResult<HealthStatus>> GetHealthStatus()`
Returns the overall health status of the service.

- **Returns**: `HealthStatus` containing aggregated health information.
- **Throws**: May throw if health checks fail or if the health check system is misconfigured.

### `public async Task<ActionResult<List<ServiceHealthInfo>>> GetServicesHealth()`
Returns a list of health information for all monitored services.

- **Returns**: A list of `ServiceHealthInfo` objects, each representing the health status of a service.
- **Throws**: May throw if health checks fail or if the health check system is misconfigured.

### `public async Task<ActionResult<ServiceHealthInfo>> GetServiceHealth(int serviceId)`
Returns the health information for a specific service identified by `serviceId`.

- **Parameters**:
  - `serviceId` (int): The unique identifier of the service to check.
- **Returns**: `ServiceHealthInfo` for the requested service.
- **Throws**: `NotFoundResult` if the service ID is invalid or not found. May throw if health checks fail.

### `public async Task<ActionResult> GetReadiness()`
Endpoint for readiness probes, indicating whether the service is ready to receive traffic.

- **Returns**:
  - `200 OK` if the service is ready.
  - `503 Service Unavailable` if the service is not ready.
- **Throws**: May throw if readiness checks fail.

### `public ActionResult GetLiveness()`
Endpoint for liveness probes, indicating whether the service is alive.

- **Returns**:
  - `200 OK` if the service is alive.
  - `503 Service Unavailable` if the service is not alive.
- **Throws**: May throw if liveness checks fail.

### `public bool IsHealthy`
Gets a value indicating whether the overall health status is healthy.

- **Returns**: `true` if the overall health status is healthy; otherwise, `false`.

### `public DateTime Timestamp`
Gets the timestamp of the last health check.

- **Returns**: The `DateTime` when the last health check was performed.

### `public int HealthyServices`
Gets the number of services currently marked as healthy.

- **Returns**: The count of healthy services.

### `public int TotalServices`
Gets the total number of services being monitored.

- **Returns**: The total count of services.

### `public string? Message`
Gets an optional message describing the health status.

- **Returns**: A message if available; otherwise, `null`.

### `public int ServiceId`
Gets the ID of the service associated with the current health check context.

- **Returns**: The service ID.

### `public string? ServiceName`
Gets the name of the service associated with the current health check context.

- **Returns**: The service name if available; otherwise, `null`.

### `public bool IsHealthy`
Gets a value indicating whether the service associated with the current health check context is healthy.

- **Returns**: `true` if the service is healthy; otherwise, `false`.

### `public DateTime LastCheckedAt`
Gets the timestamp of the last health check for the service.

- **Returns**: The `DateTime` when the last health check was performed for the service.

### `public int CheckCount`
Gets the total number of health checks performed for the service.

- **Returns**: The count of health checks performed.

### `public int FailureCount`
Gets the number of failed health checks for the service.

- **Returns**: The count of failed health checks.

### `public string? Message`
Gets an optional message describing the health status of the service.

- **Returns**: A message if available; otherwise, `null`.

## Usage

### Example 1: Checking Overall Health
