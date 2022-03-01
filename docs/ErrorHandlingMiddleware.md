# ErrorHandlingMiddleware

The `ErrorHandlingMiddleware` class is designed to intercept exceptions within the gRPC gateway request pipeline, providing a mechanism for centralized, consistent error response formatting and logging. It ensures that internal exceptions are translated into structured, client-friendly error objects, facilitating better error visibility and debugging across the system.

## API

### Constructors
*   `public ErrorHandlingMiddleware()`
    Initializes a new instance of the `ErrorHandlingMiddleware` class.

### Methods
*   `public async Task InvokeAsync(HttpContext context)`
    Asynchronously intercepts the HTTP request and processes potential exceptions occurring during downstream execution.

### Properties
*   `public string RequestId`
    A unique identifier associated with the request that triggered the error, useful for correlation and tracing.
*   `public DateTime Timestamp`
    The UTC date and time when the error was recorded.
*   `public string Message`
    A descriptive, human-readable summary of the error condition.
*   `public string? ErrorCode`
    An optional, machine-readable code that categorizes the specific type of error encountered.
*   `public Dictionary<string, object>? Details`
    An optional collection of additional, context-specific metadata or diagnostic information related to the error.

## Usage

### Registering the Middleware

To include the middleware in the request processing pipeline, register it within the application configuration:

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Register ErrorHandlingMiddleware early in the pipeline
app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapGrpcService<MyService>();
app.Run();
```

### Accessing Error Details

When an exception is caught, the middleware utilizes its properties to construct an error response:

```csharp
// Inside the InvokeAsync implementation
try
{
    await _next(context);
}
catch (Exception ex)
{
    var error = new ErrorHandlingMiddleware
    {
        RequestId = context.TraceIdentifier,
        Timestamp = DateTime.UtcNow,
        Message = ex.Message,
        ErrorCode = "INTERNAL_SERVER_ERROR",
        Details = new Dictionary<string, object> { { "Type", ex.GetType().Name } }
    };
    
    // Logic to serialize and write the 'error' object to the response body
    await context.Response.WriteAsJsonAsync(error);
}
```

## Notes

*   **Thread Safety**: The `ErrorHandlingMiddleware` is typically registered as a singleton within the ASP.NET Core pipeline. If the middleware instance is used to hold state (such as the `RequestId` or `Message` properties) for a specific request, it must be handled carefully. To ensure thread safety, state should be scoped correctly per request rather than stored directly on the middleware instance. It is recommended to use local variables within `InvokeAsync` or a dedicated error model class to store these details.
*   **Pipeline Ordering**: This middleware should be registered as early as possible in the middleware pipeline to ensure all subsequent exceptions are intercepted.
*   **Exception Handling**: The `InvokeAsync` method should safely handle exceptions that might occur while writing the error response itself to prevent infinite exception loops.
