// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;

namespace DotNetGrpcGateway.Middleware;

/// <summary>
/// Bearer token authentication handler for API authentication.
/// Validates tokens from Authorization header and establishes user principal.
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string AuthorizationHeaderName = "Authorization";
    private const string BearerScheme = "Bearer";

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Skip authentication for health check endpoint
        if (Request.Path.StartsWithSegments("/health"))
            return AuthenticateResult.NoResult();

        if (!Request.Headers.TryGetValue(AuthorizationHeaderName, out var authHeader))
            return AuthenticateResult.Fail("Missing Authorization header");

        var headerValue = authHeader.ToString();

        if (!headerValue.StartsWith(BearerScheme, StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.Fail("Invalid Authorization header format");

        var token = headerValue[($"{BearerScheme} ".Length)..].Trim();

        if (string.IsNullOrEmpty(token))
            return AuthenticateResult.Fail("Missing token");

        // Validate token format (simple UUID check; extend with actual token validation)
        if (!Guid.TryParse(token, out _))
            return AuthenticateResult.Fail("Invalid token format");

        // Create principal with token as claim
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, token),
            new Claim("token_type", "api_key")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return await Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

/// <summary>
/// Extension methods for registering API key authentication.
/// </summary>
public static class ApiKeyAuthenticationExtensions
{
    public static AuthenticationBuilder AddApiKeyAuthentication(this AuthenticationBuilder builder)
    {
        return builder.AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
            "ApiKey", options => { });
    }
}
