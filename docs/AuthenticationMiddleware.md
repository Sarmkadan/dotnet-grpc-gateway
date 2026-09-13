# Authentication middleware

`ApiKeyAuthenticationHandler` is a Bearer token authentication handler that validates API keys from the Authorization header and establishes a user principal for authenticated requests.

## Overview

The authentication middleware validates tokens presented in the `Authorization: Bearer <token>` header format. It skips authentication for health check endpoints and validates that tokens are properly formatted UUIDs. Upon successful validation, it creates a claims principal with the token as a name identifier and token type claim.

## Pipeline behavior

For each request, `HandleAuthenticateAsync()`:

1. Skips authentication if the request path starts with `/health` (health check endpoint)
2. Checks for the presence of the `Authorization` header
3. Validates that the header starts with the `Bearer` scheme (case-insensitive)
4. Extracts the token value after the "Bearer " prefix (7 characters)
5. Validates that the token is not empty
6. Validates token format as a UUID (extendable for actual token validation logic)
7. Creates claims principal with:
   - `ClaimTypes.NameIdentifier` set to the token value
   - Custom `token_type` claim set to `api_key`
8. Returns authentication success ticket

If any validation step fails, returns an appropriate authentication failure result.

## Configuration

The authentication scheme is registered via extension method:

```csharp
builder.Services.AddAuthentication()
    .AddApiKeyAuthentication();
```

This registers the "ApiKey" authentication scheme with the handler.

## Token validation

Currently validates tokens as UUID format using `Guid.TryParse()`. This can be extended to integrate with actual token validation logic (database lookup, signature verification, etc.) while maintaining the same interface.

## Claims identity

Upon successful authentication, establishes a `ClaimsIdentity` with:
- Name identifier claim containing the token value
- Token type claim indicating "api_key" authentication type

## Usage

Place authentication middleware in the ASP.NET Core pipeline after routing but before authorization:

```csharp
var app = builder.Build();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Endpoints follow
```

## Security considerations

- Tokens should be transmitted over HTTPS only
- Consider implementing token expiration and revocation mechanisms
- The current UUID validation is a placeholder - replace with production-grade token validation
- Tokens are stored as claims and accessible via `User.FindFirstValue(ClaimTypes.NameIdentifier)`