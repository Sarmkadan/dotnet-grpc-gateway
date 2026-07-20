#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using DotNetGrpcGateway.Domain;

namespace DotNetGrpcGateway.Services;

/// <summary>Supported load balancing algorithms.</summary>
public enum LoadBalancingStrategy
{
    /// <summary>Distributes requests evenly across all healthy endpoints in order.</summary>
    RoundRobin,
    /// <summary>Selects an endpoint at random.</summary>
    Random,
    /// <summary>Routes to the endpoint with the fewest active connections.</summary>
    LeastConnections
}

/// <summary>
/// Manages endpoint pools per service and selects the next endpoint to use
/// based on a configurable load balancing strategy.
/// </summary>
public interface ILoadBalancerService
{
    /// <summary>Registers an endpoint for a service.</summary>
    void RegisterEndpoint(ServiceEndpoint endpoint);

    /// <summary>Removes an endpoint from the pool of a service.</summary>
    void DeregisterEndpoint(int serviceId, int endpointId);

    /// <summary>Picks the next endpoint for the given service using the active strategy.</summary>
    ServiceEndpoint? GetNextEndpoint(int serviceId);

    /// <summary>Returns all registered endpoints for a service.</summary>
    IReadOnlyList<ServiceEndpoint> GetEndpoints(int serviceId);

    /// <summary>Updates endpoint health state.</summary>
    void UpdateEndpointHealth(int serviceId, int endpointId, bool isHealthy);

    /// <summary>Sets the draining state of an endpoint.</summary>
    void SetDraining(int serviceId, int endpointId, bool draining);

    /// <summary>Records that a request to an endpoint completed.</summary>
    void RecordRequestCompleted(int serviceId, int endpointId, double responseTimeMs, bool success);

    /// <summary>Gets or sets the load balancing strategy (default: RoundRobin).</summary>
    LoadBalancingStrategy Strategy { get; set; }
}

/// <summary>
/// In-process implementation of <see cref="ILoadBalancerService"/>.
/// Thread-safe via <see cref="ConcurrentDictionary{TKey, TValue}"/> and atomic operations.
/// </summary>
public class LoadBalancerService : ILoadBalancerService
{
    private readonly ConcurrentDictionary<int, List<ServiceEndpoint>> _endpoints = new();
    private readonly ConcurrentDictionary<int, int> _roundRobinCounters = new();
    private readonly ILogger<LoadBalancerService> _logger;

    public LoadBalancingStrategy Strategy { get; set; } = LoadBalancingStrategy.RoundRobin;

    public LoadBalancerService(ILogger<LoadBalancerService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public void RegisterEndpoint(ServiceEndpoint endpoint)
    {
        if (endpoint is null)
            throw new ArgumentNullException(nameof(endpoint));

        _endpoints.AddOrUpdate(
            endpoint.ServiceId,
            _ => new List<ServiceEndpoint> { endpoint },
            (_, list) =>
            {
                lock (list)
                {
                    if (list.All(e => e.Id != endpoint.Id))
                        list.Add(endpoint);
                }

                return list;
            });

        _logger.LogInformation(
            "Endpoint {Host}:{Port} registered for service {ServiceId}",
            endpoint.Host,
            endpoint.Port,
            endpoint.ServiceId);
    }

    /// <inheritdoc/>
    public void DeregisterEndpoint(int serviceId, int endpointId)
    {
        if (!_endpoints.TryGetValue(serviceId, out var list))
            return;

        lock (list)
        {
            var removed = list.RemoveAll(e => e.Id == endpointId);
            if (removed > 0)
            {
                _logger.LogInformation(
                    "Endpoint {EndpointId} removed from service {ServiceId}",
                    endpointId,
                    serviceId);
            }
        }
    }

    /// <inheritdoc/>
    public ServiceEndpoint? GetNextEndpoint(int serviceId)
    {
        if (!_endpoints.TryGetValue(serviceId, out var all))
            return null;

        List<ServiceEndpoint> healthy;
        lock (all)
        {
            healthy = all.Where(e => e.IsHealthy).ToList();
        }

        if (healthy.Count == 0)
        {
            _logger.LogWarning("No healthy endpoints available for service {ServiceId}", serviceId);
            return null;
        }

        // Filter out draining endpoints unless ALL are draining
        var nonDraining = healthy.Where(e => !e.Draining).ToList();
        var endpointsToConsider = nonDraining.Count > 0 ? nonDraining : healthy;

        return Strategy switch
        {
            LoadBalancingStrategy.Random => endpointsToConsider[Random.Shared.Next(endpointsToConsider.Count)],
            LoadBalancingStrategy.LeastConnections => endpointsToConsider.MinBy(e => e.ActiveConnections),
            _ => RoundRobinSelect(serviceId, endpointsToConsider)
        };
    }

    /// <inheritdoc/>
    public IReadOnlyList<ServiceEndpoint> GetEndpoints(int serviceId)
    {
        if (!_endpoints.TryGetValue(serviceId, out var list))
            return Array.Empty<ServiceEndpoint>();

        lock (list)
        {
            return list.ToList().AsReadOnly();
        }
    }

    /// <inheritdoc/>
    public void UpdateEndpointHealth(int serviceId, int endpointId, bool isHealthy)
    {
        if (!_endpoints.TryGetValue(serviceId, out var list))
            return;

        lock (list)
        {
            var endpoint = list.FirstOrDefault(e => e.Id == endpointId);
            if (endpoint is not null)
                endpoint.IsHealthy = isHealthy;
        }

        _logger.LogInformation(
            "Endpoint {EndpointId} for service {ServiceId} health updated to {IsHealthy}",
            endpointId,
            serviceId,
            isHealthy);
    }

    /// <inheritdoc/>
    public void SetDraining(int serviceId, int endpointId, bool draining)
    {
        if (!_endpoints.TryGetValue(serviceId, out var list))
            return;

        lock (list)
        {
            var endpoint = list.FirstOrDefault(e => e.Id == endpointId);
            if (endpoint is not null)
            {
                endpoint.Draining = draining;
                _logger.LogInformation(
                    "Endpoint {EndpointId} for service {ServiceId} draining set to {Draining}",
                    endpointId,
                    serviceId,
                    draining);
            }
        }
    }

    /// <inheritdoc/>
    public void RecordRequestCompleted(int serviceId, int endpointId, double responseTimeMs, bool success)
    {
        if (!_endpoints.TryGetValue(serviceId, out var list))
            return;

        lock (list)
        {
            var endpoint = list.FirstOrDefault(e => e.Id == endpointId);
            if (endpoint is null)
                return;

            Interlocked.Decrement(ref endpoint.ActiveConnections);
            if (endpoint.ActiveConnections < 0)
                endpoint.ActiveConnections = 0;

            endpoint.RecordRequest(responseTimeMs, success);
        }
    }

    private ServiceEndpoint RoundRobinSelect(int serviceId, List<ServiceEndpoint> endpoints)
    {
        var counter = _roundRobinCounters.AddOrUpdate(serviceId, 0, (_, current) => (current + 1) % endpoints.Count);
        return endpoints[counter % endpoints.Count];
    }
}
