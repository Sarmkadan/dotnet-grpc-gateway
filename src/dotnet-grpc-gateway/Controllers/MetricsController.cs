#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Infrastructure;
using DotNetGrpcGateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotNetGrpcGateway.Controllers;

/// <summary>
/// REST API controller for metrics and performance data.
/// Provides detailed performance statistics, latency percentiles, and historical data.
/// </summary>
[ApiController]
[Route(ApiRouteTemplate)]
[Produces(JsonContentType)]
public class MetricsController : ControllerBase
{
    private const string ApiRouteTemplate = "api/[controller]";
    private const string JsonContentType = "application/json";
    private const string PerformanceRoute = "performance";
    private const string RequestsRoute = "requests";
    private const string SlowRoute = "slow";
    private const string ErrorsRoute = "errors";
    private const string EndpointsRoute = "endpoints";
    private const string ResetRoute = "reset";
    private const string PerformanceMonitoringDisabledMessage = "Performance monitoring not enabled";
    private const string GettingPerformanceMetricsLogMessage = "Getting performance metrics";
    private const string FinishedGettingPerformanceMetricsLogMessage = "Finished getting performance metrics";
    private const string InvalidDaysBackMessage = "daysBack must be at least 1";
    private const string InvalidThresholdMessage = "Threshold must be non-negative";
    private const string MetricsResetLogMessage = "Metrics reset by user";
    private const string MetricsResetSuccessMessage = "Metrics reset successfully";

    private readonly IMetricsCollectionService _metricsService;
    private readonly IPerformanceMonitor? _performanceMonitor;
    private readonly ILogger<MetricsController> _logger;

    public MetricsController(
        IMetricsCollectionService metricsService,
        ILogger<MetricsController> logger,
        IPerformanceMonitor? performanceMonitor = null)
    {
        ArgumentNullException.ThrowIfNull(metricsService);
        ArgumentNullException.ThrowIfNull(logger);

        _metricsService = metricsService;
        _logger = logger;
        _performanceMonitor = performanceMonitor;
    }

    /// <summary>
    /// Gets performance metrics including latency percentiles and throughput.
    /// </summary>
    [HttpGet(PerformanceRoute)]
    [ProducesResponseType(typeof(PerformanceMetrics), StatusCodes.Status200OK)]
    public async Task<ActionResult<PerformanceMetrics>> GetPerformanceMetrics()
    {
        _logger.LogInformation(GettingPerformanceMetricsLogMessage);
        if (_performanceMonitor is null)
        {
            _logger.LogWarning(PerformanceMonitoringDisabledMessage);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, PerformanceMonitoringDisabledMessage);
        }

        var metrics = await _performanceMonitor.GetMetricsAsync();
        _logger.LogInformation(FinishedGettingPerformanceMetricsLogMessage);
        return Ok(metrics);
    }

    /// <summary>
    /// Gets request count over a time period.
    /// </summary>
    [HttpGet(RequestsRoute)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetRequestMetrics([FromQuery] int? daysBack = 7)
    {
        if (daysBack.GetValueOrDefault(7) < 1)
            return BadRequest(InvalidDaysBackMessage);

        var stats = await _metricsService.GetTodayStatisticsAsync();

        return Ok(new
        {
            totalRequests = stats.TotalRequestsProcessed,
            successfulRequests = stats.SuccessfulRequests,
            failedRequests = stats.FailedRequests,
            averageResponseTime = stats.AverageResponseTimeMs,
            date = DateTime.UtcNow.Date
        });
    }

    /// <summary>
    /// Gets slow requests exceeding a threshold.
    /// </summary>
    [HttpGet(SlowRoute)]
    [ProducesResponseType(typeof(List<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetSlowRequests([FromQuery] int thresholdMs = 1000, [FromQuery] int limit = 20)
    {
        if (thresholdMs < 0)
            return BadRequest(InvalidThresholdMessage);

        var slowRequests = await _metricsService.GetSlowRequestsAsync(thresholdMs);

        var result = slowRequests.Take(limit).Select(r => new
        {
            Path = $"{r.ServiceName}/{r.MethodName}",
            Method = r.MethodName,
            ResponseTime = r.DurationMs,
            StatusCode = r.HttpStatusCode,
            Timestamp = r.RecordedAt
        }).ToList();

        return Ok(result);
    }

    /// <summary>
    /// Gets error distribution across different error codes.
    /// </summary>
    [HttpGet(ErrorsRoute)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetErrorMetrics()
    {
        var slowRequests = await _metricsService.GetSlowRequestsAsync(0); // Get all requests

        var errorDistribution = slowRequests
            .Where(r => r.HttpStatusCode >= 400)
            .GroupBy(r => r.HttpStatusCode)
            .OrderByDescending(g => g.Count())
            .Select(g => new { statusCode = g.Key, count = g.Count() })
            .ToList();

        return Ok(new
        {
            totalErrors = errorDistribution.Sum(e => e.count),
            errorDistribution = errorDistribution
        });
    }

    /// <summary>
    /// Gets endpoint usage statistics.
    /// </summary>
    [HttpGet(EndpointsRoute)]
    [ProducesResponseType(typeof(List<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetEndpointStats()
    {
        var slowRequests = await _metricsService.GetSlowRequestsAsync(0);

        var endpointStats = slowRequests
            .GroupBy(r => $"{r.ServiceName}/{r.MethodName}")
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => new
            {
                path = g.Key,
                count = g.Count(),
                avgResponseTime = g.Average(r => r.DurationMs),
                minResponseTime = g.Min(r => r.DurationMs),
                maxResponseTime = g.Max(r => r.DurationMs)
            })
            .ToList();

        return Ok(endpointStats);
    }

    /// <summary>
    /// Resets all performance metrics.
    /// </summary>
    [HttpPost(ResetRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> ResetMetrics()
    {
        if (_performanceMonitor is null)
            return StatusCode(StatusCodes.Status503ServiceUnavailable, PerformanceMonitoringDisabledMessage);

        await _performanceMonitor.ResetAsync();
        _logger.LogInformation(MetricsResetLogMessage);

        return Ok(new { message = MetricsResetSuccessMessage });
    }
}
