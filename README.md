// ... (rest of the file remains the same)

## StructuredLogger

The `StructuredLogger` class provides a set of static methods for logging structured data across the gRPC gateway. It enables consistent and standardized logging of important events, errors, and performance metrics, making it easier to monitor and diagnose issues within the system.

### Example Usage:

```csharp
public class Program
{
    private static readonly ILogger _logger = // obtain logger instance from DI or elsewhere

    public static void Main(string[] args)
    {
        // Log request start
        StructuredLogger.LogRequestStart(_logger, "12345", "/api/users", "GET", "192.168.1.100");

        // Log request completion
        StructuredLogger.LogRequestComplete(_logger, "12345", "/api/users", 200, 45);

        // Log service discovery
        StructuredLogger.LogServiceDiscovery(_logger, 1, "user-service", true);

        // Log cache operation
        StructuredLogger.LogCacheOperation(_logger, "get", "user:123", true);

        // Log route resolution
        StructuredLogger.LogRouteResolution(_logger, "/api/users", "/api/{service}/**", 1);

        // Log rate limit
        StructuredLogger.LogRateLimit(_logger, "192.168.1.100", "/api/users", 10);

        // Log authentication
        StructuredLogger.LogAuthentication(_logger, "user-123", true);

        // Log critical error
        var ex = new Exception("Something went wrong");
        StructuredLogger.LogCriticalError(_logger, ex, "User registration failed");

        // Log performance metrics
        StructuredLogger.LogPerformanceMetrics(_logger, "database query", 12);
    }
}
```
