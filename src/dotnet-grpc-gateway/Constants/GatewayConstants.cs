// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Constants;

/// <summary>
/// Gateway-wide constants and configuration defaults
/// </summary>
public static class GatewayConstants
{
    // Default configuration values
    public const string DefaultListenAddress = "0.0.0.0";
    public const int DefaultPort = 5000;
    public const int DefaultMaxConcurrentConnections = 1000;
    public const int DefaultRequestTimeoutMs = 30000;
    public const int DefaultMaxMessageSizeBytes = 10 * 1024 * 1024; // 10MB

    // Health check settings
    public const int DefaultHealthCheckIntervalSeconds = 30;
    public const int HealthCheckTimeoutMs = 5000;
    public const int HealthCheckFailureThreshold = 3;

    // Caching defaults
    public const int DefaultCacheDurationSeconds = 60;
    public const int MaxCacheDurationSeconds = 3600; // 1 hour

    // Rate limiting defaults
    public const int DefaultRateLimitPerMinute = 1000;
    public const int MinRateLimitPerMinute = 1;
    public const int MaxRateLimitPerMinute = 1000000;

    // Request/Response headers
    public const string HeaderXRequestId = "X-Request-ID";
    public const string HeaderXForwardedFor = "X-Forwarded-For";
    public const string HeaderXForwardedProto = "X-Forwarded-Proto";
    public const string HeaderXForwardedHost = "X-Forwarded-Host";
    public const string HeaderAuthorization = "Authorization";
    public const string HeaderContentType = "Content-Type";
    public const string HeaderGrpcStatus = "grpc-status";
    public const string HeaderGrpcMessage = "grpc-message";

    // Authentication
    public const string AuthenticationScheme = "Bearer";
    public const int TokenHashLength = 64;

    // Logging
    public const string LogContextKeyRequestId = "RequestId";
    public const string LogContextKeyServiceName = "ServiceName";
    public const string LogContextKeyMethodName = "MethodName";
    public const string LogContextKeyClientIp = "ClientIp";

    // Database
    public const string DefaultConnectionStringName = "DefaultConnection";

    // Error codes
    public const string ErrorCodeUnknown = "UNKNOWN_ERROR";
    public const string ErrorCodeValidation = "VALIDATION_ERROR";
    public const string ErrorCodeNotFound = "NOT_FOUND";
    public const string ErrorCodeUnauthorized = "UNAUTHORIZED";
    public const string ErrorCodeForbidden = "FORBIDDEN";
    public const string ErrorCodeConflict = "CONFLICT";
    public const string ErrorCodeInternalError = "INTERNAL_ERROR";
}

/// <summary>
/// gRPC status codes mapping
/// </summary>
public enum GrpcStatusCode
{
    Ok = 0,
    Cancelled = 1,
    Unknown = 2,
    InvalidArgument = 3,
    DeadlineExceeded = 4,
    NotFound = 5,
    AlreadyExists = 6,
    PermissionDenied = 7,
    ResourceExhausted = 8,
    FailedPrecondition = 9,
    Aborted = 10,
    OutOfRange = 11,
    Unimplemented = 12,
    Internal = 13,
    Unavailable = 14,
    DataLoss = 15,
    Unauthenticated = 16
}

/// <summary>
/// Request processing status
/// </summary>
public enum RequestStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Cancelled,
    Retrying,
    CacheHit
}

/// <summary>
/// Service health status
/// </summary>
public enum ServiceHealthStatus
{
    Unknown,
    Healthy,
    Degraded,
    Unhealthy,
    Maintenance
}

/// <summary>
/// Token authentication types
/// </summary>
public enum TokenType
{
    Bearer,
    ApiKey,
    OAuth2,
    Custom
}

/// <summary>
/// Cache strategies
/// </summary>
public enum CacheStrategy
{
    None,
    InMemory,
    Distributed,
    Hybrid
}

/// <summary>
/// Compression algorithms
/// </summary>
public enum CompressionAlgorithm
{
    None,
    Gzip,
    Deflate,
    Brotli
}
