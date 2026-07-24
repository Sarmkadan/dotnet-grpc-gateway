#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Domain;

namespace DotNetGrpcGateway.Services;

/// <summary>
/// Factory for creating and caching HTTP clients for downstream gRPC service communication.
/// Manages per-service client lifecycle, TLS configuration, and provides both unary
/// and server-streaming invocation methods.
/// </summary>
public interface IGrpcClientFactory
{
    /// <summary>
    /// Creates or retrieves a cached <see cref="HttpClient"/> configured for the specified gRPC service.
    /// TLS certificate validation is relaxed when <see cref="GrpcService.UseTls"/> is <c>false</c>.
    /// </summary>
    /// <param name="service">The downstream gRPC service definition with endpoint and TLS settings.</param>
    /// <returns>A configured <see cref="HttpClient"/> for the service endpoint.</returns>
    HttpClient CreateHttpClient(GrpcService service);

    /// <summary>
    /// Creates or retrieves a cached <see cref="HttpClient"/> configured for the specified gRPC service,
    /// with per-route overrides applied on top of the service defaults.
    /// </summary>
    /// <param name="service">The downstream gRPC service definition.</param>
    /// <param name="routeChannelOptions">Per-route channel overrides; may be <see langword="null"/>.</param>
    /// <returns>A configured <see cref="HttpClient"/> for the service endpoint.</returns>
    HttpClient CreateHttpClient(GrpcService service, RouteChannelOptions? routeChannelOptions);

    /// <summary>
    /// Invokes a unary gRPC method on the specified service and returns the deserialized response.
    /// </summary>
    /// <typeparam name="T">The expected response type.</typeparam>
    /// <param name="service">The target gRPC service.</param>
    /// <param name="methodName">The gRPC method name to invoke.</param>
    /// <param name="request">The request payload to serialize and send.</param>
    /// <param name="cancellationToken">Token to cancel the call.</param>
    /// <returns>The deserialized response of type <typeparamref name="T"/>.</returns>
    Task<T> InvokeAsync<T>(GrpcService service, string methodName, object request, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Invokes a server-streaming gRPC method and returns the response body as a readable stream.
    /// Uses <see cref="HttpCompletionOption.ResponseHeadersRead"/> for efficient streaming.
    /// </summary>
    /// <param name="service">The target gRPC service.</param>
    /// <param name="methodName">The gRPC method name to invoke.</param>
    /// <param name="request">The request payload.</param>
    /// <param name="cancellationToken">Token to cancel the call.</param>
    /// <returns>A <see cref="Stream"/> for reading the server-streamed response.</returns>
    Task<Stream> InvokeStreamingAsync(GrpcService service, string methodName, object request, CancellationToken cancellationToken = default);
}

