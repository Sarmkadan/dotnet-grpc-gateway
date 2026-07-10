# IRouteResolutionService

The `IRouteResolutionService` interface defines the contract for resolving and managing gateway routes and target gRPC services within the `dotnet-grpc-gateway` framework. It provides methods for route resolution, service discovery, route matching, access validation, and cache management. Implementations of this interface are responsible for translating incoming requests into concrete `GatewayRoute` and `GrpcService` instances, as well as maintaining an internal cache to optimize repeated lookups.

## API

### `RouteResolutionService`

- **Type:** Property (`RouteResolutionService`)
- **Description:** Gets the underlying `RouteResolutionService` instance used for route resolution operations. This property provides access to the concrete service object that implements the route resolution logic.
- **Returns:** `RouteResolutionService`

### `ResolveRouteAsync`

- **Signature:** `Task<GatewayRoute> ResolveRouteAsync(...)`
- **Description:** Resolves a route asynchronously based on the provided request context. The method evaluates the incoming request and returns the corresponding `GatewayRoute` that matches the request criteria.
- **Returns:** `GatewayRoute` – the resolved route.
- **Throws:** An exception if the route cannot be resolved (e.g., no matching route exists or the request context is invalid).

### `ResolveTargetServiceAsync`

- **Signature:** `Task<GrpcService> ResolveTargetServiceAsync(...)`
- **Description:** Resolves the target gRPC service asynchronously. This method determines which backend gRPC service should handle the request based on the resolved route or other contextual information.
- **Returns:** `GrpcService` – the resolved target service.
- **Throws:** An exception if the target service cannot be found or is unavailable.

### `FindMatchingRouteAsync`

- **Signature:** `Task<GatewayRoute?> FindMatchingRouteAsync(...)`
- **Description:** Finds a matching route asynchronously. Unlike `ResolveRouteAsync`, this method returns `null` if no route matches, rather than throwing an exception.
- **Returns:** `GatewayRoute?` – the matching route, or `null` if no match is found.
- **Throws:** May throw for invalid arguments or internal errors.

### `GetRoutesForServiceAsync`

- **Signature:** `Task<List<GatewayRoute>> GetRoutesForServiceAsync(...)`
- **Description:** Retrieves all routes that are associated with a specific gRPC service. The service is identified by a parameter (e.g., service name or identifier).
- **Returns:** `List<GatewayRoute>` – a list of routes for the specified service. The list may be empty if no routes are defined for that service.
- **Throws:** An exception if the service identifier is invalid or the service is not registered.

### `ValidateRouteAccessAsync`

- **Signature:** `Task ValidateRouteAccessAsync(...)`
- **Description:** Validates that the current request has permission to access the resolved route. This method performs authorization checks based on the route’s access policies.
- **Returns:** `Task` – completes successfully if access is granted.
- **Throws:** An exception (e.g., `UnauthorizedAccessException`) if access is denied.

### `ClearRouteCache`

- **Signature:** `void ClearRouteCache()`
- **Description:** Clears the internal route cache. After calling this method, all cached route resolutions are discarded, and subsequent resolution requests will perform a full lookup.
- **Returns:** `void`

### `GetCachedRouteCount`

- **Signature:** `int GetCachedRouteCount()`
- **Description:** Returns the number of routes currently stored in the internal cache.
- **Returns:** `int` – the count of cached routes.

## Usage

### Example 1: Resolving a Route and Target Service

```csharp
public async Task<GrpcService> ResolveAndGetServiceAsync(
    IRouteResolutionService routeService,
    HttpContext httpContext)
{
    // Resolve the route for the incoming request
    GatewayRoute route = await routeService.ResolveRouteAsync(httpContext);

    // Validate that the caller has access to this route
    await routeService.ValidateRouteAccessAsync(httpContext, route);

    // Resolve the target gRPC service that will handle the request
    GrpcService targetService = await routeService.ResolveTargetServiceAsync(route);

    return targetService;
}
```

### Example 2: Cache Management

```csharp
public void ManageCache(IRouteResolutionService routeService)
{
    // Check current cache size
    int cachedCount = routeService.GetCachedRouteCount();
    Console.WriteLine($"Current cached routes: {cachedCount}");

    // If the cache is too large, clear it
    if (cachedCount > 1000)
    {
        routeService.ClearRouteCache();
        Console.WriteLine("Route cache cleared.");
    }
}
```

## Notes

- **Edge Cases:**  
  - `FindMatchingRouteAsync` returns `null` when no route matches; callers should handle this case gracefully.  
  - `GetRoutesForServiceAsync` may return an empty list if the service exists but has no routes defined.  
  - `ClearRouteCache` and `GetCachedRouteCount` operate on the internal cache; calling `ClearRouteCache` while resolution operations are in progress may cause those operations to perform full lookups instead of using cached results.

- **Thread Safety:**  
  - The asynchronous methods (`ResolveRouteAsync`, `ResolveTargetServiceAsync`, `FindMatchingRouteAsync`, `GetRoutesForServiceAsync`, `ValidateRouteAccessAsync`) are designed to be thread-safe for concurrent invocation.  
  - `ClearRouteCache` and `GetCachedRouteCount` are not guaranteed to be thread-safe when called concurrently with each other or with the resolution methods. External synchronization (e.g., a lock) should be used if these methods are called from multiple threads simultaneously.  
  - The `RouteResolutionService` property is expected to return a stable reference; the underlying service object should be thread-safe or used in a single-threaded context.
