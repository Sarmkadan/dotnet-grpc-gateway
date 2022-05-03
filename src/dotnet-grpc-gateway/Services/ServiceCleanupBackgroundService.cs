#nullable enable
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
    private const int UnhealthyObservationsBeforeDeactivation = 3;
    private readonly Dictionary<int, int> _consecutiveUnhealthyObservations = new();

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
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        try
        {
            var allServices = await gatewayService.GetAllServicesAsync();
            var cleanupCount = 0;

            // Drop tracking state for services that no longer exist.
            var knownIds = allServices.Select(s => s.Id).ToHashSet();
            foreach (var staleId in _consecutiveUnhealthyObservations.Keys.Where(id => !knownIds.Contains(id)).ToList())
                _consecutiveUnhealthyObservations.Remove(staleId);

            foreach (var service in allServices)
            {
                stoppingToken.ThrowIfCancellationRequested();

                _logger.LogDebug("Service cleanup check - Service: {ServiceName} (ID: {ServiceId})",
                    service.Name, service.Id);

                if (!service.IsActive)
                    continue;

                if (service.IsHealthy)
                {
                    _consecutiveUnhealthyObservations.Remove(service.Id);
                    continue;
                }

                var observations = _consecutiveUnhealthyObservations.GetValueOrDefault(service.Id) + 1;
                _consecutiveUnhealthyObservations[service.Id] = observations;

                if (observations < UnhealthyObservationsBeforeDeactivation)
                    continue;

                // Mark services inactive after the threshold number of consecutive
                // unhealthy observations across cleanup cycles.
                service.IsActive = false;
                service.ModifiedAt = DateTime.UtcNow;
                await unitOfWork.Services.UpdateAsync(service);
                _consecutiveUnhealthyObservations.Remove(service.Id);
                cleanupCount++;

                _logger.LogWarning(
                    "Service marked inactive after {Observations} consecutive unhealthy checks - Service: {ServiceName} (ID: {ServiceId})",
                    observations, service.Name, service.Id);
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
