#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net;
using System.Text.Json;
using DotNetGrpcGateway.Exceptions;

namespace DotNetGrpcGateway.Infrastructure;

/// <summary>
/// Middleware for handling and logging errors with structured responses
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorHandlingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger for error handling middleware.</param>
    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(logger);

        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware asynchronously.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var requestId = context.TraceIdentifier;
        _logger.LogInformation("InvokeAsync called with {RequestId}", requestId);

        try
        {
            await _next(context);
        }
        catch (ObjectDisposedException ex)
        {
            // The response stream was disposed because the client closed the connection
            // mid-stream. This is normal for server-streaming RPCs and must not produce
            // a 500 error log entry.
            _logger.LogDebug(
                ex,
                "Client disconnected before stream completed (request {RequestId})",
                requestId);
        }
        catch (OperationCanceledException ex) when (context.RequestAborted.IsCancellationRequested)
        {
            // Client cancelled the request (e.g. closed the browser tab).
            _logger.LogDebug(
                ex,
                "Request {RequestId} was cancelled by the client",
                requestId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Handling unexpected exception {ExceptionType} for request {RequestId}", ex.GetType().Name, requestId);
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex, requestId);
        }

        _logger.LogInformation("InvokeAsync completed with {RequestId}", requestId);
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, string requestId)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            RequestId = requestId,
            Timestamp = DateTime.UtcNow,
            Message = exception.Message
        };

        switch (exception)
        {
            case GatewayException ex:
                context.Response.StatusCode = ex.HttpStatusCode ?? 500;
                response.ErrorCode = ex.ErrorCode;
                response.Details = ex.Details;
                break;

            case ArgumentException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.ErrorCode = "VALIDATION_ERROR";
                break;

            case UnauthorizedAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.ErrorCode = "UNAUTHORIZED";
                break;

            case KeyNotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.ErrorCode = "NOT_FOUND";
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.ErrorCode = "INTERNAL_ERROR";
                break;
        }

        return context.Response.WriteAsJsonAsync(response);
    }
}

/// <summary>
/// Structured error response sent to clients
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Gets or sets the request identifier.
    /// </summary>
    public string RequestId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the timestamp of the error.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets the error details.
    /// </summary>
    public Dictionary<string, object>? Details { get; set; }
}
