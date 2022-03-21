# ILoadBalancerService

The `ILoadBalancerService` interface defines a contract for managing and distributing load across multiple service endpoints in a gRPC gateway environment. It provides methods to register, deregister, and monitor endpoints, as well as to retrieve endpoints for load balancing purposes. Implementations of this interface are responsible for tracking endpoint health and selecting the next available endpoint based on the configured load balancing strategy.

## API

### `LoadBalancingStrategy Strategy`

Gets the strategy used by the load balancer to select endpoints. This property returns a value indicating the algorithm (e.g., round-robin, least connections) employed when determining which endpoint to use next.

### `LoadBalancerService()`

Constructs a new instance of the load balancer service. This constructor initializes the internal state required for tracking endpoints and their health status.

### `void RegisterEndpoint(ServiceEndpoint endpoint)`

Registers a new service endpoint with the load balancer. The endpoint becomes eligible for selection by `GetNextEndpoint` once registered.

- **Parameters**:
  - `endpoint`: The service endpoint to register. Must not be `null`.
- **Throws**:
  - `ArgumentNullException`: If `endpoint` is `null`.

### `void DeregisterEndpoint(ServiceEndpoint endpoint)`

Removes a previously registered service endpoint from the load balancer. The endpoint will no longer be considered for load balancing operations after deregistration.

- **Parameters**:
  - `endpoint`: The service endpoint to deregister. Must not be `null`.
- **Throws**:
  - `ArgumentNullException`: If `endpoint` is `null`.
  - `InvalidOperationException`: If the endpoint was not previously registered.

### `ServiceEndpoint? GetNextEndpoint()`

Selects and returns the next available service endpoint based on the configured `Strategy`. Returns `null` if no endpoints are currently registered or all endpoints are unhealthy.

- **Returns**:
  - The next `ServiceEndpoint` to use, or `null` if none are available.

### `IReadOnlyList<ServiceEndpoint> GetEndpoints()`

Returns an immutable list of all currently registered service endpoints, including unhealthy ones.

- **Returns**:
  - An `IReadOnlyList<ServiceEndpoint>` containing all registered endpoints.

### `void UpdateEndpointHealth(ServiceEndpoint endpoint, bool isHealthy)`

Updates the health status of a registered service endpoint.

- **Parameters**:
  - `endpoint`: The service endpoint whose health status is to be updated. Must not be `null`.
  - `isHealthy`: `true` if the endpoint is healthy; otherwise, `false`.
- **Throws**:
  - `ArgumentNullException`: If `endpoint` is `null`.
  - `InvalidOperationException`: If the endpoint was not previously registered.

### `void RecordRequestCompleted(ServiceEndpoint endpoint)`

Records the completion of a request to the specified endpoint. Implementations may use this to track metrics such as response times or error rates for load balancing decisions.

- **Parameters**:
  - `endpoint`: The service endpoint that handled the request. Must not be `null`.
- **Throws**:
  - `ArgumentNullException`: If `endpoint` is `null`.
  - `InvalidOperationException`: If the endpoint was not previously registered.

## Usage

### Example 1: Basic Load Balancing Setup
