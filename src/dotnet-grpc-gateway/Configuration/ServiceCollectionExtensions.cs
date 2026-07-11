#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using DotNetGrpcGateway.Infrastructure;
using DotNetGrpcGateway.Services;
using DotNetGrpcGateway.Options;
using Microsoft.Extensions.Options;

namespace DotNetGrpcGateway.Configuration;

/// <summary>
/// Extension methods for configuring gateway services and dependencies
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all gateway services to the dependency injection container
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null</exception>
    public static IServiceCollection AddGatewayServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

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
    /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
    /// <param name="configuration">The application configuration</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="configuration"/> is null</exception>
    public static IServiceCollection AddGatewayConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<DotnetGrpcGatewayOptions>()
            .Bind(configuration.GetSection(DotnetGrpcGatewayOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    /// <summary>
    /// Adds gateway health checks
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add health checks to</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null</exception>
    public static IServiceCollection AddGatewayHealthChecks(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHealthChecks()
            .AddCheck<GatewayHealthCheck>("gateway")
            .AddCheck<ServiceDiscoveryHealthCheck>("service-discovery");

        return services;
    }

    /// <summary>
    /// Adds gRPC Server Reflection discovery support and its associated health check.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add reflection services to</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null</exception>
    public static IServiceCollection AddGatewayReflection(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpClient<IReflectionService, ReflectionService>();
        services.AddHealthChecks().AddCheck<ReflectionHealthCheck>("reflection");

        return services;
    }
}

/// <summary>
/// Health check for the gateway itself
/// </summary>
public sealed class GatewayHealthCheck : IHealthCheck
{
    private readonly IGatewayService _gatewayService;
    private readonly ILogger<GatewayHealthCheck> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GatewayHealthCheck"/> class
    /// </summary>
    /// <param name="gatewayService">The gateway service to check</param>
    /// <param name="logger">The logger</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="gatewayService"/> or <paramref name="logger"/> is null</exception>
    public GatewayHealthCheck(
        IGatewayService gatewayService,
        ILogger<GatewayHealthCheck> logger)
    {
        _gatewayService = gatewayService ?? throw new ArgumentNullException(nameof(gatewayService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Runs the health check
    /// </summary>
    /// <param name="context">The health check context</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The health check result</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var config = await _gatewayService.GetConfigurationAsync();

            if (config is null || !config.IsActive)
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
/// Health check for the gRPC Server Reflection subsystem. Reports Healthy when at least
/// one registered service has cached reflection data available, Degraded when only a
/// subset does, and Unhealthy when no services respond to the reflection probe.
/// </summary>
public sealed class ReflectionHealthCheck : IHealthCheck
{
    private readonly IReflectionService _reflectionService;
    private readonly ILogger<ReflectionHealthCheck> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReflectionHealthCheck"/> class
    /// </summary>
    /// <param name="reflectionService">The reflection service to check</param>
    /// <param name="logger">The logger</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="reflectionService"/> or <paramref name="logger"/> is null</exception>
    public ReflectionHealthCheck(
        IReflectionService reflectionService,
        ILogger<ReflectionHealthCheck> logger)
    {
        _reflectionService = reflectionService ?? throw new ArgumentNullException(nameof(reflectionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Runs the health check
    /// </summary>
    /// <param name="context">The health check context</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The health check result</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var allInfo = await _reflectionService.GetAllReflectionInfoAsync(cancellationToken);
            var total = allInfo.Count;

            if (total == 0)
                return HealthCheckResult.Healthy("No services registered; reflection subsystem is idle");

            var available = allInfo.Count(i => i.IsAvailable);
            var pct = (double)available / total * 100;

            return available switch
            {
                var x when x == total => HealthCheckResult.Healthy($"Reflection available on all {total} service(s)"),
                var x when x > 0 => HealthCheckResult.Degraded($"Reflection available on {available}/{total} service(s) ({pct:F0}%)"),
                _ => HealthCheckResult.Unhealthy($"Reflection unreachable on all {total} registered service(s)")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Reflection health check failed");
            return HealthCheckResult.Unhealthy("Reflection health check failed", ex);
        }
    }
}

/// <summary>
/// Health check for service discovery
/// </summary>
public sealed class ServiceDiscoveryHealthCheck : IHealthCheck
{
    private readonly IServiceDiscoveryService _discoveryService;
    private readonly ILogger<ServiceDiscoveryHealthCheck> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceDiscoveryHealthCheck"/> class
    /// </summary>
    /// <param name="discoveryService">The service discovery service to check</param>
    /// <param name="logger">The logger</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="discoveryService"/> or <paramref name="logger"/> is null</exception>
    public ServiceDiscoveryHealthCheck(
        IServiceDiscoveryService discoveryService,
        ILogger<ServiceDiscoveryHealthCheck> logger)
    {
        _discoveryService = discoveryService ?? throw new ArgumentNullException(nameof(discoveryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Runs the health check
    /// </summary>
    /// <param name="context">The health check context</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The health check result</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var health = await _discoveryService.GetAllServicesHealthAsync();
            var healthyCount = health.Values.Count(x => x == DotNetGrpcGateway.Services.ServiceHealthStatus.Healthy);
            var totalCount = health.Count;

            return totalCount switch
            {
                0 => HealthCheckResult.Healthy("No services registered"),
                var x when healthyCount >= x * 0.8 => HealthCheckResult.Healthy($"{Math.Round((double)healthyCount / totalCount * 100)}% of services are healthy"),
                var x when healthyCount >= x * 0.5 => HealthCheckResult.Degraded($"{Math.Round((double)healthyCount / totalCount * 100)}% of services are healthy"),
                _ => HealthCheckResult.Unhealthy($"Only {Math.Round((double)healthyCount / totalCount * 100)}% of services are healthy")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service discovery health check failed");
            return HealthCheckResult.Unhealthy("Service discovery health check failed", ex);
        }
    }
}