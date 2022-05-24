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
