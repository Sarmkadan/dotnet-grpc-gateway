// ... (rest of the file remains the same)

## DotnetGrpcGatewayOptionsExtensions

The `DotnetGrpcGatewayOptionsExtensions` class provides a set of extension methods for configuring `DotnetGrpcGatewayOptions` instances. These methods allow for easy customization of gateway settings, such as listening addresses, ports, and health check configurations.

### Example Usage:

```csharp
var options = new DotnetGrpcGatewayOptions();

options
    .UseLocalhost()
    .UsePort(50051)
    .DisableReflection()
    .ConfigureHealthCheck(hc => hc.FailureThreshold = 3)
    .SetLogLevel("Information")
    .SetRequestLoggingVerbosity(RequestLoggingVerbosity.Verbose)
    .SetRequestLoggingEnabled(true);
```
// ... (rest of the file remains the same)
