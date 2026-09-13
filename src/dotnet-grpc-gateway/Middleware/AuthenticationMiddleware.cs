#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Security.Claims;
using System.Text.Encodings.Web;
using DotNetGrpcGateway.Constants;
using Microsoft.AspNetCore.Authentication;

namespace DotNetGrpcGateway.Middleware;

/// <summary>
/// Bearer token authentication handler for API authentication.
/// Validates tokens from Authorization header and establishes user principal.
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string HealthCheckPath = "/health";
    private const string MissingAuthorizationHeaderMessage = "Missing Authorization header";
    private const string InvalidAuthorizationHeaderFormatMessage = "Invalid Authorization header format";
    private const string MissingTokenMessage = "Missing token";
    private const string InvalidTokenFormatMessage = "Invalid token format";
    private const string TokenTypeClaimName = "token_type";
    private const string ApiKeyTokenType = "api_key";
    private const int BearerTokenPrefixLength = 7;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiKeyAuthenticationHandler"/> class.
    /// </summary>
    /// <param name="options">The monitor for the authentication scheme options.</param>
    /// <param name="logger">The logger factory used to create a logger for this handler.</param>
    /// <param name="encoder">The URL encoder used for encoding URLs.</param>
    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(encoder);
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Skip authentication for health check endpoint
        if (Request.Path.StartsWithSegments(HealthCheckPath))
            return AuthenticateResult.NoResult();

        if (!Request.Headers.TryGetValue(GatewayConstants.HeaderAuthorization, out var authHeader))
            return AuthenticateResult.Fail(MissingAuthorizationHeaderMessage);

        var headerValue = authHeader.ToString();

        if (!headerValue.StartsWith(GatewayConstants.AuthenticationScheme, StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.Fail(InvalidAuthorizationHeaderFormatMessage);

        var token = headerValue[BearerTokenPrefixLength..].Trim();

        if (string.IsNullOrEmpty(token))
            return AuthenticateResult.Fail(MissingTokenMessage);

        // Validate token format (simple UUID check; extend with actual token validation)
        if (!Guid.TryParse(token, out _))
            return AuthenticateResult.Fail(InvalidTokenFormatMessage);

        // Create principal with token as claim
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, token),
            new Claim(TokenTypeClaimName, ApiKeyTokenType)
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
    private const string ApiKeyAuthenticationScheme = "ApiKey";

    /// <summary>
    /// Adds API key authentication to the authentication builder.
    /// </summary>
    /// <param name="builder">The authentication builder to add the scheme to.</param>
    /// <returns>The authentication builder for chaining.</returns>
    public static AuthenticationBuilder AddApiKeyAuthentication(this AuthenticationBuilder builder)
    {
        return builder.AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
            ApiKeyAuthenticationScheme, options => { });
    }
}
