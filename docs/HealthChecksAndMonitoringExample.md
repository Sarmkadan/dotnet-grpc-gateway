# HealthChecksAndMonitoringExample

`HealthChecksAndMonitoringExample` is a utility class designed to demonstrate and facilitate health checks and monitoring operations for a gRPC gateway. It provides methods to assess the health, readiness, and liveness of services, as well as to poll and display detailed status information. This class is typically used in diagnostic or administrative tools to validate the operational state of the gateway and its underlying services.

## API

### `HealthChecksAndMonitoringExample()`

**Purpose**: Initializes a new instance of the `HealthChecksAndMonitoringExample` class.

**Parameters**: None.

**Return Value**: A new instance of `HealthChecksAndMonitoringExample`.

**Exceptions**: None.

---

### `CheckGatewayHealthAsync()`

**Purpose**: Performs a health check on the gRPC gateway to determine its current operational status.

**Parameters**: None.

**Return Value**: `Task` representing the asynchronous operation.

**Exceptions**: Throws `HttpRequestException` if the health check endpoint is unreachable. Throws `InvalidOperationException` if the gateway configuration is invalid.

---

### `DisplayDetailedHealthStatusAsync()`

**Purpose**: Retrieves and displays comprehensive health status information for all monitored services.

**Parameters**: None.

**Return Value**: `Task` representing the asynchronous operation.

**Exceptions**: Throws `InvalidOperationException` if health data is unavailable or malformed.

---

### `DisplayAllServicesHealthAsync()`

**Purpose**: Lists the health status of all registered services in the gateway.

**Parameters**: None.

**Return Value**: `Task` representing the asynchronous operation.

**Exceptions**: Throws `HttpRequestException` if the service registry is inaccessible.

---

### `DisplayServiceHealthDetailAsync()`

**Purpose**: Retrieves and displays detailed health information for a specific service (implementation-defined which service).

**Parameters**: None.

**Return Value**: `Task` representing the asynchronous operation.

**Exceptions**: Throws `KeyNotFoundException` if the target service is not registered. Throws `HttpRequestException` if the service health endpoint is unreachable.

---

### `CheckReadinessAsync()`

**Purpose**: Checks whether the gateway is ready to accept traffic (e.g., dependencies are healthy, initialization complete).

**Parameters**: None.

**Return Value**: `Task` representing the asynchronous operation.

**Exceptions**: Throws `InvalidOperationException` if readiness checks fail due to unmet prerequisites.

---

### `CheckLivenessAsync()`

**Purpose**: Checks whether the gateway process is alive and responsive (e.g., not deadlocked or crashed).

**Parameters**: None.

**Return Value**: `Task` representing the asynchronous operation.

**Exceptions**: Throws `InvalidOperationException` if the liveness check indicates the process is unresponsive.

---

### `PollHealthStatusAsync()`

**Purpose**: Continuously polls the gateway health status at regular intervals until manually stopped or an error occurs.

**Parameters**: None.

**Return Value**: `Task` representing the asynchronous operation.

**Exceptions**: Throws `OperationCanceledException` if the polling is externally canceled. Throws `HttpRequestException` if repeated polling attempts fail.

---

### `Main()`

**Purpose**: Entry point for the application, demonstrating health check and monitoring workflows.

**Parameters**: None.

**Return Value**: `Task` representing the asynchronous execution of the application.

**Exceptions**: Propagates exceptions from invoked health check methods.

## Usage

### Example 1: Basic Gateway Health Check

```csharp
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var monitor = new HealthChecksAndMonitoringExample();
        try
        {
            await monitor.CheckGatewayHealthAsync();
            Console.WriteLine("Gateway health check completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Health check failed: {ex.Message}");
        }
    }
}
```

### Example 2: Continuous Health Polling

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var monitor = new HealthChecksAndMonitoringExample();
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        
        try
        {
            await monitor.PollHealthStatusAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Polling stopped due to timeout.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Polling error: {ex.Message}");
        }
    }
}
```

## Notes

- **Thread Safety**: This class does not implement thread-safe mechanisms. Concurrent calls to instance methods may result in race conditions or inconsistent state if shared resources are accessed without external synchronization.
- **Blocking Behavior**: Although all methods are asynchronous, invoking them synchronously (e.g., `.Result` or `.Wait()`) may cause deadlocks in certain contexts, particularly in UI or ASP.NET environments.
- **Polling Lifecycle**: `PollHealthStatusAsync` runs indefinitely unless a cancellation token is provided. Implementations should ensure proper cancellation handling to avoid resource leaks.
- **Error Handling**: Most methods throw exceptions on failure rather than returning error codes. Callers must handle exceptions explicitly to ensure robust operation.
- **Service Specificity**: `DisplayServiceHealthDetailAsync` behavior depends on internal logic for selecting a target service. If no default service is configured, it may throw `KeyNotFoundException`.
