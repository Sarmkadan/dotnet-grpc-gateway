#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Security.Cryptography;
using System.Text;

namespace DotNetGrpcGateway.Extensions;

/// <summary>
/// Extension methods for string operations
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Hashes a string using SHA256
    /// </summary>
    /// <param name="input">The input string to hash.</param>
    /// <returns>The SHA256 hash as a hexadecimal string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="input"/> is empty or consists only of whitespace.</exception>
    public static string ToSha256Hash(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be empty or consist only of whitespace.", nameof(input));

        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hashedBytes);
    }

    /// <summary>
    /// Converts a string to a URL-safe slug
    /// </summary>
    /// <param name="input">The input string to convert to a slug.</param>
    /// <returns>A URL-safe slug string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string ToSlug(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var slug = input.Trim().ToLowerInvariant();
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");

        return slug.TrimEnd('-');
    }

    /// <summary>
    /// Truncates a string to a maximum length
    /// </summary>
    /// <param name="input">The input string to truncate.</param>
    /// <param name="maxLength">The maximum length of the resulting string.</param>
    /// <param name="suffix">The suffix to append when truncating. Defaults to "...".</param>
    /// <returns>The truncated string, or the original string if it's shorter than <paramref name="maxLength"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxLength"/> is less than 0.</exception>
    public static string Truncate(this string input, int maxLength, string suffix = "...")
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);

        if (input.Length <= maxLength)
            return input;

        return string.Concat(input.AsSpan(0, maxLength - suffix.Length), suffix);
    }

    /// <summary>
    /// Checks if a string is a valid IPv4 or IPv6 address
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns><see langword="true"/> if the string is a valid IP address; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidIpAddress(this string input)
    {
        return System.Net.IPAddress.TryParse(input, out _);
    }

    /// <summary>
    /// Checks if a string matches a pattern (supports wildcards)
    /// </summary>
    /// <param name="input">The input string to match.</param>
    /// <param name="pattern">The pattern to match against, supporting <c>*</c> and <c>?</c> wildcards.</param>
    /// <returns><see langword="true"/> if the input matches the pattern; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> or <paramref name="pattern"/> is <see langword="null"/>.</exception>
    public static bool MatchesPattern(this string input, string pattern)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(pattern);

        var regexPattern = System.Text.RegularExpressions.Regex.Escape(pattern)
            .Replace("\\*", ".*")
            .Replace("\\?", ".");

        return System.Text.RegularExpressions.Regex.IsMatch(input, $"^{regexPattern}$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
}