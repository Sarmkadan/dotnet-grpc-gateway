// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using DotNetGrpcGateway.Utilities;

namespace DotNetGrpcGateway.Formatters;

/// <summary>
/// JSON output formatter. Converts objects to JSON strings with consistent formatting.
/// </summary>
public class JsonFormatter : IOutputFormatter
{
    public string ContentType => "application/json";

    public string Format<T>(T? data, bool pretty = false)
    {
        if (data == null)
            return "null";

        return pretty
            ? JsonSerializationUtility.SerializePretty(data)
            : JsonSerializationUtility.Serialize(data);
    }

    public async Task<string> FormatAsync<T>(T? data, bool pretty = false)
    {
        return await Task.FromResult(Format(data, pretty));
    }
}
