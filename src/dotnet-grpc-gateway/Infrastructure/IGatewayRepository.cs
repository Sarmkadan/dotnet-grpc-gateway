#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Exceptions;

namespace DotNetGrpcGateway.Infrastructure;

/// <summary>
/// Repository for <see cref="GatewayConfiguration"/> entities.
/// </summary>
public interface IGatewayRepository
{
    /// <summary>
    /// Gets a gateway configuration by its identifier.
    /// </summary>
    /// <param name="id">The configuration identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the gateway configuration.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="id"/> is less than or equal to zero.</exception>
    /// <exception cref="NotFoundException">Thrown when a gateway configuration with the specified <paramref name="id"/> is not found.</exception>
    Task<GatewayConfiguration> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all gateway configurations.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of gateway configurations.</returns>
    Task<List<GatewayConfiguration>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active gateway configurations.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of active gateway configurations.</returns>
    Task<List<GatewayConfiguration>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new gateway configuration.
    /// </summary>
    /// <param name="config">The gateway configuration to create.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created gateway configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
    Task<GatewayConfiguration> CreateAsync(GatewayConfiguration config, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing gateway configuration.
    /// </summary>
    /// <param name="config">The gateway configuration to update.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
    /// <exception cref="NotFoundException">Thrown when a gateway configuration with the specified identifier is not found.</exception>
    Task UpdateAsync(GatewayConfiguration config, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a gateway configuration by its identifier.
    /// </summary>
    /// <param name="id">The configuration identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="id"/> is less than or equal to zero.</exception>
    /// <exception cref="NotFoundException">Thrown when a gateway configuration with the specified <paramref name="id"/> is not found.</exception>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of gateway configurations.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the count of gateway configurations.</returns>
    Task<int> CountAsync(CancellationToken cancellationToken = default);
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

    public Task<GatewayConfiguration> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(id, 0);
        cancellationToken.ThrowIfCancellationRequested();

        return _retryPolicy.ExecuteAsync(_ =>
        {
            if (_memoryStore.TryGetValue(id, out var config))
                return Task.FromResult(config);

            throw new NotFoundException(nameof(GatewayConfiguration), id);
        }, nameof(GetByIdAsync));
    }

    public Task<List<GatewayConfiguration>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _retryPolicy.ExecuteAsync(_ => Task.FromResult(_memoryStore.Values.ToList()), nameof(GetAllAsync));
    }

    public Task<List<GatewayConfiguration>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _retryPolicy.ExecuteAsync(
            _ => Task.FromResult(_memoryStore.Values.Where(configuration => configuration.IsActive).ToList()),
            nameof(GetActiveAsync));
    }

    public Task<GatewayConfiguration> CreateAsync(GatewayConfiguration config, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(config);
        cancellationToken.ThrowIfCancellationRequested();

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

    public Task UpdateAsync(GatewayConfiguration config, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(config);
        cancellationToken.ThrowIfCancellationRequested();

        config.Validate();

        return _retryPolicy.ExecuteAsync(_ =>
        {
            if (!_memoryStore.ContainsKey(config.Id))
                throw new NotFoundException(nameof(GatewayConfiguration), config.Id);

            config.UpdateModifiedDate();
            _memoryStore[config.Id] = config;
            return Task.CompletedTask;
        }, nameof(UpdateAsync));
    }

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(id, 0);
        cancellationToken.ThrowIfCancellationRequested();

        return _retryPolicy.ExecuteAsync(_ =>
        {
            if (!_memoryStore.Remove(id))
                throw new NotFoundException(nameof(GatewayConfiguration), id);

            return Task.CompletedTask;
        }, nameof(DeleteAsync));
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _retryPolicy.ExecuteAsync(_ => Task.FromResult(_memoryStore.Count), nameof(CountAsync));
    }
}