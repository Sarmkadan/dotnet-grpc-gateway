# BasicServiceRegistrationExample

The `BasicServiceRegistrationExample` class serves as a standalone utility for demonstrating and validating the lifecycle management of gRPC services within the `dotnet-grpc-gateway` ecosystem. It encapsulates the core operations required to register a service with the gateway, verify its availability, monitor its health status, and gracefully unregister it, providing a reference implementation for integration tests and bootstrap scenarios.

## API

### `public BasicServiceRegistrationExample`
Initializes a new instance of the `BasicServiceRegistrationExample` class. This constructor prepares the internal state required to interact with the gRPC gateway, typically configuring default endpoints or loading necessary context from the environment. It does not accept parameters and does not return a value.

### `public async Task RegisterServiceAsync`
Asynchronously registers the current service instance with the gRPC gateway.
*   **Parameters**: None.
*   **Return Value**: A `Task` representing the asynchronous operation. The task completes when the registration handshake is successfully acknowledged by the gateway.
*   **Exceptions**: Throws an exception if the gateway is unreachable, if the service identifier is already in use, or if network connectivity fails during the handshake.

### `public async Task VerifyServiceRegistrationAsync`
Asynchronously confirms that the service has been successfully registered and is visible to the gateway's routing table.
*   **Parameters**: None.
*   **Return Value**: A `Task` that completes upon successful verification.
*   **Exceptions**: Throws an exception if the service cannot be found in the gateway registry or if the registration state is inconsistent.

### `public async Task CheckServiceHealthAsync`
Asynchronously performs a health check against the registered service endpoint to ensure it is responsive and ready to handle traffic.
*   **Parameters**: None.
*   **Return Value**: A `Task` representing the asynchronous health probe.
*   **Exceptions**: Throws an exception if the service endpoint fails to respond within the timeout period or returns an unhealthy status code.

### `public async Task UnregisterServiceAsync`
Asynchronously removes the service from the gRPC gateway, stopping further traffic routing to this instance.
*   **Parameters**: None.
*   **Return Value**: A `Task` that completes when the deregistration signal is acknowledged.
*   **Exceptions**: Throws an exception if the service was not previously registered or if the gateway fails to process the removal request.

### `public static async Task Main`
The static entry point for the application. This method orchestrates the full lifecycle demonstration by sequentially invoking registration, verification, health checking, and unregistration logic.
*   **Parameters**: None (implicitly accepts `string[] args` via standard C# entry point conventions, though not explicitly listed in the signature provided).
*   **Return Value**: A `Task` representing the execution of the entire demo workflow.
*   **Exceptions**: Propagates any unhandled exceptions occurring during the lifecycle sequence to the runtime.

## Usage

### Example 1: Programmatic Lifecycle Management
This example demonstrates how to instantiate the class and manually control the service lifecycle within a custom host environment.

```csharp
using System;
using System.Threading.Tasks;

public class Program
{
    public static async Task RunCustomLifecycle()
    {
        var registrar = new BasicServiceRegistrationExample();

        try
        {
            // Register the service with the gateway
            await registrar.RegisterServiceAsync();
            Console.WriteLine("Service registered successfully.");

            // Verify the registration is active
            await registrar.VerifyServiceRegistrationAsync();
            Console.WriteLine("Registration verified.");

            // Perform a health check
            await registrar.CheckServiceHealthAsync();
            Console.WriteLine("Service health check passed.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Lifecycle error: {ex.Message}");
            // Attempt cleanup on failure
            try 
            {
                await registrar.UnregisterServiceAsync();
            } 
            catch { /* Ignore secondary errors during cleanup */ }
            throw;
        }
        finally
        {
            // Gracefully unregister
            await registrar.UnregisterServiceAsync();
            Console.WriteLine("Service unregistered.");
        }
    }
}
```

### Example 2: Integration Test Scenario
This example illustrates using the class to validate gateway connectivity and registration logic in an automated test suite.

```csharp
using System;
using System.Threading.Tasks;
using Xunit;

public class GatewayIntegrationTests
{
    [Fact]
    public async Task Service_Should_Register_And_Pass_Health_Check()
    {
        var registrar = new BasicServiceRegistrationExample();

        // Act: Register and Verify
        await registrar.RegisterServiceAsync();
        await registrar.VerifyServiceRegistrationAsync();

        // Assert: Health check must complete without throwing
        var healthTask = registrar.CheckServiceHealthAsync();
        await healthTask; // Will throw if health check fails

        // Cleanup
        await registrar.UnregisterServiceAsync();
    }
}
```

## Notes

*   **Execution Order**: The methods are designed to be called in a specific sequence: `RegisterServiceAsync` must precede `VerifyServiceRegistrationAsync`, `CheckServiceHealthAsync`, and `UnregisterServiceAsync`. Calling verification or health checks prior to successful registration will result in exceptions.
*   **Idempotency**: `RegisterServiceAsync` and `UnregisterServiceAsync` are not guaranteed to be idempotent. Attempting to register an already active service or unregister a service that has already been removed may throw exceptions depending on the gateway's current state.
*   **Thread Safety**: The instance methods of `BasicServiceRegistrationExample` are not thread-safe. Concurrent calls to `RegisterServiceAsync`, `UnregisterServiceAsync`, or state-checking methods on the same instance may lead to race conditions or inconsistent state errors. External synchronization is required if multiple threads access the same instance.
*   **Resource Disposal**: While the class manages logical registration states, it does not implement `IDisposable`. Ensure that `UnregisterServiceAsync` is explicitly awaited during application shutdown to prevent orphaned entries in the gateway registry.
*   **Static Entry Point**: The `Main` method is intended for direct execution as a console application or demo tool and should not be invoked programmatically alongside instance methods on the same process context to avoid duplicate registration attempts.
