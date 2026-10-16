#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Options;

/// <summary>
/// Configuration options for load balancing, bound from the "LoadBalancer" section.
/// Validated at startup by <see cref="LoadBalancerOptionsValidator"/>.
/// </summary>
public class LoadBalancerOptions
{
    public const string SectionName = "LoadBalancer";

    /// <summary>
    /// Name of the load balancing policy. Must be one of the <see cref="Services.LoadBalancingStrategy"/> names
    /// (<c>RoundRobin</c>, <c>Random</c>, <c>LeastConnections</c>). Kept as a string so that an unknown value
    /// is reported by the validator with the list of valid values, rather than failing during binding.
    /// </summary>
    public string Policy { get; set; } = nameof(Services.LoadBalancingStrategy.RoundRobin);

    /// <summary>
    /// Interval between endpoint health checks. Must be greater than zero and at most five minutes.
    /// </summary>
    public TimeSpan HealthCheckInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Maximum number of retries for a failed call. Must be between 0 and 10 inclusive.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Optional per-endpoint weights. When set, the count must match the number of
    /// <c>ServiceDiscovery:Endpoints</c> and every weight must be greater than 0.
    /// Empty means no weighting is configured.
    /// </summary>
    public List<int> Weights { get; set; } = new();
}
