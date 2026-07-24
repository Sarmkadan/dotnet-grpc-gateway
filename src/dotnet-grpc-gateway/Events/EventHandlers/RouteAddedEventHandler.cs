#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Events.EventHandlers;

/// <summary>
/// Event handler for route addition events.
/// Logs route additions and optionally triggers cache invalidation.
/// </summary>
public class RouteAddedEventHandler : EventHandlerBase<RouteAddedEvent>, IEventHandler<RouteAddedEvent>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RouteAddedEventHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    public RouteAddedEventHandler(ILogger<RouteAddedEventHandler> logger)
        : base(logger)
    {
    }

    /// <summary>
    /// Handles route addition events by logging the addition.
    /// </summary>
    /// <param name="@event">The route addition event.</param>
    /// <exception cref="ArgumentNullException">Thrown when the event is null.</exception>
    public async Task HandleAsync(RouteAddedEvent @event)
    {
        ValidateEvent(@event);

        SafeLog(
            LogLevel.Information,
            "Route added - RouteId: {RouteId}, Pattern: {Pattern}, TargetServiceId: {TargetServiceId}, EventId: {@EventId}",
            @event.RouteId, @event.Pattern, @event.TargetServiceId, @event.EventId);

        // In a real scenario, this would:
        // 1. Invalidate route matching cache
        // 2. Update load balancer configuration
        // 3. Send notifications to monitoring systems

        await Task.CompletedTask;
    }
}
