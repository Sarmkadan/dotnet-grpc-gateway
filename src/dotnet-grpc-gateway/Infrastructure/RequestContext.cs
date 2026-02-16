// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Infrastructure;

/// <summary>
/// Request context that flows through the entire request pipeline.
/// Provides correlation IDs, user information, and request metadata for logging and tracing.
/// </summary>
public class RequestContext
{
    /// <summary>
    /// Unique request identifier for tracking across logs and systems.
    /// </summary>
    public string RequestId { get; } = GenerateRequestId();

    /// <summary>
    /// Correlation ID from incoming request or generated for tracking related operations.
    /// </summary>
    public string CorrelationId { get; set; }

    /// <summary>
    /// Client IP address making the request.
    /// </summary>
    public string ClientIp { get; set; } = string.Empty;

    /// <summary>
    /// Authenticated user identifier if available.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Request path being processed.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// HTTP method (GET, POST, etc.).
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// Request start time for measuring duration.
    /// </summary>
    public DateTime StartTime { get; } = DateTime.UtcNow;

    /// <summary>
    /// Custom properties bag for storing request-specific data.
    /// </summary>
    public Dictionary<string, object> Properties { get; } = new();

    /// <summary>
    /// Gets the elapsed time since request started.
    /// </summary>
    public TimeSpan Elapsed => DateTime.UtcNow - StartTime;

    public RequestContext()
    {
        CorrelationId = RequestId;
    }

    /// <summary>
    /// Sets a property value in the context.
    /// </summary>
    public void SetProperty(string key, object? value)
    {
        if (string.IsNullOrEmpty(key))
            return;

        if (value == null)
            Properties.Remove(key);
        else
            Properties[key] = value;
    }

    /// <summary>
    /// Gets a property value from the context.
    /// </summary>
    public T? GetProperty<T>(string key)
    {
        if (string.IsNullOrEmpty(key) || !Properties.TryGetValue(key, out var value))
            return default;

        return (T?)value;
    }

    /// <summary>
    /// Generates a unique request ID based on the current timestamp and random component.
    /// </summary>
    private static string GenerateRequestId()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(100000, 999999);
        return $"{timestamp}-{random}";
    }
}

/// <summary>
/// Accessor for the current request context in async context.
/// </summary>
public class RequestContextAccessor
{
    private static readonly AsyncLocal<RequestContext?> _context = new();

    public RequestContext? Current
    {
        get => _context.Value;
        set => _context.Value = value;
    }
}
