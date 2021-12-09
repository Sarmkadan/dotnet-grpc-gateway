// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Domain;

/// <summary>
/// Represents the configuration for the gRPC gateway instance
/// </summary>
public class GatewayConfiguration
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = string.Empty;

    public string ListenAddress { get; set; } = "0.0.0.0";

    public int Port { get; set; } = 5000;

    public bool EnableReflection { get; set; } = true;

    public bool EnableMetrics { get; set; } = true;

    public bool EnableWebSocketSupport { get; set; } = true;

    public int MaxConcurrentConnections { get; set; } = 1000;

    public int RequestTimeoutMs { get; set; } = 30000;

    public int MaxMessageSize { get; set; } = 10 * 1024 * 1024; // 10MB

    public bool EnableCorsPolicy { get; set; } = true;

    public string CorsOrigins { get; set; } = "*";

    public bool EnableCompressionByDefault { get; set; } = true;

    public string CompressionAlgorithm { get; set; } = "gzip";

    public bool ValidateSslCertificates { get; set; } = true;

    public string LogLevel { get; set; } = "Information";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new InvalidOperationException("Gateway name is required");

        if (Port < 1 || Port > 65535)
            throw new InvalidOperationException("Port must be between 1 and 65535");

        if (MaxConcurrentConnections < 1)
            throw new InvalidOperationException("Max concurrent connections must be greater than 0");

        if (RequestTimeoutMs < 100)
            throw new InvalidOperationException("Request timeout must be at least 100ms");

        if (MaxMessageSize < 1024)
            throw new InvalidOperationException("Max message size must be at least 1KB");
    }

    public void UpdateModifiedDate() => ModifiedAt = DateTime.UtcNow;
}
