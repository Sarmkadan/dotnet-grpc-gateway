# GrpcServiceExtensions

`GrpcServiceExtensions` provides a set of static utility methods for monitoring and managing the health status of gRPC services within the `dotnet-grpc-gateway` project. It tracks request outcomes, determines when a health check is due, and generates summary information, enabling basic circuit-breaker or readiness-probe patterns without external dependencies.

## API

### `IsHealthCheckDue`

```csharp
public static bool IsHealthCheckDue { get; }
```

**Purpose:** Indicates whether a health check should be performed based on internal timing or failure thresholds.

**Return value:** `true` if the conditions for triggering a health check have been met; otherwise `false`.

**Exceptions:** None.

### `GetHealthCheckUri`

```csharp
public static string GetHealthCheckUri { get; }
```

**Purpose:** Returns the URI endpoint that should be called to perform the actual health check against the downstream gRPC service.

**Return value:** A non-null string containing the health check URI.

**Exceptions:** None.

### `RecordSuccessfulRequest`

```csharp
public static void RecordSuccessfulRequest()
```

**Purpose:** Records that a gRPC request completed successfully, resetting or advancing internal counters used to determine health status.

**Parameters:** None.

**Return value:** None.

**Exceptions:** None.

### `RecordFailedRequest`

```csharp
public static void RecordFailedRequest()
```

**Purpose:** Records that a gRPC request failed, incrementing failure counters that may cause `IsHealthCheckDue` to return `true` once a threshold is reached.

**Parameters:** None.

**Return value:** None.

**Exceptions:** None.

### `GetSummary`

```csharp
public static string GetSummary()
```

**Purpose:** Produces a human-readable summary of the current health tracking state, including success/failure counts and the health check status.

**Return value:** A string summarizing the internal state.

**Exceptions:** None.

## Usage

### Example 1: Recording outcomes and checking health before each call

```csharp
// Before making a gRPC call, check if a health check is due
if (GrpcServiceExtensions.IsHealthCheckDue)
{
    var uri = GrpcServiceExtensions.GetHealthCheckUri;
    // Perform the health check against uri, e.g., with HttpClient
    // If the health check passes, proceed; otherwise, break or retry
}

try
{
    // Make the actual gRPC call
    await client.SomeMethodAsync(request);
    GrpcServiceExtensions.RecordSuccessfulRequest();
}
catch (RpcException)
{
    GrpcServiceExtensions.RecordFailedRequest();
    throw;
}
```

### Example 2: Periodic logging of health state

```csharp
// Periodically log the health summary for diagnostics
var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
while (await timer.WaitForNextTickAsync())
{
    var summary = GrpcServiceExtensions.GetSummary();
    logger.LogInformation("gRPC health state: {Summary}", summary);

    if (GrpcServiceExtensions.IsHealthCheckDue)
    {
        logger.LogWarning("Health check is due; endpoint: {Uri}", GrpcServiceExtensions.GetHealthCheckUri);
    }
}
```

## Notes

- All members are static and operate on shared state. In multi-threaded environments, `RecordSuccessfulRequest` and `RecordFailedRequest` may be called concurrently. The implementation must ensure atomic updates to internal counters to avoid race conditions or torn reads.
- `IsHealthCheckDue` and `GetHealthCheckUri` are properties, not methods; accessing them does not mutate state.
- `GetSummary` returns a snapshot of the current state. The values it reports may be stale by the time the caller acts on them if other threads continue recording outcomes.
- The exact failure threshold and timing logic that govern `IsHealthCheckDue` are internal. Callers should treat `IsHealthCheckDue` as a hint and not assume a specific number of failures will always trigger it deterministically without consulting the implementation details.
- `GetHealthCheckUri` returns a fixed or configured URI; it does not change based on recorded request outcomes.
