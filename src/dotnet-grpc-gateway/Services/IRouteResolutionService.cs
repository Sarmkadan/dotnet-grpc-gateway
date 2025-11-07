// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Exceptions;

namespace DotNetGrpcGateway.Services;

/// <summary>
/// Resolves incoming requests to target gRPC services based on routes
/// </summary>
public interface IRouteResolutionService
{
    Task<GatewayRoute> ResolveRouteAsync(string serviceName, string methodName);
    Task<GrpcService> ResolveTargetServiceAsync(int serviceId);
    Task<GatewayRoute?> FindMatchingRouteAsync(string serviceName, string methodName);
    Task<List<GatewayRoute>> GetRoutesForServiceAsync(int serviceId);
    Task ValidateRouteAccessAsync(GatewayRoute route, string? clientId = null);
}

public class RouteResolutionService : IRouteResolutionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RouteResolutionService> _logger;
    private readonly Dictionary<string, (GatewayRoute Route, DateTime CachedAt)> _routeCache = new();
    private const int RouteCacheDurationSeconds = 60;

    public RouteResolutionService(IUnitOfWork unitOfWork, ILogger<RouteResolutionService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GatewayRoute> ResolveRouteAsync(string serviceName, string methodName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
            throw new ArgumentException("Service name is required", nameof(serviceName));

        if (string.IsNullOrWhiteSpace(methodName))
            throw new ArgumentException("Method name is required", nameof(methodName));

        var cacheKey = $"{serviceName}:{methodName}";

        // Check cache
        if (_routeCache.TryGetValue(cacheKey, out var cached))
        {
            if ((DateTime.UtcNow - cached.CachedAt).TotalSeconds < RouteCacheDurationSeconds)
            {
                _logger.LogDebug("Route resolved from cache: {CacheKey}", cacheKey);
                return cached.Route;
            }

            _routeCache.Remove(cacheKey);
        }

        var route = await FindMatchingRouteAsync(serviceName, methodName);
        if (route == null)
        {
            throw new RouteResolutionException(
                $"{serviceName}.{methodName}",
                "No matching route found");
        }

        _routeCache[cacheKey] = (route, DateTime.UtcNow);

        _logger.LogInformation(
            "Route resolved for {Service}.{Method} -> Service {ServiceId}",
            serviceName,
            methodName,
            route.TargetServiceId);

        return route;
    }

    public async Task<GrpcService> ResolveTargetServiceAsync(int serviceId)
    {
        var service = await _unitOfWork.Services.GetByIdAsync(serviceId);
        if (service == null)
            throw new ServiceNotFoundException($"Service with ID {serviceId}");

        if (!service.IsActive)
            throw new ServiceUnavailableException(service.Name, "Service is inactive");

        if (!service.IsHealthy)
            throw new ServiceUnavailableException(service.Name, "Service health check failed");

        return service;
    }

    public async Task<GatewayRoute?> FindMatchingRouteAsync(string serviceName, string methodName)
    {
        var routes = await _unitOfWork.Routes.GetActiveAsync();

        // Sort by priority (highest first) and find first match
        var matchingRoute = routes
            .OrderByDescending(x => x.Priority)
            .FirstOrDefault(x => x.MatchesRequest(serviceName, methodName));

        return matchingRoute;
    }

    public async Task<List<GatewayRoute>> GetRoutesForServiceAsync(int serviceId)
    {
        return await _unitOfWork.Routes.GetByServiceIdAsync(serviceId);
    }

    public async Task ValidateRouteAccessAsync(GatewayRoute route, string? clientId = null)
    {
        if (route == null)
            throw new ArgumentNullException(nameof(route));

        if (route.RequiresAuthentication && string.IsNullOrWhiteSpace(clientId))
        {
            throw new AuthenticationException("Route requires authentication but no client ID provided");
        }

        if (!string.IsNullOrWhiteSpace(route.AuthorizationPolicy) && !string.IsNullOrWhiteSpace(clientId))
        {
            // In real implementation, would check authorization policy
            _logger.LogDebug(
                "Validating authorization policy {Policy} for client {ClientId}",
                route.AuthorizationPolicy,
                clientId);
        }
    }

    public void ClearRouteCache()
    {
        _routeCache.Clear();
        _logger.LogInformation("Route cache cleared");
    }

    public int GetCachedRouteCount() => _routeCache.Count;
}
