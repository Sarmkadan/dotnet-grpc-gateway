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

	private string? _logLevel;
	private string? _message;

	/// <summary>
	/// Log level for this entry. When not set explicitly, it is derived from the
	/// entry state: ERROR for failed requests, WARN for slow, oversized, or retried
	/// requests, and INFO otherwise.
	/// </summary>
	public string? LogLevel
	{
		get => _logLevel ?? ComputeLogLevel();
		set => _logLevel = value;
	}

	/// <summary>
	/// Log message. When not set explicitly, it is composed from the entry state
	/// (outcome, service/method, status, duration, cache, retry, and size details).
	/// </summary>
	public string? Message
	{
		get => _message ?? ComposeMessage();
		set => _message = value;
	}

	/// <summary>Request ID.</summary>
	public string? RequestId { get; set; } = Guid.NewGuid().ToString();

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
	public bool IsSuccessful { get; set; } = true;

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

	/// <summary>Duration above which a request is considered slow (milliseconds).</summary>
	private const long SlowRequestThresholdMs = 1000;

	/// <summary>Combined payload size above which a request is considered large (bytes).</summary>
	private const long LargePayloadThresholdBytes = 1024 * 1024;

	private string ComputeLogLevel()
	{
		if (!IsSuccessful)
			return "ERROR";

		if (RetryCount > 0 ||
			DurationMs > SlowRequestThresholdMs ||
			RequestSizeBytes + ResponseSizeBytes > LargePayloadThresholdBytes)
		{
			return "WARN";
		}

		return "INFO";
	}

	private string ComposeMessage()
	{
		var target = string.IsNullOrEmpty(ServiceName) && string.IsNullOrEmpty(MethodName)
			? GrpcMethod ?? Path ?? "unknown"
			: $"{ServiceName}.{MethodName}";

		var parts = new List<string>
		{
			IsSuccessful ? $"Request completed - {target}" : $"Request failed - {target}",
			$"Status: {HttpStatusCode}",
			$"Duration: {DurationMs}ms"
		};

		if (!IsSuccessful && !string.IsNullOrEmpty(ErrorMessage))
			parts.Add(ErrorMessage);

		if (DurationMs > SlowRequestThresholdMs)
			parts.Add($"Slow request ({DurationMs}ms)");

		if (RequestSizeBytes + ResponseSizeBytes > LargePayloadThresholdBytes)
			parts.Add($"Large request/response ({RequestSizeBytes + ResponseSizeBytes} bytes)");

		if (RetryCount > 0)
			parts.Add($"Retry count: {RetryCount}");

		parts.Add(CacheHit ? "Cache HIT" : "Cache MISS");

		return string.Join(" - ", parts);
	}
}