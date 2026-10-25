# LoadBalancerController

`LoadBalancerController` is the REST management API for the gateway's load balancer. It lists and manages the endpoint pool of each backend service, toggles endpoint health and draining, and reads or changes the active load balancing strategy.

- **Source:** `src/dotnet-grpc-gateway/Controllers/LoadBalancerController.cs`
- **Service:** `ILoadBalancerService` (implemented by `LoadBalancerService`, registered as a singleton)
- **Base route:** `api/loadbalancer`
- **Response format:** `application/json`

For the routing model and strategy internals, see [LOAD-BALANCING.md](LOAD-BALANCING.md). For the service-level types, see [ILoadBalancerService](ILoadBalancerService.md).

## Endpoints

| Method | Route | Purpose | Success | Other responses |
|--------|-------|---------|---------|-----------------|
| GET | `/api/loadbalancer/services/{serviceId}/endpoints` | List all endpoints of a service | 200 `IReadOnlyList<ServiceEndpoint>` | — |
| POST | `/api/loadbalancer/services/{serviceId}/endpoints` | Register an endpoint for a service | 201 Created (body: endpoint) | 400 if body is missing |
| DELETE | `/api/loadbalancer/services/{serviceId}/endpoints/{endpointId}` | Remove an endpoint from a service pool | 204 No Content | — |
| PATCH | `/api/loadbalancer/services/{serviceId}/endpoints/{endpointId}/health` | Set health of an endpoint | 200 `{ serviceId, endpointId, isHealthy }` | — |
| PATCH | `/api/loadbalancer/services/{serviceId}/endpoints/{endpointId}/draining` | Set draining state of an endpoint | 200 `{ serviceId, endpointId, draining }` | — |
| GET | `/api/loadbalancer/strategy` | Get the active strategy | 200 `{ strategy }` | — |
| PUT | `/api/loadbalancer/strategy` | Set the active strategy | 200 `{ strategy }` | 400 if the name is unknown |

### GetEndpoints

`GET /api/loadbalancer/services/{serviceId}/endpoints`

Returns every endpoint registered for `serviceId`, healthy or not, draining or not. If the service has no pool, the response is an empty array.

### RegisterEndpoint

`POST /api/loadbalancer/services/{serviceId}/endpoints`

The body is a `ServiceEndpoint`. The controller overwrites `ServiceId` with the route value, so the body's `serviceId` is ignored.

```json
{
  "host": "10.0.0.5",
  "port": 50051,
  "useTls": false,
  "weight": 1
}
```

- A `null` body returns `400 Bad Request` with `Endpoint is required`.
- On success the response is `201 Created` with a `Location` header pointing at the service's endpoint collection and the stored endpoint as the body.
- Registering an endpoint whose `Id` already exists in the pool is a no-op in the service. The controller still returns 201, and the existing entry is not replaced.

### DeregisterEndpoint

`DELETE /api/loadbalancer/services/{serviceId}/endpoints/{endpointId}`

Removes the endpoint with `Id == endpointId` from the service pool. Returns `204 No Content` whether or not the endpoint existed.

### UpdateEndpointHealth

`PATCH /api/loadbalancer/services/{serviceId}/endpoints/{endpointId}/health`

The body is a bare JSON boolean:

```json
true
```

Sets `IsHealthy` on the endpoint. An unhealthy endpoint is excluded from selection until it is marked healthy again. Unknown service or endpoint IDs are silently ignored, and the response is still `200 OK`.

### SetEndpointDraining

`PATCH /api/loadbalancer/services/{serviceId}/endpoints/{endpointId}/draining`

The body is a bare JSON boolean (`true` to drain, `false` to restore). Sets `Draining` on the endpoint. A draining endpoint receives no new requests while any non-draining healthy endpoint remains in the pool. Like the health route, unknown IDs return `200 OK` without effect.

### GetStrategy

`GET /api/loadbalancer/strategy`

```json
{ "strategy": "RoundRobin" }
```

### SetStrategy

`PUT /api/loadbalancer/strategy`

The body is a bare JSON string containing the strategy name. Parsing is case-insensitive.

```json
"LeastConnections"
```

- Valid values: `RoundRobin`, `Random`, `LeastConnections`.
- An unknown name returns `400 Bad Request` with the message `Unknown strategy '<name>'. Valid values: RoundRobin, Random, LeastConnections`.
- On success the new strategy takes effect on the next selection and the response echoes the canonical name.

## Strategies

The strategy is a property of the `LoadBalancerService` singleton, so it applies to all services at once. The default is `RoundRobin`.

Every strategy runs the same candidate filter first:

1. Only endpoints with `IsHealthy == true` are considered. If none are healthy, selection returns `null`, and a warning is logged.
2. Draining endpoints are removed from the candidates. If every healthy endpoint is draining, the draining ones are used so that traffic is not dropped entirely.

The remaining candidates are then passed to the active strategy.

### RoundRobin (default)

Cycles through the candidates in pool order. Each service has its own counter, kept in a `ConcurrentDictionary`, so rotation is independent between services. Because the candidate list shrinks and grows as health and draining states change, the position in the rotation can jump when the pool changes.

### Random

Picks a candidate uniformly at random using `Random.Shared`. There is no memory between calls, so it works well when many gateway instances share the same backends.

### LeastConnections

Picks the candidate with the lowest `ActiveConnections` value (`MinBy`). Ties are resolved by pool order.

> **Current limitation:** nothing in the codebase increments `ActiveConnections`. `RecordRequestCompleted` only decrements it, clamped at 0. In practice every endpoint reads 0, so `LeastConnections` currently behaves like "first candidate in pool order" rather than balancing by load. The connection counter needs to be incremented when a request starts for this strategy to have an effect.

## Behavior notes

- **Idempotency:** `DeregisterEndpoint`, `UpdateEndpointHealth`, and `SetEndpointDraining` return the same result whether or not the target exists. Callers that need to detect a missing endpoint must check `GET .../endpoints` first.
- **No validation in the controller:** the controller does not call `LoadBalancerControllerValidation`. Only the endpoint body and strategy name are checked, as described above. The validation helpers are available for callers that want to validate `ServiceEndpoint` (host length, port 1–65535, weight 1–100, and date consistency) or service and endpoint IDs (≥ 1) before calling the service.
- **Weights:** `ServiceEndpoint.Weight` is stored and validated, but none of the three strategies currently use it.
- **Concurrency:** the controller holds no state. Pool mutations are protected by locks on each service's endpoint list. Strategy changes are plain property writes and take effect on the next selection.
- **Errors:** the controller has no try/catch. Unexpected exceptions are handled by `ErrorHandlingMiddleware` (registered in `Program.cs`).

## Examples

Register an endpoint, mark it healthy, and switch to least-connections:

```bash
BASE=http://localhost:5000/api/loadbalancer

curl -i -X POST "$BASE/services/1/endpoints" \
  -H "Content-Type: application/json" \
  -d '{"host":"10.0.0.5","port":50051,"useTls":false,"weight":1}'

curl -X PATCH "$BASE/services/1/endpoints/3/health" \
  -H "Content-Type: application/json" -d 'true'

curl -X PUT "$BASE/strategy" \
  -H "Content-Type: application/json" -d '"LeastConnections"'
```

Drain an endpoint before removing it from the pool:

```bash
curl -X PATCH "$BASE/services/1/endpoints/3/draining" \
  -H "Content-Type: application/json" -d 'true'

curl -X DELETE "$BASE/services/1/endpoints/3"
```
