// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotNetGrpcGateway.Controllers;

/// <summary>
/// REST API controller for health status and liveness/readiness checks.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    private readonly IServiceDiscoveryService _discoveryService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        IServiceDiscoveryService discoveryService,
        ILogger<HealthController> logger)
    {
        _discoveryService = discoveryService ?? throw new ArgumentNullException(nameof(discoveryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets overall gateway health status.
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(HealthStatus), StatusCodes.Status200OK)]
    public async Task<ActionResult<HealthStatus>> GetHealthStatus()
    {
        try
        {
            var allServicesHealth = await _discoveryService.GetAllServicesHealthAsync();

            var healthyCount = allServicesHealth.Count(h => h.IsHealthy);
            var totalCount = allServicesHealth.Count();

            var status = new HealthStatus
            {
                IsHealthy = healthyCount == totalCount,
                Timestamp = DateTime.UtcNow,
                HealthyServices = healthyCount,
                TotalServices = totalCount,
                Message = healthyCount == totalCount
                    ? "All services are healthy"
                    : $"{healthyCount}/{totalCount} services are healthy"
            };

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking gateway health");
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new HealthStatus { IsHealthy = false, Message = ex.Message });
        }
    }

    /// <summary>
    /// Gets detailed health information for all services.
    /// </summary>
    [HttpGet("services")]
    [ProducesResponseType(typeof(List<ServiceHealthStatus>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ServiceHealthStatus>>> GetServicesHealth()
    {
        try
        {
            var allServicesHealth = await _discoveryService.GetAllServicesHealthAsync();

            var serviceStatuses = allServicesHealth.Select(h => new ServiceHealthStatus
            {
                ServiceId = h.ServiceId,
                ServiceName = h.ServiceName,
                IsHealthy = h.IsHealthy,
                LastCheckedAt = h.LastCheckedAt,
                CheckCount = h.CheckCount,
                FailureCount = h.FailureCount,
                Message = h.IsHealthy ? "Service is responding normally" : h.FailureMessage
            }).ToList();

            return Ok(serviceStatuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving service health details");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets health for a specific service.
    /// </summary>
    [HttpGet("services/{serviceId}")]
    [ProducesResponseType(typeof(ServiceHealthStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceHealthStatus>> GetServiceHealth(int serviceId)
    {
        try
        {
            var report = await _discoveryService.PerformHealthCheckAsync(serviceId);

            if (report == null)
                return NotFound($"Service {serviceId} not found");

            var status = new ServiceHealthStatus
            {
                ServiceId = report.ServiceId,
                ServiceName = report.ServiceName,
                IsHealthy = report.IsHealthy,
                LastCheckedAt = report.LastCheckedAt,
                CheckCount = report.CheckCount,
                FailureCount = report.FailureCount,
                Message = report.IsHealthy ? "Service is healthy" : report.FailureMessage
            };

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking health for service {ServiceId}", serviceId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Readiness probe - checks if gateway is ready to receive requests.
    /// </summary>
    [HttpGet("ready")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult> GetReadiness()
    {
        try
        {
            var allServicesHealth = await _discoveryService.GetAllServicesHealthAsync();

            if (allServicesHealth.Count == 0)
                return StatusCode(StatusCodes.Status503ServiceUnavailable);

            var healthyCount = allServicesHealth.Count(h => h.IsHealthy);

            // Consider ready if at least 50% of services are healthy
            return healthyCount >= (allServicesHealth.Count / 2)
                ? Ok("Ready")
                : StatusCode(StatusCodes.Status503ServiceUnavailable, "Not enough healthy services");
        }
        catch
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
    }

    /// <summary>
    /// Liveness probe - checks if gateway process is still running.
    /// </summary>
    [HttpGet("live")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetLiveness()
    {
        return Ok("Alive");
    }
}

/// <summary>
/// Health status response model.
/// </summary>
public class HealthStatus
{
    public bool IsHealthy { get; set; }
    public DateTime Timestamp { get; set; }
    public int HealthyServices { get; set; }
    public int TotalServices { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// Service health status response model.
/// </summary>
public class ServiceHealthStatus
{
    public int ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public bool IsHealthy { get; set; }
    public DateTime LastCheckedAt { get; set; }
    public int CheckCount { get; set; }
    public int FailureCount { get; set; }
    public string? Message { get; set; }
}
