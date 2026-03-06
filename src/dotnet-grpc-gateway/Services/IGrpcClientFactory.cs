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

public class GrpcClientFactory : IGrpcClientFactory
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GrpcClientFactory> _logger;
    private readonly Dictionary<int, HttpClient> _clientCache = new();

    public GrpcClientFactory(HttpClient httpClient, ILogger<GrpcClientFactory> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public HttpClient CreateHttpClient(GrpcService service)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        if (_clientCache.TryGetValue(service.Id, out var cachedClient))
            return cachedClient;

        var handler = new HttpClientHandler();
        if (!service.UseTls)
        {
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        }

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri(service.GetEndpointUri()),
            Timeout = TimeSpan.FromSeconds(30)
        };

        client.DefaultRequestHeaders.Add("User-Agent", "DotNetGrpcGateway/1.0");

        _clientCache[service.Id] = client;

        _logger.LogInformation(
            "Created HTTP client for service {ServiceName} at {Endpoint}",
            service.Name,
            service.GetEndpointUri());

        return client;
    }

    public async Task<T> InvokeAsync<T>(GrpcService service, string methodName, object request, CancellationToken cancellationToken) where T : class
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        if (string.IsNullOrWhiteSpace(methodName))
            throw new ArgumentException("Method name is required", nameof(methodName));

        if (request is null)
            throw new ArgumentNullException(nameof(request));

        try
        {
            var client = CreateHttpClient(service);

            // In real implementation, would serialize request and call gRPC service
            var response = await client.GetAsync($"/{service.ServiceFullName}/{methodName}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Service {ServiceName}.{Method} returned status {Status}",
                    service.Name,
                    methodName,
                    response.StatusCode);

                throw new HttpRequestException($"Service returned {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("Response from {Service}.{Method}: {Content}", service.Name, methodName, content);

            // In real implementation, would deserialize response to T
            return null! as T;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error invoking {Service}.{Method}",
                service.Name,
                methodName);

            throw;
        }
    }

    public async Task<Stream> InvokeStreamingAsync(GrpcService service, string methodName, object request, CancellationToken cancellationToken)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        try
        {
            var client = CreateHttpClient(service);
            var response = await client.GetAsync(
                $"/{service.ServiceFullName}/{methodName}",
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Service returned {response.StatusCode}");
            }

            return await response.Content.ReadAsStreamAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error invoking streaming {Service}.{Method}",
                service.Name,
                methodName);

            throw;
        }
    }

    public void ClearClientCache()
    {
        foreach (var client in _clientCache.Values)
        {
            client?.Dispose();
        }

        _clientCache.Clear();
        _logger.LogInformation("gRPC client cache cleared");
    }
}
