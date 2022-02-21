#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System;

namespace DotNetGrpcGateway.Domain;

/// <summary>
/// A single recorded request/response log entry captured by the gateway.
/// </summary>
public class RequestLogEntry
{
	public Guid Id { get; set; } = Guid.NewGuid();

	public DateTime Timestamp { get; set; } = DateTime.UtcNow;

	/// <summary>Log level for this entry.</summary>
	public string? LogLevel { get; set; }

	/// <summary>Log message.</summary>
	public string? Message { get; set; }

	/// <summary>Request ID.</summary>
	public string? RequestId { get; set; }

	/// <summary>Service name.</summary>
	public string? ServiceName { get; set; }

	/// <summary>Method name.</summary>
	public string? MethodName { get; set; }

	/// <summary>HTTP method (POST for gRPC).</summary>
	public string Method { get; set; } = null!;

	/// <summary>Request path (e.g. /package.Service/Method).</summary>
	public string Path { get; set; } = null!;

	/// <summary>Resolved gRPC method name derived from the path.</summary>
	public string GrpcMethod { get; set; } = null!;

	/// <summary>HTTP status code of the response.</summary>
	public int HttpStatusCode { get; set; }

	/// <summary>Total request processing duration in milliseconds.</summary>
	public long DurationMs { get; set; }

	/// <summary>Client IP address.</summary>
	public string? ClientIp { get; set; }

	/// <summary>Upstream service address the request was forwarded to.</summary>
	public string? UpstreamAddress { get; set; }

	/// <summary>Request headers (sensitive values redacted).</summary>
	public Dictionary<string, string> RequestHeaders { get; set; } = new();

	/// <summary>Request body size in bytes.</summary>
	public long RequestSizeBytes { get; set; }

	/// <summary>Response body size in bytes.</summary>
	public long ResponseSizeBytes { get; set; }

	/// <summary>Error message if the request failed.</summary>
	public string? ErrorMessage { get; set; }

	/// <summary>Whether the request completed successfully (status &lt; 400).</summary>
	public bool IsSuccessful { get; set; }

	/// <summary>Whether the request was served from cache.</summary>
	public bool CacheHit { get; set; }

	/// <summary>Number of retry attempts for this request.</summary>
	public int RetryCount { get; set; }

	/// <summary>Stack trace of any exception that occurred during processing.</summary>
	public string? StackTrace { get; set; }

	/// <summary>HTTP status code of the response (alias for HttpStatusCode).</summary>
	[Obsolete("Use HttpStatusCode instead")]
	public int StatusCode
	{
		get => HttpStatusCode;
		set => HttpStatusCode = value;
	}

	/// <summary>Total request processing duration in milliseconds (alias for DurationMs).</summary>
	[Obsolete("Use DurationMs instead")]
	public long Duration
	{
		get => DurationMs;
		set => DurationMs = value;
	}

	/// <summary>Request body size in bytes (alias for RequestSizeBytes).</summary>
	[Obsolete("Use RequestSizeBytes instead")]
	public long RequestBodyBytes
	{
		get => RequestSizeBytes;
		set => RequestSizeBytes = value;
	}

	/// <summary>Response body size in bytes (alias for ResponseSizeBytes).</summary>
	[Obsolete("Use ResponseSizeBytes instead")]
	public long ResponseBodyBytes
	{
		get => ResponseSizeBytes;
		set => ResponseSizeBytes = value;
	}

	/// <summary>Whether the request completed successfully (status &lt; 400) (alias for IsSuccessful).</summary>
	[Obsolete("Use IsSuccessful instead")]
	public bool IsSuccess
	{
		get => IsSuccessful;
		set => IsSuccessful = value;
	}
}