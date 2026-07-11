# RequestLogEntryExtensions

The `RequestLogEntryExtensions` class provides a set of static extension methods and properties designed to augment `RequestLogEntry` instances within the `dotnet-grpc-gateway` pipeline. These members facilitate the extraction of derived metrics, human-readable formatting, and categorical analysis of gRPC request logs without modifying the underlying log entry structure. By centralizing logic for status code interpretation, duration formatting, and payload size calculation, this utility class ensures consistent reporting and monitoring capabilities across the gateway infrastructure.

## API

### WasCacheHit
Determines whether the processed request was served from the cache rather than being forwarded to the upstream service.
*   **Return Value**: `true` if the response was retrieved from the cache; otherwise, `false`.
*   **Exceptions**: None.

### WasSuccessful
Indicates whether the request completed with a success status code.
*   **Return Value**: `true` if the status code represents a successful operation (typically 2xx); otherwise, `false`.
*   **Exceptions**: None.

### DurationSeconds
Retrieves the total elapsed time for the request processing in seconds.
*   **Return Value**: A `double` representing the duration in seconds, including fractional components.
*   **Exceptions**: None.

### TotalSizeBytes
Calculates the aggregate size of the request and response payloads in bytes.
*   **Return Value**: A `long` representing the total number of bytes transferred.
*   **Exceptions**: None.

### FormattedDuration
Provides a human-readable string representation of the request duration.
*   **Return Value**: A `string` formatted to an appropriate time unit (e.g., "ms", "s") based on the magnitude of the duration.
*   **Exceptions**: None.

### FormattedSize
Provides a human-readable string representation of the total data size.
*   **Return Value**: A `string` formatted with appropriate binary prefixes (e.g., "KB", "MB") based on the magnitude of the size.
*   **Exceptions**: None.

### StatusCodeCategory
Returns a textual categorization of the HTTP/gRPC status code associated with the request.
*   **Return Value**: A `string` describing the category (e.g., "Success", "Redirection", "Client Error", "Server Error").
*   **Exceptions**: None.

### IsClientError
Determines if the request resulted in a client-side error.
*   **Return Value**: `true` if the status code falls within the client error range (typically 4xx); otherwise, `false`.
*   **Exceptions**: None.

### IsServerError
Determines if the request resulted in a server-side error.
*   **Return Value**: `true` if the status code falls within the server error range (typically 5xx); otherwise, `false`.
*   **Exceptions**: None.

### GetSummary
Generates a concise textual summary of the log entry, typically combining the target address, status, and duration.
*   **Return Value**: A `string` containing the formatted summary.
*   **Exceptions**: None.

### GetTargetAddress
Extracts the specific target address or endpoint invoked by the request.
*   **Return Value**: A `string` representing the destination address.
*   **Exceptions**: None.

### GetErrorDetails
Retrieves detailed error information if the request failed.
*   **Return Value**: A `string` containing the error details if available; otherwise, `null`.
*   **Exceptions**: None.

## Usage

### Example 1: Conditional Logging Based on Status
This example demonstrates how to use the extension properties to filter and log specific details only for failed requests, distinguishing between client and server errors.

```csharp
using DotNetGrpcGateway.Extensions;

public void LogFailedRequests(RequestLogEntry entry)
{
    if (!entry.WasSuccessful)
    {
        var category = entry.StatusCodeCategory;
        var errorDetails = entry.GetErrorDetails() ?? "No detailed error message available.";
        
        if (entry.IsClientError)
        {
            Console.WriteLine($"[Client Error] {entry.GetTargetAddress()} - {category}: {errorDetails}");
        }
        else if (entry.IsServerError)
        {
            Console.WriteLine($"[Server Error] {entry.GetTargetAddress()} - {category} ({entry.FormattedDuration})");
        }
    }
}
```

### Example 2: Performance and Payload Reporting
This example illustrates generating a formatted report for successful requests, utilizing the human-readable formatting helpers for duration and size, and checking cache status.

```csharp
using DotNetGrpcGateway.Extensions;

public void GeneratePerformanceReport(IEnumerable<RequestLogEntry> logs)
{
    foreach (var entry in logs)
    {
        if (entry.WasSuccessful)
        {
            var cacheStatus = entry.WasCacheHit ? "HIT" : "MISS";
            var summary = entry.GetSummary();
            
            Console.WriteLine($"{summary} | Size: {entry.FormattedSize} | Duration: {entry.FormattedDuration} | Cache: {cacheStatus}");
        }
    }
}
```

## Notes

*   **Null Safety**: The `GetErrorDetails` method returns a nullable string (`string?`). Callers must handle the `null` case appropriately, as not all log entries will contain populated error details even if the request was unsuccessful.
*   **Thread Safety**: As this class consists entirely of static members that operate on passed-in `RequestLogEntry` instances without maintaining internal mutable state, it is inherently thread-safe. However, the thread safety of the `RequestLogEntry` object itself passed to these extensions depends on the implementation of that base type.
*   **Precision**: The `DurationSeconds` property returns a `double`, which may introduce floating-point precision limitations for extremely high-frequency measurements. For display purposes, `FormattedDuration` should be preferred.
*   **Dependencies**: These extensions assume the underlying `RequestLogEntry` contains valid initialization data. Accessing members on a partially initialized or null log entry instance (if passed as null to an extension method) will result in standard `NullReferenceException` behavior inherent to C# extension methods on null instances.
