// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Domain;

/// <summary>
/// Represents a routing rule that maps requests to gRPC services
/// </summary>
public class GatewayRoute
{
    public int Id { get; set; }

    public string Pattern { get; set; } = null!;

    public int TargetServiceId { get; set; }

    public int Priority { get; set; } = 100;

    public RouteMatchType MatchType { get; set; } = RouteMatchType.ExactMatch;

    public string? Description { get; set; }

    public Dictionary<string, string> Headers { get; set; } = new();

    public Dictionary<string, string> Metadata { get; set; } = new();

    public bool RequiresAuthentication { get; set; } = false;

    public string? AuthorizationPolicy { get; set; }

    public int RateLimitPerMinute { get; set; } = 1000;

    public bool EnableCaching { get; set; } = false;

    public int CacheDurationSeconds { get; set; } = 60;

    public string? RequestTransformationScript { get; set; }

    public string? ResponseTransformationScript { get; set; }

    public bool EnableCompression { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Pattern))
            throw new InvalidOperationException("Route pattern is required");

        if (TargetServiceId <= 0)
            throw new InvalidOperationException("Target service ID must be valid");

        if (Priority < 0 || Priority > 1000)
            throw new InvalidOperationException("Priority must be between 0 and 1000");

        if (RateLimitPerMinute <= 0)
            throw new InvalidOperationException("Rate limit must be greater than 0");

        if (CacheDurationSeconds < 0)
            throw new InvalidOperationException("Cache duration cannot be negative");
    }

    public bool MatchesRequest(string serviceName, string methodName)
    {
        return MatchType switch
        {
            RouteMatchType.ExactMatch => Pattern == $"{serviceName}.{methodName}",
            RouteMatchType.Prefix => $"{serviceName}.{methodName}".StartsWith(Pattern),
            RouteMatchType.Regex => System.Text.RegularExpressions.Regex.IsMatch($"{serviceName}.{methodName}", Pattern),
            _ => false
        };
    }

    public void UpdateModifiedDate() => ModifiedAt = DateTime.UtcNow;
}

public enum RouteMatchType
{
    ExactMatch = 0,
    Prefix = 1,
    Regex = 2
}
