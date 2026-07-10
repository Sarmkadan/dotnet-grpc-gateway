# GatewayUtilities

A utility class providing common helper methods for gRPC Gateway operations such as request ID generation, JSON serialization/deserialization, time measurement, data formatting, hashing, token generation, and dictionary manipulation.

## API

### `public static string GenerateRequestId()`

Generates a unique request identifier string. The format is implementation-defined and suitable for tracing requests across services.

**Returns**
A non-null, non-empty string representing the generated request ID.

---

### `public static string ToJson<T>(T value)`

Serializes the given value to a JSON string.

**Type Parameters**
- `T`: The type of the value to serialize.

**Parameters**
- `value`: The value to serialize. Can be `null`.

**Returns**
A JSON string representation of the value, or `null` if the input is `null`.

---

### `public static T? FromJson<T>(string? json)`

Deserializes a JSON string back into an object of type `T`.

**Type Parameters**
- `T`: The target type to deserialize into.

**Parameters**
- `json`: The JSON string to deserialize. Can be `null`.

**Returns**
The deserialized object of type `T`, or `null` if the input is `null` or deserialization fails.

**Throws**
- `JsonException`: If the JSON is malformed or cannot be deserialized into type `T`.

---

### `public static TimeSpan GetElapsedTime(DateTime startTime)`

Calculates the elapsed time between the given start time and the current UTC time.

**Parameters**
- `startTime`: The start time as a UTC `DateTime`.

**Returns**
A `TimeSpan` representing the duration between `startTime` and `DateTime.UtcNow`.

---

### `public static string FormatDuration(TimeSpan duration)`

Formats a `TimeSpan` into a human-readable string (e.g., "1.23s", "45ms").

**Parameters**
- `duration`: The time span to format.

**Returns**
A non-null string representing the formatted duration.

---

### `public static string FormatBytes(long bytes)`

Converts a byte count into a human-readable file size string (e.g., "1.23 KB", "45 B").

**Parameters**
- `bytes`: The number of bytes to format.

**Returns**
A non-null string representing the formatted size.

---

### `public static string NormalizeServiceName(string name)`

Normalizes a service name by trimming whitespace and converting to lowercase.

**Parameters**
- `name`: The service name to normalize.

**Returns**
A normalized service name string, or `null` if the input is `null`.

---

### `public static string ComputeSha256Hash(string input)`

Computes the SHA-256 hash of the given input string and returns it as a hexadecimal string.

**Parameters**
- `input`: The string to hash.

**Returns**
A non-null hexadecimal string representing the SHA-256 hash.

**Throws**
- `ArgumentNullException`: If `input` is `null`.

---

### `public static string GenerateRandomToken(int length = 32)`

Generates a cryptographically secure random token of the specified length.

**Parameters**
- `length`: The desired length of the token in bytes. Defaults to 32.

**Returns**
A non-null random token string.

**Throws**
- `ArgumentOutOfRangeException`: If `length` is less than 1.

---

### `public static T? SafeGetValue<T>(this Dictionary<string, object>? dictionary, string key)`

Safely retrieves a value from a dictionary and casts it to the specified type.

**Type Parameters**
- `T`: The expected type of the value.

**Parameters**
- `dictionary`: The dictionary to search, or `null`.
- `key`: The key to look up.

**Returns**
The value cast to type `T`, or `default` if the key is not found, the dictionary is `null`, or the value cannot be cast.

---
### `public static Dictionary<string, T> MergeDictionaries<T>(params Dictionary<string, T>[] dictionaries)`

Merges multiple dictionaries into a single dictionary. In case of duplicate keys, the last occurrence wins.

**Type Parameters**
- `T`: The type of values in the dictionaries.

**Parameters**
- `dictionaries`: An array of dictionaries to merge.

**Returns**
A new dictionary containing all key-value pairs from the input dictionaries.

**Throws**
- `ArgumentNullException`: If `dictionaries` is `null`.

## Usage

### Example 1: Request Tracking and Timing
