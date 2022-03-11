using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetGrpcGateway.Controllers;

public static class ReflectionControllerJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string ToJson(this ReflectionController value, bool indented = false)
    {
        if (value == null)
        {
            return "{}";
        }

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    public static ReflectionController? FromJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<ReflectionController>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static bool TryFromJson(string json, out ReflectionController? value)
    {
        value = null;
        try
        {
            value = JsonSerializer.Deserialize<ReflectionController>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}