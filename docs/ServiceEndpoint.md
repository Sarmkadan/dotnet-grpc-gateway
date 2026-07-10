# ServiceEndpoint

Represents a single endpoint of a gRPC service within the gateway’s load‑balancing and health‑tracking infrastructure. Each instance holds the network address, TLS configuration, current health status, weight for weighted routing, and runtime metrics such as request count and average response time. The class is designed to be used by the gateway’s internal service discovery and load‑balancing logic.

## API

### `public int Id`
Unique identifier for this endpoint within the gateway’s data store.  
**Returns:** The endpoint’s numeric ID.  
**Throws:** Never.

### `public int ServiceId`
Identifier of the parent service to which this endpoint belongs.  
**Returns:** The service ID.  
**Throws:** Never.

### `public string Host`
Hostname or IP address of the endpoint.  
**Returns:** The host string.  
**Throws:** Never.

### `public int Port`
TCP port on which the gRPC service is listening.  
**Returns:** The port number.  
**Throws:** Never.

### `public bool UseTls`
Indicates whether the endpoint requires TLS (HTTPS/gRPC‑TLS).  
**Returns:** `true` if TLS is enabled; otherwise `false`.  
**Throws:** Never.

### `public bool IsHealthy`
Current health status of the endpoint as determined by the gateway’s health‑check mechanism.  
**Returns:** `true` if the endpoint is considered healthy; otherwise `false`.  
**Throws:** Never.

### `public int Weight`
Weight assigned to this endpoint for weighted round‑robin or weighted random load balancing. Higher values increase the proportion of traffic directed to this endpoint.  
**Returns:** The weight value (non‑negative).  
**Throws:** Never.

### `public long TotalRequestsHandled`
Total number of requests that have been routed to this endpoint since the gateway started tracking it.  
**Returns:** The cumulative request count.  
**Throws:** Never.

### `public double AverageResponseTimeMs`
Rolling average response time of requests handled by this endpoint, in milliseconds.  
**Returns:** The average response time.  
**Throws:** Never.

### `public DateTime RegisteredAt`
Timestamp (UTC) when this endpoint was first registered with the gateway.  
**Returns:** The registration time.  
**Throws:** Never.

### `public DateTime LastUsedAt`
Timestamp (UTC) of the most recent request routed to this endpoint.  
**Returns:** The last‑used time.  
**Throws:** Never.

### `public string GetUri`
Computed URI string for the endpoint, built from `Host`, `Port`, and `UseTls`. For example, `https://host:port` when TLS is enabled, otherwise `http://host:port`.  
**Returns:** The fully qualified URI string.  
**Throws:** Never.

### `public void RecordRequest()`
Records a single request handled by this endpoint. This method updates `TotalRequestsHandled`, `LastUsedAt`, and the internal rolling average used by `AverageResponseTimeMs`.  
**Parameters:** None.  
**Returns:** Nothing.  
**Throws:** Never.

## Usage

The following example demonstrates creating a `ServiceEndpoint` instance and recording a request.

```csharp
var endpoint = new ServiceEndpoint
{
    Id = 1,
    ServiceId = 42,
    Host = "grpc.example.com",
    Port = 443,
    UseTls = true,
    IsHealthy = true,
    Weight = 10,
    RegisteredAt = DateTime.UtcNow,
    LastUsedAt = DateTime.UtcNow
};

// Simulate handling a request
endpoint.RecordRequest();
Console.WriteLine($"URI: {endpoint.GetUri}");
Console.WriteLine($"Total requests: {endpoint.TotalRequestsHandled}");
```

A more realistic usage inside a load‑balancing loop:

```csharp
public void RouteRequest(ServiceEndpoint endpoint)
{
    if (!endpoint.IsHealthy)
    {
        // Skip unhealthy endpoints
        return;
    }

    // Forward the gRPC call to the endpoint's URI
    var uri = endpoint.GetUri;
    // ... actual gRPC call ...

    // Record the request for metrics
    endpoint.RecordRequest();
}
```

## Notes

- **Thread safety:** The `ServiceEndpoint` class does not provide built‑in synchronization. If instances are shared across multiple threads (e.g., in a concurrent load‑balancer), external locking or an immutable snapshot pattern should be used to avoid race conditions on `RecordRequest` and the mutable fields (`IsHealthy`, `Weight`, `TotalRequestsHandled`, `AverageResponseTimeMs`, `LastUsedAt`).
- **Edge cases:**  
  - `Host` may be an empty string or `null`; `GetUri` will still produce a URI (e.g., `http://:8080`). Consumers should validate the host before using the URI.  
  - `Weight` can be zero, which effectively excludes the endpoint from weighted load balancing unless the algorithm explicitly handles zero weights.  
  - `AverageResponseTimeMs` is updated only when `RecordRequest` is called; it will remain at its default value (`0.0`) until the first call.  
  - `TotalRequestsHandled` can overflow after `long.MaxValue` requests (approximately 9.2 × 10¹⁸). In practice this is unlikely, but consumers should be aware of the theoretical limit.  
- **Initial state:** After construction, `TotalRequestsHandled` is `0`, `AverageResponseTimeMs` is `0.0`, and `LastUsedAt` retains whatever value was assigned (or the default `DateTime.MinValue` if not set).
