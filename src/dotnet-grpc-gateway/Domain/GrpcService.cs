// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Domain;

/// <summary>
/// Represents a gRPC service that can be routed through the gateway
/// </summary>
public class GrpcService
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string ServiceFullName { get; set; } = null!;

    public string Host { get; set; } = null!;

    public int Port { get; set; } = 5000;

    public bool UseTls { get; set; } = false;

    public string? Description { get; set; }

    public string? ProtoPackage { get; set; }

    public int HealthCheckIntervalSeconds { get; set; } = 30;

    public int MaxRetries { get; set; } = 3;

    public bool IsHealthy { get; set; } = true;

    public DateTime LastHealthCheckAt { get; set; }

    public string? LastHealthCheckError { get; set; }

    public double AverageResponseTimeMs { get; set; } = 0;

    public long TotalRequestsProcessed { get; set; } = 0;

    public long FailedRequestsCount { get; set; } = 0;

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public string GetEndpointUri() => $"{(UseTls ? "https" : "http")}://{Host}:{Port}";

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new InvalidOperationException("Service name is required");

        if (string.IsNullOrWhiteSpace(ServiceFullName))
            throw new InvalidOperationException("Service full name is required");

        if (string.IsNullOrWhiteSpace(Host))
            throw new InvalidOperationException("Service host is required");

        if (Port < 1 || Port > 65535)
            throw new InvalidOperationException("Service port must be between 1 and 65535");

        if (HealthCheckIntervalSeconds < 1)
            throw new InvalidOperationException("Health check interval must be at least 1 second");

        if (MaxRetries < 0)
            throw new InvalidOperationException("Max retries cannot be negative");
    }

    public void UpdateHealthStatus(bool isHealthy, string? errorMessage = null)
    {
        IsHealthy = isHealthy;
        LastHealthCheckAt = DateTime.UtcNow;
        LastHealthCheckError = errorMessage;
    }

    public void RecordRequestMetric(double responseTimeMs, bool success)
    {
        TotalRequestsProcessed++;
        if (!success) FailedRequestsCount++;

        // Update running average response time
        AverageResponseTimeMs = (AverageResponseTimeMs * (TotalRequestsProcessed - 1) + responseTimeMs) / TotalRequestsProcessed;
        ModifiedAt = DateTime.UtcNow;
    }
}
