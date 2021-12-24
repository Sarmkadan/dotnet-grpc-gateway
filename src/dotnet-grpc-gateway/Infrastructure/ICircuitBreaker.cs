#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Infrastructure;

/// <summary>Circuit breaker state machine states.</summary>
public enum CircuitBreakerState
{
    /// <summary>Normal operation; requests flow through.</summary>
    Closed,
    /// <summary>Failure threshold exceeded; requests are rejected immediately.</summary>
    Open,
    /// <summary>Testing whether the downstream service has recovered.</summary>
    HalfOpen
}

/// <summary>
/// Per-service circuit breaker that prevents cascading failures by rejecting calls
/// when a service is known to be unhealthy.
/// </summary>
public interface ICircuitBreaker
{
    /// <summary>Gets the service identifier this circuit breaker is scoped to.</summary>
    int ServiceId { get; }

    /// <summary>Gets the current state of the circuit.</summary>
    CircuitBreakerState State { get; }

    /// <summary>Gets the total number of consecutive failures recorded.</summary>
    int ConsecutiveFailures { get; }

    /// <summary>Gets the timestamp when the circuit was last opened.</summary>
    DateTime? OpenedAt { get; }

    /// <summary>
    /// Returns <see langword="true"/> if the circuit allows the call to proceed,
    /// <see langword="false"/> when the circuit is open and the call should be rejected.
    /// </summary>
    bool AllowRequest();

    /// <summary>Records a successful call and may close an open or half-open circuit.</summary>
    void RecordSuccess();

    /// <summary>Records a failed call and may open the circuit.</summary>
    void RecordFailure();

    /// <summary>Manually resets the circuit to the closed state.</summary>
    void Reset();
}

/// <summary>
/// Thread-safe implementation of <see cref="ICircuitBreaker"/> using a
/// closed → open → half-open → closed state machine.
/// </summary>
public class CircuitBreaker : ICircuitBreaker
{
    private readonly CircuitBreakerOptions _options;
    private readonly ILogger<CircuitBreaker> _logger;
    private readonly object _lock = new();

    private CircuitBreakerState _state = CircuitBreakerState.Closed;
    private int _consecutiveFailures;
    private int _halfOpenSuccesses;
    private DateTime? _openedAt;

    public int ServiceId { get; }

    public CircuitBreakerState State
    {
        get
        {
            lock (_lock)
                return _state;
        }
    }

    public int ConsecutiveFailures
    {
        get
        {
            lock (_lock)
                return _consecutiveFailures;
        }
    }

    public DateTime? OpenedAt
    {
        get
        {
            lock (_lock)
                return _openedAt;
        }
    }

    public CircuitBreaker(int serviceId, CircuitBreakerOptions options, ILogger<CircuitBreaker> logger)
    {
        ServiceId = serviceId;
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public bool AllowRequest()
    {
        lock (_lock)
        {
            switch (_state)
            {
                case CircuitBreakerState.Closed:
                    return true;

                case CircuitBreakerState.Open:
                    if (_openedAt.HasValue && DateTime.UtcNow - _openedAt.Value >= _options.OpenDuration)
                    {
                        _state = CircuitBreakerState.HalfOpen;
                        _halfOpenSuccesses = 0;
                        _logger.LogInformation(
                            "Circuit for service {ServiceId} transitioned to HalfOpen",
                            ServiceId);
                        return true;
                    }

                    return false;

                case CircuitBreakerState.HalfOpen:
                    return true;

                default:
                    return true;
            }
        }
    }

    /// <inheritdoc/>
    public void RecordSuccess()
    {
        lock (_lock)
        {
            _consecutiveFailures = 0;

            if (_state == CircuitBreakerState.HalfOpen)
            {
                _halfOpenSuccesses++;
                if (_halfOpenSuccesses >= _options.HalfOpenSuccessThreshold)
                {
                    _state = CircuitBreakerState.Closed;
                    _openedAt = null;
                    _logger.LogInformation(
                        "Circuit for service {ServiceId} closed after recovery",
                        ServiceId);
                }
            }
        }
    }

    /// <inheritdoc/>
    public void RecordFailure()
    {
        lock (_lock)
        {
            _consecutiveFailures++;

            if (_state == CircuitBreakerState.HalfOpen)
            {
                _state = CircuitBreakerState.Open;
                _openedAt = DateTime.UtcNow;
                _logger.LogWarning(
                    "Circuit for service {ServiceId} re-opened after half-open failure",
                    ServiceId);
                return;
            }

            if (_state == CircuitBreakerState.Closed && _consecutiveFailures >= _options.FailureThreshold)
            {
                _state = CircuitBreakerState.Open;
                _openedAt = DateTime.UtcNow;
                _logger.LogWarning(
                    "Circuit for service {ServiceId} opened after {Failures} consecutive failures",
                    ServiceId,
                    _consecutiveFailures);
            }
        }
    }

    /// <inheritdoc/>
    public void Reset()
    {
        lock (_lock)
        {
            _state = CircuitBreakerState.Closed;
            _consecutiveFailures = 0;
            _halfOpenSuccesses = 0;
            _openedAt = null;
            _logger.LogInformation("Circuit for service {ServiceId} manually reset", ServiceId);
        }
    }
}
