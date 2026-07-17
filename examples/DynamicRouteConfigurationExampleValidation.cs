#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace DotNetGrpcGateway.Examples;

/// <summary>
/// Provides validation helpers for route configuration objects used in
/// DynamicRouteConfigurationExample.
/// </summary>
public sealed class DynamicRouteConfigurationExampleValidation
{
    /// <summary>
    /// Validates a route configuration object.
    /// </summary>
    /// <param name="value">The route configuration to validate</param>
    /// <returns>List of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public IReadOnlyList<string> Validate(object? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Use reflection to get all properties from the anonymous type
        var properties = value.GetType().GetProperties();

        foreach (var property in properties)
        {
            var propValue = property.GetValue(value);
            var propName = property.Name;

            switch (propName)
            {
                case "pattern" when string.IsNullOrWhiteSpace((string?)propValue):
                    problems.Add("Pattern cannot be null or whitespace.");
                    break;

                case "targetServiceId" when propValue is int serviceId && serviceId <= 0:
                    problems.Add("TargetServiceId must be a positive integer.");
                    break;

                case "matchType" when propValue is int matchType && (matchType < 0 || matchType > 2):
                    problems.Add("MatchType must be 0 (exact), 1 (prefix), or 2 (regex).");
                    break;

                case "priority" when propValue is int priority && (priority < 0 || priority > 1000):
                    problems.Add("Priority must be between 0 and 1000.");
                    break;

                case "rateLimitPerMinute" when propValue is int rateLimit && (rateLimit <= 0 || rateLimit > 100000):
                    problems.Add("RateLimitPerMinute must be between 1 and 100,000.");
                    break;

                case "cacheDurationSeconds" when propValue is int cacheDuration && cacheDuration < 0:
                    problems.Add("CacheDurationSeconds cannot be negative.");
                    break;

                case "description" when string.IsNullOrWhiteSpace((string?)propValue):
                    problems.Add("Description cannot be null or whitespace.");
                    break;

                case "tags" when propValue is string[] tags && tags.Length > 50:
                    problems.Add("Tags array cannot exceed 50 elements.");
                    break;

                case "createdAt" when propValue is string createdAtStr:
                    if (!DateTime.TryParse(createdAtStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _))
                    {
                        problems.Add("CreatedAt must be a valid ISO 8601 date/time string.");
                    }
                    break;
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a route configuration object is valid.
    /// </summary>
    /// <param name="value">The route configuration to check</param>
    /// <returns>True if valid; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public bool IsValid(object? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a route configuration object is valid, throwing an exception
    /// with detailed validation messages if it is not.
    /// </summary>
    /// <param name="value">The route configuration to validate</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public void EnsureValid(object? value)
    {
        var problems = Validate(value);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Route configuration is invalid:{Environment.NewLine} - {
                    string.Join(Environment.NewLine + " - ", problems)
                }");
        }
    }
}