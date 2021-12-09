# dotnet-grpc-gateway

A minimal, production-ready gRPC-Web gateway for .NET — browser-compatible gRPC without Envoy or complex proxies.

> **Fast, lightweight, and built for scale.** Enable direct browser access to your gRPC backend services with a single .NET application.

---

## Table of Contents

- [Overview](#overview)
- [Why dotnet-grpc-gateway?](#why-dotnet-grpc-gateway)
- [Architecture](#architecture)
- [Features](#features)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Usage Examples](#usage-examples)
- [API Reference](#api-reference)
- [Configuration Reference](#configuration-reference)
- [Deployment](#deployment)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)
- [License](#license)

---

## Overview

**dotnet-grpc-gateway** is a lightweight gateway layer that bridges web clients and gRPC backend services. Instead of deploying complex infrastructure like Envoy proxies or multiple load balancers, you get a single .NET application that:

- Converts HTTP/1.1 REST requests to gRPC
- Handles gRPC-Web protocol translation
- Routes requests to backend gRPC services
- Monitors service health and availability
- Collects detailed metrics and performance data
- Enforces rate limits and authentication
- Caches responses intelligently

Built with modern .NET 10 and C# 14 features, it's designed for cloud-native deployments, Kubernetes, and traditional servers alike.

### Key Use Cases

1. **Legacy Web Frontend + gRPC Backend**: Modernize backend services while keeping existing web clients
2. **Microservices Aggregation**: Single entry point for multiple gRPC services
3. **API Monetization**: Apply rate limiting and authentication to gRPC APIs
4. **Hybrid Protocols**: Support both gRPC and REST clients from one gateway
5. **Service Mesh Alternatives**: Lightweight alternative to Istio/Linkerd for small-to-medium deployments

---

## Why dotnet-grpc-gateway?

| Feature | Envoy | Kong | dotnet-grpc-gateway |
|---------|-------|------|---------------------|
| **Memory** | 50-100MB | 30-50MB | 15-30MB |
| **Setup Time** | 30+ min | 20 min | 5 min |
| **gRPC-Web Support** | ✓ | ✗ (plugin) | ✓ |
| **Configuration as Code** | ✗ (YAML) | ✗ (YAML/UI) | ✓ (C#) |
| **Service Discovery** | Manual | Automatic | Automatic |
| **Built-in Health Checks** | Requires xDS | Plugin | ✓ |
| **Metrics Export** | gRPC | REST API | REST API |
| **Kubernetes Native** | ✓ | ✓ | ✓ |

---

## Architecture

### System Design

```
┌─────────────────────────────────────────────────────────────┐
│                    Web Clients (Browser)                     │
│              (HTTP/1.1 + gRPC-Web Protocol)                  │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       │ HTTP/1.1 REST
                       │
┌──────────────────────▼──────────────────────────────────────┐
│                  dotnet-grpc-gateway                          │
│  ┌────────────────────────────────────────────────────────┐  │
│  │  Request Pipeline                                      │  │
│  │  - CORS Middleware                                     │  │
│  │  - Authentication Middleware                           │  │
│  │  - Rate Limiting Middleware                            │  │
│  │  - Request Logging Middleware                          │  │
│  │  - gRPC-Web Protocol Handler                           │  │
│  └────────────────────────────────────────────────────────┘  │
│                                                               │
│  ┌────────────────────────────────────────────────────────┐  │
│  │  Core Services                                         │  │
│  │  - Route Resolution Service                            │  │
│  │  - Service Discovery Service                           │  │
│  │  - Metrics Collection Service                          │  │
│  │  - Cache Service                                       │  │
│  │  - Validation Service                                  │  │
│  └────────────────────────────────────────────────────────┘  │
│                                                               │
│  ┌────────────────────────────────────────────────────────┐  │
│  │  Infrastructure & Persistence                          │  │
│  │  - PostgreSQL Repository                               │  │
│  │  - Service Registry                                    │  │
│  │  - Route Storage                                       │  │
│  │  - Metrics Storage                                     │  │
│  │  - Health Check Status                                 │  │
│  └────────────────────────────────────────────────────────┘  │
└──────────────┬──────────────────────────┬───────────────────┘
               │                          │
          gRPC (port 5001-5010)     PostgreSQL (port 5432)
               │                          │
    ┌──────────▼────────────┐    ┌────────▼──────────┐
    │   Backend gRPC        │    │  Configuration &  │
    │   Services            │    │  Metrics Database │
    │ - UserService         │    │                   │
    │ - ProductService      │    └───────────────────┘
    │ - OrderService        │
    └───────────────────────┘
```

### Core Components

#### Domain Models (7 entities)
```
GatewayConfiguration     Main gateway configuration
GrpcService             Registered backend gRPC services
GatewayRoute            Request routing rules
RequestMetric           Individual request metrics
AuthenticationToken     API authentication tokens
ServiceHealthReport     Health check results
GatewayStatistics       Aggregated statistics
```

#### Business Services (8 services)
```
GatewayService                  Core gateway operations
ServiceDiscoveryService         Service discovery & health monitoring
RouteResolutionService          Request-to-service routing
RouteManagementService          Route validation & conflict detection
MetricsCollectionService        Metrics aggregation & analytics
RequestMetricsAnalyzerService   Pattern analysis & anomaly detection
ValidationService               Request & configuration validation
GrpcClientFactory               Downstream gRPC service clients
```

#### Infrastructure Layer
```
ErrorHandlingMiddleware         Global error handling & formatting
RequestLoggingMiddleware        Request/response logging
RateLimitingMiddleware          Token bucket rate limiting
AuthenticationMiddleware        Bearer token authentication
PerformanceMonitor              Latency percentiles (P50, P95, P99)
StructuredLogger                Consistent structured logging
RequestContext                  Async-safe request correlation IDs
```

#### Data Access Layer
```
IUnitOfWork                     Transaction management
IGatewayRepository              Configuration persistence
IServiceRegistry                Service registry management
IRouteRepository                Route management
IMetricsRepository              Metrics persistence
IConnectionStringProvider       Connection string management
```

---

## Features

### ✨ gRPC-Web Support
- Browser-compatible HTTP/1.1 translation
- Automatic protocol detection
- CORS handling
- Streaming support

### 🔍 Service Discovery
- Automatic service registration/deregistration
- Real-time health checks (30-second intervals)
- Service availability tracking
- Per-service health reports

### 🛣️ Request Routing
- Pattern-based routing with wildcards
- Priority-based matching
- Route conflict detection
- Dynamic route updates (no restart required)

### 📊 Metrics & Analytics
- Per-request metrics collection
- Latency percentiles (P50, P95, P99)
- Error rate tracking
- Request volume by endpoint
- Slow request detection
- 30-day retention

### 🛡️ Security
- Bearer token authentication
- Per-service authorization policies
- Rate limiting (requests per minute)
- IP-based rate limiting
- Request validation

### ⚡ Performance
- In-memory caching with TTL
- Cache statistics
- Response compression
- Connection pooling
- Configurable timeouts

### 📈 Monitoring
- Health check endpoints
- Readiness/liveness probes
- Performance metrics export
- Error distribution tracking
- Webhook integrations

---

## Installation

### Option 1: Docker Compose (Recommended)

```bash
# Clone the repository
git clone https://github.com/Sarmkadan/dotnet-grpc-gateway.git
cd dotnet-grpc-gateway

# Start with Docker Compose
docker-compose up -d

# Gateway will be available at http://localhost:5000
# PostgreSQL at localhost:5432
```

### Option 2: Manual Installation

**Prerequisites:**
- .NET 10 SDK or later
- PostgreSQL 14 or later
- Git

**Steps:**

```bash
# Clone repository
git clone https://github.com/Sarmkadan/dotnet-grpc-gateway.git
cd dotnet-grpc-gateway

# Restore dependencies
dotnet restore

# Build solution
dotnet build -c Release

# Update database
export CONNECTION_STRING="Host=localhost;Port=5432;Database=grpc_gateway;Username=postgres;Password=postgres"
dotnet run --project src/dotnet-grpc-gateway -- migrate

# Run the gateway
dotnet run --project src/dotnet-grpc-gateway -c Release
```

### Option 3: From NuGet Package

```bash
dotnet new console -n MyGateway
cd MyGateway
dotnet add package DotNetGrpcGateway
```

### Option 4: Kubernetes Deployment

See [docs/deployment.md](docs/deployment.md) for Kubernetes manifests.

---

## Quick Start

### 1. Start the Gateway

```bash
docker-compose up -d
```

### 2. Register a gRPC Service

```bash
curl -X POST http://localhost:5000/api/gateway/services \
  -H "Content-Type: application/json" \
  -d '{
    "name": "UserService",
    "serviceFullName": "user.UserService",
    "host": "localhost",
    "port": 5001,
    "useTls": false,
    "healthCheckIntervalSeconds": 30
  }'
```

### 3. Add a Route

```bash
curl -X POST http://localhost:5000/api/gateway/routes \
  -H "Content-Type: application/json" \
  -d '{
    "pattern": "user.UserService.*",
    "targetServiceId": 1,
    "matchType": 1,
    "priority": 100,
    "rateLimitPerMinute": 1000,
    "enableCaching": true,
    "cacheDurationSeconds": 300
  }'
```

### 4. Make a Request

```bash
# Direct REST API call
curl http://localhost:5000/user/GetUser?userId=123

# gRPC-Web call (from browser)
fetch('http://localhost:5000/user.UserService/GetUser', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/grpc-web+proto',
  },
  body: /* protobuf binary */
})
```

### 5. Check Health

```bash
curl http://localhost:5000/health
curl http://localhost:5000/api/health/status
curl http://localhost:5000/api/health/services
```

---

## Usage Examples

### Example 1: Basic Service Registration

```csharp
using DotNetGrpcGateway.Services;
using DotNetGrpcGateway.Domain;

public class ServiceSetup
{
    private readonly IGatewayService _gatewayService;

    public ServiceSetup(IGatewayService gatewayService)
    {
        _gatewayService = gatewayService;
    }

    public async Task RegisterUserServiceAsync()
    {
        var service = new GrpcService
        {
            Name = "UserService",
            ServiceFullName = "user.UserService",
            Host = "api.example.com",
            Port = 5001,
            UseTls = true,
            HealthCheckIntervalSeconds = 30
        };

        var result = await _gatewayService.RegisterServiceAsync(service);
        Console.WriteLine($"Service registered with ID: {result.Id}");
    }
}
```

### Example 2: Dynamic Route Configuration

```csharp
using DotNetGrpcGateway.Services;
using DotNetGrpcGateway.Domain;

public class RouteSetup
{
    private readonly IGatewayService _gatewayService;

    public RouteSetup(IGatewayService gatewayService)
    {
        _gatewayService = gatewayService;
    }

    public async Task ConfigureRoutesAsync(int serviceId)
    {
        // High-priority read operations with caching
        var listRoute = new GatewayRoute
        {
            Pattern = "user.UserService.List*",
            TargetServiceId = serviceId,
            MatchType = RouteMatchType.Prefix,
            Priority = 100,
            RateLimitPerMinute = 5000,
            EnableCaching = true,
            CacheDurationSeconds = 300
        };

        // Medium-priority write operations (no cache)
        var updateRoute = new GatewayRoute
        {
            Pattern = "user.UserService.Update*",
            TargetServiceId = serviceId,
            MatchType = RouteMatchType.Prefix,
            Priority = 50,
            RateLimitPerMinute = 1000,
            EnableCaching = false
        };

        await _gatewayService.AddRouteAsync(listRoute);
        await _gatewayService.AddRouteAsync(updateRoute);
    }
}
```

### Example 3: Health Monitoring

```csharp
using DotNetGrpcGateway.Services;

public class HealthMonitoring
{
    private readonly IServiceDiscoveryService _discoveryService;

    public HealthMonitoring(IServiceDiscoveryService discoveryService)
    {
        _discoveryService = discoveryService;
    }

    public async Task CheckAllServicesAsync()
    {
        // Get health of all services
        var healthMap = await _discoveryService.GetAllServicesHealthAsync();
        
        foreach (var (serviceId, isHealthy) in healthMap)
        {
            Console.WriteLine($"Service {serviceId}: {(isHealthy ? "Healthy" : "Unhealthy")}");
        }

        // Perform single health check
        var report = await _discoveryService.PerformHealthCheckAsync(1);
        Console.WriteLine($"Status: {report.Status}");
        Console.WriteLine($"Response Time: {report.ResponseTimeMs}ms");
    }
}
```

### Example 4: Metrics Collection

```csharp
using DotNetGrpcGateway.Services;

public class MetricsAnalysis
{
    private readonly IMetricsCollectionService _metricsService;

    public MetricsAnalysis(IMetricsCollectionService metricsService)
    {
        _metricsService = metricsService;
    }

    public async Task AnalyzeTodayPerformanceAsync()
    {
        // Daily statistics
        var todayStats = await _metricsService.GetTodayStatisticsAsync();
        Console.WriteLine($"Total Requests: {todayStats.TotalRequests}");
        Console.WriteLine($"Error Rate: {todayStats.ErrorRate:P2}");

        // Slow requests (>1000ms)
        var slowRequests = await _metricsService.GetSlowRequestsAsync(1000);
        Console.WriteLine($"Slow Requests: {slowRequests.Count}");

        // Average response time
        var avgTime = await _metricsService.GetAverageResponseTimeAsync();
        Console.WriteLine($"Average Response Time: {avgTime}ms");
    }
}
```

### Example 5: Authentication & Rate Limiting

```csharp
using DotNetGrpcGateway.Services;
using DotNetGrpcGateway.Domain;

public class SecuritySetup
{
    private readonly IGatewayService _gatewayService;

    public SecuritySetup(IGatewayService gatewayService)
    {
        _gatewayService = gatewayService;
    }

    public async Task SetupAuthenticationAsync()
    {
        // Create authentication token
        var token = new AuthenticationToken
        {
            Token = "secret-api-key-12345",
            Description = "API Client Token",
            ExpiresAt = DateTime.UtcNow.AddYears(1)
        };

        await _gatewayService.CreateAuthTokenAsync(token);

        // Configure protected route
        var protectedRoute = new GatewayRoute
        {
            Pattern = "admin.*",
            TargetServiceId = 1,
            MatchType = RouteMatchType.Prefix,
            Priority = 200,
            RequiresAuthentication = true,
            RateLimitPerMinute = 100
        };

        await _gatewayService.AddRouteAsync(protectedRoute);
    }
}
```

### Example 6: REST API Integration

```csharp
// Get current gateway configuration
GET /api/gateway/configuration
Response: {
  "id": 1,
  "port": 5000,
  "maxConcurrentConnections": 1000,
  "requestTimeoutMs": 30000
}

// List all registered services
GET /api/gateway/services
Response: [{
  "id": 1,
  "name": "UserService",
  "host": "localhost",
  "port": 5001,
  "isHealthy": true
}]

// Get performance metrics
GET /api/metrics/performance
Response: {
  "throughputRequestsPerSecond": 125.5,
  "averageLatencyMs": 45.2,
  "p50LatencyMs": 35,
  "p95LatencyMs": 150,
  "p99LatencyMs": 350
}
```

### Example 7: Webhook Integration

```csharp
using DotNetGrpcGateway.Integration;
using DotNetGrpcGateway.Domain;

public class WebhookSetup
{
    private readonly IWebhookService _webhookService;

    public WebhookSetup(IWebhookService webhookService)
    {
        _webhookService = webhookService;
    }

    public async Task SendHealthAlertAsync(string serviceId)
    {
        await _webhookService.SendWebhookAsync(
            url: "https://alerts.example.com/health-check",
            payload: new
            {
                serviceId,
                event = "health_check_failed",
                timestamp = DateTime.UtcNow
            },
            retries: 3
        );
    }
}
```

### Example 8: Route Conflict Detection

```csharp
using DotNetGrpcGateway.Services;

public class RouteConflictDetection
{
    private readonly IRouteManagementService _routeService;

    public RouteConflictDetection(IRouteManagementService routeService)
    {
        _routeService = routeService;
    }

    public async Task DetectConflictsAsync()
    {
        var routes = new[]
        {
            new GatewayRoute { Pattern = "user.*", Priority = 100 },
            new GatewayRoute { Pattern = "user.UserService.*", Priority = 50 }
        };

        var conflicts = await _routeService.DetectConflictsAsync(routes);
        
        foreach (var conflict in conflicts)
        {
            Console.WriteLine($"Conflict: {conflict.Route1.Pattern} vs {conflict.Route2.Pattern}");
        }
    }
}
```

### Example 9: Caching Strategy

```csharp
using DotNetGrpcGateway.Services;
using DotNetGrpcGateway.Domain;

public class CachingStrategy
{
    private readonly ICacheService _cacheService;

    public CachingStrategy(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<string?> GetCachedResponseAsync(string key)
    {
        // Try to get from cache
        if (_cacheService.TryGetValue(key, out var cachedValue))
        {
            return cachedValue;
        }

        // Cache miss - would fetch from backend
        var response = await FetchFromBackendAsync(key);
        
        // Store in cache for 5 minutes
        _cacheService.Set(key, response, TimeSpan.FromMinutes(5));
        
        return response;
    }

    public void DisplayCacheStats()
    {
        var stats = _cacheService.GetCacheStatistics();
        Console.WriteLine($"Cache Hits: {stats.Hits}");
        Console.WriteLine($"Cache Misses: {stats.Misses}");
        Console.WriteLine($"Hit Rate: {stats.HitRate:P2}");
    }

    private async Task<string> FetchFromBackendAsync(string key)
    {
        // Simulated backend call
        await Task.Delay(100);
        return $"Response for {key}";
    }
}
```

### Example 10: Custom Middleware

```csharp
using DotNetGrpcGateway.Infrastructure;

public class CustomMetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CustomMetricsMiddleware> _logger;

    public CustomMetricsMiddleware(RequestDelegate next, ILogger<CustomMetricsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IPerformanceMonitor monitor)
    {
        var startTime = DateTime.UtcNow;
        var originalBody = context.Response.Body;

        try
        {
            using (var memoryStream = new MemoryStream())
            {
                context.Response.Body = memoryStream;
                await _next(context);

                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation(
                    "Request {Path} completed in {Duration}ms",
                    context.Request.Path,
                    duration.TotalMilliseconds
                );

                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(originalBody);
            }
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }
}
```

---

## API Reference

### Gateway Configuration Endpoints

```
GET /api/gateway/configuration
  Get current gateway configuration
  Response: { id, port, maxConcurrentConnections, requestTimeoutMs }

PUT /api/gateway/configuration
  Update gateway configuration
  Body: { port?, maxConcurrentConnections?, requestTimeoutMs? }
```

### Service Management Endpoints

```
GET /api/gateway/services
  List all registered services
  
GET /api/gateway/services/healthy
  List only healthy services
  
GET /api/gateway/services/{id}
  Get specific service details
  
POST /api/gateway/services
  Register new service
  Body: { name, serviceFullName, host, port, useTls, healthCheckIntervalSeconds }
  
DELETE /api/gateway/services/{id}
  Unregister service
```

### Route Management Endpoints

```
GET /api/gateway/routes
  List all routes
  
POST /api/gateway/routes
  Add new route
  Body: { pattern, targetServiceId, matchType, priority, rateLimitPerMinute, enableCaching, cacheDurationSeconds }
  
DELETE /api/gateway/routes/{id}
  Remove route
```

### Metrics Endpoints

```
GET /api/metrics/performance
  Latency percentiles (P50, P95, P99) and throughput
  
GET /api/metrics/requests
  Request counts and patterns by endpoint
  
GET /api/metrics/slow
  Requests exceeding latency threshold
  
GET /api/metrics/errors
  Error distribution by status code
  
GET /api/metrics/endpoints
  Top endpoints by usage volume
  
POST /api/metrics/reset
  Reset all metrics
```

### Health & Readiness Endpoints

```
GET /health
  Overall gateway health status
  
GET /api/health/status
  Detailed gateway health
  
GET /api/health/services
  Per-service health details
  
GET /api/health/services/{id}
  Specific service health
  
GET /api/health/ready
  Readiness probe (for load balancers)
  
GET /api/health/live
  Liveness probe (for Kubernetes)
```

### Service Discovery Endpoints

```
GET /api/servicediscovery/services
  All registered services with metadata
  
GET /api/servicediscovery/services/{id}/routes
  Routes targeting specific service
  
POST /api/servicediscovery/route-match
  Find matching route for request
  Body: { serviceName, methodName }
  
POST /api/servicediscovery/route-conflicts
  Detect conflicts in route definitions
  Body: [ { pattern, priority }, ... ]
```

### Statistics Endpoints

```
GET /api/gateway/statistics/today
  Today's statistics
  
GET /api/gateway/statistics/{date}
  Statistics for specific date (YYYY-MM-DD)
  
GET /api/gateway/metrics/slow-requests
  Requests exceeding 1000ms latency
  
GET /api/gateway/metrics/average-response-time
  Average response time across all requests
```

---

## Configuration Reference

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=grpc_gateway;Username=postgres;Password=postgres"
  },
  "Gateway": {
    "ListenAddress": "0.0.0.0",
    "Port": 5000,
    "EnableReflection": true,
    "EnableMetrics": true,
    "MaxConcurrentConnections": 1000,
    "RequestTimeoutMs": 30000,
    "LogLevel": "Information",
    "HealthCheck": {
      "IntervalSeconds": 30,
      "TimeoutMs": 5000,
      "FailureThreshold": 3
    },
    "Metrics": {
      "EnableMetrics": true,
      "CollectionIntervalSeconds": 60,
      "RetentionDays": 30
    }
  }
}
```

### Environment Variables

```
ASPNETCORE_ENVIRONMENT          Development/Production
ASPNETCORE_URLS                 http://0.0.0.0:5000
Gateway__Port                   5000
Gateway__MaxConcurrentConnections   1000
Gateway__RequestTimeoutMs       30000
ConnectionStrings__DefaultConnection   PostgreSQL connection string
```

---

## Deployment

### Docker Compose (Recommended for Development)

See `docker-compose.yml` for full configuration.

### Kubernetes

See [docs/deployment.md](docs/deployment.md) for Kubernetes manifests, Helm charts, and scaling guidelines.

### Environment-Specific Configuration

```bash
# Development
dotnet run --project src/dotnet-grpc-gateway

# Production
dotnet run --project src/dotnet-grpc-gateway -c Release

# With custom appsettings
ASPNETCORE_ENVIRONMENT=Production dotnet run --project src/dotnet-grpc-gateway
```

---

## Troubleshooting

### Service Not Found (502)

**Problem**: Registered service returns 502 Bad Gateway

**Solutions**:
1. Verify service is healthy: `curl http://localhost:5000/api/health/services`
2. Check service host/port configuration
3. Ensure gRPC service is running and listening
4. Check firewall rules

```bash
# Test direct gRPC connection
grpcurl -plaintext localhost:5001 list
```

### High Memory Usage

**Problem**: Gateway consuming excessive memory

**Solutions**:
1. Reduce cache size: Lower `Metrics.RetentionDays` from 30 to 7
2. Reduce metrics collection interval
3. Reduce `MaxConcurrentConnections`
4. Use response compression

```json
{
  "Gateway": {
    "MaxConcurrentConnections": 500,
    "Metrics": {
      "RetentionDays": 7,
      "CollectionIntervalSeconds": 120
    }
  }
}
```

### Slow Response Times

**Problem**: Requests taking >1000ms

**Solutions**:
1. Check `GET /api/metrics/slow` for problematic endpoints
2. Verify backend service performance
3. Enable caching for read-heavy operations
4. Increase timeouts if needed
5. Check database connection pooling

```bash
curl http://localhost:5000/api/metrics/slow?threshold=500
```

### Rate Limiting Not Working

**Problem**: Requests exceeding limits still succeed

**Solutions**:
1. Verify `RateLimitPerMinute` is set on route
2. Check rate limiting middleware is enabled in Program.cs
3. Clear cache: `POST /api/metrics/reset`
4. Check client IP (X-Forwarded-For header)

### Database Connection Issues

**Problem**: "Connection string not found" error

**Solutions**:
1. Set `DefaultConnection` in appsettings.json
2. Or set environment variable: `ConnectionStrings__DefaultConnection=...`
3. Verify PostgreSQL is running
4. Check credentials and network access

```bash
# Test PostgreSQL connection
psql -h localhost -U postgres -d grpc_gateway
```

### gRPC-Web Protocol Errors

**Problem**: Browser requests fail with protocol errors

**Solutions**:
1. Enable CORS: Verify `GrpcWebPolicy` is configured
2. Check gRPC-Web middleware is added: `app.UseGrpcWeb();`
3. Verify `Content-Type: application/grpc-web`
4. Check browser console for CORS errors

---

## Contributing

Contributions are welcome! This is an open-source project, and we value community involvement.

### Getting Started

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/my-feature`
3. Make your changes
4. Write tests for new functionality
5. Ensure all tests pass: `dotnet test`
6. Submit a pull request

### Code Standards

- Follow C# naming conventions (PascalCase for classes/methods, camelCase for properties)
- Add XML documentation comments to public members
- Keep methods under 50 lines
- Include unit tests for business logic
- Use dependency injection for testability

### Running Tests

```bash
dotnet test
dotnet test --coverage
```

### Pull Request Process

1. Update CHANGELOG.md with your changes
2. Ensure CI/CD pipeline passes
3. Add descriptive PR title and description
4. Link related issues
5. Request review from maintainers

---

## License

MIT License - Copyright (c) 2026 Vladyslav Zaiets

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

See [LICENSE](LICENSE) file for full details.

---

**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**

[Portfolio](https://sarmkadan.com) | [GitHub](https://github.com/Sarmkadan) | [Telegram](https://t.me/sarmkadan)
