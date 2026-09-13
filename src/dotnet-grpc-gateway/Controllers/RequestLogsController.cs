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
/// REST API for querying the structured request/response log store.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RequestLogsController : ControllerBase
{
    private readonly IRequestLogService _logService;
    private readonly ILogger<RequestLogsController> _logger;

    public RequestLogsController(IRequestLogService logService, ILogger<RequestLogsController> logger)
    {
        ArgumentNullException.ThrowIfNull(logService);
        ArgumentNullException.ThrowIfNull(logger);
        _logService = logService;
        _logger = logger;
    }

    /// <summary>
    /// Returns the most recent request log entries.
    /// </summary>
    /// <param name="limit">The maximum number of entries to return.</param>
    /// <returns>The most recent retained request log entries.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RequestLogEntry>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<RequestLogEntry>> GetRecent([FromQuery] int limit = 50)
    {
        if (limit < 1 || limit > 1000)
            limit = 50;

        return Ok(_logService.GetRecent(limit));
    }

    /// <summary>
    /// Searches log entries by method, status code, or time range.
    /// </summary>
    /// <param name="method">The optional HTTP or gRPC method to match.</param>
    /// <param name="statusCode">The optional response status code to match.</param>
    /// <param name="from">The optional earliest timestamp to include.</param>
    /// <param name="to">The optional latest timestamp to include.</param>
    /// <param name="limit">The maximum number of entries to return.</param>
    /// <returns>The matching request log entries, or a bad request result when the time range is invalid.</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<RequestLogEntry>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<IReadOnlyList<RequestLogEntry>> Search(
        [FromQuery] string? method = null,
        [FromQuery] int? statusCode = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int limit = 50)
    {
        if (from.HasValue && to.HasValue && from > to)
            return BadRequest("'from' must be before 'to'");

        return Ok(_logService.Search(method, statusCode, from, to, limit));
    }

    /// <summary>
    /// Returns aggregate statistics over retained log entries.
    /// </summary>
    /// <returns>Aggregate statistics for the retained request log entries.</returns>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(RequestLogSummary), StatusCodes.Status200OK)]
    public ActionResult<RequestLogSummary> GetSummary() =>
        Ok(_logService.GetSummary());

    /// <summary>
    /// Clears all retained log entries.
    /// </summary>
    /// <returns>A response indicating that the request log entries were cleared.</returns>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public ActionResult Clear()
    {
        _logService.Clear();
        _logger.LogInformation("Request log store cleared via API");
        return NoContent();
    }
}
