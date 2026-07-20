#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotNetGrpcGateway.Controllers;

/// <summary>
/// REST API for managing load balancer endpoints and strategy.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LoadBalancerController : ControllerBase
{
    private readonly ILoadBalancerService _loadBalancer;
    private readonly ILogger<LoadBalancerController> _logger;

    public LoadBalancerController(ILoadBalancerService loadBalancer, ILogger<LoadBalancerController> logger)
    {
        _loadBalancer = loadBalancer ?? throw new ArgumentNullException(nameof(loadBalancer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Returns all endpoints registered for a service.</summary>
    [HttpGet("services/{serviceId}/endpoints")]
    [ProducesResponseType(typeof(IReadOnlyList<ServiceEndpoint>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<ServiceEndpoint>> GetEndpoints(int serviceId) =>
        Ok(_loadBalancer.GetEndpoints(serviceId));

    /// <summary>Registers a new endpoint for a service.</summary>
    [HttpPost("services/{serviceId}/endpoints")]
    [ProducesResponseType(typeof(ServiceEndpoint), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult RegisterEndpoint(int serviceId, [FromBody] ServiceEndpoint endpoint)
    {
        if (endpoint is null)
            return BadRequest("Endpoint is required");

        endpoint.ServiceId = serviceId;
        _loadBalancer.RegisterEndpoint(endpoint);
        _logger.LogInformation("Registered endpoint {Uri} for service {ServiceId}", endpoint.GetUri(), serviceId);

        return Created($"/api/loadbalancer/services/{serviceId}/endpoints", endpoint);
    }

    /// <summary>Removes an endpoint from a service pool.</summary>
    [HttpDelete("services/{serviceId}/endpoints/{endpointId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public ActionResult DeregisterEndpoint(int serviceId, int endpointId)
    {
        _loadBalancer.DeregisterEndpoint(serviceId, endpointId);
        return NoContent();
    }

    /// <summary>Updates the health state of a specific endpoint.</summary>
    [HttpPatch("services/{serviceId}/endpoints/{endpointId}/health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult UpdateEndpointHealth(int serviceId, int endpointId, [FromBody] bool isHealthy)
    {
        _loadBalancer.UpdateEndpointHealth(serviceId, endpointId, isHealthy);
        return Ok(new { serviceId, endpointId, isHealthy });
    }

    /// <summary>Sets the draining state of a specific endpoint.</summary>
    [HttpPatch("services/{serviceId}/endpoints/{endpointId}/draining")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult SetEndpointDraining(int serviceId, int endpointId, [FromBody] bool draining)
    {
        _loadBalancer.SetDraining(serviceId, endpointId, draining);
        _logger.LogInformation("Set endpoint {EndpointId} draining state to {Draining} for service {ServiceId}", endpointId, draining, serviceId);
        return Ok(new { serviceId, endpointId, draining });
    }

    /// <summary>Gets the current load balancing strategy.</summary>
    [HttpGet("strategy")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public ActionResult GetStrategy() =>
        Ok(new { strategy = _loadBalancer.Strategy.ToString() });

    /// <summary>Sets the active load balancing strategy.</summary>
    [HttpPut("strategy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult SetStrategy([FromBody] string strategyName)
    {
        if (!Enum.TryParse<LoadBalancingStrategy>(strategyName, true, out var strategy))
        {
            return BadRequest(
                $"Unknown strategy '{strategyName}'. Valid values: RoundRobin, Random, LeastConnections");
        }

        _loadBalancer.Strategy = strategy;
        _logger.LogInformation("Load balancing strategy changed to {Strategy}", strategy);
        return Ok(new { strategy = strategy.ToString() });
    }
}
