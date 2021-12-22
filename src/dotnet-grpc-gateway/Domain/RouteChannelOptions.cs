#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Domain;

/// <summary>
/// Per-route gRPC channel configuration that overrides gateway-level defaults.
/// Attach an instance to <see cref="GatewayRoute.ChannelOptions"/> to customise
/// TLS, timeouts, message sizes, and metadata for a single route's upstream channel.
/// </summary>
public class RouteChannelOptions
{
    /// <summary>
    /// Per-call deadline applied to every RPC on this route.
    /// When <see langword="null"/> the gateway-level <c>RequestTimeoutMs</c> is used.
    /// </summary>
    public TimeSpan? CallTimeout { get; set; }

    /// <summary>
    /// Maximum size in bytes of a single message received from the upstream service.
    /// Overrides the gateway-level <c>MaxReceiveMessageSize</c> when set.
    /// </summary>
    public int? MaxReceiveMessageSize { get; set; }

    /// <summary>
    /// Maximum size in bytes of a single message sent to the upstream service.
    /// Overrides the gateway-level <c>MaxSendMessageSize</c> when set.
    /// </summary>
    public int? MaxSendMessageSize { get; set; }

    /// <summary>
    /// Additional HTTP headers forwarded with every upstream request on this route.
    /// Useful for passing service-account credentials or routing hints.
    /// </summary>
    public Dictionary<string, string> AdditionalHeaders { get; set; } = new();

    /// <summary>
    /// When <see langword="true"/>, the upstream TLS certificate is not validated.
    /// Use only in development or when the certificate is trusted through other means.
    /// </summary>
    public bool SkipTlsVerification { get; set; } = false;

    /// <summary>
    /// Optional SNI host name sent during TLS handshake with the upstream service.
    /// When <see langword="null"/> the host derived from <see cref="GrpcService.Host"/> is used.
    /// </summary>
    public string? TlsTargetName { get; set; }
}
