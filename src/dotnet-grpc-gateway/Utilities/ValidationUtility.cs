// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net;

namespace DotNetGrpcGateway.Utilities;

/// <summary>
/// Validation utilities for common patterns and data formats.
/// Provides methods for validating URLs, IPs, ports, and other network/data formats.
/// </summary>
public static class ValidationUtility
{
    /// <summary>
    /// Validates if a string is a valid absolute URI.
    /// </summary>
    public static bool IsValidUri(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        return Uri.TryCreate(value, UriKind.Absolute, out _);
    }

    /// <summary>
    /// Validates if a string is a valid IPv4 or IPv6 address.
    /// </summary>
    public static bool IsValidIpAddress(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        return IPAddress.TryParse(value, out _);
    }

    /// <summary>
    /// Validates if a port number is within valid range (1-65535).
    /// </summary>
    public static bool IsValidPort(int port)
    {
        return port > 0 && port <= 65535;
    }

    /// <summary>
    /// Validates if a string is a valid hostname or domain.
    /// </summary>
    public static bool IsValidHostname(string? value)
    {
        if (string.IsNullOrEmpty(value) || value.Length > 253)
            return false;

        // Check if hostname contains only valid characters
        return System.Text.RegularExpressions.Regex.IsMatch(
            value,
            @"^([a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)*[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?$");
    }

    /// <summary>
    /// Validates if a string is a valid GUID.
    /// </summary>
    public static bool IsValidGuid(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        return Guid.TryParse(value, out _);
    }

    /// <summary>
    /// Validates if an object is null or empty (works with strings, collections, etc.).
    /// </summary>
    public static bool IsNullOrEmpty(object? value)
    {
        if (value == null)
            return true;

        if (value is string str)
            return string.IsNullOrWhiteSpace(str);

        if (value is System.Collections.ICollection collection)
            return collection.Count == 0;

        return false;
    }

    /// <summary>
    /// Validates if a value falls within a numeric range.
    /// </summary>
    public static bool IsInRange(int value, int min, int max)
    {
        return value >= min && value <= max;
    }

    /// <summary>
    /// Validates if a value falls within a numeric range (decimal).
    /// </summary>
    public static bool IsInRange(decimal value, decimal min, decimal max)
    {
        return value >= min && value <= max;
    }

    /// <summary>
    /// Validates if a percent value is valid (0-100).
    /// </summary>
    public static bool IsValidPercentage(int value)
    {
        return IsInRange(value, 0, 100);
    }

    /// <summary>
    /// Validates if a string matches a minimum length requirement.
    /// </summary>
    public static bool MeetsMinimumLength(string? value, int minLength)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        return value.Length >= minLength;
    }

    /// <summary>
    /// Validates if a string has a reasonable format for a service name.
    /// Service names should be alphanumeric with optional dots and underscores.
    /// </summary>
    public static bool IsValidServiceName(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        return System.Text.RegularExpressions.Regex.IsMatch(value, @"^[a-zA-Z0-9._]+$");
    }

    /// <summary>
    /// Validates if a string is a valid protocol (http, https, grpc, grpcs).
    /// </summary>
    public static bool IsValidProtocol(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        var validProtocols = new[] { "http", "https", "grpc", "grpcs" };
        return validProtocols.Contains(value.ToLowerInvariant());
    }

    /// <summary>
    /// Validates path to ensure it doesn't contain suspicious patterns.
    /// </summary>
    public static bool IsValidPath(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        // Reject paths with directory traversal attempts
        return !value.Contains("..") && !value.Contains("~");
    }

    /// <summary>
    /// Validates a timeout value in milliseconds.
    /// </summary>
    public static bool IsValidTimeout(int timeoutMs)
    {
        return timeoutMs > 0 && timeoutMs <= 300000; // Max 5 minutes
    }
}
