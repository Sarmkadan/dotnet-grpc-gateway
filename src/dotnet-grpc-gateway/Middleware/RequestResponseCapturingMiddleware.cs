#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Extensions;
using DotNetGrpcGateway.Services;

namespace DotNetGrpcGateway.Middleware;

/// <summary>
/// Captures request/response metadata for every inbound request and stores it
/// in <see cref="IRequestLogService"/> for structured querying.
/// </summary>
/// <remarks>
/// Captures method, path, status code, duration, client IP, headers (with sensitive
/// values redacted), and body sizes. Actual body content is not stored to avoid
/// high memory usage.
/// </remarks>
public class RequestResponseCapturingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseCapturingMiddleware> _logger;

    private static readonly HashSet<string> SensitiveHeaders =
        new(StringComparer.OrdinalIgnoreCase) { "Authorization", "Cookie", "X-Api-Key" };

    public RequestResponseCapturingMiddleware(
        RequestDelegate next,
        ILogger<RequestResponseCapturingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var originalResponseBody = context.Response.Body;

        await using var responseBuffer = new MemoryStream();
        context.Response.Body = responseBuffer;

        string? errorMessage = null;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            try
            {
                var logService = context.RequestServices.GetService<IRequestLogService>();
                if (logService is not null)
                {
                    var entry = BuildEntry(context, stopwatch.ElapsedMilliseconds, responseBuffer.Length, errorMessage);
                    logService.Append(entry);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to append request log entry");
            }

            responseBuffer.Seek(0, SeekOrigin.Begin);
            await responseBuffer.CopyToAsync(originalResponseBody);
            context.Response.Body = originalResponseBody;
        }
    }

    private static RequestLogEntry BuildEntry(
        HttpContext context,
        long elapsedMs,
        long responseBodyBytes,
        string? errorMessage)
    {
        var request = context.Request;
        var response = context.Response;

        var path = request.Path.Value ?? string.Empty;
        var grpcMethod = path.StartsWith('/') && path.Count(c => c == '/') >= 2
            ? path.TrimStart('/')
            : $"{request.Method} {path}";

        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in request.Headers)
            headers[key] = SensitiveHeaders.Contains(key) ? "***" : value.ToString();

        string? upstream = null;
        if (context.Items.TryGetValue("UpstreamAddress", out var upstreamObject) && upstreamObject is string address)
            upstream = address;

        return new RequestLogEntry
        {
            Timestamp = DateTime.UtcNow,
            Method = request.Method,
            Path = path,
            GrpcMethod = grpcMethod,
            StatusCode = response.StatusCode,
            DurationMs = elapsedMs,
            ClientIp = context.GetClientIpAddress(),
            UpstreamAddress = upstream,
            RequestHeaders = headers,
            RequestBodyBytes = request.ContentLength ?? 0,
            ResponseBodyBytes = responseBodyBytes,
            ErrorMessage = errorMessage
        };
    }
}
