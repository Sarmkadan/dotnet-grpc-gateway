// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Services;

/// <summary>
/// Background service for cleaning up stale or unhealthy services.
/// Periodically removes inactive services and orphaned routes.
/// </summary>
public class ServiceCleanupBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ServiceCleanupBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1); // Cleanup every hour

    public ServiceCleanupBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ServiceCleanupBackgroundService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Service cleanup background service starting");

        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait before first cleanup

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupServicesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during service cleanup");
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

        _logger.LogInformation("Service cleanup background service stopped");
    }

    private async Task CleanupServicesAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var gatewayService = scope.ServiceProvider.GetRequiredService<IGatewayService>();

        try
        {
            var allServices = await gatewayService.GetAllServicesAsync();
            var cleanupCount = 0;

            foreach (var service in allServices)
            {
                // Mark services as inactive if they've been unhealthy for extended period
                // This is a placeholder - in a real implementation, you'd track health check history
                // and mark services inactive after threshold number of failed checks

                _logger.LogDebug("Service cleanup check - Service: {ServiceName} (ID: {ServiceId})",
                    service.Name, service.Id);
            }

            if (cleanupCount > 0)
                _logger.LogInformation("Service cleanup completed - {Count} services cleaned up", cleanupCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during service cleanup");
        }
    }
}
