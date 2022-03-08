# IMetricsRepository

`IMetricsRepository` is an interface that provides methods for recording, retrieving, and analyzing request metrics within the `dotnet-grpc-gateway` project. It serves as a centralized store for tracking request performance, aggregating statistics, and identifying slow requests to facilitate monitoring and optimization of gateway operations.

## API

### `Task<RequestMetric> RecordRequestAsync`
Records a single request metric and updates associated statistics.

**Purpose:**
Logs details of an individual request (e.g., latency, endpoint, status) for later retrieval and analysis. This method is typically called after a request completes.

**Parameters:**
None.

**Returns:**
A `Task<RequestMetric>` representing the recorded request metric.

**Throws:**
- `InvalidOperationException`: If the underlying storage mechanism fails (e.g., database connection issues).

---

### `Task<List<RequestMetric>> GetMetricsAsync`
Retrieves all recorded request metrics.

**Purpose:**
Returns a comprehensive list of all request metrics stored in the repository. Useful for auditing or detailed analysis.

**Parameters:**
None.

**Returns:**
A `Task<List<RequestMetric>>` containing all request metrics. Returns an empty list if no metrics are recorded.

**Throws:**
- `InvalidOperationException`: If the retrieval operation fails.

---

### `Task<List<RequestMetric>> GetServiceMetricsAsync`
Retrieves request metrics filtered by a specific service.

**Purpose:**
Returns metrics scoped to a single service, enabling service-level performance analysis.

**Parameters:**
None (assumes filtering is handled internally, e.g., via a service identifier passed during recording).

**Returns:**
A `Task<List<RequestMetric>>` containing metrics for the specified service. Returns an empty list if no metrics exist for the service.

**Throws:**
- `InvalidOperationException`: If the retrieval operation fails.

---

### `Task<GatewayStatistics> GetStatisticsAsync`
Retrieves aggregated statistics for the gateway.

**Purpose:**
Provides high-level metrics (e.g., total requests, average latency, error rates) derived from recorded request data. Useful for dashboards or health checks.

**Parameters:**
None.

**Returns:**
A `Task<GatewayStatistics>` object containing aggregated statistics.

**Throws:**
- `InvalidOperationException`: If statistics cannot be computed or retrieved.

---

### `Task UpdateStatisticsAsync`
Updates the aggregated statistics based on newly recorded requests.

**Purpose:**
Recalculates statistics (e.g., averages, percentiles) to reflect recent request data. This method is typically called after `RecordRequestAsync` to ensure statistics remain current.

**Parameters:**
None.

**Returns:**
A `Task` representing the asynchronous operation.

**Throws:**
- `InvalidOperationException`: If the update operation fails.

---

### `Task<List<RequestMetric>> GetSlowRequestsAsync`
Retrieves requests exceeding a configured latency threshold.

**Purpose:**
Identifies slow requests for performance tuning or troubleshooting. The threshold is defined by implementation-specific logic (e.g., a fixed duration or percentile-based).

**Parameters:**
None.

**Returns:**
A `Task<List<RequestMetric>>` containing slow requests. Returns an empty list if no slow requests are detected.

**Throws:**
- `InvalidOperationException`: If the retrieval operation fails.

## Usage

### Example 1: Recording and Retrieving Metrics
