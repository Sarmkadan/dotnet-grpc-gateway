#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Exceptions;

namespace DotNetGrpcGateway.Infrastructure;

/// <summary>
/// Repository for RequestMetric and GatewayStatistics entities
/// </summary>
public interface IMetricsRepository
{
    /// <summary>
    /// Records a request metric.
    /// </summary>
    /// <param name="metric">The request metric to record.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the recorded request metric.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metric"/> is null.</exception>
    Task<RequestMetric> RecordRequestAsync(RequestMetric metric, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a batch of request metrics in a single operation. Intended for use by the
    /// background metrics writer so per-request inserts never happen on the proxied
    /// request's hot path.
    /// </summary>
    /// <param name="metrics">The batch of metrics to persist.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of metrics successfully persisted.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    Task<int> BulkInsertAsync(IReadOnlyCollection<RequestMetric> metrics, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets request metrics within a specified time range.
    /// </summary>
    /// <param name="from">The start date (inclusive).</param>
    /// <param name="to">The end date (inclusive).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of request metrics.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="from"/> is after <paramref name="to"/>.</exception>
    Task<List<RequestMetric>> GetMetricsAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets request metrics for a specific service.
    /// </summary>
    /// <param name="serviceId">The service identifier.</param>
    /// <param name="take">The maximum number of metrics to return.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of request metrics.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="serviceId"/> is less than or equal to zero or <paramref name="take"/> is less than 1.</exception>
    /// <exception cref="NotFoundException">Thrown when no metrics exist for the specified <paramref name="serviceId"/>.</exception>
    Task<List<RequestMetric>> GetServiceMetricsAsync(int serviceId, int take = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets gateway statistics for a specific date.
    /// </summary>
    /// <param name="date">The date for which to retrieve statistics.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the gateway statistics.</returns>
    /// <exception cref="NotFoundException">Thrown when statistics for the specified <paramref name="date"/> are not found.</exception>
    Task<GatewayStatistics> GetStatisticsAsync(DateTime date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates gateway statistics.
    /// </summary>
    /// <param name="stats">The gateway statistics to update.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stats"/> is null.</exception>
    /// <exception cref="NotFoundException">Thrown when statistics to update are not found.</exception>
    Task UpdateStatisticsAsync(GatewayStatistics stats, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets slow requests that exceed a specified duration threshold.
    /// </summary>
    /// <param name="thresholdMs">The duration threshold in milliseconds.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of slow request metrics.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="thresholdMs"/> is less than or equal to zero.</exception>
    Task<List<RequestMetric>> GetSlowRequestsAsync(double thresholdMs, CancellationToken cancellationToken = default);
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

    public Task<RequestMetric> RecordRequestAsync(RequestMetric metric, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(metric);
        cancellationToken.ThrowIfCancellationRequested();

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

    /// <summary>
    /// Persists a batch of request metrics in a single operation.
    /// </summary>
    /// <param name="metrics">The batch of metrics to persist.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of metrics successfully persisted.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    public Task<int> BulkInsertAsync(IReadOnlyCollection<RequestMetric> metrics, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        cancellationToken.ThrowIfCancellationRequested();

        if (metrics.Count == 0)
            return Task.FromResult(0);

        foreach (var metric in metrics)
            metric.Validate();

        return _retryPolicy.ExecuteAsync(_ =>
        {
            foreach (var metric in metrics)
            {
                metric.Id = _nextMetricId++;
                metric.RecordedAt = DateTime.UtcNow;

                _metricsById[metric.Id] = metric;

                if (!_metricsByService.TryGetValue(metric.ServiceName, out var serviceMetrics))
                {
                    serviceMetrics = new List<RequestMetric>();
                    _metricsByService[metric.ServiceName] = serviceMetrics;
                }

                serviceMetrics.Add(metric);
            }

            return Task.FromResult(metrics.Count);
        }, nameof(BulkInsertAsync));
    }

    public Task<List<RequestMetric>> GetMetricsAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(from, to);
        cancellationToken.ThrowIfCancellationRequested();
        return _retryPolicy.ExecuteAsync(_ => Task.FromResult(_metricsById.Values
            .Where(x => x.RecordedAt >= from && x.RecordedAt <= to)
            .OrderByDescending(x => x.RecordedAt)
            .ToList()), nameof(GetMetricsAsync));
    }

    public Task<List<RequestMetric>> GetServiceMetricsAsync(int serviceId, int take = 100, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(serviceId, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(take, 0);
        cancellationToken.ThrowIfCancellationRequested();

        return _retryPolicy.ExecuteAsync(_ =>
        {
            var metrics = _metricsById.Values
                .Where(x => x.RouteId == serviceId)
                .OrderByDescending(x => x.RecordedAt)
                .Take(take)
                .ToList();

            if (metrics.Count == 0)
                throw new NotFoundException(nameof(RequestMetric), $"serviceId={serviceId}");

            return Task.FromResult(metrics);
        }, nameof(GetServiceMetricsAsync));
    }

    public Task<GatewayStatistics> GetStatisticsAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _retryPolicy.ExecuteAsync(_ =>
        {
            var dateKey = date.Date;

            if (_statisticsByDate.TryGetValue(dateKey, out var stats))
                return Task.FromResult(stats);

            throw new NotFoundException(nameof(GatewayStatistics), dateKey.ToString("yyyy-MM-dd"));
        }, nameof(GetStatisticsAsync));
    }

    public Task UpdateStatisticsAsync(GatewayStatistics stats, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stats);
        cancellationToken.ThrowIfCancellationRequested();

        stats.Validate();

        return _retryPolicy.ExecuteAsync(_ =>
        {
            stats.UpdatedAt = DateTime.UtcNow;

            var dateKey = stats.StatisticsDate.Date;
            if (!_statisticsByDate.ContainsKey(dateKey))
                throw new NotFoundException(nameof(GatewayStatistics), dateKey.ToString("yyyy-MM-dd"));

            _statisticsByDate[dateKey] = stats;
            return Task.CompletedTask;
        }, nameof(UpdateStatisticsAsync));
    }

    public Task<List<RequestMetric>> GetSlowRequestsAsync(double thresholdMs, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(thresholdMs, 0);
        cancellationToken.ThrowIfCancellationRequested();
        return _retryPolicy.ExecuteAsync(_ => Task.FromResult(_metricsById.Values
            .Where(x => x.IsSlowRequest(thresholdMs))
            .OrderByDescending(x => x.DurationMs)
            .ToList()), nameof(GetSlowRequestsAsync));
    }
}