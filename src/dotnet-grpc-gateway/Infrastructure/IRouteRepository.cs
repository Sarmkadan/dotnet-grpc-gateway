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
    Task<GatewayRoute> GetByIdAsync(int id);
    Task<List<GatewayRoute>> GetAllAsync();
    Task<List<GatewayRoute>> GetActiveAsync();
    Task<List<GatewayRoute>> GetByServiceIdAsync(int serviceId);
    Task<GatewayRoute> CreateAsync(GatewayRoute route);
    Task UpdateAsync(GatewayRoute route);
    Task DeleteAsync(int id);
    Task<List<GatewayRoute>> GetByPatternAsync(string pattern);
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

    public async Task<GatewayRoute> GetByIdAsync(int id)
    {
        if (_routesById.TryGetValue(id, out var route))
            return route;

        throw new KeyNotFoundException($"Route with ID {id} not found");
    }

    public async Task<List<GatewayRoute>> GetAllAsync()
    {
        return _routesById.Values.ToList();
    }

    public async Task<List<GatewayRoute>> GetActiveAsync()
    {
        return _routesById.Values
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.Priority)
            .ToList();
    }

    public async Task<List<GatewayRoute>> GetByServiceIdAsync(int serviceId)
    {
        if (_routesByServiceId.TryGetValue(serviceId, out var routes))
            return routes;

        return new List<GatewayRoute>();
    }

    public async Task<GatewayRoute> CreateAsync(GatewayRoute route)
    {
        if (route == null)
            throw new ArgumentNullException(nameof(route));

        route.Validate();

        int nextId = _routesById.Count > 0 ? _routesById.Keys.Max() + 1 : 1;
        route.Id = nextId;
        route.CreatedAt = DateTime.UtcNow;
        route.ModifiedAt = DateTime.UtcNow;

        _routesById[nextId] = route;

        if (!_routesByServiceId.ContainsKey(route.TargetServiceId))
            _routesByServiceId[route.TargetServiceId] = new List<GatewayRoute>();

        _routesByServiceId[route.TargetServiceId].Add(route);

        return route;
    }

    public async Task UpdateAsync(GatewayRoute route)
    {
        if (route == null)
            throw new ArgumentNullException(nameof(route));

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
    }

    public async Task DeleteAsync(int id)
    {
        if (!_routesById.TryGetValue(id, out var route))
            throw new KeyNotFoundException($"Route with ID {id} not found");

        _routesById.Remove(id);

        if (_routesByServiceId.TryGetValue(route.TargetServiceId, out var list))
            list.Remove(route);
    }

    public async Task<List<GatewayRoute>> GetByPatternAsync(string pattern)
    {
        return _routesById.Values
            .Where(x => x.Pattern.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
