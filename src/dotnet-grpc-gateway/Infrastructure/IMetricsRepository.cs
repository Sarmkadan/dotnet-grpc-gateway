#nullable enable
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
    private readonly IRetryPolicy _retryPolicy;
    private int _nextMetricId = 1;

    public MetricsRepository(IConnectionStringProvider connectionProvider, IRetryPolicy retryPolicy)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        _retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
    }

    public Task<RequestMetric> RecordRequestAsync(RequestMetric metric)
    {
        if (metric is null)
            throw new ArgumentNullException(nameof(metric));

        metric.Validate();

        return _retryPolicy.ExecuteAsync(_ =>
        {
            metric.Id = _nextMetricId++;
            metric.RecordedAt = DateTime.UtcNow;

            _metricsById[metric.Id] = metric;

            if (!_metricsByService.ContainsKey(metric.ServiceName))
                _metricsByService[metric.ServiceName] = new List<RequestMetric>();

            _metricsByService[metric.ServiceName].Add(metric);

            return Task.FromResult(metric);
        }, nameof(RecordRequestAsync));
    }

    public Task<List<RequestMetric>> GetMetricsAsync(DateTime from, DateTime to) =>
        _retryPolicy.ExecuteAsync(_ => Task.FromResult(_metricsById.Values
            .Where(x => x.RecordedAt >= from && x.RecordedAt <= to)
            .OrderByDescending(x => x.RecordedAt)
            .ToList()), nameof(GetMetricsAsync));

    public Task<List<RequestMetric>> GetServiceMetricsAsync(int serviceId, int take = 100) =>
        _retryPolicy.ExecuteAsync(_ => Task.FromResult(_metricsById.Values
            .Where(x => x.RouteId == serviceId)
            .OrderByDescending(x => x.RecordedAt)
            .Take(take)
            .ToList()), nameof(GetServiceMetricsAsync));

    public Task<GatewayStatistics> GetStatisticsAsync(DateTime date) =>
        _retryPolicy.ExecuteAsync(_ =>
        {
            var dateKey = date.Date;

            if (_statisticsByDate.TryGetValue(dateKey, out var stats))
                return Task.FromResult(stats);

            var newStats = new GatewayStatistics { StatisticsDate = dateKey };
            _statisticsByDate[dateKey] = newStats;
            return Task.FromResult(newStats);
        }, nameof(GetStatisticsAsync));

    public Task UpdateStatisticsAsync(GatewayStatistics stats)
    {
        if (stats is null)
            throw new ArgumentNullException(nameof(stats));

        stats.Validate();

        return _retryPolicy.ExecuteAsync(_ =>
        {
            stats.UpdatedAt = DateTime.UtcNow;

            var dateKey = stats.StatisticsDate.Date;
            _statisticsByDate[dateKey] = stats;
            return Task.CompletedTask;
        }, nameof(UpdateStatisticsAsync));
    }

    public Task<List<RequestMetric>> GetSlowRequestsAsync(double thresholdMs) =>
        _retryPolicy.ExecuteAsync(_ => Task.FromResult(_metricsById.Values
            .Where(x => x.IsSlowRequest(thresholdMs))
            .OrderByDescending(x => x.DurationMs)
            .ToList()), nameof(GetSlowRequestsAsync));
}
