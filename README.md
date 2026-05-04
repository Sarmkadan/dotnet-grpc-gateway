# dotnet-grpc-gateway

A minimal gRPC-Web gateway for .NET - browser-compatible gRPC without Envoy or complex proxies.

## Overview

dotnet-grpc-gateway is a lightweight, production-ready gateway that bridges the gap between web clients and gRPC backend services. It provides:

- **gRPC-Web Support**: Enable direct browser access to gRPC services
- **Service Discovery**: Automatic service registration and health checking
- **Request Routing**: Pattern-based routing with priority-based matching
- **Metrics & Analytics**: Built-in request metrics, performance tracking, and statistics
- **Health Monitoring**: Continuous health checks with detailed reporting
- **Authentication & Authorization**: Token-based access control with per-service policies
- **Rate Limiting**: Configurable rate limits per route
- **Request Caching**: Intelligent caching strategies for improved performance
- **Error Handling**: Comprehensive error handling with structured error responses

## Architecture

### Core Components

**Domain Models** (7 entities)
- `GatewayConfiguration`: Main gateway configuration
- `GrpcService`: Registered backend gRPC services
- `GatewayRoute`: Request routing rules
- `RequestMetric`: Individual request metrics
- `AuthenticationToken`: API authentication tokens
- `ServiceHealthReport`: Health check results
- `GatewayStatistics`: Aggregated statistics

**Services** (5 business logic services)
- `GatewayService`: Core gateway operations
- `ServiceDiscoveryService`: Service discovery and health monitoring
- `RouteResolutionService`: Request-to-service routing
- `MetricsCollectionService`: Metrics aggregation and analytics
- `ValidationService`: Request and configuration validation

**Data Access**
- `IUnitOfWork`: Transaction management
- `IGatewayRepository`: Gateway configuration persistence
- `IServiceRegistry`: Service registry management
- `IRouteRepository`: Route management
- `IMetricsRepository`: Metrics persistence

**Infrastructure**
- `ErrorHandlingMiddleware`: Global error handling
- `GrpcClientFactory`: Downstream gRPC service clients
- Health checks and monitoring

## Features

### Service Registration
```csharp
var service = new GrpcService
{
    Name = "UserService",
    ServiceFullName = "user.UserService",
    Host = "api.example.com",
    Port = 5001,
    UseTls = true,
    HealthCheckIntervalSeconds = 30
};

await gatewayService.RegisterServiceAsync(service);
```

### Route Configuration
```csharp
var route = new GatewayRoute
{
    Pattern = "user.UserService.*",
    TargetServiceId = 1,
    MatchType = RouteMatchType.Prefix,
    Priority = 100,
    RateLimitPerMinute = 1000,
    EnableCaching = true,
    CacheDurationSeconds = 60
};

await gatewayService.AddRouteAsync(route);
```

### Health Monitoring
```csharp
var report = await discoveryService.PerformHealthCheckAsync(serviceId);
var health = await discoveryService.GetAllServicesHealthAsync();
```

### Metrics & Analytics
```csharp
var todayStats = await metricsService.GetTodayStatisticsAsync();
var slowRequests = await metricsService.GetSlowRequestsAsync(1000);
var avgResponseTime = await metricsService.GetAverageResponseTimeAsync();
```

## REST API Endpoints

### Gateway Configuration
- `GET /api/gateway/configuration` - Get current configuration
- `PUT /api/gateway/configuration` - Update configuration

### Service Management
- `GET /api/gateway/services` - List all services
- `GET /api/gateway/services/healthy` - List healthy services
- `GET /api/gateway/services/{id}` - Get service details
- `POST /api/gateway/services` - Register a service
- `DELETE /api/gateway/services/{id}` - Unregister a service

### Route Management
- `GET /api/gateway/routes` - List all routes
- `POST /api/gateway/routes` - Add a route
- `DELETE /api/gateway/routes/{id}` - Remove a route

### Metrics
- `GET /api/gateway/statistics/today` - Today's statistics
- `GET /api/gateway/statistics/{date}` - Statistics for a date
- `GET /api/gateway/metrics/slow-requests` - Slow requests
- `GET /api/gateway/metrics/average-response-time` - Average response time

### Health Check
- `GET /health` - Overall health status

## Configuration

Edit `appsettings.json` to configure the gateway:

```json
{
  "Gateway": {
    "ListenAddress": "0.0.0.0",
    "Port": 5000,
    "EnableReflection": true,
    "EnableMetrics": true,
    "MaxConcurrentConnections": 1000,
    "RequestTimeoutMs": 30000,
    "HealthCheck": {
      "IntervalSeconds": 30,
      "TimeoutMs": 5000,
      "FailureThreshold": 3
    }
  }
}
```

## Building & Running

### Prerequisites
- .NET 10 SDK or later
- Visual Studio Code or Visual Studio

### Build
```bash
dotnet build dotnet-grpc-gateway.sln
```

### Run
```bash
dotnet run --project src/dotnet-grpc-gateway/dotnet-grpc-gateway.csproj
```

The gateway will start on `http://0.0.0.0:5000`

## Error Handling

The gateway returns structured error responses:

```json
{
  "requestId": "0HN8HJRE6HFBC:00000001",
  "timestamp": "2026-05-04T12:00:00Z",
  "message": "Service not found",
  "errorCode": "SERVICE_NOT_FOUND",
  "details": {
    "service_name": "UserService"
  }
}
```

## Development

The codebase is organized into clear layers:

```
src/dotnet-grpc-gateway/
├── Domain/              # Entity models
├── Services/            # Business logic
├── Infrastructure/      # Data access & middleware
├── Configuration/       # DI setup
├── Controllers/         # REST API
├── Constants/           # Enums and constants
├── Exceptions/          # Custom exceptions
└── Program.cs           # Entry point
```

## License

MIT License - Copyright (c) 2026 Vladyslav Zaiets

See LICENSE file for details.

## Author

**Vladyslav Zaiets**  
CTO & Software Architect  
https://sarmkadan.com
