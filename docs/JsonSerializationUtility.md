# JsonSerializationUtility

The `JsonSerializationUtility` class provides a centralized, static interface for performing common JSON serialization and deserialization operations within the `dotnet-grpc-gateway` project. It abstracts underlying JSON handling logic to offer type-safe methods for converting objects to JSON strings, parsing JSON back into strongly-typed objects, validating JSON structure, formatting output, and performing dynamic data extraction or merging. This utility is designed to streamline data interchange patterns typical in gRPC gateway scenarios where HTTP/JSON payloads must be efficiently mapped to and from protobuf-derived models.

## API

### `Serialize<T>`
```csharp
public static string Serialize<T>(T value)
```
Converts the specified object into a compact JSON string representation.
*   **Parameters**: `value` - The object of type `T` to serialize.
*   **Returns**: A string containing the JSON representation of the object without extra whitespace.
*   **Throws**: Throws a serialization exception if the object graph contains circular references or types that are not supported by the underlying JSON serializer.

### `SerializePretty<T>`
```csharp
public static string SerializePretty<T>(T value)
```
Converts the specified object into a human-readable JSON string with indentation and line breaks.
*   **Parameters**: `value` - The object of type `T` to serialize.
*   **Returns**: A formatted string containing the JSON representation of the object.
*   **Throws**: Throws a serialization exception if the object cannot be serialized.

### `Deserialize<T>`
```csharp
public static T? Deserialize<T>(string json)
```
Parses a JSON string and converts it into an object of the specified type.
*   **Parameters**: `json` - The JSON string to parse.
*   **Returns**: An instance of `T` populated with data from the JSON string, or `null` if the JSON represents a null value.
*   **Throws**: Throws a deserialization exception if the JSON is malformed, empty, or cannot be mapped to the target type `T`.

### `TryDeserialize<T>`
```csharp
public static (bool Success, T? Data, string? Error) TryDeserialize<T>(string json)
```
Attempts to parse a JSON string into an object of the specified type without throwing exceptions on failure.
*   **Parameters**: `json` - The JSON string to parse.
*   **Returns**: A tuple containing:
    *   `Success`: `true` if parsing succeeded, otherwise `false`.
    *   `Data`: The deserialized object if successful, otherwise `default(T)`.
    *   `Error`: An error message describing the failure if `Success` is `false`, otherwise `null`.
*   **Throws**: This method does not throw exceptions for invalid JSON; errors are captured in the return tuple.

### `IsValidJson`
```csharp
public static bool IsValidJson(string json)
```
Validates whether a given string constitutes well-formed JSON.
*   **Parameters**: `json` - The string to validate.
*   **Returns**: `true` if the string is valid JSON; `false` otherwise.
*   **Throws**: Does not throw exceptions; returns `false` for null, empty, or malformed inputs.

### `FormatJson`
```csharp
public static string? FormatJson(string json)
```
Takes a compact JSON string and returns a pretty-printed version.
*   **Parameters**: `json` - The compact JSON string to format.
*   **Returns**: A formatted JSON string if the input is valid; `null` if the input is invalid or null.
*   **Throws**: Does not throw exceptions; returns `null` on failure.

### `MergeObjects<T>`
```csharp
public static T? MergeObjects<T>(T first, T second)
```
Merges two objects of the same type into a single instance, where properties from the second object overwrite properties from the first.
*   **Parameters**: 
    *   `first` - The base object.
    *   `second` - The object containing override values.
*   **Returns**: A new instance of `T` containing the merged data, or `null` if both inputs are null.
*   **Throws**: Throws an exception if the types are incompatible for merging or if internal property access fails.

### `GetValueByPath`
```csharp
public static object? GetValueByPath(string json, string path)
```
Extracts a specific value from a JSON string using a dot-notated path (e.g., "user.address.city").
*   **Parameters**: 
    *   `json` - The source JSON string.
    *   `path` - The dot-separated path to the desired value.
*   **Returns**: The value found at the specified path as an `object`, or `null` if the path does not exist or the JSON is invalid.
*   **Throws**: Does not throw exceptions for missing paths; returns `null`.

## Usage

### Example 1: Safe Deserialization with Fallback
This example demonstrates how to safely attempt deserialization of an incoming payload, providing a default instance if the JSON is malformed or missing required fields, utilizing `TryDeserialize`.

```csharp
using DotNetGrpcGateway;

public class UserConfig
{
    public string Username { get; set; } = string.Empty;
    public int TimeoutMs { get; set; } = 5000;
}

public void ProcessConfig(string incomingJson)
{
    var result = JsonSerializationUtility.TryDeserialize<UserConfig>(incomingJson);

    if (result.Success && result.Data != null)
    {
        Console.WriteLine($"Config loaded for user: {result.Data.Username}");
        ApplySettings(result.Data);
    }
    else
    {
        Console.WriteLine($"Failed to load config: {result.Error}. Using defaults.");
        // Fallback to a default instance
        ApplySettings(new UserConfig { Username = "default", TimeoutMs = 10000 });
    }
}
```

### Example 2: Dynamic Data Extraction and Merging
This example shows extracting a specific nested value from a raw JSON string without defining a full class, and then merging two configuration objects where the update takes precedence.

```csharp
using DotNetGrpcGateway;

public void HandlePartialUpdate(string fullPayload, string updatePayload)
{
    // Extract a specific flag without deserializing the whole object
    var isEnabled = JsonSerializationUtility.GetValueByPath(fullPayload, "features.newDashboard.enabled");
    
    if (isEnabled is bool flag && flag)
    {
        // Deserialize both objects
        var baseConfig = JsonSerializationUtility.Deserialize<DashboardConfig>(fullPayload);
        var updates = JsonSerializationUtility.Deserialize<DashboardConfig>(updatePayload);

        if (baseConfig != null && updates != null)
        {
            // Merge updates into the base config
            var finalConfig = JsonSerializationUtility.MergeObjects(baseConfig, updates);
            
            // Serialize for logging or storage
            string compactJson = JsonSerializationUtility.Serialize(finalConfig);
            SaveToStorage(compactJson);
        }
    }
}
```

## Notes

*   **Thread Safety**: As all members of `JsonSerializationUtility` are static and operate purely on input parameters without maintaining internal mutable state, the class is thread-safe. Multiple threads may safely call `Serialize`, `Deserialize`, and other methods concurrently.
*   **Null Handling**: 
    *   `Deserialize<T>` and `MergeObjects<T>` may return `null` if the input JSON represents a null token or if all inputs are null, respectively. Consumers should handle nullable reference types appropriately.
    *   `GetValueByPath` returns `null` both when the path is missing and when the value at the path is explicitly JSON `null`. Distinction requires checking the existence of the path via other means if necessary.
*   **Error Handling Strategy**: Prefer `TryDeserialize<T>` over `Deserialize<T>` when processing untrusted input (e.g., HTTP request bodies) to avoid try-catch overhead and control flow interruption. `Deserialize<T>` should be reserved for scenarios where valid JSON is guaranteed by prior validation or contract.
*   **Performance**: `Serialize<T>` produces compact JSON suitable for network transmission, while `SerializePretty<T>` and `FormatJson` introduce whitespace overhead and should be reserved for logging, debugging, or user-facing outputs.
*   **Path Syntax**: The `GetValueByPath` method expects standard dot notation. It does not support array indexing syntax (e.g., `items[0]`) unless explicitly implemented in the underlying parser; complex array traversals should rely on full deserialization.
