// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using DotNetGrpcGateway.Extensions;

namespace DotNetGrpcGateway.Middleware;

/// <summary>
/// Token bucket rate limiting middleware. Prevents API abuse by limiting requests per client.
/// Tracks request counts per IP address with configurable window and limits.
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly int _requestsPerWindow;
    private readonly int _windowSeconds;
    // Key: IP address, Value: (request count, window start time)
    private readonly ConcurrentDictionary<string, (int count, DateTime windowStart)> _requestTracker = new();

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger,
        int requestsPerWindow = 100, int windowSeconds = 60)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _requestsPerWindow = requestsPerWindow;
        _windowSeconds = windowSeconds;

        // Cleanup old entries periodically
        _ = CleanupExpiredEntriesAsync();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = context.GetClientIpAddress();

        if (!IsRateLimited(clientIp))
        {
            await _next(context);
            return;
        }

        // Rate limit exceeded - return 429 Too Many Requests
        _logger.LogWarning("Rate limit exceeded for IP: {ClientIp}", clientIp);

        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "Rate limit exceeded",
            clientIp = clientIp,
            retryAfterSeconds = _windowSeconds
        };

        await context.Response.WriteAsJsonAsync(response);
    }

    private bool IsRateLimited(string clientIp)
    {
        var now = DateTime.UtcNow;
        var key = clientIp;

        while (true)
        {
            // Try to get or create entry for this IP
            if (_requestTracker.TryGetValue(key, out var entry))
            {
                var (count, windowStart) = entry;
                var elapsed = now - windowStart;

                // Window has expired, reset counter
                if (elapsed.TotalSeconds >= _windowSeconds)
                {
                    if (_requestTracker.TryUpdate(key, (1, now), entry))
                        return false;
                }
                // Within window and under limit
                else if (count < _requestsPerWindow)
                {
                    if (_requestTracker.TryUpdate(key, (count + 1, windowStart), entry))
                        return false;
                }
                // Rate limit exceeded
                else
                {
                    return true;
                }
            }
            else
            {
                // First request from this IP
                if (_requestTracker.TryAdd(key, (1, now)))
                    return false;
            }
        }
    }

    private async Task CleanupExpiredEntriesAsync()
    {
        // Runs every minute to clean up old entries from inactive clients
        while (true)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1));

                var now = DateTime.UtcNow;
                var expiredKeys = _requestTracker
                    .Where(x => (now - x.Value.windowStart).TotalSeconds > _windowSeconds * 2)
                    .Select(x => x.Key)
                    .ToList();

                foreach (var key in expiredKeys)
                {
                    _requestTracker.TryRemove(key, out _);
                }

                if (expiredKeys.Count > 0)
                    _logger.LogDebug("Cleaned up {Count} expired rate limit entries", expiredKeys.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during rate limit cleanup");
            }
        }
    }
}
