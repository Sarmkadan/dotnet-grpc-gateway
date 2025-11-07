// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.RegularExpressions;

namespace DotNetGrpcGateway.Utilities;

/// <summary>
/// String manipulation and validation utilities.
/// Provides methods for common string operations like sanitization, truncation, and pattern matching.
/// </summary>
public static class StringUtility
{
    /// <summary>
    /// Safely truncates a string to max length and appends ellipsis if truncated.
    /// </summary>
    public static string Truncate(string? value, int maxLength = 100)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Length <= maxLength)
            return value;

        return $"{value[..(maxLength - 3)]}...";
    }

    /// <summary>
    /// Removes whitespace characters from both ends and normalizes internal spaces.
    /// </summary>
    public static string NormalizeWhitespace(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Trim and collapse multiple spaces into single space
        return Regex.Replace(value.Trim(), @"\s+", " ");
    }

    /// <summary>
    /// Generates a slug from a string (lowercase, hyphenated, alphanumeric only).
    /// </summary>
    public static string ToSlug(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Convert to lowercase and remove non-alphanumeric characters
        var slug = Regex.Replace(value.ToLowerInvariant(), @"[^a-z0-9\s-]", "");
        // Replace spaces and multiple hyphens with single hyphen
        slug = Regex.Replace(slug, @"[\s-]+", "-");
        // Remove leading/trailing hyphens
        return slug.Trim('-');
    }

    /// <summary>
    /// Masks sensitive parts of a string (useful for logging credentials).
    /// </summary>
    public static string MaskSensitiveData(string? value, int showChars = 4)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= showChars)
            return "***";

        var visiblePart = value[..showChars];
        var maskedPart = new string('*', value.Length - showChars);
        return $"{visiblePart}{maskedPart}";
    }

    /// <summary>
    /// Checks if a string matches a wildcard pattern (e.g., "user.*.Get*").
    /// </summary>
    public static bool MatchesWildcardPattern(string? value, string? pattern)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(pattern))
            return false;

        // Convert wildcard pattern to regex
        var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
        return Regex.IsMatch(value, regexPattern, RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Converts camelCase to PascalCase.
    /// </summary>
    public static string ToPascalCase(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return char.ToUpperInvariant(value[0]) + value[1..];
    }

    /// <summary>
    /// Converts PascalCase or camelCase to kebab-case.
    /// </summary>
    public static string ToKebabCase(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Insert hyphen before uppercase letters
        var kebab = Regex.Replace(value, "(?<!^)(?=[A-Z])", "-");
        return kebab.ToLowerInvariant();
    }

    /// <summary>
    /// Checks if a string is a valid email format (basic validation).
    /// </summary>
    public static bool IsValidEmail(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(value);
            return addr.Address == value;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a string contains only alphanumeric characters.
    /// </summary>
    public static bool IsAlphanumeric(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        return Regex.IsMatch(value, @"^[a-zA-Z0-9]+$");
    }
}
