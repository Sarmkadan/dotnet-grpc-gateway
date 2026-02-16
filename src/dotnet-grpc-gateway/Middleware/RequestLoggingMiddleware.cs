// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using System.Text;
using DotNetGrpcGateway.Extensions;

namespace DotNetGrpcGateway.Middleware;

/// <summary>
/// Middleware for comprehensive request/response logging with performance metrics.
/// Captures request body, response body, headers, and execution time for debugging and analysis.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private const int MaxBodyLogSize = 5000; // Limit body logging to 5KB to avoid log bloat

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var originalResponseBody = context.Response.Body;

        try
        {
            // Log incoming request
            await LogRequestAsync(context);

            // Enable response body buffering to capture response data
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // Call the next middleware
            await _next(context);

            stopwatch.Stop();

            // Log response with elapsed time
            await LogResponseAsync(context, stopwatch.ElapsedMilliseconds, responseBody);

            // Write buffered response to original body stream
            await responseBody.CopyToAsync(originalResponseBody);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Unhandled exception in request processing. Path: {Path}, Time: {ElapsedMs}ms",
                context.Request.Path, stopwatch.ElapsedMilliseconds);
            throw;
        }
        finally
        {
            context.Response.Body = originalResponseBody;
        }
    }

    private async Task LogRequestAsync(HttpContext context)
    {
        var request = context.Request;

        // Only log body for content-bearing methods (POST, PUT, PATCH)
        string? bodyContent = null;
        if (request.ContentLength.GetValueOrDefault() > 0 && IsBodyAllowed(request.Method))
        {
            request.EnableBuffering(); // Allow body to be read multiple times
            bodyContent = await ReadBodyAsync(request.Body);
            request.Body.Position = 0; // Reset for actual processing
        }

        var logData = new
        {
            Method = request.Method,
            Path = request.Path.Value,
            QueryString = request.QueryString.Value,
            ContentType = request.ContentType,
            ContentLength = request.ContentLength,
            Body = bodyContent != null ? TruncateBody(bodyContent) : null
        };

        _logger.LogInformation("Incoming request: {@RequestData}", logData);
    }

    private async Task LogResponseAsync(HttpContext context, long elapsedMs, MemoryStream responseBody)
    {
        var response = context.Response;
        responseBody.Seek(0, SeekOrigin.Begin);

        string? responseContent = null;
        if (responseBody.Length > 0)
        {
            using var reader = new StreamReader(responseBody, Encoding.UTF8);
            responseContent = await reader.ReadToEndAsync();
            responseBody.Seek(0, SeekOrigin.Begin);
        }

        var logData = new
        {
            StatusCode = response.StatusCode,
            ContentType = response.ContentType,
            ContentLength = response.ContentLength,
            ElapsedMs = elapsedMs,
            Body = responseContent != null ? TruncateBody(responseContent) : null
        };

        var logLevel = response.StatusCode >= 500 ? LogLevel.Error :
                      response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

        _logger.Log(logLevel, "Request completed: {@ResponseData}", logData);
    }

    private static async Task<string> ReadBodyAsync(Stream body)
    {
        using var reader = new StreamReader(body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }

    private static string TruncateBody(string? body)
    {
        if (string.IsNullOrEmpty(body))
            return string.Empty;

        return body.Length > MaxBodyLogSize
            ? $"{body[..MaxBodyLogSize]}... (truncated)"
            : body;
    }

    private static bool IsBodyAllowed(string method)
    {
        return method is "POST" or "PUT" or "PATCH";
    }
}
