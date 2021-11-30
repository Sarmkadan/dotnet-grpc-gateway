// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Configuration;
using Microsoft.Extensions.Options;

namespace DotNetGrpcGateway.Services;

/// <summary>
/// Background service that periodically performs health checks on registered services
/// </summary>
public class HealthCheckBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HealthCheckBackgroundService> _logger;
    private readonly IOptions<GatewayOptions> _options;
    private PeriodicTimer? _timer;

    public HealthCheckBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<HealthCheckBackgroundService> logger,
        IOptions<GatewayOptions> options)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(_options.Value.HealthCheck.IntervalSeconds);
        _timer = new PeriodicTimer(interval);

        _logger.LogInformation(
            "Health check background service started with interval {IntervalSeconds}s",
            _options.Value.HealthCheck.IntervalSeconds);

        try
        {
            while (await _timer.WaitForNextTickAsync(stoppingToken))
            {
                await PerformHealthChecksAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Health check background service is stopping");
        }
    }

    private async Task PerformHealthChecksAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var discoveryService = scope.ServiceProvider.GetRequiredService<IServiceDiscoveryService>();
            var gatewayService = scope.ServiceProvider.GetRequiredService<IGatewayService>();

            try
            {
                var services = await gatewayService.GetAllServicesAsync();

                if (services.Count == 0)
                {
                    _logger.LogDebug("No services registered for health check");
                    return;
                }

                _logger.LogDebug("Performing health checks for {ServiceCount} services", services.Count);

                var tasks = services
                    .Where(s => s.IsActive)
                    .Select(async service =>
                    {
                        try
                        {
                            await discoveryService.PerformHealthCheckAsync(service.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(
                                ex,
                                "Health check failed for service {ServiceName}",
                                service.Name);
                        }
                    });

                await Task.WhenAll(tasks);

                // Get updated health status
                var health = await discoveryService.GetAllServicesHealthAsync();
                var healthyCount = health.Values.Count(x => x == ServiceHealthStatus.Healthy);

                _logger.LogInformation(
                    "Health check completed: {HealthyCount}/{TotalCount} services healthy",
                    healthyCount,
                    services.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing health checks");
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Dispose();
        await base.StopAsync(cancellationToken);
    }
}
