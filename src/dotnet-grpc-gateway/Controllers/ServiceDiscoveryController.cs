#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =anchors.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotNetGrpcGateway.Controllers;

/// <summary>
/// REST API controller for service discovery operations.
/// Provides endpoints for service registration, discovery, and dynamic configuration.
/// </summary>
[ApiController]
[Route(ApiRoute)]
[Produces(JsonContentType)]
public class ServiceDiscoveryController : ControllerBase
{
    private const string ApiRoute = "api/[controller]";
    private const string JsonContentType = "application/json";
    private const string ServicesRoute = "services";
    private const string ServiceRoutesRoute = "services/{serviceId}/routes";
    private const string RouteMatchRoute = "route-match";
    private const string RouteConflictsRoute = "route-conflicts";
    private const string ServiceNotFoundMessageFormat = "Service {0} not found";
    private const string PathRequiredMessage = "Path is required";
    private const string NoMatchingRouteMessage = "No matching route found";
    private const string PatternRequiredMessage = "Pattern is required";
    private const string RetrievingServicesErrorMessage = "Error retrieving services";
    private const string RetrievingRoutesErrorMessage = "Error retrieving routes for service {ServiceId}";
    private const string FindingRouteErrorMessage = "Error finding matching route for path {Path}";
    private const string CheckingRouteConflictsErrorMessage = "Error checking for conflicting routes";
    private const int UnknownServiceId = 0;

    private readonly IGatewayService _gatewayService;
    private readonly IServiceDiscoveryService _discoveryService;
    private readonly IRouteManagementService _routeManagementService;
    private readonly ILogger<ServiceDiscoveryController> _logger;

    public ServiceDiscoveryController(
        IGatewayService gatewayService,
        IServiceDiscoveryService discoveryService,
        IRouteManagementService routeManagementService,
        ILogger<ServiceDiscoveryController> logger)
    {
        ArgumentNullException.ThrowIfNull(gatewayService);
        ArgumentNullException.ThrowIfNull(discoveryService);
        ArgumentNullException.ThrowIfNull(routeManagementService);
        ArgumentNullException.ThrowIfNull(logger);

        _gatewayService = gatewayService;
        _discoveryService = discoveryService;
        _routeManagementService = routeManagementService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all registered services with their metadata.
    /// </summary>
    [HttpGet(ServicesRoute)]
    [ProducesResponseType(typeof(List<ServiceInfo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ServiceInfo>>> GetAllServices()
    {
        try
        {
            var services = await _gatewayService.GetAllServicesAsync();
            var serviceInfos = services.Select(s => new ServiceInfo
            {
                Id = s.Id,
                Name = s.Name,
                ServiceFullName = s.ServiceFullName,
                Host = s.Host,
                Port = s.Port,
                UseTls = s.UseTls,
                IsActive = s.IsActive
            }).ToList();

            return Ok(serviceInfos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, RetrievingServicesErrorMessage);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets routes for a specific service.
    /// </summary>
    [HttpGet(ServiceRoutesRoute)]
    [ProducesResponseType(typeof(List<GatewayRoute>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<GatewayRoute>>> GetServiceRoutes(int serviceId)
    {
        try
        {
            var service = await _gatewayService.GetServiceAsync(serviceId);
            if (service is null)
                return NotFound(string.Format(ServiceNotFoundMessageFormat, serviceId));

            var routes = await _routeManagementService.GetRoutesByServiceAsync(serviceId);
            return Ok(routes);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, RetrievingRoutesErrorMessage, serviceId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets the matching route for a given path.
    /// </summary>
    [HttpPost(RouteMatchRoute)]
    [ProducesResponseType(typeof(RouteMatchResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RouteMatchResult>> FindMatchingRoute([FromBody] RouteMatchRequest request)
    {
        if (string.IsNullOrEmpty(request?.Path))
            return BadRequest(PathRequiredMessage);

        try
        {
            var route = await _routeManagementService.FindMatchingRouteAsync(request.Path);
            if (route is null)
                return NotFound(NoMatchingRouteMessage);

            var service = await _gatewayService.GetServiceAsync(route.TargetServiceId);

            return Ok(new RouteMatchResult
            {
                RouteId = route.Id,
                Pattern = route.Pattern,
                ServiceId = service?.Id ?? UnknownServiceId,
                ServiceName = service?.Name,
                Priority = route.Priority
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FindingRouteErrorMessage, request.Path);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets conflicting routes that might affect a given pattern.
    /// </summary>
    [HttpPost(RouteConflictsRoute)]
    [ProducesResponseType(typeof(List<GatewayRoute>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<GatewayRoute>>> GetConflictingRoutes([FromBody] RoutePatternRequest request)
    {
        if (string.IsNullOrEmpty(request?.Pattern))
            return BadRequest(PatternRequiredMessage);

        try
        {
            var conflicts = await _routeManagementService.GetConflictingRoutesAsync(request.Pattern);
            return Ok(conflicts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, CheckingRouteConflictsErrorMessage);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}

/// <summary>
/// Service information response model.
/// </summary>
public class ServiceInfo
{
    private const string StringRepresentationFormat = "ServiceInfo {{ Id = {0}, Name = {1}, ServiceFullName = {2}, Host = {3}, Port = {4}, UseTls = {5} }}";

    public int Id { get; set; }
    public string? Name { get; set; }
    public string? ServiceFullName { get; set; }
    public string? Host { get; set; }
    public int Port { get; set; }
    public bool UseTls { get; set; }
    public bool IsActive { get; set; }

    public override string ToString() => string.Format(
        StringRepresentationFormat,
        Id,
        Name,
        ServiceFullName,
        Host,
        Port,
        UseTls);
}

/// <summary>
/// Route match request model.
/// </summary>
public class RouteMatchRequest
{
    public string? Path { get; set; }
}

/// <summary>
/// Route match result model.
/// </summary>
public class RouteMatchResult
{
    public int RouteId { get; set; }
    public string? Pattern { get; set; }
    public int ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public int Priority { get; set; }
}

/// <summary>
/// Route pattern request model.
/// </summary>
public class RoutePatternRequest
{
    public string? Pattern { get; set; }
}
