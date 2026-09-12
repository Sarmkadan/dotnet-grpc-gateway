#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Events;

/// <summary>
/// Interface for publishing events in the gateway system.
/// Enables decoupled communication between components.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes an event to all registered subscribers.
    /// </summary>
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : GatewayEvent;
}

/// <summary>
/// Interface for subscribing to events.
/// Handlers implement this to receive specific event types.
/// </summary>
public interface IEventHandler<TEvent> where TEvent : GatewayEvent
{
    /// <summary>
    /// Handles the specified event asynchronously.
    /// </summary>
    /// <param name="event">The event to handle.</param>
    Task HandleAsync(TEvent @event);
}
