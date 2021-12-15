// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;

namespace DotNetGrpcGateway.Utilities;

/// <summary>
/// Common utilities and helper methods for the gateway
/// </summary>
public static class GatewayUtilities
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    /// <summary>
    /// Generates a unique request ID
    /// </summary>
    public static string GenerateRequestId()
    {
        return Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// Serializes an object to JSON
    /// </summary>
    public static string ToJson<T>(T obj)
    {
        if (obj == null)
            return string.Empty;

        return JsonSerializer.Serialize(obj, JsonOptions);
    }

    /// <summary>
    /// Deserializes JSON to an object
    /// </summary>
    public static T? FromJson<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Calculates the elapsed time between two dates
    /// </summary>
    public static TimeSpan GetElapsedTime(DateTime from, DateTime to)
    {
        return to > from ? to - from : from - to;
    }

    /// <summary>
    /// Converts milliseconds to a human-readable format
    /// </summary>
    public static string FormatDuration(double milliseconds)
    {
        if (milliseconds < 1000)
            return $"{milliseconds:F2}ms";

        if (milliseconds < 60000)
            return $"{milliseconds / 1000:F2}s";

        return $"{milliseconds / 60000:F2}m";
    }

    /// <summary>
    /// Converts bytes to human-readable format
    /// </summary>
    public static string FormatBytes(long bytes)
    {
        var sizes = new[] { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:F2} {sizes[order]}";
    }

    /// <summary>
    /// Validates and normalizes a service name
    /// </summary>
    public static string NormalizeServiceName(string serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
            throw new ArgumentException("Service name cannot be empty", nameof(serviceName));

        // Remove leading/trailing whitespace
        serviceName = serviceName.Trim();

        // Replace spaces with dots
        serviceName = serviceName.Replace(" ", ".");

        // Remove invalid characters
        serviceName = System.Text.RegularExpressions.Regex.Replace(serviceName, @"[^a-zA-Z0-9._-]", "");

        if (string.IsNullOrEmpty(serviceName))
            throw new ArgumentException("Service name contains no valid characters", nameof(serviceName));

        return serviceName;
    }

    /// <summary>
    /// Computes the SHA256 hash of a string
    /// </summary>
    public static string ComputeSha256Hash(string input)
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hashedBytes);
        }
    }

    /// <summary>
    /// Generates a random token string
    /// </summary>
    public static string GenerateRandomToken(int length = 32)
    {
        using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
        {
            var tokenData = new byte[length];
            rng.GetBytes(tokenData);
            return Convert.ToBase64String(tokenData);
        }
    }

    /// <summary>
    /// Safely gets a value from a dictionary
    /// </summary>
    public static T? SafeGetValue<T>(Dictionary<string, T> dict, string key)
    {
        if (dict == null || string.IsNullOrWhiteSpace(key))
            return default;

        dict.TryGetValue(key, out var value);
        return value;
    }

    /// <summary>
    /// Merges two dictionaries
    /// </summary>
    public static Dictionary<string, T> MergeDictionaries<T>(
        Dictionary<string, T> first,
        Dictionary<string, T> second)
    {
        var result = new Dictionary<string, T>(first);

        foreach (var kvp in second)
        {
            result[kvp.Key] = kvp.Value;
        }

        return result;
    }
}
