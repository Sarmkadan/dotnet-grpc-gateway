// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetGrpcGateway.Utilities;

/// <summary>
/// JSON serialization utilities with consistent settings and error handling.
/// Ensures uniform JSON formatting across the application.
/// </summary>
public static class JsonSerializationUtility
{
    // Default serialization options with camelCase property naming
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    // Pretty-print options for debugging and API responses
    private static readonly JsonSerializerOptions PrettyOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes an object to JSON string with default formatting.
    /// </summary>
    public static string Serialize<T>(T? value)
    {
        if (value == null)
            return "null";

        return JsonSerializer.Serialize(value, DefaultOptions);
    }

    /// <summary>
    /// Serializes an object to pretty-printed JSON string.
    /// </summary>
    public static string SerializePretty<T>(T? value)
    {
        if (value == null)
            return "null";

        return JsonSerializer.Serialize(value, PrettyOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to an object.
    /// </summary>
    public static T? Deserialize<T>(string json)
    {
        if (string.IsNullOrEmpty(json))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(json, DefaultOptions);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    /// <summary>
    /// Safely deserializes with error information.
    /// </summary>
    public static (bool Success, T? Data, string? Error) TryDeserialize<T>(string json)
    {
        if (string.IsNullOrEmpty(json))
            return (false, default, "JSON string is null or empty");

        try
        {
            var result = JsonSerializer.Deserialize<T>(json, DefaultOptions);
            return (true, result, null);
        }
        catch (JsonException ex)
        {
            return (false, default, ex.Message);
        }
    }

    /// <summary>
    /// Checks if a string is valid JSON.
    /// </summary>
    public static bool IsValidJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            JsonDocument.Parse(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Formats a JSON string with consistent indentation.
    /// </summary>
    public static string? FormatJson(string? json)
    {
        if (!IsValidJson(json))
            return json;

        try
        {
            using var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc.RootElement, PrettyOptions);
        }
        catch
        {
            return json;
        }
    }

    /// <summary>
    /// Merges two JSON objects (simple shallow merge).
    /// </summary>
    public static T? MergeObjects<T>(T? obj1, T? obj2) where T : class
    {
        if (obj1 == null)
            return obj2;
        if (obj2 == null)
            return obj1;

        var json1 = Serialize(obj1);
        var json2 = Serialize(obj2);

        try
        {
            using var doc1 = JsonDocument.Parse(json1);
            using var doc2 = JsonDocument.Parse(json2);

            var merged = MergeJsonElements(doc1.RootElement, doc2.RootElement);
            return JsonSerializer.Deserialize<T>(merged.GetRawText(), DefaultOptions);
        }
        catch
        {
            return obj2;
        }
    }

    /// <summary>
    /// Extracts a value from a JSON path (dot notation: "user.profile.name").
    /// </summary>
    public static object? GetValueByPath(string json, string path)
    {
        if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(path))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement;

            foreach (var segment in path.Split('.'))
            {
                if (element.TryGetProperty(segment, out var property))
                    element = property;
                else
                    return null;
            }

            return element.GetRawText();
        }
        catch
        {
            return null;
        }
    }

    private static JsonElement MergeJsonElements(JsonElement element1, JsonElement element2)
    {
        if (element1.ValueKind != JsonValueKind.Object || element2.ValueKind != JsonValueKind.Object)
            return element2;

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false }))
        {
            writer.WriteStartObject();

            foreach (var property in element1.EnumerateObject())
                property.WriteTo(writer);

            foreach (var property in element2.EnumerateObject())
                property.WriteTo(writer);

            writer.WriteEndObject();
        }

        stream.Position = 0;
        using var doc = JsonDocument.Parse(stream);
        return doc.RootElement.Clone();
    }
}
