#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Options;

/// <summary>
/// Configuration options for service discovery, bound from the "ServiceDiscovery" section.
/// Validated at startup by <see cref="ServiceDiscoveryOptionsValidator"/>.
/// </summary>
public class ServiceDiscoveryOptions
{
    public const string SectionName = "ServiceDiscovery";

    /// <summary>
    /// Static backend endpoints (absolute http/https URIs). At least one endpoint
    /// or a <see cref="Provider"/> must be configured.
    /// </summary>
    public List<string> Endpoints { get; set; } = new();

    /// <summary>
    /// Name of an external discovery provider. Optional when <see cref="Endpoints"/> is set.
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// Interval between discovery refreshes. Must be greater than 0 and less than 3600 (one hour).
    /// </summary>
    public int RefreshIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Timeout for establishing a connection to a discovered endpoint. Must be greater than 0.
    /// </summary>
    public int ConnectTimeoutMs { get; set; } = 2000;

    /// <summary>
    /// Timeout for a single discovery request. Must be greater than 0.
    /// </summary>
    public int RequestTimeoutMs { get; set; } = 5000;
}
