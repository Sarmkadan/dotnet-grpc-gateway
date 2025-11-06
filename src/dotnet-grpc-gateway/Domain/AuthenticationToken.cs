// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Domain;

/// <summary>
/// Represents an API authentication token for accessing the gateway
/// </summary>
public class AuthenticationToken
{
    public int Id { get; set; }

    public string TokenHash { get; set; } = null!;

    public string ClientName { get; set; } = null!;

    public string ClientId { get; set; } = null!;

    public string? ClientSecret { get; set; }

    public string TokenType { get; set; } = "Bearer";

    public List<string> Scopes { get; set; } = new();

    public List<int> AllowedServiceIds { get; set; } = new();

    public bool AllowAllServices { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; }

    public DateTime? LastUsedAt { get; set; }

    public int UsageCount { get; set; } = 0;

    public bool IsRevoked { get; set; } = false;

    public string? RevokedReason { get; set; }

    public DateTime? RevokedAt { get; set; }

    public bool IsActive { get; set; } = true;

    public string? IpWhitelistCsv { get; set; }

    public string? UserAgent { get; set; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(TokenHash))
            throw new InvalidOperationException("Token hash is required");

        if (string.IsNullOrWhiteSpace(ClientName))
            throw new InvalidOperationException("Client name is required");

        if (string.IsNullOrWhiteSpace(ClientId))
            throw new InvalidOperationException("Client ID is required");

        if (!AllowAllServices && AllowedServiceIds.Count == 0)
            throw new InvalidOperationException("Token must allow all services or specify allowed services");

        if (ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow)
            throw new InvalidOperationException("Token expiration date cannot be in the past");
    }

    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow >= ExpiresAt.Value;

    public bool IsValid => IsActive && !IsRevoked && !IsExpired;

    public bool CanAccessService(int serviceId) => AllowAllServices || AllowedServiceIds.Contains(serviceId);

    public void RecordUsage(string? userAgent = null)
    {
        LastUsedAt = DateTime.UtcNow;
        UsageCount++;
        if (!string.IsNullOrWhiteSpace(userAgent))
            UserAgent = userAgent;
    }

    public void Revoke(string? reason = null)
    {
        IsRevoked = true;
        RevokedReason = reason;
        RevokedAt = DateTime.UtcNow;
    }

    public bool IsIpAllowed(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(IpWhitelistCsv))
            return true;

        var allowedIps = IpWhitelistCsv.Split(',')
            .Select(ip => ip.Trim())
            .ToList();

        return allowedIps.Contains(ipAddress);
    }
}
