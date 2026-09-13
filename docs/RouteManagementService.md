# RouteManagementService

`RouteManagementService` provides read-only route lookup, wildcard matching, conflict detection, and configuration validation for gateway routes. It implements `IRouteManagementService` and reads the current route set through `IRouteRepository`; it does not create, update, or delete routes.

## Responsibilities

- Retrieve all routes for a target service and order them from highest to lowest priority.
- Select the highest-priority active route whose pattern matches a request path.
- Identify existing patterns that overlap a proposed pattern.
- Validate a route's pattern, priority, rate limit, cache duration, and pattern uniqueness.
- Log lookup, conflict, validation, and repository failures without logging unmasked input values.

Wildcard matching is case-insensitive, matches the complete input, and treats `*` as zero or more characters. For example, `UserService.*` matches both `UserService.GetUser` and `userservice.ListUsers`.

## Construction and registration

```csharp
public RouteManagementService(
    IRouteRepository routeRepository,
    IEventPublisher eventPublisher,
    ILogger<RouteManagementService> logger)
```

All three dependencies are required; passing `null` for any dependency throws `ArgumentNullException`. The event publisher is a constructor dependency but the current route-management operations do not publish events.

Register the service behind its interface with dependency injection:

```csharp
using DotNetGrpcGateway.Services;

services.AddScoped<IRouteManagementService, RouteManagementService>();
```

`IRouteRepository`, `IEventPublisher`, and logging must also be registered. The gateway's built-in service registration already registers `RouteManagementService` as scoped.

## Public API

### `GetRoutesByServiceAsync`

```csharp
Task<List<GatewayRoute>> GetRoutesByServiceAsync(
    int serviceId,
    CancellationToken cancellationToken = default)
```

Loads all routes, keeps routes whose `TargetServiceId` equals `serviceId`, and returns them ordered by descending `Priority`. This method does not filter inactive routes or validate the service ID. It returns an empty list if there are no matches or if repository access fails.

### `FindMatchingRouteAsync`

```csharp
Task<GatewayRoute?> FindMatchingRouteAsync(
    string path,
    CancellationToken cancellationToken = default)
```

Loads all routes and returns the highest-priority active route for which `path` matches the route's wildcard `Pattern`. It returns `null` when `path` is `null` or empty, no active route matches, or repository access fails. An empty path is rejected before the repository is queried.

### `GetConflictingRoutesAsync`

```csharp
Task<List<GatewayRoute>> GetConflictingRoutesAsync(
    string pattern,
    CancellationToken cancellationToken = default)
```

Returns routes for which either the supplied pattern matches the stored pattern or the stored pattern matches the supplied pattern. Routes whose pattern is exactly equal to the supplied string are excluded. Conflict detection considers both active and inactive routes.

The method returns an empty list when `pattern` is `null` or empty, no conflicts exist, or repository access fails. An empty pattern is rejected before the repository is queried.

### `ValidateRouteAsync`

```csharp
Task<bool> ValidateRouteAsync(
    GatewayRoute route,
    CancellationToken cancellationToken = default)
```

Returns `true` only when all of the following conditions hold:

- `Pattern` is not `null`, empty, or whitespace.
- `Priority` is between `0` and `1000`, inclusive.
- `RateLimitPerMinute` is zero or greater.
- When caching is enabled, `CacheDurationSeconds` is zero or greater.
- No other stored route has the same case-sensitive `Pattern`. A route with the same `Id` is ignored so an existing route can validate itself during an update.

Passing a `null` route throws `ArgumentNullException`. Other validation or repository errors are logged and produce `false`. This validation is narrower than `GatewayRoute.Validate()`; notably, it does not validate `TargetServiceId`, and it allows a rate limit of zero.

All methods pass the supplied `CancellationToken` to `IRouteRepository.GetAllAsync`. Because repository exceptions are handled by the service, a cancellation surfaced by the repository produces the same fallback result as another repository failure.

## Usage examples

### Resolve the best route for a request

```csharp
using DotNetGrpcGateway.Services;

public sealed class RouteResolver
{
    private readonly IRouteManagementService _routes;

    public RouteResolver(IRouteManagementService routes) => _routes = routes;

    public async Task<int?> ResolveServiceAsync(
        string requestPath,
        CancellationToken cancellationToken)
    {
        var route = await _routes.FindMatchingRouteAsync(
            requestPath,
            cancellationToken);

        return route?.TargetServiceId;
    }
}
```

Given active routes with patterns `UserService.*` at priority `100` and `*.GetUser` at priority `300`, resolving `UserService.GetUser` selects the second route because it has the higher priority.

### Validate a route and inspect conflicts before saving

```csharp
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Infrastructure;
using DotNetGrpcGateway.Services;

public sealed class RouteRegistrar
{
    private readonly IRouteManagementService _routeManagement;
    private readonly IRouteRepository _routeRepository;

    public RouteRegistrar(
        IRouteManagementService routeManagement,
        IRouteRepository routeRepository)
    {
        _routeManagement = routeManagement;
        _routeRepository = routeRepository;
    }

    public async Task<bool> TryAddRouteAsync(
        GatewayRoute candidate,
        CancellationToken cancellationToken)
    {
        if (!await _routeManagement.ValidateRouteAsync(candidate, cancellationToken))
            return false;

        var conflicts = await _routeManagement.GetConflictingRoutesAsync(
            candidate.Pattern,
            cancellationToken);

        if (conflicts.Count != 0)
            return false;

        await _routeRepository.CreateAsync(candidate, cancellationToken);
        return true;
    }
}
```

Validation and conflict detection do not reserve or persist a route. If concurrent writers are possible, enforce uniqueness in the persistence layer as well.

### List a service's routes in evaluation order

```csharp
var serviceRoutes = await routeManagement.GetRoutesByServiceAsync(
    serviceId: 42,
    cancellationToken);

foreach (var route in serviceRoutes)
    Console.WriteLine($"{route.Priority}: {route.Pattern} (active: {route.IsActive})");
```
