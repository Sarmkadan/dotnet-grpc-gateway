# AuthenticationAndSecurityExample

Demonstrates authentication and security patterns in gRPC gateway services, including token creation, protected route access, and tiered authorization.

## API

### `AuthenticationAndSecurityExample`
Public entry point for the example. Initializes client connections and configuration for authentication demonstrations.

### `CreateAuthenticationTokenAsync`
Creates a JWT authentication token with configurable claims and expiration.

- **Parameters**:
  - `issuer` (string): Token issuer identifier.
  - `audience` (string): Token audience identifier.
  - `expiresInMinutes` (int): Token validity duration in minutes.
  - `claims` (Dictionary<string, object>): Additional claims to include in the token.
- **Return value**: `Task<string>` – The generated JWT token as a base64-encoded string.
- **Exceptions**: Throws `ArgumentException` if `issuer` or `audience` is null or empty.

### `CreateProtectedRouteAsync`
Configures a gRPC gateway route that requires authentication. Simulates server-side route protection setup.

- **Parameters**:
  - `routeName` (string): Name of the protected route to create.
  - `requiredScope` (string): Scope required to access the route.
- **Return value**: `Task` – Completes when the route is registered.
- **Exceptions**: Throws `ArgumentNullException` if `routeName` or `requiredScope` is null.

### `MakeAuthenticatedRequestAsync`
Performs an authenticated gRPC request to a protected service endpoint.

- **Parameters**:
  - `token` (string): JWT token to attach to the request.
  - `serviceMethod` (string): Name of the gRPC method to invoke.
  - `requestData` (Dictionary<string, string>): Key-value pairs representing request payload.
- **Return value**: `Task<string>` – Response payload from the service.
- **Exceptions**:
  - Throws `InvalidOperationException` if the token is malformed or expired.
  - Throws `RpcException` if the service method call fails due to authorization.

### `TryUnauthorizedAccessAsync`
Attempts to access a protected route without authentication. Demonstrates unauthorized access behavior.

- **Parameters**:
  - `routeName` (string): Name of the protected route to attempt access to.
- **Return value**: `Task<bool>` – `true` if unauthorized access was prevented; `false` if bypass occurred (unexpected).
- **Exceptions**: Throws `InvalidOperationException` if route configuration is invalid.

### `ConfigureTieredAccessAsync`
Sets up multiple access tiers with different permission scopes on protected routes.

- **Parameters**:
  - `routeName` (string): Name of the route to tier.
  - `scopes` (IEnumerable<string>): Ordered list of scopes, from least to most privileged.
- **Return value**: `Task` – Completes when tiered scopes are applied.
- **Exceptions**: Throws `ArgumentException` if `scopes` is empty or contains duplicates.

### `DisplayTokensAsync`
Prints token metadata (issuer, audience, expiration) to the console for inspection.

- **Parameters**:
  - `token` (string): JWT token to decode and display.
- **Return value**: `Task` – Completes after printing token details.
- **Exceptions**: Throws `FormatException` if the token is not a valid JWT.

### `Main`
Entry point for the console application. Orchestrates the authentication and security demonstration flow.

- **Parameters**:
  - `args` (string[]): Command-line arguments (unused).
- **Return value**: `Task<int>` – Application exit code (0 on success).
- **Exceptions**: Propagates exceptions from called methods; returns non-zero on failure.

## Usage

### Example 1: Basic Token Creation and Authenticated Request
