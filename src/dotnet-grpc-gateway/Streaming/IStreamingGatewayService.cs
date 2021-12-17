#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Streaming;

/// <summary>
/// Provides full-duplex streaming capabilities with integrated backpressure and flow control
/// for proxying gRPC streams between callers and downstream services.
/// </summary>
public interface IStreamingGatewayService
{
    /// <summary>
    /// Opens a bidirectional streaming session to the downstream gRPC service described
    /// by <paramref name="request"/>. Returns the session descriptor used for all
    /// subsequent read/write operations.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the maximum concurrent session limit is reached.
    /// </exception>
    /// <exception cref="DotNetGrpcGateway.Exceptions.RouteResolutionException">
    /// Thrown when no route matches the requested service and method.
    /// </exception>
    Task<StreamSession> OpenBidirectionalStreamAsync(
        StreamSessionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes a single <see cref="StreamFrame"/> into the session's outbound channel.
    /// Suspends the caller when backpressure is active or the channel buffer is full.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when <paramref name="sessionId"/> is unknown.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the session is not accepting writes.</exception>
    /// <exception cref="TimeoutException">
    /// Thrown when flow-control credits cannot be acquired within the configured deadline.
    /// </exception>
    Task WriteFrameAsync(
        string sessionId,
        StreamFrame frame,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the next available <see cref="StreamFrame"/> from the session's inbound channel.
    /// Returns <see langword="null"/> when the stream is closed or fully drained.
    /// </summary>
    Task<StreamFrame?> ReadFrameAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Projects all inbound frames as an async sequence, completing when the stream
    /// ends or <paramref name="cancellationToken"/> is cancelled.
    /// </summary>
    IAsyncEnumerable<StreamFrame> ConsumeInboundStreamAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Initiates graceful shutdown: stops accepting new outbound writes, drains any
    /// buffered frames, then signals the downstream service.
    /// </summary>
    Task CloseSessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a real-time statistics snapshot for the given session,
    /// or <see langword="null"/> when no such session exists.
    /// </summary>
    Task<StreamingStats?> GetSessionStatsAsync(
        string sessionId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Manages per-session sliding-window flow-control credits used to prevent the gateway
/// from overwhelming downstream gRPC services.
/// </summary>
public interface IFlowController
{
    /// <summary>
    /// Attempts to acquire <paramref name="bytes"/> credits from the current window.
    /// Waits asynchronously when the window is exhausted; returns <see langword="false"/>
    /// only when the configured acquisition timeout elapses.
    /// </summary>
    ValueTask<bool> TryAcquireCreditsAsync(
        string sessionId,
        int bytes,
        CancellationToken cancellationToken = default);

    /// <summary>Returns unused credits to the pool (e.g. after a partial write or write failure).</summary>
    void ReturnCredits(string sessionId, int bytes);

    /// <summary>
    /// Applies a <see cref="BackpressureSignal"/> received from a downstream service,
    /// adjusting the effective window size and optionally stalling the sender.
    /// </summary>
    void ApplyBackpressure(string sessionId, BackpressureSignal signal);

    /// <summary>Returns the live <see cref="FlowControlWindow"/> for the session, or <see langword="null"/>.</summary>
    FlowControlWindow? GetWindow(string sessionId);

    /// <summary>Opens a new window seeded with <paramref name="initialWindowSize"/> bytes.</summary>
    void InitialiseWindow(string sessionId, int initialWindowSize);

    /// <summary>Releases all state when the session ends.</summary>
    void ReleaseWindow(string sessionId);
}

/// <summary>
/// Tracks buffer utilisation across active streams and determines when to activate or
/// deactivate backpressure on behalf of each session.
/// </summary>
public interface IBackpressureMonitor
{
    /// <summary>Records a buffer-utilisation sample for the session.</summary>
    void RecordBufferUsage(string sessionId, int pendingBytes, int capacity);

    /// <summary>
    /// Returns the latest <see cref="BackpressureMetrics"/> for the session,
    /// or <see langword="null"/> if no samples have been recorded yet.
    /// </summary>
    BackpressureMetrics? GetMetrics(string sessionId);

    /// <summary>Returns <see langword="true"/> when write operations should be throttled.</summary>
    bool ShouldThrottle(string sessionId);

    /// <summary>Returns <see langword="true"/> when a throttled session may safely resume.</summary>
    bool ShouldResume(string sessionId);

    /// <summary>Removes all tracking state for a closed session.</summary>
    void RemoveSession(string sessionId);
}

/// <summary>
/// Manages the full lifecycle of <see cref="StreamSession"/> instances —
/// creation, state transitions, and removal.
/// </summary>
public interface IStreamSessionManager
{
    /// <summary>
    /// Allocates a new <see cref="StreamSession"/> with bounded channels configured
    /// according to <paramref name="options"/> and registers it for tracking.
    /// </summary>
    StreamSession CreateSession(string serviceName, string methodName, StreamingOptions options);

    /// <summary>Returns an active session by its ID, or <see langword="null"/>.</summary>
    StreamSession? GetSession(string sessionId);

    /// <summary>
    /// Atomically transitions the session from <paramref name="from"/> to
    /// <paramref name="to"/>. Returns <see langword="false"/> when the current state
    /// does not match <paramref name="from"/>.
    /// </summary>
    bool TryTransitionState(string sessionId, StreamState from, StreamState to);

    /// <summary>Removes a fully closed session from the registry.</summary>
    void RemoveSession(string sessionId);

    /// <summary>Returns a point-in-time snapshot of all currently tracked sessions.</summary>
    IReadOnlyList<StreamSession> GetActiveSessions();
}
