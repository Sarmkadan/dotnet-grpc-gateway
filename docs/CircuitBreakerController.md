# CircuitBreakerController

`CircuitBreakerController` is the REST API for inspecting and manually resetting the per-service circuit breakers used by the gateway. It is a thin wrapper over `ICircuitBreakerRegistry`: every action reads or changes the registry and returns JSON. Breakers are keyed by the integer `serviceId`, not by name.

- **Namespace**: `DotNetGrpcGateway.Controllers`
- **Base route**: `api/CircuitBreaker`
- **Response content type**: `application/json`
- **Dependencies**: `ICircuitBreakerRegistry`, `ILogger<CircuitBreakerController>`

## Endpoints

| Method | Route | Purpose | Success | Not found |
|--------|-------|---------|---------|-----------|
| GET | `/api/CircuitBreaker` | State of all breakers | 200 | — |
| GET | `/api/CircuitBreaker/services/{serviceId}` | Detailed status of one breaker | 200 | 404 |
| POST | `/api/CircuitBreaker/services/{serviceId}/reset` | Manually reset one breaker to Closed | 200 | 404 |

## API

### `public CircuitBreakerController(ICircuitBreakerRegistry registry, ILogger<CircuitBreakerController> logger)`

Constructor used by dependency injection. Both arguments are required; `null` throws `ArgumentNullException`. The controller holds no state of its own.

### `public ActionResult GetAll()`

`GET /api/CircuitBreaker`

Returns the state of every circuit breaker in the registry.

- **Response body**: a JSON object that maps each `serviceId` (serialized as a string key) to the breaker's state name: `"Closed"`, `"Open"`, or `"HalfOpen"`.
- **Empty registry**: returns `200 OK` with `{}`.
- **Status codes**: `200 OK`.

Example response:

```json
{
  "1": "Closed",
  "2": "Open",
  "7": "HalfOpen"
}
```

### `public ActionResult GetStatus(int serviceId)`

`GET /api/CircuitBreaker/services/{serviceId}`

Returns the detailed status of the circuit breaker for one service.

- **Parameters**:
  - `serviceId` (`int`, route): the identifier of the service whose breaker to inspect.
- **Response body** (`200 OK`):

  | Field | Type | Description |
  |-------|------|-------------|
  | `serviceId` | integer | The requested service identifier. |
  | `state` | string | `"Closed"`, `"Open"`, or `"HalfOpen"`. |
  | `consecutiveFailures` | integer | Failures recorded since the last success. |
  | `openedAt` | string (ISO 8601) or `null` | When the breaker last opened. `null` if it has not opened. |

- **Status codes**:
  - `200 OK`: the breaker exists.
  - `404 Not Found`: no breaker is registered for `serviceId`. The body is the message `No circuit breaker registered for service {serviceId}`.

Example response:

```json
{
  "serviceId": 2,
  "state": "Open",
  "consecutiveFailures": 5,
  "openedAt": "2026-10-25T10:14:03.512Z"
}
```

The lookup uses `TryGet`, so this action never creates a breaker. A service that has never been called has no breaker and returns 404.

### `public ActionResult Reset(int serviceId)`

`POST /api/CircuitBreaker/services/{serviceId}/reset`

Manually resets the circuit breaker for one service to the Closed state. The request has no body.

- **Parameters**:
  - `serviceId` (`int`, route): the identifier of the service whose breaker to reset.
- **Behavior**: the controller first checks that the breaker exists, then calls `ICircuitBreakerRegistry.Reset(serviceId)`. The guard prevents the registry from throwing for an unknown service. Reset clears the breaker's failure state; it does not create a breaker.
- **Status codes**:
  - `200 OK`: the breaker was reset.
  - `404 Not Found`: no breaker is registered for `serviceId`. Nothing is changed. The body is the message `No circuit breaker registered for service {serviceId}`.
- **Response body** (`200 OK`): the response always reports `"Closed"`, because the action returns the state it reset to, not a fresh read of the breaker.

Example response:

```json
{
  "serviceId": 2,
  "state": "Closed"
}
```

## Response fields and state values

| State | Meaning |
|-------|---------|
| `Closed` | Normal operation; requests flow through. |
| `Open` | The failure threshold was exceeded; requests are rejected immediately. |
| `HalfOpen` | The breaker is testing whether the downstream service has recovered. |

The state names come from the `CircuitBreakerState` enum and are serialized with `ToString()`.

## Usage

### Example 1: Monitoring all circuit breakers

```bash
curl -s http://localhost:5000/api/CircuitBreaker
```

### Example 2: Inspecting one service

```bash
curl -s http://localhost:5000/api/CircuitBreaker/services/2
```

### Example 3: Resetting a breaker after the downstream service recovers

```bash
curl -s -X POST http://localhost:5000/api/CircuitBreaker/services/2/reset
```

### Example 4: Calling the endpoints from C#

```csharp
using var http = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };

var all = await http.GetFromJsonAsync<Dictionary<string, string>>("api/CircuitBreaker");

var status = await http.GetAsync("api/CircuitBreaker/services/2");
if (status.IsSuccessStatusCode)
{
    var details = await status.Content.ReadFromJsonAsync<JsonElement>();
}

var reset = await http.PostAsync("api/CircuitBreaker/services/2/reset", content: null);
```

## Notes

- **Identifiers are integers.** A non-integer `serviceId` in the route does not match a valid breaker and is rejected by model binding.
- **Logging.** Each action logs at `Information` level on entry and completion. A missing breaker is logged at `Warning` level.
- **No authorization attributes.** The controller does not declare any authorization or rate-limiting attributes. Protect the `api/CircuitBreaker` routes at the application level (for example, through the gateway's authentication middleware) if they should not be publicly reachable.
- **Manual reset bypasses the breaker's normal recovery.** `Reset` closes the breaker immediately, even if the downstream service is still failing. The next failures will reopen it once the threshold is reached again.

## Related

- [CircuitBreaker](CircuitBreaker.md): the breaker implementation, options, and state transitions.
- [ICircuitBreakerRegistry](ICircuitBreakerRegistry.md): the registry interface used by this controller.
- [HealthController](HealthController.md) and [MetricsController](MetricsController.md): other monitoring endpoints.
