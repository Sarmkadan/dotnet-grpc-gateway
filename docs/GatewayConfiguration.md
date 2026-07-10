# GatewayConfiguration

`GatewayConfiguration` is a data-transfer object used to configure gRPC Gateway instances, specifying network endpoints, performance constraints, security settings, and operational features for proxying gRPC services to HTTP/JSON clients.

## API

### `public int Id`
A unique identifier for the gateway configuration. Used for persistence, logging, and administrative operations. Must be positive.

### `public string Name`
Human-readable name of the gateway configuration. Used in logs, metrics, and administrative UIs. Must not be null or empty.

### `public string Description`
Optional human-readable description of the gateway’s purpose or scope. May be null or empty.

### `public string ListenAddress`
The network address on which the gateway listens for incoming connections (e.g., `0.0.0.0`, `127.0.0.1`, or a specific IP). Must be a valid IP address or hostname. Defaults to `0.0.0.0`.

### `public int Port`
The TCP port number on which the gateway listens. Must be between 1 and 65535. Conflicts on the same host/address are detected at runtime.

### `public bool EnableReflection`
Enables or disables gRPC reflection service on the gateway. When enabled, clients can dynamically discover available services and methods. Defaults to `false`.

### `public bool EnableMetrics`
Enables or disables collection and exposure of Prometheus-compatible metrics (e.g., request counts, latencies). Defaults to `false`.

### `public bool EnableWebSocketSupport`
Enables or disables WebSocket transport for gRPC connections. Useful for browser-based clients or real-time streaming. Defaults to `false`.

### `public int MaxConcurrentConnections`
Maximum number of concurrent client connections the gateway will accept. Must be positive. Exceeding this limit results in connection refusals. Defaults to 1024.

### `public int RequestTimeoutMs`
Timeout in milliseconds for individual client requests. Must be non-negative. A value of 0 disables timeout. Defaults to 30000 (30 seconds).

### `public int MaxMessageSize`
Maximum size in bytes for incoming and outgoing messages. Must be positive. Messages exceeding this size are rejected. Defaults to 4194304 (4 MiB).

### `public bool EnableCorsPolicy`
Enables or disables CORS (Cross-Origin Resource Sharing) policy for HTTP endpoints. When enabled, `CorsOrigins` must be configured. Defaults to `false`.

### `public string CorsOrigins`
Comma-separated list of allowed origins for CORS requests (e.g., `https://example.com,http://localhost:8080`). Used only when `EnableCorsPolicy` is `true`. Must not be null if policy is enabled.

### `public bool EnableCompressionByDefault`
Enables or disables automatic compression of responses using the algorithm specified in `CompressionAlgorithm`. Defaults to `false`.

### `public string CompressionAlgorithm`
Name of the compression algorithm to use when `EnableCompressionByDefault` is `true` (e.g., `gzip`, `deflate`). Must be a supported algorithm. Defaults to `gzip`.

### `public bool ValidateSslCertificates`
Enables or disables server-side SSL/TLS certificate validation for downstream gRPC services. When `false`, insecure connections are allowed. Defaults to `true`.

### `public string LogLevel`
Minimum severity level for log output (e.g., `Debug`, `Info`, `Warning`, `Error`, `Critical`). Must be a valid log level. Defaults to `Info`.

### `public DateTime CreatedAt`
Timestamp indicating when the configuration was created. Set automatically on creation; do not modify.

### `public DateTime ModifiedAt`
Timestamp indicating when the configuration was last modified. Updated automatically on property changes; do not modify.

### `public bool IsActive`
Indicates whether the gateway is currently active and accepting connections. Used for activation/deactivation without deleting the configuration.

## Usage

### Example 1: Basic Configuration
