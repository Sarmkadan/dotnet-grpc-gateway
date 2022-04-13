# RouteChannelOptionsExtensions

The `RouteChannelOptionsExtensions` class provides a set of static extension methods for configuring `RouteChannelOptions` instances in a fluent, chainable style. Each method returns the same `RouteChannelOptions` object, allowing multiple settings to be applied in a single expression. These methods are designed to simplify the construction of channel options for gRPC gateway routes by reducing repetitive property assignments and improving code readability.

## API

### `WithCallTimeoutMs`

```csharp
public static RouteChannelOptions WithCallTimeoutMs(this RouteChannelOptions options, int milliseconds)
```

Sets the call timeout in milliseconds.  
**Parameters:**  
- `options` – The `RouteChannelOptions` instance to modify. Must not be `null`.  
- `milliseconds` – The timeout value in milliseconds. Must be greater than zero.  

**Returns:** The same `RouteChannelOptions` instance for chaining.  

**Throws:**  
- `ArgumentNullException` if `options` is `null`.  
- `ArgumentOutOfRangeException` if `milliseconds` is less than or equal to zero.

---

### `WithMaxReceiveMessageSize`

```csharp
public static RouteChannelOptions WithMaxReceiveMessageSize(this RouteChannelOptions options, int bytes)
```

Sets the maximum size of a received message in bytes.  
**Parameters:**  
- `options` – The `RouteChannelOptions` instance to modify. Must not be `null`.  
- `bytes` – The maximum message size in bytes. Must be greater than zero.  

**Returns:** The same `RouteChannelOptions` instance for chaining.  

**Throws:**  
- `ArgumentNullException` if `options` is `null`.  
- `ArgumentOutOfRangeException` if `bytes` is less than or equal to zero.

---

### `WithMaxSendMessageSize`

```csharp
public static RouteChannelOptions WithMaxSendMessageSize(this RouteChannelOptions options, int bytes)
```

Sets the maximum size of a sent message in bytes.  
**Parameters:**  
- `options` – The `RouteChannelOptions` instance to modify. Must not be `null`.  
- `bytes` – The maximum message size in bytes. Must be greater than zero.  

**Returns:** The same `RouteChannelOptions` instance for chaining.  

**Throws:**  
- `ArgumentNullException` if `options` is `null`.  
- `ArgumentOutOfRangeException` if `bytes` is less than or equal to zero.

---

### `WithHeader`

```csharp
public static RouteChannelOptions WithHeader(this RouteChannelOptions options, string key, string value)
```

Adds a custom header to the channel options.  
**Parameters:**  
- `options` – The `RouteChannelOptions` instance to modify. Must not be `null`.  
- `key` – The header name. Must not be `null` or empty.  
- `value` – The header value. Must not be `null`.  

**Returns:** The same `RouteChannelOptions` instance for chaining.  

**Throws:**  
- `ArgumentNullException` if `options`, `key`, or `value` is `null`.  
- `ArgumentException` if `key` is empty or consists only of white space.

---

### `WithTlsTargetName`

```csharp
public static RouteChannelOptions WithTlsTargetName(this RouteChannelOptions options, string targetName)
```

Sets the TLS target name override for server certificate validation.  
**Parameters:**  
- `options` – The `RouteChannelOptions` instance to modify. Must not be `null`.  
- `targetName` – The expected TLS server name. Must not be `null` or empty.  

**Returns:** The same `RouteChannelOptions` instance for chaining.  

**Throws:**  
- `ArgumentNullException` if `options` or `targetName` is `null`.  
- `ArgumentException` if `targetName` is empty or consists only of white space.

---

### `WithSkipTlsVerification`

```csharp
public static RouteChannelOptions WithSkipTlsVerification(this RouteChannelOptions options)
```

Enables skipping of TLS certificate verification.  
**Parameters:**  
- `options` – The `RouteChannelOptions` instance to modify. Must not be `null`.  

**Returns:** The same `RouteChannelOptions` instance for chaining.  

**Throws:**  
- `ArgumentNullException` if `options` is `null`.

---

### `UpdateFrom`

```csharp
public static RouteChannelOptions UpdateFrom(this RouteChannelOptions options, RouteChannelOptions source)
```

Copies all configurable properties from the `source` `RouteChannelOptions` into the current instance.  
**Parameters:**  
- `options` – The target `RouteChannelOptions` instance to update. Must not be `null`.  
- `source` – The source `RouteChannelOptions` instance whose values are copied. Must not be `null`.  

**Returns:** The same `RouteChannelOptions` instance (`options`) for chaining.  

**Throws:**  
- `ArgumentNullException` if `options` or `source` is `null`.

## Usage

### Example 1: Fluent configuration of a new channel options instance

```csharp
var options = new RouteChannelOptions()
    .WithCallTimeoutMs(5000)
    .WithMaxReceiveMessageSize(64 * 1024)
    .WithMaxSendMessageSize(128 * 1024)
    .WithHeader("x-custom-trace-id", "abc-123")
    .WithTlsTargetName("api.example.com")
    .WithSkipTlsVerification(); // for development only
```

### Example 2: Applying a base configuration from a template

```csharp
var baseOptions = new RouteChannelOptions()
    .WithCallTimeoutMs(10000)
    .WithMaxReceiveMessageSize(256 * 1024);

var routeOptions = new RouteChannelOptions()
    .WithHeader("x-route-id", "route-42")
    .UpdateFrom(baseOptions); // inherits timeout and receive size from base
```

## Notes

- All methods modify the `RouteChannelOptions` instance passed as the first argument and return the same instance. This design is not thread-safe; concurrent modification of the same `RouteChannelOptions` object from multiple threads can lead to inconsistent state.  
- `WithSkipTlsVerification` should be used only in non-production environments, as it disables certificate validation and exposes the connection to man-in-the-middle attacks.  
- The `UpdateFrom` method performs a shallow copy of property values. If the source object contains reference-type properties (e.g., a collection of headers), the target will share the same references unless the implementation performs deep cloning.  
- Negative or zero values for timeout and message size parameters cause an `ArgumentOutOfRangeException`.  
- Header keys are case-insensitive per HTTP/2 conventions, but the extension method does not perform any normalization. Duplicate header keys may be added; behavior depends on how the underlying channel processes headers.  
- `WithTlsTargetName` overrides the default server name used during TLS handshake. Setting an incorrect value will cause certificate validation to fail unless `WithSkipTlsVerification` is also used.
