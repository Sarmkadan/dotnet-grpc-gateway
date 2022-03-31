# LoadBalancerServiceTests

This test class contains unit tests for the `LoadBalancerService` implementation, verifying the correctness of endpoint selection algorithms and lifecycle operations such as registration, deregistration, and health updates.

## API

### GetNextEndpoint_NoEndpointsRegistered_ReturnsNull
- **Purpose**: Confirms that invoking `GetNextEndpoint` on a service with no registered endpoints yields `null`.
- **Parameters**: None.
- **Return Value**: `void` (test method). Success is indicated by the test passing; failure occurs if the method returns a non‑null value.
- **Throws**: No exceptions are expected; the test will fail if an unexpected exception is thrown.

### GetNextEndpoint_AllEndpointsUnhealthy_ReturnsNull
- **Purpose**: Ensures that when all registered endpoints are marked unhealthy, `GetNextEndpoint` returns `null`.
- **Parameters**: None.
- **Return Value**: `void`. The test passes if the method returns `null`; otherwise it fails.
- **Throws**: No expected exceptions; any exception causes test failure.

### GetNextEndpoint_RoundRobin_CyclesThroughEndpoints
- **Purpose**: Validates that the round‑robin selection logic cycles through the registered endpoints in the order they were added.
- **Parameters**: None.
- **Return Value**: `void`. The test asserts successive calls return each endpoint once before repeating.
- **Throws**: No expected exceptions; failure indicates a deviation from round‑robin behavior.

### GetNextEndpoint_LeastConnections_SelectsEndpointWithFewerConnections
- **Purpose**: Checks that the least‑connections algorithm selects the endpoint with the lowest current connection count.
- **Parameters**: None.
- **Return Value**: `void`. The test verifies that the returned endpoint matches the one with the minimal connections.
- **Throws**: No expected exceptions; any exception results in test failure.

### DeregisterEndpoint_RemovesEndpointFromPool
- **Purpose**: Asserts that calling `DeregisterEndpoint` removes the specified endpoint from the internal pool so it is no longer considered for selection.
- **Parameters**: None.
- **Return Value**: `void`. The test passes if subsequent calls to `GetNextEndpoint` never return the deregistered endpoint.
- **Throws**: No expected exceptions; failure indicates the endpoint remains selectable.

### UpdateEndpointHealth_MarksEndpointUnhealthy_ExcludesFromSelection
- **Purpose**: Confirms that marking an endpoint as unhealthy via `UpdateEndpointHealth` excludes it from future selections until it is marked healthy again.
- **Parameters**: None.
- **Return Value**: `void`. The test passes if the unhealthy endpoint is never returned by `GetNextEndpoint` while unhealthy.
- **Throws**: No expected exceptions; any exception leads to test failure.

## Usage

The following examples illustrate typical interactions with `LoadBalancerService` that correspond to the scenarios covered by the tests.

### Example 1: Round‑robin selection
```csharp
var service = new LoadBalancerService();
service.RegisterEndpoint("http://endpoint1");
service.RegisterEndpoint("http://endpoint2");
service.RegisterEndpoint("http://endpoint3");

// First three calls should return each endpoint in order.
Assert.AreEqual("http://endpoint1", service.GetNextEndpoint());
Assert.AreEqual("http://endpoint2", service.GetNextEndpoint());
Assert.AreEqual("http://endpoint3", service.GetNextEndpoint();

// Fourth call wraps back to the first endpoint.
Assert.AreEqual("http://endpoint1", service.GetNextEndpoint());
```

### Example 2: Least‑connections selection with health updates
```csharp
var service = new LoadBalancerService();
service.RegisterEndpoint("http://a");
service.RegisterEndpoint("http://b");

// Simulate different connection counts.
service.IncrementConnectionCount("http://a"); // a:1, b:0
service.IncrementConnectionCount("http://a"); // a:2, b:0

// The endpoint with fewer connections (b) should be selected.
Assert.AreEqual("http://b", service.GetNextEndpoint());

// Mark b as unhealthy; now a should be selected despite higher count.
service.UpdateEndpointHealth("http://b", false);
Assert.AreEqual("http://a", service.GetNextEndpoint());
```

## Notes
- The `LoadBalancerService` class is **not thread‑safe**; concurrent calls to `RegisterEndpoint`, `DeregisterEndpoint`, `UpdateEndpointHealth`, or `GetNextEndpoint` from multiple threads may result in undefined behavior. External synchronization is required for multi‑threaded scenarios.
- If all endpoints are deregistered or marked unhealthy, `GetNextEndpoint` will consistently return `null` until at least one healthy endpoint is re‑registered or restored to a healthy state.
- The least‑connections algorithm assumes that connection counts are updated accurately by the caller; incorrect counts can lead to suboptimal endpoint selection.
- Test methods in this class are designed to run in isolation; they do not rely on shared static state and therefore can be executed in any order without side effects.
