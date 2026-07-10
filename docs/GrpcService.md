# GrpcService

Represents a gRPC service registered in the `dotnet-grpc-gateway` system, providing metadata, health status, and operational metrics for routing and monitoring purposes.

## API

### `public int Id`
Unique identifier for the gRPC service within the gateway registry. Used for internal reference and lookup.

### `public string Name`
Human-readable name of the service, typically matching the protobuf service name or a configured alias. Used for display and identification in logs and dashboards.

### `public string ServiceFullName`
Fully qualified name of the service as defined in the protobuf file (e.g., `package.ServiceName`). Used for routing and service discovery.

### `public string Host`
Network hostname or IP address where the gRPC service is reachable. May be a DNS name resolvable within the cluster or environment.

### `public int Port`
Network port number on which the gRPC service listens. Must be a valid TCP port (1–65535).

### `public bool UseTls`
Indicates whether the service should be contacted using TLS/SSL encryption. When `true`, the gateway will establish a secure channel; when `false`, an insecure channel is used.

### `public string? Description`
Optional human-readable description of the service purpose, version, or environment. May be `null` if not provided.

### `public string? ProtoPackage`
Optional name of the protobuf package associated with this service. Used for schema validation and code generation. May be `null` if not applicable.

### `public int HealthCheckIntervalSeconds`
Interval in seconds between consecutive health checks performed by the gateway. Must be a positive integer.

### `public int MaxRetries`
Maximum number of retry attempts the gateway will make when a request fails due to transient errors (e.g., connection drops, timeouts). Must be a non-negative integer.

### `public bool IsHealthy`
Current health status of the service as determined by the last health check. `true` indicates the service is operational; `false` indicates failure or unreachable state.

### `public DateTime LastHealthCheckAt`
Timestamp of the most recent health check execution. Reflects when the gateway last attempted to verify service availability.

### `public string? LastHealthCheckError`
Error message from the last health check attempt, if any. Contains `null` if the last check succeeded or if no check has been performed yet.

### `public double AverageResponseTimeMs`
Exponential moving average of response times (in milliseconds) across successful requests to this service. Updated periodically based on recent performance.

### `public long TotalRequestsProcessed`
Total number of requests successfully processed by this service since registration. Does not include retries or failed requests.

### `public long FailedRequestsCount`
Total number of requests that failed (including retries) since registration. Used to calculate error rates and trigger alerts.

### `public DateTime RegisteredAt`
Timestamp when the service was first registered with the gateway. Immutable after creation.

### `public DateTime ModifiedAt`
Timestamp of the last update to any mutable property of this service. Updated whenever `Name`, `Host`, `Port`, `UseTls`, `Description`, or health-related fields are changed.

### `public bool IsActive`
Indicates whether the service is currently enabled for routing. When `false`, the gateway will not forward requests to this service, though health checks may still run.

### `public string GetEndpointUri()`
Constructs and returns the full gRPC endpoint URI for this service based on `Host`, `Port`, and `UseTls`.

- **Returns**: A string in the format `grpc[s]://host:port` (e.g., `grpc://myservice:50051` or `grpcs://secure-service:443`).
- **Throws**: `InvalidOperationException` if `Host` or `Port` is invalid (e.g., empty host or port out of range).

## Usage

### Example 1: Registering a new gRPC service
