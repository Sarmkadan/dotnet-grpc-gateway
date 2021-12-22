#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;

namespace DotNetGrpcGateway.Middleware;

/// <summary>
/// Middleware that encodes gRPC trailing metadata as a gRPC-Web trailer frame and appends
/// it to the response body so browser clients receive full trailer information.
///
/// The gRPC-Web spec encodes trailers as a special data frame with bit 7 of the
/// compression-flags byte set (0x80). This allows HTTP/1.1 clients — including browsers
/// — to receive trailer metadata that would otherwise only be visible on HTTP/2.
/// </summary>
public class GrpcWebTrailerForwardingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GrpcWebTrailerForwardingMiddleware> _logger;

    // gRPC-Web trailer frame: first byte has bit 7 set
    private const byte TrailerFrameFlag = 0x80;

    // Headers that carry gRPC status and error detail trailers
    private static readonly string[] GrpcTrailerNames =
    {
        "grpc-status",
        "grpc-message",
        "grpc-status-details-bin",
        "grpc-encoding",
        "grpc-accept-encoding",
    };

    public GrpcWebTrailerForwardingMiddleware(
        RequestDelegate next,
        ILogger<GrpcWebTrailerForwardingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!IsGrpcWebRequest(context.Request))
        {
            await _next(context);
            return;
        }

        var originalBody = context.Response.Body;
        using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await _next(context);
        }
        finally
        {
            context.Response.Body = originalBody;
        }

        // Collect any gRPC trailer headers that arrived on the response.
        // ASP.NET Core surfaces HTTP/2 trailers via response.AppendTrailer, but when
        // the upstream sends them as regular headers we forward them as a gRPC-Web
        // trailer frame appended to the body.
        var trailers = context.Response.Headers
            .Where(h => GrpcTrailerNames.Contains(h.Key, StringComparer.OrdinalIgnoreCase)
                     || h.Key.StartsWith("grpc-", StringComparison.OrdinalIgnoreCase))
            .ToList();

        buffer.Seek(0, SeekOrigin.Begin);
        var responseBytes = buffer.ToArray();

        if (trailers.Count > 0)
        {
            // Build the trailer block as "key: value\r\n" pairs
            var trailerText = string.Concat(
                trailers.Select(h => $"{h.Key}: {string.Join(",", h.Value!)}\r\n"));

            var trailerPayload = Encoding.ASCII.GetBytes(trailerText);

            // 5-byte gRPC-Web frame header: [flags (1)] [length (4 BE)]
            var frame = new byte[5 + trailerPayload.Length];
            frame[0] = TrailerFrameFlag;
            var lengthBytes = BitConverter.GetBytes(trailerPayload.Length);
            if (BitConverter.IsLittleEndian) Array.Reverse(lengthBytes);
            Buffer.BlockCopy(lengthBytes, 0, frame, 1, 4);
            Buffer.BlockCopy(trailerPayload, 0, frame, 5, trailerPayload.Length);

            await originalBody.WriteAsync(responseBytes);
            await originalBody.WriteAsync(frame);

            _logger.LogDebug(
                "Forwarded {TrailerCount} gRPC trailer(s) to browser client as gRPC-Web trailer frame",
                trailers.Count);
        }
        else
        {
            await originalBody.WriteAsync(responseBytes);
        }
    }

    private static bool IsGrpcWebRequest(HttpRequest request)
    {
        var contentType = request.ContentType ?? string.Empty;
        return contentType.StartsWith("application/grpc-web", StringComparison.OrdinalIgnoreCase);
    }
}
