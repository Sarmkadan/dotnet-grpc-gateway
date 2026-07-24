#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Events.EventHandlers;

/// <summary>
/// Event handler for service unregistration events.
/// Logs service removals and cleans up associated routes.
/// </summary>
public class ServiceUnregisteredEventHandler : EventHandlerBase<ServiceUnregisteredEvent>, IEventHandler<ServiceUnregisteredEvent>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceUnregisteredEventHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    public ServiceUnregisteredEventHandler(ILogger<ServiceUnregisteredEventHandler> logger)
        : base(logger)
    {
    }

    /// <summary>
    /// Handles service unregistration events by logging the removal.
    /// </summary>
    /// <param name="@event">The service unregistration event.</param>
    /// <exception cref="ArgumentNullException">Thrown when the event is null.</exception>
    public async Task HandleAsync(ServiceUnregisteredEvent @event)
    {
        ValidateEvent(@event);

        SafeLog(
            LogLevel.Warning,
            "Service unregistered - ServiceId: {ServiceId}, ServiceName: {ServiceName}, EventId: {@EventId}",
            @event.ServiceId, @event.ServiceName, @event.EventId);

        // In a real scenario, this would:
        // 1. Remove all routes pointing to this service
        // 2. Clear any cached data related to the service
        // 3. Notify monitoring systems

        await Task.CompletedTask;
    }
}
