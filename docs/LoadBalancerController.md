# LoadBalancerController

The `LoadBalancerController` exposes HTTP endpoints for managing the set of backend service endpoints used by the gRPC‑gateway load‑balancing layer. It allows callers to query the current endpoint registry, add or remove endpoints, update their health status, and inspect or change the load‑balancing strategy in use.

## API

### GetEndpoints
- **Purpose**: Retrieves the current list of registered service endpoints.
- **Parameters**: None.
- **Return Value**: `ActionResult<IReadOnlyList<ServiceEndpoint>>`. On success, the payload contains an immutable list of `ServiceEndpoint` objects representing all known backends. On failure, the result contains an appropriate HTTP error status (e.g., 500 Internal Server Error) with an error message.
- **Throws**: Does not throw exceptions directly; any unexpected error is caught and transformed into a non‑2xx `ActionResult`.

### RegisterEndpoint
- **Purpose**: Adds a new service endpoint to the load‑balancer’s registry.
- **Parameters**: 
  - `endpoint` (`ServiceEndpoint`) – the endpoint to register; must not be null and must contain a valid address and port.
- **Return Value**: `ActionResult`. Returns 200 OK if the endpoint was added successfully, 400 Bad Request if the supplied endpoint is invalid, or 409 Conflict if an endpoint with the same identifier already exists.
- **Throws**: Exceptions are caught and returned as error responses; no exceptions propagate outward.

### DeregisterEndpoint
- **Purpose**: Removes an existing service endpoint from the registry.
- **Parameters**: 
  - `endpointId` (`string`) – the unique identifier of the endpoint to deregister; must not be null or empty.
- **Return Value**: `ActionResult`. Returns 200 OK on successful removal, 404 Not Found if no endpoint with the given identifier exists, or 400 Bad Request for an invalid identifier.
- **Throws**: Errors are converted to appropriate `ActionResult` instances.

### UpdateEndpointHealth
- **Purpose**: Updates the health status of a registered endpoint.
- **Parameters**: 
  - `endpointId` (`string`) – identifier of the endpoint whose health is being updated.
  - `isHealthy` (`bool`) – true to mark the endpoint as healthy, false to mark it as unhealthy.
- **Return Value**: `ActionResult`. Returns 200 OK when the health status is updated, 404 Not Found if the endpoint does not exist, or 400 Bad Request if the identifier is invalid.
- **Throws**: Any internal errors are returned as non‑successful `ActionResult` objects.

### GetStrategy
- **Purpose**: Obtains the current load‑balancing strategy in use.
- **Parameters**: None.
- **Return Value**: `ActionResult<string>` (or a strongly‑typed enum wrapper, depending on implementation). On success, the payload contains the name of the active strategy (e.g., “RoundRobin”, “LeastConnections”). Errors are reported via the action result.
- **Throws**: Exceptions are caught and mapped to error responses.

### SetStrategy
- **Purpose**: Changes the load‑balancing strategy used by the controller.
- **Parameters**: 
  - `strategy` (`string`) – the name of the strategy to apply; must correspond to a strategy supported by the underlying load‑balancer.
- **Return Value**: `ActionResult`. Returns 200 OK if the strategy was changed successfully, 400 Bad Request if the strategy name is unknown, or 500 Internal Server Error if the change could not be applied.
- **Throws**: Errors are transformed into appropriate `ActionResult` results; no exceptions leak out of the method.

## Usage

```csharp
// Example 1: Retrieve all registered endpoints
var endpointsResult = await loadBalancerController.GetEndpoints();
if (endpointsResult.Result is OkObjectResult ok)
{
    var endpoints = ok.Value as IReadOnlyList<ServiceEndpoint>;
    foreach (var ep in endpoints)
    {
        Console.WriteLine($"{ep.Address}:{ep.Port}");
    }
}
else
{
    // Handle error (e.g., log endpointsResult.Result)
}
```

```csharp
// Example 2: Register a new endpoint and then update its health
var newEndpoint = new ServiceEndpoint { Id = "svc3", Address = "10.0.0.5", Port = 50051 };

var registerResult = await loadBalancerController.RegisterEndpoint(newEndpoint);
if (registerResult.Result is OkResult)
{
    Console.WriteLine("Endpoint registered.");
}
else
{
    // Handle registration failure
    return;
}

var healthResult = await loadBalancerController.UpdateEndpointHealth(newEndpoint.Id, true);
if (healthResult.Result is OkResult)
{
    Console.WriteLine("Endpoint marked healthy.");
}
else
{
    // Handle health‑update failure
}
```

## Notes

- All methods are idempotent where applicable: calling `RegisterEndpoint` with an already‑registered identifier will return a conflict response rather than duplicating the entry; calling `DeregisterEndpoint` on a non‑existent identifier yields a not‑found response.
- Null or invalid arguments (e.g., null `ServiceEndpoint`, empty `endpointId`) result in a 400 Bad Request response; the controller does not throw `ArgumentNullException`.
- The controller itself holds no mutable state; it delegates to an underlying load‑balancer service. Consequently, the controller is thread‑safe for concurrent HTTP requests, but callers must ensure that the backing service provides its own synchronization if required.
- Strategy changes via `SetStrategy` take effect immediately for subsequent load‑balancing decisions; endpoints already in flight are not affected.
- Health updates (`UpdateEndpointHealth`) are reflected instantly in the load‑balancer’s routing table; marking an endpoint unhealthy will prevent new requests from being sent to it, though existing in‑flight calls may continue until they complete.
