# Error handling middleware

`ErrorHandlingMiddleware` is the outer exception boundary for the gateway's ASP.NET Core request pipeline. It records the request trace identifier, invokes the next component, and translates unhandled application exceptions into JSON error responses.

## Pipeline behavior

For every request, `InvokeAsync(HttpContext)`:

1. Reads `HttpContext.TraceIdentifier` as the request ID and logs that invocation has started.
2. Calls the next `RequestDelegate` in the pipeline.
3. Handles any exception raised by downstream middleware or endpoints.
4. Logs that invocation has completed.

Register the middleware before every component whose exceptions it should handle. The application currently places it near the start of the pipeline:

```csharp
using DotNetGrpcGateway.Infrastructure;

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

// Routing, authentication, other middleware, and endpoints follow.
```

ASP.NET Core supplies the constructor's `RequestDelegate` and `ILogger<ErrorHandlingMiddleware>` dependencies. Both constructor arguments, and the `HttpContext` passed to `InvokeAsync`, are required and are checked for `null`.

### Client disconnects and cancellation

Two exception paths do not create an error response:

- `ObjectDisposedException` is treated as a client disconnect during streaming and logged at `Debug` level.
- `OperationCanceledException` is treated as client cancellation only when `HttpContext.RequestAborted` is already cancelled. It is also logged at `Debug` level.

In both cases the exception is consumed and response status and body are left unchanged by this middleware. An `OperationCanceledException` raised while `RequestAborted` is not cancelled follows the normal unhandled-exception path and maps to a 500 response.

### Unhandled exceptions

All other exceptions are logged at `Warning` level with their type and request ID, then at `Error` level with the exception and its message. The middleware sets the response content type to `application/json`, selects a status and error code, and writes an `ErrorResponse` as JSON.

Exception matching follows the order shown below. Because `GatewayException` is checked first, its own status, code, and details take precedence over the general mappings.

| Exception | HTTP status | `errorCode` | `details` |
| --- | ---: | --- | --- |
| `GatewayException` | `HttpStatusCode`, or 500 when it is `null` | Exception's `ErrorCode` | Exception's `Details` |
| `ArgumentException` | 400 Bad Request | `VALIDATION_ERROR` | `null` |
| `UnauthorizedAccessException` | 401 Unauthorized | `UNAUTHORIZED` | `null` |
| `KeyNotFoundException` | 404 Not Found | `NOT_FOUND` | `null` |
| Any other exception | 500 Internal Server Error | `INTERNAL_ERROR` | `null` |

## Response format

The response body is the JSON serialization of `ErrorResponse`. With ASP.NET Core's web JSON naming convention, its fields are:

| Field | JSON type | Source |
| --- | --- | --- |
| `requestId` | string | `HttpContext.TraceIdentifier` |
| `timestamp` | string | `DateTime.UtcNow`, serialized in ISO 8601 format |
| `message` | string | `Exception.Message` |
| `errorCode` | string or `null` | Mapping above |
| `details` | object or `null` | `GatewayException.Details`; otherwise `null` |

Example response for a missing item:

```http
HTTP/1.1 404 Not Found
Content-Type: application/json
```

```json
{
  "requestId": "0HN7Q5J6P4A2C:00000001",
  "timestamp": "2026-09-13T12:00:00Z",
  "message": "Item not found",
  "errorCode": "NOT_FOUND",
  "details": null
}
```

A `GatewayException` can provide its own client-facing status, code, and structured details. For example, a `ServiceNotFoundException` produces a 404 response with error code `SERVICE_NOT_FOUND` and a `service_name` entry in `details`.

## Operational considerations

- The middleware only catches exceptions from components registered after it.
- Successful requests pass through unchanged apart from the start and completion information logs.
- The response exposes `Exception.Message` to clients. Exception messages should not contain credentials, tokens, internal connection strings, or other sensitive data.
- If the HTTP response has already started when an exception occurs, changing its status or writing the JSON body may fail; the middleware does not contain a separate `Response.HasStarted` path.
- JSON property naming and null-value inclusion can be affected by the application's ASP.NET Core JSON configuration.
