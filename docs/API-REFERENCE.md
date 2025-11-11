# API Reference

Complete REST API reference for dotnet-grpc-gateway.

## Base URL

```
http://localhost:5000/api
```

## Authentication

APIs supporting authentication use Bearer tokens:

```
Authorization: Bearer <your-token>
```

## Response Format

All responses are JSON unless otherwise specified:

```json
{
  "data": {...},
  "success": true,
  "message": "Operation completed successfully"
}
```

Error responses:

```json
{
  "error": {
    "code": "INVALID_REQUEST",
    "message": "Service not found",
    "details": {
      "serviceId": 999
    }
  },
  "success": false,
  "timestamp": "2026-05-04T12:00:00Z"
}
```

---

## Gateway Configuration

### Get Gateway Configuration

```
GET /gateway/configuration
```

**Response** (200 OK):
```json
{
  "id": 1,
  "port": 5000,
  "listenAddress": "0.0.0.0",
  "maxConcurrentConnections": 1000,
  "requestTimeoutMs": 30000,
  "enableMetrics": true,
  "enableReflection": true
}
```

### Update Gateway Configuration

```
PUT /gateway/configuration
Content-Type: application/json
```

**Request**:
```json
{
  "port": 5000,
  "maxConcurrentConnections": 2000,
  "requestTimeoutMs": 45000
}
```

**Response** (200 OK):
```json
{
  "id": 1,
  "port": 5000,
  "maxConcurrentConnections": 2000,
  "requestTimeoutMs": 45000
}
```

---

## Service Management

### List All Services

```
GET /gateway/services
```

**Query Parameters**:
- `limit` (optional): Max results (default: 100)
- `offset` (optional): Skip N results (default: 0)
- `isHealthy` (optional): Filter by health status

**Response** (200 OK):
```json
[
  {
    "id": 1,
    "name": "UserService",
    "serviceFullName": "user.UserService",
    "host": "api.example.com",
    "port": 5001,
    "useTls": true,
    "isHealthy": true,
    "lastHealthCheckAt": "2026-05-04T12:00:00Z",
    "healthCheckIntervalSeconds": 30,
    "registeredAt": "2026-05-03T10:30:00Z"
  }
]
```

### Get Service Details

```
GET /gateway/services/{serviceId}
```

**Path Parameters**:
- `serviceId`: Numeric service ID

**Response** (200 OK):
```json
{
  "id": 1,
  "name": "UserService",
  "serviceFullName": "user.UserService",
  "host": "api.example.com",
  "port": 5001,
  "useTls": true,
  "isHealthy": true,
  "lastHealthCheckAt": "2026-05-04T12:00:00Z"
}
```

**Errors**:
- 404 Not Found: Service doesn't exist

### Register New Service

```
POST /gateway/services
Content-Type: application/json
```

**Request**:
```json
{
  "name": "UserService",
  "serviceFullName": "user.UserService",
  "host": "api.example.com",
  "port": 5001,
  "useTls": true,
  "healthCheckIntervalSeconds": 30
}
```

**Required Fields**:
- `name`: Service display name
- `serviceFullName`: Fully qualified service name (from .proto)
- `host`: Service hostname or IP
- `port`: Service port
- `useTls`: Enable TLS/SSL
- `healthCheckIntervalSeconds`: Health check frequency

**Response** (201 Created):
```json
{
  "id": 1,
  "name": "UserService",
  "host": "api.example.com",
  "port": 5001,
  "isHealthy": false
}
```

### Delete Service

```
DELETE /gateway/services/{serviceId}
```

**Response** (204 No Content)

**Errors**:
- 404 Not Found: Service doesn't exist
- 409 Conflict: Routes depend on this service

---

## Route Management

### List All Routes

```
GET /gateway/routes
```

**Query Parameters**:
- `serviceId` (optional): Filter by service
- `pattern` (optional): Filter by pattern
- `limit` (optional): Max results (default: 100)

**Response** (200 OK):
```json
[
  {
    "id": 1,
    "pattern": "user.UserService.*",
    "targetServiceId": 1,
    "matchType": 1,
    "priority": 100,
    "rateLimitPerMinute": 1000,
    "enableCaching": true,
    "cacheDurationSeconds": 300,
    "requiresAuthentication": false,
    "createdAt": "2026-05-03T10:30:00Z"
  }
]
```

### Create Route

```
POST /gateway/routes
Content-Type: application/json
```

**Request**:
```json
{
  "pattern": "user.UserService.*",
  "targetServiceId": 1,
  "matchType": 1,
  "priority": 100,
  "rateLimitPerMinute": 1000,
  "enableCaching": true,
  "cacheDurationSeconds": 300,
  "requiresAuthentication": false
}
```

**Fields**:
- `pattern`: Route pattern (supports wildcards)
- `targetServiceId`: Target service ID
- `matchType`: 0=Exact, 1=Prefix, 2=Regex
- `priority`: Higher = checked first
- `rateLimitPerMinute`: Rate limit (0 = unlimited)
- `enableCaching`: Enable response caching
- `cacheDurationSeconds`: Cache TTL in seconds
- `requiresAuthentication`: Require bearer token

**Response** (201 Created):
```json
{
  "id": 1,
  "pattern": "user.UserService.*",
  "targetServiceId": 1,
  "priority": 100
}
```

### Delete Route

```
DELETE /gateway/routes/{routeId}
```

**Response** (204 No Content)

---

## Health Monitoring

### Gateway Health Status

```
GET /health
```

**Response** (200 OK):
```json
{
  "status": "Healthy",
  "timestamp": "2026-05-04T12:00:00Z"
}
```

### Detailed Health Status

```
GET /api/health/status
```

**Response** (200 OK):
```json
{
  "status": "Healthy",
  "uptime": "12:34:56",
  "requestsProcessed": 125000,
  "activeConnections": 42,
  "cacheHitRate": 0.78
}
```

### All Services Health

```
GET /api/health/services
```

**Response** (200 OK):
```json
[
  {
    "serviceId": 1,
    "serviceName": "UserService",
    "isHealthy": true,
    "lastCheckAt": "2026-05-04T12:00:00Z",
    "responseTimeMs": 15
  },
  {
    "serviceId": 2,
    "serviceName": "ProductService",
    "isHealthy": false,
    "lastCheckAt": "2026-05-04T12:00:00Z",
    "responseTimeMs": null
  }
]
```

### Specific Service Health

```
GET /api/health/services/{serviceId}
```

**Response** (200 OK):
```json
{
  "serviceId": 1,
  "serviceName": "UserService",
  "isHealthy": true,
  "lastCheckAt": "2026-05-04T12:00:00Z",
  "responseTimeMs": 15,
  "consecutiveFailures": 0,
  "failureThreshold": 3
}
```

### Readiness Probe

```
GET /api/health/ready
```

Returns 200 if gateway is ready to serve traffic, 503 otherwise.

**Response** (200 OK):
```json
{
  "ready": true,
  "reason": "All systems operational"
}
```

### Liveness Probe

```
GET /api/health/live
```

Returns 200 if gateway process is alive, 503 otherwise.

**Response** (200 OK):
```json
{
  "alive": true
}
```

---

## Metrics & Analytics

### Performance Metrics

```
GET /metrics/performance
```

**Query Parameters**:
- `timespan` (optional): "1h", "24h", "7d" (default: "24h")

**Response** (200 OK):
```json
{
  "throughputRequestsPerSecond": 125.5,
  "averageLatencyMs": 45.2,
  "p50LatencyMs": 35,
  "p95LatencyMs": 150,
  "p99LatencyMs": 350,
  "totalRequests": 10837500,
  "errorCount": 1204,
  "errorRate": 0.0111
}
```

### Request Metrics

```
GET /metrics/requests
```

**Response** (200 OK):
```json
{
  "totalRequests": 10837500,
  "successfulRequests": 10836296,
  "failedRequests": 1204,
  "averageResponseTimeMs": 45.2,
  "requestsPerSecond": 125.5,
  "topEndpoints": [
    {
      "endpoint": "user.UserService.GetUser",
      "count": 5000000,
      "avgLatencyMs": 35
    },
    {
      "endpoint": "product.ProductService.List",
      "count": 3000000,
      "avgLatencyMs": 50
    }
  ]
}
```

### Slow Requests

```
GET /metrics/slow
```

**Query Parameters**:
- `threshold` (optional): Latency threshold in ms (default: 1000)
- `limit` (optional): Max results (default: 100)

**Response** (200 OK):
```json
[
  {
    "requestId": "abc123",
    "endpoint": "user.UserService.GetUser",
    "latencyMs": 2500,
    "statusCode": 200,
    "timestamp": "2026-05-04T12:00:00Z"
  },
  {
    "requestId": "def456",
    "endpoint": "product.ProductService.Search",
    "latencyMs": 1800,
    "statusCode": 200,
    "timestamp": "2026-05-04T11:59:00Z"
  }
]
```

### Error Distribution

```
GET /metrics/errors
```

**Response** (200 OK):
```json
{
  "errors": [
    {
      "statusCode": 500,
      "count": 500,
      "percentage": 41.5
    },
    {
      "statusCode": 504,
      "count": 400,
      "percentage": 33.2
    },
    {
      "statusCode": 429,
      "count": 304,
      "percentage": 25.2
    }
  ],
  "totalErrors": 1204
}
```

### Top Endpoints

```
GET /metrics/endpoints
```

**Query Parameters**:
- `limit` (optional): Max results (default: 20)

**Response** (200 OK):
```json
[
  {
    "endpoint": "user.UserService.GetUser",
    "requestCount": 5000000,
    "successRate": 0.998,
    "avgLatencyMs": 35,
    "p99LatencyMs": 200
  },
  {
    "endpoint": "product.ProductService.List",
    "requestCount": 3000000,
    "successRate": 0.995,
    "avgLatencyMs": 50,
    "p99LatencyMs": 250
  }
]
```

### Reset Metrics

```
POST /metrics/reset
```

**Response** (200 OK):
```json
{
  "message": "Metrics reset successfully",
  "resetAt": "2026-05-04T12:00:00Z"
}
```

---

## Statistics

### Today's Statistics

```
GET /gateway/statistics/today
```

**Response** (200 OK):
```json
{
  "date": "2026-05-04",
  "totalRequests": 125000,
  "successfulRequests": 124000,
  "failedRequests": 1000,
  "errorRate": 0.008,
  "averageLatencyMs": 45.2,
  "p99LatencyMs": 350
}
```

### Statistics by Date

```
GET /gateway/statistics/{date}
```

**Path Parameters**:
- `date`: Date in YYYY-MM-DD format

**Response** (200 OK):
```json
{
  "date": "2026-05-03",
  "totalRequests": 150000,
  "successfulRequests": 148500,
  "failedRequests": 1500,
  "errorRate": 0.01,
  "averageLatencyMs": 50.0,
  "p99LatencyMs": 400
}
```

### Slow Requests

```
GET /gateway/metrics/slow-requests
```

**Query Parameters**:
- `threshold` (optional): Latency threshold (default: 1000ms)

**Response** (200 OK):
```json
{
  "slowRequests": [
    {
      "requestId": "abc123",
      "service": "user.UserService",
      "method": "GetUser",
      "latencyMs": 2500,
      "timestamp": "2026-05-04T12:00:00Z"
    }
  ],
  "count": 25,
  "averageLatencyMs": 1500
}
```

### Average Response Time

```
GET /gateway/metrics/average-response-time
```

**Query Parameters**:
- `days` (optional): Number of days to average (default: 7)

**Response** (200 OK):
```json
{
  "averageResponseTimeMs": 45.2,
  "periodDays": 7,
  "minResponseTimeMs": 10,
  "maxResponseTimeMs": 350,
  "stdDeviation": 32.5
}
```

---

## Service Discovery

### List Services

```
GET /servicediscovery/services
```

**Response** (200 OK):
```json
[
  {
    "id": 1,
    "name": "UserService",
    "serviceFullName": "user.UserService",
    "host": "api.example.com",
    "port": 5001,
    "useTls": true,
    "status": "Healthy"
  }
]
```

### Service Routes

```
GET /servicediscovery/services/{serviceId}/routes
```

**Response** (200 OK):
```json
[
  {
    "id": 1,
    "pattern": "user.UserService.*",
    "priority": 100,
    "rateLimitPerMinute": 1000
  }
]
```

### Route Matching

```
POST /servicediscovery/route-match
Content-Type: application/json
```

**Request**:
```json
{
  "serviceName": "user.UserService",
  "methodName": "GetUser"
}
```

**Response** (200 OK):
```json
{
  "routeId": 1,
  "pattern": "user.UserService.*",
  "targetServiceId": 1,
  "rateLimitPerMinute": 1000,
  "enableCaching": true,
  "cacheDurationSeconds": 300
}
```

### Route Conflicts

```
POST /servicediscovery/route-conflicts
Content-Type: application/json
```

**Request**:
```json
[
  {
    "pattern": "user.*",
    "priority": 100
  },
  {
    "pattern": "user.UserService.*",
    "priority": 50
  }
]
```

**Response** (200 OK):
```json
{
  "conflicts": [
    {
      "route1Pattern": "user.*",
      "route2Pattern": "user.UserService.*",
      "severity": "Warning",
      "description": "More specific pattern should have higher priority"
    }
  ],
  "hasConflicts": true
}
```

---

## Error Codes

| Code | HTTP Status | Description |
|------|-------------|-------------|
| INVALID_REQUEST | 400 | Invalid request parameters |
| SERVICE_NOT_FOUND | 404 | Service doesn't exist |
| ROUTE_NOT_FOUND | 404 | Route doesn't exist |
| UNAUTHORIZED | 401 | Missing or invalid authentication token |
| RATE_LIMIT_EXCEEDED | 429 | Too many requests |
| INTERNAL_ERROR | 500 | Gateway internal error |
| SERVICE_UNAVAILABLE | 503 | Service temporarily unavailable |
| OPERATION_TIMEOUT | 504 | Request timeout |

---

## Rate Limiting

API calls are rate-limited per IP address. Check response headers:

```
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1620129600
```

When exceeded, returns 429 Too Many Requests:

```json
{
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "Rate limit exceeded",
    "retryAfter": 60
  }
}
```

---

## Pagination

List endpoints support pagination:

**Query Parameters**:
- `limit`: Results per page (default: 100, max: 1000)
- `offset`: Skip N results (default: 0)

**Response Headers**:
- `X-Total-Count`: Total number of results
- `X-Page-Size`: Current page size
- `X-Page-Offset`: Current offset

```json
{
  "data": [...],
  "pagination": {
    "total": 5000,
    "limit": 100,
    "offset": 0,
    "hasMore": true
  }
}
```

---

## Versioning

Current API version: `v1`

Future versions will use `/api/v2`, `/api/v3`, etc.

---

## Support

For API issues or questions:

- 📧 Email: support@sarmkadan.com
- 🐛 Issues: [GitHub Issues](https://github.com/Sarmkadan/dotnet-grpc-gateway/issues)
- 💬 Discussions: [GitHub Discussions](https://github.com/Sarmkadan/dotnet-grpc-gateway/discussions)
