#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Events.EventHandlers;

/// <summary>
/// Event handler for route removal events.
/// Logs route removals and triggers related cleanup operations.
/// </summary>
public class RouteRemovedEventHandler : EventHandlerBase<RouteRemovedEvent>, IEventHandler<RouteRemovedEvent>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RouteRemovedEventHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    public RouteRemovedEventHandler(ILogger<RouteRemovedEventHandler> logger)
        : base(logger)
    {
    }

    /// <summary>
    /// Handles route removal events by logging the removal.
    /// </summary>
    /// <param name="@event">The route removal event.</param>
    /// <exception cref="ArgumentNullException">Thrown when the event is null.</exception>
    public async Task HandleAsync(RouteRemovedEvent @event)
    {
        ValidateEvent(@event);

        SafeLog(
            LogLevel.Information,
            "Route removed - RouteId: {RouteId}, Pattern: {Pattern}, EventId: {@EventId}",
            @event.RouteId, @event.Pattern, @event.EventId);

        // In a real scenario, this would:
        // 1. Invalidate route matching cache
        // 2. Close any active connections using this route
        // 3. Update load balancer configuration

        await Task.CompletedTask;
    }
}
