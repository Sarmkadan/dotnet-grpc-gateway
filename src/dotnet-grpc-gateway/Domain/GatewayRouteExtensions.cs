#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text;

namespace DotNetGrpcGateway.Domain;

/// <summary>
/// Extension methods for <see cref="GatewayRoute"/> that provide common routing operations and utilities.
/// </summary>
public static class GatewayRouteExtensions
{
    /// <summary>
    /// Determines whether this route should handle a request based on the provided service and method names.
    /// </summary>
    /// <param name="route">The gateway route to check</param>
    /// <param name="serviceName">The gRPC service name</param>
    /// <param name="methodName">The gRPC method name</param>
    /// <returns>True if the route matches the request; otherwise, false</returns>
    /// <exception cref="ArgumentNullException"><paramref name="route"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="serviceName"/> is null or whitespace</exception>
    /// <exception cref="ArgumentException"><paramref name="methodName"/> is null or whitespace</exception>
    public static bool ShouldHandleRequest(this GatewayRoute route, string serviceName, string methodName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);
        ArgumentException.ThrowIfNullOrWhiteSpace(methodName);

        return route.MatchesRequest(serviceName, methodName);
    }

    /// <summary>
    /// Gets the effective rate limit for this route, considering both the route-specific limit and system defaults.
    /// </summary>
    /// <param name="route">The gateway route</param>
    /// <param name="defaultRateLimit">The system-wide default rate limit to use as fallback</param>
    /// <returns>The effective rate limit per minute</returns>
    /// <exception cref="ArgumentNullException"><paramref name="route"/> is null</exception>
    public static int GetEffectiveRateLimit(this GatewayRoute route, int defaultRateLimit = 1000)
    {
        ArgumentNullException.ThrowIfNull(route);

        return route.RateLimitPerMinute > 0 ? route.RateLimitPerMinute : defaultRateLimit;
    }

    /// <summary>
    /// Gets the effective cache duration for this route, considering both the route-specific duration and system defaults.
    /// </summary>
    /// <param name="route">The gateway route</param>
    /// <param name="defaultCacheDuration">The system-wide default cache duration in seconds</param>
    /// <returns>The effective cache duration in seconds, or 0 if caching is disabled</returns>
    /// <exception cref="ArgumentNullException"><paramref name="route"/> is null</exception>
    public static int GetEffectiveCacheDuration(this GatewayRoute route, int defaultCacheDuration = 60)
    {
        ArgumentNullException.ThrowIfNull(route);

        return !route.EnableCaching ? 0 : route.CacheDurationSeconds >= 0 ? route.CacheDurationSeconds : defaultCacheDuration;
    }

    /// <summary>
    /// Creates a diagnostic string for this route that includes key routing information.
    /// Useful for logging, debugging, and monitoring.
    /// </summary>
    /// <param name="route">The gateway route</param>
    /// <returns>A formatted diagnostic string</returns>
    /// <exception cref="ArgumentNullException"><paramref name="route"/> is null</exception>
    public static string ToDiagnosticString(this GatewayRoute route)
    {
        ArgumentNullException.ThrowIfNull(route);

        var sb = new StringBuilder();
        sb.AppendLine($"GatewayRoute Diagnostic Information:");
        sb.AppendLine($" ID: {route.Id}");
        sb.AppendLine($" Pattern: {route.Pattern}");
        sb.AppendLine($" Target Service ID: {route.TargetServiceId}");
        sb.AppendLine($" Priority: {route.Priority}");
        sb.AppendLine($" Match Type: {route.MatchType}");
        sb.AppendLine($" Description: {route.Description ?? "(none)"}");
        sb.AppendLine($" Active: {route.IsActive}");
        sb.AppendLine($" Requires Authentication: {route.RequiresAuthentication}");
        sb.AppendLine($" Rate Limit: {route.GetEffectiveRateLimit()} per minute");
        sb.AppendLine($" Caching: {(route.EnableCaching ? $"Enabled ({route.GetEffectiveCacheDuration()}s)" : "Disabled")}");
        sb.AppendLine($" Compression: {(route.EnableCompression ? "Enabled" : "Disabled")}");
        sb.AppendLine($" Created: {route.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($" Modified: {route.ModifiedAt:yyyy-MM-dd HH:mm:ss}");

        if (route.Headers.Count > 0)
        {
            sb.AppendLine(" Headers:");
            foreach (var header in route.Headers)
            {
                sb.AppendLine($"  {header.Key}: {header.Value}");
            }
        }

        if (route.Metadata.Count > 0)
        {
            sb.AppendLine(" Metadata:");
            foreach (var meta in route.Metadata)
            {
                sb.AppendLine($"  {meta.Key}: {meta.Value}");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Determines whether this route requires authentication based on either the route-specific setting
    /// or the presence of an authorization policy.
    /// </summary>
    /// <param name="route">The gateway route</param>
    /// <returns>True if authentication is required; otherwise, false</returns>
    /// <exception cref="ArgumentNullException"><paramref name="route"/> is null</exception>
    public static bool RequiresAuth(this GatewayRoute route)
    {
        ArgumentNullException.ThrowIfNull(route);

        return route.RequiresAuthentication || !string.IsNullOrWhiteSpace(route.AuthorizationPolicy);
    }
}