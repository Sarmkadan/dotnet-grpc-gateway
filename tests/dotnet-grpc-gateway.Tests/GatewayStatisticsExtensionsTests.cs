using System;
using System.Collections.Generic;
using DotNetGrpcGateway.Domain;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class GatewayStatisticsExtensionsTests
{
    [Fact]
    public void GetTotalErrors_HappyPath_ReturnsSumOfErrors()
    {
        // Arrange
        var stats = new GatewayStatistics
        {
            ErrorsByType = new Dictionary<string, int>
            {
                { "Timeout", 5 },
                { "Connection", 3 },
                { "Validation", 2 }
            }
        };

        // Act
        var result = stats.GetTotalErrors();

        // Assert
        Assert.Equal(10, result);
    }

    [Fact]
    public void GetTotalErrors_EmptyErrors_ReturnsZero()
    {
        // Arrange
        var stats = new GatewayStatistics { ErrorsByType = new Dictionary<string, int>() };

        // Act
        var result = stats.GetTotalErrors();
        // Assert
        Assert.Equal(0, result);
    }


    [Fact]
    public void GetTopServicesByRequestCount_HappyPath_ReturnsTopNOrderedByCount()
    {
        // Arrange
        var stats = new GatewayStatistics
        {
            RequestsByService = new Dictionary<string, long>
            {
                { "UserService", 100 }, { "OrderService", 250 },
                { "ProductService", 75 }, { "PaymentService", 300 },
                { "NotificationService", 50 }
            }
        };

        // Act
        var result = stats.GetTopServicesByRequestCount(3);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("PaymentService", result[0].Key);
        Assert.Equal(300, result[0].Value);
        Assert.Equal("OrderService", result[1].Key);
        Assert.Equal(250, result[1].Value);
        Assert.Equal("UserService", result[2].Key);
        Assert.Equal(100, result[2].Value);
    }

    [Fact]
    public void GetTopServicesByRequestCount_DefaultTopN_ReturnsTop5()
    {
        // Arrange
        var stats = new GatewayStatistics
        {
            RequestsByService = new Dictionary<string, long>
            {
                { "ServiceA", 10 }, { "ServiceB", 20 }, { "ServiceC", 30 },
                { "ServiceD", 40 }, { "ServiceE", 50 }, { "ServiceF", 60 },
                { "ServiceG", 70 }
            }
        };

        // Act
        var result = stats.GetTopServicesByRequestCount();

        // Assert
        Assert.Equal(5, result.Count);
        Assert.Equal("ServiceG", result[0].Key);
        Assert.Equal("ServiceF", result[1].Key);
    }

    [Fact]
    public void GetTopServicesByRequestCount_EmptyServices_ReturnsEmptyList()
    {
        // Arrange
        var stats = new GatewayStatistics();

        // Act
        var result = stats.GetTopServicesByRequestCount();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetTopServicesByRequestCount_NullStatistics_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((GatewayStatistics)null!).GetTopServicesByRequestCount());
    }

    [Fact]
    public void GetTopServicesByRequestCount_NegativeTopN_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var stats = new GatewayStatistics();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => stats.GetTopServicesByRequestCount(-1));
    }

    [Theory]
    [InlineData(0, "0 B")]
    [InlineData(1, "1 B")]
    [InlineData(512, "512 B")]
    [InlineData(1023, "1023 B")]
    [InlineData(1024, "1.0 KB")]
    [InlineData(1536, "1.5 KB")]
    [InlineData(1024 * 1024, "1.0 MB")]
    [InlineData(1572864, "1.5 MB")]
    [InlineData(1024L * 1024 * 1024, "1.0 GB")]
    [InlineData(1610612736L, "1.5 GB")]
    public void GetFormattedDataProcessed_HappyPath_ReturnsCorrectFormat(long bytes, string expected)
    {
        // Arrange
        var stats = new GatewayStatistics { TotalDataProcessedBytes = bytes };

        // Act
        var result = stats.GetFormattedDataProcessed();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetFormattedDataProcessed_NullStatistics_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((GatewayStatistics)null!).GetFormattedDataProcessed());
    }

    [Theory]
    [InlineData(0.9, 95, 100, true)]
    [InlineData(0.9, 85, 100, false)]
    public void IsGatewayHealthy_HappyPath_ThresholdComparison(double threshold, int healthy, int total, bool expected)
    {
        // Arrange
        var stats = new GatewayStatistics { HealthyServices = healthy, TotalServices = total };

        // Act
        var result = stats.IsGatewayHealthy(threshold);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsGatewayHealthy_DefaultThreshold_HealthyWhenAbove90Percent()
    {
        // Arrange
        var stats = new GatewayStatistics { HealthyServices = 91, TotalServices = 100 };

        // Act
        var result = stats.IsGatewayHealthy();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsGatewayHealthy_ZeroServices_ReturnsTrue()
    {
        // Arrange
        var stats = new GatewayStatistics { HealthyServices = 0, TotalServices = 0 };

        // Act
        var result = stats.IsGatewayHealthy();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsGatewayHealthy_NullStatistics_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((GatewayStatistics)null!).IsGatewayHealthy());
    }

    [Fact]
    public void IsGatewayHealthy_NegativeThreshold_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var stats = new GatewayStatistics();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => stats.IsGatewayHealthy(-0.1));
    }

    [Fact]
    public void IsGatewayHealthy_ThresholdAboveOne_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var stats = new GatewayStatistics();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => stats.IsGatewayHealthy(1.1));
    }
}