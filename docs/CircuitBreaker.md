# Circuit Breaker

The circuit breaker mechanism prevents cascading failures by temporarily blocking requests to unhealthy services. It implements the circuit breaker pattern with three states: Closed, Open, and Half-Open, allowing automatic recovery when the downstream service becomes healthy again.

## States

### Closed
Normal operation state where requests flow through to the service. Failure counts are tracked, and if the failure threshold is exceeded, the circuit transitions to Open.

### Open
Failure threshold has been exceeded; requests are rejected immediately without calling the service. After the open duration elapses, the circuit transitions to Half-Open to test if the service has recovered.

### Half-Open
Testing state where a limited number of requests are allowed through to verify service recovery. If successful calls reach the half-open success threshold, the circuit closes; if any call fails, it reopens.

## Configuration

### CircuitBreakerOptions
Configuration for an individual circuit breaker instance.

#### FailureThreshold
`public int FailureThreshold { get; set; } = 3;`
Number of consecutive failures required to open the circuit.

#### OpenDuration
`public TimeSpan OpenDuration { get; set; } = TimeSpan.FromSeconds(30);`
Duration the circuit stays open before entering the half-open state.

#### HalfOpenSuccessThreshold
`public int HalfOpenSuccessThreshold { get; set; } = 2;`
Number of successful calls in half-open state required to close the circuit.

## Interface: ICircuitBreaker

### Properties

#### ServiceId
`public int ServiceId { get; }`
Gets the service identifier this circuit breaker is scoped to.

#### State
`public CircuitBreakerState State { get; }`
Gets the current state of the circuit (Closed, Open, or HalfOpen).

#### ConsecutiveFailures
`public int ConsecutiveFailures { get; }`
Gets the total number of consecutive failures recorded.

#### OpenedAt
`public DateTime? OpenedAt { get; }`
Gets the timestamp when the circuit was last opened.

### Methods

#### AllowRequest()
`public bool AllowRequest()`
Returns `true` if the circuit allows the call to proceed, `false` when the circuit is open and the call should be rejected.

Behavior by state:
- **Closed**: Always returns `true`
- **Open**: Returns `false` unless the open duration has elapsed, then transitions to HalfOpen and returns `true`
- **HalfOpen**: Always returns `true` (allows test requests)

#### RecordSuccess()
`public void RecordSuccess()`
Records a successful call and may close an open or half-open circuit:
- Resets consecutive failure count to zero
- In HalfOpen state: increments success counter; if half-open success threshold is reached, transitions to Closed

#### RecordFailure()
`public void RecordFailure()`
Records a failed call and may open the circuit:
- Increments consecutive failure count
- In HalfOpen state: immediately transitions to Open
- In Closed state: if failure threshold is exceeded, transitions to Open

#### Reset()
`public void Reset()`
Manually resets the circuit to the closed state:
- Sets state to Closed
- Resets consecutive failures and half-open successes to zero
- Clears opened timestamp

## Usage

### Basic Pattern
```csharp
if (circuitBreaker.AllowRequest())
{
    try
    {
        var response = await serviceClient.CallAsync();
        circuitBreaker.RecordSuccess();
        return response;
    }
    catch (Exception)
    {
        circuitBreaker.RecordFailure();
        throw;
    }
}
else
{
    throw new CircuitBreakerOpenException("Service is temporarily unavailable.");
}
```

### Manual Reset
```csharp
// Administratively force a reset of the circuit breaker
// typically used when manual intervention confirms service recovery.
circuitBreaker.Reset();
```

### Custom Configuration
```csharp
var options = new CircuitBreakerOptions
{
    FailureThreshold = 5,
    OpenDuration = TimeSpan.FromMinutes(1),
    HalfOpenSuccessThreshold = 3
};

var circuitBreaker = new CircuitBreaker(
    serviceId: 123,
    options: options,
    logger: logger);
```

## Thread Safety
The CircuitBreaker implementation is thread-safe, using locking mechanisms to protect state updates and reads. All methods (`AllowRequest`, `RecordSuccess`, `RecordFailure`, `Reset`) are safe for concurrent access.

## Event Publishing
When an `IEventPublisher` is provided, the circuit breaker publishes `CircuitBreakerStateChangedEvent` events on state transitions, enabling monitoring and alerting on circuit breaker activity.

## Dependencies
- `Microsoft.Extensions.Logging.ILogger<CircuitBreaker>` for logging
- `DotNetGrpcGateway.Events.IEventPublisher` (optional) for state change events