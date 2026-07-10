# ReflectionController
The `ReflectionController` provides HTTP endpoints that expose gRPC server reflection data, allowing clients to discover available services, their methods, and the health of the reflection subsystem.

## API
### GetAllReflection
```csharp
public async Task<ActionResult<IReadOnlyList<ServiceReflectionInfo>>> GetAllReflection()
```
**Purpose:** Returns reflection information for every service currently known to the server.  
**Parameters:** None.  
**Return Value:** `ActionResult<IReadOnlyList<ServiceReflectionInfo>>`. On success, the `Result` is an `OkObjectResult` containing a read‑only list of `ServiceReflectionInfo` instances. On failure, the result is a status code indicating the error (e.g., 500 Internal Server Error).  
**Throws:** Does not throw exceptions directly; error conditions are conveyed through the returned `ActionResult`.

### GetServiceReflection
```csharp
public async Task<ActionResult<ServiceReflectionInfo>> GetServiceReflection(string serviceName)
```
**Purpose:** Retrieves reflection data for a specific service.  
**Parameters:**  
- `serviceName`: The fully qualified name of the gRPC service to query. Must not be `null` or empty.  
**Return Value:** `ActionResult<ServiceReflectionInfo>`. On success, the `Result` is an `OkObjectResult` containing the `ServiceReflectionInfo` for the requested service. If the service is not found, returns a 404 Not Found result. Other errors produce appropriate status codes (e.g., 500).  
**Throws:** Does not throw exceptions directly; error conditions are conveyed through the returned `ActionResult`.

### GetServiceMethods
```csharp
public async Task<ActionResult<List<ServiceMethodDescriptor>>> GetServiceMethods(string serviceName)
```
**Purpose:** Returns the list of method descriptors for a given service.  
**Parameters:**  
- `serviceName`: The fully qualified name of the gRPC service. Must not be `null` or empty.  
**Return Value:** `ActionResult<List<ServiceMethodDescriptor>>`. On success, the `Result` is an `OkObjectResult` containing a list of `ServiceMethodDescriptor` objects. If the service is not found, returns a 404 Not Found result. Other errors produce appropriate status codes.  
**Throws:** Does not throw exceptions directly; error conditions are conveyed through the returned `ActionResult`.

### RefreshServiceReflection
```csharp
public async Task<ActionResult<ServiceReflectionInfo>> RefreshServiceReflection(string serviceName)
```
**Purpose:** Forces a refresh of the reflection data for a specific service and returns the updated information.  
**Parameters:**  
- `serviceName`: The fully qualified name of the gRPC service to refresh. Must not be `null` or empty.  
**Return Value:** `ActionResult<ServiceReflectionInfo>`. On success, the `Result` is an `OkObjectResult` containing the refreshed `ServiceReflectionInfo`. If the service is not found, returns a 404 Not Found result. Other errors produce appropriate status codes.  
**Throws:** Does not throw exceptions directly; error conditions are conveyed through the returned `ActionResult`.

### RefreshAllReflections
```csharp
public async Task<ActionResult> RefreshAllReflections()
```
**Purpose:** Triggers a refresh of reflection data for all known services.  
**Parameters:** None.  
**Return Value:** `ActionResult`. On success, returns an `OkResult` (HTTP 200). On failure, returns an appropriate error status code (e.g., 500 Internal Server Error).  
**Throws:** Does not throw exceptions directly; error conditions are conveyed through the returned `ActionResult`.

### GetReflectionHealth
```csharp
public async Task<ActionResult<ReflectionHealthSummary>> GetReflectionHealth()
```
**Purpose:** Provides a summary of the health of the reflection subsystem.  
**Parameters:** None.  
**Return Value:** `ActionResult<ReflectionHealthSummary>`. On success, the `Result` is an `OkObjectResult` containing a `ReflectionHealthSummary` instance. Errors produce appropriate status codes (e.g., 500).  
**Throws:** Does not throw exceptions directly; error conditions are conveyed through the returned `ActionResult`.

### IsAvailable
```csharp
public bool IsAvailable { get; }
```
**Purpose:** Indicates whether the reflection subsystem is currently operational and able to serve reflection data.  
**Return Value:** `true` if reflection is available; otherwise `false`.  
**Throws:** None.

### AvailableServiceCount
```csharp
public int AvailableServiceCount { get; }
```
**Purpose:** Number of services for which reflection data is successfully available at the last check.  
**Return Value:** Non‑negative integer.  
**Throws:** None.

### TotalServiceCount
```csharp
public int TotalServiceCount { get; }
```
**Purpose:** Total number of services known to the server, regardless of whether their reflection data is currently available.  
**Return Value:** Non‑negative integer (≥ `AvailableServiceCount`).  
**Throws:** None.

### CheckedAt
```csharp
public DateTime CheckedAt { get; }
```
**Purpose:** Timestamp of the most recent reflection health check.  
**Return Value:** A `DateTime` representing when the subsystem was last probed.  
**Throws:** None.

### Message
```csharp
public string? Message { get; }
```
**Purpose:** Optional diagnostic message providing additional context about the reflection subsystem’s state (e.g., error details when `IsAvailable` is `false`).  
**Return Value:** A string message or `null` if no message is available.  
**Throws:** None.

## Usage
### Example 1: Listing all reflected services
```csharp
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

// Assume the controller is instantiated via dependency injection in an ASP.NET Core app.
var reflectionController = new ReflectionController(/* required dependencies */);

ActionResult<IReadOnlyList<ServiceReflectionInfo>> result = await reflectionController.GetAllReflection();

if (result.Result is OkObjectResult ok && ok.Value is IReadOnlyList<ServiceReflectionInfo> infos)
{
    foreach (var info in infos)
    {
        Console.WriteLine($"Service: {info.ServiceName}");
    }
}
else
{
    // Handle error status codes via result.Result (e.g., NotFound, StatusCodeResult)
    Console.WriteLine($"Failed to retrieve reflections: {result.Result}");
}
```

### Example 2: Refreshing a specific service and handling missing service
```csharp
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

var reflectionController = new ReflectionController(/* required dependencies */);
string serviceName = "my.package.MyService";

ActionResult<ServiceReflectionInfo> refreshResult = await reflectionController.RefreshServiceReflection(serviceName);

if (refreshResult.Result is OkObjectResult ok && ok.Value is ServiceReflectionInfo info)
{
    Console.WriteLine($"Reflection for {info.ServiceName} refreshed at {info.LastUpdated}");
}
else if (refreshResult.Result is NotFoundResult)
{
    Console.WriteLine($"Service '{serviceName}' not found.");
}
else
{
    // Unexpected error
    Console.WriteLine($"Refresh failed with status: {refreshResult.Result}");
}
```

## Notes
- The controller does not maintain mutable state across requests; each method call operates on the current state of the underlying reflection provider, making the class thread‑safe for concurrent HTTP requests.
- Property values (`IsAvailable`, `AvailableServiceCount`, `TotalServiceCount`, `CheckedAt`, `Message`) reflect the state at the moment they are accessed and may change between calls; consumers should not rely on them remaining constant during a sequence of operations.
- Passing `null`, empty, or whitespace‑only strings for `serviceName` in methods that accept a service name will result in a 400 Bad Request response (the underlying model binding or validation will reject the input before the method body executes).
- If the reflection subsystem is disabled or encounters an internal failure, `IsAvailable` will return `false` and `Message` may contain a diagnostic string; the various `Get*` and `Refresh*` endpoints will then return appropriate error status codes rather than throwing exceptions.
- The `DateTime` returned by `CheckedAt` is expressed in the server’s local time zone; callers requiring UTC should convert accordingly (`CheckedAt.ToUniversalTime()`).
