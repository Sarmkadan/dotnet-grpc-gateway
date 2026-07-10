# DotnetGrpcGatewayOptions

The `DotnetGrpcGatewayOptions` class encapsulates all configurable parameters for the dotnet-grpc-gateway server. It controls network binding, protocol features (reflection, compression, metrics), health-check probing, request logging, and resource limits. Instances of this class are typically bound from configuration (e.g., `IConfiguration` section) or created programmatically and passed to the gateway builder.

## API

| Member | Type | Description |
|--------|------|-------------|
| `ListenAddress` | `string` | The IP address or hostname on which the gateway listens. Defaults to `"0.0.0.0"` if not set. |
| `Port` | `int` | The TCP port for the gateway. Must be between 1 and 65535. |
| `EnableReflection` | `bool` | When `true`, enables gRPC reflection service for client tooling (e.g., grpcurl). |
| `EnableMetrics` | `bool` | Global switch to enable Prometheus-style metrics export. When `false`, all metric collection is disabled regardless of sub‑options. |
| `MaxConcurrentConnections` | `int` | Maximum number of simultaneous gRPC connections. A value of `0` or less means unlimited. |
| `RequestTimeoutMs` | `int` | Timeout in milliseconds for a single gRPC request. A non‑positive value disables the timeout. |
| `LogLevel` | `string` | Minimum log level for the gateway’s own logging (e.g., `"Information"`, `"Warning"`). Accepts standard Microsoft.Extensions.Logging level names. |
| `EnableCompression` | `bool` | When `true`, enables gRPC message compression (deflate/gzip) for responses. |
| `HealthCheck` | `HealthCheckOptions` | Sub‑object containing health‑check probe settings. See nested members below. |
| `Metrics` | `MetricsOptions` | Sub‑object controlling metric collection details. See nested members below. |
| `RequestLogging` | `RequestLoggingOptions` | Sub‑object configuring request/response logging. See nested members below. |
| `IntervalSeconds` | `int` | Interval in seconds between health‑check probes. Part of `HealthCheckOptions`. Must be greater than zero. |
| `TimeoutMs` | `int` | Timeout in milliseconds for each health‑check probe. Part of `HealthCheckOptions`. A non‑positive value means no timeout. |
| `FailureThreshold` | `int` | Number of consecutive failed probes before the service is marked unhealthy. Part of `HealthCheckOptions`. Must be at least 1. |
| `EnableMetrics` | `bool` | Enables or disables metric collection within the `Metrics` sub‑object. This is separate from the top‑level `EnableMetrics` flag; both must be `true` for metrics to be active. |
| `CollectionIntervalSeconds` | `int` | Interval in seconds between metric snapshots. Part of `MetricsOptions`. Must be greater than zero. |
| `RetentionDays` | `int` | Number of days to retain metric data in memory. Part of `MetricsOptions`. A value of `0` means retain indefinitely. |
| `Enabled` | `bool` | Enables or disables request logging entirely. Part of `RequestLoggingOptions`. |
| `Verbosity` | `RequestLoggingVerbosity` | Level of detail for logged requests (e.g., `Headers`, `Full`). Part of `RequestLoggingOptions`. |

### Exceptions

- Setting `Port` to a value outside [1, 65535] throws `ArgumentOutOfRangeException`.
- Setting `IntervalSeconds` or `CollectionIntervalSeconds` to a non‑positive value throws `ArgumentOutOfRangeException`.
- Setting `FailureThreshold` to less than 1 throws `ArgumentOutOfRangeException`.
- Setting `ListenAddress` to `null` or an empty string throws `ArgumentException`.

## Usage

### Example 1: Configuration from appsettings.json

```csharp
// appsettings.json
{
  "GrpcGateway": {
    "ListenAddress": "0.0.0.0",
    "Port": 5000,
    "EnableReflection": true,
    "EnableMetrics": true,
    "MaxConcurrentConnections": 100,
    "RequestTimeoutMs": 30000,
    "LogLevel": "Information",
    "EnableCompression": true,
    "HealthCheck": {
      "IntervalSeconds": 10,
      "TimeoutMs": 5000,
      "FailureThreshold": 3
    },
    "Metrics": {
      "EnableMetrics": true,
      "CollectionIntervalSeconds": 15,
      "RetentionDays": 7
    },
    "RequestLogging": {
      "Enabled": true,
      "Verbosity": "Headers"
    }
  }
}

// Startup code
var options = new DotnetGrpcGatewayOptions();
configuration.GetSection("GrpcGateway").Bind(options);
var gateway = new GrpcGatewayBuilder(options).Build();
```

### Example 2: Programmatic creation with custom settings

```csharp
var options = new DotnetGrpcGatewayOptions
{
    ListenAddress = "localhost",
    Port = 8080,
    EnableReflection = false,
    EnableMetrics = true,
    MaxConcurrentConnections = 50,
    RequestTimeoutMs = 10000,
    LogLevel = "Warning",
    EnableCompression = false,
    HealthCheck = new HealthCheckOptions
    {
        IntervalSeconds = 5,
        TimeoutMs = 2000,
        FailureThreshold = 2
    },
    Metrics = new MetricsOptions
    {
        EnableMetrics = true,
        CollectionIntervalSeconds = 30,
        RetentionDays = 0
    },
    RequestLogging = new RequestLoggingOptions
    {
        Enabled = true,
        Verbosity = RequestLoggingVerbosity.Full
    }
};

var gateway = new GrpcGatewayBuilder(options).Build();
```

## Notes

- **Thread safety**: `DotnetGrpcGatewayOptions` is not thread‑safe for concurrent writes. It should be fully configured before being passed to the gateway builder. After the gateway is built, the options instance is not read again; modifications have no effect.
- **Nested options**: The `HealthCheck`, `Metrics`, and `RequestLogging` properties are objects. Their individual fields (`IntervalSeconds`, `EnableMetrics`, `Enabled`, etc.) are exposed as direct properties on `DotnetGrpcGatewayOptions` for convenience when binding from flat configuration sources. When setting these values programmatically, prefer assigning the sub‑object properties directly to avoid ambiguity.
- **Duplicate `EnableMetrics`**: There are two properties named `EnableMetrics`. The first (top‑level) acts as a master switch for all metrics. The second (inside `Metrics`) controls whether the metrics subsystem itself is active. Both must be `true` for metrics to be exported.
- **Zero or negative values**: `MaxConcurrentConnections` of `0` or less means unlimited. `RequestTimeoutMs` of `0` or less disables the timeout. `RetentionDays` of `0` means data is kept indefinitely. All other numeric fields (`Port`, `IntervalSeconds`, `CollectionIntervalSeconds`, `FailureThreshold`) reject non‑positive values.
- **LogLevel**: The string is case‑insensitive and must match a standard `LogLevel` enum name (e.g., `"Trace"`, `"Debug"`, `"Information"`, `"Warning"`, `"Error"`, `"Critical"`, `"None"`). An unrecognized value defaults to `"Information"`.
- **Health‑check defaults**: If `HealthCheck` is `null`, health‑check endpoints are disabled. When set, `IntervalSeconds` defaults to 30, `TimeoutMs` to 5000, and `FailureThreshold` to 3 if not explicitly assigned.
