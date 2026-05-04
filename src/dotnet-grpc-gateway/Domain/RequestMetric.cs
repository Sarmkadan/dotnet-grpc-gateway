// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Domain;

/// <summary>
/// Tracks metrics for individual requests processed by the gateway
/// </summary>
public class RequestMetric
{
    public int Id { get; set; }

    public string RequestId { get; set; } = Guid.NewGuid().ToString();

    public string ServiceName { get; set; } = null!;

    public string MethodName { get; set; } = null!;

    public string ClientIpAddress { get; set; } = null!;

    public int RouteId { get; set; }

    public long RequestSizeBytes { get; set; } = 0;

    public long ResponseSizeBytes { get; set; } = 0;

    public double DurationMs { get; set; } = 0;

    public int HttpStatusCode { get; set; } = 200;

    public string? GrpcStatusCode { get; set; }

    public bool IsSuccessful { get; set; } = true;

    public string? ErrorMessage { get; set; }

    public string? StackTrace { get; set; }

    public Dictionary<string, string> RequestHeaders { get; set; } = new();

    public Dictionary<string, string> ResponseHeaders { get; set; } = new();

    public string? CacheHitStatus { get; set; }

    public bool WasRetried { get; set; } = false;

    public int RetryCount { get; set; } = 0;

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ServiceName))
            throw new InvalidOperationException("Service name is required");

        if (string.IsNullOrWhiteSpace(MethodName))
            throw new InvalidOperationException("Method name is required");

        if (string.IsNullOrWhiteSpace(ClientIpAddress))
            throw new InvalidOperationException("Client IP address is required");

        if (DurationMs < 0)
            throw new InvalidOperationException("Duration cannot be negative");

        if (RequestSizeBytes < 0 || ResponseSizeBytes < 0)
            throw new InvalidOperationException("Message sizes cannot be negative");
    }

    public bool IsSlowRequest(double slowThresholdMs = 1000) => DurationMs > slowThresholdMs;

    public void RecordError(string errorMessage, string? stackTrace = null)
    {
        IsSuccessful = false;
        ErrorMessage = errorMessage;
        StackTrace = stackTrace;
    }

    public void RecordRetry() => RetryCount++;

    public void SetCacheStatus(string status) => CacheHitStatus = status;
}
