// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Domain;

namespace DotNetGrpcGateway.Services;

/// <summary>
/// Collects and aggregates metrics for gateway requests and services
/// </summary>
public interface IMetricsCollectionService
{
    Task RecordRequestMetricAsync(RequestMetric metric);
    Task<GatewayStatistics> GetTodayStatisticsAsync();
    Task<GatewayStatistics> GetStatisticsAsync(DateTime date);
    Task<List<RequestMetric>> GetSlowRequestsAsync(double thresholdMs = 1000);
    Task<Dictionary<string, int>> GetRequestsPerServiceAsync();
    Task<double> GetAverageResponseTimeAsync();
    Task UpdateServiceMetricsAsync(int serviceId, double responseTimeMs, bool success);
}

public class MetricsCollectionService : IMetricsCollectionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MetricsCollectionService> _logger;
    private GatewayStatistics? _todayStats;

    public MetricsCollectionService(IUnitOfWork unitOfWork, ILogger<MetricsCollectionService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task RecordRequestMetricAsync(RequestMetric metric)
    {
        if (metric == null)
            throw new ArgumentNullException(nameof(metric));

        try
        {
            metric.Validate();

            // Record the metric
            await _unitOfWork.Metrics.RecordRequestAsync(metric);

            // Update today's statistics
            var todayStats = await GetTodayStatisticsAsync();
            todayStats.RecordRequest(metric.IsSuccessful, metric.DurationMs, metric.RequestSizeBytes);
            todayStats.RecordServiceRequest(metric.ServiceName);
            todayStats.RecordMethodCall(metric.MethodName);

            if (!metric.IsSuccessful)
            {
                todayStats.RecordError(metric.GrpcStatusCode ?? "Unknown");
            }

            if (metric.CacheHitStatus != null)
            {
                todayStats.RecordCacheHit(metric.CacheHitStatus == "HIT");
            }

            await _unitOfWork.Metrics.UpdateStatisticsAsync(todayStats);

            if (metric.IsSlowRequest(1000))
            {
                _logger.LogWarning(
                    "Slow request detected: {Service}.{Method} took {Duration}ms",
                    metric.ServiceName,
                    metric.MethodName,
                    metric.DurationMs);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording request metric");
            throw;
        }
    }

    public async Task<GatewayStatistics> GetTodayStatisticsAsync()
    {
        return await GetStatisticsAsync(DateTime.UtcNow);
    }

    public async Task<GatewayStatistics> GetStatisticsAsync(DateTime date)
    {
        var stats = await _unitOfWork.Metrics.GetStatisticsAsync(date);

        if (date.Date == DateTime.UtcNow.Date)
        {
            _todayStats = stats;
        }

        return stats;
    }

    public async Task<List<RequestMetric>> GetSlowRequestsAsync(double thresholdMs = 1000)
    {
        return await _unitOfWork.Metrics.GetSlowRequestsAsync(thresholdMs);
    }

    public async Task<Dictionary<string, int>> GetRequestsPerServiceAsync()
    {
        var todayStats = await GetTodayStatisticsAsync();
        return todayStats.RequestsByService
            .ToDictionary(x => x.Key, x => (int)x.Value);
    }

    public async Task<double> GetAverageResponseTimeAsync()
    {
        var todayStats = await GetTodayStatisticsAsync();
        return todayStats.AverageResponseTimeMs;
    }

    public async Task UpdateServiceMetricsAsync(int serviceId, double responseTimeMs, bool success)
    {
        try
        {
            var service = await _unitOfWork.Services.GetByIdAsync(serviceId);
            if (service != null)
            {
                service.RecordRequestMetric(responseTimeMs, success);
                await _unitOfWork.Services.UpdateAsync(service);

                _logger.LogDebug(
                    "Service {ServiceId} metrics updated: {Duration}ms, Success={Success}",
                    serviceId,
                    responseTimeMs,
                    success);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating service metrics for service {ServiceId}", serviceId);
        }
    }
}
