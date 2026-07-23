using System;
using System.Collections.Generic;
using DotNetGrpcGateway.Domain;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class GatewayStatisticsExtensionsTests
{
    [Fact]
    public void GetTotalErrors_HappyPath_ReturnsSum()
    {
        var stats = new GatewayStatistics
        {
            ErrorsByType = new Dictionary<string, int>
            {
                { "Timeout", 4 },
                { "Validation", 2 }
            }
        };

        var result = stats.GetTotalErrors();

        Assert.Equal(6, result);
    }

    [Fact]
    public void GetTopServicesByRequestCount_HappyPath_ReturnsTopNOrdered()
    {
        var stats = new GatewayStatistics
        {
            RequestsByService = new Dictionary<string, long>
            {
                { "A", 10 }, { "B", 30 }, { "C", 20 }, { "D", 40 }
            }
        };

        var top = stats.GetTopServicesByRequestCount(3);

        Assert.Equal(3, top.Count);
        Assert.Equal("D", top[0].Key);
        Assert.Equal(40, top[0].Value);
        Assert.Equal("B", top[1].Key);
        Assert.Equal(30, top[1].Value);
        Assert.Equal("C", top[2].Key);
        Assert.Equal(20, top[2].Value);
    }

    [Theory]
    [InlineData(0, "0 B")]
    [InlineData(512, "512 B")]
    [InlineData(1024, "1.0 KB")]
    [InlineData(1536, "1.5 KB")]
    [InlineData(1024 * 1024, "1.0 MB")]
    [InlineData(1572864, "1.5 MB")]
    [InlineData(1024L * 1024 * 1024, "1.0 GB")]
    [InlineData(1610612736L, "1.5 GB")]
    public void GetFormattedDataProcessed_HappyPath_ReturnsCorrectFormat(long bytes, string expected)
    {
        var stats = new GatewayStatistics { TotalDataProcessedBytes = bytes };
        var result = stats.GetFormattedDataProcessed();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0.9, 90, 100, true)]
    [InlineData(0.9, 85, 100, false)]
    [InlineData(0.5, 5, 10, true)]
    public void IsGatewayHealthy_HappyPath_ThresholdComparison(double threshold, int healthy, int total, bool expected)
    {
        var stats = new GatewayStatistics { HealthyServices = healthy, TotalServices = total };
        var result = stats.IsGatewayHealthy(threshold);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetTopServicesByRequestCount_NullStatistics_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ((GatewayStatistics)null!).GetTopServicesByRequestCount());
    }

    [Fact]
    public void GetFormattedDataProcessed_NullStatistics_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ((GatewayStatistics)null!).GetFormattedDataProcessed());
    }

    [Fact]
    public void IsGatewayHealthy_NullStatistics_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ((GatewayStatistics)null!).IsGatewayHealthy());
    }

    [Fact]
    public void GetTopServicesByRequestCount_NegativeTopN_ThrowsArgumentOutOfRangeException()
    {
        var stats = new GatewayStatistics();
        Assert.Throws<ArgumentOutOfRangeException>(() => stats.GetTopServicesByRequestCount(-1));
    }
}
