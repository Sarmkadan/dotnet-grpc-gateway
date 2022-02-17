#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Domain;

namespace DotNetGrpcGateway.Infrastructure;

/// <summary>
/// Repository for <see cref="GatewayConfiguration"/> entities.
/// </summary>
public interface IGatewayRepository
{
    Task<GatewayConfiguration> GetByIdAsync(int id);
    Task<List<GatewayConfiguration>> GetAllAsync();
    Task<List<GatewayConfiguration>> GetActiveAsync();
    Task<GatewayConfiguration> CreateAsync(GatewayConfiguration config);
    Task UpdateAsync(GatewayConfiguration config);
    Task DeleteAsync(int id);
    Task<int> CountAsync();
}

public class GatewayRepository : IGatewayRepository
{
    private readonly IConnectionStringProvider _connectionProvider;
    private readonly Dictionary<int, GatewayConfiguration> _memoryStore = new();

    public GatewayRepository(IConnectionStringProvider connectionProvider)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
    }

    public Task<GatewayConfiguration> GetByIdAsync(int id)
    {
        if (_memoryStore.TryGetValue(id, out var config))
            return Task.FromResult(config);

        throw new KeyNotFoundException($"Gateway configuration with ID {id} not found");
    }

    public Task<List<GatewayConfiguration>> GetAllAsync() =>
        Task.FromResult(_memoryStore.Values.ToList());

    public Task<List<GatewayConfiguration>> GetActiveAsync() =>
        Task.FromResult(_memoryStore.Values.Where(configuration => configuration.IsActive).ToList());

    public Task<GatewayConfiguration> CreateAsync(GatewayConfiguration config)
    {
        if (config is null)
            throw new ArgumentNullException(nameof(config));

        config.Validate();

        var nextId = _memoryStore.Count > 0 ? _memoryStore.Keys.Max() + 1 : 1;
        config.Id = nextId;
        config.CreatedAt = DateTime.UtcNow;
        config.ModifiedAt = DateTime.UtcNow;

        _memoryStore[nextId] = config;
        return Task.FromResult(config);
    }

    public Task UpdateAsync(GatewayConfiguration config)
    {
        if (config is null)
            throw new ArgumentNullException(nameof(config));

        if (!_memoryStore.ContainsKey(config.Id))
            throw new KeyNotFoundException($"Gateway configuration with ID {config.Id} not found");

        config.Validate();
        config.UpdateModifiedDate();
        _memoryStore[config.Id] = config;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        if (!_memoryStore.Remove(id))
            throw new KeyNotFoundException($"Gateway configuration with ID {id} not found");

        return Task.CompletedTask;
    }

    public Task<int> CountAsync() =>
        Task.FromResult(_memoryStore.Count);
}
