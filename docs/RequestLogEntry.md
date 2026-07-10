# RequestLogEntry

The `RequestLogEntry` class serves as a structured data transfer object representing a single logged request within the `dotnet-grpc-gateway` pipeline. It captures comprehensive metadata regarding the lifecycle of a gRPC or HTTP request, including timing, routing details, payload sizes, status outcomes, and diagnostic information such as correlation IDs and error messages. This type is primarily utilized for auditing, monitoring, and debugging gateway traffic by providing a immutable snapshot of request execution context.

## API

The following members constitute the public surface area of the `RequestLogEntry` type.

### `Id`
```csharp
public Guid Id { get; set; }
```
A globally unique identifier assigned to this specific log entry. It is used to correlate log records across distributed systems or within internal tracing mechanisms.

### `Timestamp`
```csharp
public DateTime Timestamp { get; set; }
```
The precise date and time when the request was processed or logged. This value typically reflects the UTC time of the event completion.

### `LogLevel`
```csharp
public string? LogLevel { get; set; }
```
Indicates the severity level of the log entry (e.g., "Information", "Warning", "Error"). This property is nullable and may be null if the level is undefined or inherited from a default context.

### `Message`
```csharp
public string? Message { get; set; }
```
A human-readable description of the event or the primary log message associated with the request. This property is nullable.

### `RequestId`
```csharp
public string? RequestId { get; set; }
```
The correlation ID extracted from the incoming request headers (often `X-Request-Id`), used to trace the request across upstream and downstream services. This property is nullable.

### `ServiceName`
```csharp
public string? ServiceName { get; set; }
```
The name of the gRPC service being invoked. This property is nullable if the service name could not be resolved.

### `MethodName`
```csharp
public string? MethodName { get; set; }
```
The specific method name within the gRPC service that was called. This property is nullable.

### `Method`
```csharp
public string Method { get; set; }
```
The HTTP verb used for the request (e.g., "GET", "POST"). Unlike other string properties, this is non-nullable and always contains a value.

### `Path`
```csharp
public string Path { get; set; }
```
The relative path of the requested resource. This is non-nullable.

### `GrpcMethod`
```csharp
public string GrpcMethod { get; set; }
```
The fully qualified gRPC method name in the format `/ServiceName/MethodName`. This is non-nullable.

### `HttpStatusCode`
```csharp
public int HttpStatusCode { get; set; }
```
The HTTP status code returned to the client (e.g., 200, 404, 500).

### `DurationMs`
```csharp
public long DurationMs { get; set; }
```
The total time taken to process the request, measured in milliseconds.

### `ClientIp`
```csharp
public string? ClientIp { get; set; }
```
The IP address of the originating client. This property is nullable if the IP address cannot be determined.

### `UpstreamAddress`
```csharp
public string? UpstreamAddress { get; set; }
```
The address of the upstream backend service that handled the request. This property is nullable.

### `RequestHeaders`
```csharp
public Dictionary<string, string> RequestHeaders { get; set; }
```
A collection of key-value pairs representing the HTTP headers sent with the request. This dictionary is non-nullable but may be empty.

### `RequestSizeBytes`
```csharp
public long RequestSizeBytes { get; set; }
```
The size of the request body in bytes.

### `ResponseSizeBytes`
```csharp
public long ResponseSizeBytes { get; set; }
```
The size of the response body in bytes.

### `ErrorMessage`
```csharp
public string? ErrorMessage { get; set; }
```
Contains the error message or stack trace if the request resulted in a failure. This property is nullable and typically populated only when `IsSuccessful` is false.

### `IsSuccessful`
```csharp
public bool IsSuccessful { get; set; }
```
A boolean flag indicating whether the request completed successfully (typically corresponding to HTTP 2xx status codes).

### `CacheHit`
```csharp
public bool CacheHit { get; set; }
```
Indicates whether the response was served from a cache layer rather than being processed by the upstream service.

## Usage

### Example 1: Inspecting a Log Entry for Diagnostics
This example demonstrates how to inspect a `RequestLogEntry` instance to determine if a request failed and to retrieve relevant diagnostic details.

```csharp
public void DiagnoseRequest(RequestLogEntry entry)
{
    if (!entry.IsSuccessful)
    {
        Console.WriteLine($"Request {entry.Id} failed at {entry.Timestamp}");
        Console.WriteLine($"Service: {entry.ServiceName}, Method: {entry.MethodName}");
        Console.WriteLine($"Status: {entry.HttpStatusCode}, Duration: {entry.DurationMs}ms");
        
        if (!string.IsNullOrEmpty(entry.ErrorMessage))
        {
            Console.WriteLine($"Error Details: {entry.ErrorMessage}");
        }
    }
    else if (entry.CacheHit)
    {
        Console.WriteLine($"Request {entry.Id} served from cache in {entry.DurationMs}ms");
    }
}
```

### Example 2: Aggregating Request Metrics
This example shows how to iterate over a collection of log entries to calculate aggregate statistics, such as total bandwidth and average duration.

```csharp
public void CalculateMetrics(IEnumerable<RequestLogEntry> logs)
{
    var totalRequests = logs.Count();
    var totalInboundBytes = logs.Sum(l => l.RequestSizeBytes);
    var totalOutboundBytes = logs.Sum(l => l.ResponseSizeBytes);
    var avgDuration = logs.Any() ? logs.Average(l => l.DurationMs) : 0;

    Console.WriteLine($"Total Requests: {totalRequests}");
    Console.WriteLine($"Total Traffic: {totalInboundBytes + totalOutboundBytes} bytes");
    Console.WriteLine($"Average Duration: {avgDuration:F2}ms");
    
    // Accessing headers safely
    foreach (var log in logs.Where(l => l.RequestHeaders.ContainsKey("Authorization")))
    {
        // Process authorized requests
    }
}
```

## Notes

*   **Mutability**: All members of `RequestLogEntry` are read-write properties. The type does not enforce immutability upon construction; consumers must ensure that instances are not modified concurrently after being published to shared storage or logging sinks.
*   **Thread Safety**: The type itself is not thread-safe. Specifically, the `RequestHeaders` property exposes a mutable `Dictionary<string, string>`. If multiple threads access a single `RequestLogEntry` instance where one thread enumerates `RequestHeaders` while another modifies it, a runtime exception will occur. External synchronization or defensive copying is required in multi-threaded scenarios.
*   **Nullable Reference Types**: Several string properties (`LogLevel`, `Message`, `RequestId`, `ServiceName`, `MethodName`, `ClientIp`, `UpstreamAddress`, `ErrorMessage`) are annotated as nullable (`string?`). Consumers should perform null checks before accessing members of these strings or passing them to methods requiring non-null arguments.
*   **Data Integrity**: The `GrpcMethod`, `Method`, `Path`, and boolean/numeric fields are non-nullable and expected to always contain valid default values (e.g., 0 for numbers, `false` for booleans) if specific data was not captured during the request lifecycle.
