# HttpUtilityValidation

Provides static validation utilities for common HTTP protocol elements including headers, status codes, and content types. Each validation method returns a list of error messages for detailed diagnostics, while corresponding `IsValid*` properties offer a simple boolean check for conditional logic.

## API

### `public static IReadOnlyList<string> Validate(string value)`

Validates a general HTTP header value for protocol compliance.

**Parameters**  
- `value`: The header value to validate.

**Returns**  
An empty list if the value is valid; otherwise, a list of human-readable error messages describing each violation.

**Throws**  
- `ArgumentNullException` if `value` is null.

---

### `public static IReadOnlyList<string> ValidateAuthorizationHeader(string value)`

Validates an `Authorization` header value against RFC 7235 syntax requirements.

**Parameters**  
- `value`: The `Authorization` header value to validate.

**Returns**  
An empty list if the value conforms to the `scheme token68` format; otherwise, a list of error messages.

**Throws**  
- `ArgumentNullException` if `value` is null.

---

### `public static IReadOnlyList<string> ValidateAcceptHeader(string value)`

Validates an `Accept` header value for media type syntax and quality factor formatting.

**Parameters**  
- `value`: The `Accept` header value to validate.

**Returns**  
An empty list if the value is a valid comma-separated list of media ranges; otherwise, a list of error messages.

**Throws**  
- `ArgumentNullException` if `value` is null.

---

### `public static IReadOnlyList<string> ValidateStatusCode(int statusCode)`

Validates an HTTP status code against the IANA registry ranges (100–599).

**Parameters**  
- `statusCode`: The integer status code to validate.

**Returns**  
An empty list if the code falls within a defined standard range; otherwise, a list containing a single error message.

**Throws**  
This method does not throw.

---

### `public static IReadOnlyList<string> ValidateContentType(string value)`

Validates a `Content-Type` header value for media type syntax, parameters, and charset formatting.

**Parameters**  
- `value`: The `Content-Type` header value to validate.

**Returns**  
An empty list if the value is a valid media type with optional parameters; otherwise, a list of error messages.

**Throws**  
- `ArgumentNullException` if `value` is null.

---

### `public static bool IsValid(string value)`

Returns `true` if `Validate(value)` produces no errors; otherwise `false`.

**Parameters**  
- `value`: The header value to check.

**Returns**  
`true` for valid values; `false` for invalid or null values.

**Throws**  
This method does not throw.

---

### `public static bool IsValidAuthorizationHeader(string value)`

Returns `true` if `ValidateAuthorizationHeader(value)` produces no errors; otherwise `false`.

**Parameters**  
- `value`: The `Authorization` header value to check.

**Returns**  
`true` for valid values; `false` for invalid or null values.

**Throws**  
This method does not throw.

---

### `public static bool IsValidAcceptHeader(string value)`

Returns `true` if `ValidateAcceptHeader(value)` produces no errors; otherwise `false`.

**Parameters**  
- `value`: The `Accept` header value to check.

**Returns**  
`true` for valid values; `false` for invalid or null values.

**Throws**  
This method does not throw.

---

### `public static bool IsValidStatusCode(int statusCode)`

Returns `true` if `ValidateStatusCode(statusCode)` produces no errors; otherwise `false`.

**Parameters**  
- `statusCode`: The status code to check.

**Returns**  
`true` if the code is within 100–599; otherwise `false`.

**Throws**  
This method does not throw.

---

### `public static bool IsValidContentType(string value)`

Returns `true` if `ValidateContentType(value)` produces no errors; otherwise `false`.

**Parameters**  
- `value`: The `Content-Type` header value to check.

**Returns**  
`true` for valid values; `false` for invalid or null values.

**Throws**  
This method does not throw.

## Usage

### Example 1: Middleware validation with detailed error reporting

```csharp
app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
    var errors = HttpUtilityValidation.ValidateAuthorizationHeader(authHeader);
    
    if (errors.Count > 0)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new { errors });
        return;
    }
    
    await next();
});
```

### Example 2: Conditional content negotiation

```csharp
public IActionResult GetResource(string acceptHeader)
{
    if (!HttpUtilityValidation.IsValidAcceptHeader(acceptHeader))
    {
        return BadRequest("Invalid Accept header format.");
    }

    var mediaTypes = MediaTypeHeaderValue.ParseList(acceptHeader);
    var negotiated = _negotiator.Negotiate(mediaTypes);
    
    return negotiated.Match(
        ok => Ok(ok.Content),
        notAcceptable => StatusCode(StatusCodes.Status406NotAcceptable)
    );
}
```

## Notes

- All `Validate*` methods treat `null` input as invalid and throw `ArgumentNullException` (except `ValidateStatusCode`, which accepts any integer). The `IsValid*` counterparts safely return `false` for `null` without throwing.
- Validation follows RFC 7230, RFC 7231, and RFC 7235 syntax rules. It does not enforce semantic correctness (e.g., unknown authentication schemes in `Authorization` are accepted as syntactically valid).
- `ValidateStatusCode` accepts any integer in the range 100–599 inclusive, including unassigned codes. It does not validate against the current IANA registry snapshot.
- The type is stateless and thread-safe. All members are pure functions with no shared mutable state, making them safe for concurrent use in high-throughput middleware pipelines.
- Error messages are culture-invariant and intended for logging or developer-facing diagnostics, not for direct end-user display.
