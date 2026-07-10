# GatewayEvent

The `GatewayEvent` type serves as the foundational abstraction for representing state changes and lifecycle occurrences within the `dotnet-grpc-gateway` infrastructure. It encapsulates metadata common to all gateway activities, including temporal data, unique identification, and service context, while specific derived types handle distinct scenarios such as service registration, unregistration, and route configuration updates.

## API

The following members are exposed by the `GatewayEvent` type and its concrete implementations found in the provided signature list.

### Properties

#### `EventId`
*   **Type:** `public string`
*   **Purpose:** Provides a unique identifier for the specific event instance. This value is used for tracing and deduplication within the gateway's event log.
*   **Returns:** A non-null string representing the event's unique ID.
*   **Throws:** Never.

#### `OccurredAt`
*   **Type:** `public DateTime`
*   **Purpose:** Records the precise timestamp when the event was generated.
*   **Returns:** A `DateTime` structure indicating the occurrence time.
*   **Throws:** Never.

#### `CorrelationId`
*   **Type:** `public string?`
*   **Purpose:** Links this event to a broader distributed transaction or request chain. If the event is not part of a correlated operation, this value is `null`.
*   **Returns:** A string containing the correlation identifier, or `null`.
*   **Throws:** Never.

#### `CausedBy`
*   **Type:** `public string?`
*   **Purpose:** Identifies the antecedent event or actor that triggered this specific event. Used for establishing causal chains in event sourcing scenarios.
*   **Returns:** A string describing the cause, or `null` if the event was spontaneous or system-initiated without a direct predecessor.
*   **Throws:** Never.

#### `ServiceId`
*   **Type:** `public int`
*   **Purpose:** Represents the numeric identifier of the service associated with the event. In base contexts, this reflects the primary service involved.
*   **Returns:** An integer representing the service ID.
*   **Throws:** Never.

#### `ServiceName`
*   **Type:** `public string`
*   **Purpose:** Contains the logical name of the service associated with the event.
*   **Returns:** A non-null string representing the service name.
*   **Throws:** Never.

#### `ServiceFullName`
*   **Type:** `public string`
*   **Purpose:** Provides the fully qualified name of the service, typically including namespace or domain context.
*   **Returns:** A non-null string representing the full service name.
*   **Throws:** Never.

#### `Host`
*   **Type:** `public string`
*   **Purpose:** Specifies the network hostname or IP address where the service involved in the event is located.
*   **Returns:** A non-null string representing the host address.
*   **Throws:** Never.

#### `Port`
*   **Type:** `public int`
*   **Purpose:** Indicates the network port number on which the service is listening or was registered.
*   **Returns:** An integer representing the port number.
*   **Throws:** Never.

#### `RouteId`
*   **Type:** `public int`
*   **Purpose:** Specific to routing events; identifies the unique ID of the route configuration being modified.
*   **Returns:** An integer representing the route ID.
*   **Throws:** Never.

#### `Pattern`
*   **Type:** `public string`
*   **Purpose:** Specific to routing events; defines the URL pattern or gRPC method path associated with the route.
*   **Returns:** A non-null string representing the route pattern.
*   **Throws:** Never.

#### `TargetServiceId`
*   **Type:** `public int`
*   **Purpose:** Specific to routing events; identifies the backend service ID to which the route directs traffic.
*   **Returns:** An integer representing the target service ID.
*   **Throws:** Never.

### Constructors

#### `ServiceRegisteredEvent()`
*   **Purpose:** Initializes a new instance of the `ServiceRegisteredEvent` class with default values.
*   **Parameters:** None.
*   **Throws:** Never.

#### `ServiceUnregisteredEvent()`
*   **Purpose:** Initializes a new instance of the `ServiceUnregisteredEvent` class with default values.
*   **Parameters:** None.
*   **Throws:** Never.

#### `ServiceUnregisteredEvent(int serviceId, string serviceName)`
*   **Purpose:** Initializes a new instance of the `ServiceUnregisteredEvent` class with specific service identification details.
*   **Parameters:**
    *   `serviceId`: The integer ID of the service being unregistered.
    *   `serviceName`: The logical name of the service being unregistered.
*   **Throws:** May throw `ArgumentNullException` if `serviceName` is null (standard C# convention for string arguments), though specific implementation validation depends on the base constructor logic.

#### `RouteAddedEvent()`
*   **Purpose:** Initializes a new instance of the `RouteAddedEvent` class with default values.
*   **Parameters:** None.
*   **Throws:** Never.

#### `RouteAddedEvent(int routeId, string pattern, int targetServiceId)`
*   **Purpose:** Initializes a new instance of the `RouteAddedEvent` class with specific routing configuration details.
*   **Parameters:**
    *   `routeId`: The unique identifier for the new route.
    *   `pattern`: The URL or gRPC pattern for the route.
    *   `targetServiceId`: The ID of the service that will handle requests for this route.
*   **Throws:** May throw `ArgumentNullException` if `pattern` is null.

## Usage

### Example 1: Handling Service Registration
This example demonstrates creating a `ServiceRegisteredEvent` and accessing its core properties to log a new service discovery.

```csharp
using System;
using DotNetGrpcGateway.Events; // Hypothetical namespace based on project name

public class ServiceDiscoveryHandler
{
    public void OnServiceDiscovered()
    {
        // Initialize the event using the parameterless constructor
        var registrationEvent = new ServiceRegisteredEvent();
        
        // In a real scenario, properties like ServiceId and Host 
        // would be populated by the discovery mechanism before usage.
        // Assuming population occurred internally or via setter access not shown in signature:
        
        Console.WriteLine($"Service '{registrationEvent.ServiceName}' registered at {registrationEvent.Host}:{registrationEvent.Port}");
        Console.WriteLine($"Event ID: {registrationEvent.EventId}");
        Console.WriteLine($"Occurred: {registrationEvent.OccurredAt:O}");
        
        if (registrationEvent.CorrelationId != null)
        {
            Console.WriteLine($"Correlated to: {registrationEvent.CorrelationId}");
        }
    }
}
```

### Example 2: Processing Route Configuration Changes
This example illustrates the instantiation of a `RouteAddedEvent` with specific parameters and extracting routing details.

```csharp
using System;
using DotNetGrpcGateway.Events;

public class RouteConfigurator
{
    public void ConfigureNewRoute()
    {
        int newRouteId = 1024;
        string urlPattern = "/api/v1/orders";
        int backendServiceId = 55;

        // Initialize the event with specific routing data
        var routeEvent = new RouteAddedEvent(newRouteId, urlPattern, backendServiceId);

        // Access specific routing properties
        Console.WriteLine($"Route added: {routeEvent.Pattern}");
        Console.WriteLine($"Target Service ID: {routeEvent.TargetServiceId}");
        Console.WriteLine($"Route Internal ID: {routeEvent.RouteId}");
        
        // Access inherited gateway context
        Console.WriteLine($"Triggered by: {routeEvent.CausedBy ?? "System"}");
    }
}
```

## Notes

*   **Immutability and Initialization:** The provided signatures indicate that critical data for specific event types (e.g., `ServiceUnregisteredEvent`, `RouteAddedEvent`) is primarily injected via constructors. However, the presence of a parameterless constructor alongside public settable properties (implied by the property list without `init` or `readonly` modifiers in the raw list) suggests these objects may be mutable after creation or intended for object-initializer syntax. Care should be taken to ensure all required fields are populated before publishing the event to avoid inconsistent state.
*   **Nullability:** Properties `CorrelationId` and `CausedBy` are explicitly nullable (`string?`). Consumers must perform null checks before accessing these members to avoid `NullReferenceException`. Conversely, properties like `ServiceName`, `Host`, and `Pattern` are defined as non-nullable `string`, implying they must always contain a value; initialization logic should guarantee this invariant.
*   **Thread Safety:** The type exposes mutable public properties and standard constructors without evidence of internal synchronization mechanisms (e.g., locks, concurrent collections). Therefore, instances of `GatewayEvent` and its derivatives are **not thread-safe** for mutation. If an event instance is shared across threads, it should be treated as immutable after construction, or external synchronization must be applied during modification.
*   **Inheritance Structure:** The signature list implies a hierarchy where `ServiceRegisteredEvent`, `ServiceUnregisteredEvent`, and `RouteAddedEvent` share common properties defined in `GatewayEvent` (such as `EventId`, `OccurredAt`, `Host`, `Port`). The repetition of `ServiceId` and `ServiceName` in the raw list suggests these properties exist on the base class and are potentially shadowed or simply listed redundantly in the reflection output; logically, they represent the context of the service involved in the specific event type.
