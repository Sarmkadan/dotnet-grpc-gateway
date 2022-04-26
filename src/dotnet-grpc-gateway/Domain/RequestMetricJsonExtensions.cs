#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetGrpcGateway.Domain;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="RequestMetric"/>.
/// </summary>
public static class RequestMetricJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes the <see cref="RequestMetric"/> to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="RequestMetric"/> to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON string representation of the <see cref="RequestMetric"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this RequestMetric value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentException.ThrowIfNullOrWhiteSpace(value.ServiceName);
        ArgumentException.ThrowIfNullOrWhiteSpace(value.MethodName);
        ArgumentException.ThrowIfNullOrWhiteSpace(value.ClientIpAddress);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a <see cref="RequestMetric"/> from a JSON string.
    /// </summary>
    /// <param name="json">JSON string to deserialize. Can be <see langword="null"/> or whitespace.</param>
    /// <returns>The deserialized <see cref="RequestMetric"/>, or <see langword="null"/> if <paramref name="json"/> is <see langword="null"/> or empty.</returns>
    public static RequestMetric? FromJson(string json)
    {
        return string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<RequestMetric>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a <see cref="RequestMetric"/> from a JSON string.
    /// </summary>
    /// <param name="json">JSON string to deserialize. Can be <see langword="null"/> or whitespace.</param>
    /// <param name="value">Output parameter for the deserialized <see cref="RequestMetric"/>.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    public static bool TryFromJson(string json, out RequestMetric? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<RequestMetric>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}