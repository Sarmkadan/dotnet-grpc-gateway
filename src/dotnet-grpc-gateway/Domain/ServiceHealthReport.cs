#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Domain;

/// <summary>
/// Represents health check results for a gRPC service
/// </summary>
public class ServiceHealthReport
{
    /// <summary>
    /// Identifier of the health report
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Identifier of the service
    /// </summary>
    public int ServiceId { get; set; }

    /// <summary>
    /// Indicates whether the service is healthy
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Current health status string
    /// </summary>
    public string HealthStatus { get; set; } = "Unknown";

    /// <summary>
    /// Response time in milliseconds
    /// </summary>
    public long ResponseTimeMs { get; set; } = 0;

    /// <summary>
    /// HTTP status code from the health check
    /// </summary>
    public int HttpStatusCode { get; set; } = 0;

    /// <summary>
    /// Error message if the health check failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Stack trace if the health check failed
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// Number of consecutive successful health checks
    /// </summary>
    public int SuccessfulChecksInARow { get; set; } = 0;

    /// <summary>
    /// Number of consecutive failed health checks
    /// </summary>
    public int FailedChecksInARow { get; set; } = 0;

    /// <summary>
    /// Total number of health checks performed
    /// </summary>
    public int TotalHealthChecks { get; set; } = 0;

    /// <summary>
    /// Number of successful health checks
    /// </summary>
    public int SuccessfulHealthChecks { get; set; } = 0;

    /// <summary>
    /// Percentage of successful health checks (0-100)
    /// </summary>
    public double HealthCheckSuccessRate { get; set; } = 0.0;

    /// <summary>
    /// Timestamp of the last health check
    /// </summary>
    public DateTime LastCheckAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp for the next scheduled health check
    /// </summary>
    public DateTime NextCheckScheduledAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// URL endpoint used for health checking
    /// </summary>
    public string? HealthCheckEndpoint { get; set; }

    /// <summary>
    /// Timestamp when the report was generated
    /// </summary>
    public DateTime ReportedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// List of diagnostic messages (limited to 10)
    /// </summary>
    public List<string> DiagnosticMessages { get; set; } = new();

    public override string ToString()
        => $"ServiceHealthReport {{ Id = {Id}, ServiceId = {ServiceId}, IsHealthy = {IsHealthy}, HealthStatus = {HealthStatus}, ResponseTimeMs = {ResponseTimeMs}, HttpStatusCode = {HttpStatusCode}, ErrorMessage = {ErrorMessage}, LastCheckAt = {LastCheckAt}, HealthCheckSuccessRate = {HealthCheckSuccessRate} }}";

    public void Validate()
    {
        if (ServiceId <= 0)
            throw new InvalidOperationException("Service ID must be valid");

        if (string.IsNullOrWhiteSpace(HealthStatus))
            throw new InvalidOperationException("Health status is required");

        if (HealthCheckSuccessRate < 0 || HealthCheckSuccessRate > 100)
            throw new InvalidOperationException("Success rate must be between 0 and 100");

        if (ResponseTimeMs < 0)
            throw new InvalidOperationException("Response time cannot be negative");
    }

    public void RecordCheckResult(bool success, long responseTimeMs, string? errorMessage = null)
    {
        TotalHealthChecks++;
        ResponseTimeMs = responseTimeMs;

        if (success)
        {
            SuccessfulChecksInARow++;
            FailedChecksInARow = 0;
            SuccessfulHealthChecks++;
            IsHealthy = true;
            HealthStatus = "Healthy";
            ErrorMessage = null;
            StackTrace = null;
        }
        else
        {
            FailedChecksInARow++;
            SuccessfulChecksInARow = 0;
            IsHealthy = FailedChecksInARow < 3; // Unhealthy after 3 failures
            HealthStatus = IsHealthy ? "Degraded" : "Unhealthy";
            ErrorMessage = errorMessage;
        }

        HealthCheckSuccessRate = (double)SuccessfulHealthChecks / TotalHealthChecks * 100;
        LastCheckAt = DateTime.UtcNow;
    }

    public void AddDiagnosticMessage(string message)
    {
        if (DiagnosticMessages.Count >= 10)
            DiagnosticMessages.RemoveAt(0);

        DiagnosticMessages.Add(message);
    }

    public bool ShouldBeMarkedUnhealthy => FailedChecksInARow >= 3;

    public double GetAvailabilityPercentage =>
        TotalHealthChecks == 0 ? 0.0 : (double)SuccessfulHealthChecks / TotalHealthChecks * 100;
}