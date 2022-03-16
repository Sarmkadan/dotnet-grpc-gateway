# ServiceHealthReport

A lightweight report object produced by health-checking a gRPC gateway service endpoint. It aggregates the outcome of one or more health-check probes into a single snapshot that downstream dashboards and load balancers can consume to determine whether the service is currently healthy and how reliable it has been over time.

## API

### `public int Id`
Unique numeric identifier assigned to this report instance. Used to correlate reports with the service instance that produced them.

### `public int ServiceId`
Identifier of the gRPC gateway service that emitted this report. Allows grouping and filtering of reports by service.

### `public bool IsHealthy`
True when the service is considered healthy according to the configured health-check policy; otherwise false.

### `public string HealthStatus`
Human-readable summary of the overall health state (e.g., "Healthy", "Degraded", "Unhealthy").

### `public long ResponseTimeMs`
Duration, in milliseconds, that the health-check probe took to complete. Negative values indicate a timeout.

### `public int HttpStatusCode`
HTTP status code returned by the health-check endpoint (200 for success, 5xx for server errors, etc.).

### `public string? ErrorMessage`
Optional error message returned by the health-check probe when the check failed.

### `public string? StackTrace`
Optional stack trace captured at the moment the check failed. May be redacted in production builds.

### `public int SuccessfulChecksInARow`
Number of consecutive successful health-check probes immediately preceding this report.

### `public int FailedChecksInARow`
Number of consecutive failed health-check probes immediately preceding this report.

### `public int TotalHealthChecks`
Total number of health-check probes performed by the service up to and including this report.

### `public int SuccessfulHealthChecks`
Total number of successful health-check probes performed by the service up to and including this report.

### `public double HealthCheckSuccessRate`
Ratio of successful checks to total checks, expressed as a value between 0.0 and 1.0.

### `public DateTime LastCheckAt`
Timestamp of the most recent health-check probe that contributed to this report.

### `public DateTime NextCheckScheduledAt`
Timestamp when the next health-check probe is scheduled to run.

### `public string? HealthCheckEndpoint`
Absolute URL of the endpoint that was probed to produce this report.

### `public DateTime ReportedAt`
Timestamp when the report was assembled and emitted.

### `public List<string> DiagnosticMessages`
Collection of additional diagnostic strings produced during the health-check (e.g., warnings, retry attempts, DNS resolution times).

### `public void Validate()`
Throws an `InvalidOperationException` if any required property is missing or inconsistent (e.g., `HealthCheckSuccessRate` outside [0,1], negative `ResponseTimeMs`, or `IsHealthy` contradicting `HealthStatus`). Otherwise, returns silently.

### `public void RecordCheckResult(bool success, long responseTimeMs, int httpStatusCode, string? errorMessage, string? stackTrace, string? healthCheckEndpoint, List<string> diagnosticMessages)`
Updates the report with the outcome of a single health-check probe.
- `success` – true if the probe succeeded.
- `responseTimeMs` – duration of the probe in milliseconds.
- `httpStatusCode` – HTTP status code returned by the endpoint.
- `errorMessage` – optional error message on failure.
- `stackTrace` – optional stack trace on failure.
- `healthCheckEndpoint` – URL of the endpoint probed.
- `diagnosticMessages` – additional diagnostic strings to include.

## Usage
