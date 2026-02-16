// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Extensions;

/// <summary>
/// Extension methods for HttpContext
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Gets the client's IP address, accounting for proxies
    /// </summary>
    public static string GetClientIpAddress(this HttpContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        // Check for X-Forwarded-For header (set by proxies)
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
        {
            var ips = forwarded.ToString().Split(',');
            if (ips.Length > 0)
                return ips[0].Trim();
        }

        // Fall back to RemoteIpAddress
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    /// <summary>
    /// Gets the value of a header, or null if not present
    /// </summary>
    public static string? GetHeader(this HttpContext context, string headerName)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        if (string.IsNullOrWhiteSpace(headerName))
            return null;

        context.Request.Headers.TryGetValue(headerName, out var value);
        return value.FirstOrDefault();
    }

    /// <summary>
    /// Gets the authorization token from the Authorization header
    /// </summary>
    public static string? GetAuthorizationToken(this HttpContext context)
    {
        var authHeader = context.GetHeader("Authorization");
        if (string.IsNullOrWhiteSpace(authHeader))
            return null;

        var parts = authHeader.Split(' ');
        if (parts.Length != 2 || parts[0] != "Bearer")
            return null;

        return parts[1];
    }

    /// <summary>
    /// Gets the request ID (or creates one if not present)
    /// </summary>
    public static string GetRequestId(this HttpContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        var requestId = context.GetHeader("X-Request-ID");
        if (!string.IsNullOrWhiteSpace(requestId))
            return requestId;

        return context.TraceIdentifier;
    }

    /// <summary>
    /// Checks if the request is a gRPC request
    /// </summary>
    public static bool IsGrpcRequest(this HttpContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        var contentType = context.Request.ContentType ?? "";
        return contentType.Contains("application/grpc");
    }

    /// <summary>
    /// Checks if the request is a gRPC-Web request
    /// </summary>
    public static bool IsGrpcWebRequest(this HttpContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        var contentType = context.Request.ContentType ?? "";
        return contentType.Contains("application/grpc-web");
    }
}
