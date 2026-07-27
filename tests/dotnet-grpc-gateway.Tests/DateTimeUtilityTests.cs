#nullable enable
using System;
using System.Linq;
using Xunit;
using DotNetGrpcGateway.Utilities;

namespace DotNetGrpcGateway.Tests;

public class DateTimeUtilityTests
{
    [Fact]
    public void GetTodayStartUtc_ReturnsMidnightUtcOfCurrentDay()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var start = DateTimeUtility.GetTodayStartUtc();

        // Assert
        Assert.Equal(DateTimeKind.Utc, start.Kind);
        Assert.Equal(0, start.Hour);
        Assert.Equal(0, start.Minute);
        Assert.Equal(0, start.Second);
        Assert.Equal(now.Year, start.Year);
        Assert.Equal(now.Month, start.Month);
        Assert.Equal(now.Day, start.Day);
    }

    [Fact]
    public void GetTodayEndUtc_ReturnsLastSecondUtcOfCurrentDay()
    {
        var now = DateTime.UtcNow;
        var end = DateTimeUtility.GetTodayEndUtc();

        Assert.Equal(DateTimeKind.Utc, end.Kind);
        Assert.Equal(23, end.Hour);
        Assert.Equal(59, end.Minute);
        Assert.Equal(59, end.Second);
        Assert.Equal(now.Year, end.Year);
        Assert.Equal(now.Month, end.Month);
        Assert.Equal(now.Day, end.Day);
    }

    [Fact]
    public void GetDayStartAndEndUtc_ReturnCorrectBoundaries()
    {
        var sample = new DateTime(2023, 5, 15, 13, 45, 30, DateTimeKind.Utc);

        var start = DateTimeUtility.GetDayStartUtc(sample);
        var end   = DateTimeUtility.GetDayEndUtc(sample);

        Assert.Equal(new DateTime(2023, 5, 15, 0, 0, 0, DateTimeKind.Utc), start);
        Assert.Equal(new DateTime(2023, 5, 15, 23, 59, 59, DateTimeKind.Utc), end);
    }

    [Fact]
    public void GetWeekStartUtc_AlignsWithImplementationLogic()
    {
        // The implementation uses:
        // var start = now.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
        var now = DateTime.UtcNow;
        var expected = now.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
        expected = new DateTime(expected.Year, expected.Month, expected.Day, 0, 0, 0, DateTimeKind.Utc);

        var actual = DateTimeUtility.GetWeekStartUtc();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetMonthStartAndEndUtc_ReturnCorrectValues()
    {
        var now = DateTime.UtcNow;
        var start = DateTimeUtility.GetMonthStartUtc();
        var end   = DateTimeUtility.GetMonthEndUtc();

        var expectedStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastDay = DateTime.DaysInMonth(now.Year, now.Month);
        var expectedEnd = new DateTime(now.Year, now.Month, lastDay, 23, 59, 59, DateTimeKind.Utc);

        Assert.Equal(expectedStart, start);
        Assert.Equal(expectedEnd,   end);
    }

    [Theory]
    [InlineData(500, "500ms")]
    [InlineData(1500, "1s")]
    [InlineData(90_000, "1m 30s")]
    [InlineData(3_600_000, "1h 0m 0s")]
    [InlineData(0, "0ms")]
    public void MillisecondsToHumanReadable_FormatsCorrectly(long ms, string expected)
    {
        var result = DateTimeUtility.MillisecondsToHumanReadable(ms);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsToday_And_DaysDifference_WorkAsExpected()
    {
        var today = DateTime.UtcNow;
        var yesterday = today.AddDays(-1);
        var tomorrow = today.AddDays(1);

        Assert.True(DateTimeUtility.IsToday(today));
        Assert.False(DateTimeUtility.IsToday(yesterday));

        Assert.Equal(0, DateTimeUtility.DaysDifference(today, today));
        Assert.Equal(1, DateTimeUtility.DaysDifference(yesterday, today));
        Assert.Equal(-1, DateTimeUtility.DaysDifference(tomorrow, today));
    }
}
