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

public class StreamSessionRequest
{
    public string ServiceName { get; set; } = null!;
    public string MethodName { get; set; } = null!;
    public string? RoutePath { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
}

public class StreamFrame
{
    public byte[] Payload { get; set; } = Array.Empty<byte>();
    public bool EndOfStream { get; set; }
    public string? ContentType { get; set; }
}

public class StreamingOptions
{
    public int InitialWindowSizeBytes { get; set; } = 65_535;
    public int ChannelCapacity { get; set; } = 128;
    public TimeSpan WriteTimeout { get; set; } = TimeSpan.FromSeconds(30);
}

public class FlowControlWindow
{
    public int InitialSize { get; set; }
    public int AvailableCredits { get; set; }
    public bool IsThrottled { get; set; }
}

public class BackpressureMetrics
{
    public int PendingBytes { get; set; }
    public int Capacity { get; set; }
    public double UtilisationPct { get; set; }
    public bool IsThrottled { get; set; }
}

public class StreamingStats
{
    public string SessionId { get; set; } = null!;
    public StreamState State { get; set; }
    public long FramesRead { get; set; }
    public long FramesWritten { get; set; }
    public long BytesRead { get; set; }
    public long BytesWritten { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
}

public class StreamSession
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString("N");
    public string ServiceName { get; set; } = null!;
    public string MethodName { get; set; } = null!;
    public StreamState State { get; set; } = StreamState.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public StreamingOptions Options { get; set; } = new();
}
