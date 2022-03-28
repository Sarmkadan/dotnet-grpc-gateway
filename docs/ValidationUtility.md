# ValidationUtility

`ValidationUtility` is a static utility class that provides a collection of validation methods for common input types in distributed systems and API gateways. It centralises checks for URIs, network addresses, identifiers, ranges, and protocol-specific constraints, enabling consistent pre-condition enforcement across the `dotnet-grpc-gateway` project.

## API

### `IsValidUri`

```csharp
public static bool IsValidUri(string uri)
```

Validates whether the supplied string represents a well-formed absolute URI.

**Parameters:**
- `uri` — The string to evaluate.

**Return value:**
`true` if `uri` is non-null, non-empty, and can be parsed as an absolute URI with a recognised scheme; otherwise `false`.

**Exceptions:**
None. All parsing failures are caught internally and result in `false`.

---

### `IsValidIpAddress`

```csharp
public static bool IsValidIpAddress(string ip)
```

Determines whether the string is a valid IPv4 or IPv6 address.

**Parameters:**
- `ip` — The candidate IP address string.

**Return value:**
`true` if `ip` can be parsed by `IPAddress.TryParse`; otherwise `false`.

**Exceptions:**
None.

---

### `IsValidPort`

```csharp
public static bool IsValidPort(int port)
```

Checks that the integer falls within the valid TCP/UDP port range (1–65535).

**Parameters:**
- `port` — The port number to validate.

**Return value:**
`true` if `port` is between 1 and 65535 inclusive; otherwise `false`.

**Exceptions:**
None.

---

### `IsValidHostname`

```csharp
public static bool IsValidHostname(string hostname)
```

Validates a hostname according to RFC 1123 conventions (labels of up to 63 alphanumeric/hyphen characters, total length ≤ 255, no leading/trailing hyphens per label).

**Parameters:**
- `hostname` — The hostname string to check.

**Return value:**
`true` if the hostname conforms to the expected format; otherwise `false`.

**Exceptions:**
None.

---

### `IsValidGuid`

```csharp
public static bool IsValidGuid(string guid)
```

Checks whether the string can be parsed as a GUID in any of the standard formats (D, N, B, P, X).

**Parameters:**
- `guid` — The string to test.

**Return value:**
`true` if `Guid.TryParse` succeeds; otherwise `false`.

**Exceptions:**
None.

---

### `IsNullOrEmpty`

```csharp
public static bool IsNullOrEmpty(string value)
```

Returns `true` when the string is `null`, empty, or consists solely of white-space characters.

**Parameters:**
- `value` — The string to examine.

**Return value:**
`true` if `string.IsNullOrWhiteSpace(value)`; otherwise `false`.

**Exceptions:**
None.

---

### `IsInRange` (integer)

```csharp
public static bool IsInRange(int value, int min, int max)
```

Tests whether an integer lies within a closed interval.

**Parameters:**
- `value` — The integer to test.
- `min` — Inclusive lower bound.
- `max` — Inclusive upper bound.

**Return value:**
`true` if `min ≤ value ≤ max`; otherwise `false`.

**Exceptions:**
None.

---

### `IsInRange` (double)

```csharp
public static bool IsInRange(double value, double min, double max)
```

Tests whether a double-precision floating-point number lies within a closed interval.

**Parameters:**
- `value` — The double to test.
- `min` — Inclusive lower bound.
- `max` — Inclusive upper bound.

**Return value:**
`true` if `min ≤ value ≤ max`; otherwise `false`.

**Exceptions:**
None.

---

### `IsValidPercentage`

```csharp
public static bool IsValidPercentage(double value)
```

Checks that the value is a valid percentage (0.0 through 100.0 inclusive).

**Parameters:**
- `value` — The double to evaluate.

**Return value:**
`true` if `0.0 ≤ value ≤ 100.0`; otherwise `false`.

**Exceptions:**
None.

---

### `MeetsMinimumLength`

```csharp
public static bool MeetsMinimumLength(string value, int minLength)
```

Verifies that a string’s length is at least the specified minimum, after trimming leading and trailing white-space.

**Parameters:**
- `value` — The string to measure.
- `minLength` — The minimum required length.

**Return value:**
`true` if `value` is non-null and its trimmed length ≥ `minLength`; otherwise `false`.

**Exceptions:**
None.

---

### `IsValidServiceName`

```csharp
public static bool IsValidServiceName(string name)
```

Validates a gRPC service name against the canonical pattern (typically `package.Service` or a DNS-like name with allowed characters).

**Parameters:**
- `name` — The service name string.

**Return value:**
`true` if the name matches the expected service naming rules; otherwise `false`.

**Exceptions:**
None.

---

### `IsValidProtocol`

```csharp
public static bool IsValidProtocol(string protocol)
```

Checks whether the protocol string is one of the recognised schemes (e.g., `http`, `https`, `grpc`, `grpcs`).

**Parameters:**
- `protocol` — The protocol identifier.

**Return value:**
`true` if the protocol is in the allowed set; otherwise `false`.

**Exceptions:**
None.

---

### `IsValidPath`

```csharp
public static bool IsValidPath(string path)
```

Validates that the string represents a well-formed relative URI path (no scheme, no authority, starts with `/`, no illegal characters).

**Parameters:**
- `path` — The path string to check.

**Return value:**
`true` if the path is syntactically valid; otherwise `false`.

**Exceptions:**
None.

---

### `IsValidTimeout`

```csharp
public static bool IsValidTimeout(string timeout)
```

Parses a duration string (e.g., `30s`, `5m`, `1h`) and confirms it represents a positive, finite time span.

**Parameters:**
- `timeout` — The duration string.

**Return value:**
`true` if the string can be parsed into a positive `TimeSpan`; otherwise `false`.

**Exceptions:**
None.

---

### `Ensure`

```csharp
public static void Ensure(bool condition, string message)
```

Throws an `ArgumentException` (or a project-specific validation exception) when the condition is `false`. Used to enforce pre-conditions in a compact form.

**Parameters:**
- `condition` — The boolean expression that must be `true`.
- `message` — The error message included in the exception.

**Return value:**
None (void).

**Exceptions:**
Throws `ArgumentException` (or a derived type) when `condition` is `false`.

---

## Usage

### Example 1: Validating gRPC gateway configuration inputs

```csharp
public void ConfigureGateway(string endpoint, string timeout)
{
    if (!ValidationUtility.IsValidUri(endpoint))
        throw new ArgumentException("Endpoint must be a valid absolute URI.", nameof(endpoint));

    if (!ValidationUtility.IsValidTimeout(timeout))
        throw new ArgumentException("Timeout must be a positive duration string (e.g., '30s').", nameof(timeout));

    // Proceed with configuration
}
```

### Example 2: Enforcing pre-conditions with Ensure

```csharp
public void RegisterService(string serviceName, int port)
{
    ValidationUtility.Ensure(
        ValidationUtility.IsValidServiceName(serviceName),
        $"Service name '{serviceName}' does not match the required pattern.");

    ValidationUtility.Ensure(
        ValidationUtility.IsValidPort(port),
        $"Port {port} is outside the valid range 1–65535.");

    // Registration logic
}
```

## Notes

- All predicate methods (`IsValid*`, `IsNullOrEmpty`, `IsInRange`, `MeetsMinimumLength`) are pure and thread-safe; they hold no state and perform no I/O.
- `IsValidUri` requires an *absolute* URI. Relative URIs, including those starting with `/`, will return `false`. Use `IsValidPath` for relative path validation.
- `IsValidHostname` permits both fully qualified names and single-label hostnames as long as they conform to RFC 1123. Internationalised domain names must be supplied in ASCII-compatible encoding (punycode) to pass validation.
- `IsValidGuid` accepts input in any of the standard GUID formats but rejects strings with surrounding whitespace or extra characters.
- `IsNullOrEmpty` treats white-space-only strings as empty, which is stricter than `string.IsNullOrEmpty`.
- `IsInRange` overloads use inclusive bounds. When `min > max`, the result is always `false`.
- `IsValidPercentage` treats boundary values 0.0 and 100.0 as valid. Floating-point values slightly outside the range due to precision (e.g., `100.0000001`) will return `false`.
- `Ensure` is the only member that throws. The exact exception type may be `ArgumentException` or a custom subclass defined elsewhere in the project; consult the implementation for the precise type.
- None of the methods modify their arguments or cause side effects. They are safe to call concurrently from multiple threads without synchronisation.
