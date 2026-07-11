# OutputFormatterFactoryExtensions

The `OutputFormatterFactoryExtensions` class provides a set of static extension methods and helpers designed to simplify the retrieval and execution of output formatters within the `dotnet-grpc-gateway` pipeline. It abstracts the complexity of content negotiation and asynchronous serialization, allowing developers to resolve appropriate formatters based on request content types and directly serialize objects to string representations with minimal boilerplate.

## API

### `GetFormatterOrDefault`
```csharp
public static IOutputFormatter? GetFormatterOrDefault(this OutputFormatterFactory factory, string? contentType)
```
Retrieves the first `IOutputFormatter` instance capable of handling the specified `contentType`. If no registered formatter supports the provided media type, or if the factory itself is null, this method returns `null` instead of throwing an exception. This is useful for scenarios where a fallback behavior is preferred over strict content negotiation failures.

*   **Parameters**:
    *   `factory`: The source `OutputFormatterFactory` instance.
    *   `contentType`: The media type string (e.g., "application/json") to match against registered formatters. Can be `null`.
*   **Returns**: An `IOutputFormatter` instance if a match is found; otherwise, `null`.
*   **Throws**: No exceptions are thrown by this method regarding missing formatters; it strictly returns `null` on failure.

### `TryGetFormatter`
```csharp
public static bool TryGetFormatter(this OutputFormatterFactory factory, string? contentType, [NotNullWhen(true)] out IOutputFormatter? formatter)
```
Attempts to retrieve an `IOutputFormatter` for the given `contentType`. This method follows the standard .NET "Try" pattern, returning a boolean success flag and outputting the formatter via an `out` parameter. The `[NotNullWhen(true)]` attribute ensures that when the method returns `true`, the `formatter` output is guaranteed to be non-null.

*   **Parameters**:
    *   `factory`: The source `OutputFormatterFactory` instance.
    *   `contentType`: The media type string to match.
    *   `formatter`: When the method returns `true`, contains the matched `IOutputFormatter`; otherwise, `null`.
*   **Returns**: `true` if a suitable formatter was found; `false` otherwise.
*   **Throws**: No exceptions are thrown for missing formatters.

### `Format<T>`
```csharp
public static string Format<T>(this OutputFormatterFactory factory, T value, string? contentType)
```
Synchronously resolves a formatter for the specified `contentType` and immediately serializes the provided `value` to a string representation. This method combines resolution and execution into a single call.

*   **Parameters**:
    *   `factory`: The source `OutputFormatterFactory` instance.
    *   `value`: The object instance of type `T` to be serialized.
    *   `contentType`: The target media type used to select the formatter.
*   **Returns**: The serialized string representation of `value`.
*   **Throws**:
    *   `InvalidOperationException`: If no formatter can be found for the specified `contentType`.
    *   Exceptions inherent to the underlying formatter's synchronous serialization logic.

### `FormatAsync<T>`
```csharp
public static async Task<string> FormatAsync<T>(this OutputFormatterFactory factory, T value, string? contentType)
```
Asynchronously resolves a formatter for the specified `contentType` and serializes the provided `value` to a string. This is the preferred method for serialization tasks that involve I/O or computationally expensive operations within the formatter.

*   **Parameters**:
    *   `factory`: The source `OutputFormatterFactory` instance.
    *   `value`: The object instance of type `T` to be serialized.
    *   `contentType`: The target media type used to select the formatter.
*   **Returns**: A `Task<string>` representing the asynchronous operation, with the result being the serialized string.
*   **Throws**:
    *   `InvalidOperationException`: If no formatter can be found for the specified `contentType`.
    *   Exceptions inherent to the underlying formatter's asynchronous serialization logic.

## Usage

### Example 1: Safe Formatter Resolution
Use `TryGetFormatter` when the content type is dynamic or user-provided, and the application should degrade gracefully if a specific format is not supported.

```csharp
using DotNet.Grpc.Gateway.Formatters;

public void ProcessRequest(OutputFormatterFactory factory, string? requestedType, object data)
{
    if (factory.TryGetFormatter(requestedType, out var formatter))
    {
        // Formatter found, proceed with custom serialization logic
        Console.WriteLine($"Using formatter: {formatter.GetType().Name}");
        // Note: Actual writing would depend on IOutputFormatter interface details
    }
    else
    {
        // Handle unsupported media type gracefully
        Console.WriteLine($"No formatter available for '{requestedType}'. Defaulting to error response.");
    }
}
```

### Example 2: Direct Asynchronous Serialization
Use `FormatAsync` for high-throughput scenarios where the goal is simply to obtain the string payload without managing the formatter lifecycle manually.

```csharp
using DotNet.Grpc.Gateway.Formatters;
using System.Threading.Tasks;

public async Task<string> SerializeResponseAsync(OutputFormatterFactory factory, MyGrpcMessage message)
{
    const string JsonContentType = "application/json";
    
    // Automatically resolves the JSON formatter and serializes the message
    string payload = await factory.FormatAsync(message, JsonContentType);
    
    return payload;
}
```

## Notes

*   **Null Content Types**: All methods accept a `null` `contentType`. Behavior in this case depends on the internal implementation of `OutputFormatterFactory`; typically, this may attempt to match a default formatter or return null/false immediately.
*   **Exception Handling**: Unlike the `Get...` and `Try...` methods, the `Format` and `FormatAsync` methods will throw an `InvalidOperationException` if no matching formatter is found. Callers must ensure content type validity or wrap these calls in try-catch blocks if the media type is uncertain.
*   **Thread Safety**: As these are static extension methods operating on an injected `OutputFormatterFactory` instance, thread safety is contingent upon the implementation of the `OutputFormatterFactory` and the specific `IOutputFormatter` instances it returns. Generally, factory resolution is read-only and thread-safe, but the resulting formatter's `Write` or `WriteAsync` methods should be assumed to be stateful unless documented otherwise by the specific formatter implementation.
*   **Synchronous vs Asynchronous**: Prefer `FormatAsync` in web request pipelines to prevent thread pool starvation, especially if the underlying formatter performs file I/O or network calls during serialization. Use `Format` only in contexts where asynchronous execution is impossible or the formatter is known to be purely CPU-bound and fast.
