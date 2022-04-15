# HealthChecksAndMonitoringExampleExtensions

Extension methods and helper utilities for performing health checks and monitoring on gRPC gateway services. Provides synchronous and asynchronous methods to assess service health, generate reports, and track key metrics such as uptime, failure thresholds, and cache performance.

## API

### `CheckMultipleServicesHealthAsync`

Asynchronously checks the health status of multiple services and returns a dictionary mapping service IDs to their health status.

- **Parameters**:
  - `serviceIds` (`IEnumerable<int>`): Collection of service identifiers to check.
  - `httpClient` (`HttpClient`): HTTP client configured for service communication.
  - `cancellationToken` (`CancellationToken`, optional): Token to monitor for cancellation requests.
- **Returns**: `Task<Dictionary<int, bool>>` – A dictionary where keys are service IDs and values indicate whether each service is healthy (`true`) or unhealthy (`false`).
- **Exceptions**: Throws `ArgumentNullException` if `serviceIds` or `httpClient` is `null`.

---

### `GetHealthReportAsync`

Asynchronously generates a comprehensive health report string summarizing the status of all monitored services.

- **Parameters**:
  - `httpClient` (`HttpClient`): HTTP client configured for service communication.
  - `cancellationToken` (`CancellationToken`, optional): Token to monitor for cancellation requests.
- **Returns**: `Task<string>` – A formatted string containing health metrics, uptime, failure counts, and overall system status.
- **Exceptions**: Throws `ArgumentNullException` if `httpClient` is `null`.

---

### `AreAllServicesHealthyAsync`

Asynchronously checks whether all monitored services are currently healthy.

- **Parameters**:
  - `httpClient` (`HttpClient`): HTTP client configured for service communication.
  - `cancellationToken` (`CancellationToken`, optional): Token to monitor for cancellation requests.
- **Returns**: `Task<bool>>` – `true` if all services are healthy; otherwise, `false`.
- **Exceptions**: Throws `ArgumentNullException` if `httpClient` is `null`.

---

### `GetDetailedHealthStatusAsync`

Asynchronously retrieves a detailed `HealthStatus` object representing the overall health of the service ecosystem.

- **Parameters**:
  - `httpClient` (`HttpClient`): HTTP client configured for service communication.
  - `cancellationToken` (`CancellationToken`, optional): Token to monitor for cancellation requests.
- **Returns**: `Task<HealthStatus>` – An object containing aggregated health metrics, including service counts, failure rates, and response times.
- **Exceptions**: Throws `ArgumentNullException` if `httpClient` is `null`.

---
### `CheckServiceHealthAsync`

Asynchronously performs a health check on a single service identified by its ID.

- **Parameters**:
  - `serviceId` (`int`): The unique identifier of the service to check.
  - `httpClient` (`HttpClient`): HTTP client configured for service communication.
  - `cancellationToken` (`CancellationToken`, optional): Token to monitor for cancellation requests.
- **Returns**: `Task<bool>>` – `true` if the service is healthy; otherwise, `false`.
- **Exceptions**:
  - Throws `ArgumentNullException` if `httpClient` is `null`.
  - Throws `ArgumentOutOfRangeException` if `serviceId` is negative.

---
### `GetHttpClient`

Creates and configures a new `HttpClient` instance with default settings suitable for health check operations.

- **Parameters**: None.
- **Returns**: `HttpClient` – A configured `HttpClient` instance with base address and timeout settings optimized for health monitoring.
- **Exceptions**: None.

---
### `Status`

Gets the current health status string of the service (e.g., "Healthy", "Degraded", "Unhealthy").

- **Type**: `string?`
- **Access**: Read-only property.
- **Exceptions**: None.

---
### `Uptime`

Gets the current uptime duration of the service as a human-readable string (e.g., "2h 30m 15s").

- **Type**: `string?`
- **Access**: Read-only property.
- **Exceptions**: None.

---
### `RequestsProcessed`

Gets the total number of requests processed by the service since startup.

- **Type**: `long`
- **Access**: Read-only property.
- **Exceptions**: None.

---
### `ActiveConnections`

Gets the current number of active client connections to the service.

- **Type**: `int`
- **Access**: Read-only property.
- **Exceptions**: None.

---
### `CacheHitRate`

Gets the current cache hit rate as a value between 0.0 and 1.0.

- **Type**: `double`
- **Access**: Read-only property.
- **Exceptions**: None.

---
### `ServiceId`

Gets the unique identifier of the service.

- **Type**: `int`
- **Access**: Read-only property.
- **Exceptions**: None.

---
### `ServiceName`

Gets the human-readable name of the service.

- **Type**: `string?`
- **Access**: Read-only property.
- **Exceptions**: None.

---
### `IsHealthy`

Gets a value indicating whether the service is currently considered healthy.

- **Type**: `bool`
- **Access**: Read-only property.
- **Exceptions**: None.

---
### `ResponseTimeMs`

Gets the average response time of the service in milliseconds.

- **Type**: `int`
- **Access**: Read-only property.
- **Exceptions**: None.

---
### `LastCheckAt`

Gets the timestamp of the last health check in ISO 8601 format.

- **Type**: `string?`
- **Access**: Read-only property.
- **Exceptions**: None.

---
### `ConsecutiveFailures`

Gets the number of consecutive health check failures observed.

- **Type**: `int`
- **Access**: Read-only property.
- **Exceptions**: None.

---
### `FailureThreshold`

Gets the maximum allowed number of consecutive failures before the service is marked as unhealthy.

- **Type**: `int`
- **Access**: Read-only property.
- **Exceptions**: None.

## Usage
