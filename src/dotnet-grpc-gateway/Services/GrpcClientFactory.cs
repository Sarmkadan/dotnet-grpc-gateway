#nullable enable
// ====================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

using System.Diagnostics;
using DotNetGrpcGateway.Domain;

namespace DotNetGrpcGateway.Services;

/// <summary>
/// Factory implementation for creating and caching HTTP clients for downstream gRPC service communication.
/// Manages per-service client lifecycle, TLS configuration, and provides both unary
/// and server-streaming invocation methods.
/// </summary>
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
        => CreateHttpClient(service, routeChannelOptions: null);

    public HttpClient CreateHttpClient(GrpcService service, RouteChannelOptions? routeChannelOptions)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        // When route-level overrides are present we build a dedicated client so the
        // gateway-level cached client is not accidentally mutated.
        if (routeChannelOptions is not null)
            return BuildHttpClient(service, routeChannelOptions);

        if (_clientCache.TryGetValue(service.Id, out var cachedClient))
            return cachedClient;

        var client = BuildHttpClient(service, routeChannelOptions: null);
        _clientCache[service.Id] = client;
        return client;
    }

    private HttpClient BuildHttpClient(GrpcService service, RouteChannelOptions? routeChannelOptions)
    {
        var skipTls = routeChannelOptions?.SkipTlsVerification ?? false;
        var handler = new HttpClientHandler();
        if (!service.UseTls || skipTls)
        {
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        }

        var timeout = routeChannelOptions?.CallTimeout ?? TimeSpan.FromSeconds(30);
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri(service.GetEndpointUri()),
            Timeout = timeout
        };

        client.DefaultRequestHeaders.Add("User-Agent", "DotNetGrpcGateway/1.0");

        // Propagate correlation ID from current Activity if available
        // This ensures downstream services receive the W3C trace context
        var activity = Activity.Current;
        if (activity != null)
        {
            // Add traceparent header for W3C Trace Context
            var traceParent = $"00-{activity.TraceId: x32}-{activity.SpanId: x16}-{(byte)(activity.ActivityTraceFlags & ActivityTraceFlags.Recorded):x2}";
            client.DefaultRequestHeaders.Add("traceparent", traceParent);

            // Also add legacy correlation ID header for backward compatibility
            if (!string.IsNullOrEmpty(activity.TraceId.ToHexString()))
            {
                client.DefaultRequestHeaders.Add("X-Correlation-ID", activity.TraceId.ToHexString());
            }
        }

        if (routeChannelOptions?.AdditionalHeaders is { Count: > 0 } headers)
        {
            foreach (var (key, value) in headers)
                client.DefaultRequestHeaders.TryAddWithoutValidation(key, value);
        }

        _logger.LogInformation(
            "Created HTTP client for service {ServiceName} at {Endpoint} (timeout={Timeout})",
            service.Name, service.GetEndpointUri(), timeout);

        return client;
    }

    public async Task<T> InvokeAsync<T>(GrpcService service, string methodName, object request, CancellationToken cancellationToken = default) where T : class
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
                    service.Name, methodName, response.StatusCode);

                throw new HttpRequestException($"Service returned {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("Response from {Service}.{Method}: {Content}", service.Name, methodName, content);

            // In real implementation, would deserialize response to T
            return null! as T;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invoking {Service}.{Method}", service.Name, methodName);
            throw;
        }
    }

    public async Task<Stream> InvokeStreamingAsync(GrpcService service, string methodName, object request, CancellationToken cancellationToken = default)
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
                throw new HttpRequestException($"Service returned {response.StatusCode}");

            return await response.Content.ReadAsStreamAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invoking streaming {Service}.{Method}", service.Name, methodName);
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