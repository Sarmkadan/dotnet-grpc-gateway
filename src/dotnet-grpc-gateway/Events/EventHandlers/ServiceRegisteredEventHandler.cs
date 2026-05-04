// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Events.EventHandlers;

/// <summary>
/// Event handler for service registration events.
/// Logs service registrations and updates service discovery cache.
/// </summary>
public class ServiceRegisteredEventHandler : IEventHandler<ServiceRegisteredEvent>
{
    private readonly ILogger<ServiceRegisteredEventHandler> _logger;

    public ServiceRegisteredEventHandler(ILogger<ServiceRegisteredEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(ServiceRegisteredEvent @event)
    {
        _logger.LogInformation(
            "Service registered - Service: {ServiceName}, Host: {Host}:{Port}, " +
            "FullName: {ServiceFullName}, EventId: {@EventId}",
            @event.ServiceName, @event.Host, @event.Port, @event.ServiceFullName, @event.EventId);

        // In a real scenario, this would update the service discovery cache or registry
        // For now, we just log the event

        await Task.CompletedTask;
    }
}
