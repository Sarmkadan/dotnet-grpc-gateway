// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Events.EventHandlers;

/// <summary>
/// Event handler for route removal events.
/// Logs route removals and triggers related cleanup operations.
/// </summary>
public class RouteRemovedEventHandler : IEventHandler<RouteRemovedEvent>
{
    private readonly ILogger<RouteRemovedEventHandler> _logger;

    public RouteRemovedEventHandler(ILogger<RouteRemovedEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(RouteRemovedEvent @event)
    {
        _logger.LogInformation(
            "Route removed - RouteId: {RouteId}, Pattern: {Pattern}, EventId: {@EventId}",
            @event.RouteId, @event.Pattern, @event.EventId);

        // In a real scenario, this would:
        // 1. Invalidate route matching cache
        // 2. Close any active connections using this route
        // 3. Update load balancer configuration

        await Task.CompletedTask;
    }
}
