// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net.Http.Headers;

namespace DotNetGrpcGateway.Integration;

/// <summary>
/// Factory for creating and managing HTTP clients with standardized configuration.
/// Manages connection pooling, timeouts, and default headers.
/// </summary>
public interface IHttpClientProvider
{
    HttpClient CreateClient(string name, HttpClientOptions? options = null);
    HttpClient GetClient(string name);
    void RemoveClient(string name);
}

/// <summary>
/// Configuration options for HTTP clients.
/// </summary>
public class HttpClientOptions
{
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public int MaxRetries { get; set; } = 3;
    public bool AllowAutoRedirect { get; set; } = true;
    public int MaxConnectionsPerServer { get; set; } = 10;
    public Dictionary<string, string>? DefaultHeaders { get; set; }
}

/// <summary>
/// HTTP client provider implementation.
/// </summary>
public class HttpClientProvider : IHttpClientProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpClientProvider> _logger;
    private readonly Dictionary<string, HttpClient> _clients = new();

    public HttpClientProvider(IHttpClientFactory httpClientFactory, ILogger<HttpClientProvider> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public HttpClient CreateClient(string name, HttpClientOptions? options = null)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Client name cannot be null or empty", nameof(name));

        options ??= new HttpClientOptions();

        try
        {
            var client = _httpClientFactory.CreateClient(name);

            // Configure client
            client.Timeout = options.Timeout;

            // Set default headers
            if (options.DefaultHeaders != null)
            {
                foreach (var header in options.DefaultHeaders)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }

            // Add User-Agent if not already set
            if (!client.DefaultRequestHeaders.Contains("User-Agent"))
            {
                client.DefaultRequestHeaders.UserAgent.Add(
                    new ProductInfoHeaderValue("dotnet-grpc-gateway", "1.0"));
            }

            _clients[name] = client;
            _logger.LogInformation("HTTP client created: {ClientName} (Timeout: {Timeout}s)",
                name, options.Timeout.TotalSeconds);

            return client;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating HTTP client: {ClientName}", name);
            throw;
        }
    }

    public HttpClient GetClient(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Client name cannot be null or empty", nameof(name));

        if (_clients.TryGetValue(name, out var client))
            return client;

        // Create default client if not exists
        _logger.LogDebug("Client {ClientName} not found, creating default instance", name);
        return CreateClient(name);
    }

    public void RemoveClient(string name)
    {
        if (string.IsNullOrEmpty(name))
            return;

        if (_clients.Remove(name, out var client))
        {
            client?.Dispose();
            _logger.LogInformation("HTTP client removed: {ClientName}", name);
        }
    }
}
