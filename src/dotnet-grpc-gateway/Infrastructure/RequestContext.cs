#nullable enable
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

        if (value is null)
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
///
/// <para>
/// Usage pattern:
/// <code>
/// // Set the context (typically in middleware)
/// RequestContextAccessor.Current = new RequestContext { ... };
///
/// // Get the context (anywhere in the async call chain)
/// var context = RequestContextAccessor.Current;
/// </code>
/// </para>
///
/// <para>
/// The accessor is registered as scoped in the DI container, ensuring one instance
/// per HTTP request. The <see cref="AsyncLocal{T}"/> ensures the value flows through
/// async operations within that request's scope.
/// </para>
/// </remarks>
/// <example>
/// Example middleware usage:
/// <code>
/// public class MyMiddleware
/// {
///     private readonly RequestDelegate _next;
///
///     public MyMiddleware(RequestDelegate next)
///     {
///         _next = next;
///     }
///
///     public async Task InvokeAsync(HttpContext context)
///     {
///         var requestContext = new RequestContext
///         {
///             Path = context.Request.Path,
///             Method = context.Request.Method
///         };
///         RequestContextAccessor.Current = requestContext;
///         await _next(context);
///     }
/// }
/// </code>
/// </example>
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
