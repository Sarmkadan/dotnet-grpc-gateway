#nullable enable
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
    /// <summary>
    /// Gets the current gateway configuration.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="GatewayConfiguration"/>.</returns>
    Task<GatewayConfiguration> GetConfigurationAsync();

    /// <summary>
    /// Updates the gateway configuration.
    /// </summary>
    /// <param name="config">The new configuration.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated <see cref="GatewayConfiguration"/>.</returns>
    Task<GatewayConfiguration> UpdateConfigurationAsync(GatewayConfiguration config);

    /// <summary>
    /// Registers a new gRPC service.
    /// </summary>
    /// <param name="service">The service to register.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RegisterServiceAsync(GrpcService service);

    /// <summary>
    /// Unregisters a gRPC service by ID.
    /// </summary>
    /// <param name="serviceId">The ID of the service.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UnregisterServiceAsync(int serviceId);

    /// <summary>
    /// Gets a gRPC service by ID.
    /// </summary>
    /// <param name="serviceId">The ID of the service.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="GrpcService"/>.</returns>
    Task<GrpcService> GetServiceAsync(int serviceId);

    /// <summary>
    /// Gets all registered gRPC services.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="GrpcService"/>.</returns>
    Task<List<GrpcService>> GetAllServicesAsync();

    /// <summary>
    /// Gets all healthy gRPC services.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of healthy <see cref="GrpcService"/>.</returns>
    Task<List<GrpcService>> GetHealthyServicesAsync();

    /// <summary>
    /// Adds a new routing rule.
    /// </summary>
    /// <param name="route">The route to add.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the added <see cref="GatewayRoute"/>.</returns>
    Task<GatewayRoute> AddRouteAsync(GatewayRoute route);

    /// <summary>
    /// Removes a routing rule by ID.
    /// </summary>
    /// <param name="routeId">The ID of the route.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RemoveRouteAsync(int routeId);

    /// <summary>
    /// Gets all active routing rules.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of active <see cref="GatewayRoute"/>.</returns>
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
        try
        {
            if (_currentConfig is not null)
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
        catch (Exception ex)
        {
            throw new ConfigurationException("GetConfiguration", ex.Message);
        }
    }

    public async Task<GatewayConfiguration> UpdateConfigurationAsync(GatewayConfiguration config)
    {
        if (config is null)
            throw new ArgumentNullException(nameof(config));

        try
        {
            config.Validate();
            await _unitOfWork.Gateways.UpdateAsync(config);
            _currentConfig = config;
            return config;
        }
        catch (InvalidOperationException ex)
        {
            throw new ConfigurationException("UpdateConfiguration", ex.Message);
        }
        catch (Exception ex)
        {
            throw new ConfigurationException("UpdateConfiguration", ex.Message);
        }
    }

    public async Task RegisterServiceAsync(GrpcService service)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        try
        {
            service.Validate();

            var existing = await _unitOfWork.Services.GetByNameAsync(service.Name);
            if (existing is not null)
                throw new InvalidOperationException($"Service '{service.Name}' already registered");

            await _unitOfWork.Services.RegisterAsync(service);
        }
        catch (InvalidOperationException ex)
        {
            throw new ConfigurationException("RegisterService", ex.Message);
        }
        catch (Exception ex)
        {
            throw new ConfigurationException("RegisterService", ex.Message);
        }
    }

    public async Task UnregisterServiceAsync(int serviceId)
    {
        if (serviceId <= 0)
            throw new ArgumentOutOfRangeException(nameof(serviceId), "Service ID must be greater than zero.");

        try
        {
            var service = await _unitOfWork.Services.GetByIdAsync(serviceId);
            if (service is null)
                throw new ServiceNotFoundException("unknown");

            // Remove all routes associated with this service
            var routes = await _unitOfWork.Routes.GetByServiceIdAsync(serviceId);
            foreach (var route in routes)
            {
                await _unitOfWork.Routes.DeleteAsync(route.Id);
            }

            await _unitOfWork.Services.UnregisterAsync(serviceId);
        }
        catch (ServiceNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ConfigurationException("UnregisterService", ex.Message);
        }
    }

    public async Task<GrpcService> GetServiceAsync(int serviceId)
    {
        if (serviceId <= 0)
            throw new ArgumentOutOfRangeException(nameof(serviceId), "Service ID must be greater than zero.");

        try
        {
            return await _unitOfWork.Services.GetByIdAsync(serviceId);
        }
        catch (Exception ex)
        {
            throw new ConfigurationException("GetService", ex.Message);
        }
    }

    public async Task<List<GrpcService>> GetAllServicesAsync()
    {
        try
        {
            return await _unitOfWork.Services.GetAllAsync();
        }
        catch (Exception ex)
        {
            throw new ConfigurationException("GetAllServices", ex.Message);
        }
    }

    public async Task<List<GrpcService>> GetHealthyServicesAsync()
    {
        try
        {
            var services = await _unitOfWork.Services.GetAllAsync();
            return services.Where(x => x.IsHealthy && x.IsActive).ToList();
        }
        catch (Exception ex)
        {
            throw new ConfigurationException("GetHealthyServices", ex.Message);
        }
    }

    public async Task<GatewayRoute> AddRouteAsync(GatewayRoute route)
    {
        if (route is null)
            throw new ArgumentNullException(nameof(route));

        try
        {
            route.Validate();

            // Verify service exists
            var service = await _unitOfWork.Services.GetByIdAsync(route.TargetServiceId);
            if (service is null)
                throw new ServiceNotFoundException("unknown");

            return await _unitOfWork.Routes.CreateAsync(route);
        }
        catch (InvalidOperationException ex)
        {
            throw new ConfigurationException("AddRoute", ex.Message);
        }
        catch (Exception ex)
        {
            throw new ConfigurationException("AddRoute", ex.Message);
        }
    }

    public async Task RemoveRouteAsync(int routeId)
    {
        if (routeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(routeId), "Route ID must be greater than zero.");

        try
        {
            await _unitOfWork.Routes.DeleteAsync(routeId);
        }
        catch (Exception ex)
        {
            throw new ConfigurationException("RemoveRoute", ex.Message);
        }
    }

    public async Task<List<GatewayRoute>> GetAllRoutesAsync()
    {
        try
        {
            return await _unitOfWork.Routes.GetActiveAsync();
        }
        catch (Exception ex)
        {
            throw new ConfigurationException("GetAllRoutes", ex.Message);
        }
    }
}
