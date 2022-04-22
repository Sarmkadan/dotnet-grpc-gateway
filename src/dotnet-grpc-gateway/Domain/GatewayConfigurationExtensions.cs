#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Net;

namespace DotNetGrpcGateway.Domain;

/// <summary>
/// Extension methods for GatewayConfiguration providing common operations and utilities
/// </summary>
public static class GatewayConfigurationExtensions
{
    /// <summary>
    /// Gets the full endpoint URL for this gateway configuration
    /// </summary>
    /// <param name="configuration">The gateway configuration</param>
    /// <returns>Formatted endpoint URL (http://{ListenAddress}:{Port} or https://{ListenAddress}:{Port})</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is null.</exception>
    public static string GetEndpointUrl(this GatewayConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return configuration.EnableSsl()
            ? $"https://{configuration.ListenAddress}:{configuration.Port}"
            : $"http://{configuration.ListenAddress}:{configuration.Port}";
    }

    /// <summary>
    /// Determines whether SSL/TLS should be enabled based on the configuration
    /// </summary>
    /// <param name="configuration">The gateway configuration</param>
    /// <returns>True if SSL should be enabled, false otherwise</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is null.</exception>
    public static bool EnableSsl(this GatewayConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return !configuration.ListenAddress.Equals("0.0.0.0", StringComparison.OrdinalIgnoreCase)
            && !configuration.ListenAddress.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            && configuration.Port != 443;
    }

    /// <summary>
    /// Gets the maximum allowed request size in bytes
    /// </summary>
    /// <param name="configuration">The gateway configuration</param>
    /// <returns>Maximum message size in bytes</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is null.</exception>
    public static long GetMaxRequestSizeBytes(this GatewayConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return configuration.MaxMessageSize;
    }

    /// <summary>
    /// Gets the request timeout as a TimeSpan
    /// </summary>
    /// <param name="configuration">The gateway configuration</param>
    /// <returns>Request timeout as TimeSpan</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is null.</exception>
    public static TimeSpan GetRequestTimeout(this GatewayConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return TimeSpan.FromMilliseconds(configuration.RequestTimeoutMs);
    }

    /// <summary>
    /// Creates a new GatewayConfiguration with default values
    /// </summary>
    /// <param name="name">The name for the new configuration</param>
    /// <returns>New GatewayConfiguration instance</returns>
    /// <exception cref="ArgumentException"><paramref name="name"/> cannot be null or whitespace.</exception>
    public static GatewayConfiguration CreateDefault(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        return new GatewayConfiguration
        {
            Id = 0,
            Name = name,
            Description = $"Default configuration for {name}",
            ListenAddress = "0.0.0.0",
            Port = 5000,
            EnableReflection = true,
            EnableMetrics = true,
            EnableWebSocketSupport = true,
            MaxConcurrentConnections = 1000,
            RequestTimeoutMs = 30000,
            MaxMessageSize = 10 * 1024 * 1024,
            EnableCorsPolicy = true,
            CorsOrigins = "*",
            EnableCompressionByDefault = true,
            CompressionAlgorithm = "gzip",
            ValidateSslCertificates = true,
            LogLevel = "Information",
            IsActive = true
        };
    }

    /// <summary>
    /// Creates a clone of the current GatewayConfiguration
    /// </summary>
    /// <param name="configuration">The gateway configuration to clone</param>
    /// <returns>New GatewayConfiguration instance with same values</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is null.</exception>
    public static GatewayConfiguration Clone(this GatewayConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return new GatewayConfiguration
        {
            Id = configuration.Id,
            Name = configuration.Name,
            Description = configuration.Description,
            ListenAddress = configuration.ListenAddress,
            Port = configuration.Port,
            EnableReflection = configuration.EnableReflection,
            EnableMetrics = configuration.EnableMetrics,
            EnableWebSocketSupport = configuration.EnableWebSocketSupport,
            MaxConcurrentConnections = configuration.MaxConcurrentConnections,
            RequestTimeoutMs = configuration.RequestTimeoutMs,
            MaxMessageSize = configuration.MaxMessageSize,
            EnableCorsPolicy = configuration.EnableCorsPolicy,
            CorsOrigins = configuration.CorsOrigins,
            EnableCompressionByDefault = configuration.EnableCompressionByDefault,
            CompressionAlgorithm = configuration.CompressionAlgorithm,
            ValidateSslCertificates = configuration.ValidateSslCertificates,
            LogLevel = configuration.LogLevel,
            IsActive = configuration.IsActive,
            CreatedAt = configuration.CreatedAt,
            ModifiedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Determines if the gateway is configured to listen on a specific IP address
    /// </summary>
    /// <param name="configuration">The gateway configuration</param>
    /// <param name="ipAddress">The IP address to check</param>
    /// <returns>True if the gateway listens on the specified IP</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="ipAddress"/> cannot be null or empty.</exception>
    public static bool ListensOnIp(this GatewayConfiguration configuration, string ipAddress)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrEmpty(ipAddress);

        return configuration.ListenAddress.Equals(ipAddress, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the formatted connection string for monitoring and metrics
    /// </summary>
    /// <param name="configuration">The gateway configuration</param>
    /// <returns>Connection string suitable for monitoring tools</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is null.</exception>
    public static string GetMonitoringConnectionString(this GatewayConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return $"grpc://{configuration.ListenAddress}:{configuration.Port}";
    }
}