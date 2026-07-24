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
    private readonly IRetryPolicy _retryPolicy;
    private readonly Dictionary<int, GatewayConfiguration> _memoryStore = new();

    public GatewayRepository(IConnectionStringProvider connectionProvider, IRetryPolicy retryPolicy)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        _retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
    }

    public Task<GatewayConfiguration> GetByIdAsync(int id) =>
        _retryPolicy.ExecuteAsync(_ =>
        {
            if (_memoryStore.TryGetValue(id, out var config))
                return Task.FromResult(config);

            throw new KeyNotFoundException($"Gateway configuration with ID {id} not found");
        }, nameof(GetByIdAsync));

    public Task<List<GatewayConfiguration>> GetAllAsync() =>
        _retryPolicy.ExecuteAsync(_ => Task.FromResult(_memoryStore.Values.ToList()), nameof(GetAllAsync));

    public Task<List<GatewayConfiguration>> GetActiveAsync() =>
        _retryPolicy.ExecuteAsync(
            _ => Task.FromResult(_memoryStore.Values.Where(configuration => configuration.IsActive).ToList()),
            nameof(GetActiveAsync));

    public Task<GatewayConfiguration> CreateAsync(GatewayConfiguration config)
    {
        if (config is null)
            throw new ArgumentNullException(nameof(config));

        config.Validate();

        return _retryPolicy.ExecuteAsync(_ =>
        {
            var nextId = _memoryStore.Count > 0 ? _memoryStore.Keys.Max() + 1 : 1;
            config.Id = nextId;
            config.CreatedAt = DateTime.UtcNow;
            config.ModifiedAt = DateTime.UtcNow;

            _memoryStore[nextId] = config;
            return Task.FromResult(config);
        }, nameof(CreateAsync));
    }

    public Task UpdateAsync(GatewayConfiguration config)
    {
        if (config is null)
            throw new ArgumentNullException(nameof(config));

        config.Validate();

        return _retryPolicy.ExecuteAsync(_ =>
        {
            if (!_memoryStore.ContainsKey(config.Id))
                throw new KeyNotFoundException($"Gateway configuration with ID {config.Id} not found");

            config.UpdateModifiedDate();
            _memoryStore[config.Id] = config;
            return Task.CompletedTask;
        }, nameof(UpdateAsync));
    }

    public Task DeleteAsync(int id) =>
        _retryPolicy.ExecuteAsync(_ =>
        {
            if (!_memoryStore.Remove(id))
                throw new KeyNotFoundException($"Gateway configuration with ID {id} not found");

            return Task.CompletedTask;
        }, nameof(DeleteAsync));

    public Task<int> CountAsync() =>
        _retryPolicy.ExecuteAsync(_ => Task.FromResult(_memoryStore.Count), nameof(CountAsync));
}
