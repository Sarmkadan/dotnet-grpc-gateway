# AuthenticationToken

Represents an authentication token used to authorize requests to the gRPC gateway. Each token is associated with a client, defines allowed scopes and services, tracks usage and revocation status, and provides a `Validate` method to check whether the token is currently valid for use.

## API

### `public int Id`
Unique identifier for the token record.

### `public string TokenHash`
Hash of the token value. Used for lookup without storing the plaintext token.

### `public string ClientName`
Human-readable name of the client that owns this token.

### `public string ClientId`
Unique identifier of the client application.

### `public string? ClientSecret`
Optional secret associated with the client. May be `null` if not applicable.

### `public string TokenType`
Type of the token (e.g., `"Bearer"`, `"ApiKey"`).

### `public List<string> Scopes`
List of OAuth2 scopes granted to this token. Can be empty.

### `public List<int> AllowedServiceIds`
List of service IDs that this token is permitted to access. Ignored if `AllowAllServices` is `true`.

### `public bool AllowAllServices`
If `true`, the token is allowed to access all services regardless of `AllowedServiceIds`.

### `public DateTime CreatedAt`
UTC timestamp when the token was created.

### `public DateTime? ExpiresAt`
UTC timestamp after which the token is considered expired. `null` means the token never expires.

### `public DateTime? LastUsedAt`
UTC timestamp of the last time the token was used. `null` if never used.

### `public int UsageCount`
Number of times the token has been used.

### `public bool IsRevoked`
Indicates whether the token has been explicitly revoked.

### `public string? RevokedReason`
Optional reason provided when the token was revoked. `null` if not revoked or no reason given.

### `public DateTime? RevokedAt`
UTC timestamp when the token was revoked. `null` if not revoked.

### `public bool IsActive`
Computed property that returns `true` if the token is not revoked and has not expired (i.e., `!IsRevoked` and (`ExpiresAt` is `null` or `ExpiresAt > DateTime.UtcNow`)). Read-only.

### `public string? IpWhitelistCsv`
Optional comma-separated list of IP addresses or CIDR ranges allowed to use this token. `null` means no IP restriction.

### `public string? UserAgent`
Optional user-agent string that must match for the token to be valid. `null` means no restriction.

### `public void Validate()`
Validates the token's current state. Throws an exception if the token is invalid.

**Throws:**
- `InvalidOperationException` if `IsActive` is `false` (token is revoked or expired).
- `UnauthorizedAccessException` if the token is not yet active (e.g., `CreatedAt` is in the future, though not enforced by `IsActive` – implementation may check additional conditions).
- Any other exception derived from `System.Exception` as defined by the implementation.

**Parameters:** None.  
**Returns:** `void`.

## Usage

### Example 1: Creating and validating a token

```csharp
var token = new AuthenticationToken
{
    Id = 1,
    TokenHash = "abc123hash",
    ClientName = "MyApp",
    ClientId = "client-xyz",
    TokenType = "Bearer",
    Scopes = new List<string> { "read", "write" },
    AllowedServiceIds = new List<int> { 101, 102 },
    AllowAllServices = false,
    CreatedAt = DateTime.UtcNow,
    ExpiresAt = DateTime.UtcNow.AddHours(1),
    IsRevoked = false,
    UsageCount = 0
};

try
{
    token.Validate();
    Console.WriteLine("Token is valid.");
}
catch (Exception ex)
{
    Console.WriteLine($"Token invalid: {ex.Message}");
}
```

### Example 2: Checking token state before authorization

```csharp
public bool TryAuthorize(AuthenticationToken token, int serviceId, string ipAddress)
{
    // Validate basic token state
    try
    {
        token.Validate();
    }
    catch
    {
        return false;
    }

    // Check IP whitelist
    if (!string.IsNullOrEmpty(token.IpWhitelistCsv))
    {
        var allowedIps = token.IpWhitelistCsv.Split(',');
        if (!allowedIps.Contains(ipAddress))
            return false;
    }

    // Check service access
    if (!token.AllowAllServices && !token.AllowedServiceIds.Contains(serviceId))
        return false;

    // Update usage tracking
    token.LastUsedAt = DateTime.UtcNow;
    token.UsageCount++;

    return true;
}
```

## Notes

- **Thread safety:** `AuthenticationToken` is a mutable data model and is not designed for concurrent access. If instances are shared across threads, external synchronization (e.g., locking) must be used.
- **Nullable fields:** `ClientSecret`, `ExpiresAt`, `LastUsedAt`, `RevokedReason`, `RevokedAt`, `IpWhitelistCsv`, and `UserAgent` may be `null`. Code consuming these properties should handle `null` appropriately.
- **`IsActive` computation:** This property is typically computed from `IsRevoked` and `ExpiresAt`. It does not consider `CreatedAt` or other constraints; the `Validate` method may enforce additional rules.
- **`Validate` behavior:** The exact exceptions thrown depend on the implementation. Always catch the most specific exception type available, or `Exception` as a fallback.
- **Empty lists:** `Scopes` and `AllowedServiceIds` can be empty. An empty `AllowedServiceIds` list combined with `AllowAllServices = false` effectively denies access to all services.
- **IP whitelist format:** The `IpWhitelistCsv` property is a plain string; parsing and matching logic must be implemented externally. No validation is performed by the type itself.
