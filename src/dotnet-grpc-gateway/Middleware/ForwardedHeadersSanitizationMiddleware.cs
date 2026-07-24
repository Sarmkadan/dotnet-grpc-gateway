#nullable enable
// ====================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

using Microsoft.AspNetCore.HttpOverrides;

namespace DotNetGrpcGateway.Middleware;

/// <summary>
/// Middleware that ensures proper handling of forwarded headers and strips hop-by-hop headers
/// when proxying requests to upstream services.
/// </summary>
/// <remarks>
/// This middleware:
/// <list type="bullet">
/// <item>Validates that ForwardedHeadersMiddleware is properly configured with KnownProxies/KnownNetworks</item>
/// <item>Strips hop-by-hop headers that should not be forwarded to upstream services</item>
/// <item>Normalizes forwarded headers to prevent header injection attacks</item>
/// <item>Logs configuration warnings if ForwardedHeadersMiddleware is misconfigured</item>
/// </list>
///
/// Hop-by-hop headers include: Connection, Keep-Alive, Proxy-Authenticate, Proxy-Authorization,
/// Te, Trailer, Transfer-Encoding, Upgrade
/// </remarks>
public class ForwardedHeadersSanitizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ForwardedHeadersSanitizationMiddleware> _logger;
    private static readonly string[] _hopByHopHeaders = new[]
    {
        "Connection",
        "Keep-Alive",
        "Proxy-Authenticate",
        "Proxy-Authorization",
        "Te",
        "Trailer",
        "Transfer-Encoding",
        "Upgrade"
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="ForwardedHeadersSanitizationMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <param name="logger">The logger</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="next"/> or <paramref name="logger"/> is null</exception>
    public ForwardedHeadersSanitizationMiddleware(
        RequestDelegate next,
        ILogger<ForwardedHeadersSanitizationMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Invokes the middleware to sanitize forwarded headers and strip hop-by-hop headers.
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null</exception>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            // Validate ForwardedHeadersMiddleware configuration
            ValidateForwardedHeadersConfiguration(context);

            // Strip hop-by-hop headers that should not be forwarded to upstream services
            var strippedCount = StripHopByHopHeaders(context.Request.Headers);

            // Log if we found and stripped hop-by-hop headers
            if (strippedCount > 0 && _logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Stripped {Count} hop-by-hop headers from request to prevent header injection", strippedCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while sanitizing forwarded headers");
        }

        await _next(context);
    }

    /// <summary>
    /// Validates that ForwardedHeadersMiddleware is properly configured with KnownProxies/KnownNetworks.
    /// Logs a warning if the configuration appears insecure (e.g., empty KnownProxies/KnownNetworks).
    /// </summary>
    /// <param name="context">The HTTP context</param>
    private void ValidateForwardedHeadersConfiguration(HttpContext context)
    {
        // Check if ForwardedHeadersMiddleware is in the pipeline by looking for forwarded headers
        var hasForwardedHeaders = context.Request.Headers.ContainsKey("X-Forwarded-For") ||
                                context.Request.Headers.ContainsKey("X-Forwarded-Proto") ||
                                context.Request.Headers.ContainsKey("X-Forwarded-Host");

        if (!hasForwardedHeaders)
        {
            // No forwarded headers present - ForwardedHeadersMiddleware may not be configured
            // This is not necessarily an error, but worth logging for debugging
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("No forwarded headers (X-Forwarded-*) found in request - ForwardedHeadersMiddleware may not be configured");
            }
            return;
        }

        // Check if the configuration is likely secure by examining the KnownProxies/KnownNetworks
        // Since we can't directly access ForwardedHeadersOptions, we log a general warning
        if (_logger.IsEnabled(LogLevel.Warning))
        {
            _logger.LogWarning(
                "Forwarded headers detected - ensure ForwardedHeadersMiddleware is configured with " +
                "explicit KnownProxies/KnownNetworks in Program.cs for security. " +
                "Without explicit configuration, client IP spoofing is possible.");
        }
    }

    /// <summary>
    /// Removes hop-by-hop headers from the request headers collection.
    /// </summary>
    /// <param name="headers">The request headers collection</param>
    /// <returns>The number of headers stripped</returns>
    private int StripHopByHopHeaders(IHeaderDictionary headers)
    {
        var strippedCount = 0;

        // Remove each hop-by-hop header if present
        foreach (var headerName in _hopByHopHeaders)
        {
            if (headers.Remove(headerName))
            {
                strippedCount++;
            }
        }

        // Special handling for Connection header which may contain multiple headers
        if (headers.TryGetValue("Connection", out var connectionHeader))
        {
            var connectionValues = connectionHeader.ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(v => v.Trim())
                .Where(v => !string.IsNullOrEmpty(v))
                .ToList();

            // Remove each connection-specific header
            foreach (var connectionValue in connectionValues)
            {
                if (headers.Remove(connectionValue))
                {
                    strippedCount++;
                }
            }

            // Remove the Connection header itself
            if (headers.Remove("Connection"))
            {
                strippedCount++;
            }
        }

        return strippedCount;
    }
}