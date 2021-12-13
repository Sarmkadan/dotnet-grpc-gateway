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
[Route("api/[controller]")]
[Produces("application/json")]
public class MetricsController : ControllerBase
{
    private readonly IMetricsCollectionService _metricsService;
    private readonly IPerformanceMonitor? _performanceMonitor;
    private readonly ILogger<MetricsController> _logger;

    public MetricsController(
        IMetricsCollectionService metricsService,
        ILogger<MetricsController> logger,
        IPerformanceMonitor? performanceMonitor = null)
    {
        _metricsService = metricsService ?? throw new ArgumentNullException(nameof(metricsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _performanceMonitor = performanceMonitor;
    }

    /// <summary>
    /// Gets performance metrics including latency percentiles and throughput.
    /// </summary>
    [HttpGet("performance")]
    [ProducesResponseType(typeof(PerformanceMetrics), StatusCodes.Status200OK)]
    public async Task<ActionResult<PerformanceMetrics>> GetPerformanceMetrics()
    {
        if (_performanceMonitor == null)
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Performance monitoring not enabled");

        var metrics = await _performanceMonitor.GetMetricsAsync();
        return Ok(metrics);
    }

    /// <summary>
    /// Gets request count over a time period.
    /// </summary>
    [HttpGet("requests")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetRequestMetrics([FromQuery] int? daysBack = 7)
    {
        if (daysBack.GetValueOrDefault(7) < 1)
            return BadRequest("daysBack must be at least 1");

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
    [HttpGet("slow")]
    [ProducesResponseType(typeof(List<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetSlowRequests([FromQuery] int thresholdMs = 1000, [FromQuery] int limit = 20)
    {
        if (thresholdMs < 0)
            return BadRequest("Threshold must be non-negative");

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
    [HttpGet("errors")]
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
    [HttpGet("endpoints")]
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
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> ResetMetrics()
    {
        if (_performanceMonitor == null)
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Performance monitoring not enabled");

        await _performanceMonitor.ResetAsync();
        _logger.LogInformation("Metrics reset by user");

        return Ok(new { message = "Metrics reset successfully" });
    }
}
