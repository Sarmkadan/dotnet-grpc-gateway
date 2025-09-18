// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Domain;

namespace DotNetGrpcGateway.Services;

/// <summary>
/// Factory for creating gRPC clients to downstream services
/// </summary>
public interface IGrpcClientFactory
{
    HttpClient CreateHttpClient(GrpcService service);
    Task<T> InvokeAsync<T>(GrpcService service, string methodName, object request) where T : class;
    Task<Stream> InvokeStreamingAsync(GrpcService service, string methodName, object request);
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
        if (service == null)
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

    public async Task<T> InvokeAsync<T>(GrpcService service, string methodName, object request) where T : class
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));

        if (string.IsNullOrWhiteSpace(methodName))
            throw new ArgumentException("Method name is required", nameof(methodName));

        if (request == null)
            throw new ArgumentNullException(nameof(request));

        try
        {
            var client = CreateHttpClient(service);

            // In real implementation, would serialize request and call gRPC service
            var response = await client.GetAsync($"/{service.ServiceFullName}/{methodName}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Service {ServiceName}.{Method} returned status {Status}",
                    service.Name,
                    methodName,
                    response.StatusCode);

                throw new HttpRequestException($"Service returned {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
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

    public async Task<Stream> InvokeStreamingAsync(GrpcService service, string methodName, object request)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));

        try
        {
            var client = CreateHttpClient(service);
            var response = await client.GetAsync(
                $"/{service.ServiceFullName}/{methodName}",
                HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Service returned {response.StatusCode}");
            }

            return await response.Content.ReadAsStreamAsync();
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
