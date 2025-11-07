// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Events;
using DotNetGrpcGateway.Utilities;

namespace DotNetGrpcGateway.Services;

/// <summary>
/// Service for advanced route management operations.
/// Handles route validation, priority management, and route analysis.
/// </summary>
public interface IRouteManagementService
{
    Task<List<GatewayRoute>> GetRoutesByServiceAsync(int serviceId);
    Task<GatewayRoute?> FindMatchingRouteAsync(string path);
    Task<List<GatewayRoute>> GetConflictingRoutesAsync(string pattern);
    Task<bool> ValidateRouteAsync(GatewayRoute route);
}

/// <summary>
/// Implementation of route management service.
/// </summary>
public class RouteManagementService : IRouteManagementService
{
    private readonly IRouteRepository _routeRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<RouteManagementService> _logger;

    public RouteManagementService(
        IRouteRepository routeRepository,
        IEventPublisher eventPublisher,
        ILogger<RouteManagementService> logger)
    {
        _routeRepository = routeRepository ?? throw new ArgumentNullException(nameof(routeRepository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<GatewayRoute>> GetRoutesByServiceAsync(int serviceId)
    {
        try
        {
            var routes = await _routeRepository.GetAllAsync();
            return routes.Where(r => r.TargetServiceId == serviceId).OrderByDescending(r => r.Priority).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving routes for service {ServiceId}", serviceId);
            return new List<GatewayRoute>();
        }
    }

    public async Task<GatewayRoute?> FindMatchingRouteAsync(string path)
    {
        if (string.IsNullOrEmpty(path))
            return null;

        try
        {
            var allRoutes = await _routeRepository.GetAllAsync();

            // Find routes that match the path, ordered by priority
            var matchingRoute = allRoutes
                .Where(r => r.IsActive && StringUtility.MatchesWildcardPattern(path, r.Pattern))
                .OrderByDescending(r => r.Priority)
                .FirstOrDefault();

            if (matchingRoute != null)
                _logger.LogDebug("Found matching route for path {Path}: {Pattern}",
                    StringUtility.MaskSensitiveData(path), matchingRoute.Pattern);

            return matchingRoute;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding matching route for path {Path}",
                StringUtility.MaskSensitiveData(path));
            return null;
        }
    }

    public async Task<List<GatewayRoute>> GetConflictingRoutesAsync(string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            return new List<GatewayRoute>();

        try
        {
            var allRoutes = await _routeRepository.GetAllAsync();

            // Find routes with overlapping patterns
            var conflicting = allRoutes.Where(r =>
                r.Pattern != pattern &&
                (StringUtility.MatchesWildcardPattern(pattern, r.Pattern) ||
                 StringUtility.MatchesWildcardPattern(r.Pattern, pattern)))
                .ToList();

            _logger.LogDebug("Found {Count} conflicting routes for pattern {Pattern}",
                conflicting.Count, StringUtility.MaskSensitiveData(pattern));

            return conflicting;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding conflicting routes for pattern {Pattern}",
                StringUtility.MaskSensitiveData(pattern));
            return new List<GatewayRoute>();
        }
    }

    public async Task<bool> ValidateRouteAsync(GatewayRoute route)
    {
        try
        {
            // Validate pattern
            if (string.IsNullOrWhiteSpace(route.Pattern))
            {
                _logger.LogWarning("Route pattern is empty");
                return false;
            }

            // Validate priority
            if (route.Priority < 0 || route.Priority > 1000)
            {
                _logger.LogWarning("Route priority {Priority} is out of valid range", route.Priority);
                return false;
            }

            // Validate rate limit
            if (route.RateLimitPerMinute < 0)
            {
                _logger.LogWarning("Route rate limit cannot be negative");
                return false;
            }

            // Validate cache duration
            if (route.EnableCaching && route.CacheDurationSeconds < 0)
            {
                _logger.LogWarning("Cache duration cannot be negative");
                return false;
            }

            // Check for duplicate patterns
            var allRoutes = await _routeRepository.GetAllAsync();
            var duplicate = allRoutes.FirstOrDefault(r => r.Id != route.Id && r.Pattern == route.Pattern);

            if (duplicate != null)
            {
                _logger.LogWarning("Route pattern {Pattern} already exists (ID: {DuplicateId})",
                    StringUtility.MaskSensitiveData(route.Pattern), duplicate.Id);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating route");
            return false;
        }
    }
}
