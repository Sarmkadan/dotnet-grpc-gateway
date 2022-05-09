#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.RegularExpressions;

namespace DotNetGrpcGateway.Tests;

/// <summary>
/// Extension methods for string manipulation and validation used in tests.
/// Provides utility methods for common string operations during test scenarios.
/// </summary>
public static class StringUtilityTestsExtensions
{
    /// <summary>
    /// Creates a string with repeated characters for testing purposes.
    /// </summary>
    /// <param name="character">The character to repeat</param>
    /// <param name="count">Number of repetitions</param>
    /// <returns>A string with the specified character repeated</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is negative</exception>
    public static string Repeat(this char character, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        return new string(character, count);
    }

    /// <summary>
    /// Checks if a string contains only alphabetic characters (no numbers or special characters).
    /// </summary>
    /// <param name="input">The string to check</param>
    /// <returns>True if the string contains only letters A-Z or a-z; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null</exception>
    public static bool IsAlphabetic(this string? input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.Length == 0)
            return false;

        return Regex.IsMatch(input, "^[a-zA-Z]+$");
    }

    /// <summary>
    /// Checks if a string contains only numeric characters (no letters or special characters).
    /// </summary>
    /// <param name="input">The string to check</param>
    /// <returns>True if the string contains only digits 0-9; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null</exception>
    public static bool IsNumeric(this string? input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.Length == 0)
            return false;

        return Regex.IsMatch(input, "^[0-9]+$");
    }

    /// <summary>
    /// Counts the number of lines in a string using platform-appropriate line endings.
    /// </summary>
    /// <param name="input">The string to analyze</param>
    /// <returns>Number of lines in the string, or 0 for null/empty input</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null</exception>
    public static int CountLines(this string? input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.Length == 0)
            return 0;

        return input.Split(new[] { '\n', '\r' }, StringSplitOptions.None).Length;
    }

    /// <summary>
    /// Removes all whitespace characters from a string.
    /// </summary>
    /// <param name="input">The string to process</param>
    /// <returns>A new string with all whitespace removed, or empty string for null input</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null</exception>
    public static string RemoveWhitespace(this string? input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Regex.Replace(input, @"\s+", "");
    }
}