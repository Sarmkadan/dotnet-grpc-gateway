// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Domain;

namespace DotNetGrpcGateway.Infrastructure;

/// <summary>
/// Repository for RequestMetric and GatewayStatistics entities
/// </summary>
public interface IMetricsRepository
{
    Task<RequestMetric> RecordRequestAsync(RequestMetric metric);
    Task<List<RequestMetric>> GetMetricsAsync(DateTime from, DateTime to);
    Task<List<RequestMetric>> GetServiceMetricsAsync(int serviceId, int take = 100);
    Task<GatewayStatistics> GetStatisticsAsync(DateTime date);
    Task UpdateStatisticsAsync(GatewayStatistics stats);
    Task<List<RequestMetric>> GetSlowRequestsAsync(double thresholdMs);
}

public class MetricsRepository : IMetricsRepository
{
    private readonly Dictionary<int, RequestMetric> _metricsById = new();
    private readonly Dictionary<string, List<RequestMetric>> _metricsByService = new();
    private readonly Dictionary<DateTime, GatewayStatistics> _statisticsByDate = new();
    private readonly IConnectionStringProvider _connectionProvider;
    private int _nextMetricId = 1;

    public MetricsRepository(IConnectionStringProvider connectionProvider)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
    }

    public async Task<RequestMetric> RecordRequestAsync(RequestMetric metric)
    {
        if (metric == null)
            throw new ArgumentNullException(nameof(metric));

        metric.Validate();

        metric.Id = _nextMetricId++;
        metric.RecordedAt = DateTime.UtcNow;

        _metricsById[metric.Id] = metric;

        if (!_metricsByService.ContainsKey(metric.ServiceName))
            _metricsByService[metric.ServiceName] = new List<RequestMetric>();

        _metricsByService[metric.ServiceName].Add(metric);

        return metric;
    }

    public async Task<List<RequestMetric>> GetMetricsAsync(DateTime from, DateTime to)
    {
        return _metricsById.Values
            .Where(x => x.RecordedAt >= from && x.RecordedAt <= to)
            .OrderByDescending(x => x.RecordedAt)
            .ToList();
    }

    public async Task<List<RequestMetric>> GetServiceMetricsAsync(int serviceId, int take = 100)
    {
        return _metricsById.Values
            .Where(x => x.RouteId == serviceId)
            .OrderByDescending(x => x.RecordedAt)
            .Take(take)
            .ToList();
    }

    public async Task<GatewayStatistics> GetStatisticsAsync(DateTime date)
    {
        var dateKey = date.Date;

        if (_statisticsByDate.TryGetValue(dateKey, out var stats))
            return stats;

        var newStats = new GatewayStatistics { StatisticsDate = dateKey };
        _statisticsByDate[dateKey] = newStats;
        return newStats;
    }

    public async Task UpdateStatisticsAsync(GatewayStatistics stats)
    {
        if (stats == null)
            throw new ArgumentNullException(nameof(stats));

        stats.Validate();
        stats.UpdatedAt = DateTime.UtcNow;

        var dateKey = stats.StatisticsDate.Date;
        _statisticsByDate[dateKey] = stats;
    }

    public async Task<List<RequestMetric>> GetSlowRequestsAsync(double thresholdMs)
    {
        return _metricsById.Values
            .Where(x => x.IsSlowRequest(thresholdMs))
            .OrderByDescending(x => x.DurationMs)
            .ToList();
    }
}
