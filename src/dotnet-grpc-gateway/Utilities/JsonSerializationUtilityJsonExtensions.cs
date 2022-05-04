#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;

namespace DotNetGrpcGateway.Utilities;

/// <summary>
/// Provides JSON serialization extension methods for JsonSerializationUtility type.
/// </summary>
public static class JsonSerializationUtilityJsonExtensions
{
    // Cached options matching JsonSerializationUtility.DefaultOptions
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a JsonSerializationUtility instance to a JSON string.
    /// </summary>
    /// <param name="value">The JsonSerializationUtility instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability</param>
    /// <returns>A JSON string representation</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static string ToJson(this JsonSerializationUtility? value, bool indented = false)
        => JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        } : DefaultOptions);

    /// <summary>
    /// Deserializes a JSON string to a JsonSerializationUtility instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>A JsonSerializationUtility instance if successful; otherwise, null</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
    public static JsonSerializationUtility? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<JsonSerializationUtility>(json, DefaultOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a JsonSerializationUtility instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Output parameter receiving the deserialized instance</param>
    /// <returns>True if deserialization succeeded; otherwise, false</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
    public static bool TryFromJson(string json, out JsonSerializationUtility? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<JsonSerializationUtility>(json, DefaultOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}