#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Domain;

/// <summary>
/// Provides validation helpers for <see cref="AuthenticationToken"/> instances
/// </summary>
public static class AuthenticationTokenValidation
{
    /// <summary>
    /// Validates an authentication token and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="token">The token to validate</param>
    /// <returns>An empty list if valid, otherwise a list of validation error messages</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="token"/> is null</exception>
    public static IReadOnlyList<string> GetValidationErrors(this AuthenticationToken? token)
    {
        ArgumentNullException.ThrowIfNull(token);

        var errors = new List<string>();

        // Validate required string properties
        if (string.IsNullOrWhiteSpace(token.TokenHash))
            errors.Add("TokenHash is required and cannot be null or whitespace");

        if (string.IsNullOrWhiteSpace(token.ClientName))
            errors.Add("ClientName is required and cannot be null or whitespace");

        if (string.IsNullOrWhiteSpace(token.ClientId))
            errors.Add("ClientId is required and cannot be null or whitespace");

        if (string.IsNullOrWhiteSpace(token.TokenType))
            errors.Add("TokenType is required and cannot be null or whitespace");

        // Validate Id (must be positive)
        if (token.Id <= 0)
            errors.Add("Id must be a positive integer");

        // Validate ClientSecret if present
        if (token.ClientSecret is not null && string.IsNullOrWhiteSpace(token.ClientSecret))
            errors.Add("ClientSecret cannot be an empty string");

        // Validate scopes collection
        if (token.Scopes is null)
            errors.Add("Scopes collection cannot be null");
        else if (token.Scopes.Count == 0)
        {
            // Empty scopes is allowed, but check for whitespace values
            if (token.Scopes.Any(string.IsNullOrWhiteSpace))
                errors.Add("Scopes collection cannot contain null or whitespace values");
        }
        else if (token.Scopes.Any(string.IsNullOrWhiteSpace))
        {
            errors.Add("Scopes collection cannot contain null or whitespace values");
        }

        // Validate allowed service IDs collection
        if (token.AllowedServiceIds is null)
            errors.Add("AllowedServiceIds collection cannot be null");
        else if (token.AllowedServiceIds.Count == 0)
        {
            if (!token.AllowAllServices)
                errors.Add("Token must allow all services or specify at least one allowed service ID");
        }
        else if (token.AllowedServiceIds.Any(id => id <= 0))
        {
            errors.Add("AllowedServiceIds collection cannot contain non-positive integers");
        }

        // Validate service access configuration
        if (!token.AllowAllServices && (token.AllowedServiceIds is null || token.AllowedServiceIds.Count == 0))
            errors.Add("Token must allow all services or specify at least one allowed service ID");

        // Validate dates
        if (token.CreatedAt == default)
            errors.Add("CreatedAt must be set to a valid date");
        else if (token.CreatedAt > DateTime.UtcNow.AddMinutes(5))
            errors.Add("CreatedAt cannot be in the future");

        if (token.ExpiresAt.HasValue)
        {
            if (token.ExpiresAt.Value < token.CreatedAt)
                errors.Add("ExpiresAt cannot be earlier than CreatedAt");

            if (token.ExpiresAt.Value <= DateTime.UtcNow)
                errors.Add("ExpiresAt cannot be in the past");
        }

        if (token.LastUsedAt.HasValue)
        {
            if (token.LastUsedAt.Value > DateTime.UtcNow.AddMinutes(5))
                errors.Add("LastUsedAt cannot be in the future");

            if (token.LastUsedAt.Value < token.CreatedAt)
                errors.Add("LastUsedAt cannot be earlier than CreatedAt");
        }

        if (token.RevokedAt.HasValue)
        {
            if (token.RevokedAt.Value > DateTime.UtcNow.AddMinutes(5))
                errors.Add("RevokedAt cannot be in the future");

            if (token.RevokedAt.Value < token.CreatedAt)
                errors.Add("RevokedAt cannot be earlier than CreatedAt");
        }

        // Validate usage count
        if (token.UsageCount < 0)
            errors.Add("UsageCount cannot be negative");

        // Validate IP whitelist format
        if (!string.IsNullOrWhiteSpace(token.IpWhitelistCsv))
        {
            var allowedIps = token.IpWhitelistCsv.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(ip => ip.Trim())
                .Where(ip => !string.IsNullOrWhiteSpace(ip))
                .ToList();

            if (allowedIps.Count == 0)
                errors.Add("IpWhitelistCsv must contain at least one valid IP address or CIDR range");
        }

        // Validate revocation state consistency
        if (token.IsRevoked)
        {
            if (!token.RevokedAt.HasValue)
                errors.Add("Revoked tokens must have RevokedAt set");

            if (string.IsNullOrWhiteSpace(token.RevokedReason))
                errors.Add("Revoked tokens must have a RevokedReason");
        }
        else
        {
            if (token.RevokedAt.HasValue)
                errors.Add("Only revoked tokens can have RevokedAt set");

            if (!string.IsNullOrWhiteSpace(token.RevokedReason))
                errors.Add("RevokedReason can only be set for revoked tokens");
        }

        // Validate active state consistency
        if (!token.IsActive && !token.IsRevoked)
            errors.Add("Inactive tokens should be marked as revoked");

        if (token.IsActive && token.RevokedAt.HasValue)
            errors.Add("Active tokens cannot have RevokedAt set");

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether an authentication token is valid.
    /// </summary>
    /// <param name="token">The token to check</param>
    /// <returns>True if the token is valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="token"/> is null</exception>
    public static bool IsValid(this AuthenticationToken? token)
    {
        ArgumentNullException.ThrowIfNull(token);

        var errors = token.GetValidationErrors();
        return errors.Count == 0;
    }

    /// <summary>
    /// Ensures that an authentication token is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="token">The token to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="token"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if the token is invalid, containing a list of validation errors</exception>
    public static void EnsureValid(this AuthenticationToken? token)
    {
        ArgumentNullException.ThrowIfNull(token);

        var errors = token.GetValidationErrors();

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Authentication token is invalid:{Environment.NewLine}- {
                string.Join(Environment.NewLine + "- ", errors)
            }");
        }
    }
}