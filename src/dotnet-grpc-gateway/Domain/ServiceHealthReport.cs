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
    public int Id { get; set; }

    public int ServiceId { get; set; }

    public bool IsHealthy { get; set; }

    public string HealthStatus { get; set; } = "Unknown";

    public long ResponseTimeMs { get; set; } = 0;

    public int HttpStatusCode { get; set; } = 0;

    public string? ErrorMessage { get; set; }

    public string? StackTrace { get; set; }

    public int SuccessfulChecksInARow { get; set; } = 0;

    public int FailedChecksInARow { get; set; } = 0;

    public int TotalHealthChecks { get; set; } = 0;

    public int SuccessfulHealthChecks { get; set; } = 0;

    public double HealthCheckSuccessRate { get; set; } = 0.0;

    public DateTime LastCheckAt { get; set; }

    public DateTime NextCheckScheduledAt { get; set; }

    public string? HealthCheckEndpoint { get; set; }

    public DateTime ReportedAt { get; set; } = DateTime.UtcNow;

    public List<string> DiagnosticMessages { get; set; } = new();

    public void Validate()
    {
        if (ServiceId <= 0)
            throw new InvalidOperationException("Service ID must be valid");

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

        DiagnosticMessages.Add($"[{DateTime.UtcNow:O}] {message}");
    }

    public bool ShouldBeMarkedUnhealthy => FailedChecksInARow >= 3;

    public double GetAvailabilityPercentage => HealthCheckSuccessRate;
}
