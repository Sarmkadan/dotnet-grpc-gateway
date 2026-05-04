// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Infrastructure;

/// <summary>
/// Structured logging helper that provides consistent logging with context.
/// Used across the application for standardized log messages and diagnostic data.
/// </summary>
public static class StructuredLogger
{
    /// <summary>
    /// Logs a request start event with context information.
    /// </summary>
    public static void LogRequestStart(ILogger logger, string requestId, string path, string method, string clientIp)
    {
        logger.LogInformation(
            "Request started - RequestId: {RequestId}, Path: {Path}, Method: {Method}, ClientIp: {ClientIp}",
            requestId, path, method, clientIp);
    }

    /// <summary>
    /// Logs a request completion with duration and status.
    /// </summary>
    public static void LogRequestComplete(ILogger logger, string requestId, string path, int statusCode, long durationMs)
    {
        var logLevel = statusCode >= 500 ? LogLevel.Error :
                       statusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

        logger.Log(logLevel,
            "Request completed - RequestId: {RequestId}, Path: {Path}, StatusCode: {StatusCode}, DurationMs: {DurationMs}",
            requestId, path, statusCode, durationMs);
    }

    /// <summary>
    /// Logs a service discovery event.
    /// </summary>
    public static void LogServiceDiscovery(ILogger logger, int serviceId, string serviceName, bool healthy)
    {
        logger.LogInformation(
            "Service discovery check - ServiceId: {ServiceId}, ServiceName: {ServiceName}, Healthy: {Healthy}",
            serviceId, serviceName, healthy);
    }

    /// <summary>
    /// Logs a cache operation.
    /// </summary>
    public static void LogCacheOperation(ILogger logger, string operation, string key, bool hit)
    {
        logger.LogDebug("Cache operation - Operation: {Operation}, Key: {Key}, Hit: {Hit}",
            operation, key, hit);
    }

    /// <summary>
    /// Logs a route resolution.
    /// </summary>
    public static void LogRouteResolution(ILogger logger, string path, string routePattern, int targetServiceId)
    {
        logger.LogInformation(
            "Route resolved - Path: {Path}, Pattern: {Pattern}, TargetServiceId: {TargetServiceId}",
            path, routePattern, targetServiceId);
    }

    /// <summary>
    /// Logs a rate limit event.
    /// </summary>
    public static void LogRateLimit(ILogger logger, string clientIp, string path, int limit)
    {
        logger.LogWarning("Rate limit exceeded - ClientIp: {ClientIp}, Path: {Path}, Limit: {Limit}",
            clientIp, path, limit);
    }

    /// <summary>
    /// Logs an authentication event.
    /// </summary>
    public static void LogAuthentication(ILogger logger, string? userId, bool success, string? reason = null)
    {
        var logLevel = success ? LogLevel.Information : LogLevel.Warning;
        logger.Log(logLevel, "Authentication - UserId: {UserId}, Success: {Success}, Reason: {Reason}",
            userId ?? "unknown", success, reason ?? "");
    }

    /// <summary>
    /// Logs a critical error with full context.
    /// </summary>
    public static void LogCriticalError(ILogger logger, Exception ex, string context, Dictionary<string, object>? additionalData = null)
    {
        var logData = new Dictionary<string, object> { { "context", context } };

        if (additionalData != null)
        {
            foreach (var kvp in additionalData)
                logData[kvp.Key] = kvp.Value;
        }

        logger.LogError(ex, "Critical error occurred - Context: {Context}, AdditionalData: {@AdditionalData}",
            context, logData);
    }

    /// <summary>
    /// Logs performance metrics.
    /// </summary>
    public static void LogPerformanceMetrics(ILogger logger, string operation, long durationMs, int? itemCount = null)
    {
        if (itemCount.HasValue)
            logger.LogInformation("Performance - Operation: {Operation}, DurationMs: {DurationMs}, ItemCount: {ItemCount}",
                operation, durationMs, itemCount);
        else
            logger.LogInformation("Performance - Operation: {Operation}, DurationMs: {DurationMs}",
                operation, durationMs);
    }
}
