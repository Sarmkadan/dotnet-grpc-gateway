#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Extensions;

/// <summary>
/// Extension methods for <see cref="HttpContext"/> providing common HTTP request utilities
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Gets the client's IP address, accounting for proxies
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> instance</param>
    /// <returns>The client IP address as a string, or "unknown" if not determinable</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null</exception>
    public static string GetClientIpAddress(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Check for X-Forwarded-For header (set by proxies)
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
        {
            var ips = forwarded.ToString().Split(',');
            return ips.Length > 0 ? ips[0].Trim() : context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        // Fall back to RemoteIpAddress
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    /// <summary>
    /// Gets the value of a header, or null if not present
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> instance</param>
    /// <param name="headerName">The name of the header to retrieve</param>
    /// <returns>The header value if present, otherwise null</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="headerName"/> is null or empty</exception>
    public static string? GetHeader(this HttpContext context, string headerName)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrEmpty(headerName);

        context.Request.Headers.TryGetValue(headerName, out var value);
        return value.FirstOrDefault();
    }

    /// <summary>
    /// Gets the authorization token from the Authorization header
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> instance</param>
    /// <returns>The bearer token if present and valid, otherwise null</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null</exception>
    public static string? GetAuthorizationToken(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var authHeader = context.GetHeader("Authorization");
        if (string.IsNullOrWhiteSpace(authHeader))
            return null;

        var parts = authHeader.Split(' ');
        return parts.Length == 2 && parts[0] == "Bearer" ? parts[1] : null;
    }

    /// <summary>
    /// Gets the request ID (or creates one if not present)
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> instance</param>
    /// <returns>The request ID from X-Request-ID header or TraceIdentifier</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null</exception>
    public static string GetRequestId(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var requestId = context.GetHeader("X-Request-ID");
        return !string.IsNullOrWhiteSpace(requestId) ? requestId : context.TraceIdentifier;
    }

    /// <summary>
    /// Checks if the request is a gRPC request
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> instance</param>
    /// <returns>True if the request is a gRPC request, otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null</exception>
    public static bool IsGrpcRequest(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var contentType = context.Request.ContentType ?? string.Empty;
        return contentType.Contains("application/grpc", StringComparison.Ordinal);
    }

    /// <summary>
    /// Checks if the request is a gRPC-Web request
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> instance</param>
    /// <returns>True if the request is a gRPC-Web request, otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null</exception>
    public static bool IsGrpcWebRequest(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var contentType = context.Request.ContentType ?? string.Empty;
        return contentType.Contains("application/grpc-web", StringComparison.Ordinal);
    }
}