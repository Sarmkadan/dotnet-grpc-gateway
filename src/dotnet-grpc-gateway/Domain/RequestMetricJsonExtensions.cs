#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetGrpcGateway.Domain;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="RequestMetric"/>.
/// </summary>
public static class RequestMetricJsonExtensions
{
    // Maximum allowed JSON payload size: 1 MB
    // This prevents DoS attacks via oversized payloads while allowing reasonable metric sizes
    private const int MaxJsonPayloadSizeBytes = 1_048_576; // 1 MB

    // Hardened JSON serializer options for deserializing untrusted input
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        // Prevent potential DoS via deep nesting
        MaxDepth = 64
        // Note: TypeNameHandling is not enabled by default in JsonSerializerDefaults.Web
        // This prevents polymorphic type resolution attacks
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
    /// <exception cref="ArgumentException"><paramref name="json"/> exceeds <see cref="MaxJsonPayloadSizeBytes"/> bytes.</exception>
    /// <exception cref="JsonException">JSON is malformed or cannot be deserialized.</exception>
    public static RequestMetric? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        // Enforce maximum payload size to prevent DoS attacks via oversized payloads
        if (Encoding.UTF8.GetByteCount(json) > MaxJsonPayloadSizeBytes)
        {
            throw new JsonException($"JSON payload exceeds maximum allowed size of {MaxJsonPayloadSizeBytes} bytes");
        }

        var metric = JsonSerializer.Deserialize<RequestMetric>(json, _jsonOptions);

        // Validate the deserialized object to ensure it contains required fields
        metric?.Validate();

        return metric;
    }

    /// <summary>
    /// Attempts to deserialize a <see cref="RequestMetric"/> from a JSON string.
    /// </summary>
    /// <param name="json">JSON string to deserialize. Can be <see langword="null"/> or whitespace.</param>
    /// <param name="value">Output parameter for the deserialized <see cref="RequestMetric"/>.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> exceeds <see cref="MaxJsonPayloadSizeBytes"/> bytes.</exception>
    public static bool TryFromJson(string json, out RequestMetric? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        // Enforce maximum payload size to prevent DoS attacks via oversized payloads
        if (Encoding.UTF8.GetByteCount(json) > MaxJsonPayloadSizeBytes)
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<RequestMetric>(json, _jsonOptions);

            // Validate the deserialized object to ensure it contains required fields
            value?.Validate();

            return value is not null;
        }
        catch (JsonException) when (json.Length > 0)
        {
            // Only catch JsonException for non-empty strings
            return false;
        }
        catch (InvalidOperationException)
        {
            // Catch validation errors as deserialization failures
            return false;
        }
    }
}