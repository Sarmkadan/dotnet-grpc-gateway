# Request Logs API

REST API for querying the structured request/response log store.

## Endpoints

### Get Recent Log Entries

```
GET /api/requestlogs
```

Returns the most recent request log entries.

#### Query Parameters

| Name | Type | Required | Description |
|------|------|----------|-------------|
| limit | integer | No | The maximum number of entries to return (default: 50, min: 1, max: 1000) |

#### Responses

| Status Code | Description |
|-------------|-------------|
| 200 OK | Returns an array of request log entries |

#### Example Response

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "timestamp": "2026-09-16T10:30:00Z",
    "logLevel": "INFO",
    "message": "Request completed - helloworld.Greeter.SayHello - Status: 200 - Duration: 15ms",
    "requestId": "a1b2c3d4-e5f6-7890-g1h2-i3j4k5l6m7n8",
    "correlationId": "00-0af7651916cd43dd8448eb211c80319c-00f067aa0ba902b7-01",
    "serviceName": "helloworld.Greeter",
    "methodName": "SayHello",
    "method": "POST",
    "path": "/helloworld.Greeter/SayHello",
    "grpcMethod": "helloworld.Greeter/SayHello",
    "httpStatusCode": 200,
    "durationMs": 15,
    "clientIp": "192.168.1.100",
    "upstreamAddress": "localhost:50051",
    "requestHeaders": {
      "content-type": "application/grpc",
      "user-agent": "grpc-go/1.45.0"
    },
    "requestSizeBytes": 120,
    "responseSizeBytes": 88,
    "errorMessage": null,
    "isSuccessful": true,
    "cacheHit": false,
    "retryCount": 0,
    "stackTrace": null
  }
]
```

### Search Log Entries

```
GET /api/requestlogs/search
```

Searches log entries by method, status code, or time range.

#### Query Parameters

| Name | Type | Required | Description |
|------|------|----------|-------------|
| method | string | No | The optional HTTP or gRPC method to match |
| statusCode | integer | No | The optional response status code to match |
| from | date-time | No | The optional earliest timestamp to include (ISO 8601 format) |
| to | date-time | No | The optional latest timestamp to include (ISO 8601 format) |
| limit | integer | No | The maximum number of entries to return (default: 50, min: 1, max: 1000) |

#### Responses

| Status Code | Description |
|-------------|-------------|
| 200 OK | Returns an array of matching request log entries |
| 400 Bad Request | When the 'from' timestamp is after the 'to' timestamp |

#### Example Request

```
GET /api/requestlogs/search?method=POST&statusCode=200&from=2026-09-16T00:00:00Z&to=2026-09-16T23:59:59Z&limit=100
```

#### Example Response

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "timestamp": "2026-09-16T10:30:00Z",
    "logLevel": "INFO",
    "message": "Request completed - helloworld.Greeter.SayHello - Status: 200 - Duration: 15ms",
    "requestId": "a1b2c3d4-e5f6-7890-g1h2-i3j4k5l6m7n8",
    "correlationId": "00-0af7651916cd43dd8448eb211c80319c-00f067aa0ba902b7-01",
    "serviceName": "helloworld.Greeter",
    "methodName": "SayHello",
    "method": "POST",
    "path": "/helloworld.Greeter/SayHello",
    "grpcMethod": "helloworld.Greeter/SayHello",
    "httpStatusCode": 200,
    "durationMs": 15,
    "clientIp": "192.168.1.100",
    "upstreamAddress": "localhost:50051",
    "requestHeaders": {
      "content-type": "application/grpc",
      "user-agent": "grpc-go/1.45.0"
    },
    "requestSizeBytes": 120,
    "responseSizeBytes": 88,
    "errorMessage": null,
    "isSuccessful": true,
    "cacheHit": false,
    "retryCount": 0,
    "stackTrace": null
  }
]
```

### Get Log Summary

```
GET /api/requestlogs/summary
```

Returns aggregate statistics over retained log entries.

#### Responses

| Status Code | Description |
|-------------|-------------|
| 200 OK | Returns aggregate statistics for the retained request log entries |

#### Example Response

```json
{
  "totalEntries": 1250,
  "successCount": 1180,
  "errorCount": 70,
  "successRatePct": 94.4,
  "averageDurationMs": 23.5,
  "minDurationMs": 2,
  "maxDurationMs": 1250,
  "oldestEntry": "2026-09-15T08:15:22Z",
  "newestEntry": "2026-09-16T10:30:00Z"
}
```

### Clear Log Entries

```
DELETE /api/requestlogs
```

Clears all retained log entries.

#### Responses

| Status Code | Description |
|-------------|-------------|
| 204 No Content | Indicates that the request log entries were cleared |

## Data Models

### RequestLogEntry

A single recorded request/response log entry captured by the gateway.

| Property | Type | Description |
|----------|------|-------------|
| id | string (uuid) | Unique identifier for the log entry |
| timestamp | string (date-time) | When the request was processed (UTC) |
| logLevel | string | Log level (INFO, WARN, ERROR) derived from entry state |
| message | string | Human-readable log message describing the request |
| requestId | string | Unique identifier for the request |
| correlationId | string | Correlation ID from the incoming request (W3C traceparent or X-Correlation-ID header) |
| serviceName | string | Name of the service handling the request |
| methodName | string | Name of the method being invoked |
| method | string | HTTP method (POST for gRPC) |
| path | string | Request path (e.g. /package.Service/Method) |
| grpcMethod | string | Resolved gRPC method name derived from the path |
| httpStatusCode | integer | HTTP status code of the response |
| durationMs | integer | Total request processing duration in milliseconds |
| clientIp | string | Client IP address |
| upstreamAddress | string | Upstream service address the request was forwarded to |
| requestHeaders | object | Request headers (sensitive values redacted) |
| requestSizeBytes | integer | Request body size in bytes |
| responseSizeBytes | integer | Response body size in bytes |
| errorMessage | string | Error message if the request failed |
| isSuccessful | boolean | Whether the request completed successfully (status < 400) |
| cacheHit | boolean | Whether the request was served from cache |
| retryCount | integer | Number of retry attempts for this request |
| stackTrace | string | Stack trace of any exception that occurred during processing |

### RequestLogSummary

Aggregate statistics over retained log entries.

| Property | Type | Description |
|----------|------|-------------|
| totalEntries | integer | Total number of log entries |
| successCount | integer | Number of successful requests |
| errorCount | integer | Number of failed requests |
| successRatePct | number | Percentage of successful requests |
| averageDurationMs | number | Average request processing duration in milliseconds |
| minDurationMs | integer | Minimum request processing duration in milliseconds |
| maxDurationMs | integer | Maximum request processing duration in milliseconds |
| oldestEntry | string (date-time) | Timestamp of the oldest log entry |
| newestEntry | string (date-time) | Timestamp of the newest log entry |

## Error Responses

### 400 Bad Request

Returned when search parameters are invalid, such as when the 'from' timestamp is after the 'to' timestamp.

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "traceId": "00-0af7651916cd43dd8448eb211c80319c-00f067aa0ba902b7-01"
}
```