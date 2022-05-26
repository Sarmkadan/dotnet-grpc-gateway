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

## GatewayStatistics

`GatewayStatistics` provides aggregated metrics and analytics for the gRPC gateway and its connected services. It tracks request volumes, response times, error rates, data throughput, connection statistics, service health status, and cache performance across all gateway operations.

### Example Usage:

```csharp
using System;
using System.Collections.Generic;
using DotNetGrpcGateway.Domain;

class Program
{
    static void Main()
    {
        // Create statistics instance
        var stats = new GatewayStatistics
        {
            Id = 1,
            StatisticsDate = DateTime.UtcNow.Date,
            TotalRequestsProcessed = 15000,
            SuccessfulRequests = 14850,
            FailedRequests = 150,
            SuccessRate = 99.0,
            AverageResponseTimeMs = 45.2,
            MinResponseTimeMs = 5.1,
            MaxResponseTimeMs = 245.8,
            TotalDataProcessedBytes = 2500000,
            ActiveConnections = 42,
            PeakConnections = 120,
            RequestsByService = new Dictionary<string, long>
            {
                {"UserService", 8500},
                {"OrderService", 4200},
                {"ProductService", 2300}
            },
            RequestsByMethod = new Dictionary<string, long>
            {
                {"GetUser", 5000},
                {"CreateOrder", 2000},
                {"SearchProducts", 3000}
            },
            ErrorsByType = new Dictionary<string, int>
            {
                {"Timeout", 80},
                {"Validation", 45},
                {"Connection", 25}
            },
            HealthyServices = 8,
            UnhealthyServices = 1,
            TotalServices = 9,
            CacheHitRate = 87.5,
            CacheHits = 1750,
            CacheMisses = 250,
            RecordedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Display key metrics
        Console.WriteLine($"Statistics for {stats.StatisticsDate:yyyy-MM-dd}");
        Console.WriteLine($"Total requests: {stats.TotalRequestsProcessed:N0}");
        Console.WriteLine($"Success rate: {stats.SuccessRate:F1}%");
        Console.WriteLine($"Avg response time: {stats.AverageResponseTimeMs:F1}ms");
        Console.WriteLine($"Data processed: {stats.TotalDataProcessedBytes:N0} bytes");
        Console.WriteLine($"Active connections: {stats.ActiveConnections}");
        Console.WriteLine($"Peak connections: {stats.PeakConnections}");
        Console.WriteLine($"Healthy services: {stats.HealthyServices}/{stats.TotalServices}");
        Console.WriteLine($"Cache hit rate: {stats.CacheHitRate:F1}%");
    }
}
```

## HttpContextExtensions

`HttpContextExtensions` provides extension methods for `HttpContext` that simplify common HTTP request operations including IP address extraction, header access, authorization token parsing, request identification, and gRPC request detection.

### Example Usage:

```csharp
using System;
using Microsoft.AspNetCore.Http;
using DotNetGrpcGateway.Extensions;

class Program
{
    static void Main()
    {
        // Create a mock HttpContext
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Forwarded-For"] = "192.168.1.100, 10.0.0.1";
        context.Request.Headers["Authorization"] = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
        context.Request.Headers["X-Request-ID"] = "abc-123-def";
        context.Request.ContentType = "application/grpc";
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.100");
        
        // Get client IP address (handles proxies via X-Forwarded-For)
        string clientIp = context.GetClientIpAddress();
        Console.WriteLine($"Client IP: {clientIp}"); // Output: "192.168.1.100"
        
        // Get a specific header
        string? userAgent = context.GetHeader("User-Agent");
        Console.WriteLine($"User-Agent: {userAgent}");
        
        // Get authorization token
        string? token = context.GetAuthorizationToken();
        Console.WriteLine($"Token: {token}"); // Output: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
        
        // Get request ID
        string requestId = context.GetRequestId();
        Console.WriteLine($"Request ID: {requestId}"); // Output: "abc-123-def"
        
        // Check if it's a gRPC request
        bool isGrpc = context.IsGrpcRequest();
        Console.WriteLine($"Is gRPC request: {isGrpc}"); // Output: True
        
        // Check if it's a gRPC-Web request
        bool isGrpcWeb = context.IsGrpcWebRequest();
        Console.WriteLine($"Is gRPC-Web request: {isGrpcWeb}"); // Output: False
    }
}
```

## StringExtensions

`StringExtensions` provides a set of useful extension methods for string manipulation and validation. It includes methods for generating SHA256 hashes, creating URL-safe slugs, truncating strings, validating IP addresses, and pattern matching with wildcards.

### Example Usage

```csharp
using System;
using DotNetGrpcGateway.Extensions;

class Program
{
    static void Main()
    {
        string text = "Hello World 123!";
        
        // Generate SHA256 hash
        string hash = text.ToSha256Hash();
        Console.WriteLine($"SHA256 Hash: {hash}");
        
        // Convert to URL-safe slug
        string slug = text.ToSlug();
        Console.WriteLine($"Slug: {slug}");
        
        // Truncate string
        string truncated = text.Truncate(10);
        Console.WriteLine($"Truncated: {truncated}");
        
        // Validate IP address
        string ipAddress = "192.168.1.1";
        bool isValidIp = ipAddress.IsValidIpAddress();
        Console.WriteLine($"Is valid IP: {isValidIp}");
        
        // Match pattern with wildcards
        string pattern = "test-*";
        string testValue = "test-123";
        bool matches = testValue.MatchesPattern(pattern);
        Console.WriteLine($"Matches pattern: {matches}");
    }
}
```

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