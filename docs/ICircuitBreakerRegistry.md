# ICircuitBreakerRegistry
The `ICircuitBreakerRegistry` interface provides a way to manage a collection of circuit breakers, which are used to detect when a service is not responding and prevent further requests from being sent to it until it becomes available again. This helps to prevent cascading failures in a distributed system.

## API
* `CircuitBreakerRegistry`: The default implementation of the `ICircuitBreakerRegistry` interface.
* `ICircuitBreaker GetOrCreate(string name)`: Retrieves a circuit breaker with the given name, creating a new one if it does not exist. The `name` parameter is the unique identifier of the circuit breaker. The method returns the circuit breaker instance. It does not throw any exceptions.
* `ICircuitBreaker? TryGet(string name)`: Attempts to retrieve a circuit breaker with the given name. The `name` parameter is the unique identifier of the circuit breaker. The method returns the circuit breaker instance if found, or `null` if not found. It does not throw any exceptions.
* `void Reset(string name)`: Resets the circuit breaker with the given name. The `name` parameter is the unique identifier of the circuit breaker. The method does not return any value. It throws an exception if the circuit breaker with the given name does not exist.
* `IReadOnlyDictionary<int, CircuitBreakerState> GetAllStates()`: Retrieves the states of all circuit breakers in the registry. The method returns a dictionary where the key is the identifier of the circuit breaker and the value is its state. It does not throw any exceptions.

## Usage
The following example demonstrates how to use the `ICircuitBreakerRegistry` to create and retrieve circuit breakers:
```csharp
var registry = new CircuitBreakerRegistry();
var breaker = registry.GetOrCreate("myService");
// Use the circuit breaker
```
Another example shows how to reset a circuit breaker:
```csharp
var registry = new CircuitBreakerRegistry();
registry.Reset("myService");
```

## Notes
The `ICircuitBreakerRegistry` is designed to be thread-safe, allowing multiple threads to access and modify the circuit breakers concurrently. However, the `Reset` method may throw an exception if the circuit breaker with the given name does not exist, so it should be used with caution. Additionally, the `GetAllStates` method returns a snapshot of the current states of the circuit breakers, which may not reflect the actual state of the circuit breakers at the time of retrieval. Edge cases such as concurrent modifications to the circuit breakers or attempts to retrieve a non-existent circuit breaker should be handled carefully.
