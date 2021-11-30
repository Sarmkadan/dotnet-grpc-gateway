// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Events.EventHandlers;

/// <summary>
/// Event handler for route addition events.
/// Logs route additions and optionally triggers cache invalidation.
/// </summary>
public class RouteAddedEventHandler : IEventHandler<RouteAddedEvent>
{
    private readonly ILogger<RouteAddedEventHandler> _logger;

    public RouteAddedEventHandler(ILogger<RouteAddedEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(RouteAddedEvent @event)
    {
        _logger.LogInformation(
            "Route added - RouteId: {RouteId}, Pattern: {Pattern}, TargetServiceId: {TargetServiceId}, EventId: {@EventId}",
            @event.RouteId, @event.Pattern, @event.TargetServiceId, @event.EventId);

        // In a real scenario, this would:
        // 1. Invalidate route matching cache
        // 2. Update load balancer configuration
        // 3. Send notifications to monitoring systems

        await Task.CompletedTask;
    }
}
