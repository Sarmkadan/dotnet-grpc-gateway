#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace DotNetGrpcGateway.Utilities;

/// <summary>
/// Validation helpers for DateTime operations.
/// Provides validation, checking, and exception-throwing utilities for date/time values
/// that are used with DateTimeUtility methods.
/// </summary>
public static class DateTimeUtilityValidation
{
    /// <summary>
    /// Validates a DateTime value to ensure it's a valid UTC date/time.
    /// </summary>
    /// <param name="dateTime">The DateTime value to validate.</param>
    /// <param name="paramName">The name of the parameter being validated (for error messages).</param>
    /// <returns>An empty list if valid, or a list of validation error messages if invalid.</returns>
    public static IReadOnlyList<string> Validate(this DateTime dateTime, string paramName = "dateTime")
    {
        var errors = new List<string>();

        if (dateTime.Kind == DateTimeKind.Unspecified)
        {
            errors.Add($"{paramName}: DateTime must have a specified kind (UTC or Local), but was Unspecified.");
        }

        if (dateTime == default)
        {
            errors.Add($"{paramName}: DateTime cannot be the default value (0001-01-01).");
        }

        if (dateTime < DateTime.MinValue || dateTime > DateTime.MaxValue)
        {
            errors.Add($"{paramName}: DateTime value is out of valid range.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a nullable DateTime value to ensure it's a valid UTC date/time.
    /// </summary>
    /// <param name="dateTime">The nullable DateTime value to validate.</param>
    /// <param name="paramName">The name of the parameter being validated (for error messages).</param>
    /// <returns>An empty list if valid or null, or a list of validation error messages if invalid.</returns>
    public static IReadOnlyList<string> Validate(this DateTime? dateTime, string paramName = "dateTime")
    {
        if (dateTime == null)
        {
            return Array.Empty<string>();
        }

        return dateTime.Value.Validate(paramName);
    }

    /// <summary>
    /// Validates a long milliseconds value to ensure it's non-negative.
    /// </summary>
    /// <param name="milliseconds">The milliseconds value to validate.</param>
    /// <param name="paramName">The name of the parameter being validated (for error messages).</param>
    /// <returns>An empty list if valid, or a list of validation error messages if invalid.</returns>
    public static IReadOnlyList<string> Validate(this long milliseconds, string paramName = "milliseconds")
    {
        var errors = new List<string>();

        if (milliseconds < 0)
        {
            errors.Add($"{paramName}: Milliseconds value cannot be negative. Received: {milliseconds}");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a nullable long milliseconds value to ensure it's non-negative.
    /// </summary>
    /// <param name="milliseconds">The nullable milliseconds value to validate.</param>
    /// <param name="paramName">The name of the parameter being validated (for error messages).</param>
    /// <returns>An empty list if valid or null, or a list of validation error messages if invalid.</returns>
    public static IReadOnlyList<string> Validate(this long? milliseconds, string paramName = "milliseconds")
    {
        if (milliseconds == null)
        {
            return Array.Empty<string>();
        }

        return milliseconds.Value.Validate(paramName);
    }

    /// <summary>
    /// Validates two DateTime values for operations that compare them.
    /// </summary>
    /// <param name="from">The starting DateTime.</param>
    /// <param name="to">The ending DateTime.</param>
    /// <param name="fromParamName">The name of the 'from' parameter.</param>
    /// <param name="toParamName">The name of the 'to' parameter.</param>
    /// <returns>An empty list if valid, or a list of validation error messages if invalid.</returns>
    public static IReadOnlyList<string> Validate(
        this DateTime from,
        DateTime to,
        string fromParamName = "from",
        string toParamName = "to")
    {
        var errors = new List<string>();

        var fromErrors = from.Validate(fromParamName);
        var toErrors = to.Validate(toParamName);

        errors.AddRange(fromErrors);
        errors.AddRange(toErrors);

        if (fromErrors.Count == 0 && toErrors.Count == 0)
        {
            if (from > to)
            {
                errors.Add($"{fromParamName} ({from}) cannot be after {toParamName} ({to}).");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates two DateTime values for operations that compare them (nullable version).
    /// </summary>
    /// <param name="from">The nullable starting DateTime.</param>
    /// <param name="to">The nullable ending DateTime.</param>
    /// <param name="fromParamName">The name of the 'from' parameter.</param>
    /// <param name="toParamName">The name of the 'to' parameter.</param>
    /// <returns>An empty list if valid or both null, or a list of validation error messages if invalid.</returns>
    public static IReadOnlyList<string> Validate(
        this DateTime? from,
        DateTime? to,
        string fromParamName = "from",
        string toParamName = "to")
    {
        if (from == null && to == null)
        {
            return Array.Empty<string>();
        }

        if (from == null)
        {
            return to!.Value.Validate(toParamName);
        }

        if (to == null)
        {
            return from.Value.Validate(fromParamName);
        }

        return from.Value.Validate(to.Value, fromParamName, toParamName);
    }

    /// <summary>
    /// Validates a DateTime value for business hours check.
    /// </summary>
    /// <param name="dateTime">The DateTime value to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>An empty list if valid, or a list of validation error messages if invalid.</returns>
    public static IReadOnlyList<string> ValidateForBusinessHours(this DateTime dateTime, string paramName = "dateTime")
    {
        return dateTime.Validate(paramName);
    }

    /// <summary>
    /// Checks if a DateTime value is valid (UTC, not default).
    /// </summary>
    /// <param name="dateTime">The DateTime value to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this DateTime dateTime)
    {
        return dateTime.Validate().Count == 0;
    }

    /// <summary>
    /// Checks if a nullable DateTime value is valid (UTC, not default, not null).
    /// </summary>
    /// <param name="dateTime">The nullable DateTime value to check.</param>
    /// <returns>True if valid or null; otherwise, false.</returns>
    public static bool IsValid(this DateTime? dateTime)
    {
        return dateTime.Validate().Count == 0;
    }

    /// <summary>
    /// Checks if a long milliseconds value is valid (non-negative).
    /// </summary>
    /// <param name="milliseconds">The milliseconds value to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this long milliseconds)
    {
        return milliseconds.Validate().Count == 0;
    }

    /// <summary>
    /// Checks if a nullable long milliseconds value is valid (non-negative or null).
    /// </summary>
    /// <param name="milliseconds">The nullable milliseconds value to check.</param>
    /// <returns>True if valid or null; otherwise, false.</returns>
    public static bool IsValid(this long? milliseconds)
    {
        return milliseconds.Validate().Count == 0;
    }

    /// <summary>
    /// Checks if two DateTime values are valid for comparison operations.
    /// </summary>
    /// <param name="from">The starting DateTime.</param>
    /// <param name="to">The ending DateTime.</param>
    /// <returns>True if both are valid and from <= to; otherwise, false.</returns>
    public static bool IsValid(this DateTime from, DateTime to)
    {
        return from.Validate(to).Count == 0;
    }

    /// <summary>
    /// Checks if two nullable DateTime values are valid for comparison operations.
    /// </summary>
    /// <param name="from">The nullable starting DateTime.</param>
    /// <param name="to">The nullable ending DateTime.</param>
    /// <returns>True if both are valid/consistent and from <= to; otherwise, false.</returns>
    public static bool IsValid(this DateTime? from, DateTime? to)
    {
        return from.Validate(to).Count == 0;
    }

    /// <summary>
    /// Ensures that a DateTime value is valid (UTC, not default), throwing an ArgumentException if not.
    /// </summary>
    /// <param name="dateTime">The DateTime value to validate.</param>
    /// <param name="paramName">The name of the parameter being validated (defaults to "dateTime").</param>
    /// <exception cref="ArgumentException">Thrown when value is invalid.</exception>
    public static void EnsureValid(this DateTime dateTime, string paramName = "dateTime")
    {
        var errors = dateTime.Validate(paramName);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"DateTime validation failed for {paramName}:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }

    /// <summary>
    /// Ensures that a nullable DateTime value is valid (UTC, not default) or null, throwing an ArgumentException if not.
    /// </summary>
    /// <param name="dateTime">The nullable DateTime value to validate.</param>
    /// <param name="paramName">The name of the parameter being validated (defaults to "dateTime").</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null and null is not allowed.</exception>
    /// <exception cref="ArgumentException">Thrown when value contains validation errors.</exception>
    public static void EnsureValid(this DateTime? dateTime, string paramName = "dateTime")
    {
        if (dateTime == null)
        {
            return;
        }

        var errors = dateTime.Value.Validate(paramName);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"DateTime validation failed for {paramName}:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }

    /// <summary>
    /// Ensures that a long milliseconds value is valid (non-negative), throwing an ArgumentException if not.
    /// </summary>
    /// <param name="milliseconds">The milliseconds value to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentException">Thrown when value is invalid.</exception>
    public static void EnsureValid(this long milliseconds, string paramName = "milliseconds")
    {
        var errors = milliseconds.Validate(paramName);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Milliseconds validation failed for {paramName}:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }

    /// <summary>
    /// Ensures that a nullable long milliseconds value is valid (non-negative) or null, throwing an ArgumentException if not.
    /// </summary>
    /// <param name="milliseconds">The nullable milliseconds value to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentException">Thrown when value contains validation errors.</exception>
    public static void EnsureValid(this long? milliseconds, string paramName = "milliseconds")
    {
        if (milliseconds == null)
        {
            return;
        }

        var errors = milliseconds.Value.Validate(paramName);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Milliseconds validation failed for {paramName}:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }

    /// <summary>
    /// Ensures that two DateTime values are valid for comparison (from <= to), throwing an ArgumentException if not.
    /// </summary>
    /// <param name="from">The starting DateTime.</param>
    /// <param name="to">The ending DateTime.</param>
    /// <param name="fromParamName">The name of the 'from' parameter.</param>
    /// <param name="toParamName">The name of the 'to' parameter.</param>
    /// <exception cref="ArgumentException">Thrown when values are invalid or from > to.</exception>
    public static void EnsureValid(
        this DateTime from,
        DateTime to,
        string fromParamName = "from",
        string toParamName = "to")
    {
        var errors = from.Validate(to, fromParamName, toParamName);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"DateTime comparison validation failed for {fromParamName} and {toParamName}:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }

    /// <summary>
    /// Ensures that two nullable DateTime values are valid for comparison (from <= to), throwing an ArgumentException if not.
    /// </summary>
    /// <param name="from">The nullable starting DateTime.</param>
    /// <param name="to">The nullable ending DateTime.</param>
    /// <param name="fromParamName">The name of the 'from' parameter.</param>
    /// <param name="toParamName">The name of the 'to' parameter.</param>
    /// <exception cref="ArgumentException">Thrown when values are invalid or from > to.</exception>
    public static void EnsureValid(
        this DateTime? from,
        DateTime? to,
        string fromParamName = "from",
        string toParamName = "to")
    {
        var errors = from.Validate(to, fromParamName, toParamName);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"DateTime comparison validation failed for {fromParamName} and {toParamName}:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }

    /// <summary>
    /// Ensures that a DateTime value is valid for business hours check, throwing an ArgumentException if not.
    /// </summary>
    /// <param name="dateTime">The DateTime value to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentException">Thrown when value is invalid.</exception>
    public static void EnsureValidForBusinessHours(this DateTime dateTime, string paramName = "dateTime")
    {
        var errors = dateTime.ValidateForBusinessHours(paramName);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"DateTime business hours validation failed for {paramName}:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}