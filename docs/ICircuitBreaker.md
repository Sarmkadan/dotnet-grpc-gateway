# ICircuitBreaker

The `ICircuitBreaker` interface defines a contract for implementing the circuit breaker pattern, enabling resilient service-to-service communication. By tracking success and failure rates, it provides a standardized mechanism to prevent overwhelming failing services with requests by temporarily blocking further calls once a defined failure threshold is met.

## API

### ServiceId
`public int ServiceId { get; }`
Gets the unique identifier of the service associated with this circuit breaker instance.

### CircuitBreaker
`public CircuitBreaker()`
Initializes a new instance of the `CircuitBreaker` class.

### AllowRequest
`public bool AllowRequest()`
Checks whether a request is permitted to proceed based on the current state of the circuit.
*   **Returns:** `true` if the request is permitted; `false` if the circuit is open and the request should be rejected.

### RecordSuccess
`public void RecordSuccess()`
Updates the internal state of the circuit breaker to reflect that an operation has completed successfully. This may trigger a state transition, such as moving from a half-open state to a closed state.

### RecordFailure
`public void RecordFailure()`
Updates the internal state of the circuit breaker to reflect that an operation has failed. Repeated calls to this method will drive the circuit toward an open state.

### Reset
`public void Reset()`
Forces the circuit breaker to return to its initial closed state, clearing all accumulated failure statistics.

## Usage

### Example 1: Guarding a Service Call
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

### Example 2: Resetting the Circuit
```csharp
// Administratively force a reset of the circuit breaker
// typically used when manual intervention confirms service recovery.
circuitBreaker.Reset();
```

## Notes

*   **Thread Safety:** Implementations of `ICircuitBreaker` must be thread-safe. As this interface is intended to be used in high-concurrency, asynchronous environments, state updates (`RecordSuccess`, `RecordFailure`, `Reset`) and checks (`AllowRequest`) must handle race conditions appropriately, typically using atomic operations or locking mechanisms.
*   **State Transitions:** The implementation should define the logic for transitioning between "Closed", "Open", and potentially "Half-Open" states. The `AllowRequest` method is responsible for evaluating these states before authorizing a request.
*   **Performance:** `AllowRequest` should be implemented to be highly performant, as it is executed on every request path before the downstream call is attempted.
