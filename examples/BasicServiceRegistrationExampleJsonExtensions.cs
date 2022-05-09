#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;

namespace DotNetGrpcGateway.Examples;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="BasicServiceRegistrationExample"/>.
/// </summary>
public static class BasicServiceRegistrationExampleJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes a <see cref="BasicServiceRegistrationExample"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="BasicServiceRegistrationExample"/> instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for human readability.</param>
    /// <returns>A JSON string representation of the <see cref="BasicServiceRegistrationExample"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this BasicServiceRegistrationExample value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a <see cref="BasicServiceRegistrationExample"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>
    /// The deserialized <see cref="BasicServiceRegistrationExample"/> instance if successful;
    /// <see langword="null"/> if <paramref name="json"/> is <see langword="null"/>, empty, or whitespace.
    /// </returns>
    public static BasicServiceRegistrationExample? FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<BasicServiceRegistrationExample>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a <see cref="BasicServiceRegistrationExample"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">When this method returns, contains the deserialized value if successful, otherwise <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="true"/> if deserialization succeeded;
    /// <see langword="false"/> if <paramref name="json"/> is <see langword="null"/>, empty, whitespace, or invalid JSON.
    /// </returns>
    public static bool TryFromJson(string? json, out BasicServiceRegistrationExample? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<BasicServiceRegistrationExample>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}