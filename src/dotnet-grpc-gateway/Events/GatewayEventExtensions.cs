#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace DotNetGrpcGateway.Events;

/// <summary>
/// Extension methods for <see cref="GatewayEvent"/> to enhance event processing and analysis.
/// </summary>
public static class GatewayEventExtensions
{
    /// <summary>
    /// Determines whether the event is related to a service registration or unregistration.
    /// </summary>
    /// <param name="event">The event to check.</param>
    /// <returns><see langword="true"/> if the event is service-related; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="event"/> is <see langword="null"/>.</exception>
    public static bool MaybeServiceRelated(this GatewayEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return @event is ServiceRegisteredEvent or ServiceUnregisteredEvent;
    }

    /// <summary>
    /// Gets the service name if the event is service-related.
    /// </summary>
    /// <param name="event">The event to check.</param>
    /// <returns>The service name if available; otherwise, <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="event"/> is <see langword="null"/>.</exception>
    public static string? GetServiceNameIfAvailable(this GatewayEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return @event switch
        {
            ServiceRegisteredEvent e => e.ServiceName,
            ServiceUnregisteredEvent e => e.ServiceName,
            _ => null
        };
    }

    /// <summary>
    /// Converts the event to a summary string for logging or auditing.
    /// </summary>
    /// <param name="event">The event to summarize.</param>
    /// <returns>A summary string containing event type, ID, timestamp, and correlation ID.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="event"/> is <see langword="null"/>.</exception>
    public static string ToEventSummary(this GatewayEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return $"{@event.GetType().Name} (ID: {@event.EventId}, Time: {@event.OccurredAt:O}, Correlation: {@event.CorrelationId ?? "none"})";
    }
}
