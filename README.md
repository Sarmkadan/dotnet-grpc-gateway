// ... (rest of the file remains the same)

## ErrorHandlingMiddleware

The `ErrorHandlingMiddleware` class is responsible for handling and logging errors with structured responses. It catches exceptions thrown by the application and returns a JSON error response with details about the error.

### Example Usage:

```csharp
public class Program
{
  public static async Task Main(string[] args)
  {
    var middleware = new ErrorHandlingMiddleware(next: null, logger: null);

    var context = new HttpContext();
    context.TraceIdentifier = "1234567890";

    try
    {
      await middleware.InvokeAsync(context);
    }
    catch (Exception ex)
    {
      await middleware.HandleExceptionAsync(context, ex, context.TraceIdentifier);
    }

    Console.WriteLine($"RequestId: {context.TraceIdentifier}");
    Console.WriteLine($"Timestamp: {middleware.Timestamp}");
    Console.WriteLine($"Message: {middleware.Message}");
    Console.WriteLine($"ErrorCode: {middleware.ErrorCode}");
    Console.WriteLine($"Details: {JsonSerializer.Serialize(middleware.Details)}");
  }
}
```
```