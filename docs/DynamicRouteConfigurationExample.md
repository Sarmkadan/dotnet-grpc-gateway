# DynamicRouteConfigurationExample

The `DynamicRouteConfigurationExample` class provides a self-contained demonstration of dynamic route configuration for a gRPC‑Gateway application. It illustrates how to programmatically define, inspect, test, and detect conflicts among routes without relying on static configuration files. The class is intended for learning, prototyping, or integration testing scenarios.

## API

### `public DynamicRouteConfigurationExample()`

Initializes a new instance of the example. The constructor sets up an internal collection of predefined route definitions that will be used by the other instance methods.

- **Parameters**: None.
- **Return value**: None.
- **Throws**: None.

### `public async Task ConfigureRoutesAsync()`

Applies the predefined route definitions to the underlying gateway configuration. This method simulates the process of registering routes with the routing infrastructure.

- **Parameters**: None.
- **Return value**: A `Task` representing the asynchronous operation.
- **Throws**: `InvalidOperationException` if the configuration has already been applied or if the internal state is inconsistent.

### `public async Task DisplayAllRoutesAsync()`

Outputs the currently configured routes to the console (or the default output stream). Each route is shown with its HTTP method, path template, and associated gRPC service method.

- **Parameters**: None.
- **Return value**: A `Task` representing the asynchronous operation.
- **Throws**: `InvalidOperationException` if `ConfigureRoutesAsync` has not been called first.

### `public async Task TestRouteMatchingAsync()`

Runs a series of predefined HTTP request simulations against the configured routes and prints whether each request matches a route. The test cases are hardcoded within the method.

- **Parameters**: None.
- **Return value**: A `Task` representing the asynchronous operation.
- **Throws**: `InvalidOperationException` if routes have not been configured.

### `public async Task DetectConflictsAsync()`

Analyzes the configured routes for overlapping path patterns or ambiguous method/verb combinations. If any conflicts are found, they are reported to the console.

- **Parameters**: None.
- **Return value**: A `Task` representing the asynchronous operation.
- **Throws**: `InvalidOperationException` if routes have not been configured.

### `public static async Task Main()`

The application entry point. It creates an instance of `DynamicRouteConfigurationExample`, calls the methods in a logical sequence (configure, display, test, detect conflicts), and handles any exceptions that occur.

- **Parameters**: None.
- **Return value**: A `Task` representing the asynchronous operation.
- **Throws**: None (exceptions are caught and printed to the console).

## Usage

The following examples demonstrate typical ways to use the class.

### Example 1: Basic demonstration

```csharp
using System;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main()
    {
        var example = new DynamicRouteConfigurationExample();
        await example.ConfigureRoutesAsync();
        await example.DisplayAllRoutesAsync();
        await example.TestRouteMatchingAsync();
        await example.DetectConflictsAsync();
    }
}
```

This example runs the full demonstration: configuration, display, matching tests, and conflict detection. All output is written to the console.

### Example 2: Selective use of methods

```csharp
using System;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main()
    {
        var example = new DynamicRouteConfigurationExample();
        await example.ConfigureRoutesAsync();

        // Only display routes and test matching, skip conflict detection
        await example.DisplayAllRoutesAsync();
        await example.TestRouteMatchingAsync();
    }
}
```

This example configures the routes, displays them, and runs the matching tests without performing conflict detection. It is useful when only a subset of the demonstration is needed.

## Notes

- **Thread safety**: The instance methods are not thread‑safe. Concurrent calls to `ConfigureRoutesAsync`, `DisplayAllRoutesAsync`, `TestRouteMatchingAsync`, or `DetectConflictsAsync` on the same instance may produce undefined behavior. The static `Main` method is safe to call from a single thread.
- **Order of operations**: `ConfigureRoutesAsync` must be called before any of the other instance methods. Calling `DisplayAllRoutesAsync`, `TestRouteMatchingAsync`, or `DetectConflictsAsync` before configuration will throw an `InvalidOperationException`.
- **Hardcoded routes**: The route definitions are embedded in the class and cannot be modified at runtime. The example is intended for demonstration only; production code should use a configurable route store.
- **Edge cases**: The conflict detection logic considers exact path duplicates and overlapping parameterized templates (e.g., `/users/{id}` and `/users/{name}`). It does not detect conflicts that arise from query string parameters or HTTP method mismatches. The matching tests include edge cases such as trailing slashes and case sensitivity, which are handled according to the default ASP.NET Core routing conventions.
- **Disposal**: The class does not implement `IDisposable` and holds no unmanaged resources. No explicit cleanup is required.
