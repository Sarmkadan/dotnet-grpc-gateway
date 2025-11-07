// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Utilities;

/// <summary>
/// DateTime utilities for common time-related operations.
/// Handles UTC normalization, time ranges, and human-readable formatting.
/// </summary>
public static class DateTimeUtility
{
    /// <summary>
    /// Gets the start of the current UTC day (00:00:00).
    /// </summary>
    public static DateTime GetTodayStartUtc()
    {
        var now = DateTime.UtcNow;
        return new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
    }

    /// <summary>
    /// Gets the end of the current UTC day (23:59:59).
    /// </summary>
    public static DateTime GetTodayEndUtc()
    {
        var now = DateTime.UtcNow;
        return new DateTime(now.Year, now.Month, now.Day, 23, 59, 59, DateTimeKind.Utc);
    }

    /// <summary>
    /// Gets the start of a specific UTC date.
    /// </summary>
    public static DateTime GetDayStartUtc(DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
    }

    /// <summary>
    /// Gets the end of a specific UTC date.
    /// </summary>
    public static DateTime GetDayEndUtc(DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, DateTimeKind.Utc);
    }

    /// <summary>
    /// Gets the start of the current UTC week (Monday 00:00:00).
    /// </summary>
    public static DateTime GetWeekStartUtc()
    {
        var now = DateTime.UtcNow;
        var start = now.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
        return new DateTime(start.Year, start.Month, start.Day, 0, 0, 0, DateTimeKind.Utc);
    }

    /// <summary>
    /// Gets the start of the current UTC month (1st day 00:00:00).
    /// </summary>
    public static DateTime GetMonthStartUtc()
    {
        var now = DateTime.UtcNow;
        return new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    /// <summary>
    /// Gets the end of the current UTC month (last day 23:59:59).
    /// </summary>
    public static DateTime GetMonthEndUtc()
    {
        var now = DateTime.UtcNow;
        var lastDay = DateTime.DaysInMonth(now.Year, now.Month);
        return new DateTime(now.Year, now.Month, lastDay, 23, 59, 59, DateTimeKind.Utc);
    }

    /// <summary>
    /// Converts milliseconds to a human-readable duration string (e.g., "1h 30m 45s").
    /// </summary>
    public static string MillisecondsToHumanReadable(long milliseconds)
    {
        var ts = TimeSpan.FromMilliseconds(milliseconds);

        if (ts.TotalHours >= 1)
            return $"{(int)ts.TotalHours}h {ts.Minutes}m {ts.Seconds}s";

        if (ts.TotalMinutes >= 1)
            return $"{(int)ts.TotalMinutes}m {ts.Seconds}s";

        if (ts.TotalSeconds >= 1)
            return $"{ts.Seconds}s";

        return $"{milliseconds}ms";
    }

    /// <summary>
    /// Checks if a given date is today (UTC).
    /// </summary>
    public static bool IsToday(DateTime date)
    {
        var today = DateTime.UtcNow.Date;
        return date.Date == today;
    }

    /// <summary>
    /// Gets the number of days elapsed between two dates.
    /// </summary>
    public static int DaysDifference(DateTime from, DateTime to)
    {
        return (to.Date - from.Date).Days;
    }

    /// <summary>
    /// Checks if two datetimes are within the same UTC day.
    /// </summary>
    public static bool IsSameDay(DateTime dateTime1, DateTime dateTime2)
    {
        return dateTime1.Date == dateTime2.Date;
    }

    /// <summary>
    /// Rounds a datetime down to the nearest minute.
    /// </summary>
    public static DateTime RoundDownToMinute(DateTime dateTime)
    {
        return dateTime.AddTicks(-(dateTime.Ticks % TimeSpan.TicksPerMinute));
    }

    /// <summary>
    /// Rounds a datetime down to the nearest hour.
    /// </summary>
    public static DateTime RoundDownToHour(DateTime dateTime)
    {
        return dateTime.AddTicks(-(dateTime.Ticks % TimeSpan.TicksPerHour));
    }

    /// <summary>
    /// Checks if a time is within business hours (9 AM - 5 PM UTC).
    /// </summary>
    public static bool IsBusinessHours(DateTime dateTime)
    {
        var hour = dateTime.Hour;
        return hour >= 9 && hour < 17;
    }
}
