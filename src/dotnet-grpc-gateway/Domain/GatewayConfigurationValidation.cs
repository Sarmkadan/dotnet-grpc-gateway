using System.Globalization;

namespace DotNetGrpcGateway.Domain;

/// <summary>
/// Provides validation helpers for <see cref="GatewayConfiguration"/> instances
/// </summary>
public static class GatewayConfigurationValidation
{
    /// <summary>
    /// Validates the specified <see cref="GatewayConfiguration"/> instance and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The configuration to validate</param>
    /// <returns>An empty list if valid; otherwise, a list of human-readable error messages</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(GatewayConfiguration value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (value.Id < 0)
        {
            errors.Add($"Id must be a non-negative integer, but was {value.Id}");
        }

        // Validate Name
        if (string.IsNullOrWhiteSpace(value.Name))
        {
            errors.Add("Name is required and cannot be null or whitespace");
        }
        else if (value.Name.Length > 256)
        {
            errors.Add("Name cannot exceed 256 characters");
        }

        // Validate Description
        if (value.Description != null && value.Description.Length > 2048)
        {
            errors.Add("Description cannot exceed 2048 characters");
        }

        // Validate ListenAddress
        if (string.IsNullOrWhiteSpace(value.ListenAddress))
        {
            errors.Add("ListenAddress is required and cannot be null or whitespace");
        }
        else if (value.ListenAddress.Length > 256)
        {
            errors.Add("ListenAddress cannot exceed 256 characters");
        }

        // Validate Port
        if (value.Port < 1 || value.Port > 65535)
        {
            errors.Add("Port must be between 1 and 65535");
        }

        // Validate EnableReflection
        // No validation needed for boolean flags

        // Validate EnableMetrics
        // No validation needed for boolean flags

        // Validate EnableWebSocketSupport
        // No validation needed for boolean flags

        // Validate MaxConcurrentConnections
        if (value.MaxConcurrentConnections < 1)
        {
            errors.Add("MaxConcurrentConnections must be greater than 0");
        }

        // Validate RequestTimeoutMs
        if (value.RequestTimeoutMs < 100)
        {
            errors.Add("RequestTimeoutMs must be at least 100 milliseconds");
        }

        // Validate MaxMessageSize
        if (value.MaxMessageSize < 1024)
        {
            errors.Add("MaxMessageSize must be at least 1024 bytes (1KB)");
        }

        // Validate EnableCorsPolicy
        // No validation needed for boolean flags

        // Validate CorsOrigins
        if (value.EnableCorsPolicy && string.IsNullOrWhiteSpace(value.CorsOrigins))
        {
            errors.Add("CorsOrigins is required when EnableCorsPolicy is true");
        }
        else if (value.CorsOrigins != null && value.CorsOrigins.Length > 2048)
        {
            errors.Add("CorsOrigins cannot exceed 2048 characters");
        }

        // Validate EnableCompressionByDefault
        // No validation needed for boolean flags

        // Validate CompressionAlgorithm
        if (value.EnableCompressionByDefault && string.IsNullOrWhiteSpace(value.CompressionAlgorithm))
        {
            errors.Add("CompressionAlgorithm is required when EnableCompressionByDefault is true");
        }
        else if (value.CompressionAlgorithm != null && value.CompressionAlgorithm.Length > 64)
        {
            errors.Add("CompressionAlgorithm cannot exceed 64 characters");
        }

        // Validate ValidateSslCertificates
        // No validation needed for boolean flags

        // Validate LogLevel
        if (string.IsNullOrWhiteSpace(value.LogLevel))
        {
            errors.Add("LogLevel is required and cannot be null or whitespace");
        }
        else if (value.LogLevel.Length > 64)
        {
            errors.Add("LogLevel cannot exceed 64 characters");
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt must be set to a valid DateTime");
        }

        // Validate ModifiedAt
        if (value.ModifiedAt == default)
        {
            errors.Add("ModifiedAt must be set to a valid DateTime");
        }

        // Validate IsActive
        // No validation needed for boolean flags

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="GatewayConfiguration"/> instance is valid.
    /// </summary>
    /// <param name="value">The configuration to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    public static bool IsValid(GatewayConfiguration value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Validates the specified <see cref="GatewayConfiguration"/> instance and throws an <see cref="ArgumentException"/>
    /// if it contains validation errors.
    /// </summary>
    /// <param name="value">The configuration to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of error messages</exception>
    public static void EnsureValid(GatewayConfiguration value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"GatewayConfiguration validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }
}