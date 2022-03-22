# IReflectionService

`IReflectionService` provides an abstraction for retrieving and refreshing gRPC service reflection metadata. It allows consumers to obtain detailed information about individual services or all registered services, and to force a refresh of cached reflection data when the underlying service definitions change.

## API

### `ReflectionService`

```csharp
public ReflectionService ReflectionService { get; }
```

Exposes the underlying `ReflectionService` instance that this implementation wraps. This property provides direct access to the core reflection functionality for scenarios that require lower-level control beyond the methods defined on this interface.

### `GetServiceReflectionAsync`

```csharp
public Task<ServiceReflectionInfo?> GetServiceReflectionAsync(string serviceName);
```

Retrieves reflection metadata for a single service identified by its fully qualified name.

**Parameters:**
- `serviceName` — The fully qualified service name (e.g., `"package.ServiceName"`).

**Return value:**
A `Task` that resolves to a `ServiceReflectionInfo` instance containing methods, request/response types, and related metadata, or `null` if the service is not found.

**Exceptions:**
- `ArgumentNullException` — Thrown when `serviceName` is `null`.
- `ArgumentException` — Thrown when `serviceName` is empty or whitespace.

### `GetAllReflectionInfoAsync`

```csharp
public Task<IReadOnlyList<ServiceReflectionInfo>> GetAllReflectionInfoAsync();
```

Retrieves reflection metadata for all services currently registered in the reflection source.

**Return value:**
A `Task` that resolves to a read-only list of `ServiceReflectionInfo` instances. Returns an empty list when no services are registered.

### `RefreshServiceReflectionAsync`

```csharp
public async Task<ServiceReflectionInfo> RefreshServiceReflectionAsync(string serviceName);
```

Forces a refresh of cached reflection data for the specified service and returns the updated metadata. This method bypasses any cached state and re-fetches the service definition from the underlying source.

**Parameters:**
- `serviceName` — The fully qualified service name to refresh.

**Return value:**
A `Task` that resolves to the refreshed `ServiceReflectionInfo` instance.

**Exceptions:**
- `ArgumentNullException` — Thrown when `serviceName` is `null`.
- `ArgumentException` — Thrown when `serviceName` is empty or whitespace.
- `InvalidOperationException` — Thrown when the specified service cannot be found after refresh.

### `RefreshAllReflectionsAsync`

```csharp
public async Task RefreshAllReflectionsAsync();
```

Forces a refresh of cached reflection data for all registered services. After completion, subsequent calls to `GetAllReflectionInfoAsync` or `GetServiceReflectionAsync` will return the updated metadata.

## Usage

### Retrieving a single service's metadata

```csharp
IReflectionService reflectionService = /* obtained via DI or factory */;

ServiceReflectionInfo? info = await reflectionService.GetServiceReflectionAsync("mypackage.MyService");

if (info is not null)
{
    foreach (var method in info.Methods)
    {
        Console.WriteLine($"Method: {method.Name}");
        Console.WriteLine($"  Request: {method.RequestType.FullName}");
        Console.WriteLine($"  Response: {method.ResponseType.FullName}");
    }
}
else
{
    Console.WriteLine("Service not found.");
}
```

### Refreshing all services after a hot-reload

```csharp
IReflectionService reflectionService = /* obtained via DI or factory */;

// Application detects that proto definitions have been updated at runtime
await reflectionService.RefreshAllReflectionsAsync();

// Consume the updated metadata
IReadOnlyList<ServiceReflectionInfo> allServices = await reflectionService.GetAllReflectionInfoAsync();

foreach (var service in allServices)
{
    Console.WriteLine($"Service: {service.FullName} ({service.Methods.Count} methods)");
}
```

## Notes

- Implementations are expected to cache reflection data internally. `GetServiceReflectionAsync` and `GetAllReflectionInfoAsync` may return stale data if the underlying service registry has changed; call the corresponding `Refresh*` method to ensure up-to-date results.
- `RefreshServiceReflectionAsync` throws `InvalidOperationException` when the service is not found after refresh, whereas `GetServiceReflectionAsync` returns `null` for unknown services. Callers should handle this distinction when refreshing individual services that may have been removed.
- The `IReadOnlyList<ServiceReflectionInfo>` returned by `GetAllReflectionInfoAsync` is a snapshot and will not reflect subsequent changes without an explicit refresh.
- Thread safety depends on the implementation. Callers should assume that concurrent calls to refresh methods and read methods may produce inconsistent results unless the implementation documents otherwise.
