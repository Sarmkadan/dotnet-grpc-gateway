// ... (rest of the file remains the same)

## GrpcServiceExtensions

The `GrpcServiceExtensions` class provides utility methods for working with `GrpcService` objects, offering convenient access to common service operations such as health checks, request tracking, and summarization. These extension methods simplify service management and monitoring.

### Example Usage:

```csharp
var grpcService = new GrpcService
{
    Id = 1,
    Name = "ExampleService",
    ServiceFullName = "example.ExampleService",
    Host = "localhost",
    Port = 5001,
    UseTls = false,
    HealthCheckIntervalSeconds = 30
};

// Check if a health check is due
if (grpcService.IsHealthCheckDue())
{
    Console.WriteLine("Health check is due.");
}

// Get the health check URI
var healthCheckUri = grpcService.GetHealthCheckUri();
Console.WriteLine($"Health check URI: {healthCheckUri}");

// Simulate a successful request
grpcService.RecordSuccessfulRequest(50);

// Simulate a failed request
grpcService.RecordFailedRequest("Test error message");

// Get a summary of the service
var summary = grpcService.GetSummary();
Console.WriteLine(summary);
```

These methods enable easy integration of gRPC services with the gateway's monitoring and logging capabilities, making it simpler to manage and troubleshoot services.

// ... (rest of the file remains the same)
