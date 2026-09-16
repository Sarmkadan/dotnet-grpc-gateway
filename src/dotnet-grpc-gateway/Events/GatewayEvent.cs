#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Events;

/// <summary>
/// Base class for all gateway events in the publish-subscribe system.
/// Events are immutable and timestamped for audit trails and event ordering.
/// </summary>
public abstract class GatewayEvent
{
    /// <summary>
    /// Gets the unique identifier of the event.
    /// </summary>
    public string EventId { get; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets the UTC timestamp when the event occurred.
    /// </summary>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the correlation identifier used to trace related events.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the event that caused this event, if any.
    /// </summary>
    public string? CausedBy { get; set; }

    protected GatewayEvent()
    {
    }

    protected GatewayEvent(string? correlationId, string? causedBy = null)
    {
        CorrelationId = correlationId;
        CausedBy = causedBy;
    }

    public override string ToString() =>
        $"GatewayEvent {{ EventId = {EventId}, OccurredAt = {OccurredAt}, CorrelationId = {CorrelationId}, CausedBy = {CausedBy} }}";
}

/// <summary>
/// Raised when a gRPC service is registered.
/// </summary>
public class ServiceRegisteredEvent : GatewayEvent
{
    /// <summary>
    /// Gets or sets the unique identifier of the service.
    /// </summary>
    public int ServiceId { get; set; }

    /// <summary>
    /// Gets or sets the name of the service.
    /// </summary>
    public string ServiceName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the full name of the service (including namespace).
    /// </summary>
    public string ServiceFullName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the host where the service is running.
    /// </summary>
    public string Host { get; set; } = null!;

    /// <summary>
    /// Gets or sets the port on which the service is listening.
    /// </summary>
    public int Port { get; set; }

    public ServiceRegisteredEvent() { }

    public ServiceRegisteredEvent(int serviceId, string serviceName, string serviceFullName, string host, int port)
        : this()
    {
        ServiceId = serviceId;
        ServiceName = serviceName;
        ServiceFullName = serviceFullName;
        Host = host;
        Port = port;
    }
}

/// <summary>
/// Raised when a gRPC service is unregistered.
/// </summary>
public class ServiceUnregisteredEvent : GatewayEvent
{
    /// <summary>
    /// Gets or sets the unique identifier of the service.
    /// </summary>
    public int ServiceId { get; set; }

    /// <summary>
    /// Gets or sets the name of the service.
    /// </summary>
    public string ServiceName { get; set; } = null!;

    public ServiceUnregisteredEvent() { }

    public ServiceUnregisteredEvent(int serviceId, string serviceName) : this()
    {
        ServiceId = serviceId;
        ServiceName = serviceName;
    }
}

/// <summary>
/// Raised when a route is added to the gateway.
/// </summary>
public class RouteAddedEvent : GatewayEvent
{
    /// <summary>
    /// Gets or sets the unique identifier of the route.
    /// </summary>
    public int RouteId { get; set; }

    /// <summary>
    /// Gets or sets the route pattern.
    /// </summary>
    public string Pattern { get; set; } = null!;

    /// <summary>
    /// Gets or sets the identifier of the service the route targets.
    /// </summary>
    public int TargetServiceId { get; set; }

    public RouteAddedEvent() { }

    public RouteAddedEvent(int routeId, string pattern, int targetServiceId) : this()
    {
        RouteId = routeId;
        Pattern = pattern;
        TargetServiceId = targetServiceId;
    }
}

/// <summary>
/// Raised when a route is removed from the gateway.
/// </summary>
public class RouteRemovedEvent : GatewayEvent
{
    /// <summary>
    /// Gets or sets the unique identifier of the route.
    /// </summary>
    public int RouteId { get; set; }

    /// <summary>
    /// Gets or sets the route pattern.
    /// </summary>
    public string Pattern { get; set; } = null!;

    public RouteRemovedEvent() { }

    public RouteRemovedEvent(int routeId, string pattern) : this()
    {
        RouteId = routeId;
        Pattern = pattern;
    }
}

/// <summary>
/// Raised when a service health check fails.
/// </summary>
public class ServiceHealthCheckFailedEvent : GatewayEvent
{
    /// <summary>
    /// Gets or sets the unique identifier of the service.
    /// </summary>
    public int ServiceId { get; set; }

    /// <summary>
    /// Gets or sets the name of the service.
    /// </summary>
    public string ServiceName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the error message associated with the health check failure.
    /// </summary>
    public string? ErrorMessage { get; set; }

    public ServiceHealthCheckFailedEvent() { }

    public ServiceHealthCheckFailedEvent(int serviceId, string serviceName, string? errorMessage = null)
        : this()
    {
        ServiceId = serviceId;
        ServiceName = serviceName;
        ErrorMessage = errorMessage;
    }
}

/// <summary>
/// Raised when gateway configuration is updated.
/// </summary>
public class ConfigurationUpdatedEvent : GatewayEvent
{
    /// <summary>
    /// Gets or sets the dictionary of configuration changes.
    /// </summary>
    public Dictionary<string, object?> Changes { get; set; } = new();

    public ConfigurationUpdatedEvent() { }

    public ConfigurationUpdatedEvent(Dictionary<string, object?> changes) : this()
    {
        Changes = changes;
    }
}

/// <summary>
/// Raised when a request is throttled due to rate limiting.
/// </summary>
public class RequestThrottledEvent : GatewayEvent
{
    /// <summary>
    /// Gets or sets the IP address of the client that was throttled.
    /// </summary>
    public string ClientIp { get; set; } = null!;

    /// <summary>
    /// Gets or sets the path of the request that was throttled.
    /// </summary>
    public string RequestPath { get; set; } = null!;

    /// <summary>
    /// Gets or sets the rate limit per window that was exceeded.
    /// </summary>
    public int RateLimitPerWindow { get; set; }

    public RequestThrottledEvent() { }

    public RequestThrottledEvent(string clientIp, string requestPath, int rateLimitPerWindow) : this()
    {
        ClientIp = clientIp;
        RequestPath = requestPath;
        RateLimitPerWindow = rateLimitPerWindow;
    }
}
