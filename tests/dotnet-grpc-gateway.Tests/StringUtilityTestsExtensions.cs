#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text;
using System.Text.RegularExpressions;

namespace DotNetGrpcGateway.Tests;

public static class StringUtilityTestsExtensions
{
    /// <summary>
    /// Creates a string with repeated characters for testing purposes.
    /// </summary>
    /// <param name="character">The character to repeat</param>
    /// <param name="count">Number of repetitions</param>
    /// <returns>A string with the specified character repeated</returns>
    public static string Repeat(this char character, int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be non-negative");

        return new string(character, count);
    }

    /// <summary>
    /// Checks if a string contains only alphabetic characters (no numbers or special chars)
    /// </summary>
    /// <param name="input">The string to check</param>
    /// <returns>True if the string contains only letters A-Z or a-z</returns>
    public static bool IsAlphabetic(this string? input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        return Regex.IsMatch(input, "^[a-zA-Z]+", RegexOptions.Compiled);
    }

    /// <summary>
    /// Checks if a string contains only numeric characters (no letters or special chars)
    /// </summary>
    /// <param name="input">The string to check</param>
    /// <returns>True if the string contains only digits 0-9</returns>
    public static bool IsNumeric(this string? input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        return Regex.IsMatch(input, "^[0-9]+", RegexOptions.Compiled);
    }

    /// <summary>
    /// Counts the number of lines in a string (splits by Environment.NewLine)
    /// </summary>
    /// <param name="input">The string to analyze</param>
    /// <returns>Number of lines in the string</returns>
    public static int CountLines(this string? input)
    {
        if (string.IsNullOrEmpty(input))
            return 0;

        return input.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Length;
    }

    /// <summary>
    /// Removes all whitespace characters from a string
    /// </summary>
    /// <param name="input">The string to process</param>
    /// <returns>A new string with all whitespace removed</returns>
    public static string RemoveWhitespace(this string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return Regex.Replace(input, @"\s+", "", RegexOptions.Compiled);
    }
}