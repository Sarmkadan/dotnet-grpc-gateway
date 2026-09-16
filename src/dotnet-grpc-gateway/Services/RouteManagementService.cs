#nullable enable
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
    Task<List<GatewayRoute>> GetRoutesByServiceAsync(int serviceId, CancellationToken cancellationToken = default);
    Task<GatewayRoute?> FindMatchingRouteAsync(string path, CancellationToken cancellationToken = default);
    Task<List<GatewayRoute>> GetConflictingRoutesAsync(string pattern, CancellationToken cancellationToken = default);
    Task<bool> ValidateRouteAsync(GatewayRoute route, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of route management service.
/// </summary>
public class RouteManagementService : IRouteManagementService
{
    private const int MinimumPriority = 0;
    private const int MaximumPriority = 1000;
    private const int MinimumRateLimitPerMinute = 0;
    private const int MinimumCacheDurationSeconds = 0;

    private const string RoutesRetrievalErrorMessage = "Error retrieving routes for service {ServiceId}";
    private const string MatchingRouteFoundMessage = "Found matching route for path {Path}: {Pattern}";
    private const string MatchingRouteErrorMessage = "Error finding matching route for path {Path}";
    private const string ConflictingRoutesFoundMessage = "Found {Count} conflicting routes for pattern {Pattern}";
    private const string ConflictingRoutesErrorMessage = "Error finding conflicting routes for pattern {Pattern}";
    private const string EmptyPatternWarningMessage = "Route validation failed - Pattern is empty (RouteId: {RouteId})";
    private const string InvalidPriorityWarningMessage = "Route validation failed - Priority {Priority} is out of valid range (RouteId: {RouteId})";
    private const string NegativeRateLimitWarningMessage = "Route validation failed - Rate limit cannot be negative (RouteId: {RouteId})";
    private const string NegativeCacheDurationWarningMessage = "Route validation failed - Cache duration cannot be negative (RouteId: {RouteId})";
    private const string DuplicatePatternWarningMessage = "Route pattern {Pattern} already exists (ID: {DuplicateId})";
    private const string RouteValidationErrorMessage = "Error validating route";

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

    /// <summary>
    /// Retrieves all routes associated with a specific service, ordered by priority in descending order.
    /// </summary>
    /// <param name="serviceId">The ID of the service to retrieve routes for.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of GatewayRoute objects for the specified service, ordered by priority (highest first).
    /// Returns an empty list if no routes are found or an error occurs.</returns>
    public async Task<List<GatewayRoute>> GetRoutesByServiceAsync(int serviceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var routes = await _routeRepository.GetAllAsync(cancellationToken);
            return routes.Where(r => r.TargetServiceId == serviceId).OrderByDescending(r => r.Priority).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, RoutesRetrievalErrorMessage, serviceId);
            return new List<GatewayRoute>();
        }
    }

    /// <summary>
    /// Finds the highest-priority active route whose pattern matches the specified path.
    /// </summary>
    /// <param name="path">The request path to match against route patterns.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The matching GatewayRoute, or null if no route matches or an error occurs.</returns>
    public async Task<GatewayRoute?> FindMatchingRouteAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(path))
            return null;

        try
        {
            var allRoutes = await _routeRepository.GetAllAsync(cancellationToken);

            // Find routes that match the path, ordered by priority
            var matchingRoute = allRoutes
                .Where(r => r.IsActive && StringUtility.MatchesWildcardPattern(path, r.Pattern))
                .OrderByDescending(r => r.Priority)
                .FirstOrDefault();

            if (matchingRoute is not null)
                _logger.LogDebug(MatchingRouteFoundMessage,
                    StringUtility.MaskSensitiveData(path), matchingRoute.Pattern);

            return matchingRoute;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, MatchingRouteErrorMessage,
                StringUtility.MaskSensitiveData(path));
            return null;
        }
    }

    /// <summary>
    /// Finds all routes that have patterns conflicting with the specified pattern.
    /// Two patterns conflict if one matches the other (wildcard matching).
    /// </summary>
    /// <param name="pattern">The route pattern to check for conflicts against.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of GatewayRoute objects that conflict with the specified pattern.
    /// Returns an empty list if no conflicts are found, the pattern is null/empty, or an error occurs.</returns>
    public async Task<List<GatewayRoute>> GetConflictingRoutesAsync(string pattern, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(pattern))
            return new List<GatewayRoute>();

        try
        {
            var allRoutes = await _routeRepository.GetAllAsync(cancellationToken);

            // Find routes with overlapping patterns
            var conflicting = allRoutes.Where(r =>
                r.Pattern != pattern &&
                (StringUtility.MatchesWildcardPattern(pattern, r.Pattern) ||
                 StringUtility.MatchesWildcardPattern(r.Pattern, pattern)))
                .ToList();

            _logger.LogDebug(ConflictingRoutesFoundMessage,
                conflicting.Count, StringUtility.MaskSensitiveData(pattern));

            return conflicting;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ConflictingRoutesErrorMessage,
                StringUtility.MaskSensitiveData(pattern));
            return new List<GatewayRoute>();
        }
    }

    /// <summary>
    /// Validates a route's configuration, including pattern, priority, rate limit, cache duration, and duplicate patterns.
    /// </summary>
    /// <param name="route">The route to validate.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>true if the route is valid; otherwise, false.</returns>
    public async Task<bool> ValidateRouteAsync(GatewayRoute route, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(route);

        try
        {
            // Validate pattern
            if (string.IsNullOrWhiteSpace(route.Pattern))
            {
                _logger.LogWarning(EmptyPatternWarningMessage, route.Id);
                return false;
            }

            // Validate priority
            if (route.Priority < MinimumPriority || route.Priority > MaximumPriority)
            {
                _logger.LogWarning(InvalidPriorityWarningMessage, route.Priority, route.Id);
                return false;
            }

            // Validate rate limit
            if (route.RateLimitPerMinute < MinimumRateLimitPerMinute)
            {
                _logger.LogWarning(NegativeRateLimitWarningMessage, route.Id);
                return false;
            }

            // Validate cache duration
            if (route.EnableCaching && route.CacheDurationSeconds < MinimumCacheDurationSeconds)
            {
                _logger.LogWarning(NegativeCacheDurationWarningMessage, route.Id);
                return false;
            }

            // Check for duplicate patterns
            var allRoutes = await _routeRepository.GetAllAsync(cancellationToken);
            var duplicate = allRoutes.FirstOrDefault(r => r.Id != route.Id && r.Pattern == route.Pattern);

            if (duplicate is not null)
            {
                _logger.LogWarning(DuplicatePatternWarningMessage,
                    StringUtility.MaskSensitiveData(route.Pattern), duplicate.Id);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, RouteValidationErrorMessage);
            return false;
        }
    }
}
