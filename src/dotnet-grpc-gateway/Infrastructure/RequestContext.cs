#nullable enable
// ====================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

using System.Diagnostics;

namespace DotNetGrpcGateway.Infrastructure;

/// <summary>
/// Request context that flows through the entire request pipeline.
/// Provides correlation IDs, user information, and request metadata for logging and tracing.
/// </summary>
/// <remarks>
/// This context integrates with System.Diagnostics.Activity for W3C Trace Context support.
/// When Activity.Current is available (e.g., from incoming traceparent header), the CorrelationId
/// will be sourced from the activity's TraceId. Otherwise, a new correlation ID is generated.
/// </remarks>
public class RequestContext
{
    /// <summary>
    /// Unique request identifier for tracking across logs and systems.
    /// </summary>
    /// <remarks>This is a unique ID generated per request, distinct from the correlation ID.</remarks>
    public string RequestId { get; } = GenerateRequestId();

    /// <summary>
    /// Correlation ID from incoming W3C traceparent header or generated for tracking related operations.
    /// This ID flows through the entire request pipeline and is propagated to downstream services.
    /// </summary>
    /// <remarks>
    /// When W3C Trace Context is active (Activity.Current is set from incoming traceparent header),
    /// this value will be the hex-encoded TraceId from the activity.
    /// Otherwise, it will be a generated correlation ID that follows the same format.
    /// </remarks>
    public string CorrelationId
    {
        get => _correlationId ?? Activity.Current?.TraceId.ToHexString() ?? RequestId;
        set => _correlationId = value;
    }
    private string? _correlationId;

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

    /// <summary>
    /// Gets the W3C Trace Context traceparent header value for this request.
    /// </summary>
    /// <remarks>
    /// Returns the traceparent header value that should be sent to downstream services.
    /// Format: version-trace-id-parent-id-trace-flags
    /// </remarks>
    public string? GetTraceParentHeader()
    {
        var activity = Activity.Current;
        if (activity == null)
            return null;

        // W3C Trace Context format: version-trace-id-parent-id-trace-flags
        // version: 00 (current version)
        // trace-id: 32 hex characters (16 bytes)
        // parent-id: 16 hex characters (8 bytes) - span ID
        // trace-flags: 2 hex characters (8 bits) - 01 means sampled

        var traceId = activity.TraceId.ToHexString();
        var spanId = activity.SpanId.ToHexString();
        var traceFlags = ((byte)(activity.ActivityTraceFlags & ActivityTraceFlags.Recorded)).ToString("x2");

        return $"00-{traceId}-{spanId}-{traceFlags}";
    }

    public RequestContext()
    {
        // Initialize correlation ID from activity if available, otherwise use RequestId
        CorrelationId = Activity.Current?.TraceId.ToHexString() ?? RequestId;
    }

    /// <summary>
    /// Sets a property value in the context.
    /// </summary>
    /// <param name="key">The property key</param>
    /// <param name="value">The property value (null removes the property)</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null or empty</exception>
    public void SetProperty(string key, object? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        if (value is null)
            Properties.Remove(key);
        else
            Properties[key] = value;
    }

    /// <summary>
    /// Gets a property value from the context.
    /// </summary>
    /// <typeparam name="T">The expected property type</typeparam>
    /// <param name="key">The property key</param>
    /// <returns>The property value, or default(T) if not found</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null or empty</exception>
    public T? GetProperty<T>(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        if (Properties.TryGetValue(key, out var value))
            return (T?)value;

        return default;
    }

    /// <summary>
    /// Generates a unique request ID based on the current timestamp and random component.
    /// </summary>
    private static string GenerateRequestId()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
        var random = Random.Shared.Next(100000, 999999);
        return $"{timestamp}-{random}";
    }
}

/// <summary>
/// Provides ambient access to the current <see cref="RequestContext"/> within an async context.
/// </summary>
/// <remarks>
/// This accessor uses <see cref="AsyncLocal{T}"/> to ensure that the request context
/// flows correctly through async/await boundaries, preventing context leakage between
/// concurrent requests in a high-throughput gateway scenario.
/// </remarks>
public static class RequestContextAccessor
{
    private static readonly AsyncLocal<RequestContext?> _context = new();

    /// <summary>
    /// Gets or sets the current <see cref="RequestContext"/> for the current async context.
    /// </summary>
    /// <value>The current request context, or null if not set.</value>
    /// <remarks>
    /// Setting this value establishes the context for the current async flow.
    /// Getting this value retrieves the context that was set in the current or
    /// parent async context.
    /// </remarks>
    public static RequestContext? Current
    {
        get => _context.Value;
        set => _context.Value = value;
    }
}