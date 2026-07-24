using System;
using DotNetGrpcGateway.Domain;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class ServiceHealthReportExtensionsTests
{
    [Fact]
    public void IsUnhealthyForLongTime_ReturnsTrue_WhenUnhealthyAndThresholdExceeded()
    {
        // Arrange
        var report = new ServiceHealthReport
        {
            IsHealthy = false,
            LastCheckAt = DateTime.UtcNow.AddHours(-5)
        };
        var threshold = TimeSpan.FromHours(4);

        // Act
        var result = report.IsUnhealthyForLongTime(threshold);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsUnhealthyForLongTime_ReturnsFalse_WhenHealthy()
    {
        // Arrange
        var report = new ServiceHealthReport
        {
            IsHealthy = true,
            LastCheckAt = DateTime.UtcNow.AddHours(-10)
        };
        var threshold = TimeSpan.FromHours(1);

        // Act
        var result = report.IsUnhealthyForLongTime(threshold);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsUnhealthyForLongTime_ReturnsFalse_WhenUnhealthyButBelowThreshold()
    {
        // Arrange
        var report = new ServiceHealthReport
        {
            IsHealthy = false,
            LastCheckAt = DateTime.UtcNow.AddMinutes(-30)
        };
        var threshold = TimeSpan.FromHours(1);

        // Act
        var result = report.IsUnhealthyForLongTime(threshold);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsUnhealthyForLongTime_ThrowsArgumentNullException_WhenReportIsNull()
    {
        // Arrange
        ServiceHealthReport? report = null;
        var threshold = TimeSpan.FromMinutes(5);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => report!.IsUnhealthyForLongTime(threshold));
    }

    [Fact]
    public void CalculateAverageResponseTime_ReturnsZero_WhenNoHealthChecks()
    {
        // Arrange
        var report = new ServiceHealthReport
        {
            TotalHealthChecks = 0,
            ResponseTimeMs = 0
        };

        // Act
        var avg = report.CalculateAverageResponseTime();

        // Assert
        Assert.Equal(0, avg);
    }

    [Fact]
    public void CalculateAverageResponseTime_ReturnsCorrectAverage()
    {
        // Arrange
        var report = new ServiceHealthReport
        {
            TotalHealthChecks = 4,
            ResponseTimeMs = 800 // total ms
        };

        // Act
        var avg = report.CalculateAverageResponseTime();

        // Assert
        Assert.Equal(200, avg);
    }

    [Fact]
    public void CalculateAverageResponseTime_ThrowsArgumentNullException_WhenReportIsNull()
    {
        // Arrange
        ServiceHealthReport? report = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => report!.CalculateAverageResponseTime());
    }

    [Fact]
    public void GetHealthStatusSummary_ReturnsHealthyString_WhenReportIsHealthy()
    {
        // Arrange
        var report = new ServiceHealthReport
        {
            IsHealthy = true,
            SuccessfulHealthChecks = 7
        };

        // Act
        var summary = report.GetHealthStatusSummary();

        // Assert
        Assert.Equal("Healthy (7 successful checks)", summary);
    }

    [Fact]
    public void GetHealthStatusSummary_ReturnsUnhealthyString_WhenReportIsUnhealthy()
    {
        // Arrange
        var report = new ServiceHealthReport
        {
            IsHealthy = false,
            FailedChecksInARow = 3
        };

        // Act
        var summary = report.GetHealthStatusSummary();

        // Assert
        Assert.Equal("Unhealthy (3 failed checks in a row)", summary);
    }

    [Fact]
    public void GetHealthStatusSummary_ThrowsArgumentNullException_WhenReportIsNull()
    {
        // Arrange
        ServiceHealthReport? report = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => report!.GetHealthStatusSummary());
    }
}
