// ... (rest of the file remains the same)

## GatewayException

The `GatewayException` class represents a base exception for all gateway-related errors. It provides a standardized way to handle and log errors across the gRPC gateway.

### Example Usage:

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            // Simulate an error
            throw new GatewayException("Something went wrong", "GATEWAY_ERROR", 500);
        }
        catch (GatewayException ex)
        {
            Console.WriteLine($"Error code: {ex.ErrorCode}");
            Console.WriteLine($"HTTP status code: {ex.HttpStatusCode}");
            Console.WriteLine($"Details: {string.Join(", ", ex.Details?.Select(x => $"{x.Key}: {x.Value}"))}");
        }
    }
}
```

## GatewayEvent

`GatewayEvent` is the abstract base class for all events emitted by the gateway. It supplies immutable identifiers (`EventId`), a timestamp (`OccurredAt`), and optional correlation information (`CorrelationId`, `CausedBy`). Specific event types such as `ServiceRegisteredEvent`, `ServiceUnregisteredEvent`, and `RouteAddedEvent` inherit these members and add their own payload data.

### Example Usage

```csharp
using System;
using DotNetGrpcGateway.Events;

class Program
{
    static void Main()
    {
        // Create a service‑registered event
        var ev = new ServiceRegisteredEvent(
            serviceId: 42,
            serviceName: "UserService",
            serviceFullName: "MyApp.Services.UserService",
            host: "localhost",
            port: 5001);

        // Access base members
        Console.WriteLine($"EventId: {ev.EventId}");
        Console.WriteLine($"OccurredAt (UTC): {ev.OccurredAt:u}");

        // Access derived members
        Console.WriteLine($"ServiceId: {ev.ServiceId}");
        Console.WriteLine($"ServiceName: {ev.ServiceName}");
        Console.WriteLine($"ServiceFullName: {ev.ServiceFullName}");
        Console.WriteLine($"Host: {ev.Host}");
        Console.WriteLine($"Port: {ev.Port}");
    }
}
```
