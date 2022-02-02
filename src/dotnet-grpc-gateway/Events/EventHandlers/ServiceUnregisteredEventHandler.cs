// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Events.EventHandlers;

/// <summary>
/// Event handler for service unregistration events.
/// Logs service removals and cleans up associated routes.
/// </summary>
public class ServiceUnregisteredEventHandler : IEventHandler<ServiceUnregisteredEvent>
{
    private readonly ILogger<ServiceUnregisteredEventHandler> _logger;

    public ServiceUnregisteredEventHandler(ILogger<ServiceUnregisteredEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(ServiceUnregisteredEvent @event)
    {
        _logger.LogWarning(
            "Service unregistered - ServiceId: {ServiceId}, ServiceName: {ServiceName}, EventId: {@EventId}",
            @event.ServiceId, @event.ServiceName, @event.EventId);

        // In a real scenario, this would:
        // 1. Remove all routes pointing to this service
        // 2. Clear any cached data related to the service
        // 3. Notify monitoring systems

        await Task.CompletedTask;
    }
}
