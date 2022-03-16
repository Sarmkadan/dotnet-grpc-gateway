# RequestMetric

`RequestMetric` is a data transfer object used to capture and record telemetry about individual gRPC gateway requests. It contains metadata about the request, response, performance characteristics, and error conditions to enable observability, diagnostics, and performance analysis in distributed systems.

## API

### `public int Id`
Unique identifier assigned to this metric record. Used to correlate metrics with other telemetry sources.

### `public string RequestId`
Correlation identifier propagated from the incoming request. Enables tracing a single logical operation across service boundaries.

### `public string ServiceName`
Name of the gRPC service handling the request (e.g., `OrderService`, `UserService`). Used for service-level aggregation and filtering.

### `public string MethodName`
Name of the gRPC method invoked (e.g., `GetOrder`, `CreateUser`). Enables method-level performance and error analysis.

### `public string ClientIpAddress`
IP address of the client initiating the request. Used for geolocation, rate limiting, and access pattern analysis.

### `public int RouteId`
Identifier of the configured route that matched the request. Useful for analyzing routing behavior and performance per route.

### `public long RequestSizeBytes`
Size of the request payload in bytes, including headers and body. Used to monitor payload sizes and detect anomalies.

### `public long ResponseSizeBytes`
Size of the response payload in bytes, including headers and body. Used to analyze response payloads and bandwidth usage.

### `public double DurationMs`
Total duration of the request in milliseconds, measured from receipt to completion. Includes processing time and network latency. Used for performance monitoring and SLA validation.

### `public int HttpStatusCode`
HTTP status code returned to the client (e.g., 200, 404, 500). Indicates the outcome of the gateway’s handling of the request.

### `public string? GrpcStatusCode`
gRPC status code returned by the service (e.g., `OK`, `NotFound`, `Internal`). Indicates the outcome of the service’s execution. `null` if the request did not reach the service.

### `public bool IsSuccessful`
Indicates whether the request was processed successfully. `true` if the HTTP status code is in the 2xx range and no errors occurred. Used for success-rate monitoring and alerting.

### `public string? ErrorMessage`
Human-readable error message if the request failed. `null` if the request succeeded. Used for diagnostics and user support.

### `public string? StackTrace`
Stack trace of the exception thrown during processing, if any. `null` if the request succeeded or no exception was captured. Used for debugging and root-cause analysis.

### `public Dictionary<string, string> RequestHeaders`
Collection of HTTP headers included in the incoming request. Keys are header names, values are header values. Used for auditing and header-based routing decisions.

### `public Dictionary<string, string> ResponseHeaders`
Collection of HTTP headers included in the outgoing response. Keys are header names, values are header values. Used for auditing and response metadata analysis.

### `public string? CacheHitStatus`
Indicates whether the response was served from cache and the cache status (e.g., `HIT`, `MISS`, `BYPASS`). `null` if caching was not applied. Used to analyze cache effectiveness.

### `public bool WasRetried`
Indicates whether the request was retried due to transient failures. Used to assess reliability and retry policy effectiveness.

### `public int RetryCount`
Number of retry attempts made for this request. Zero if no retries occurred. Used to analyze retry behavior and failure modes.

### `public DateTime RecordedAt`
Timestamp when the metric was recorded, in UTC. Used for time-based analysis and correlation with other telemetry.

## Usage

### Recording a successful request
