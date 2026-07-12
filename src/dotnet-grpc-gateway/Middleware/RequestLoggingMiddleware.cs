#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using System.Text;
using DotNetGrpcGateway.Extensions;
using DotNetGrpcGateway.Options;
using Microsoft.Extensions.Options;

namespace DotNetGrpcGateway.Middleware;

/// <summary>
/// Middleware for structured gRPC request/response logging with configurable verbosity.
/// </summary>
/// <remarks>
/// Verbosity levels (configured via <c>Gateway:RequestLogging:Verbosity</c>):
/// <list type="bullet">
///   <item><b>Minimal</b> – gRPC method, response status, and duration.</item>
///   <item><b>Normal</b> – Minimal + request/response headers and upstream service address.</item>
///   <item><b>Verbose</b> – Normal + request and response body sizes.</item>
/// </list>
/// </remarks>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly RequestLoggingOptions _options;

    private const int MaxBodyLogSize = 5000;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger,
        IOptions<DotnetGrpcGatewayOptions> gatewayOptions)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = (gatewayOptions ?? throw new ArgumentNullException(nameof(gatewayOptions))).Value.RequestLogging;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_options.Enabled)
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var originalResponseBody = context.Response.Body;

        using var responseBodyBuffer = new MemoryStream();
        context.Response.Body = responseBodyBuffer;

        try
        {
            long requestBodyBytes = 0;

            if (_options.Verbosity >= RequestLoggingVerbosity.Verbose && IsBodyAllowed(context.Request.Method))
            {
                context.Request.EnableBuffering();
                requestBodyBytes = context.Request.ContentLength ?? 0;
            }

            await _next(context);

            stopwatch.Stop();

            await LogCompletedRequestAsync(context, stopwatch.ElapsedMilliseconds, requestBodyBytes, responseBodyBuffer);

            responseBodyBuffer.Seek(0, SeekOrigin.Begin);
            await responseBodyBuffer.CopyToAsync(originalResponseBody);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "Unhandled exception. Method={GrpcMethod} Path={Path} Duration={ElapsedMs}ms",
                GetGrpcMethod(context), context.Request.Path, stopwatch.ElapsedMilliseconds);
            throw;
        }
        finally
        {
            context.Response.Body = originalResponseBody;
        }
    }

    private async Task LogCompletedRequestAsync(
        HttpContext context,
        long elapsedMs,
        long requestBodyBytes,
        MemoryStream responseBodyBuffer)
    {
        var request = context.Request;
        var response = context.Response;
        var grpcMethod = GetGrpcMethod(context);
        var upstream = GetUpstreamAddress(context);

        var logLevel = response.StatusCode >= 500 ? LogLevel.Error
                     : response.StatusCode >= 400 ? LogLevel.Warning
                     : LogLevel.Information;

        switch (_options.Verbosity)
        {
            case RequestLoggingVerbosity.Minimal:
                _logger.Log(logLevel,
                    "[{Timestamp}] gRPC {GrpcMethod} {Status} {ElapsedMs}ms",
                    DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture),
                    grpcMethod,
                    response.StatusCode,
                    elapsedMs);
                break;

            case RequestLoggingVerbosity.Normal:
                _logger.Log(logLevel,
                    "[{Timestamp}] gRPC {GrpcMethod} upstream={Upstream} {Status} {ElapsedMs}ms {@RequestHeaders}",
                    DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture),
                    grpcMethod,
                    upstream,
                    response.StatusCode,
                    elapsedMs,
                    GetSafeHeaders(request.Headers));
                break;

            case RequestLoggingVerbosity.Verbose:
            default:
                var responseBodyBytes = responseBodyBuffer.Length;
                _logger.Log(logLevel,
                    "[{Timestamp}] gRPC {GrpcMethod} upstream={Upstream} {Status} {ElapsedMs}ms " +
                    "req_bytes={RequestBytes} res_bytes={ResponseBytes} {@RequestHeaders}",
                    DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture),
                    grpcMethod,
                    upstream,
                    response.StatusCode,
                    elapsedMs,
                    requestBodyBytes,
                    responseBodyBytes,
                    GetSafeHeaders(request.Headers));
                break;
        }
    }

    /// <summary>
    /// Resolves the gRPC method name from the request path or the gRPC-method header.
    /// Falls back to the HTTP method + path when neither is available.
    /// </summary>
    private static string GetGrpcMethod(HttpContext context)
    {
        // gRPC-Web requests include a path like /package.Service/Method
        var path = context.Request.Path.Value;
        if (!string.IsNullOrEmpty(path) && path.StartsWith('/') && path.Count(c => c == '/') >= 2)
            return path.TrimStart('/');

        return $"{context.Request.Method} {context.Request.Path}";
    }

    private static string GetUpstreamAddress(HttpContext context)
    {
        // The upstream address may be stored by the proxy/gateway logic in a custom header or item
        if (context.Items.TryGetValue("UpstreamAddress", out var upstream) && upstream is string addr)
            return addr;

        if (context.Request.Headers.TryGetValue("X-Upstream-Address", out var headerVal))
            return headerVal.ToString();

        return "unknown";
    }

    private static Dictionary<string, string> GetSafeHeaders(IHeaderDictionary headers)
    {
        var safe = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in headers)
        {
            // Skip sensitive headers
            if (key.Equals("Authorization", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("Cookie", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("X-Api-Key", StringComparison.OrdinalIgnoreCase))
            {
                safe[key] = "***";
                continue;
            }

            safe[key] = value.ToString();
        }

        return safe;
    }

    private static string TruncateBody(string? body)
    {
        if (string.IsNullOrEmpty(body))
            return string.Empty;

        return body.Length > MaxBodyLogSize
            ? $"{body[..MaxBodyLogSize]}... (truncated)"
            : body;
    }

    private static bool IsBodyAllowed(string method) =>
        method is "POST" or "PUT" or "PATCH";
}
