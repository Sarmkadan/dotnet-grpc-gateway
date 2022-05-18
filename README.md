// ... (rest of the file remains the same)

## RouteChannelOptionsExtensions

The `RouteChannelOptionsExtensions` class provides utility methods for configuring gRPC channel options. It allows you to set call timeout, maximum receive and send message sizes, add headers, and configure TLS settings.

### Example Usage:

```csharp
var options = new RouteChannelOptions();

options
    .WithCallTimeoutMs(5000)
    .WithMaxReceiveMessageSize(1024 * 1024)
    .WithMaxSendMessageSize(1024 * 1024)
    .WithHeader("Authorization", "Bearer token")
    .WithTlsTargetName("example.com")
    .WithSkipTlsVerification();

var otherOptions = new RouteChannelOptions
{
    CallTimeout = TimeSpan.FromSeconds(10),
    MaxReceiveMessageSize = 2048,
};

options.UpdateFrom(otherOptions);
``` 

// ... (rest of the file remains the same)
