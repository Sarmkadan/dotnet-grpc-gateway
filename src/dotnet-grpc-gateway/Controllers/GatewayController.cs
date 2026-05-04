// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotNetGrpcGateway.Controllers;

/// <summary>
/// REST API controller for managing gateway configuration and services
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class GatewayController : ControllerBase
{
    private readonly IGatewayService _gatewayService;
    private readonly IMetricsCollectionService _metricsService;
    private readonly ILogger<GatewayController> _logger;

    public GatewayController(
        IGatewayService gatewayService,
        IMetricsCollectionService metricsService,
        ILogger<GatewayController> logger)
    {
        _gatewayService = gatewayService ?? throw new ArgumentNullException(nameof(gatewayService));
        _metricsService = metricsService ?? throw new ArgumentNullException(nameof(metricsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the current gateway configuration
    /// </summary>
    [HttpGet("configuration")]
    [ProducesResponseType(typeof(GatewayConfiguration), StatusCodes.Status200OK)]
    public async Task<ActionResult<GatewayConfiguration>> GetConfiguration()
    {
        var config = await _gatewayService.GetConfigurationAsync();
        return Ok(config);
    }

    /// <summary>
    /// Updates gateway configuration
    /// </summary>
    [HttpPut("configuration")]
    [ProducesResponseType(typeof(GatewayConfiguration), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GatewayConfiguration>> UpdateConfiguration([FromBody] GatewayConfiguration config)
    {
        if (config == null)
            return BadRequest("Configuration is required");

        var updated = await _gatewayService.UpdateConfigurationAsync(config);
        return Ok(updated);
    }

    /// <summary>
    /// Gets all registered services
    /// </summary>
    [HttpGet("services")]
    [ProducesResponseType(typeof(List<GrpcService>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<GrpcService>>> GetServices()
    {
        var services = await _gatewayService.GetAllServicesAsync();
        return Ok(services);
    }

    /// <summary>
    /// Gets all healthy services
    /// </summary>
    [HttpGet("services/healthy")]
    [ProducesResponseType(typeof(List<GrpcService>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<GrpcService>>> GetHealthyServices()
    {
        var services = await _gatewayService.GetHealthyServicesAsync();
        return Ok(services);
    }

    /// <summary>
    /// Gets a specific service by ID
    /// </summary>
    [HttpGet("services/{serviceId}")]
    [ProducesResponseType(typeof(GrpcService), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GrpcService>> GetService(int serviceId)
    {
        try
        {
            var service = await _gatewayService.GetServiceAsync(serviceId);
            return Ok(service);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Service with ID {serviceId} not found");
        }
    }

    /// <summary>
    /// Registers a new gRPC service
    /// </summary>
    [HttpPost("services")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RegisterService([FromBody] GrpcService service)
    {
        if (service == null)
            return BadRequest("Service is required");

        try
        {
            await _gatewayService.RegisterServiceAsync(service);
            _logger.LogInformation("Service '{ServiceName}' registered", service.Name);
            return Created($"/api/gateway/services/{service.Id}", service);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Unregisters a service
    /// </summary>
    [HttpDelete("services/{serviceId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UnregisterService(int serviceId)
    {
        try
        {
            await _gatewayService.UnregisterServiceAsync(serviceId);
            _logger.LogInformation("Service with ID {ServiceId} unregistered", serviceId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Gets all routes
    /// </summary>
    [HttpGet("routes")]
    [ProducesResponseType(typeof(List<GatewayRoute>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<GatewayRoute>>> GetRoutes()
    {
        var routes = await _gatewayService.GetAllRoutesAsync();
        return Ok(routes);
    }

    /// <summary>
    /// Adds a new route
    /// </summary>
    [HttpPost("routes")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AddRoute([FromBody] GatewayRoute route)
    {
        if (route == null)
            return BadRequest("Route is required");

        try
        {
            var created = await _gatewayService.AddRouteAsync(route);
            _logger.LogInformation("Route '{Pattern}' added", route.Pattern);
            return Created($"/api/gateway/routes/{created.Id}", created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Removes a route
    /// </summary>
    [HttpDelete("routes/{routeId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveRoute(int routeId)
    {
        try
        {
            await _gatewayService.RemoveRouteAsync(routeId);
            _logger.LogInformation("Route with ID {RouteId} removed", routeId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Route with ID {routeId} not found");
        }
    }

    /// <summary>
    /// Gets today's statistics
    /// </summary>
    [HttpGet("statistics/today")]
    [ProducesResponseType(typeof(GatewayStatistics), StatusCodes.Status200OK)]
    public async Task<ActionResult<GatewayStatistics>> GetTodayStatistics()
    {
        var stats = await _metricsService.GetTodayStatisticsAsync();
        return Ok(stats);
    }

    /// <summary>
    /// Gets statistics for a specific date
    /// </summary>
    [HttpGet("statistics/{date}")]
    [ProducesResponseType(typeof(GatewayStatistics), StatusCodes.Status200OK)]
    public async Task<ActionResult<GatewayStatistics>> GetStatistics(string date)
    {
        if (!DateTime.TryParse(date, out var parsedDate))
            return BadRequest("Invalid date format");

        var stats = await _metricsService.GetStatisticsAsync(parsedDate);
        return Ok(stats);
    }

    /// <summary>
    /// Gets slow requests
    /// </summary>
    [HttpGet("metrics/slow-requests")]
    [ProducesResponseType(typeof(List<RequestMetric>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RequestMetric>>> GetSlowRequests([FromQuery] double thresholdMs = 1000)
    {
        if (thresholdMs <= 0)
            return BadRequest("Threshold must be greater than 0");

        var slowRequests = await _metricsService.GetSlowRequestsAsync(thresholdMs);
        return Ok(slowRequests);
    }

    /// <summary>
    /// Gets average response time
    /// </summary>
    [HttpGet("metrics/average-response-time")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAverageResponseTime()
    {
        var avgTime = await _metricsService.GetAverageResponseTimeAsync();
        return Ok(new { averageResponseTimeMs = avgTime });
    }
}
