# ServiceDiscoveryController

REST API controller for service discovery operations in the dotnet-grpc-gateway project. Provides endpoints for service registration, discovery, route matching, and dynamic configuration management.

## Endpoints

### Get All Services
```
GET /api/servicediscovery/services
```

Retrieves all registered services with their metadata.

**Response:**
- `200 OK` - Returns `List<ServiceInfo>` containing service details
- `500 Internal Server Error` - If an error occurs during service retrieval

**Response Body (ServiceInfo):**
```json
{
  "id": 0,
  "name": "string",
  "serviceFullName": "string",
  "host": "string",
  "port": 0,
  "useTls": true,
  "isActive": true
}
```

### Get Service Routes
```
GET /api/servicediscovery/services/{serviceId}/routes
```

Retrieves all routes configured for a specific service.

**Parameters:**
- `serviceId` (path, integer) - The ID of the service

**Response:**
- `200 OK` - Returns `List<GatewayRoute>` containing route details
- `404 Not Found` - If the service is not found
- `500 Internal Server Error` - If an error occurs during route retrieval

### Find Matching Route
```
POST /api/servicediscovery/route-match
```

Finds the matching route for a given request path.

**Request Body:**
```json
{
  "path": "string"
}
```

**Response:**
- `200 OK` - Returns `RouteMatchResult` with matching route details
- `400 Bad Request` - If path is null or empty
- `404 Not Found` - If no matching route is found
- `500 Internal Server Error` - If an error occurs during route matching

**Response Body (RouteMatchResult):**
```json
{
  "routeId": 0,
  "pattern": "string",
  "serviceId": 0,
  "serviceName": "string",
  "priority": 0
}
```

### Get Conflicting Routes
```
POST /api/servicediscovery/route-conflicts
```

Retrieves routes that might conflict with a given pattern.

**Request Body:**
```json
{
  "pattern": "string"
}
```

**Response:**
- `200 OK` - Returns `List<GatewayRoute>` containing conflicting routes
- `400 Bad Request` - If pattern is null or empty
- `500 Internal Server Error` - If an error occurs during conflict detection

## Data Models

### ServiceInfo
Contains metadata about a registered service:
- `id`: Unique service identifier
- `name`: Service name
- `serviceFullName`: Fully qualified service name
- `host`: Service host address
- `port`: Service port number
- `useTls`: Whether TLS is used for communication
- `isActive`: Whether the service is currently active

### RouteMatchRequest
Request model for finding matching routes:
- `path`: The request path to match against registered routes

### RouteMatchResult
Result of a route matching operation:
- `routeId`: ID of the matched route
- `pattern`: URL pattern of the matched route
- `serviceId`: ID of the target service
- `serviceName`: Name of the target service
- `priority`: Priority level of the matched route

### RoutePatternRequest
Request model for checking route conflicts:
- `pattern`: URL pattern to check for conflicts

## Error Handling
All endpoints follow consistent error handling:
- Validation errors return `400 Bad Request` with descriptive messages
- Not found conditions return `404 Not Found`
- Unexpected errors return `500 Internal Server Error` and are logged
- All responses use JSON content type

## Dependencies
The controller depends on several services injected via constructor:
- `IGatewayService`: Core gateway operations
- `IServiceDiscoveryService`: Service discovery functionality
- `IRouteManagementService`: Route management operations
- `ILogger<ServiceDiscoveryController>`: Logging functionality