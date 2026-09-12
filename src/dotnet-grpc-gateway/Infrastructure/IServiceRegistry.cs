#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Domain;

namespace DotNetGrpcGateway.Infrastructure;

/// <summary>
/// Repository for GrpcService entities - registry of available gRPC services
/// </summary>
public interface IServiceRegistry
{
    /// <summary>
    /// Gets the service with the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the service to retrieve.</param>
    /// <returns>
    /// A task whose result is the service with the specified identifier.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// No service with the specified identifier is registered.
    /// </exception>
    Task<GrpcService> GetByIdAsync(int id);
    
    /// <summary>
    /// Gets the service with the specified name.
    /// </summary>
    /// <param name="serviceName">The name of the service to retrieve.</param>
    /// <returns>
    /// A task whose result is the matching service, or <see langword="null"/> if no service is registered
    /// with the specified name.
    /// </returns>
    Task<GrpcService?> GetByNameAsync(string serviceName);
    
    /// <summary>
    /// Gets the service with the specified fully qualified name.
    /// </summary>
    /// <param name="serviceFullName">The fully qualified name of the service to retrieve.</param>
    /// <returns>
    /// A task whose result is the matching service, or <see langword="null"/> if no service is registered
    /// with the specified fully qualified name.
    /// </returns>
    Task<GrpcService?> GetByFullNameAsync(string serviceFullName);
    
    /// <summary>
    /// Gets all registered services.
    /// </summary>
    /// <returns>A task whose result contains all registered services.</returns>
    Task<List<GrpcService>> GetAllAsync();
    
    /// <summary>
    /// Gets all registered services that are active.
    /// </summary>
    /// <returns>A task whose result contains all active services.</returns>
    Task<List<GrpcService>> GetActiveAsync();
    
    /// <summary>
    /// Registers a service.
    /// </summary>
    /// <param name="service">The service to register.</param>
    /// <returns>A task whose result is the registered service.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="service"/> is invalid.</exception>
    Task<GrpcService> RegisterAsync(GrpcService service);
    
    /// <summary>
    /// Updates an existing service.
    /// </summary>
    /// <param name="service">The service to update.</param>
    /// <returns>A task that represents the update operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="KeyNotFoundException">The service is not registered.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="service"/> is invalid.</exception>
    Task UpdateAsync(GrpcService service);
    
    /// <summary>
    /// Unregisters the service with the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the service to unregister.</param>
    /// <returns>A task that represents the unregister operation.</returns>
    /// <exception cref="KeyNotFoundException">
    /// No service with the specified identifier is registered.
    /// </exception>
    Task UnregisterAsync(int id);
    
    /// <summary>
    /// Finds all registered services on the specified host.
    /// </summary>
    /// <param name="host">The host name to match.</param>
    /// <returns>A task whose result contains the services registered on the specified host.</returns>
    Task<List<GrpcService>> FindByHostAsync(string host);
}

public class ServiceRegistry : IServiceRegistry
{
    private readonly Dictionary<int, GrpcService> _servicesById = new();
    private readonly Dictionary<string, GrpcService> _servicesByName = new();
    private readonly Dictionary<string, GrpcService> _servicesByFullName = new();
    private readonly IConnectionStringProvider _connectionProvider;

    public ServiceRegistry(IConnectionStringProvider connectionProvider)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
    }

    public async Task<GrpcService> GetByIdAsync(int id)
    {
        if (_servicesById.TryGetValue(id, out var service))
            return service;

        throw new KeyNotFoundException($"Service with ID {id} not found");
    }

    public async Task<GrpcService?> GetByNameAsync(string serviceName)
    {
        _servicesByName.TryGetValue(serviceName, out var service);
        return service;
    }

    public async Task<GrpcService?> GetByFullNameAsync(string serviceFullName)
    {
        _servicesByFullName.TryGetValue(serviceFullName, out var service);
        return service;
    }

    public async Task<List<GrpcService>> GetAllAsync()
    {
        return _servicesById.Values.ToList();
    }

    public async Task<List<GrpcService>> GetActiveAsync()
    {
        return _servicesById.Values
            .Where(x => x.IsActive)
            .ToList();
    }

    public async Task<GrpcService> RegisterAsync(GrpcService service)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        service.Validate();

        int nextId = _servicesById.Count > 0 ? _servicesById.Keys.Max() + 1 : 1;
        service.Id = nextId;
        service.RegisteredAt = DateTime.UtcNow;
        service.ModifiedAt = DateTime.UtcNow;

        _servicesById[nextId] = service;
        _servicesByName[service.Name] = service;
        _servicesByFullName[service.ServiceFullName] = service;

        return service;
    }

    public async Task UpdateAsync(GrpcService service)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        if (!_servicesById.ContainsKey(service.Id))
            throw new KeyNotFoundException($"Service with ID {service.Id} not found");

        service.Validate();
        service.ModifiedAt = DateTime.UtcNow;

        _servicesById[service.Id] = service;
        _servicesByName[service.Name] = service;
        _servicesByFullName[service.ServiceFullName] = service;
    }

    public async Task UnregisterAsync(int id)
    {
        if (!_servicesById.TryGetValue(id, out var service))
            throw new KeyNotFoundException($"Service with ID {id} not found");

        _servicesById.Remove(id);
        _servicesByName.Remove(service.Name);
        _servicesByFullName.Remove(service.ServiceFullName);
    }

    public async Task<List<GrpcService>> FindByHostAsync(string host)
    {
        return _servicesById.Values
            .Where(x => x.Host == host)
            .ToList();
    }
}
