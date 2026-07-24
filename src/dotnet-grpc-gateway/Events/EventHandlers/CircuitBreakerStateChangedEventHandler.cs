#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Infrastructure;
using Microsoft.Extensions.Logging;

namespace DotNetGrpcGateway.Events.EventHandlers;

/// <summary>
/// Event handler for circuit breaker state changes.
/// Logs state transitions and can be extended for monitoring/alerting.
/// </summary>
public class CircuitBreakerStateChangedEventHandler : EventHandlerBase<CircuitBreakerStateChangedEvent>, IEventHandler<CircuitBreakerStateChangedEvent>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CircuitBreakerStateChangedEventHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    public CircuitBreakerStateChangedEventHandler(ILogger<CircuitBreakerStateChangedEventHandler> logger)
        : base(logger)
    {
    }

    /// <summary>
    /// Handles circuit breaker state change events by logging the state transition.
    /// </summary>
    /// <param name="@event">The circuit breaker state change event.</param>
    /// <exception cref="ArgumentNullException">Thrown when the event is null.</exception>
    public async Task HandleAsync(CircuitBreakerStateChangedEvent @event)
    {
        ValidateEvent(@event);

        var stateChangeMessage = @event.NewState switch
        {
            CircuitBreakerState.Closed => "Circuit breaker CLOSED - service is healthy",
            CircuitBreakerState.Open => "Circuit breaker OPENED - service is unavailable",
            CircuitBreakerState.HalfOpen => "Circuit breaker HALF-OPEN - testing service recovery",
            _ => @event.NewState.ToString()
        };

        SafeLog(
            LogLevel.Information,
            "Circuit breaker state change - Service: {ServiceName} (ID: {ServiceId}), " +
            "State: {PreviousState} → {NewState}, Failures: {ConsecutiveFailures}, " +
            "{StateChangeMessage}",
            @event.ServiceName, @event.ServiceId, @event.PreviousState, @event.NewState,
            @event.ConsecutiveFailures, stateChangeMessage);

        // Additional logging for open state
        if (@event.NewState == CircuitBreakerState.Open)
        {
            SafeLog(
                LogLevel.Warning,
                "Service {ServiceName} (ID: {ServiceId}) is now protected by circuit breaker. " +
                "Consecutive failures: {ConsecutiveFailures}. Circuit will remain open for: {OpenDuration}",
                @event.ServiceName, @event.ServiceId, @event.ConsecutiveFailures,
                @event.OpenedAt.HasValue
                    ? (DateTime.UtcNow - @event.OpenedAt.Value).TotalSeconds.ToString("F1") + " seconds"
                    : "unknown");
        }

        await Task.CompletedTask;
    }
}
