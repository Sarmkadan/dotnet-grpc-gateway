#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Domain;

namespace DotNetGrpcGateway.Infrastructure;

/// <summary>
/// Repository for GatewayRoute entities
/// </summary>
public interface IRouteRepository
{
    /// <summary>
    /// Gets a gateway route by its identifier.
    /// </summary>
    /// <param name="id">The route identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the gateway route.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="id"/> is less than or equal to zero.</exception>
    Task<GatewayRoute> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all gateway routes.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of gateway routes.</returns>
    Task<List<GatewayRoute>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active gateway routes.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of active gateway routes.</returns>
    Task<List<GatewayRoute>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets gateway routes by service identifier.
    /// </summary>
    /// <param name="serviceId">The service identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of gateway routes.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="serviceId"/> is less than or equal to zero.</exception>
    Task<List<GatewayRoute>> GetByServiceIdAsync(int serviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new gateway route.
    /// </summary>
    /// <param name="route">The gateway route to create.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created gateway route.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="route"/> is null.</exception>
    Task<GatewayRoute> CreateAsync(GatewayRoute route, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing gateway route.
    /// </summary>
    /// <param name="route">The gateway route to update.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="route"/> is null.</exception>
    Task UpdateAsync(GatewayRoute route, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a gateway route by its identifier.
    /// </summary>
    /// <param name="id">The route identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="id"/> is less than or equal to zero.</exception>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets gateway routes matching a specific pattern.
    /// </summary>
    /// <param name="pattern">The pattern to search for.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of gateway routes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pattern"/> is null.</exception>
    Task<List<GatewayRoute>> GetByPatternAsync(string pattern, CancellationToken cancellationToken = default);
}

public class RouteRepository : IRouteRepository
{
    private readonly Dictionary<int, GatewayRoute> _routesById = new();
    private readonly Dictionary<int, List<GatewayRoute>> _routesByServiceId = new();
    private readonly IConnectionStringProvider _connectionProvider;

    public RouteRepository(IConnectionStringProvider connectionProvider)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
    }

    public Task<GatewayRoute> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(id, 0);
        cancellationToken.ThrowIfCancellationRequested();

        if (_routesById.TryGetValue(id, out var route))
            return Task.FromResult(route);

        throw new KeyNotFoundException($"Route with ID {id} not found");
    }

    public Task<List<GatewayRoute>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_routesById.Values.ToList());
    }

    public Task<List<GatewayRoute>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_routesById.Values
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.Priority)
            .ToList());
    }

    public Task<List<GatewayRoute>> GetByServiceIdAsync(int serviceId, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(serviceId, 0);
        cancellationToken.ThrowIfCancellationRequested();

        if (_routesByServiceId.TryGetValue(serviceId, out var routes))
            return Task.FromResult(routes);

        return Task.FromResult(new List<GatewayRoute>());
    }

    public Task<GatewayRoute> CreateAsync(GatewayRoute route, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(route);
        cancellationToken.ThrowIfCancellationRequested();

        route.Validate();

        int nextId = _routesById.Count > 0 ? _routesById.Keys.Max() + 1 : 1;
        route.Id = nextId;
        route.CreatedAt = DateTime.UtcNow;
        route.ModifiedAt = DateTime.UtcNow;

        _routesById[nextId] = route;

        if (!_routesByServiceId.ContainsKey(route.TargetServiceId))
            _routesByServiceId[route.TargetServiceId] = new List<GatewayRoute>();

        _routesByServiceId[route.TargetServiceId].Add(route);

        return Task.FromResult(route);
    }

    public Task UpdateAsync(GatewayRoute route, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(route);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_routesById.ContainsKey(route.Id))
            throw new KeyNotFoundException($"Route with ID {route.Id} not found");

        route.Validate();
        route.UpdateModifiedDate();

        var oldRoute = _routesById[route.Id];

        _routesById[route.Id] = route;

        // Update service mapping if it changed
        if (oldRoute.TargetServiceId != route.TargetServiceId)
        {
            if (_routesByServiceId.TryGetValue(oldRoute.TargetServiceId, out var oldList))
                oldList.Remove(oldRoute);

            if (!_routesByServiceId.ContainsKey(route.TargetServiceId))
                _routesByServiceId[route.TargetServiceId] = new List<GatewayRoute>();

            _routesByServiceId[route.TargetServiceId].Add(route);
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(id, 0);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_routesById.TryGetValue(id, out var route))
            throw new KeyNotFoundException($"Route with ID {id} not found");

        _routesById.Remove(id);

        if (_routesByServiceId.TryGetValue(route.TargetServiceId, out var list))
            list.Remove(route);

        return Task.CompletedTask;
    }

    public Task<List<GatewayRoute>> GetByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(_routesById.Values
            .Where(x => x.Pattern.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            .ToList());
    }
}
