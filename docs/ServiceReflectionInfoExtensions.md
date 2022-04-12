# ServiceReflectionInfoExtensions

The `ServiceReflectionInfoExtensions` class provides a set of static extension methods designed to facilitate runtime introspection and health verification of gRPC service definitions within the `dotnet-grpc-gateway` ecosystem. By operating on service reflection metadata, these utilities enable developers to programmatically retrieve method descriptors, enumerate available operations, generate human-readable summaries, and validate the structural integrity of service configurations without requiring direct instantiation of the underlying service types.

## API

### ToSummaryString
Generates a concise, human-readable representation of the service reflection information.
*   **Purpose**: Converts the complex metadata of a service into a single string summary, typically including the service name and method count for logging or diagnostic display.
*   **Parameters**: Accepts the target service reflection object as the `this` parameter.
*   **Return Value**: Returns a `string` containing the formatted summary.
*   **Exceptions**: Throws `ArgumentNullException` if the input reflection object is null.

### GetMethodNames
Retrieves a collection of all method names defined within the specified service.
*   **Purpose**: Enumerates the identifiers of every RPC method exposed by the service, useful for dynamic routing or validation logic.
*   **Parameters**: Accepts the target service reflection object as the `this` parameter.
*   **Return Value**: Returns an `IEnumerable<string>` containing the names of the methods.
*   **Exceptions**: Throws `ArgumentNullException` if the input reflection object is null.

### IsHealthy
Validates the structural integrity and readiness of the service reflection data.
*   **Purpose**: Determines if the service metadata is correctly populated and consistent, ensuring it is safe to use for request dispatching.
*   **Parameters**: Accepts the target service reflection object as the `this` parameter.
*   **Return Value**: Returns a `bool` indicating `true` if the service is valid and ready, or `false` if inconsistencies or missing data are detected.
*   **Exceptions**: Does not throw exceptions for invalid states; returns `false` instead. Throws `ArgumentNullException` if the input is null.

### GetMethodByName
Locates a specific method descriptor by its unique name.
*   **Purpose**: Fetches the detailed descriptor for a single RPC method, allowing access to its input/output types and handling logic.
*   **Parameters**: Accepts the target service reflection object as the `this` parameter and a `string` representing the method name to search for.
*   **Return Value**: Returns a `ServiceMethodDescriptor?` containing the method details if found, or `null` if no method matches the provided name.
*   **Exceptions**: Throws `ArgumentNullException` if the input reflection object or the method name string is null.

## Usage

### Example 1: Service Health Check and Method Enumeration
This example demonstrates how to verify a service's integrity before iterating through its available methods for logging purposes.

```csharp
using DotNetGrpcGateway;

public void DiagnoseService(ServiceReflectionInfo serviceInfo)
{
    if (serviceInfo.IsHealthy())
    {
        Console.WriteLine($"Service '{serviceInfo.ToSummaryString()}' is operational.");
        
        foreach (var methodName in serviceInfo.GetMethodNames())
        {
            Console.WriteLine($"  - Found method: {methodName}");
        }
    }
    else
    {
        Console.Error.WriteLine("Service reflection data is corrupted or incomplete.");
    }
}
```

### Example 2: Dynamic Method Resolution
This example illustrates retrieving a specific method descriptor dynamically to handle an incoming request based on a runtime-provided method name.

```csharp
using DotNetGrpcGateway;

public async Task HandleRequest(ServiceReflectionInfo serviceInfo, string requestedMethod)
{
    var methodDescriptor = serviceInfo.GetMethodByName(requestedMethod);
    
    if (methodDescriptor.HasValue)
    {
        // Proceed with invocation logic using methodDescriptor.Value
        await InvokeMethodAsync(methodDescriptor.Value);
    }
    else
    {
        throw new RpcException(new Status(StatusCode.NotFound, $"Method '{requestedMethod}' not found."));
    }
}
```

## Notes

*   **Null Safety**: All extension methods strictly validate the input `ServiceReflectionInfo` instance. Passing `null` as the source object will result in an `ArgumentNullException` being thrown immediately. Additionally, `GetMethodByName` validates that the provided method name string is not null.
*   **Return Semantics**: `GetMethodByName` utilizes a nullable return type (`ServiceMethodDescriptor?`). Callers must check `HasValue` or compare against `null` before accessing the descriptor properties to avoid runtime errors. `IsHealthy` returns `false` rather than throwing exceptions when metadata is invalid, making it suitable for conditional logic without try-catch blocks.
*   **Thread Safety**: As these methods are stateless static extensions that operate purely on the provided immutable reflection data, they are inherently thread-safe. Multiple threads may safely call these methods concurrently on the same `ServiceReflectionInfo` instance.
*   **Performance**: `GetMethodNames` returns an `IEnumerable<string>`. The underlying implementation may defer execution until enumeration; consumers should be aware that iterating multiple times over the result without caching could incur repeated reflection costs depending on the concrete implementation of the source object.
