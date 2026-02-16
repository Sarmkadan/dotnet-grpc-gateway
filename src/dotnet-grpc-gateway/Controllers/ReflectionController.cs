// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotNetGrpcGateway.Controllers;

/// <summary>
/// REST controller exposing gRPC Server Reflection metadata and a dedicated
/// health check endpoint for the reflection subsystem.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReflectionController : ControllerBase
{
    private readonly IReflectionService _reflectionService;
    private readonly ILogger<ReflectionController> _logger;

    public ReflectionController(
        IReflectionService reflectionService,
        ILogger<ReflectionController> logger)
    {
        _reflectionService = reflectionService ?? throw new ArgumentNullException(nameof(reflectionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Lists cached reflection metadata for all registered services.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ServiceReflectionInfo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ServiceReflectionInfo>>> GetAllReflection(
        CancellationToken cancellationToken)
    {
        try
        {
            var info = await _reflectionService.GetAllReflectionInfoAsync(cancellationToken);
            return Ok(info);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all reflection metadata");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Returns cached reflection metadata for a specific service.
    /// </summary>
    /// <param name="serviceId">Identifier of the registered gRPC service.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    [HttpGet("{serviceId:int}")]
    [ProducesResponseType(typeof(ServiceReflectionInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceReflectionInfo>> GetServiceReflection(
        int serviceId,
        CancellationToken cancellationToken)
    {
        try
        {
            var info = await _reflectionService.GetServiceReflectionAsync(serviceId, cancellationToken);

            if (info == null)
                return NotFound(
                    $"No reflection data available for service {serviceId}. " +
                    $"Call POST /api/reflection/{serviceId}/refresh first.");

            return Ok(info);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reflection metadata for service {ServiceId}", serviceId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Lists the RPC methods discovered for a specific service.
    /// </summary>
    /// <param name="serviceId">Identifier of the registered gRPC service.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    [HttpGet("{serviceId:int}/methods")]
    [ProducesResponseType(typeof(List<ServiceMethodDescriptor>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ServiceMethodDescriptor>>> GetServiceMethods(
        int serviceId,
        CancellationToken cancellationToken)
    {
        try
        {
            var info = await _reflectionService.GetServiceReflectionAsync(serviceId, cancellationToken);

            if (info == null)
                return NotFound($"No reflection data for service {serviceId}");

            return Ok(info.Methods);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving methods for service {ServiceId}", serviceId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Triggers a fresh reflection probe for a single service and returns the updated snapshot.
    /// </summary>
    /// <param name="serviceId">Identifier of the registered gRPC service.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    [HttpPost("{serviceId:int}/refresh")]
    [ProducesResponseType(typeof(ServiceReflectionInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceReflectionInfo>> RefreshServiceReflection(
        int serviceId,
        CancellationToken cancellationToken)
    {
        try
        {
            var info = await _reflectionService.RefreshServiceReflectionAsync(serviceId, cancellationToken);
            return Ok(info);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Service {serviceId} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing reflection for service {ServiceId}", serviceId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Triggers concurrent reflection probes for all active services and refreshes the cache.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(object), StatusCodes.Status202Accepted)]
    public async Task<ActionResult> RefreshAllReflections(CancellationToken cancellationToken)
    {
        try
        {
            await _reflectionService.RefreshAllReflectionsAsync(cancellationToken);
            return Accepted(new { message = "Reflection refresh completed", timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing all reflection data");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Health check endpoint for the reflection subsystem.
    /// Returns <c>200 OK</c> when at least one back-end service has a reachable
    /// reflection endpoint; <c>503</c> when none are reachable.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    [HttpGet("health")]
    [ProducesResponseType(typeof(ReflectionHealthSummary), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReflectionHealthSummary), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ReflectionHealthSummary>> GetReflectionHealth(
        CancellationToken cancellationToken)
    {
        try
        {
            var allInfo = await _reflectionService.GetAllReflectionInfoAsync(cancellationToken);
            var available = allInfo.Count(i => i.IsAvailable);
            var total = allInfo.Count;

            var summary = new ReflectionHealthSummary
            {
                IsAvailable          = available > 0,
                AvailableServiceCount = available,
                TotalServiceCount    = total,
                CheckedAt            = DateTime.UtcNow,
                Message              = total == 0
                    ? "No services registered; reflection subsystem is idle"
                    : $"{available}/{total} service(s) have reflection enabled"
            };

            return summary.IsAvailable || total == 0
                ? Ok(summary)
                : StatusCode(StatusCodes.Status503ServiceUnavailable, summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking reflection health");
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new ReflectionHealthSummary { IsAvailable = false, Message = ex.Message, CheckedAt = DateTime.UtcNow });
        }
    }
}

/// <summary>
/// Response model summarising the health state of the gRPC reflection subsystem.
/// </summary>
public class ReflectionHealthSummary
{
    /// <summary>Gets or sets whether at least one service has a reachable reflection endpoint.</summary>
    public bool IsAvailable { get; set; }

    /// <summary>Gets or sets the number of services that responded to the reflection probe.</summary>
    public int AvailableServiceCount { get; set; }

    /// <summary>Gets or sets the total number of registered services checked.</summary>
    public int TotalServiceCount { get; set; }

    /// <summary>Gets or sets when this health summary was generated.</summary>
    public DateTime CheckedAt { get; set; }

    /// <summary>Gets or sets a human-readable status message.</summary>
    public string? Message { get; set; }
}
