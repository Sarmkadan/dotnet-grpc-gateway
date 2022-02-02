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

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = context.TraceIdentifier;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex, requestId);
        }
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
    public string RequestId { get; set; } = null!;

    public DateTime Timestamp { get; set; }

    public string Message { get; set; } = null!;

    public string? ErrorCode { get; set; }

    public Dictionary<string, object>? Details { get; set; }
}
