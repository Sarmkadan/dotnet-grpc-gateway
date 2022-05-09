#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetGrpcGateway.Tests;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="RequestMetricTests"/>.
/// </summary>
public static class RequestMetricTestsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes the <see cref="RequestMetricTests"/> to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="RequestMetricTests"/> to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON string representation of the <see cref="RequestMetricTests"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this RequestMetricTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a <see cref="RequestMetricTests"/> from a JSON string.
    /// </summary>
    /// <param name="json">JSON string to deserialize. Can be <see langword="null"/> or whitespace.</param>
    /// <returns>The deserialized <see cref="RequestMetricTests"/>, or <see langword="null"/> if <paramref name="json"/> is <see langword="null"/> or empty.</returns>
    public static RequestMetricTests? FromJson(string json) =>
        string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<RequestMetricTests>(json, _jsonSerializerOptions);

    /// <summary>
    /// Attempts to deserialize a <see cref="RequestMetricTests"/> from a JSON string.
    /// </summary>
    /// <param name="json">JSON string to deserialize. Can be <see langword="null"/> or whitespace.</param>
    /// <param name="value">Output parameter for the deserialized <see cref="RequestMetricTests"/>.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out RequestMetricTests? value)
    {
        value = null;

        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<RequestMetricTests>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}