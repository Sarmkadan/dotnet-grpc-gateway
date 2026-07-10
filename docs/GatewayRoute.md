# GatewayRoute

Represents a single route definition within a gRPC gateway. Each `GatewayRoute` instance maps an incoming request pattern to a target service, specifies matching rules, authentication, caching, rate limiting, and optional request/response transformations. This class is typically used to build the routing table that the gateway engine evaluates at runtime.

## API

| Member | Type | Description |
|--------|------|-------------|
| `Id` | `int` | Unique identifier for the route. |
| `Pattern` | `string` | The URI pattern to match against incoming requests. Supports wildcards and placeholders as defined by the gateway’s routing syntax. |
| `TargetServiceId` | `int` | Identifier of the downstream gRPC service that will handle requests matching this route. |
| `Priority` | `int` | Numeric priority used to resolve conflicts when multiple routes match the same request. Higher values take precedence. |
| `MatchType` | `RouteMatchType` | Enum indicating how the `Pattern` should be matched (e.g., exact, prefix, regex). |
| `Description` | `string?` | Optional human-readable description of the route. |
| `Headers` | `Dictionary<string, string>` | A dictionary of header key-value pairs that must be present in the request for the route to match. |
| `Metadata` | `Dictionary<string, string>` | Arbitrary key-value metadata attached to the route, often used for logging or custom processing. |
| `RequiresAuthentication` | `bool` | If `true`, the gateway will enforce authentication before forwarding the request. |
| `AuthorizationPolicy` | `string?` | Name of the authorization policy to apply when `RequiresAuthentication` is `true`. `null` means no specific policy. |
| `RateLimitPerMinute` | `int` | Maximum number of requests allowed per minute for this route. A value of `0` typically disables rate limiting. |
| `EnableCaching` | `bool` | If `true`, the gateway may cache responses for this route. |
| `CacheDurationSeconds` | `int` | Time in seconds that cached responses remain valid. Only meaningful when `EnableCaching` is `true`. |
| `RequestTransformationScript` | `string?` | Path or inline script that transforms the request before forwarding. `null` means no transformation. |
| `ResponseTransformationScript` | `string?` | Path or inline script that transforms the response before returning to the client. `null` means no transformation. |
| `EnableCompression` | `bool` | If `true`, the gateway may compress the response body. |
| `ChannelOptions` | `RouteChannelOptions?` | Optional configuration for the gRPC channel used to communicate with the target service. |
| `CreatedAt` | `DateTime` | Timestamp when the route was created. |
| `ModifiedAt` | `DateTime` | Timestamp when the route was last modified. |
| `IsActive` | `bool` | If `false`, the route is ignored by the gateway. |

All properties are read/write. No property throws exceptions on assignment; validation (if any) is expected to occur at a higher level (e.g., when the route is added to a gateway configuration).

## Usage

### Example 1: Creating and configuring a route

```csharp
var route = new GatewayRoute
{
    Id = 101,
    Pattern = "/api/v1/users/{userId}",
    TargetServiceId = 2,
    Priority = 10,
    MatchType = RouteMatchType.Prefix,
    Description = "User profile endpoint",
    Headers = new Dictionary<string, string>
    {
        ["X-API-Version"] = "1"
    },
    RequiresAuthentication = true,
    AuthorizationPolicy = "user-read",
    RateLimitPerMinute = 100,
    EnableCaching = true,
    CacheDurationSeconds = 60,
    EnableCompression = true,
    IsActive = true,
    CreatedAt = DateTime.UtcNow,
    ModifiedAt = DateTime.UtcNow
};
```

### Example 2: Using a route in a gateway configuration builder

```csharp
public class GatewayConfig
{
    public List<GatewayRoute> Routes { get; set; } = new();

    public void AddDefaultRoutes()
    {
        Routes.Add(new GatewayRoute
        {
            Id = 1,
            Pattern = "/health",
            TargetServiceId = 0,
            Priority = 100,
            MatchType = RouteMatchType.Exact,
            RequiresAuthentication = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        });

        Routes.Add(new GatewayRoute
        {
            Id = 2,
            Pattern = "/api/**",
            TargetServiceId = 1,
            Priority = 50,
            MatchType = RouteMatchType.Wildcard,
            RequiresAuthentication = true,
            RateLimitPerMinute = 500,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        });
    }
}
```

## Notes

- **Thread safety**: `GatewayRoute` is a mutable reference type. Instances are not inherently thread-safe. If a route is shared across multiple threads (e.g., during hot-reload of configuration), external synchronization (e.g., locks or immutable snapshots) is required.
- **Nullable members**: `Description`, `AuthorizationPolicy`, `RequestTransformationScript`, `ResponseTransformationScript`, and `ChannelOptions` may be `null`. Code consuming these properties should check for `null` before use.
- **Default values**: The `Headers` and `Metadata` dictionaries are initially empty. `RateLimitPerMinute` of `0` is typically interpreted as “no limit”. `CacheDurationSeconds` is only relevant when `EnableCaching` is `true`; a value of `0` may disable caching even if `EnableCaching` is `true` (behavior depends on gateway implementation).
- **Timestamps**: `CreatedAt` and `ModifiedAt` are `DateTime` structs; they are not automatically updated by the class. The caller is responsible for setting them appropriately.
- **Pattern matching**: The exact semantics of `Pattern` and `MatchType` are defined by the gateway engine. Empty or malformed patterns may cause runtime errors when the route is evaluated.
- **Priority conflicts**: When two routes have the same `Priority` and both match a request, the outcome is undefined. It is recommended to assign unique priority values or ensure patterns are mutually exclusive.
