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
    Task<GrpcService> GetByIdAsync(int id);
    Task<GrpcService?> GetByNameAsync(string serviceName);
    Task<GrpcService?> GetByFullNameAsync(string serviceFullName);
    Task<List<GrpcService>> GetAllAsync();
    Task<List<GrpcService>> GetActiveAsync();
    Task<GrpcService> RegisterAsync(GrpcService service);
    Task UpdateAsync(GrpcService service);
    Task UnregisterAsync(int id);
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
        if (service == null)
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
        if (service == null)
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
