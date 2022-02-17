#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Infrastructure;
using DotNetGrpcGateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotNetGrpcGateway.Controllers;

/// <summary>
/// REST API for inspecting and managing per-service circuit breakers.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CircuitBreakerController : ControllerBase
{
    private readonly ICircuitBreakerRegistry _registry;
    private readonly ILogger<CircuitBreakerController> _logger;

    public CircuitBreakerController(ICircuitBreakerRegistry registry, ILogger<CircuitBreakerController> logger)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Returns the state of all circuit breakers.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyDictionary<int, string>), StatusCodes.Status200OK)]
    public ActionResult GetAll()
    {
        var states = _registry.GetAllStates()
            .ToDictionary(pair => pair.Key, pair => pair.Value.ToString());
        return Ok(states);
    }

    /// <summary>Returns the status of the circuit breaker for a specific service.</summary>
    [HttpGet("services/{serviceId}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult GetStatus(int serviceId)
    {
        var breaker = _registry.TryGet(serviceId);
        if (breaker is null)
            return NotFound($"No circuit breaker registered for service {serviceId}");

        return Ok(new
        {
            serviceId,
            state = breaker.State.ToString(),
            consecutiveFailures = breaker.ConsecutiveFailures,
            openedAt = breaker.OpenedAt
        });
    }

    /// <summary>Manually resets the circuit breaker for a service to Closed state.</summary>
    [HttpPost("services/{serviceId}/reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult Reset(int serviceId)
    {
        var breaker = _registry.TryGet(serviceId);
        if (breaker is null)
            return NotFound($"No circuit breaker registered for service {serviceId}");

        _registry.Reset(serviceId);
        _logger.LogInformation("Circuit breaker for service {ServiceId} manually reset via API", serviceId);
        return Ok(new { serviceId, state = CircuitBreakerState.Closed.ToString() });
    }
}
