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
    /// <returns>Formatted endpoint URL (http://{ListenAddress}:{Port})</returns>
    public static string GetEndpointUrl(this GatewayConfiguration configuration)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        return configuration.EnableSsl()
            ? $"https://{configuration.ListenAddress}:{configuration.Port}"
            : $"http://{configuration.ListenAddress}:{configuration.Port}";
    }

    /// <summary>
    /// Determines whether SSL/TLS should be enabled based on the configuration
    /// </summary>
    /// <param name="configuration">The gateway configuration</param>
    /// <returns>True if SSL should be enabled, false otherwise</returns>
    public static bool EnableSsl(this GatewayConfiguration configuration)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        return configuration.ListenAddress.Equals("0.0.0.0", StringComparison.OrdinalIgnoreCase) == false
            && configuration.ListenAddress.Equals("localhost", StringComparison.OrdinalIgnoreCase) == false
            && configuration.Port != 443;
    }

    /// <summary>
    /// Gets the maximum allowed request size in bytes
    /// </summary>
    /// <param name="configuration">The gateway configuration</param>
    /// <returns>Maximum message size in bytes</returns>
    public static long GetMaxRequestSizeBytes(this GatewayConfiguration configuration)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        return configuration.MaxMessageSize;
    }

    /// <summary>
    /// Gets the request timeout as a TimeSpan
    /// </summary>
    /// <param name="configuration">The gateway configuration</param>
    /// <returns>Request timeout as TimeSpan</returns>
    public static TimeSpan GetRequestTimeout(this GatewayConfiguration configuration)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        return TimeSpan.FromMilliseconds(configuration.RequestTimeoutMs);
    }

    /// <summary>
    /// Creates a new GatewayConfiguration with default values
    /// </summary>
    /// <param name="name">The name for the new configuration</param>
    /// <returns>New GatewayConfiguration instance</returns>
    public static GatewayConfiguration CreateDefault(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or whitespace", nameof(name));

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
    public static GatewayConfiguration Clone(this GatewayConfiguration configuration)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

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
    public static bool ListensOnIp(this GatewayConfiguration configuration, string ipAddress)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        if (string.IsNullOrWhiteSpace(ipAddress))
            throw new ArgumentException("IP address cannot be null or empty", nameof(ipAddress));

        return configuration.ListenAddress.Equals(ipAddress, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the formatted connection string for monitoring and metrics
    /// </summary>
    /// <param name="configuration">The gateway configuration</param>
    /// <returns>Connection string suitable for monitoring tools</returns>
    public static string GetMonitoringConnectionString(this GatewayConfiguration configuration)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        return $"grpc://{configuration.ListenAddress}:{configuration.Port}";
    }
}