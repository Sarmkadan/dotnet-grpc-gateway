# RequestLogsController

The `RequestLogsController` provides a set of endpoints for inspecting and managing the request log entries collected by the gateway. It exposes read operations for retrieving recent logs, searching through historical entries, and obtaining a summary of logged activity, as well as a clear operation to remove all stored entries. The controller is intended for diagnostic and administrative use.

## API

### `public RequestLogsController()`

Initializes a new instance of the controller. No parameters are required; the controller relies on the default dependency injection container to resolve its dependencies.

### `public ActionResult<IReadOnlyList<RequestLogEntry>> GetRecent()`

Retrieves the most recent request log entries.  
**Parameters:** None.  
**Return value:** An `ActionResult` containing a read-only list of `RequestLogEntry` objects. The list is ordered by timestamp in descending order.  
**Throws:** No exceptions are thrown under normal operation. If the underlying store is unavailable, an HTTP 500 response is returned.

### `public ActionResult<IReadOnlyList<RequestLogEntry>> Search()`

Searches the request log entries based on criteria provided via the request query string or body.  
**Parameters:** None (search parameters are extracted from the HTTP request).  
**Return value:** An `ActionResult` containing a read-only list of `RequestLogEntry` objects that match the search criteria.  
**Throws:** If the search parameters are malformed, an HTTP 400 response is returned. If the store is unavailable, an HTTP 500 response is returned.

### `public ActionResult<RequestLogSummary> GetSummary()`

Returns a summary of the request logs, including total count, average response time, and error rates over a configurable time window.  
**Parameters:** None (time window may be specified via query parameters).  
**Return value:** An `ActionResult` containing a `RequestLogSummary` object.  
**Throws:** If the time window parameters are invalid, an HTTP 400 response is returned. If the store is unavailable, an HTTP 500 response is returned.

### `public ActionResult Clear()`

Deletes all stored request log entries.  
**Parameters:** None.  
**Return value:** An `ActionResult` indicating success (HTTP 200) or failure.  
**Throws:** If the store is unavailable, an HTTP 500 response is returned.

## Usage

The following examples demonstrate how to call the controller actions from a test client or an administrative tool.

**Example 1: Retrieving recent logs and clearing them**

```csharp
using var client = new HttpClient { BaseAddress = new Uri("https://gateway.example.com") };

// Get the 10 most recent log entries
var recentResponse = await client.GetAsync("/logs/recent");
recentResponse.EnsureSuccessStatusCode();
var recentLogs = await recentResponse.Content.ReadFromJsonAsync<IReadOnlyList<RequestLogEntry>>();

// Clear all logs after inspection
var clearResponse = await client.PostAsync("/logs/clear", null);
clearResponse.EnsureSuccessStatusCode();
```

**Example 2: Searching logs and obtaining a summary**

```csharp
using var client = new HttpClient { BaseAddress = new Uri("https://gateway.example.com") };

// Search for logs with a specific status code
var searchResponse = await client.GetAsync("/logs/search?statusCode=500");
searchResponse.EnsureSuccessStatusCode();
var errorLogs = await searchResponse.Content.ReadFromJsonAsync<IReadOnlyList<RequestLogEntry>>();

// Get a summary for the last hour
var summaryResponse = await client.GetAsync("/logs/summary?windowMinutes=60");
summaryResponse.EnsureSuccessStatusCode();
var summary = await summaryResponse.Content.ReadFromJsonAsync<RequestLogSummary>();
```

## Notes

- **Edge cases:**  
  - `GetRecent` returns an empty list when no logs have been recorded.  
  - `Search` returns an empty list when no entries match the criteria.  
  - `Clear` is idempotent; calling it on an already empty store still returns success.  
  - Large result sets from `GetRecent` or `Search` may be truncated by server-side pagination; the controller does not guarantee that all matching entries are returned in a single call.

- **Thread safety:**  
  The controller is designed to be stateless and thread-safe. Each request creates a new scope for dependencies, and the underlying log store is expected to handle concurrent reads and writes. No instance state is shared across requests.
