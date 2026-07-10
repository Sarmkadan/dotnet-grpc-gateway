using System.Text.Json;
using System.Text.Json.Serialization;

namespace dotnet_grpc_gateway.Benchmarks
{
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
        public static string ToJson(this GatewayBenchmarks value, bool indented = false)
        {
            if (value == null)
            {
                return "{}";
            }

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
        /// <returns>A GatewayBenchmarks instance, or null if deserialization fails.</returns>
        public static GatewayBenchmarks? FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<GatewayBenchmarks>(json, _jsonSerializerOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a GatewayBenchmarks instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized GatewayBenchmarks instance, or null if deserialization fails.</param>
        /// <returns>True if deserialization succeeds; otherwise, false.</returns>
        public static bool TryFromJson(string json, out GatewayBenchmarks? value)
        {
            value = null;

            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<GatewayBenchmarks>(json, _jsonSerializerOptions);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}