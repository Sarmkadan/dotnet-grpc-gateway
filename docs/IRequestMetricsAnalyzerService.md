# IRequestMetricsAnalyzerService

Provides metrics analysis for gRPC gateway request patterns, exposing aggregated statistics and anomaly detection capabilities for monitoring and troubleshooting service health.

## API

### Properties

- **`AlertType`** (string?)
  Gets the type of alert triggered by the most recent analysis, if any. Returns `null` when no alert is active. Used to categorize detected issues for downstream alerting systems.

- **`AverageResponseTime`** (double)
  Gets the average response time across all recorded requests, in milliseconds. Calculates as the arithmetic mean of all measured response times. Returns `0` if no requests have been recorded.

- **`DetectedAt`** (DateTime)
  Gets the timestamp when the most recent alert or significant event was detected. Useful for correlating alerts with external monitoring systems.

- **`Endpoint`** (string?)
  Gets the endpoint path associated with the most recent alert or analysis result. Returns `null` when the analysis does not pertain to a specific endpoint.

- **`FailureCount`** (int)
  Gets the total number of failed requests recorded by the analyzer. Failed requests are determined by non-successful status codes or exceptions during processing.

- **`HealthScore`** (double)
  Gets a normalized score between 0.0 and 100.0 representing the overall health of the service based on request patterns, response times, and failure rates. Higher values indicate better health.

- **`Message`** (string?)
  Gets a human-readable message describing the most recent alert or analysis outcome. Returns `null` when no alert is active.

- **`MostAccessedEndpoint`** (string?)
  Gets the endpoint path that has received the highest number of requests during the recorded period. Returns `null` if no requests have been recorded.

- **`RequestCount`** (int)
  Gets the total number of requests processed by the analyzer during its lifetime. This count includes both successful and failed requests.

- **`Severity`** (int)
  Gets the severity level of the most recent alert, expressed as an integer. Higher values indicate more severe issues. The scale and meaning of severity levels are implementation-defined.

- **`SlowestEndpoint`** (string?)
  Gets the endpoint path with the highest average response time during the recorded period. Returns `null` if no requests have been recorded.

- **`Status`** (string?)
  Gets the current operational status of the analyzer, such as `"Healthy"`, `"Degraded"`, or `"Critical"`. Returns `null` if the status has not been set.

- **`SuccessCount`** (int)
  Gets the total number of successful requests recorded by the analyzer. Successful requests are determined by successful status codes and absence of exceptions.

- **`SuccessRate`** (double)
  Gets the ratio of successful requests to total requests, expressed as a value between 0.0 and 1.0. Calculated as `SuccessCount / RequestCount`. Returns `0.0` if no requests have been recorded.

- **`TotalRequests`** (int)
  Gets the total number of requests processed by the analyzer during its lifetime. Equivalent to `RequestCount`.

### Methods

- **`AnalyzeRequestPatternsAsync()`** (async Task<RequestPatternAnalysis>)
  Asynchronously analyzes the accumulated request metrics and generates a structured report of patterns, anomalies, and health indicators.

  **Returns**
  A `Task<RequestPatternAnalysis>` that resolves to a `RequestPatternAnalysis` object containing detailed findings, including endpoint-level metrics, anomalies, and recommendations.

  **Remarks**
  This method should be called periodically or in response to significant changes in request volume or error rates. It may throw `InvalidOperationException` if no requests have been recorded, or `ObjectDisposedException` if the analyzer has been disposed.

- **`RequestMetricsAnalyzerService`** (constructor)
  Initializes a new instance of the `RequestMetricsAnalyzerService` with default configuration. The analyzer begins collecting metrics immediately upon instantiation.

## Usage
