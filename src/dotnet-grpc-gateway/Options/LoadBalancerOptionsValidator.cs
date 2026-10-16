#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Services;
using Microsoft.Extensions.Options;

namespace DotNetGrpcGateway.Options;

/// <summary>
/// Validates <see cref="LoadBalancerOptions"/> at startup. Every failure message names
/// the configuration key it refers to, so a misconfigured host fails fast with an actionable error.
/// Weights are checked against <see cref="ServiceDiscoveryOptions.Endpoints"/>, since weights are positional.
/// </summary>
public sealed class LoadBalancerOptionsValidator : IValidateOptions<LoadBalancerOptions>
{
    private static readonly TimeSpan MaxHealthCheckInterval = TimeSpan.FromMinutes(5);
    private const int MaxRetriesLimit = 10;

    private readonly IOptions<ServiceDiscoveryOptions> _serviceDiscovery;

    public LoadBalancerOptionsValidator(IOptions<ServiceDiscoveryOptions> serviceDiscovery)
    {
        _serviceDiscovery = serviceDiscovery ?? throw new ArgumentNullException(nameof(serviceDiscovery));
    }

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, LoadBalancerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        var validPolicies = Enum.GetNames<LoadBalancingStrategy>();
        if (string.IsNullOrWhiteSpace(options.Policy) ||
            !validPolicies.Contains(options.Policy, StringComparer.OrdinalIgnoreCase))
        {
            failures.Add(
                $"{LoadBalancerOptions.SectionName}:Policy ('{options.Policy}') is not a valid policy. " +
                $"Valid values: {string.Join(", ", validPolicies)}.");
        }

        if (options.HealthCheckInterval <= TimeSpan.Zero || options.HealthCheckInterval > MaxHealthCheckInterval)
        {
            failures.Add(
                $"{LoadBalancerOptions.SectionName}:HealthCheckInterval ({options.HealthCheckInterval}) " +
                $"must be greater than 0 and at most {MaxHealthCheckInterval}.");
        }

        if (options.MaxRetries is < 0 or > MaxRetriesLimit)
        {
            failures.Add(
                $"{LoadBalancerOptions.SectionName}:MaxRetries ({options.MaxRetries}) " +
                $"must be between 0 and {MaxRetriesLimit} inclusive.");
        }

        if (options.Weights.Count > 0)
        {
            var endpointCount = _serviceDiscovery.Value.Endpoints.Count;
            if (options.Weights.Count != endpointCount)
            {
                failures.Add(
                    $"{LoadBalancerOptions.SectionName}:Weights must contain one entry per endpoint in " +
                    $"{ServiceDiscoveryOptions.SectionName}:Endpoints (expected {endpointCount}, got {options.Weights.Count}).");
            }

            for (var i = 0; i < options.Weights.Count; i++)
            {
                if (options.Weights[i] <= 0)
                {
                    failures.Add(
                        $"{LoadBalancerOptions.SectionName}:Weights[{i}] ({options.Weights[i]}) must be greater than 0.");
                }
            }
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
