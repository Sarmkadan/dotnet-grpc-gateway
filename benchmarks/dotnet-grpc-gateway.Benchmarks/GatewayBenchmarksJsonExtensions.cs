using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace dotnet_grpc_gateway.Benchmarks
{
    /// <summary>
    /// Provides JSON serialization and deserialization extensions for <see cref="GatewayBenchmarks"/> instances.
    /// </summary>
    public static class GatewayBenchmarksJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes the GatewayBenchmarks instance to a JSON string.
        /// </summary>
        /// <param name="value">The GatewayBenchmarks instance to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the GatewayBenchmarks instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        public static string ToJson(this GatewayBenchmarks value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(_jsonSerializerOptions)
                {
                    WriteIndented = true
                }
                : _jsonSerializerOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string to a GatewayBenchmarks instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>A GatewayBenchmarks instance, or <see langword="null"/> if deserialization fails.</returns>
        /// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/>, empty, or whitespace.</exception>
        public static GatewayBenchmarks? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(json);

            try
            {
                return JsonSerializer.Deserialize<GatewayBenchmarks>(json, _jsonSerializerOptions);
            }
            catch (JsonException ex)
            {
                throw new FormatException("The provided JSON string is not a valid GatewayBenchmarks serialization.", ex);
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a GatewayBenchmarks instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized GatewayBenchmarks instance, or <see langword="null"/> if deserialization fails.</param>
        /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/>, empty, or whitespace.</exception>
        public static bool TryFromJson(string json, out GatewayBenchmarks? value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(json);

            try
            {
                value = JsonSerializer.Deserialize<GatewayBenchmarks>(json, _jsonSerializerOptions);
                return true;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }
    }
}