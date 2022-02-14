#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.IO.Compression;

namespace DotNetGrpcGateway.Middleware;

/// <summary>
/// Opt-in gzip compression middleware for gRPC-Web message frames.
///
/// The gRPC-Web spec supports per-message compression via the compression flag
/// (bit 0) of the 5-byte frame header.  When the browser advertises
/// <c>grpc-accept-encoding: gzip</c> and compression is enabled in
/// <see cref="DotNetGrpcGateway.Configuration.GatewayOptions.EnableCompression"/>,
/// this middleware gzip-compresses every data frame and sets the
/// <c>grpc-encoding: gzip</c> response header so the client can decompress.
/// Trailer frames (flag 0x80) are never compressed.
/// </summary>
public class GrpcWebCompressionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GrpcWebCompressionMiddleware> _logger;
    private readonly bool _enableCompression;

    // Minimum payload size worth compressing (bytes)
    private const int MinCompressibleSize = 256;

    public GrpcWebCompressionMiddleware(
        RequestDelegate next,
        ILogger<GrpcWebCompressionMiddleware> logger,
        bool enableCompression = true)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _enableCompression = enableCompression;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_enableCompression || !IsGrpcWebRequest(context.Request) || !ClientAcceptsGzip(context.Request))
        {
            await _next(context);
            return;
        }

        // Advertise that we will compress the response
        context.Response.Headers["grpc-encoding"] = "gzip";

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

        buffer.Seek(0, SeekOrigin.Begin);
        var compressedBody = await CompressGrpcFramesAsync(buffer);
        await originalBody.WriteAsync(compressedBody);
    }

    /// <summary>
    /// Reads the buffered response as a sequence of gRPC-Web frames and gzip-compresses
    /// the payload of each data frame.  Trailer frames are forwarded unchanged.
    /// </summary>
    private async Task<byte[]> CompressGrpcFramesAsync(Stream source)
    {
        using var output = new MemoryStream();

        var headerBuf = new byte[5];
        while (true)
        {
            var bytesRead = await ReadExactAsync(source, headerBuf, 0, 5);
            if (bytesRead == 0) break; // end of stream
            if (bytesRead < 5) { await output.WriteAsync(headerBuf, 0, bytesRead); break; }

            var flags = headerBuf[0];
            var length = ReadInt32BigEndian(headerBuf, 1);

            var payload = new byte[length];
            await ReadExactAsync(source, payload, 0, length);

            // Trailer frame (bit 7 set) — forward as-is
            if ((flags & 0x80) != 0)
            {
                await output.WriteAsync(headerBuf);
                await output.WriteAsync(payload);
                continue;
            }

            // Data frame — compress if large enough
            if (length >= MinCompressibleSize)
            {
                using var compressedStream = new MemoryStream();
                await using (var gzip = new GZipStream(compressedStream, CompressionLevel.Fastest, leaveOpen: true))
                    await gzip.WriteAsync(payload);

                var compressed = compressedStream.ToArray();

                // Set compression flag (bit 0) and write updated header
                var newHeader = new byte[5];
                newHeader[0] = (byte)(flags | 0x01);
                WriteInt32BigEndian(newHeader, 1, compressed.Length);

                await output.WriteAsync(newHeader);
                await output.WriteAsync(compressed);

                _logger.LogDebug(
                    "gRPC-Web frame compressed {Original} → {Compressed} bytes",
                    length,
                    compressed.Length);
            }
            else
            {
                // Too small to bother compressing — forward unchanged
                await output.WriteAsync(headerBuf);
                await output.WriteAsync(payload);
            }
        }

        return output.ToArray();
    }

    private static async Task<int> ReadExactAsync(Stream stream, byte[] buffer, int offset, int count)
    {
        var total = 0;
        while (total < count)
        {
            var read = await stream.ReadAsync(buffer, offset + total, count - total);
            if (read == 0) break;
            total += read;
        }
        return total;
    }

    private static int ReadInt32BigEndian(byte[] buf, int offset)
        => (buf[offset] << 24) | (buf[offset + 1] << 16) | (buf[offset + 2] << 8) | buf[offset + 3];

    private static void WriteInt32BigEndian(byte[] buf, int offset, int value)
    {
        buf[offset]     = (byte)(value >> 24);
        buf[offset + 1] = (byte)(value >> 16);
        buf[offset + 2] = (byte)(value >> 8);
        buf[offset + 3] = (byte)value;
    }

    private static bool IsGrpcWebRequest(HttpRequest request)
    {
        var contentType = request.ContentType ?? string.Empty;
        return contentType.StartsWith("application/grpc-web", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ClientAcceptsGzip(HttpRequest request)
    {
        var acceptEncoding = request.Headers["grpc-accept-encoding"].ToString();
        return acceptEncoding.Contains("gzip", StringComparison.OrdinalIgnoreCase);
    }
}
