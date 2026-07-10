# IRequestLogService
The `IRequestLogService` interface provides a logging mechanism for tracking and analyzing incoming requests. It allows for the storage and retrieval of request logs, providing insights into the performance and behavior of the system. The interface offers methods for appending new log entries, searching and retrieving recent logs, and calculating summary statistics.

## API
* `int TotalEntries`: Gets the total number of log entries.
* `int SuccessCount`: Gets the number of successful requests.
* `int ErrorCount`: Gets the number of requests that resulted in errors.
* `double SuccessRatePct`: Gets the percentage of successful requests.
* `double AverageDurationMs`: Gets the average duration of all requests in milliseconds.
* `long MinDurationMs`: Gets the minimum duration of all requests in milliseconds.
* `long MaxDurationMs`: Gets the maximum duration of all requests in milliseconds.
* `DateTime? OldestEntry`: Gets the timestamp of the oldest log entry.
* `DateTime? NewestEntry`: Gets the timestamp of the newest log entry.
* `RequestLogService`: Not applicable (property, not a method).
* `void Append`: Appends a new log entry to the service. Parameters and return value not specified.
* `IReadOnlyList<RequestLogEntry> GetRecent`: Retrieves a list of recent log entries. Parameters and return value not specified.
* `IReadOnlyList<RequestLogEntry> Search`: Searches for log entries based on specified criteria. Parameters and return value not specified.
* `RequestLogSummary GetSummary`: Calculates and returns a summary of the log entries. Parameters and return value not specified.
* `void Clear`: Clears all log entries from the service. Parameters and return value not specified.

## Usage
The following examples demonstrate how to use the `IRequestLogService` interface:
```csharp
// Example 1: Append a new log entry and retrieve recent logs
var logService = new RequestLogService();
logService.Append(new RequestLogEntry { DurationMs = 100, Success = true });
var recentLogs = logService.GetRecent(10);
foreach (var log in recentLogs)
{
    Console.WriteLine($"Request {log.Id} took {log.DurationMs}ms");
}

// Example 2: Search for log entries with errors and calculate summary statistics
var errorLogs = logService.Search(new RequestLogEntry { Success = false });
var summary = logService.GetSummary();
Console.WriteLine($"Error rate: {summary.ErrorCount / (double)summary.TotalEntries * 100}%");
```

## Notes
When using the `IRequestLogService` interface, consider the following:
* The `Append` method may throw an exception if the log entry is invalid or if the service is not properly initialized.
* The `GetRecent` and `Search` methods may return an empty list if no log entries match the specified criteria.
* The `GetSummary` method may throw an exception if the log entries are not properly initialized or if the calculation fails.
* The `Clear` method may throw an exception if the service is not properly initialized or if the log entries cannot be cleared.
* The `IRequestLogService` interface is not thread-safe by default. Implementations should consider synchronizing access to the log entries and summary statistics to ensure thread safety.
