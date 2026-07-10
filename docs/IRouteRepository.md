# IRouteRepository

The `IRouteRepository` interface defines the contract for data access operations related to gRPC gateway routing configurations within the `dotnet-grpc-gateway` project. It provides an asynchronous API for persisting, retrieving, updating, and deleting `GatewayRoute` entities, enabling the gateway to dynamically manage routing rules based on service identifiers, URL patterns, and activation states.

## API

### `RouteRepository`
Represents the concrete implementation type associated with this interface. While the interface defines the contract, this member indicates the standard implementation class name used for dependency injection or direct instantiation in the codebase.

### `GetByIdAsync`
Retrieves a specific gateway route by its unique identifier.
*   **Parameters**: Accepts a unique identifier (typically `string` or `Guid`, depending on the domain model) corresponding to the route.
*   **Return Value**: Returns a `Task<GatewayRoute>`. The result is the matching `GatewayRoute` object if found; otherwise, it may return `null` depending on the implementation strategy for missing records.
*   **Exceptions**: Throws an exception if the underlying data store is unavailable or if the provided identifier format is invalid.

### `GetAllAsync`
Retrieves a complete list of all gateway routes stored in the repository, regardless of their active status.
*   **Parameters**: None.
*   **Return Value**: Returns a `Task<List<GatewayRoute>>` containing all recorded routes. An empty list is returned if no routes exist.
*   **Exceptions**: Throws an exception if the data retrieval operation fails due to connectivity issues or internal storage errors.

### `GetActiveAsync`
Retrieves a list of gateway routes that are currently marked as active.
*   **Parameters**: None.
*   **Return Value**: Returns a `Task<List<GatewayRoute>>` containing only routes where the active flag is set to true.
*   **Exceptions**: Throws an exception if the query execution fails against the data store.

### `GetByServiceIdAsync`
Retrieves all gateway routes associated with a specific backend service identifier.
*   **Parameters**: Accepts the service identifier used to filter the routes.
*   **Return Value**: Returns a `Task<List<GatewayRoute>>` containing all routes mapped to the specified service. Returns an empty list if no matches are found.
*   **Exceptions**: Throws an exception if the service identifier is null/empty (if validation is enforced at this layer) or if the data store query fails.

### `CreateAsync`
Persists a new gateway route entity to the data store.
*   **Parameters**: Accepts a `GatewayRoute` object containing the configuration for the new route.
*   **Return Value**: Returns a `Task<GatewayRoute>`, typically representing the newly created entity with system-generated fields (such as IDs or timestamps) populated.
*   **Exceptions**: Throws an exception if a route with the same unique key already exists, if the input object is invalid, or if the write operation fails.

### `UpdateAsync`
Updates an existing gateway route entity in the data store.
*   **Parameters**: Accepts a `GatewayRoute` object containing the updated configuration. The object must include a valid identifier for the existing record.
*   **Return Value**: Returns a `Task` indicating completion of the operation.
*   **Exceptions**: Throws an exception if the specified route does not exist, if concurrency conflicts occur (e.g., optimistic locking failure), or if the update violates data constraints.

### `DeleteAsync`
Removes a gateway route from the data store.
*   **Parameters**: Accepts the unique identifier of the route to be deleted.
*   **Return Value**: Returns a `Task` indicating completion of the operation.
*   **Exceptions**: Throws an exception if the specified route does not exist or if the deletion is constrained by foreign key relationships or business rules.

### `GetByPatternAsync`
Retrieves gateway routes that match a specific URL pattern or path template.
*   **Parameters**: Accepts a string representing the route pattern to search for.
*   **Return Value**: Returns a `Task<List<GatewayRoute>>` containing routes matching the provided pattern. Matching logic (exact vs. wildcard) is defined by the repository implementation.
*   **Exceptions**: Throws an exception if the pattern is malformed or if the database query fails.

## Usage

### Example 1: Initializing Routes on Startup
The following example demonstrates retrieving all active routes during application startup to populate the in-memory routing table.

```csharp
public class RouteInitializer
{
    private readonly IRouteRepository _routeRepository;

    public RouteInitializer(IRouteRepository routeRepository)
    {
        _routeRepository = routeRepository;
    }

    public async Task InitializeAsync()
    {
        // Fetch only active routes to configure the gateway
        var activeRoutes = await _routeRepository.GetActiveAsync();

        if (activeRoutes.Count == 0)
        {
            // Seed default route if none exist
            var defaultRoute = new GatewayRoute
            {
                Id = "default-route",
                ServiceId = "core-service",
                Pattern = "/api/v1/{**catch-all}",
                IsActive = true
            };
            
            await _routeRepository.CreateAsync(defaultRoute);
        }
        else
        {
            // Apply routes to the gateway configuration
            ApplyToGatewayConfig(activeRoutes);
        }
    }

    private void ApplyToGatewayConfig(List<GatewayRoute> routes)
    {
        // Implementation omitted for brevity
    }
}
```

### Example 2: Dynamic Route Management
This example illustrates handling a user request to update a specific route and then verifying the change by fetching it by service ID.

```csharp
public class RouteManagementService
{
    private readonly IRouteRepository _repository;

    public RouteManagementService(IRouteRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> DeactivateRouteForServiceAsync(string serviceId)
    {
        var routes = await _repository.GetByServiceIdAsync(serviceId);
        
        if (routes == null || routes.Count == 0)
        {
            return false;
        }

        foreach (var route in routes)
        {
            route.IsActive = false;
            await _repository.UpdateAsync(route);
        }

        // Verify updates
        var updatedRoutes = await _repository.GetByServiceIdAsync(serviceId);
        return updatedRoutes.All(r => !r.IsActive);
    }
}
```

## Notes

*   **Thread Safety**: As the methods return `Task` objects and imply asynchronous I/O operations, the implementation is expected to be thread-safe for concurrent read operations. However, concurrent write operations (`CreateAsync`, `UpdateAsync`, `DeleteAsync`) targeting the same entity may require external locking or rely on the underlying data store's concurrency controls to prevent race conditions.
*   **Null Handling**: Consumers should verify whether `GetByIdAsync` returns `null` for missing records or throws a specific exception, as behavior may vary based on the concrete `RouteRepository` implementation.
*   **Pattern Matching**: The `GetByPatternAsync` method's matching logic (e.g., strict equality, regex, or glob matching) is implementation-dependent. Callers should ensure the provided pattern adheres to the expected format to avoid empty results or exceptions.
*   **Entity State**: When using `UpdateAsync`, ensure the `GatewayRoute` object passed contains the correct primary key and reflects the current state of the entity to avoid unintentional data overwrites or concurrency violations.
