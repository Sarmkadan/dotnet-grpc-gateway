#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net.Http.Headers;
using DotNetGrpcGateway.Exceptions;

namespace DotNetGrpcGateway.Integration;

/// <summary>
/// Factory for creating and managing HTTP clients with standardized configuration.
/// Manages connection pooling, timeouts, and default headers.
/// </summary>
public interface IHttpClientProvider
{
    /// <summary>
    /// Creates a new HTTP client with the specified name and optional configuration.
    /// </summary>
    /// <param name="name">The name of the client.</param>
    /// <param name="options">Optional configuration options for the client.</param>
    /// <returns>The created <see cref="HttpClient"/>.</returns>
    HttpClient CreateClient(string name, HttpClientOptions? options = null);

    /// <summary>
    /// Gets an existing HTTP client by name, creating a default one if it does not exist.
    /// </summary>
    /// <param name="name">The name of the client.</param>
    /// <returns>The <see cref="HttpClient"/> associated with the specified name.</returns>
    HttpClient GetClient(string name);

    /// <summary>
    /// Removes and disposes the HTTP client with the specified name, if it exists.
    /// </summary>
    /// <param name="name">The name of the client to remove.</param>
    void RemoveClient(string name);
}

/// <summary>
/// Configuration options for HTTP clients.
/// </summary>
public class HttpClientOptions
{
    /// <summary>
    /// Gets or sets the request timeout. Defaults to 30 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the maximum number of retries. Defaults to 3.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Gets or sets whether automatic redirects are allowed. Defaults to true.
    /// </summary>
    public bool AllowAutoRedirect { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of connections per server. Defaults to 10.
    /// </summary>
    public int MaxConnectionsPerServer { get; set; } = 10;

    /// <summary>
    /// Gets or sets the default headers applied to every request.
    /// </summary>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientProvider"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="logger">The logger.</param>
    public HttpClientProvider(IHttpClientFactory httpClientFactory, ILogger<HttpClientProvider> logger)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(logger);

        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new HTTP client with the specified name and optional configuration.
    /// </summary>
    /// <param name="name">The name of the client.</param>
    /// <param name="options">Optional configuration options for the client.</param>
    /// <returns>The created <see cref="HttpClient"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the client name is null or empty.</exception>
    /// <exception cref="DotnetGrpcGatewayException">Thrown when an error occurs during client creation.</exception>
    public HttpClient CreateClient(string name, HttpClientOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (name.Length == 0)
            throw new ArgumentException("Client name cannot be null or empty", nameof(name));

        options ??= new HttpClientOptions();

        try
        {
            var client = _httpClientFactory.CreateClient(name);

            // Configure client
            client.Timeout = options.Timeout;

            // Set default headers
            if (options.DefaultHeaders is not null)
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
            // Wrap any exception in a custom gateway exception to keep a consistent error model
            _logger.LogError(ex, "Error creating HTTP client: {ClientName}", name);
            throw new DotnetGrpcGatewayException($"Failed to create HTTP client '{name}'.", ex);
        }
    }

    /// <summary>
    /// Gets an existing HTTP client by name, creating a default one if it does not exist.
    /// </summary>
    /// <param name="name">The name of the client.</param>
    /// <returns>The <see cref="HttpClient"/> associated with the specified name.</returns>
    /// <exception cref="ArgumentException">Thrown when the client name is null or empty.</exception>
    public HttpClient GetClient(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (name.Length == 0)
            throw new ArgumentException("Client name cannot be null or empty", nameof(name));

        if (_clients.TryGetValue(name, out var client))
            return client;

        // Create default client if not exists
        _logger.LogDebug("Client {ClientName} not found, creating default instance", name);
        return CreateClient(name);
    }

    /// <summary>
    /// Removes and disposes the HTTP client with the specified name, if it exists.
    /// </summary>
    /// <param name="name">The name of the client to remove.</param>
    public void RemoveClient(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (name.Length == 0)
            return;

        if (_clients.Remove(name, out var client))
        {
            client?.Dispose();
            _logger.LogInformation("HTTP client removed: {ClientName}", name);
        }
    }
}
