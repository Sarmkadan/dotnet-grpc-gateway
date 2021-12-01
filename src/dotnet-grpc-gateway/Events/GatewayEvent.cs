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
    public string EventId { get; } = Guid.NewGuid().ToString();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string? CorrelationId { get; set; }
    public string? CausedBy { get; set; }

    protected GatewayEvent()
    {
    }

    protected GatewayEvent(string? correlationId, string? causedBy = null)
    {
        CorrelationId = correlationId;
        CausedBy = causedBy;
    }
}

/// <summary>
/// Raised when a gRPC service is registered.
/// </summary>
public class ServiceRegisteredEvent : GatewayEvent
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = null!;
    public string ServiceFullName { get; set; } = null!;
    public string Host { get; set; } = null!;
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
    public int ServiceId { get; set; }
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
    public int RouteId { get; set; }
    public string Pattern { get; set; } = null!;
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
    public int RouteId { get; set; }
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
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = null!;
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
    public string ClientIp { get; set; } = null!;
    public string RequestPath { get; set; } = null!;
    public int RateLimitPerWindow { get; set; }

    public RequestThrottledEvent() { }

    public RequestThrottledEvent(string clientIp, string requestPath, int rateLimitPerWindow) : this()
    {
        ClientIp = clientIp;
        RequestPath = requestPath;
        RateLimitPerWindow = rateLimitPerWindow;
    }
}
