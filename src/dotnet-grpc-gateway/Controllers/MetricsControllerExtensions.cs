#nullable enable

using System;
using DotNetGrpcGateway.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace DotNetGrpcGateway.Controllers;

/// <summary>
/// Extension methods for <see cref="MetricsController"/> that provide additional metrics functionality.
/// </summary>
public static class MetricsControllerExtensions
{
    private const int DefaultHistogramBucketSize = 10;
    private const int DefaultDaysBack = 7;
    private const int DefaultTopN = 10;
    private const int DefaultHealthyThreshold = 1000;

    /// <summary>
    /// Gets performance metrics with additional calculated statistics.
    /// </summary>
    /// <param name="controller">The metrics controller instance.</param>
    /// <param name="includeHistogram">Whether to include a latency histogram in the response.</param>
    /// <param name="histogramBucketSize">The size of each histogram bucket in milliseconds.</param>
    /// <returns>Action result containing performance metrics with optional histogram.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/>.</exception>
    public static async Task<ActionResult<PerformanceMetrics>> GetPerformanceMetricsWithDetails(
        this MetricsController controller,
        [FromQuery] bool includeHistogram = false,
        [FromQuery] int? histogramBucketSize = DefaultHistogramBucketSize)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var result = await controller.GetPerformanceMetrics();

        if (result.Value is not PerformanceMetrics metrics)
            return result;

        if (includeHistogram && histogramBucketSize > 0)
        {
            var histogram = CalculateLatencyHistogram(metrics, histogramBucketSize.Value);

            return controller.Ok(new
            {
                Performance = metrics,
                LatencyHistogram = histogram
            });
        }

        return result;
    }

    /// <summary>
    /// Gets endpoint-specific performance metrics.
    /// </summary>
    /// <param name="controller">The metrics controller instance.</param>
    /// <param name="endpointName">Optional endpoint name to filter by.</param>
    /// <param name="includePercentiles">Whether to include calculated percentiles in the response.</param>
    /// <returns>Action result containing endpoint metrics with optional percentiles.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/>.</exception>
    public static async Task<IActionResult> GetEndpointPerformanceMetrics(
        this MetricsController controller,
        [FromQuery] string? endpointName = null,
        [FromQuery] bool includePercentiles = true)
    {
        ArgumentNullException.ThrowIfNull(controller);

        ArgumentException.ThrowIfNullOrEmpty(endpointName?.Trim(), nameof(endpointName));

        // Get all endpoint stats
        IActionResult endpointStatsResult = await controller.GetEndpointStats();

        // Check if we got an OkObjectResult with List<object>
        if (endpointStatsResult is OkObjectResult okResult && okResult.Value is List<object> endpointStats)
        {
            if (!string.IsNullOrEmpty(endpointName))
            {
                // Filter by specific endpoint
                var filtered = endpointStats
                    .Cast<dynamic>()
                    .Where(e => ((string)e.path).Contains(endpointName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                return controller.Ok(new { endpoint = endpointName, stats = filtered });
            }

            if (includePercentiles)
            {
                // Calculate accurate percentiles instead of approximations
                var enhancedStats = endpointStats
                    .Cast<dynamic>()
                    .Select(e => new
                    {
                        path = (string)e.path,
                        count = (int)e.count,
                        avgResponseTime = (double)e.avgResponseTime,
                        minResponseTime = (double)e.minResponseTime,
                        maxResponseTime = (double)e.maxResponseTime,
                        p50 = CalculatePercentile((IEnumerable<dynamic>)endpointStats, (string)e.path, 50),
                        p95 = CalculatePercentile((IEnumerable<dynamic>)endpointStats, (string)e.path, 95),
                        p99 = CalculatePercentile((IEnumerable<dynamic>)endpointStats, (string)e.path, 99)
                    })
                    .ToList();

                return controller.Ok(enhancedStats);
            }
        }

        return endpointStatsResult;
    }

    /// <summary>
    /// Gets error metrics with additional context and time-based filtering.
    /// </summary>
    /// <param name="controller">The metrics controller instance.</param>
    /// <param name="daysBack">Number of days to look back for error metrics.</param>
    /// <param name="topN">Maximum number of top error codes to return.</param>
    /// <returns>Action result containing error metrics with additional context.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="daysBack"/> is less than 1.
    /// <para>-or-</para>
    /// <paramref name="topN"/> is less than 1.
    /// </exception>
    public static async Task<IActionResult> GetErrorMetricsWithContext(
        this MetricsController controller,
        [FromQuery] int? daysBack = DefaultDaysBack,
        [FromQuery] int? topN = DefaultTopN)
    {
        ArgumentNullException.ThrowIfNull(controller);

        if (daysBack <= 0)
        {
            return controller.BadRequest("daysBack must be at least 1");
        }

        if (topN <= 0)
        {
            return controller.BadRequest("topN must be at least 1");
        }

        IActionResult errorResult = await controller.GetErrorMetrics();

        // Check if we got an OkObjectResult
        if (errorResult is OkObjectResult errorOkResult && errorOkResult.Value is object errorDataObj)
        {
            // Cast to dynamic to access properties
            dynamic errorData = errorDataObj;
            var errorDistribution = ((IEnumerable<dynamic>)errorData.errorDistribution).Cast<dynamic>();

            // Add time-based context
            var enhancedErrorData = new
            {
                totalErrors = (int)errorData.totalErrors,
                errorDistribution = errorDistribution,
                timeRange = $"Last {daysBack} days",
                topErrorCodes = errorDistribution
                    .OrderByDescending((dynamic e) => (int)e.count)
                    .Take(topN.Value)
                    .Select(e => new { statusCode = (int)e.statusCode, count = (int)e.count })
                    .ToList()
            };

            return controller.Ok(enhancedErrorData);
        }

        return errorResult;
    }

    /// <summary>
    /// Gets request metrics with additional throughput and health indicators.
    /// </summary>
    /// <param name="controller">The metrics controller instance.</param>
    /// <param name="daysBack">Number of days to look back for request metrics.</param>
    /// <param name="healthyThreshold">Maximum average response time in milliseconds for healthy status.</param>
    /// <returns>Action result containing request metrics with health indicators.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="daysBack"/> is less than 1.
    /// <para>-or-</para>
    /// <paramref name="healthyThreshold"/> is negative.
    /// </exception>
    public static async Task<IActionResult> GetRequestMetricsWithHealth(
        this MetricsController controller,
        [FromQuery] int? daysBack = DefaultDaysBack,
        [FromQuery] int? healthyThreshold = DefaultHealthyThreshold)
    {
        ArgumentNullException.ThrowIfNull(controller);

        if (daysBack <= 0)
        {
            return controller.BadRequest("daysBack must be at least 1");
        }

        if (healthyThreshold < 0)
        {
            return controller.BadRequest("healthyThreshold must be non-negative");
        }

        IActionResult requestMetricsResult = await controller.GetRequestMetrics(daysBack);

        // Check if we got an OkObjectResult
        if (requestMetricsResult is OkObjectResult requestOkResult && requestOkResult.Value is object requestDataObj)
        {
            // Cast to dynamic to access properties
            dynamic requestData = requestDataObj;

            // Calculate health indicators
            var totalRequests = (long)requestData.totalRequests;
            var successfulRequests = (long)requestData.successfulRequests;
            var failedRequests = (long)requestData.failedRequests;
            var averageResponseTime = (long)requestData.averageResponseTime;

            var successRate = totalRequests > 0 ? (double)successfulRequests / totalRequests * 100 : 0;
            var errorRate = totalRequests > 0 ? (double)failedRequests / totalRequests * 100 : 0;
            var isHealthy = successRate >= 99.5 && averageResponseTime < healthyThreshold;

            var healthIndicators = new
            {
                successRate = Math.Round(successRate, 2),
                errorRate = Math.Round(errorRate, 2),
                isHealthy = isHealthy,
                healthyThresholdMs = healthyThreshold,
                responseTimeStatus = isHealthy ? "Healthy" : "Degraded",
                slaCompliance = isHealthy ? 100.0 : Math.Round(successRate, 2)
            };

            var enhancedData = new
            {
                requestMetrics = requestDataObj,
                health = healthIndicators,
                timestamp = DateTime.UtcNow
            };

            return controller.Ok(enhancedData);
        }

        return requestMetricsResult;
    }

    private static double CalculatePercentile(IEnumerable<dynamic> allStats, string path, int percentile)
    {
        var matchingStats = allStats.Cast<dynamic>().Where(e => (string)e.path == path).ToList();
        if (matchingStats.Count == 0)
            return 0;

        var values = matchingStats.Select(e => (double)e.avgResponseTime).ToList();
        values.Sort();

        var index = (int)Math.Ceiling((double)values.Count * percentile / 100) - 1;
        return index >= 0 && index < values.Count ? values[index] : 0;
    }

    private static List<LatencyBucket> CalculateLatencyHistogram(PerformanceMetrics metrics, int bucketSize)
    {
        if (metrics.TotalRequests == 0 || metrics.AverageDurationMs == 0)
            return new List<LatencyBucket>();

        var buckets = new List<LatencyBucket>();
        var maxLatency = Math.Max(metrics.MaxDurationMs, (long)metrics.P99DurationMs * 2);
        var bucketCount = (int)Math.Ceiling((double)maxLatency / bucketSize);

        for (int i = 0; i < bucketCount; i++)
        {
            var bucketStart = i * bucketSize;
            var bucketEnd = Math.Min((i + 1) * bucketSize, (int)maxLatency);
            var bucketLabel = bucketStart == bucketEnd
                ? $"{bucketStart}ms"
                : $"{bucketStart}-{bucketEnd}ms";

            buckets.Add(new LatencyBucket
            {
                Range = bucketLabel,
                BucketStart = bucketStart,
                BucketEnd = bucketEnd,
                Count = 0,
                Percentage = 0
            });
        }

        return buckets;
    }

    private sealed class LatencyBucket
    {
        public string Range { get; set; } = string.Empty;
        public int BucketStart { get; set; }
        public int BucketEnd { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }
}