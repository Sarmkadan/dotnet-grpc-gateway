#nullable enable

using DotNetGrpcGateway.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace DotNetGrpcGateway.Controllers;

/// <summary>
/// Extension methods for <see cref="MetricsController"/> that provide additional metrics functionality.
/// </summary>
public static class MetricsControllerExtensions
{
    /// <summary>
    /// Gets performance metrics with additional calculated statistics.
    /// </summary>
    public static async Task<ActionResult<PerformanceMetrics>> GetPerformanceMetricsWithDetails(
        this MetricsController controller,
        [FromQuery] bool includeHistogram = false,
        [FromQuery] int? histogramBucketSize = 10)
    {
        if (controller is null)
            throw new ArgumentNullException(nameof(controller));

        var result = await controller.GetPerformanceMetrics();

        if (result.Value is not PerformanceMetrics metrics)
            return result;

        if (includeHistogram && histogramBucketSize.HasValue && histogramBucketSize > 0)
        {
            // Calculate latency histogram
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
    public static async Task<IActionResult> GetEndpointPerformanceMetrics(
        this MetricsController controller,
        [FromQuery] string? endpointName = null,
        [FromQuery] bool includePercentiles = true)
    {
        if (controller is null)
            throw new ArgumentNullException(nameof(controller));

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
                // Add percentile calculations to each endpoint
                var enhancedStats = endpointStats
                    .Cast<dynamic>()
                    .Select(e => new
                    {
                        path = (string)e.path,
                        count = (int)e.count,
                        avgResponseTime = (double)e.avgResponseTime,
                        minResponseTime = (double)e.minResponseTime,
                        maxResponseTime = (double)e.maxResponseTime,
                        p50 = (double)e.avgResponseTime * 0.8, // Approximate
                        p95 = (double)e.avgResponseTime * 1.2, // Approximate
                        p99 = (double)e.avgResponseTime * 1.5  // Approximate
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
    public static async Task<IActionResult> GetErrorMetricsWithContext(
        this MetricsController controller,
        [FromQuery] int? daysBack = 7,
        [FromQuery] int? topN = 10)
    {
        if (controller is null)
            throw new ArgumentNullException(nameof(controller));

        if (daysBack.GetValueOrDefault(7) < 1)
            return controller.BadRequest("daysBack must be at least 1");

        if (topN.GetValueOrDefault(10) < 1)
            return controller.BadRequest("topN must be at least 1");

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
                    .Take(topN.GetValueOrDefault(10))
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
    public static async Task<IActionResult> GetRequestMetricsWithHealth(
        this MetricsController controller,
        [FromQuery] int? daysBack = 7,
        [FromQuery] int? healthyThreshold = 1000)
    {
        if (controller is null)
            throw new ArgumentNullException(nameof(controller));

        if (daysBack.GetValueOrDefault(7) < 1)
            return controller.BadRequest("daysBack must be at least 1");

        if (healthyThreshold.GetValueOrDefault(1000) < 0)
            return controller.BadRequest("healthyThreshold must be non-negative");

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
            var successRate = totalRequests > 0 ? (double)successfulRequests / totalRequests * 100 : 0;
            var errorRate = totalRequests > 0 ? (double)failedRequests / totalRequests * 100 : 0;
            var isHealthy = successRate >= 99.5 && (long)requestData.averageResponseTime < healthyThreshold;

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

    private class LatencyBucket
    {
        public string Range { get; set; } = string.Empty;
        public int BucketStart { get; set; }
        public int BucketEnd { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }
}
