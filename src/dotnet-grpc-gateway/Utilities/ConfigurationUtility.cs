// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Utilities;

/// <summary>
/// Configuration utilities for reading, validating, and transforming configuration values.
/// Provides type-safe access to configuration with fallback defaults.
/// </summary>
public static class ConfigurationUtility
{
    /// <summary>
    /// Gets a configuration value or returns default if not found or invalid.
    /// </summary>
    public static T GetConfigValue<T>(IConfiguration config, string key, T defaultValue)
    {
        try
        {
            var value = config[key];
            if (value == null)
                return defaultValue;

            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Gets a boolean configuration value with fallback default.
    /// </summary>
    public static bool GetBoolValue(IConfiguration config, string key, bool defaultValue = false)
    {
        var value = config[key];
        if (string.IsNullOrEmpty(value))
            return defaultValue;

        return bool.TryParse(value, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Gets an integer configuration value with fallback default.
    /// </summary>
    public static int GetIntValue(IConfiguration config, string key, int defaultValue = 0)
    {
        var value = config[key];
        if (string.IsNullOrEmpty(value))
            return defaultValue;

        return int.TryParse(value, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Gets a TimeSpan configuration value with fallback default.
    /// </summary>
    public static TimeSpan GetTimeSpanValue(IConfiguration config, string key, TimeSpan defaultValue)
    {
        var value = config[key];
        if (string.IsNullOrEmpty(value))
            return defaultValue;

        if (TimeSpan.TryParse(value, out var result))
            return result;

        // Try to parse as seconds
        if (int.TryParse(value, out var seconds))
            return TimeSpan.FromSeconds(seconds);

        return defaultValue;
    }

    /// <summary>
    /// Gets a section from configuration and binds it to an object.
    /// </summary>
    public static T? GetSection<T>(IConfiguration config, string sectionName) where T : class, new()
    {
        try
        {
            var section = config.GetSection(sectionName);
            if (!section.Exists())
                return null;

            var result = new T();
            section.Bind(result);
            return result;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Validates that a required configuration key is present and non-empty.
    /// </summary>
    public static bool ValidateRequiredKey(IConfiguration config, string key)
    {
        var value = config[key];
        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Gets all configuration keys matching a pattern.
    /// </summary>
    public static IEnumerable<string> GetKeysMatchingPattern(IConfiguration config, string pattern)
    {
        return GetAllKeys(config).Where(k => k.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets all configuration keys recursively.
    /// </summary>
    public static IEnumerable<string> GetAllKeys(IConfiguration config)
    {
        var keys = new List<string>();

        foreach (var child in config.GetChildren())
        {
            keys.Add(child.Key);

            var childKeys = GetAllKeys(child);
            foreach (var key in childKeys)
            {
                keys.Add($"{child.Key}:{key}");
            }
        }

        return keys;
    }

    /// <summary>
    /// Merges multiple configuration sources with prioritization.
    /// Earlier sources have higher priority (override later sources).
    /// </summary>
    public static Dictionary<string, string?> MergeConfigurations(
        params Dictionary<string, string?>[] sources)
    {
        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        // Iterate in reverse to respect priority (first source wins)
        for (int i = sources.Length - 1; i >= 0; i--)
        {
            foreach (var kvp in sources[i])
            {
                result[kvp.Key] = kvp.Value;
            }
        }

        return result;
    }

    /// <summary>
    /// Checks if environment is development.
    /// </summary>
    public static bool IsDevelopment(IWebHostEnvironment env)
    {
        return env?.IsDevelopment() ?? false;
    }

    /// <summary>
    /// Checks if environment is production.
    /// </summary>
    public static bool IsProduction(IWebHostEnvironment env)
    {
        return env?.IsProduction() ?? false;
    }
}
