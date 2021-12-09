// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Infrastructure;

namespace DotNetGrpcGateway.Services;

/// <summary>
/// Background service that periodically aggregates and persists metrics.
/// Runs on a scheduled interval to compile detailed performance statistics.
/// </summary>
public class MetricsAggregationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MetricsAggregationBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5); // Aggregate every 5 minutes

    public MetricsAggregationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<MetricsAggregationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Metrics aggregation service starting");

        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // Wait for application to fully start

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await AggregateMetricsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during metrics aggregation");
            }

            try
            {
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Metrics aggregation service stopped");
    }

    private async Task AggregateMetricsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var metricsService = scope.ServiceProvider.GetRequiredService<IMetricsCollectionService>();
        var performanceMonitor = scope.ServiceProvider.GetService<IPerformanceMonitor>();

        try
        {
            // Get current statistics
            var todayStats = await metricsService.GetTodayStatisticsAsync();
            var performanceMetrics = await performanceMonitor?.GetMetricsAsync()
                ?? Task.FromResult<PerformanceMetrics?>(null);

            _logger.LogInformation(
                "Metrics aggregated - Requests: {TotalRequests}, " +
                "Avg Response Time: {AvgResponseTime}ms, " +
                "Requests/sec: {RPS:F2}",
                todayStats.TotalRequests,
                performanceMetrics?.AverageDurationMs ?? 0,
                performanceMetrics?.RequestsPerSecond ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error aggregating metrics");
        }
    }
}
