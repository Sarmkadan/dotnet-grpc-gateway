#nullable enable
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
    /// <summary>
    /// Gets or sets the unique identifier for the token.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the hashed token value.
    /// </summary>
    public string TokenHash { get; set; } = null!;

    /// <summary>
    /// Gets or sets the name of the client associated with the token.
    /// </summary>
    public string ClientName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    public string ClientId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the client secret (if applicable).
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Gets or sets the token type (default is "Bearer").
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Gets or sets the list of scopes associated with the token.
    /// </summary>
    public List<string> Scopes { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of allowed service IDs.
    /// </summary>
    public List<int> AllowedServiceIds { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the token allows access to all services.
    /// </summary>
    public bool AllowAllServices { get; set; } = false;

    /// <summary>
    /// Gets or sets the date and time when the token was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the token expires.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the token was last used.
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Gets or sets the number of times the token has been used.
    /// </summary>
    public int UsageCount { get; set; } = 0;

    /// <summary>
    /// Gets or sets a value indicating whether the token is revoked.
    /// </summary>
    public bool IsRevoked { get; set; } = false;

    /// <summary>
    /// Gets or sets the reason for revocation (if applicable).
    /// </summary>
    public string? RevokedReason { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the token was revoked.
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the token is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the comma-separated list of IP addresses allowed to use the token.
    /// </summary>
    public string? IpWhitelistCsv { get; set; }

    /// <summary>
    /// Gets or sets the user agent string associated with the token.
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Validates the token properties to ensure they meet requirements.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
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

    /// <summary>
    /// Gets a value indicating whether the token has expired.
    /// </summary>
    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow >= ExpiresAt.Value;

    /// <summary>
    /// Gets a value indicating whether the token is valid (active, not revoked, and not expired).
    /// </summary>
    public bool IsValid => IsActive && !IsRevoked && !IsExpired;

    /// <summary>
    /// Determines whether the token can access the specified service.
    /// </summary>
    /// <param name="serviceId">The identifier of the service to check access for.</param>
    /// <returns>true if the token can access the service; otherwise, false.</returns>
    public bool CanAccessService(int serviceId) => AllowAllServices || AllowedServiceIds.Contains(serviceId);

    /// <summary>
    /// Records usage of the token.
    /// </summary>
    /// <param name="userAgent">Optional user agent string associated with the usage.</param>
    public void RecordUsage(string? userAgent = null)
    {
        LastUsedAt = DateTime.UtcNow;
        UsageCount++;
        if (!string.IsNullOrWhiteSpace(userAgent))
            UserAgent = userAgent;
    }

    /// <summary>
    /// Revokes the token.
    /// </summary>
    /// <param name="reason">Optional reason for revocation.</param>
    public void Revoke(string? reason = null)
    {
        IsRevoked = true;
        RevokedReason = reason;
        RevokedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Determines whether the specified IP address is allowed to use the token.
    /// </summary>
    /// <param name="ipAddress">The IP address to check.</param>
    /// <returns>true if the IP address is allowed; otherwise, false.</returns>
    public bool IsIpAllowed(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(IpWhitelistCsv))
            return true;

        var allowedIps = IpWhitelistCsv.Split(',')
            .Select(ip => ip.Trim())
            .ToList();

        return allowedIps.Contains(ipAddress);
    }

    /// <summary>
    /// Returns a string representation of the token.
    /// </summary>
    /// <returns>A string representation of the token.</returns>
    public override string ToString()
    {
        var expiry = ExpiresAt.HasValue
            ? ExpiresAt.Value.ToString("yyyy-MM-dd HH:mm:ss 'UTC'")
            : "never";

        var state = IsRevoked ? "revoked" : IsExpired ? "expired" : IsActive ? "active" : "inactive";

        return $"AuthenticationToken {{ Id = {Id}, ClientName = {ClientName}, ClientId = {ClientId}, " +
               $"TokenType = {TokenType}, Scopes = [{string.Join(", ", Scopes)}], " +
               $"ExpiresAt = {expiry}, State = {state}, UsageCount = {UsageCount}, " +
               $"AllowAllServices = {AllowAllServices} }}";
    }
}