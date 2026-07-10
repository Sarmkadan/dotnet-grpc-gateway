#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Streaming;

public enum StreamState
{
	Open,
	Closing,
	Closed,
	Faulted
}

public enum BackpressureSignal
{
	None,
	SlowDown,
	Resume
}

/// <summary>
/// Represents a client request to initiate a streaming session with a gRPC service.
/// Contains the service and method to call, optional route path, and custom headers.
/// </summary>
public class StreamSessionRequest
{
	/// <summary>
	/// Gets or sets the name of the gRPC service to stream from.
	/// </summary>
	public string ServiceName { get; set; } = null!;

	/// <summary>
	/// Gets or sets the name of the gRPC method to invoke for streaming.
	/// </summary>
	public string MethodName { get; set; } = null!;

	/// <summary>
	/// Gets or sets the optional custom route path for the gRPC endpoint.
	/// If not specified, uses the default route for the service and method.
	/// </summary>
	public string? RoutePath { get; set; }

	/// <summary>
	/// Gets or sets the collection of HTTP headers to include in the streaming request.
	/// Headers are sent with each frame in the streaming session.
	/// </summary>
	public Dictionary<string, string> Headers { get; set; } = new();
}

/// <summary>
/// Represents a single frame of data in a streaming session.
/// Contains the payload data, end-of-stream marker, and optional content type.
/// </summary>
public class StreamFrame
{
	/// <summary>
	/// Gets or sets the binary payload data for this frame.
	/// Defaults to an empty byte array if not set.
	/// </summary>
	public byte[] Payload { get; set; } = Array.Empty<byte>();

	/// <summary>
	/// Gets or sets a value indicating whether this is the final frame in the stream.
	/// When true, indicates the stream should be closed after processing this frame.
	/// </summary>
	public bool EndOfStream { get; set; }

	/// <summary>
	/// Gets or sets the MIME type of the payload content.
	/// Used for content negotiation and proper deserialization.
	/// </summary>
	public string? ContentType { get; set; }
}

/// <summary>
/// Configuration options for streaming behavior and performance tuning.
/// Controls buffer sizes, timeouts, and flow control parameters.
/// </summary>
public class StreamingOptions
{
	/// <summary>
	/// Gets or sets the initial window size in bytes for flow control.
	/// Defaults to 65,535 bytes (64 KB).
	/// </summary>
	public int InitialWindowSizeBytes { get; set; } = 65_535;

	/// <summary>
	/// Gets or sets the maximum number of frames that can be buffered in memory.
	/// Defaults to 128 frames.
	/// </summary>
	public int ChannelCapacity { get; set; } = 128;

	/// <summary>
	/// Gets or sets the timeout for write operations.
	/// Defaults to 30 seconds.
	/// </summary>
	public TimeSpan WriteTimeout { get; set; } = TimeSpan.FromSeconds(30);
}

/// <summary>
/// Tracks flow control state for a streaming session.
/// Monitors available credits and throttling status.
/// </summary>
public class FlowControlWindow
{
	/// <summary>
	/// Gets or sets the initial credit size allocated for the stream.
	/// </summary>
	public int InitialSize { get; set; }

	/// <summary>
	/// Gets or sets the currently available credit bytes that can be sent.
	/// </summary>
	public int AvailableCredits { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the stream is currently throttled.
	/// When true, the sender should pause or reduce transmission rate.
	/// </summary>
	public bool IsThrottled { get; set; }
}

/// <summary>
/// Metrics and statistics related to backpressure and flow control.
/// Provides visibility into streaming performance and resource utilization.
/// </summary>
public class BackpressureMetrics
{
	/// <summary>
	/// Gets or sets the number of bytes currently pending in the buffer.
	/// </summary>
	public int PendingBytes { get; set; }

	/// <summary>
	/// Gets or sets the total buffer capacity in bytes.
	/// </summary>
	public int Capacity { get; set; }

	/// <summary>
	/// Gets or sets the percentage of buffer utilization (0.0 to 100.0).
	/// </summary>
	public double UtilisationPct { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the system is currently applying backpressure.
	/// When true, senders should slow down or pause transmission.
	/// </summary>
	public bool IsThrottled { get; set; }
}

/// <summary>
/// Performance statistics and state for an active streaming session.
/// Tracks frame counts, byte counts, and session lifecycle information.
/// </summary>
public class StreamingStats
{
	/// <summary>
	/// Gets or sets the unique identifier for this streaming session.
	/// </summary>
	public string SessionId { get; set; } = null!;

	/// <summary>
	/// Gets or sets the current state of the streaming session.
	/// </summary>
	public StreamState State { get; set; }

	/// <summary>
	/// Gets or sets the total number of frames received from the stream.
	/// </summary>
	public long FramesRead { get; set; }

	/// <summary>
	/// Gets or sets the total number of frames sent through the stream.
	/// </summary>
	public long FramesWritten { get; set; }

	/// <summary>
	/// Gets or sets the total number of bytes received from the stream.
	/// </summary>
	public long BytesRead { get; set; }

	/// <summary>
	/// Gets or sets the total number of bytes sent through the stream.
	/// </summary>
	public long BytesWritten { get; set; }

	/// <summary>
	/// Gets or sets the timestamp when this streaming session started.
	/// Defaults to the current UTC time when the object is created.
	/// </summary>
	public DateTime StartedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents an active streaming session between a client and a gRPC service.
/// Tracks session state, configuration, and lifecycle information.
/// </summary>
public class StreamSession
{
	/// <summary>
	/// Gets or sets the unique session identifier.
	/// Generated as a GUID without hyphens when not explicitly set.
	/// </summary>
	public string SessionId { get; set; } = Guid.NewGuid().ToString("N");

	/// <summary>
	/// Gets or sets the name of the gRPC service being streamed from.
	/// </summary>
	public string ServiceName { get; set; } = null!;

	/// <summary>
	/// Gets or sets the name of the gRPC method being invoked for streaming.
	/// </summary>
	public string MethodName { get; set; } = null!;

	/// <summary>
	/// Gets or sets the current state of the streaming session.
	/// Defaults to Open when a new session is created.
	/// </summary>
	public StreamState State { get; set; } = StreamState.Open;

	/// <summary>
	/// Gets or sets the timestamp when this session was created.
	/// Defaults to the current UTC time when the object is created.
	/// </summary>
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	/// <summary>
	/// Gets or sets the streaming configuration options for this session.
	/// Defaults to a new StreamingOptions instance with default values.
	/// </summary>
	public StreamingOptions Options { get; set; } = new();
}