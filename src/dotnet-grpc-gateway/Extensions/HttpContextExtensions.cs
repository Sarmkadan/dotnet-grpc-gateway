#nullable enable
// ====================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

using System.Diagnostics;
using System.Net;
using System.Text;

namespace DotNetGrpcGateway.Extensions;

/// <summary>
/// Extension methods for <see cref="HttpContext"/> providing common HTTP request utilities
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Gets the client's IP address, accounting for proxies with proper security validation
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> instance</param>
    /// <returns>The client IP address as a string, or "unknown" if not determinable</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null</exception>
    /// <remarks>
    /// This method properly handles X-Forwarded-For headers by:
    /// <list type="bullet">
    /// <item>Relying on ForwardedHeadersMiddleware to validate headers from trusted proxies</item>
    /// <item>Taking the rightmost IP as the client IP (most recent proxy)</item>
    /// <item>Validating IP addresses to prevent injection attacks</item>
    /// <item>Falling back to RemoteIpAddress if no valid forwarded header is present</item>
    /// </list>
    /// </remarks>
    public static string GetClientIpAddress(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // ForwardedHeadersMiddleware has processed the request and sets RemoteIpAddress correctly
        // This is the primary source of truth when ForwardedHeadersMiddleware is properly configured
        var remoteIp = context.Connection.RemoteIpAddress?.ToString();
        if (!string.IsNullOrEmpty(remoteIp))
        {
            return remoteIp;
        }

        // Fallback for cases where ForwardedHeadersMiddleware is not configured
        // Check for X-Forwarded-For header (set by proxies)
        // SECURITY: Validate that the header comes from a trusted source
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
        {
            var ips = forwarded.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);

            if (ips.Length > 0)
            {
                // Take the rightmost IP as the client IP (most recent proxy)
                // This prevents client spoofing where clients can set X-Forwarded-For: 1.2.3.4, attacker.com
                var clientIp = ips[^1].Trim();

                // Validate the IP address format to prevent injection attacks
                if (IsValidIpAddress(clientIp))
                {
                    return clientIp;
                }
            }
        }

        // Fall back to RemoteIpAddress
        return remoteIp ?? "unknown";

        static bool IsValidIpAddress(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                return false;
            }

            // Check for IPv4 format
            if (ipAddress.Contains('.') && !ipAddress.Contains(':'))
            {
                var parts = ipAddress.Split('.', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 4 && parts.All(p => byte.TryParse(p, out _)))
                {
                    return true;
                }
            }

            // Check for IPv6 format (simplified validation)
            if (ipAddress.Contains(':') && !ipAddress.Contains('.'))
            {
                // Basic validation - IPv6 addresses can be complex but we just check format
                return ipAddress.Split(':').Length >= 3;
            }

            return false;
        }
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
    /// Gets the request ID (or creates one if not present) with validation
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> instance</param>
    /// <returns>The validated request ID from X-Request-ID header or TraceIdentifier</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null</exception>
    /// <remarks>
    /// Validates that the X-Request-ID header contains only safe characters and is not excessively long
    /// </remarks>
    public static string GetRequestId(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var requestId = context.GetHeader("X-Request-ID");

        if (!string.IsNullOrWhiteSpace(requestId))
        {
            // Validate correlation ID format - should be alphanumeric with hyphens/underscores
            // Limit length to prevent abuse
            if (requestId.Length > 128)
            {
                requestId = requestId[..128];
            }

            // Remove any dangerous characters
            var sanitized = new StringBuilder(requestId.Length);
            foreach (var c in requestId)
            {
                if (char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '.')
                {
                    sanitized.Append(c);
                }
            }

            if (sanitized.Length > 0)
            {
                return sanitized.ToString();
            }
        }

        return context.TraceIdentifier;
    }

    /// <summary>
    /// Gets the correlation ID with validation and sanitization
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> instance</param>
    /// <returns>The validated correlation ID or generated request ID</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null</exception>
    /// <remarks>
    /// <para>
    /// This method supports both legacy X-Correlation-ID headers and W3C Trace Context headers.
    /// It first checks for the traceparent header (W3C standard), then falls back to X-Correlation-ID.
    /// </para>
    /// <para>
    /// If a valid traceparent header is found, it extracts the trace ID and creates an Activity
    /// to ensure proper W3C Trace Context propagation throughout the request pipeline.
    /// </para>
    /// </remarks>
    public static string GetCorrelationId(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // First, check for W3C Trace Context traceparent header (preferred)
        var traceParent = context.GetHeader("traceparent");
        if (!string.IsNullOrWhiteSpace(traceParent) && TryParseTraceParent(traceParent, out var traceId))
        {
            // Create or update Activity to ensure W3C Trace Context flows through the pipeline
            // This ensures Activity.Current will have the correct TraceId for RequestContext
            var activity = new Activity("HTTP Incoming Request");
            activity.Start();

            return traceId.ToHexString();
        }

        // Fallback to legacy X-Correlation-ID header
        var correlationId = context.GetHeader("X-Correlation-ID");

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            // Validate correlation ID format - should be alphanumeric with hyphens/underscores
            // Limit length to prevent abuse
            if (correlationId.Length > 128)
            {
                correlationId = correlationId[..128];
            }

            // Remove any dangerous characters
            var sanitized = new StringBuilder(correlationId.Length);
            foreach (var c in correlationId)
            {
                if (char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '.')
                {
                    sanitized.Append(c);
                }
            }

            if (sanitized.Length > 0)
            {
                return sanitized.ToString();
            }
        }

        // Return request ID as fallback
        return context.GetRequestId();

        /// <summary>
        /// Attempts to parse a W3C traceparent header value.
        /// </summary>
        /// <param name="traceParent">The traceparent header value</param>
        /// <param name="traceId">Output parameter for the parsed TraceId</param>
        /// <returns>True if parsing succeeded, false otherwise</returns>
        static bool TryParseTraceParent(string traceParent, out ActivityTraceId traceId)
        {
            traceId = default;

            // W3C Trace Context traceparent format: version-trace-id-parent-id-trace-flags
            // Example: 00-4bf92f3577b34da6a96410d8a26f5a8d-00f067aa0ba902b7-01

            var parts = traceParent.Split('-');
            if (parts.Length != 4)
                return false;

            // Validate version (currently only "00" is valid)
            if (parts[0] != "00")
                return false;

            // Validate and parse trace ID (32 hex chars)
    // Validate and parse trace ID (32 hex chars)
    if (parts[1].Length != 32)
        return false;
    try
    {
        traceId = ActivityTraceId.CreateFromString(parts[1]);
    }
    catch
    {
        return false;
    }

    // Validate parent ID (16 hex chars)
    if (parts[2].Length != 16)
        return false;
    try
    {
        ActivitySpanId.CreateFromString(parts[2]);
    }
    catch
    {
        return false;
    }

            // Validate trace flags (2 hex chars)
            if (parts[3].Length != 2 || !int.TryParse(parts[3], System.Globalization.NumberStyles.HexNumber, null, out _))
                return false;

            return true;
        }
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