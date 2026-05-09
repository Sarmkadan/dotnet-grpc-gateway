// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Domain;

namespace DotNetGrpcGateway.Services;

/// <summary>
/// Service for analyzing request metrics and generating insights.
/// Identifies patterns, trends, and potential issues in request data.
/// </summary>
public interface IRequestMetricsAnalyzerService
{
    Task<RequestPatternAnalysis> AnalyzeRequestPatternsAsync();
    Task<EndpointHealthScore> AnalyzeEndpointHealthAsync(string path);
    Task<List<AnomalyAlert>> DetectAnomaliesAsync();
}

/// <summary>
/// Request pattern analysis results.
/// </summary>
public class RequestPatternAnalysis
{
    public int TotalRequests { get; set; }
    public double AverageResponseTime { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public double SuccessRate { get; set; }
    public string? MostAccessedEndpoint { get; set; }
    public string? SlowestEndpoint { get; set; }
}

/// <summary>
/// Endpoint health score information.
/// </summary>
public class EndpointHealthScore
{
    public string? Endpoint { get; set; }
    public double HealthScore { get; set; } // 0-100
    public int RequestCount { get; set; }
    public double SuccessRate { get; set; }
    public double AverageResponseTime { get; set; }
    public string? Status { get; set; } // Healthy, Degraded, Unhealthy
}

/// <summary>
/// Anomaly alert information.
/// </summary>
public class AnomalyAlert
{
    public string? AlertType { get; set; } // HighLatency, HighErrorRate, LowTraffic, etc.
    public string? Endpoint { get; set; }
    public string? Message { get; set; }
    public DateTime DetectedAt { get; set; }
    public int Severity { get; set; } // 1-5
}

/// <summary>
/// Implementation of request metrics analyzer service.
/// </summary>
public class RequestMetricsAnalyzerService : IRequestMetricsAnalyzerService
{
    private readonly IMetricsCollectionService _metricsService;
    private readonly ILogger<RequestMetricsAnalyzerService> _logger;

    public RequestMetricsAnalyzerService(
        IMetricsCollectionService metricsService,
        ILogger<RequestMetricsAnalyzerService> logger)
    {
        _metricsService = metricsService ?? throw new ArgumentNullException(nameof(metricsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RequestPatternAnalysis> AnalyzeRequestPatternsAsync()
    {
        try
        {
            var stats = await _metricsService.GetTodayStatisticsAsync();
            var slowRequests = await _metricsService.GetSlowRequestsAsync(0);

            var successCount = slowRequests.Count(r => r.HttpStatusCode < 400);
            var failureCount = slowRequests.Count(r => r.HttpStatusCode >= 400);

            var mostAccessedEndpoint = slowRequests
                .GroupBy(r => $"{r.ServiceName}/{r.MethodName}")
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key;

            var slowestEndpoint = slowRequests
                .GroupBy(r => $"{r.ServiceName}/{r.MethodName}")
                .Select(g => new { path = g.Key, avgTime = g.Average(r => r.DurationMs) })
                .OrderByDescending(x => x.avgTime)
                .FirstOrDefault()?.path;

            return new RequestPatternAnalysis
            {
                TotalRequests = slowRequests.Count,
                AverageResponseTime = stats.AverageResponseTimeMs,
                SuccessCount = successCount,
                FailureCount = failureCount,
                SuccessRate = slowRequests.Count > 0 ? (double)successCount / slowRequests.Count : 1.0,
                MostAccessedEndpoint = mostAccessedEndpoint,
                SlowestEndpoint = slowestEndpoint
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing request patterns");
            return new RequestPatternAnalysis();
        }
    }

    public async Task<EndpointHealthScore> AnalyzeEndpointHealthAsync(string path)
    {
        if (string.IsNullOrEmpty(path))
            return new EndpointHealthScore { HealthScore = 0 };

        try
        {
            var slowRequests = await _metricsService.GetSlowRequestsAsync(0);
            var endpointRequests = slowRequests
                .Where(r => $"{r.ServiceName}/{r.MethodName}" == path).ToList();

            if (endpointRequests.Count == 0)
                return new EndpointHealthScore { Endpoint = path, HealthScore = 0 };

            var successCount = endpointRequests.Count(r => r.HttpStatusCode < 400);
            var successRate = (double)successCount / endpointRequests.Count;
            var avgResponseTime = endpointRequests.Average(r => r.DurationMs);

            // Calculate health score (0-100)
            var healthScore = (successRate * 80) + (Math.Max(0, 100 - avgResponseTime) / 100 * 20);
            healthScore = Math.Min(100, Math.Max(0, healthScore));

            var status = healthScore >= 80 ? "Healthy" :
                        healthScore >= 60 ? "Degraded" : "Unhealthy";

            return new EndpointHealthScore
            {
                Endpoint = path,
                HealthScore = healthScore,
                RequestCount = endpointRequests.Count,
                SuccessRate = successRate,
                AverageResponseTime = avgResponseTime,
                Status = status
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing endpoint health for path {Path}", path);
            return new EndpointHealthScore { Endpoint = path, HealthScore = 0 };
        }
    }

    public async Task<List<AnomalyAlert>> DetectAnomaliesAsync()
    {
        var alerts = new List<AnomalyAlert>();

        try
        {
            var analysis = await AnalyzeRequestPatternsAsync();

            // Alert on high error rate
            if (analysis.SuccessRate < 0.95)
            {
                alerts.Add(new AnomalyAlert
                {
                    AlertType = "HighErrorRate",
                    Message = $"Error rate is {(1 - analysis.SuccessRate) * 100:F1}% (threshold: 5%)",
                    DetectedAt = DateTime.UtcNow,
                    Severity = analysis.SuccessRate < 0.90 ? 4 : 2
                });
            }

            // Alert on high latency
            if (analysis.AverageResponseTime > 1000)
            {
                alerts.Add(new AnomalyAlert
                {
                    AlertType = "HighLatency",
                    Message = $"Average response time is {analysis.AverageResponseTime:F0}ms (threshold: 1000ms)",
                    DetectedAt = DateTime.UtcNow,
                    Severity = analysis.AverageResponseTime > 5000 ? 5 : 3
                });
            }

            _logger.LogInformation("Anomaly detection completed - Found {AlertCount} anomalies", alerts.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting anomalies");
        }

        return alerts;
    }
}
