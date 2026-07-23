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
public class CircuitBreakerStateChangedEventHandler : IEventHandler<CircuitBreakerStateChangedEvent>
{
    private readonly ILogger<CircuitBreakerStateChangedEventHandler> _logger;

    public CircuitBreakerStateChangedEventHandler(ILogger<CircuitBreakerStateChangedEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(CircuitBreakerStateChangedEvent @event)
    {
        if (@event is null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        var stateChangeMessage = @event.NewState switch
        {
            CircuitBreakerState.Closed => "Circuit breaker CLOSED - service is healthy",
            CircuitBreakerState.Open => "Circuit breaker OPENED - service is unavailable",
            CircuitBreakerState.HalfOpen => "Circuit breaker HALF-OPEN - testing service recovery",
            _ => @event.NewState.ToString()
        };

        _logger.LogInformation(
            "Circuit breaker state change - Service: {ServiceName} (ID: {ServiceId}), " +
            "State: {PreviousState} → {NewState}, Failures: {ConsecutiveFailures}, " +
            "{StateChangeMessage}",
            @event.ServiceName,
            @event.ServiceId,
            @event.PreviousState,
            @event.NewState,
            @event.ConsecutiveFailures,
            stateChangeMessage);

        // Additional logging for open state
        if (@event.NewState == CircuitBreakerState.Open)
        {
            _logger.LogWarning(
                "Service {ServiceName} (ID: {ServiceId}) is now protected by circuit breaker. " +
                "Consecutive failures: {ConsecutiveFailures}. Circuit will remain open for: {OpenDuration}",
                @event.ServiceName,
                @event.ServiceId,
                @event.ConsecutiveFailures,
                @event.OpenedAt.HasValue
                    ? (DateTime.UtcNow - @event.OpenedAt.Value).TotalSeconds.ToString("F1") + " seconds"
                    : "unknown");
        }

        await Task.CompletedTask;
    }
}
