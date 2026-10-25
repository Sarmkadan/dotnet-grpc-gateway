# ReflectionController

`ReflectionController` is the REST API for the gRPC Server Reflection cache. It exposes the reflection snapshot the gateway holds for each registered back-end service, lets callers trigger a probe, and reports whether the reflection subsystem is reachable. It is a thin wrapper over `IReflectionService`: the GET endpoints read the cache, and the POST endpoints probe the back-end and then update it.

- **Namespace**: `DotNetGrpcGateway.Controllers`
- **Base route**: `api/Reflection`
- **Response content type**: `application/json`
- **Dependencies**: `IReflectionService`, `ILogger<ReflectionController>`
- **Registration**: the service requires `AddGatewayReflection()`, which registers `IReflectionService` and the `reflection` health check. See [ServiceCollectionExtensions](ServiceCollectionExtensions.md).

## How the data is populated

- **Reads do not contact back-ends.** The GET endpoints and `health` only read the in-memory cache. A service has no entry until one of the `refresh` endpoints has probed it. Until then, `GET api/Reflection` returns an empty list.
- **Probe.** Each probe sends a `HEAD` request to `{service endpoint}/grpc.reflection.v1alpha.ServerReflection`. A response of `200`, `405`, or `415` marks the service as available. A timeout uses `DotnetGrpcGatewayOptions.HealthCheck.TimeoutMs`. Any other failure marks it unavailable and stores the exception message in `errorMessage`.
- **Methods are placeholders.** The method list is not read from the reflection response or from `.proto` files. For an available service, `ReflectionService` builds six methods from the service name: `Get`, `List`, `Create`, `Update`, `Delete`, and `Watch`. Treat `methods` as approximate.
- **Bulk refresh respects `EnableReflection`.** When `DotnetGrpcGatewayOptions.EnableReflection` is `false`, `POST api/Reflection/refresh` returns `202 Accepted` without probing anything. The single-service refresh does not check this flag.

## Endpoints

| Method | Route | Action | Success | Other status codes |
|--------|-------|--------|---------|--------------------|
| GET | `/api/Reflection` | `GetAllReflection` | 200 | 500 |
| GET | `/api/Reflection/health` | `GetReflectionHealth` | 200 | 503 |
| GET | `/api/Reflection/{serviceId}` | `GetServiceReflection` | 200 | 404, 500 |
| GET | `/api/Reflection/{serviceId}/methods` | `GetServiceMethods` | 200 | 404, 500 |
| POST | `/api/Reflection/{serviceId}/refresh` | `RefreshServiceReflection` | 200 | 404, 500 |
| POST | `/api/Reflection/refresh` | `RefreshAllReflections` | 202 | 500 |

`serviceId` is an integer route parameter (`{serviceId:int}`) and identifies a registered `GrpcService`. The literal segments `health` and `refresh` do not conflict with the integer route. A non-integer segment such as `/api/Reflection/abc` matches no route and returns `404` from routing, before the controller runs.

## API

### `public ReflectionController(IReflectionService reflectionService, ILogger<ReflectionController> logger)`

Constructor used by dependency injection. Both arguments are required; `null` throws `ArgumentNullException`. The controller holds no state of its own.

### `public async Task<ActionResult<IReadOnlyList<ServiceReflectionInfo>>> GetAllReflection(CancellationToken cancellationToken)`

`GET /api/Reflection`

Returns the cached snapshot for every service that has one.

- **Parameters**: none. `cancellationToken` is supplied by the framework.
- **200 OK**: a JSON array of `ServiceReflectionInfo`. The array is empty if nothing has been refreshed.
- **500 Internal Server Error**: an unexpected exception was thrown. It is logged and the body is empty.

### `public async Task<ActionResult<ServiceReflectionInfo>> GetServiceReflection(int serviceId, CancellationToken cancellationToken)`

`GET /api/Reflection/{serviceId}`

Returns the cached snapshot for one service.

- **Parameters**: `serviceId` (route, `int`): the registered service.
- **200 OK**: a `ServiceReflectionInfo`.
- **404 Not Found**: nothing is cached for `serviceId`. The body is the message `No reflection data available for service {serviceId}. Call POST /api/reflection/{serviceId}/refresh first.`
- **500 Internal Server Error**: an unexpected exception was thrown.

### `public async Task<ActionResult<List<ServiceMethodDescriptor>>> GetServiceMethods(int serviceId, CancellationToken cancellationToken)`

`GET /api/Reflection/{serviceId}/methods`

Returns only the `methods` array of the cached snapshot.

- **Parameters**: `serviceId` (route, `int`).
- **200 OK**: a JSON array of `ServiceMethodDescriptor`.
- **404 Not Found**: nothing is cached for `serviceId`. The body is `No reflection data for service {serviceId}`.
- **500 Internal Server Error**: an unexpected exception was thrown.

### `public async Task<ActionResult<ServiceReflectionInfo>> RefreshServiceReflection(int serviceId, CancellationToken cancellationToken)`

`POST /api/Reflection/{serviceId}/refresh`

Probes one service, replaces its cache entry, and returns the new snapshot.

- **Parameters**: `serviceId` (route, `int`).
- **200 OK**: the refreshed `ServiceReflectionInfo`. A failed probe still returns `200`, with `isAvailable: false` and `errorMessage` set (for example, `Reflection probe timed out`). Check `isAvailable`, not only the status code.
- **404 Not Found**: returned only when the service layer throws `KeyNotFoundException`. In practice `IGatewayService.GetServiceAsync` wraps repository exceptions in `ConfigurationException` and rejects non-positive IDs with `ArgumentOutOfRangeException`. Both surface as `500`, so an unknown `serviceId` usually returns `500`, not `404`.
- **500 Internal Server Error**: any other exception.

### `public async Task<ActionResult> RefreshAllReflections(CancellationToken cancellationToken)`

`POST /api/Reflection/refresh`

Probes all active services concurrently and replaces the cache with the results.

- **Parameters**: none.
- **202 Accepted**: returned after the run finishes, even when no service responds or when `EnableReflection` is `false`. The body is:

  ```json
  {
    "message": "Reflection refresh completed",
    "timestamp": "2026-10-25T10:00:00.000Z"
  }
  ```

- **500 Internal Server Error**: the refresh threw an exception.

### `public async Task<ActionResult<ReflectionHealthSummary>> GetReflectionHealth(CancellationToken cancellationToken)`

`GET /api/Reflection/health`

Summarises the cache. It does not probe back-ends, so run a refresh first if the result may be stale.

- **Parameters**: none.
- **200 OK**: `isAvailable` is `true` when at least one cached service is available, or when no services are cached. `message` is `No services registered; reflection subsystem is idle` when the cache is empty.
- **503 Service Unavailable**: services are cached but none is available. The body has the same shape, with `isAvailable: false`. If the check throws, the body also has `isAvailable: false`, `message` set to the exception message, and `checkedAt` set to the current time.

The `503` path on exception copies `ex.Message` into the response body. Be aware of this if the endpoint is publicly reachable.

## Response models

### `ServiceReflectionInfo`

| Field | Type | Description |
|-------|------|-------------|
| `serviceId` | integer | Identifier of the registered `GrpcService`. |
| `serviceName` | string | Short service name. |
| `serviceFullName` | string | Fully qualified gRPC service name (`package.ServiceName`). |
| `methods` | array of `ServiceMethodDescriptor` | RPC methods. Empty when the service is unavailable. |
| `reflectedAt` | string (ISO 8601) | When the snapshot was taken. |
| `isAvailable` | boolean | Whether the reflection endpoint responded with `200`, `405`, or `415`. |
| `errorMessage` | string or `null` | Diagnostic message when the probe failed. |
| `methodCount` | integer | Number of entries in `methods`. |
| `streamingMethodCount` | integer | Number of methods with client or server streaming. |

### `ServiceMethodDescriptor`

| Field | Type | Description |
|-------|------|-------------|
| `name` | string | Unqualified method name. |
| `requestType` | string | Request message type name. |
| `responseType` | string | Response message type name. |
| `isClientStreaming` | boolean | Client sends a stream. |
| `isServerStreaming` | boolean | Server sends a stream. |
| `streamingMode` | string | `Unary`, `ClientStreaming`, `ServerStreaming`, or `BidirectionalStreaming`. |

### `ReflectionHealthSummary`

Declared in `ReflectionController.cs`, next to the controller.

| Field | Type | Description |
|-------|------|-------------|
| `isAvailable` | boolean | At least one cached service is available. |
| `availableServiceCount` | integer | Number of cached services that are available. |
| `totalServiceCount` | integer | Number of cached services. |
| `checkedAt` | string (ISO 8601) | When the summary was generated (UTC). |
| `message` | string or `null` | Human-readable status, or the exception message on failure. |

Field names follow the application's JSON serializer settings. ASP.NET Core defaults to camelCase, as shown above.

## Usage

Refresh one service, then read its methods:

```bash
curl -X POST http://localhost:5000/api/Reflection/1/refresh
curl http://localhost:5000/api/Reflection/1/methods
```

Check the subsystem after a bulk refresh:

```bash
curl -X POST http://localhost:5000/api/Reflection/refresh
curl http://localhost:5000/api/Reflection/health
```

Replace `localhost:5000` with the gateway's address.

## Notes

- The controller is stateless. All state lives in the `IReflectionService` cache, so concurrent requests to the controller are safe at the controller level.
- The cache is per service instance. Each process has its own snapshot, and it is lost on restart.
- GET endpoints can return stale data. Nothing refreshes the cache in the background. Call a refresh endpoint to update it.
