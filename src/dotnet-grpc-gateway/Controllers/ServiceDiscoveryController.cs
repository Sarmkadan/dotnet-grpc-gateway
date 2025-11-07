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
[Route("api/[controller]")]
[Produces("application/json")]
public class ServiceDiscoveryController : ControllerBase
{
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
        _gatewayService = gatewayService ?? throw new ArgumentNullException(nameof(gatewayService));
        _discoveryService = discoveryService ?? throw new ArgumentNullException(nameof(discoveryService));
        _routeManagementService = routeManagementService ?? throw new ArgumentNullException(nameof(routeManagementService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all registered services with their metadata.
    /// </summary>
    [HttpGet("services")]
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
            _logger.LogError(ex, "Error retrieving services");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets routes for a specific service.
    /// </summary>
    [HttpGet("services/{serviceId}/routes")]
    [ProducesResponseType(typeof(List<GatewayRoute>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<GatewayRoute>>> GetServiceRoutes(int serviceId)
    {
        try
        {
            var service = await _gatewayService.GetServiceAsync(serviceId);
            if (service == null)
                return NotFound($"Service {serviceId} not found");

            var routes = await _routeManagementService.GetRoutesByServiceAsync(serviceId);
            return Ok(routes);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving routes for service {ServiceId}", serviceId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets the matching route for a given path.
    /// </summary>
    [HttpPost("route-match")]
    [ProducesResponseType(typeof(RouteMatchResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RouteMatchResult>> FindMatchingRoute([FromBody] RouteMatchRequest request)
    {
        if (string.IsNullOrEmpty(request?.Path))
            return BadRequest("Path is required");

        try
        {
            var route = await _routeManagementService.FindMatchingRouteAsync(request.Path);
            if (route == null)
                return NotFound("No matching route found");

            var service = await _gatewayService.GetServiceAsync(route.TargetServiceId);

            return Ok(new RouteMatchResult
            {
                RouteId = route.Id,
                Pattern = route.Pattern,
                ServiceId = service?.Id ?? 0,
                ServiceName = service?.Name,
                Priority = route.Priority
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding matching route for path {Path}", request.Path);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets conflicting routes that might affect a given pattern.
    /// </summary>
    [HttpPost("route-conflicts")]
    [ProducesResponseType(typeof(List<GatewayRoute>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<GatewayRoute>>> GetConflictingRoutes([FromBody] RoutePatternRequest request)
    {
        if (string.IsNullOrEmpty(request?.Pattern))
            return BadRequest("Pattern is required");

        try
        {
            var conflicts = await _routeManagementService.GetConflictingRoutesAsync(request.Pattern);
            return Ok(conflicts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for conflicting routes");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}

/// <summary>
/// Service information response model.
/// </summary>
public class ServiceInfo
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? ServiceFullName { get; set; }
    public string? Host { get; set; }
    public int Port { get; set; }
    public bool UseTls { get; set; }
    public bool IsActive { get; set; }
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
