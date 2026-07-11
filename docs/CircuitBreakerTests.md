# CircuitBreakerTests

The `CircuitBreakerTests` class contains unit tests that verify the state machine behavior of the circuit breaker implementation used in the `dotnet-grpc-gateway` project. These tests ensure that the circuit breaker correctly transitions between the Closed, Open, and Half-Open states based on recorded failures and successes, and that it properly resets when requested. Each test method exercises a specific scenario defined by its name, asserting that the circuit breaker’s internal counters and state match the expected outcome.

## API

### `InitialState_IsClosed`
- **Purpose**: Verifies that a newly created circuit breaker starts in the Closed state and allows requests to pass through.
- **Parameters**: None.
- **Return value**: `void`.
- **Throws**: `AssertionException` if the initial state is not Closed.

### `RecordFailure_BelowThreshold_RemainsClosedAndAllowsRequests`
- **Purpose**: Confirms that recording a number of failures below the configured threshold does not cause the circuit to open. The circuit breaker remains Closed and continues to allow requests.
- **Parameters**: None.
- **Return value**: `void`.
- **Throws**: `AssertionException` if the circuit opens prematurely or denies requests while still below the threshold.

### `RecordFailure_ReachesThreshold_OpensCircuit`
- **Purpose**: Validates that when the number of recorded failures reaches the configured threshold, the circuit breaker transitions to the Open state and begins rejecting requests.
- **Parameters**: None.
- **Return value**: `void`.
- **Throws**: `AssertionException` if the circuit does not open after the threshold is reached.

### `RecordSuccess_WhenHalfOpen_ClosesCircuitAfterThreshold`
- **Purpose**: Checks that when the circuit breaker is in the Half-Open state and a sufficient number of successive successes are recorded, it transitions back to the Closed state and resumes allowing requests.
- **Parameters**: None.
- **Return value**: `void`.
- **Throws**: `AssertionException` if the circuit does not close after the required number of successes.

### `RecordFailure_WhenHalfOpen_ReOpensCircuit`
- **Purpose**: Ensures that if a failure occurs while the circuit breaker is in the Half-Open state, it immediately transitions back to the Open state and continues rejecting requests.
- **Parameters**: None.
- **Return value**: `void`.
- **Throws**: `AssertionException` if the circuit does not re-open after a failure in Half-Open.

### `Reset_OpenCircuit_ClosesAndClearsCounters`
- **Purpose**: Verifies that calling `Reset` on a circuit breaker that is in the Open state transitions it to the Closed state and resets all internal failure and success counters to their initial values.
- **Parameters**: None.
- **Return value**: `void`.
- **Throws**: `AssertionException` if the state is not Closed after reset or if counters are not cleared.

## Usage

The following examples demonstrate how the circuit breaker is used in a production gRPC client. The tests in `CircuitBreakerTests` validate that the same behavior is correctly implemented.

### Example 1: Basic circuit breaker usage

```csharp
var breaker = new CircuitBreaker(failureThreshold: 3, successThreshold: 2);

// Initially closed – request is allowed
bool allowed = breaker.IsOpen; // false

// Record failures
breaker.RecordFailure();
breaker.RecordFailure();
breaker.RecordFailure(); // threshold reached, circuit opens

allowed = breaker.IsOpen; // true – requests are now rejected

// After a timeout, circuit becomes half-open (simulated here)
breaker.RecordSuccess();
breaker.RecordSuccess(); // success threshold reached, circuit closes

allowed = breaker.IsOpen; // false – requests allowed again
```

### Example 2: Integrating with a gRPC interceptor

```csharp
public class CircuitBreakerInterceptor : Interceptor
{
    private readonly CircuitBreaker _breaker;

    public CircuitBreakerInterceptor(CircuitBreaker breaker)
    {
        _breaker = breaker;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        if (_breaker.IsOpen)
        {
            throw new RpcException(new Status(StatusCode.Unavailable, "Circuit breaker is open"));
        }

        try
        {
            var response = await continuation(request, context);
            _breaker.RecordSuccess();
            return response;
        }
        catch
        {
            _breaker.RecordFailure();
            throw;
        }
    }
}
```

## Notes

- **Edge cases**: The tests assume that the circuit breaker’s threshold values are positive integers. A threshold of zero or negative values is not covered by these tests and may lead to undefined behavior. The `Reset` method is expected to work correctly even if the circuit is already Closed; the test only covers the Open-to-Closed transition.
- **Thread safety**: The test methods themselves are not thread-safe and are intended to run sequentially. The production `CircuitBreaker` implementation should use appropriate synchronization (e.g., locks or `SemaphoreSlim`) to protect internal state when accessed from multiple threads. The tests do not verify concurrent access patterns.
- **State transitions**: The Half-Open state is typically entered after a configurable timeout following an Open state. The tests do not explicitly verify the timeout mechanism; they assume the circuit breaker is placed into Half-Open manually (or via a test helper) before testing Half-Open behavior.
