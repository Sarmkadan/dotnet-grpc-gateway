#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetGrpcGateway.Examples;

/// <summary>
/// System.Text.Json serialization extensions for AuthenticationAndSecurityExample
/// </summary>
public static class AuthenticationAndSecurityExampleJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes AuthenticationAndSecurityExample to JSON string.
    /// </summary>
    /// <param name="value">The AuthenticationAndSecurityExample instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>JSON string representation</returns>
    public static string ToJson(this AuthenticationAndSecurityExample value, bool indented = false)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        var options = indented ? new JsonSerializerOptions(_jsonOptions)
        {
            WriteIndented = true
        } : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes AuthenticationAndSecurityExample from JSON string.
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>Deserialized AuthenticationAndSecurityExample instance or null if JSON is invalid</returns>
    public static AuthenticationAndSecurityExample? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<AuthenticationAndSecurityExample>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize AuthenticationAndSecurityExample from JSON string.
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="value">Output parameter for the deserialized value</param>
    /// <returns>True if deserialization succeeded, false otherwise</returns>
    public static bool TryFromJson(string json, out AuthenticationAndSecurityExample? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<AuthenticationAndSecurityExample>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}