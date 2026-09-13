#nullable enable
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
    /// <summary>
    /// Gets or sets the database identifier for this metric record.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the request. Defaults to a new GUID.
    /// </summary>
    public string RequestId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Correlation ID from the incoming request (W3C traceparent or X-Correlation-ID header).
    /// This ID flows through the entire request pipeline and is propagated to downstream services.
    /// </summary>
    /// <summary>
    /// Gets or sets the correlation ID from the incoming request (W3C traceparent or X-Correlation-ID header).
    /// This ID flows through the entire request pipeline and is propagated to downstream services.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the name of the service that processed the request.
    /// </summary>
    public string ServiceName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the name of the method that was invoked.
    /// </summary>
    public string MethodName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the IP address of the client that made the request.
    /// </summary>
    public string ClientIpAddress { get; set; } = null!;

    /// <summary>
    /// Gets or sets the identifier of the route that handled the request.
    /// </summary>
    public int RouteId { get; set; }

    /// <summary>
    /// Gets or sets the size of the request in bytes.
    /// </summary>
    public long RequestSizeBytes { get; set; } = 0;

    /// <summary>
    /// Gets or sets the size of the response in bytes.
    /// </summary>
    public long ResponseSizeBytes { get; set; } = 0;

    /// <summary>
    /// Gets or sets the duration of the request in milliseconds.
    /// </summary>
    public double DurationMs { get; set; } = 0;

    /// <summary>
    /// Gets or sets the HTTP status code of the response.
    /// </summary>
    public int HttpStatusCode { get; set; } = 200;

    /// <summary>
    /// Gets or sets the gRPC status code, if applicable.
    /// </summary>
    public string? GrpcStatusCode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the request was successful.
    /// </summary>
    public bool IsSuccessful { get; set; } = true;

    /// <summary>
    /// Gets or sets the error message if the request failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the stack trace if the request failed.
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// Gets or sets the request headers.
    /// </summary>
    public Dictionary<string, string> RequestHeaders { get; set; } = new();

    /// <summary>
    /// Gets or sets the response headers.
    /// </summary>
    public Dictionary<string, string> ResponseHeaders { get; set; } = new();

    /// <summary>
    /// Gets or sets the cache hit status (e.g., "HIT", "MISS", "BYPASS").
    /// </summary>
    public string? CacheHitStatus { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the request was retried.
    /// </summary>
    public bool WasRetried { get; set; } = false;

    /// <summary>
    /// Gets or sets the number of times the request was retried.
    /// </summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>
    /// Gets or sets the date and time when the metric was recorded (in UTC).
    /// </summary>
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    public override string ToString() =>
        $"RequestMetric {{ Id = {Id}, RequestId = {RequestId}, ServiceName = {ServiceName}, MethodName = {MethodName}, RouteId = {RouteId}, DurationMs = {DurationMs}, HttpStatusCode = {HttpStatusCode}, GrpcStatusCode = {GrpcStatusCode}, IsSuccessful = {IsSuccessful} }}";

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

    public void RecordRetry()
    {
        RetryCount++;
        WasRetried = true;
    }

    public void SetCacheStatus(string status) => CacheHitStatus = status;
}
