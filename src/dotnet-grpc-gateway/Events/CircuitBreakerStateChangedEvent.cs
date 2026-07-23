#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Infrastructure;

namespace DotNetGrpcGateway.Events;

/// <summary>
/// Raised when a circuit breaker changes state.
/// Used for monitoring, alerting, and performance tracking.
/// </summary>
public class CircuitBreakerStateChangedEvent : GatewayEvent
{
    /// <summary>Service identifier this circuit breaker is scoped to.</summary>
    public int ServiceId { get; set; }

    /// <summary>Name of the service.</summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>Previous state of the circuit breaker.</summary>
    public CircuitBreakerState PreviousState { get; set; }

    /// <summary>New state of the circuit breaker.</summary>
    public CircuitBreakerState NewState { get; set; }

    /// <summary>Number of consecutive failures at the time of state change.</summary>
    public int ConsecutiveFailures { get; set; }

    /// <summary>Timestamp when the circuit was opened (if applicable).</summary>
    public DateTime? OpenedAt { get; set; }

    public CircuitBreakerStateChangedEvent()
    {
    }

    public CircuitBreakerStateChangedEvent(
        int serviceId,
        string serviceName,
        CircuitBreakerState previousState,
        CircuitBreakerState newState,
        int consecutiveFailures,
        DateTime? openedAt = null,
        string? correlationId = null,
        string? causedBy = null)
        : base(correlationId, causedBy)
    {
        ServiceId = serviceId;
        ServiceName = serviceName;
        PreviousState = previousState;
        NewState = newState;
        ConsecutiveFailures = consecutiveFailures;
        OpenedAt = openedAt;
    }
}
