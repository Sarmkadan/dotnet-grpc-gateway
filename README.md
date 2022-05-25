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

## IHttpClientProvider

The `IHttpClientProvider` interface manages HTTP client creation and lifecycle, standardizing configuration for timeouts, retries, headers, and connection pooling. It ensures consistent HTTP client behavior across services.

### Example Usage

```csharp
using System;
using System.Collections.Generic;
using DotNetGrpcGateway.Integration;

class Program
{
    static void Main()
    {
        // Configure HTTP client options
        var options = new HttpClientOptions
        {
            Timeout = TimeSpan.FromSeconds(10),
            MaxRetries = 2,
            AllowAutoRedirect = false,
            MaxConnectionsPerServer = 20,
            DefaultHeaders = new Dictionary<string, string>
            {
                { "X-Service-Name", "UserService" },
                { "Accept", "application/json" }
            }
        };

        // Create HTTP client provider (mocked for example)
        var provider = new HttpClientProvider(new MockHttpClientFactory(), new NullLogger<HttpClientProvider>());

        // Create and use client
        var client = provider.CreateClient("UserServiceClient", options);
        var response = client.GetAsync("https://api.example.com/data").Result;

        // Retrieve existing client
        var existingClient = provider.GetClient("UserServiceClient");

        // Clean up
        provider.RemoveClient("UserServiceClient");
    }
}

// Mock implementation for example purposes
public class MockHttpClientFactory : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => new HttpClient();
}

public class NullLogger<T> : ILogger<T>
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    public bool IsEnabled(LogLevel logLevel) => false;
    public IDisposable BeginScope<TState>(TState state) => null!;
}
```

## IWebhookService

`IWebhookService` is responsible for sending webhooks to external endpoints, handling retries, timeout management, and failure tracking.

### Example Usage

```csharp
using System;
using System.Threading.Tasks;
using DotNetGrpcGateway.Integration;

class Program
{
    static async Task Main()
    {
        var webhookService = new WebhookService(new HttpClient(), new Logger<WebhookService>());
        var result = await webhookService.SendWebhookAsync("https://example.com/webhook", new { foo = "bar" });
        Console.WriteLine($"Webhook sent successfully: {result.Success}");
        Console.WriteLine($"Status code: {result.StatusCode}");
        Console.WriteLine($"Message: {result.Message}");

        var history = await webhookService.GetDeliveryHistoryAsync("https://example.com/webhook");
        foreach (var delivery in history)
        {
            Console.WriteLine($"Delivery at: {delivery.DeliveredAt}");
            Console.WriteLine($"Success: {delivery.Success}");
            Console.WriteLine($"Status code: {delivery.StatusCode}");
            Console.WriteLine($"Error message: {delivery.ErrorMessage}");
        }
    }
}
```
```