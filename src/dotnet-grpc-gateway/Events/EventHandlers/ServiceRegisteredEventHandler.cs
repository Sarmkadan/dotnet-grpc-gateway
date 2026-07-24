#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Events.EventHandlers;

/// <summary>
/// Event handler for service registration events.
/// Logs service registrations and updates service discovery cache.
/// </summary>
public class ServiceRegisteredEventHandler : EventHandlerBase<ServiceRegisteredEvent>, IEventHandler<ServiceRegisteredEvent>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceRegisteredEventHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    public ServiceRegisteredEventHandler(ILogger<ServiceRegisteredEventHandler> logger)
        : base(logger)
    {
    }

    /// <summary>
    /// Handles service registration events by logging the registration.
    /// </summary>
    /// <param name="@event">The service registration event.</param>
    /// <exception cref="ArgumentNullException">Thrown when the event is null.</exception>
    public async Task HandleAsync(ServiceRegisteredEvent @event)
    {
        ValidateEvent(@event);

        SafeLog(
            LogLevel.Information,
            "Service registered - Service: {ServiceName}, Host: {Host}:{Port}, " +
            "FullName: {ServiceFullName}, EventId: {@EventId}",
            @event.ServiceName, @event.Host, @event.Port, @event.ServiceFullName, @event.EventId);

        // In a real scenario, this would update the service discovery cache or registry
        // For now, we just log the event

        await Task.CompletedTask;
    }
}
