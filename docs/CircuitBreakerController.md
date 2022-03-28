# CircuitBreakerController

Provides endpoints to monitor and manage the state of circuit breakers within the gRPC gateway service. This controller exposes actions to retrieve the status of all circuit breakers, check the status of a specific circuit breaker, and reset a circuit breaker's state.

## API

### `public CircuitBreakerController`

Initializes a new instance of the `CircuitBreakerController` with dependencies required for circuit breaker management. The controller is designed to be injected via dependency injection and does not maintain state itself.

### `public ActionResult GetAll()`

Returns the status of all circuit breakers registered in the system.

- **Return value**: `ActionResult` containing a collection of circuit breaker statuses. Each status includes the circuit breaker's name, current state (e.g., Closed, Open, Half-Open), failure count, and last failure timestamp. Returns HTTP 200 OK on success.
- **Throws**: May throw if the underlying circuit breaker registry is unavailable or if an error occurs during status aggregation.

### `public ActionResult GetStatus(string name)`

Returns the status of a specific circuit breaker identified by name.

- **Parameters**:
  - `name` (string): The name of the circuit breaker to query.
- **Return value**: `ActionResult` containing the status of the specified circuit breaker. The status includes the circuit breaker's state, failure count, and last failure timestamp. Returns HTTP 200 OK on success. Returns HTTP 404 Not Found if the circuit breaker does not exist.
- **Throws**: May throw if the circuit breaker name is null or empty, or if the underlying registry fails during lookup.

### `public ActionResult Reset(string name)`

Resets the state of a specific circuit breaker identified by name, clearing its failure count and transitioning it to the Closed state.

- **Parameters**:
  - `name` (string): The name of the circuit breaker to reset.
- **Return value**: `ActionResult` returning HTTP 200 OK on successful reset. Returns HTTP 404 Not Found if the circuit breaker does not exist.
- **Throws**: May throw if the circuit breaker name is null or empty, or if the underlying registry fails during reset.

## Usage

### Example 1: Monitoring All Circuit Breakers
