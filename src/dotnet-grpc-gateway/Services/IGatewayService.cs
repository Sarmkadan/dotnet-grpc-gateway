// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Exceptions;

namespace DotNetGrpcGateway.Services;

/// <summary>
/// Core gateway service managing configuration, service registration, and routing
/// </summary>
public interface IGatewayService
{
    Task<GatewayConfiguration> GetConfigurationAsync();
    Task<GatewayConfiguration> UpdateConfigurationAsync(GatewayConfiguration config);
    Task RegisterServiceAsync(GrpcService service);
    Task UnregisterServiceAsync(int serviceId);
    Task<GrpcService> GetServiceAsync(int serviceId);
    Task<List<GrpcService>> GetAllServicesAsync();
    Task<List<GrpcService>> GetHealthyServicesAsync();
    Task<GatewayRoute> AddRouteAsync(GatewayRoute route);
    Task RemoveRouteAsync(int routeId);
    Task<List<GatewayRoute>> GetAllRoutesAsync();
}

public class GatewayService : IGatewayService
{
    private readonly IUnitOfWork _unitOfWork;
    private GatewayConfiguration? _currentConfig;

    public GatewayService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<GatewayConfiguration> GetConfigurationAsync()
    {
        if (_currentConfig != null)
            return _currentConfig;

        var configs = await _unitOfWork.Gateways.GetAllAsync();

        if (configs.Count == 0)
        {
            var defaultConfig = new GatewayConfiguration
            {
                Name = "Default Gateway",
                Description = "Default gRPC-Web Gateway Configuration",
                ListenAddress = "0.0.0.0",
                Port = 5000,
                EnableReflection = true,
                EnableMetrics = true
            };

            _currentConfig = await _unitOfWork.Gateways.CreateAsync(defaultConfig);
        }
        else
        {
            _currentConfig = configs.First(x => x.IsActive) ?? configs[0];
        }

        return _currentConfig;
    }

    public async Task<GatewayConfiguration> UpdateConfigurationAsync(GatewayConfiguration config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        try
        {
            config.Validate();
            await _unitOfWork.UpdateAsync(config);
            _currentConfig = config;
            return config;
        }
        catch (InvalidOperationException ex)
        {
            throw new ConfigurationException("GatewayConfiguration", ex.Message);
        }
    }

    public async Task RegisterServiceAsync(GrpcService service)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));

        try
        {
            service.Validate();

            var existing = await _unitOfWork.Services.GetByNameAsync(service.Name);
            if (existing != null)
                throw new InvalidOperationException($"Service '{service.Name}' already registered");

            await _unitOfWork.Services.RegisterAsync(service);
        }
        catch (InvalidOperationException ex)
        {
            throw new ConfigurationException("GrpcService", ex.Message);
        }
    }

    public async Task UnregisterServiceAsync(int serviceId)
    {
        var service = await _unitOfWork.Services.GetByIdAsync(serviceId);
        if (service == null)
            throw new ServiceNotFoundException("unknown");

        // Remove all routes associated with this service
        var routes = await _unitOfWork.Routes.GetByServiceIdAsync(serviceId);
        foreach (var route in routes)
        {
            await _unitOfWork.Routes.DeleteAsync(route.Id);
        }

        await _unitOfWork.Services.UnregisterAsync(serviceId);
    }

    public async Task<GrpcService> GetServiceAsync(int serviceId)
    {
        return await _unitOfWork.Services.GetByIdAsync(serviceId);
    }

    public async Task<List<GrpcService>> GetAllServicesAsync()
    {
        return await _unitOfWork.Services.GetAllAsync();
    }

    public async Task<List<GrpcService>> GetHealthyServicesAsync()
    {
        var services = await _unitOfWork.Services.GetAllAsync();
        return services.Where(x => x.IsHealthy && x.IsActive).ToList();
    }

    public async Task<GatewayRoute> AddRouteAsync(GatewayRoute route)
    {
        if (route == null)
            throw new ArgumentNullException(nameof(route));

        try
        {
            route.Validate();

            // Verify service exists
            var service = await _unitOfWork.Services.GetByIdAsync(route.TargetServiceId);
            if (service == null)
                throw new ServiceNotFoundException("unknown");

            return await _unitOfWork.Routes.CreateAsync(route);
        }
        catch (InvalidOperationException ex)
        {
            throw new ConfigurationException("GatewayRoute", ex.Message);
        }
    }

    public async Task RemoveRouteAsync(int routeId)
    {
        await _unitOfWork.Routes.DeleteAsync(routeId);
    }

    public async Task<List<GatewayRoute>> GetAllRoutesAsync()
    {
        return await _unitOfWork.Routes.GetActiveAsync();
    }
}
