// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Infrastructure;
using DotNetGrpcGateway.Services;

namespace DotNetGrpcGateway.Configuration;

/// <summary>
/// Extension methods for configuring gateway services and dependencies
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all gateway services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddGatewayServices(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        // Data access repositories
        services.AddScoped<IGatewayRepository, GatewayRepository>();
        services.AddScoped<IServiceRegistry, ServiceRegistry>();
        services.AddScoped<IRouteRepository, RouteRepository>();
        services.AddScoped<IMetricsRepository, MetricsRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Business logic services
        services.AddScoped<IGatewayService, GatewayService>();
        services.AddScoped<IServiceDiscoveryService, ServiceDiscoveryService>();
        services.AddScoped<IRouteResolutionService, RouteResolutionService>();
        services.AddScoped<IMetricsCollectionService, MetricsCollectionService>();
        services.AddScoped<IValidationService, ValidationService>();
        services.AddHttpClient<IGrpcClientFactory, GrpcClientFactory>();

        return services;
    }

    /// <summary>
    /// Configures gateway options from appsettings
    /// </summary>
    public static IServiceCollection AddGatewayConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        services.Configure<GatewayOptions>(configuration.GetSection("Gateway"));

        return services;
    }

    /// <summary>
    /// Adds gateway health checks
    /// </summary>
    public static IServiceCollection AddGatewayHealthChecks(
        this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<GatewayHealthCheck>("gateway")
            .AddCheck<ServiceDiscoveryHealthCheck>("service-discovery");

        return services;
    }
}

/// <summary>
/// Configuration options for the gateway
/// </summary>
public class GatewayOptions
{
    public const string SectionName = "Gateway";

    public string ListenAddress { get; set; } = "0.0.0.0";

    public int Port { get; set; } = 5000;

    public bool EnableReflection { get; set; } = true;

    public bool EnableMetrics { get; set; } = true;

    public int MaxConcurrentConnections { get; set; } = 1000;

    public int RequestTimeoutMs { get; set; } = 30000;

    public string LogLevel { get; set; } = "Information";

    public HealthCheckOptions HealthCheck { get; set; } = new();

    public MetricsOptions Metrics { get; set; } = new();
}

public class HealthCheckOptions
{
    public int IntervalSeconds { get; set; } = 30;

    public int TimeoutMs { get; set; } = 5000;

    public int FailureThreshold { get; set; } = 3;
}

public class MetricsOptions
{
    public bool EnableMetrics { get; set; } = true;

    public int CollectionIntervalSeconds { get; set; } = 60;

    public int RetentionDays { get; set; } = 30;
}

/// <summary>
/// Health check for the gateway itself
/// </summary>
public class GatewayHealthCheck : IHealthCheck
{
    private readonly IGatewayService _gatewayService;
    private readonly ILogger<GatewayHealthCheck> _logger;

    public GatewayHealthCheck(IGatewayService gatewayService, ILogger<GatewayHealthCheck> logger)
    {
        _gatewayService = gatewayService ?? throw new ArgumentNullException(nameof(gatewayService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var config = await _gatewayService.GetConfigurationAsync();

            if (config == null || !config.IsActive)
                return HealthCheckResult.Unhealthy("Gateway configuration is invalid or inactive");

            return HealthCheckResult.Healthy("Gateway is operational");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gateway health check failed");
            return HealthCheckResult.Unhealthy("Gateway health check failed", ex);
        }
    }
}

/// <summary>
/// Health check for service discovery
/// </summary>
public class ServiceDiscoveryHealthCheck : IHealthCheck
{
    private readonly IServiceDiscoveryService _discoveryService;
    private readonly ILogger<ServiceDiscoveryHealthCheck> _logger;

    public ServiceDiscoveryHealthCheck(
        IServiceDiscoveryService discoveryService,
        ILogger<ServiceDiscoveryHealthCheck> logger)
    {
        _discoveryService = discoveryService ?? throw new ArgumentNullException(nameof(discoveryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var health = await _discoveryService.GetAllServicesHealthAsync();
            var healthyCount = health.Values.Count(x => x == DotNetGrpcGateway.Services.ServiceHealthStatus.Healthy);
            var totalCount = health.Count;

            if (totalCount == 0)
                return HealthCheckResult.Healthy("No services registered");

            var healthPercentage = (double)healthyCount / totalCount * 100;

            if (healthPercentage >= 80)
                return HealthCheckResult.Healthy($"{healthPercentage}% of services are healthy");

            if (healthPercentage >= 50)
                return HealthCheckResult.Degraded($"{healthPercentage}% of services are healthy");

            return HealthCheckResult.Unhealthy($"Only {healthPercentage}% of services are healthy");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service discovery health check failed");
            return HealthCheckResult.Unhealthy("Service discovery health check failed", ex);
        }
    }
}
