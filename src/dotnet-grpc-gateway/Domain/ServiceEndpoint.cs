#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Domain;

/// <summary>
/// Represents a single addressable endpoint for a registered gRPC service.
/// Multiple endpoints per service enable load balancing.
/// </summary>
public class ServiceEndpoint
{
    public int Id { get; set; }
    public int ServiceId { get; set; }
    public string Host { get; set; } = null!;
    public int Port { get; set; } = 5000;
    public bool UseTls { get; set; }
    public bool IsHealthy { get; set; } = true;
    public int Weight { get; set; } = 1;
    public int ActiveConnections;
    public long TotalRequestsHandled { get; set; }
    public long FailedRequestsCount { get; set; }
    public double AverageResponseTimeMs { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUsedAt { get; set; } = DateTime.UtcNow;

    public string GetUri() => $"{(UseTls ? "https" : "http")}://{Host}:{Port}";

    public void RecordRequest(double responseTimeMs, bool success)
    {
        TotalRequestsHandled++;
        if (!success)
            FailedRequestsCount++;
        LastUsedAt = DateTime.UtcNow;
        AverageResponseTimeMs =
            (AverageResponseTimeMs * (TotalRequestsHandled - 1) + responseTimeMs) / TotalRequestsHandled;
    }
}
