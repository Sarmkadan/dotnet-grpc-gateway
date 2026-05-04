// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Exceptions;

/// <summary>
/// Base exception for all gateway-related errors
/// </summary>
public class GatewayException : Exception
{
    public string ErrorCode { get; set; }

    public int? HttpStatusCode { get; set; }

    public Dictionary<string, object>? Details { get; set; }

    public GatewayException(string message, string errorCode = "GATEWAY_ERROR", int? httpStatusCode = null)
        : base(message)
    {
        ErrorCode = errorCode;
        HttpStatusCode = httpStatusCode ?? 500;
    }

    public GatewayException(string message, Exception innerException, string errorCode = "GATEWAY_ERROR", int? httpStatusCode = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        HttpStatusCode = httpStatusCode ?? 500;
    }

    public void AddDetail(string key, object value)
    {
        Details ??= new Dictionary<string, object>();
        Details[key] = value;
    }
}

/// <summary>
/// Thrown when a requested service is not found
/// </summary>
public class ServiceNotFoundException : GatewayException
{
    public ServiceNotFoundException(string serviceName)
        : base($"Service '{serviceName}' not found", "SERVICE_NOT_FOUND", 404)
    {
        AddDetail("service_name", serviceName);
    }
}

/// <summary>
/// Thrown when service is unavailable or unhealthy
/// </summary>
public class ServiceUnavailableException : GatewayException
{
    public ServiceUnavailableException(string serviceName, string reason)
        : base($"Service '{serviceName}' is unavailable: {reason}", "SERVICE_UNAVAILABLE", 503)
    {
        AddDetail("service_name", serviceName);
        AddDetail("reason", reason);
    }
}

/// <summary>
/// Thrown when route matching fails
/// </summary>
public class RouteResolutionException : GatewayException
{
    public RouteResolutionException(string pattern, string reason)
        : base($"Failed to resolve route '{pattern}': {reason}", "ROUTE_RESOLUTION_FAILED", 404)
    {
        AddDetail("pattern", pattern);
        AddDetail("reason", reason);
    }
}

/// <summary>
/// Thrown when authentication fails
/// </summary>
public class AuthenticationException : GatewayException
{
    public AuthenticationException(string reason)
        : base($"Authentication failed: {reason}", "AUTHENTICATION_FAILED", 401)
    {
        AddDetail("reason", reason);
    }
}

/// <summary>
/// Thrown when authorization fails
/// </summary>
public class AuthorizationException : GatewayException
{
    public AuthorizationException(string reason, string? clientId = null)
        : base($"Authorization denied: {reason}", "AUTHORIZATION_DENIED", 403)
    {
        AddDetail("reason", reason);
        if (clientId != null)
            AddDetail("client_id", clientId);
    }
}

/// <summary>
/// Thrown when configuration is invalid
/// </summary>
public class ConfigurationException : GatewayException
{
    public ConfigurationException(string configKey, string reason)
        : base($"Configuration error for '{configKey}': {reason}", "CONFIGURATION_ERROR", 500)
    {
        AddDetail("config_key", configKey);
        AddDetail("reason", reason);
    }
}

/// <summary>
/// Thrown when database operation fails
/// </summary>
public class DataAccessException : GatewayException
{
    public DataAccessException(string operation, Exception innerException)
        : base($"Data access error during {operation}", innerException, "DATA_ACCESS_ERROR", 500)
    {
        AddDetail("operation", operation);
    }
}

/// <summary>
/// Thrown when rate limiting is exceeded
/// </summary>
public class RateLimitException : GatewayException
{
    public RateLimitException(string clientId, int limit, int timeWindowSeconds)
        : base($"Rate limit exceeded: {limit} requests per {timeWindowSeconds} seconds", "RATE_LIMIT_EXCEEDED", 429)
    {
        AddDetail("client_id", clientId);
        AddDetail("limit", limit);
        AddDetail("time_window_seconds", timeWindowSeconds);
    }
}

/// <summary>
/// Thrown when timeout occurs
/// </summary>
public class TimeoutException : GatewayException
{
    public TimeoutException(string operation, int timeoutMs)
        : base($"Operation '{operation}' timed out after {timeoutMs}ms", "TIMEOUT", 504)
    {
        AddDetail("operation", operation);
        AddDetail("timeout_ms", timeoutMs);
    }
}
