#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using DotNetGrpcGateway.Domain;

namespace DotNetGrpcGateway.Tests;

/// <summary>
/// Extension methods for <see cref="GatewayStatisticsTests"/> to provide test data builders for <see cref="GatewayStatistics"/> instances.
/// These methods create realistic test data with various configurations for unit and integration testing scenarios.
/// </summary>
public static class GatewayStatisticsTestsExtensions
{
    /// <summary>
    /// Creates a <see cref="GatewayStatistics"/> instance with realistic production-like values for testing.
    /// </summary>
    /// <param name="totalRequests">Total requests processed. Must be non-negative.</param>
    /// <param name="successRate">Success rate percentage (0-100).</param>
    /// <param name="avgResponseTimeMs">Average response time in milliseconds. Must be non-negative.</param>
    /// <param name="dataProcessedBytes">Total data processed in bytes. Must be non-negative.</param>
    /// <returns>Configured <see cref="GatewayStatistics"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any parameter is negative or success rate is outside [0, 100] range.</exception>
    public static GatewayStatistics CreateTestStatistics(
        this GatewayStatisticsTests _,
        long totalRequests = 1000,
        double successRate = 95.5,
        double avgResponseTimeMs = 125.5,
        long dataProcessedBytes = 1024 * 1024)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(totalRequests);
        ArgumentOutOfRangeException.ThrowIfNegative(avgResponseTimeMs);
        ArgumentOutOfRangeException.ThrowIfNegative(dataProcessedBytes);
        ArgumentOutOfRangeException.ThrowIfLessThan(successRate, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(successRate, 100);

        var stats = new GatewayStatistics
        {
            TotalRequestsProcessed = totalRequests,
            SuccessfulRequests = (long)(totalRequests * successRate / 100),
            FailedRequests = (long)(totalRequests * (100 - successRate) / 100),
            SuccessRate = successRate,
            AverageResponseTimeMs = avgResponseTimeMs,
            MinResponseTimeMs = avgResponseTimeMs * 0.5,
            MaxResponseTimeMs = avgResponseTimeMs * 2,
            TotalDataProcessedBytes = dataProcessedBytes,
            ActiveConnections = 15,
            PeakConnections = 30,
            StatisticsDate = DateTime.UtcNow.Date.AddDays(-1)
        };

        // Add some service and method tracking
        stats.RecordServiceRequest("UserService");
        stats.RecordServiceRequest("UserService");
        stats.RecordServiceRequest("OrderService");

        stats.RecordMethodCall("GetUser");
        stats.RecordMethodCall("GetUser");
        stats.RecordMethodCall("CreateOrder");

        stats.RecordError("TimeoutError");
        stats.RecordError("ConnectionError");

        stats.RecordCacheHit(isHit: true);
        stats.RecordCacheHit(isHit: true);
        stats.RecordCacheHit(isHit: false);

        stats.UpdateServiceHealth(healthyCount: 8, unhealthyCount: 2);

        return stats;
    }

    /// <summary>
    /// Creates a <see cref="GatewayStatistics"/> instance with minimal default values.
    /// </summary>
    /// <param name="date">Statistics date (defaults to today).</param>
    /// <returns><see cref="GatewayStatistics"/> instance with default values.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="date"/> is in the future.</exception>
    public static GatewayStatistics CreateEmptyStatistics(
        this GatewayStatisticsTests _,
        DateTime? date = null)
    {
        if (date.HasValue && date.Value > DateTime.UtcNow.Date)
        {
            throw new ArgumentOutOfRangeException(nameof(date), "Statistics date cannot be in the future.");
        }

        return new GatewayStatistics
        {
            StatisticsDate = date ?? DateTime.UtcNow.Date
        };
    }

    /// <summary>
    /// Creates a <see cref="GatewayStatistics"/> instance with error conditions for negative testing.
    /// </summary>
    /// <param name="negativeTotalRequests">Whether to set negative total requests.</param>
    /// <param name="negativeSuccessRate">Whether to set success rate below zero.</param>
    /// <param name="invalidMinMax">Whether to set min > max response times.</param>
    /// <returns><see cref="GatewayStatistics"/> instance configured for error scenarios.</returns>
    public static GatewayStatistics CreateErrorStatistics(
        this GatewayStatisticsTests _,
        bool negativeTotalRequests = false,
        bool negativeSuccessRate = false,
        bool invalidMinMax = false)
    {
        var stats = new GatewayStatistics
        {
            TotalRequestsProcessed = negativeTotalRequests ? -100 : 500,
            SuccessfulRequests = 450,
            FailedRequests = 50,
            SuccessRate = negativeSuccessRate ? -5.0 : 90.0,
            AverageResponseTimeMs = 100.0,
            MinResponseTimeMs = invalidMinMax ? 200.0 : 50.0,
            MaxResponseTimeMs = invalidMinMax ? 100.0 : 150.0,
            TotalDataProcessedBytes = 512 * 1024,
            StatisticsDate = DateTime.UtcNow.Date
        };

        return stats;
    }

    /// <summary>
    /// Creates a <see cref="GatewayStatistics"/> instance with zero values for edge case testing.
    /// </summary>
    /// <param name="zeroRequests">Whether to set all request counters to zero.</param>
    /// <returns><see cref="GatewayStatistics"/> instance with zero values.</returns>
    public static GatewayStatistics CreateZeroStatistics(
        this GatewayStatisticsTests _,
        bool zeroRequests = true)
    {
        var stats = new GatewayStatistics
        {
            StatisticsDate = DateTime.UtcNow.Date,
            TotalRequestsProcessed = zeroRequests ? 0 : 100,
            SuccessfulRequests = 0,
            FailedRequests = 0,
            SuccessRate = 0.0,
            AverageResponseTimeMs = 0.0,
            MinResponseTimeMs = 0.0,
            MaxResponseTimeMs = 0.0,
            TotalDataProcessedBytes = 0,
            ActiveConnections = 0,
            PeakConnections = 0,
            CacheHitRate = 0.0,
            CacheHits = 0,
            CacheMisses = 0,
            HealthyServices = 0,
            UnhealthyServices = 0,
            TotalServices = 0
        };

        return stats;
    }
}