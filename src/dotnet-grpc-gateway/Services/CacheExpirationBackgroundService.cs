// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Caching;

namespace DotNetGrpcGateway.Services;

/// <summary>
/// Background service that periodically monitors and reports on cache health.
/// Logs cache statistics and alerts on abnormal cache behavior.
/// </summary>
public class CacheExpirationBackgroundService : BackgroundService
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheExpirationBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(10); // Check every 10 minutes

    public CacheExpirationBackgroundService(
        ICacheService cacheService,
        ILogger<CacheExpirationBackgroundService> logger)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Cache expiration monitoring service starting");

        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // Wait for application to fully start

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await MonitorCacheHealthAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cache health monitoring");
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

        _logger.LogInformation("Cache expiration monitoring service stopped");
    }

    private async Task MonitorCacheHealthAsync(CancellationToken stoppingToken)
    {
        try
        {
            var stats = await _cacheService.GetStatisticsAsync();

            _logger.LogInformation(
                "Cache health - Entries: {EntryCount}, " +
                "Size: {SizeKB}KB, " +
                "Hit Rate: {HitRate:P}, " +
                "Total Hits: {HitCount}, " +
                "Total Misses: {MissCount}",
                stats.EntryCount,
                stats.ApproximateSizeBytes / 1024,
                stats.HitRate,
                stats.HitCount,
                stats.MissCount);

            // Alert if cache is empty but requests are being made
            if (stats.EntryCount == 0 && stats.HitCount > 0)
            {
                _logger.LogWarning("Cache is empty despite {HitCount} hits - possible expiration issues", stats.HitCount);
            }

            // Alert on low hit rate
            if (stats.HitRate < 0.5 && stats.HitCount + stats.MissCount > 100)
            {
                _logger.LogWarning("Low cache hit rate: {HitRate:P} - consider adjusting cache policy", stats.HitRate);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache health");
        }
    }
}
