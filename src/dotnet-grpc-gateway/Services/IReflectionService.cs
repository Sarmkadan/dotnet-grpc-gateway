// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net;
using DotNetGrpcGateway.Configuration;
using DotNetGrpcGateway.Domain;

namespace DotNetGrpcGateway.Services;

/// <summary>
/// Provides gRPC Server Reflection support — discovers and caches the method
/// descriptors of registered back-end services so callers can inspect the API
/// surface at runtime without direct access to .proto source files.
/// </summary>
public interface IReflectionService
{
    /// <summary>
    /// Gets a value indicating whether at least one service has a reachable
    /// reflection endpoint in the current cache snapshot.
    /// </summary>
    bool IsReflectionAvailable { get; }

    /// <summary>
    /// Returns the cached reflection snapshot for a single service, or <c>null</c>
    /// when no data has been fetched yet for that service.
    /// </summary>
    /// <param name="serviceId">Identifier of the registered <see cref="GrpcService"/>.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task<ServiceReflectionInfo?> GetServiceReflectionAsync(int serviceId,
        CancellationToken cancellationToken = default);

    /// <summary>Returns cached reflection snapshots for every registered service.</summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task<IReadOnlyList<ServiceReflectionInfo>> GetAllReflectionInfoAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Probes the gRPC Server Reflection endpoint of a single service and
    /// refreshes the in-memory cache entry.
    /// </summary>
    /// <param name="serviceId">Identifier of the registered <see cref="GrpcService"/>.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task<ServiceReflectionInfo> RefreshServiceReflectionAsync(int serviceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Probes the Server Reflection endpoints of all active services concurrently
    /// and replaces the in-memory cache with the latest results.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task RefreshAllReflectionsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Default <see cref="IReflectionService"/> implementation. Probes each gRPC
/// service's Server Reflection endpoint over HTTP, derives method descriptors
/// from the registered service metadata, and caches the results in memory for
/// the duration of the service instance's lifetime.
/// </summary>
public class ReflectionService : IReflectionService
{
    private readonly IGatewayService _gatewayService;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ReflectionService> _logger;
    private readonly IOptions<GatewayOptions> _options;

    // Per-scope cache — populated on demand via GetServiceReflectionAsync / Refresh*.
    private readonly Dictionary<int, ServiceReflectionInfo> _cache = new();

    public ReflectionService(
        IGatewayService gatewayService,
        HttpClient httpClient,
        ILogger<ReflectionService> logger,
        IOptions<GatewayOptions> options)
    {
        _gatewayService = gatewayService ?? throw new ArgumentNullException(nameof(gatewayService));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc/>
    public bool IsReflectionAvailable => _cache.Values.Any(r => r.IsAvailable);

    /// <inheritdoc/>
    public Task<ServiceReflectionInfo?> GetServiceReflectionAsync(
        int serviceId,
        CancellationToken cancellationToken = default)
    {
        _cache.TryGetValue(serviceId, out var cached);
        return Task.FromResult(cached);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<ServiceReflectionInfo>> GetAllReflectionInfoAsync(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ServiceReflectionInfo> result = _cache.Values.ToList().AsReadOnly();
        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public async Task<ServiceReflectionInfo> RefreshServiceReflectionAsync(
        int serviceId,
        CancellationToken cancellationToken = default)
    {
        var service = await _gatewayService.GetServiceAsync(serviceId);
        var info = await ProbeReflectionEndpointAsync(service, cancellationToken);
        _cache[serviceId] = info;
        return info;
    }

    /// <inheritdoc/>
    public async Task RefreshAllReflectionsAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Value.EnableReflection)
        {
            _logger.LogDebug("Reflection disabled via configuration; skipping refresh");
            return;
        }

        var services = await _gatewayService.GetAllServicesAsync();
        var active = services.Where(s => s.IsActive).ToList();

        _logger.LogInformation("Refreshing gRPC reflection for {Count} active service(s)", active.Count);

        // Probe all services concurrently, then update the cache sequentially to avoid races.
        var probeResults = await Task.WhenAll(
            active.Select(s => ProbeReflectionEndpointAsync(s, cancellationToken)));

        foreach (var result in probeResults)
            _cache[result.ServiceId] = result;

        var available = probeResults.Count(r => r.IsAvailable);
        _logger.LogInformation(
            "Reflection refresh complete: {Available}/{Total} service(s) responded",
            available, active.Count);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private async Task<ServiceReflectionInfo> ProbeReflectionEndpointAsync(
        GrpcService service,
        CancellationToken cancellationToken)
    {
        var info = new ServiceReflectionInfo
        {
            ServiceId       = service.Id,
            ServiceName     = service.Name,
            ServiceFullName = service.ServiceFullName,
            ReflectedAt     = DateTime.UtcNow
        };

        // The gRPC Server Reflection v1alpha service is mounted at the path below.
        // A HEAD probe is sufficient to determine whether the back-end exposes it.
        var probeUrl = $"{service.GetEndpointUri()}/grpc.reflection.v1alpha.ServerReflection";

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromMilliseconds(_options.Value.HealthCheck.TimeoutMs));

            using var request = new HttpRequestMessage(HttpMethod.Head, probeUrl);
            request.Headers.TryAddWithoutValidation("Content-Type", "application/grpc");

            var response = await _httpClient.SendAsync(
                request, HttpCompletionOption.ResponseHeadersRead, cts.Token);

            // gRPC servers return 200, 405, or 415 for a HEAD probe — each indicates presence.
            info.IsAvailable = response.IsSuccessStatusCode
                || response.StatusCode == HttpStatusCode.MethodNotAllowed
                || response.StatusCode == HttpStatusCode.UnsupportedMediaType;

            if (info.IsAvailable)
                info.Methods = BuildMethodDescriptors(service);

            _logger.LogDebug(
                "Reflection probe {ServiceName} → {Url} returned {StatusCode}",
                service.Name, probeUrl, (int)response.StatusCode);
        }
        catch (OperationCanceledException)
        {
            info.IsAvailable = false;
            info.ErrorMessage = "Reflection probe timed out";
            _logger.LogWarning("Reflection probe timed out for service {ServiceName}", service.Name);
        }
        catch (Exception ex)
        {
            info.IsAvailable = false;
            info.ErrorMessage = ex.Message;
            _logger.LogWarning(ex, "Reflection probe failed for service {ServiceName}", service.Name);
        }

        return info;
    }

    /// <summary>
    /// Derives a representative set of CRUD + Watch method descriptors from the
    /// service's registered metadata. A full implementation would parse the
    /// <c>FileDescriptorProto</c> returned by the Server Reflection wire protocol.
    /// </summary>
    private static List<ServiceMethodDescriptor> BuildMethodDescriptors(GrpcService service)
    {
        var baseName = service.ServiceFullName.Split('.').Last();

        return
        [
            new() { Name = "Get",    RequestType = $"{baseName}Request",        ResponseType = baseName },
            new() { Name = "List",   RequestType = $"List{baseName}Request",    ResponseType = $"List{baseName}Response",  IsServerStreaming = true },
            new() { Name = "Create", RequestType = $"Create{baseName}Request",  ResponseType = baseName },
            new() { Name = "Update", RequestType = $"Update{baseName}Request",  ResponseType = baseName },
            new() { Name = "Delete", RequestType = $"Delete{baseName}Request",  ResponseType = "google.protobuf.Empty" },
            new() { Name = "Watch",  RequestType = $"Watch{baseName}Request",   ResponseType = $"{baseName}Event",         IsServerStreaming = true },
        ];
    }
}
