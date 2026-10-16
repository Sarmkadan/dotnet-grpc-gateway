#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Options;

/// <summary>
/// Validates <see cref="ServiceDiscoveryOptions"/> at startup. Every failure message names
/// the configuration key it refers to, so a misconfigured host fails fast with an actionable error.
/// </summary>
public sealed class ServiceDiscoveryOptionsValidator : IValidateOptions<ServiceDiscoveryOptions>
{
    private const int MaxRefreshIntervalSeconds = 3600;

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, ServiceDiscoveryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (options.Endpoints.Count == 0 && string.IsNullOrWhiteSpace(options.Provider))
        {
            failures.Add(
                $"{ServiceDiscoveryOptions.SectionName}:Endpoints must contain at least one endpoint, " +
                $"or {ServiceDiscoveryOptions.SectionName}:Provider must be set.");
        }

        for (var i = 0; i < options.Endpoints.Count; i++)
        {
            var endpoint = options.Endpoints[i];
            if (!IsAbsoluteHttpUri(endpoint))
            {
                failures.Add(
                    $"{ServiceDiscoveryOptions.SectionName}:Endpoints[{i}] ('{endpoint}') must be an absolute URI with an http or https scheme.");
            }
        }

        if (options.RefreshIntervalSeconds <= 0 || options.RefreshIntervalSeconds >= MaxRefreshIntervalSeconds)
        {
            failures.Add(
                $"{ServiceDiscoveryOptions.SectionName}:RefreshIntervalSeconds ({options.RefreshIntervalSeconds}) " +
                $"must be greater than 0 and less than {MaxRefreshIntervalSeconds}.");
        }

        if (options.ConnectTimeoutMs <= 0)
        {
            failures.Add(
                $"{ServiceDiscoveryOptions.SectionName}:ConnectTimeoutMs ({options.ConnectTimeoutMs}) must be greater than 0.");
        }

        if (options.RequestTimeoutMs <= 0)
        {
            failures.Add(
                $"{ServiceDiscoveryOptions.SectionName}:RequestTimeoutMs ({options.RequestTimeoutMs}) must be greater than 0.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static bool IsAbsoluteHttpUri(string? endpoint) =>
        !string.IsNullOrWhiteSpace(endpoint) &&
        Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) &&
        uri.Scheme is "http" or "https";
}
