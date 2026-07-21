#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Tests for Anomaly Detection functionality in RequestMetricsAnalyzerService
// =============================================================================

using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetGrpcGateway.Tests;

/// <summary>
/// Tests for anomaly detection in RequestMetricsAnalyzerService
/// </summary>
public class AnomalyDetectionTests
{
    private readonly Mock<IMetricsCollectionService> _mockMetricsService;
    private readonly Mock<ILogger<RequestMetricsAnalyzerService>> _mockLogger;
    private readonly RequestMetricsAnalyzerService _analyzerService;

    public AnomalyDetectionTests()
    {
        _mockMetricsService = new Mock<IMetricsCollectionService>();
        _mockLogger = new Mock<ILogger<RequestMetricsAnalyzerService>>();
        _analyzerService = new RequestMetricsAnalyzerService(_mockMetricsService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task DetectAnomaliesAsync_NoAnomalies_ShouldReturnEmptyList()
    {
        // Arrange: Normal traffic with high success rate and low latency
        var normalStats = new GatewayStatistics
        {
            TotalRequestsProcessed = 1000,
            SuccessfulRequests = 995,
            FailedRequests = 5,
            AverageResponseTimeMs = 500, // Below 1000ms threshold
            SuccessRate = 0.995 // Above 0.95 threshold
        };

        var slowRequests = new List<RequestMetric>(); // No slow requests

        _mockMetricsService
            .Setup(s => s.GetTodayStatisticsAsync())
            .ReturnsAsync(normalStats);
        _mockMetricsService
            .Setup(s => s.GetSlowRequestsAsync(0))
            .ReturnsAsync(slowRequests);

        // Act
        var alerts = await _analyzerService.DetectAnomaliesAsync();

        // Assert
        Assert.NotNull(alerts);
        Assert.Empty(alerts);
        _mockMetricsService.Verify(s => s.GetTodayStatisticsAsync(), Times.Once);
        _mockMetricsService.Verify(s => s.GetSlowRequestsAsync(0), Times.Once);
    }

    [Fact]
    public async Task DetectAnomaliesAsync_HighErrorRate_ShouldGenerateAlert()
    {
        // Arrange: High error rate (below 95% success threshold)
        // Note: The success rate is calculated from slowRequests, not GatewayStatistics
        var slowRequests = new List<RequestMetric>
        {
            new RequestMetric { HttpStatusCode = 200 }, // Success
            new RequestMetric { HttpStatusCode = 200 }, // Success
            new RequestMetric { HttpStatusCode = 200 }, // Success
            new RequestMetric { HttpStatusCode = 500 }, // Failure
            new RequestMetric { HttpStatusCode = 500 }  // Failure
        };

        var stats = new GatewayStatistics
        {
            TotalRequestsProcessed = 1000,
            SuccessfulRequests = 900,
            FailedRequests = 100,
            AverageResponseTimeMs = 500,
            SuccessRate = 0.90 // Below 0.95 threshold
        };

        _mockMetricsService
            .Setup(s => s.GetTodayStatisticsAsync())
            .ReturnsAsync(stats);
        _mockMetricsService
            .Setup(s => s.GetSlowRequestsAsync(0))
            .ReturnsAsync(slowRequests);

        // Act
        var alerts = await _analyzerService.DetectAnomaliesAsync();

        // Assert
        Assert.NotNull(alerts);
        Assert.Single(alerts);
        var alert = alerts[0];
        Assert.Equal("HighErrorRate", alert.AlertType);
        Assert.Equal("Error rate is 40.0% (threshold: 5%)", alert.Message);
        Assert.Equal(DateTime.UtcNow.Date, alert.DetectedAt.Date);
        Assert.True(alert.Severity >= 2); // Should be at least severity 2
        Assert.Equal(4, alert.Severity); // Since error rate is 40% which is < 90%
    }

    [Fact]
    public async Task DetectAnomaliesAsync_CriticalErrorRate_ShouldGenerateHighSeverityAlert()
    {
        // Arrange: Critical error rate (below 90% success threshold)
        var slowRequests = new List<RequestMetric>
        {
            new RequestMetric { HttpStatusCode = 200 }, // Success
            new RequestMetric { HttpStatusCode = 500 }, // Failure
            new RequestMetric { HttpStatusCode = 500 }, // Failure
            new RequestMetric { HttpStatusCode = 500 }, // Failure
            new RequestMetric { HttpStatusCode = 500 }  // Failure
        };

        var stats = new GatewayStatistics
        {
            TotalRequestsProcessed = 1000,
            SuccessfulRequests = 850,
            FailedRequests = 150,
            AverageResponseTimeMs = 500,
            SuccessRate = 0.85 // Below 0.90 threshold
        };

        _mockMetricsService
            .Setup(s => s.GetTodayStatisticsAsync())
            .ReturnsAsync(stats);
        _mockMetricsService
            .Setup(s => s.GetSlowRequestsAsync(0))
            .ReturnsAsync(slowRequests);

        // Act
        var alerts = await _analyzerService.DetectAnomaliesAsync();

        // Assert
        Assert.NotNull(alerts);
        Assert.Single(alerts);
        var alert = alerts[0];
        Assert.Equal("HighErrorRate", alert.AlertType);
        Assert.Equal(4, alert.Severity); // High severity for critical errors
    }

    [Fact]
    public async Task DetectAnomaliesAsync_HighLatency_ShouldGenerateAlert()
    {
        // Arrange: High latency (above 1000ms threshold)
        // Note: AverageResponseTimeMs comes from GatewayStatistics, not from slowRequests
        var slowRequests = new List<RequestMetric>();

        var stats = new GatewayStatistics
        {
            TotalRequestsProcessed = 1000,
            SuccessfulRequests = 995,
            FailedRequests = 5,
            AverageResponseTimeMs = 1500, // Above 1000ms threshold
            SuccessRate = 0.995
        };

        _mockMetricsService
            .Setup(s => s.GetTodayStatisticsAsync())
            .ReturnsAsync(stats);
        _mockMetricsService
            .Setup(s => s.GetSlowRequestsAsync(0))
            .ReturnsAsync(slowRequests);

        // Act
        var alerts = await _analyzerService.DetectAnomaliesAsync();

        // Assert
        Assert.NotNull(alerts);
        Assert.Single(alerts);
        var alert = alerts[0];
        Assert.Equal("HighLatency", alert.AlertType);
        Assert.Equal("Average response time is 1500ms (threshold: 1000ms)", alert.Message);
        Assert.Equal(DateTime.UtcNow.Date, alert.DetectedAt.Date);
        Assert.True(alert.Severity >= 3); // Should be at least severity 3
        Assert.Equal(3, alert.Severity); // Since latency is 1500ms which is > 1000ms but < 5000ms
    }

    [Fact]
    public async Task DetectAnomaliesAsync_ExtremeLatency_ShouldGenerateCriticalSeverityAlert()
    {
        // Arrange: Extreme latency (above 5000ms threshold)
        var slowRequests = new List<RequestMetric>();

        var stats = new GatewayStatistics
        {
            TotalRequestsProcessed = 1000,
            SuccessfulRequests = 995,
            FailedRequests = 5,
            AverageResponseTimeMs = 6000, // Above 5000ms threshold
            SuccessRate = 0.995
        };

        _mockMetricsService
            .Setup(s => s.GetTodayStatisticsAsync())
            .ReturnsAsync(stats);
        _mockMetricsService
            .Setup(s => s.GetSlowRequestsAsync(0))
            .ReturnsAsync(slowRequests);

        // Act
        var alerts = await _analyzerService.DetectAnomaliesAsync();

        // Assert
        Assert.NotNull(alerts);
        Assert.Single(alerts);
        var alert = alerts[0];
        Assert.Equal("HighLatency", alert.AlertType);
        Assert.Equal(5, alert.Severity); // Critical severity for extreme latency
    }

    [Fact]
    public async Task DetectAnomaliesAsync_BothHighLatencyAndErrorRate_ShouldGenerateMultipleAlerts()
    {
        // Arrange: Both high latency and high error rate
        var slowRequests = new List<RequestMetric>
        {
            new RequestMetric { HttpStatusCode = 200 }, // Success
            new RequestMetric { HttpStatusCode = 500 }, // Failure
            new RequestMetric { HttpStatusCode = 500 }  // Failure
        };

        var stats = new GatewayStatistics
        {
            TotalRequestsProcessed = 1000,
            SuccessfulRequests = 880,
            FailedRequests = 120,
            AverageResponseTimeMs = 1800,
            SuccessRate = 0.88
        };

        _mockMetricsService
            .Setup(s => s.GetTodayStatisticsAsync())
            .ReturnsAsync(stats);
        _mockMetricsService
            .Setup(s => s.GetSlowRequestsAsync(0))
            .ReturnsAsync(slowRequests);

        // Act
        var alerts = await _analyzerService.DetectAnomaliesAsync();

        // Assert
        Assert.NotNull(alerts);
        Assert.Equal(2, alerts.Count);

        // Verify both alert types are present
        var errorAlerts = alerts.Where(a => a.AlertType == "HighErrorRate").ToList();
        var latencyAlerts = alerts.Where(a => a.AlertType == "HighLatency").ToList();

        Assert.Single(errorAlerts);
        Assert.Single(latencyAlerts);
    }

    [Fact]
    public async Task DetectAnomaliesAsync_WithEndpointData_ShouldIncludeEndpointInAnalysis()
    {
        // Arrange: Normal traffic but with slow requests to test that analysis works correctly
        var stats = new GatewayStatistics
        {
            TotalRequestsProcessed = 1000,
            SuccessfulRequests = 995,
            FailedRequests = 5,
            AverageResponseTimeMs = 500,
            SuccessRate = 0.995
        };

        // Create slow requests with specific endpoints
        var slowRequests = new List<RequestMetric>
        {
            new RequestMetric
            {
                ServiceName = "TestService",
                MethodName = "GetData",
                DurationMs = 1200, // Slow request
                HttpStatusCode = 200,
                IsSuccessful = true
            },
            new RequestMetric
            {
                ServiceName = "TestService",
                MethodName = "GetData",
                DurationMs = 1100, // Slow request
                HttpStatusCode = 200,
                IsSuccessful = true
            },
            new RequestMetric
            {
                ServiceName = "AnotherService",
                MethodName = "PostData",
                DurationMs = 800, // Not slow
                HttpStatusCode = 200,
                IsSuccessful = true
            }
        };

        _mockMetricsService
            .Setup(s => s.GetTodayStatisticsAsync())
            .ReturnsAsync(stats);
        _mockMetricsService
            .Setup(s => s.GetSlowRequestsAsync(0))
            .ReturnsAsync(slowRequests);

        // Act
        var alerts = await _analyzerService.DetectAnomaliesAsync();

        // Assert - should have normal analysis (no alerts since stats are normal)
        Assert.NotNull(alerts);
        // With normal stats, there should be no alerts
        Assert.Empty(alerts);
    }

    [Fact]
    public async Task DetectAnomaliesAsync_EmptyMetrics_ShouldNotThrow()
    {
        // Arrange: Empty statistics
        var stats = new GatewayStatistics
        {
            TotalRequestsProcessed = 0,
            SuccessfulRequests = 0,
            FailedRequests = 0,
            AverageResponseTimeMs = 0,
            SuccessRate = 1.0
        };

        var slowRequests = new List<RequestMetric>();

        _mockMetricsService
            .Setup(s => s.GetTodayStatisticsAsync())
            .ReturnsAsync(stats);
        _mockMetricsService
            .Setup(s => s.GetSlowRequestsAsync(0))
            .ReturnsAsync(slowRequests);

        // Act & Assert - should not throw
        var alerts = await _analyzerService.DetectAnomaliesAsync();

        Assert.NotNull(alerts);
        Assert.Empty(alerts);
    }

    [Fact]
    public async Task DetectAnomaliesAsync_ServiceException_ShouldReturnHighErrorRateAlert()
    {
        // Arrange: Service throws exception, which causes AnalyzeRequestPatternsAsync to return default analysis
        // with SuccessRate = 0, triggering HighErrorRate alert
        _mockMetricsService
            .Setup(s => s.GetTodayStatisticsAsync())
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var alerts = await _analyzerService.DetectAnomaliesAsync();

        // Assert
        Assert.NotNull(alerts);
        Assert.Single(alerts);
        var alert = alerts[0];
        Assert.Equal("HighErrorRate", alert.AlertType);
        Assert.Equal("Error rate is 100.0% (threshold: 5%)", alert.Message);
    }

    [Fact]
    public async Task DetectAnomaliesAsync_AnomalyAlertProperties_ShouldBeCorrectlySet()
    {
        // Arrange: High latency scenario
        var stats = new GatewayStatistics
        {
            TotalRequestsProcessed = 5000,
            SuccessfulRequests = 4950,
            FailedRequests = 50,
            AverageResponseTimeMs = 2500,
            SuccessRate = 0.99
        };

        var slowRequests = new List<RequestMetric>();

        _mockMetricsService
            .Setup(s => s.GetTodayStatisticsAsync())
            .ReturnsAsync(stats);
        _mockMetricsService
            .Setup(s => s.GetSlowRequestsAsync(0))
            .ReturnsAsync(slowRequests);

        // Act
        var alerts = await _analyzerService.DetectAnomaliesAsync();
        var alert = alerts.FirstOrDefault(a => a.AlertType == "HighLatency");

        // Assert all properties are set correctly
        Assert.NotNull(alert);
        Assert.Equal("HighLatency", alert.AlertType);
        Assert.StartsWith("Average response time is", alert.Message);
        Assert.NotEqual(default, alert.DetectedAt);
        Assert.InRange(alert.Severity, 1, 5);
    }

    [Fact]
    public async Task DetectAnomaliesAsync_LowErrorRate_ShouldNotGenerateAlert()
    {
        // Arrange: Low error rate (above 95% threshold)
        var stats = new GatewayStatistics
        {
            TotalRequestsProcessed = 1000,
            SuccessfulRequests = 996,
            FailedRequests = 4,
            AverageResponseTimeMs = 500,
            SuccessRate = 0.996 // Above 0.95 threshold
        };

        var slowRequests = new List<RequestMetric>();

        _mockMetricsService
            .Setup(s => s.GetTodayStatisticsAsync())
            .ReturnsAsync(stats);
        _mockMetricsService
            .Setup(s => s.GetSlowRequestsAsync(0))
            .ReturnsAsync(slowRequests);

        // Act
        var alerts = await _analyzerService.DetectAnomaliesAsync();

        // Assert
        Assert.NotNull(alerts);
        Assert.Empty(alerts);
    }

    [Fact]
    public async Task DetectAnomaliesAsync_NormalLatency_ShouldNotGenerateAlert()
    {
        // Arrange: Normal latency (below 1000ms threshold)
        var stats = new GatewayStatistics
        {
            TotalRequestsProcessed = 1000,
            SuccessfulRequests = 995,
            FailedRequests = 5,
            AverageResponseTimeMs = 800, // Below 1000ms threshold
            SuccessRate = 0.995
        };

        var slowRequests = new List<RequestMetric>();

        _mockMetricsService
            .Setup(s => s.GetTodayStatisticsAsync())
            .ReturnsAsync(stats);
        _mockMetricsService
            .Setup(s => s.GetSlowRequestsAsync(0))
            .ReturnsAsync(slowRequests);

        // Act
        var alerts = await _analyzerService.DetectAnomaliesAsync();

        // Assert
        Assert.NotNull(alerts);
        Assert.Empty(alerts);
    }
}
