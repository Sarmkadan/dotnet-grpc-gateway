#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Infrastructure;

/// <summary>
/// Provides validation helpers for StructuredLogger method parameters.
/// Validates input parameters before they are passed to logging methods to ensure
/// they meet expected criteria and prevent invalid log entries.
/// </summary>
public sealed class StructuredLoggerValidation
{
    /// <summary>
    /// Validates all parameters for the <see cref="ILogger"/> LogRequestStart method.
    /// </summary>
    /// <param name="logger">The logger instance. Cannot be null.</param>
    /// <param name="requestId">The request identifier. Must not be null or whitespace and must not exceed 100 characters.</param>
    /// <param name="path">The request path. Must not be null or whitespace and must not exceed 500 characters.</param>
    /// <param name="method">The HTTP method. Must not be null or whitespace and must not exceed 20 characters.</param>
    /// <param name="clientIp">The client IP address. Must not be null or whitespace and must not exceed 45 characters.</param>
    /// <returns>List of validation problems; empty if all parameters are valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is null.</exception>
    public static IReadOnlyList<string> ValidateLogRequestStart(
        ILogger logger,
        string requestId,
        string path,
        string method,
        string clientIp)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(method);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientIp);

        var problems = new List<string>();

        if (requestId.Length > 100)
        {
            problems.Add("RequestId exceeds maximum length of 100 characters");
        }

        if (path.Length > 500)
        {
            problems.Add("Path exceeds maximum length of 500 characters");
        }

        if (method.Length > 20)
        {
            problems.Add("Method exceeds maximum length of 20 characters");
        }

        if (clientIp.Length > 45)
        {
            problems.Add("ClientIp exceeds maximum length of 45 characters");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates all parameters for the <see cref="ILogger"/> LogRequestComplete method.
    /// </summary>
    /// <param name="logger">The logger instance. Cannot be null.</param>
    /// <param name="requestId">The request identifier. Must not be null or whitespace and must not exceed 100 characters.</param>
    /// <param name="path">The request path. Must not be null or whitespace and must not exceed 500 characters.</param>
    /// <param name="statusCode">The HTTP status code. Must be between 100 and 999 inclusive.</param>
    /// <param name="durationMs">The request duration in milliseconds. Must not be negative.</param>
    /// <returns>List of validation problems; empty if all parameters are valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is null.</exception>
    public static IReadOnlyList<string> ValidateLogRequestComplete(
        ILogger logger,
        string requestId,
        string path,
        int statusCode,
        long durationMs)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var problems = new List<string>();

        if (requestId.Length > 100)
        {
            problems.Add("RequestId exceeds maximum length of 100 characters");
        }

        if (path.Length > 500)
        {
            problems.Add("Path exceeds maximum length of 500 characters");
        }

        if (statusCode < 100 || statusCode > 999)
        {
            problems.Add("StatusCode must be between 100 and 999");
        }

        if (durationMs < 0)
        {
            problems.Add("DurationMs cannot be negative");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates all parameters for the <see cref="ILogger"/> LogServiceDiscovery method.
    /// </summary>
    /// <param name="logger">The logger instance. Cannot be null.</param>
    /// <param name="serviceId">The service identifier. Must be a positive integer.</param>
    /// <param name="serviceName">The service name. Must not be null or whitespace and must not exceed 100 characters.</param>
    /// <param name="healthy">Whether the service is healthy.</param>
    /// <returns>List of validation problems; empty if all parameters are valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is null.</exception>
    public static IReadOnlyList<string> ValidateLogServiceDiscovery(
        ILogger logger,
        int serviceId,
        string serviceName,
        bool healthy)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        var problems = new List<string>();

        if (serviceName.Length > 100)
        {
            problems.Add("ServiceName exceeds maximum length of 100 characters");
        }

        if (serviceId <= 0)
        {
            problems.Add("ServiceId must be a positive integer");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates all parameters for the <see cref="ILogger"/> LogCacheOperation method.
    /// </summary>
    /// <param name="logger">The logger instance. Cannot be null.</param>
    /// <param name="operation">The cache operation type. Must not be null or whitespace and must not exceed 50 characters.</param>
    /// <param name="key">The cache key. Must not be null or whitespace and must not exceed 200 characters.</param>
    /// <param name="hit">Whether the operation was a cache hit.</param>
    /// <returns>List of validation problems; empty if all parameters are valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is null.</exception>
    public static IReadOnlyList<string> ValidateLogCacheOperation(
        ILogger logger,
        string operation,
        string key,
        bool hit)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var problems = new List<string>();

        if (operation.Length > 50)
        {
            problems.Add("Operation exceeds maximum length of 50 characters");
        }

        if (key.Length > 200)
        {
            problems.Add("Key exceeds maximum length of 200 characters");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates all parameters for the <see cref="ILogger"/> LogRouteResolution method.
    /// </summary>
    /// <param name="logger">The logger instance. Cannot be null.</param>
    /// <param name="path">The request path. Must not be null or whitespace and must not exceed 500 characters.</param>
    /// <param name="routePattern">The route pattern. Must not be null or whitespace and must not exceed 200 characters.</param>
    /// <param name="targetServiceId">The target service identifier. Must be a positive integer.</param>
    /// <returns>List of validation problems; empty if all parameters are valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is null.</exception>
    public static IReadOnlyList<string> ValidateLogRouteResolution(
        ILogger logger,
        string path,
        string routePattern,
        int targetServiceId)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(routePattern);

        var problems = new List<string>();

        if (path.Length > 500)
        {
            problems.Add("Path exceeds maximum length of 500 characters");
        }

        if (routePattern.Length > 200)
        {
            problems.Add("RoutePattern exceeds maximum length of 200 characters");
        }

        if (targetServiceId <= 0)
        {
            problems.Add("TargetServiceId must be a positive integer");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates all parameters for the <see cref="ILogger"/> LogRateLimit method.
    /// </summary>
    /// <param name="logger">The logger instance. Cannot be null.</param>
    /// <param name="clientIp">The client IP address. Must not be null or whitespace and must not exceed 45 characters.</param>
    /// <param name="path">The request path. Must not be null or whitespace and must not exceed 500 characters.</param>
    /// <param name="limit">The rate limit value. Must be a positive integer.</param>
    /// <returns>List of validation problems; empty if all parameters are valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is null.</exception>
    public static IReadOnlyList<string> ValidateLogRateLimit(
        ILogger logger,
        string clientIp,
        string path,
        int limit)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientIp);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var problems = new List<string>();

        if (clientIp.Length > 45)
        {
            problems.Add("ClientIp exceeds maximum length of 45 characters");
        }

        if (path.Length > 500)
        {
            problems.Add("Path exceeds maximum length of 500 characters");
        }

        if (limit <= 0)
        {
            problems.Add("Limit must be a positive integer");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates all parameters for the <see cref="ILogger"/> LogAuthentication method.
    /// </summary>
    /// <param name="logger">The logger instance. Cannot be null.</param>
    /// <param name="userId">The user identifier. Optional; if provided, must not exceed 50 characters.</param>
    /// <param name="success">Whether authentication succeeded.</param>
    /// <param name="reason">Optional failure reason. If provided, must not exceed 200 characters.</param>
    /// <returns>List of validation problems; empty if all parameters are valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is null.</exception>
    public static IReadOnlyList<string> ValidateLogAuthentication(
        ILogger logger,
        string? userId,
        bool success,
        string? reason = null)
    {
        ArgumentNullException.ThrowIfNull(logger);

        var problems = new List<string>();

        if (userId is not null && userId.Length > 50)
        {
            problems.Add("UserId exceeds maximum length of 50 characters");
        }

        if (reason is not null && reason.Length > 200)
        {
            problems.Add("Reason exceeds maximum length of 200 characters");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates all parameters for the <see cref="ILogger"/> LogCriticalError method.
    /// </summary>
    /// <param name="logger">The logger instance. Cannot be null.</param>
    /// <param name="ex">The exception to log. Cannot be null.</param>
    /// <param name="context">The error context. Must not be null or whitespace and must not exceed 200 characters.</param>
    /// <param name="additionalData">Optional additional data dictionary. If provided, must not exceed 20 entries, keys must not exceed 100 characters, and string values must not exceed 500 characters.</param>
    /// <returns>List of validation problems; empty if all parameters are valid.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="logger"/> is null.
    /// Thrown if <paramref name="ex"/> is null.
    /// Thrown if <paramref name="context"/> is null.
    /// </exception>
    public static IReadOnlyList<string> ValidateLogCriticalError(
        ILogger logger,
        Exception ex,
        string context,
        Dictionary<string, object>? additionalData = null)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(ex);
        ArgumentException.ThrowIfNullOrWhiteSpace(context);

        var problems = new List<string>();

        if (context.Length > 200)
        {
            problems.Add("Context exceeds maximum length of 200 characters");
        }

        if (additionalData is not null)
        {
            if (additionalData.Count > 20)
            {
                problems.Add("AdditionalData contains more than 20 entries");
            }

            foreach (var kvp in additionalData)
            {
                if (kvp.Key.Length > 100)
                {
                    problems.Add($"AdditionalData key '{kvp.Key}' exceeds maximum length of 100 characters");
                    break;
                }

                if (kvp.Value is string strValue && strValue.Length > 500)
                {
                    problems.Add($"AdditionalData value for key '{kvp.Key}' exceeds maximum length of 500 characters");
                    break;
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates all parameters for the <see cref="ILogger"/> LogPerformanceMetrics method.
    /// </summary>
    /// <param name="logger">The logger instance. Cannot be null.</param>
    /// <param name="operation">The operation name. Must not be null or whitespace and must not exceed 100 characters.</param>
    /// <param name="durationMs">The operation duration in milliseconds. Must not be negative.</param>
    /// <param name="itemCount">Optional item count. If provided, must be a positive integer between 1 and 1,000,000 inclusive.</param>
    /// <returns>List of validation problems; empty if all parameters are valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is null.</exception>
    public static IReadOnlyList<string> ValidateLogPerformanceMetrics(
        ILogger logger,
        string operation,
        long durationMs,
        int? itemCount = null)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);

        var problems = new List<string>();

        if (operation.Length > 100)
        {
            problems.Add("Operation exceeds maximum length of 100 characters");
        }

        if (durationMs < 0)
        {
            problems.Add("DurationMs cannot be negative");
        }

        if (itemCount.HasValue && (itemCount <= 0 || itemCount > 1_000_000))
        {
            problems.Add("ItemCount must be a positive integer between 1 and 1,000,000");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if all parameters for LogRequestStart are valid.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="requestId">The request identifier.</param>
    /// <param name="path">The request path.</param>
    /// <param name="method">The HTTP method.</param>
    /// <param name="clientIp">The client IP address.</param>
    /// <returns>True if all parameters are valid; otherwise, false.</returns>
    public static bool IsValidLogRequestStart(
        ILogger logger,
        string requestId,
        string path,
        string method,
        string clientIp) =>
        ValidateLogRequestStart(logger, requestId, path, method, clientIp).Count == 0;

    /// <summary>
    /// Checks if all parameters for LogRequestComplete are valid.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="requestId">The request identifier.</param>
    /// <param name="path">The request path.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="durationMs">The request duration in milliseconds.</param>
    /// <returns>True if all parameters are valid; otherwise, false.</returns>
    public static bool IsValidLogRequestComplete(
        ILogger logger,
        string requestId,
        string path,
        int statusCode,
        long durationMs) =>
        ValidateLogRequestComplete(logger, requestId, path, statusCode, durationMs).Count == 0;

    /// <summary>
    /// Checks if all parameters for LogServiceDiscovery are valid.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="serviceId">The service identifier.</param>
    /// <param name="serviceName">The service name.</param>
    /// <param name="healthy">Whether the service is healthy.</param>
    /// <returns>True if all parameters are valid; otherwise, false.</returns>
    public static bool IsValidLogServiceDiscovery(
        ILogger logger,
        int serviceId,
        string serviceName,
        bool healthy) =>
        ValidateLogServiceDiscovery(logger, serviceId, serviceName, healthy).Count == 0;

    /// <summary>
    /// Checks if all parameters for LogCacheOperation are valid.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="operation">The cache operation type.</param>
    /// <param name="key">The cache key.</param>
    /// <param name="hit">Whether the operation was a cache hit.</param>
    /// <returns>True if all parameters are valid; otherwise, false.</returns>
    public static bool IsValidLogCacheOperation(
        ILogger logger,
        string operation,
        string key,
        bool hit) =>
        ValidateLogCacheOperation(logger, operation, key, hit).Count == 0;

    /// <summary>
    /// Checks if all parameters for LogRouteResolution are valid.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="path">The request path.</param>
    /// <param name="routePattern">The route pattern.</param>
    /// <param name="targetServiceId">The target service identifier.</param>
    /// <returns>True if all parameters are valid; otherwise, false.</returns>
    public static bool IsValidLogRouteResolution(
        ILogger logger,
        string path,
        string routePattern,
        int targetServiceId) =>
        ValidateLogRouteResolution(logger, path, routePattern, targetServiceId).Count == 0;

    /// <summary>
    /// Checks if all parameters for LogRateLimit are valid.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="clientIp">The client IP address.</param>
    /// <param name="path">The request path.</param>
    /// <param name="limit">The rate limit value.</param>
    /// <returns>True if all parameters are valid; otherwise, false.</returns>
    public static bool IsValidLogRateLimit(
        ILogger logger,
        string clientIp,
        string path,
        int limit) =>
        ValidateLogRateLimit(logger, clientIp, path, limit).Count == 0;

    /// <summary>
    /// Checks if all parameters for LogAuthentication are valid.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="success">Whether authentication succeeded.</param>
    /// <param name="reason">Optional failure reason.</param>
    /// <returns>True if all parameters are valid; otherwise, false.</returns>
    public static bool IsValidLogAuthentication(
        ILogger logger,
        string? userId,
        bool success,
        string? reason = null) =>
        ValidateLogAuthentication(logger, userId, success, reason).Count == 0;

    /// <summary>
    /// Checks if all parameters for LogCriticalError are valid.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception to log.</param>
    /// <param name="context">The error context.</param>
    /// <param name="additionalData">Optional additional data dictionary.</param>
    /// <returns>True if all parameters are valid; otherwise, false.</returns>
    public static bool IsValidLogCriticalError(
        ILogger logger,
        Exception ex,
        string context,
        Dictionary<string, object>? additionalData = null) =>
        ValidateLogCriticalError(logger, ex, context, additionalData).Count == 0;

    /// <summary>
    /// Checks if all parameters for LogPerformanceMetrics are valid.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="operation">The operation name.</param>
    /// <param name="durationMs">The operation duration in milliseconds.</param>
    /// <param name="itemCount">Optional item count.</param>
    /// <returns>True if all parameters are valid; otherwise, false.</returns>
    public static bool IsValidLogPerformanceMetrics(
        ILogger logger,
        string operation,
        long durationMs,
        int? itemCount = null) =>
        ValidateLogPerformanceMetrics(logger, operation, durationMs, itemCount).Count == 0;

    /// <summary>
    /// Ensures all parameters for LogRequestStart are valid, throwing ArgumentException if not.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="requestId">The request identifier.</param>
    /// <param name="path">The request path.</param>
    /// <param name="method">The HTTP method.</param>
    /// <param name="clientIp">The client IP address.</param>
    /// <exception cref="ArgumentException">Thrown if any parameter is invalid.</exception>
    public static void EnsureValidLogRequestStart(
        ILogger logger,
        string requestId,
        string path,
        string method,
        string clientIp)
    {
        var problems = ValidateLogRequestStart(logger, requestId, path, method, clientIp);
        if (problems.Count > 0)
        {
            throw new ArgumentException("Invalid parameters for LogRequestStart: " + string.Join("; ", problems));
        }
    }

    /// <summary>
    /// Ensures all parameters for LogRequestComplete are valid, throwing ArgumentException if not.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="requestId">The request identifier.</param>
    /// <param name="path">The request path.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="durationMs">The request duration in milliseconds.</param>
    /// <exception cref="ArgumentException">Thrown if any parameter is invalid.</exception>
    public static void EnsureValidLogRequestComplete(
        ILogger logger,
        string requestId,
        string path,
        int statusCode,
        long durationMs)
    {
        var problems = ValidateLogRequestComplete(logger, requestId, path, statusCode, durationMs);
        if (problems.Count > 0)
        {
            throw new ArgumentException("Invalid parameters for LogRequestComplete: " + string.Join("; ", problems));
        }
    }

    /// <summary>
    /// Ensures all parameters for LogServiceDiscovery are valid, throwing ArgumentException if not.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="serviceId">The service identifier.</param>
    /// <param name="serviceName">The service name.</param>
    /// <param name="healthy">Whether the service is healthy.</param>
    /// <exception cref="ArgumentException">Thrown if any parameter is invalid.</exception>
    public static void EnsureValidLogServiceDiscovery(
        ILogger logger,
        int serviceId,
        string serviceName,
        bool healthy)
    {
        var problems = ValidateLogServiceDiscovery(logger, serviceId, serviceName, healthy);
        if (problems.Count > 0)
        {
            throw new ArgumentException("Invalid parameters for LogServiceDiscovery: " + string.Join("; ", problems));
        }
    }

    /// <summary>
    /// Ensures all parameters for LogCacheOperation are valid, throwing ArgumentException if not.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="operation">The cache operation type.</param>
    /// <param name="key">The cache key.</param>
    /// <param name="hit">Whether the operation was a cache hit.</param>
    /// <exception cref="ArgumentException">Thrown if any parameter is invalid.</exception>
    public static void EnsureValidLogCacheOperation(
        ILogger logger,
        string operation,
        string key,
        bool hit)
    {
        var problems = ValidateLogCacheOperation(logger, operation, key, hit);
        if (problems.Count > 0)
        {
            throw new ArgumentException("Invalid parameters for LogCacheOperation: " + string.Join("; ", problems));
        }
    }

    /// <summary>
    /// Ensures all parameters for LogRouteResolution are valid, throwing ArgumentException if not.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="path">The request path.</param>
    /// <param name="routePattern">The route pattern.</param>
    /// <param name="targetServiceId">The target service identifier.</param>
    /// <exception cref="ArgumentException">Thrown if any parameter is invalid.</exception>
    public static void EnsureValidLogRouteResolution(
        ILogger logger,
        string path,
        string routePattern,
        int targetServiceId)
    {
        var problems = ValidateLogRouteResolution(logger, path, routePattern, targetServiceId);
        if (problems.Count > 0)
        {
            throw new ArgumentException("Invalid parameters for LogRouteResolution: " + string.Join("; ", problems));
        }
    }

    /// <summary>
    /// Ensures all parameters for LogRateLimit are valid, throwing ArgumentException if not.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="clientIp">The client IP address.</param>
    /// <param name="path">The request path.</param>
    /// <param name="limit">The rate limit value.</param>
    /// <exception cref="ArgumentException">Thrown if any parameter is invalid.</exception>
    public static void EnsureValidLogRateLimit(
        ILogger logger,
        string clientIp,
        string path,
        int limit)
    {
        var problems = ValidateLogRateLimit(logger, clientIp, path, limit);
        if (problems.Count > 0)
        {
            throw new ArgumentException("Invalid parameters for LogRateLimit: " + string.Join("; ", problems));
        }
    }

    /// <summary>
    /// Ensures all parameters for LogAuthentication are valid, throwing ArgumentException if not.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="success">Whether authentication succeeded.</param>
    /// <param name="reason">Optional failure reason.</param>
    /// <exception cref="ArgumentException">Thrown if any parameter is invalid.</exception>
    public static void EnsureValidLogAuthentication(
        ILogger logger,
        string? userId,
        bool success,
        string? reason = null)
    {
        var problems = ValidateLogAuthentication(logger, userId, success, reason);
        if (problems.Count > 0)
        {
            throw new ArgumentException("Invalid parameters for LogAuthentication: " + string.Join("; ", problems));
        }
    }

    /// <summary>
    /// Ensures all parameters for LogCriticalError are valid, throwing ArgumentException if not.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception to log.</param>
    /// <param name="context">The error context.</param>
    /// <param name="additionalData">Optional additional data dictionary.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="logger"/> is null.
    /// Thrown if <paramref name="ex"/> is null.
    /// Thrown if <paramref name="context"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">Thrown if any parameter is invalid.</exception>
    public static void EnsureValidLogCriticalError(
        ILogger logger,
        Exception ex,
        string context,
        Dictionary<string, object>? additionalData = null)
    {
        var problems = ValidateLogCriticalError(logger, ex, context, additionalData);
        if (problems.Count > 0)
        {
            throw new ArgumentException("Invalid parameters for LogCriticalError: " + string.Join("; ", problems));
        }
    }

    /// <summary>
    /// Ensures all parameters for LogPerformanceMetrics are valid, throwing ArgumentException if not.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="operation">The operation name.</param>
    /// <param name="durationMs">The operation duration in milliseconds.</param>
    /// <param name="itemCount">Optional item count.</param>
    /// <exception cref="ArgumentException">Thrown if any parameter is invalid.</exception>
    public static void EnsureValidLogPerformanceMetrics(
        ILogger logger,
        string operation,
        long durationMs,
        int? itemCount = null)
    {
        var problems = ValidateLogPerformanceMetrics(logger, operation, durationMs, itemCount);
        if (problems.Count > 0)
        {
            throw new ArgumentException("Invalid parameters for LogPerformanceMetrics: " + string.Join("; ", problems));
        }
    }
}