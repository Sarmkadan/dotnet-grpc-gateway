// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Domain;

/// <summary>
/// Holds reflection metadata discovered from a registered gRPC service's
/// Server Reflection endpoint.
/// </summary>
public class ServiceReflectionInfo
{
    /// <summary>Gets or sets the identifier of the associated <see cref="GrpcService"/>.</summary>
    public int ServiceId { get; set; }

    /// <summary>Gets or sets the short display name of the service.</summary>
    public string ServiceName { get; set; } = null!;

    /// <summary>Gets or sets the fully-qualified gRPC service name (package.ServiceName).</summary>
    public string ServiceFullName { get; set; } = null!;

    /// <summary>Gets or sets the RPC method descriptors exposed by this service.</summary>
    public List<ServiceMethodDescriptor> Methods { get; set; } = new();

    /// <summary>Gets or sets when the reflection data was last retrieved.</summary>
    public DateTime ReflectedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets whether the reflection endpoint responded successfully.</summary>
    public bool IsAvailable { get; set; }

    /// <summary>Gets or sets a diagnostic message when the reflection probe fails.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Gets the total number of RPC methods discovered.</summary>
    public int MethodCount => Methods.Count;

    /// <summary>Gets the number of streaming methods (client, server, or bidirectional).</summary>
    public int StreamingMethodCount => Methods.Count(m => m.IsClientStreaming || m.IsServerStreaming);
}

/// <summary>
/// Describes a single RPC method within a gRPC service as reported by the
/// Server Reflection protocol.
/// </summary>
public class ServiceMethodDescriptor
{
    /// <summary>Gets or sets the unqualified RPC method name.</summary>
    public string Name { get; set; } = null!;

    /// <summary>Gets or sets the fully-qualified protobuf type of the request message.</summary>
    public string RequestType { get; set; } = null!;

    /// <summary>Gets or sets the fully-qualified protobuf type of the response message.</summary>
    public string ResponseType { get; set; } = null!;

    /// <summary>Gets or sets whether the client sends a stream of request messages.</summary>
    public bool IsClientStreaming { get; set; }

    /// <summary>Gets or sets whether the server sends a stream of response messages.</summary>
    public bool IsServerStreaming { get; set; }

    /// <summary>Gets a human-readable label for the streaming mode of this method.</summary>
    public string StreamingMode =>
        (IsClientStreaming, IsServerStreaming) switch
        {
            (true, true)  => "BidirectionalStreaming",
            (true, false) => "ClientStreaming",
            (false, true) => "ServerStreaming",
            _             => "Unary"
        };
}
