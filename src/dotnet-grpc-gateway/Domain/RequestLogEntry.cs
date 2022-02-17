#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Domain;

/// <summary>
/// A single recorded request/response log entry captured by the gateway.
/// </summary>
public class RequestLogEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>HTTP method (POST for gRPC).</summary>
    public string Method { get; set; } = null!;

    /// <summary>Request path (e.g. /package.Service/Method).</summary>
    public string Path { get; set; } = null!;

    /// <summary>Resolved gRPC method name derived from the path.</summary>
    public string GrpcMethod { get; set; } = null!;

    /// <summary>HTTP status code of the response.</summary>
    public int StatusCode { get; set; }

    /// <summary>Total request processing duration in milliseconds.</summary>
    public long DurationMs { get; set; }

    /// <summary>Client IP address.</summary>
    public string? ClientIp { get; set; }

    /// <summary>Upstream service address the request was forwarded to.</summary>
    public string? UpstreamAddress { get; set; }

    /// <summary>Request headers (sensitive values redacted).</summary>
    public Dictionary<string, string> RequestHeaders { get; set; } = new();

    /// <summary>Request body size in bytes.</summary>
    public long RequestBodyBytes { get; set; }

    /// <summary>Response body size in bytes.</summary>
    public long ResponseBodyBytes { get; set; }

    /// <summary>Error message if the request failed.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Whether the request completed successfully (status &lt; 400).</summary>
    public bool IsSuccess => StatusCode > 0 && StatusCode < 400;
}
