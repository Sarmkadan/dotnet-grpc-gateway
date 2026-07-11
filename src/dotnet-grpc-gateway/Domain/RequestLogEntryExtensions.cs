#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetGrpcGateway.Domain;

/// <summary>
/// Extension methods for <see cref="RequestLogEntry"/> that provide common operations and conveniences.
/// </summary>
public static class RequestLogEntryExtensions
{
    /// <summary>
    /// Determines whether the request was a cache hit.
    /// </summary>
    /// <param name="entry">The log entry.</param>
    /// <returns>True if the request was served from cache; otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is <see langword="null"/>.</exception>
    public static bool WasCacheHit(this RequestLogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return entry.CacheHit;
    }

    /// <summary>
    /// Determines whether the request was successful (HTTP status code &lt; 400).
    /// </summary>
    /// <param name="entry">The log entry.</param>
    /// <returns>True if the request was successful; otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is <see langword="null"/>.</exception>
    public static bool WasSuccessful(this RequestLogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return entry.IsSuccessful;
    }

    /// <summary>
    /// Gets the duration of the request in seconds.
    /// </summary>
    /// <param name="entry">The log entry.</param>
    /// <returns>The duration in seconds.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is <see langword="null"/>.</exception>
    public static double DurationSeconds(this RequestLogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return entry.DurationMs / 1000.0;
    }

    /// <summary>
    /// Gets the total size of the request and response in bytes.
    /// </summary>
    /// <param name="entry">The log entry.</param>
    /// <returns>The combined size in bytes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is <see langword="null"/>.</exception>
    public static long TotalSizeBytes(this RequestLogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return entry.RequestSizeBytes + entry.ResponseSizeBytes;
    }

    /// <summary>
    /// Gets a formatted duration string for the request.
    /// </summary>
    /// <param name="entry">The log entry.</param>
    /// <returns>A formatted duration string (e.g., "125ms", "2.3s").</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is <see langword="null"/>.</exception>
    public static string FormattedDuration(this RequestLogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        return entry.DurationMs switch
        {
            < 1000 => $"{entry.DurationMs}ms",
            _ => $"{entry.DurationSeconds():0.##}s"
        };
    }

    /// <summary>
    /// Gets a formatted size string for the request and response.
    /// </summary>
    /// <param name="entry">The log entry.</param>
    /// <returns>A formatted size string (e.g., "1.2KB", "5MB").</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is <see langword="null"/>.</exception>
    public static string FormattedSize(this RequestLogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        var totalBytes = entry.TotalSizeBytes();
        return FormatBytes(totalBytes);
    }

    /// <summary>
    /// Gets the HTTP status code category (1xx, 2xx, 3xx, 4xx, 5xx).
    /// </summary>
    /// <param name="entry">The log entry.</param>
    /// <returns>The status code category.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is <see langword="null"/>.</exception>
    public static string StatusCodeCategory(this RequestLogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        var category = entry.HttpStatusCode / 100;
        return $"{category}xx";
    }

    /// <summary>
    /// Gets whether the status code indicates a client error (4xx).
    /// </summary>
    /// <param name="entry">The log entry.</param>
    /// <returns>True if the status code is in the 4xx range; otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is <see langword="null"/>.</exception>
    public static bool IsClientError(this RequestLogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return entry.HttpStatusCode is >= 400 and < 500;
    }

    /// <summary>
    /// Gets whether the status code indicates a server error (5xx).
    /// </summary>
    /// <param name="entry">The log entry.</param>
    /// <returns>True if the status code is in the 5xx range; otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is <see langword="null"/>.</exception>
    public static bool IsServerError(this RequestLogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return entry.HttpStatusCode >= 500;
    }

    /// <summary>
    /// Gets a summary of the request for display purposes.
    /// </summary>
    /// <param name="entry">The log entry.</param>
    /// <returns>A formatted summary string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is <see langword="null"/>.</exception>
    public static string GetSummary(this RequestLogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var status = entry.WasSuccessful() ? "✓" : "✗";
        var duration = entry.FormattedDuration();
        var size = entry.FormattedSize();

        return $"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss}] {status} {entry.Method} {entry.Path} - {entry.HttpStatusCode} ({entry.StatusCodeCategory()}) - {duration} - {size}";
    }

    /// <summary>
    /// Gets the upstream address if available, otherwise returns the client IP.
    /// </summary>
    /// <param name="entry">The log entry.</param>
    /// <returns>The upstream address or client IP.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is <see langword="null"/>.</exception>
    public static string GetTargetAddress(this RequestLogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return entry.UpstreamAddress ?? entry.ClientIp ?? "unknown";
    }

    /// <summary>
    /// Gets the error details if the request failed.
    /// </summary>
    /// <param name="entry">The log entry.</param>
    /// <returns>The error message if available; otherwise null.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is <see langword="null"/>.</exception>
    public static string? GetErrorDetails(this RequestLogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return entry.IsSuccessful ? null : entry.ErrorMessage ?? "Unknown error";
    }

    /// <summary>
    /// Formats a byte count into a human-readable string with appropriate units.
    /// </summary>
    /// <param name="bytes">The number of bytes to format.</param>
    /// <returns>A formatted string (e.g., "1.2KB", "5MB").</returns>
    private static string FormatBytes(long bytes)
    {
        if (bytes < 0)
        {
            return "0B";
        }

        string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
        int counter = 0;
        double value = bytes;

        while (value >= 1024 && counter < suffixes.Length - 1)
        {
            value /= 1024;
            counter++;
        }

        return $"{value:0.##} {suffixes[counter]}";
    }
}