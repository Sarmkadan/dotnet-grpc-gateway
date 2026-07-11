# DotnetGrpcGatewayOptionsExtensions

Extension methods for configuring `DotnetGrpcGatewayOptions` to customize the behavior of the gRPC gateway service, including server binding, health checks, metrics, logging, and connection management.

## API

### `UseLocalhost`
Configures the gateway to bind to the localhost interface (`127.0.0.1`).
- **Return value**: The configured `DotnetGrpcGatewayOptions` instance for method chaining.
- **Usage**: Call this method to restrict the gateway to local access only.

### `UseAllInterfaces`
Configures the gateway to bind to all available network interfaces.
- **Return value**: The configured `DotnetGrpcGatewayOptions` instance for method chaining.
- **Usage**: Call this method to expose the gateway on all network interfaces.

### `UseAddress(string address)`
Configures the gateway to bind to a specific IP address or hostname.
- **Parameters**:
  - `address` (string): The IP address or hostname to bind to.
- **Return value**: The configured `DotnetGrpcGatewayOptions` instance for method chaining.
- **Throws**: `ArgumentNullException` if `address` is `null` or empty.

### `UsePort(int port)`
Configures the gateway to listen on a specific port.
- **Parameters**:
  - `port` (int): The port number to bind to.
- **Return value**: The configured `DotnetGrpcGatewayOptions` instance for method chaining.
- **Throws**: `ArgumentOutOfRangeException` if `port` is outside the valid range (1–65535).

### `DisableReflection`
Disables the reflection service for the gateway.
- **Return value**: The configured `DotnetGrpcGatewayOptions` instance for method chaining.
- **Usage**: Call this method to disable reflection-based service discovery for security or performance reasons.

### `DisableMetrics`
Disables metrics collection and reporting for the gateway.
- **Return value**: The configured `DotnetGrpcGatewayOptions` instance for method chaining.
- **Usage**: Call this method to reduce overhead when metrics are not required.

### `ConfigureHealthCheck(Action<IHealthCheckBuilder> configure)`
Configures the health check behavior for the gateway.
- **Parameters**:
  - `configure` (Action<IHealthCheckBuilder>): A delegate to configure health check options.
- **Return value**: The configured `DotnetGrpcGatewayOptions` instance for method chaining.
- **Throws**: `ArgumentNullException` if `configure` is `null`.

### `ConfigureMetrics(Action<IMetricsBuilder> configure)`
Configures the metrics collection and reporting behavior for the gateway.
- **Parameters**:
  - `configure` (Action<IMetricsBuilder>): A delegate to configure metrics options.
- **Return value**: The configured `DotnetGrpcGatewayOptions` instance for method chaining.
- **Throws**: `ArgumentNullException` if `configure` is `null`.

### `ConfigureRequestLogging(Action<IRequestLoggingBuilder> configure)`
Configures the request logging behavior for the gateway.
- **Parameters**:
  - `configure` (Action<IRequestLoggingBuilder>): A delegate to configure request logging options.
- **Return value**: The configured `DotnetGrpcGatewayOptions` instance for method chaining.
- **Throws**: `ArgumentNullException` if `configure` is `null`.

### `SetLogLevel(LogLevel level)`
Sets the minimum log level for the gateway.
- **Parameters**:
  - `level` (LogLevel): The minimum log level (e.g., `LogLevel.Information`).
- **Return value**: The configured `DotnetGrpcGatewayOptions` instance for method chaining.
- **Throws**: `ArgumentOutOfRangeException` if `level` is not a valid `LogLevel` value.

### `DisableCompression`
Disables response compression for the gateway.
- **Return value**: The configured `DotnetGrpcGatewayOptions` instance for method chaining.
- **Usage**: Call this method to reduce CPU usage when compression is unnecessary or unwanted.

### `SetMaxConcurrentConnections(int maxConcurrentConnections)`
Sets the maximum number of concurrent connections the gateway will accept.
- **Parameters**:
  - `maxConcurrentConnections` (int): The maximum number of concurrent connections.
- **Return value**: The configured `DotnetGrpcGatewayOptions` instance for method chaining.
- **Throws**: `ArgumentOutOfRangeException` if `maxConcurrentConnections` is less than 1.

### `SetRequestTimeout(TimeSpan timeout)`
Sets the timeout for processing incoming requests.
- **Parameters**:
  - `timeout` (TimeSpan): The timeout duration.
- **Return value**: The configured `DotnetGrpcGatewayOptions` instance for method chaining.
- **Throws**: `ArgumentOutOfRangeException` if `timeout` is negative or zero.

### `SetHealthCheckFailureThreshold(int threshold)`
Sets the number of consecutive failures required to mark the health check as unhealthy.
- **Parameters**:
  - `threshold` (int): The failure threshold.
- **Return value**: The configured `DotnetGrpcGatewayOptions` instance for method chaining.
- **Throws**: `ArgumentOutOfRangeException` if `threshold` is less than 1.

### `SetHealthCheckTimeout(TimeSpan timeout)`
Sets the timeout for individual health check probes.
- **Parameters**:
  - `timeout` (TimeSpan): The timeout duration.
- **Return value**: The configured `DotnetGrpcGatewayOptions` instance for method chaining.
- **Throws**: `ArgumentOutOfRangeException` if `timeout` is negative or zero.

### `SetHealthCheckInterval(TimeSpan interval)`
Sets the interval between health check probes.
- **Parameters**:
  - `interval` (TimeSpan): The interval duration.
- **Return value**: The configured `DotnetGrpcGatewayOptions` instance for method chaining.
- **Throws**: `ArgumentOutOfRangeException` if `interval` is negative or zero.

### `SetMetricsCollectionInterval(TimeSpan interval)`
Sets the interval for collecting metrics.
- **Parameters**:
  - `interval` (TimeSpan): The collection interval.
- **Return value**: The configured `DotnetGrpcGatewayOptions` instance for method chaining.
- **Throws**: `ArgumentOutOfRangeException` if `interval` is negative or zero.

### `SetMetricsRetentionDays(int days)`
Sets the number of days to retain metrics data.
- **Parameters**:
  - `days` (int): The retention period in days.
- **Return value**: The configured `DotnetGrpcGatewayOptions` instance for method chaining.
- **Throws**: `ArgumentOutOfRangeException` if `days` is less than 0.

### `SetRequestLoggingVerbosity(RequestLoggingVerbosity verbosity)`
Sets the verbosity level for request logging.
- **Parameters**:
  - `verbosity` (RequestLoggingVerbosity): The verbosity level (e.g., `RequestLoggingVerbosity.Detailed`).
- **Return value**: The configured `DotnetGrpcGatewayOptions` instance for method chaining.
- **Throws**: `ArgumentOutOfRangeException` if `verbosity` is not a valid `RequestLoggingVerbosity` value.

### `SetRequestLoggingEnabled(bool enabled)`
Enables or disables request logging for the gateway.
- **Parameters**:
  - `enabled` (bool): `true` to enable logging; `false` to disable.
- **Return value**: The configured `DotnetGrpcGatewayOptions` instance for method chaining.

## Usage

### Example 1: Basic Configuration
