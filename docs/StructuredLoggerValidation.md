# StructuredLoggerValidation

A utility class providing validation methods and result checks for structured logging in gRPC gateway scenarios. It centralizes common validation patterns for log entries related to request lifecycle, service discovery, caching, routing, and other critical operations, ensuring consistent logging quality and error detection.

## API

### Validation Methods

#### `public static IReadOnlyList<string> ValidateLogRequestStart`
Returns a list of validation errors for a log entry representing the start of a gRPC request. Each string in the returned list describes a specific validation failure. An empty list indicates the log entry is valid.

- **Returns**: `IReadOnlyList<string>` – A read-only list of error messages; empty if valid.
- **Throws**: Does not throw exceptions; returns error messages instead.

#### `public static IReadOnlyList<string> ValidateLogRequestComplete`
Returns a list of validation errors for a log entry representing the completion of a gRPC request.

- **Returns**: `IReadOnlyList<string>` – A read-only list of error messages; empty if valid.
- **Throws**: Does not throw exceptions; returns error messages instead.

#### `public static IReadOnlyList<string> ValidateLogServiceDiscovery`
Returns a list of validation errors for a log entry related to service discovery operations.

- **Returns**: `IReadOnlyList<string>` – A read-only list of error messages; empty if valid.
- **Throws**: Does not throw exceptions; returns error messages instead.

#### `public static IReadOnlyList<string> ValidateLogCacheOperation`
Returns a list of validation errors for a log entry related to cache operations.

- **Returns**: `IReadOnlyList<string>` – A read-only list of error messages; empty if valid.
- **Throws**: Does not throw exceptions; returns error messages instead.

#### `public static IReadOnlyList<string> ValidateLogRouteResolution`
Returns a list of validation errors for a log entry related to route resolution in the gateway.

- **Returns**: `IReadOnlyList<string>` – A read-only list of error messages; empty if valid.
- **Throws**: Does not throw exceptions; returns error messages instead.

#### `public static IReadOnlyList<string> ValidateLogRateLimit`
Returns a list of validation errors for a log entry related to rate limiting.

- **Returns**: `IReadOnlyList<string>` – A read-only list of error messages; empty if valid.
- **Throws**: Does not throw exceptions; returns error messages instead.

#### `public static IReadOnlyList<string> ValidateLogAuthentication`
Returns a list of validation errors for a log entry related to authentication or authorization.

- **Returns**: `IReadOnlyList<string>` – A read-only list of error messages; empty if valid.
- **Throws**: Does not throw exceptions; returns error messages instead.

#### `public static IReadOnlyList<string> ValidateLogCriticalError`
Returns a list of validation errors for a log entry representing a critical error.

- **Returns**: `IReadOnlyList<string>` – A read-only list of error messages; empty if valid.
- **Throws**: Does not throw exceptions; returns error messages instead.

#### `public static IReadOnlyList<string> ValidateLogPerformanceMetrics`
Returns a list of validation errors for a log entry containing performance metrics.

- **Returns**: `IReadOnlyList<string>` – A read-only list of error messages; empty if valid.
- **Throws**: Does not throw exceptions; returns error messages instead.

### Validation Checks

#### `public static bool IsValidLogRequestStart`
Determines whether a log entry representing the start of a gRPC request is valid. Returns `true` if no validation errors exist.

- **Returns**: `bool` – `true` if the log entry is valid; otherwise, `false`.
- **Throws**: Does not throw exceptions.

#### `public static bool IsValidLogRequestComplete`
Determines whether a log entry representing the completion of a gRPC request is valid. Returns `true` if no validation errors exist.

- **Returns**: `bool` – `true` if the log entry is valid; otherwise, `false`.
- **Throws**: Does not throw exceptions.

#### `public static bool IsValidLogServiceDiscovery`
Determines whether a log entry related to service discovery is valid. Returns `true` if no validation errors exist.

- **Returns**: `bool` – `true` if the log entry is valid; otherwise, `false`.
- **Throws**: Does not throw exceptions.

#### `public static bool IsValidLogCacheOperation`
Determines whether a log entry related to cache operations is valid. Returns `true` if no validation errors exist.

- **Returns**: `bool` – `true` if the log entry is valid; otherwise, `false`.
- **Throws**: Does not throw exceptions.

#### `public static bool IsValidLogRouteResolution`
Determines whether a log entry related to route resolution is valid. Returns `true` if no validation errors exist.

- **Returns**: `bool` – `true` if the log entry is valid; otherwise, `false`.
- **Throws**: Does not throw exceptions.

#### `public static bool IsValidLogRateLimit`
Determines whether a log entry related to rate limiting is valid. Returns `true` if no validation errors exist.

- **Returns**: `bool` – `true` if the log entry is valid; otherwise, `false`.
- **Throws**: Does not throw exceptions.

#### `public static bool IsValidLogAuthentication`
Determines whether a log entry related to authentication or authorization is valid. Returns `true` if no validation errors exist.

- **Returns**: `bool` – `true` if the log entry is valid; otherwise, `false`.
- **Throws**: Does not throw exceptions.

#### `public static bool IsValidLogCriticalError`
Determines whether a log entry representing a critical error is valid. Returns `true` if no validation errors exist.

- **Returns**: `bool` – `true` if the log entry is valid; otherwise, `false`.
- **Throws**: Does not throw exceptions.

#### `public static bool IsValidLogPerformanceMetrics`
Determines whether a log entry containing performance metrics is valid. Returns `true` if no validation errors exist.

- **Returns**: `bool` – `true` if the log entry is valid; otherwise, `false`.
- **Throws**: Does not throw exceptions.

### Validation Enforcement

#### `public static void EnsureValidLogRequestStart`
Validates a log entry representing the start of a gRPC request and throws an `InvalidOperationException` if validation fails.

- **Throws**: `InvalidOperationException` – If the log entry is invalid.
- **Note**: Does not return a value.

#### `public static void EnsureValidLogRequestComplete`
Validates a log entry representing the completion of a gRPC request and throws an `InvalidOperationException` if validation fails.

- **Throws**: `InvalidOperationException` – If the log entry is invalid.
- **Note**: Does not return a value.

## Usage
