# Architecture Guide

This document describes the architecture and design patterns used in dotnet-grpc-gateway.

## System Overview

```
┌─────────────────────────────────────────────────────────┐
│              Client Applications                         │
│  (Web Browsers, Native Apps, SPAs)                       │
└────────────────────┬────────────────────────────────────┘
                     │ HTTP/1.1 + gRPC-Web
                     │
        ┌────────────▼──────────────┐
        │   Load Balancer / Proxy   │ (Optional)
        └────────────┬──────────────┘
                     │
    ┌────────────────▼────────────────────────────────────┐
    │         dotnet-grpc-gateway                          │
    │  ┌──────────────────────────────────────────────┐   │
    │  │  HTTP Request Pipeline                       │   │
    │  │  1. CORS Middleware                          │   │
    │  │  2. Authentication Middleware                │   │
    │  │  3. Request Logging Middleware               │   │
    │  │  4. Rate Limiting Middleware                 │   │
    │  │  5. Error Handling Middleware                │   │
    │  └──────────────────────────────────────────────┘   │
    │                                                      │
    │  ┌──────────────────────────────────────────────┐   │
    │  │  Route Resolution & Service Discovery        │   │
    │  │  - Pattern Matching (Prefix, Exact, Regex)   │   │
    │  │  - Priority-based Selection                  │   │
    │  │  - Health Status Checking                    │   │
    │  └──────────────────────────────────────────────┘   │
    │                                                      │
    │  ┌──────────────────────────────────────────────┐   │
    │  │  gRPC Protocol Translation Layer              │   │
    │  │  - gRPC-Web Protocol Handler                 │   │
    │  │  - Message Serialization (Protobuf)         │   │
    │  │  - Compression/Decompression                │   │
    │  └──────────────────────────────────────────────┘   │
    │                                                      │
    │  ┌──────────────────────────────────────────────┐   │
    │  │  Business Logic & Services                    │   │
    │  │  - Metrics Collection                         │   │
    │  │  - Cache Management                           │   │
    │  │  - Webhook Integration                        │   │
    │  │  - Performance Monitoring                     │   │
    │  └──────────────────────────────────────────────┘   │
    │                                                      │
    │  ┌──────────────────────────────────────────────┐   │
    │  │  Event System (Pub-Sub)                      │   │
    │  │  - Service Registration Events               │   │
    │  │  - Health Status Events                       │   │
    │  │  - Route Configuration Events                │   │
    │  │  - Performance Anomaly Events                │   │
    │  └──────────────────────────────────────────────┘   │
    │                                                      │
    │  ┌──────────────────────────────────────────────┐   │
    │  │  Background Services                          │   │
    │  │  - Health Check Service (every 30s)          │   │
    │  │  - Metrics Aggregation (every 60s)           │   │
    │  │  - Cache Expiration Monitor                  │   │
    │  │  - Service Cleanup                           │   │
    │  └──────────────────────────────────────────────┘   │
    │                                                      │
    └──────────────┬──────────────────────┬───────────────┘
                   │                      │
        gRPC (5001-5010)            PostgreSQL (5432)
                   │                      │
     ┌─────────────▼───┐      ┌──────────▼──────────┐
     │  Backend gRPC   │      │  Configuration DB   │
     │  Services       │      │  & Metrics          │
     │  - User Service │      │                     │
     │  - Order Service│      │  - Routes           │
     │  - Product Svc  │      │  - Services         │
     │  - Custom Svc   │      │  - Auth Tokens      │
     └─────────────────┘      │  - Metrics History  │
                               │  - Health Status    │
                               └─────────────────────┘
```

## Layer Architecture

### 1. **Presentation Layer** (Controllers)

Handles HTTP requests and responses. Three main controllers:

- **GatewayController**: Core gateway operations (services, routes, config)
- **MetricsController**: Performance statistics and analytics
- **ServiceDiscoveryController**: Service metadata and route matching
- **HealthController**: Health and readiness probes

**Responsibilities**:
- Route HTTP requests to services
- Validate input parameters
- Format responses (JSON, CSV, XML)
- Handle HTTP status codes

### 2. **Application/Business Logic Layer** (Services)

Implements core business logic with dependency injection:

```
IGatewayService
├── RegisterServiceAsync()
├── UnregisterServiceAsync()
├── AddRouteAsync()
├── RemoveRouteAsync()
└── GetConfigurationAsync()

IServiceDiscoveryService
├── PerformHealthCheckAsync()
├── GetAllServicesHealthAsync()
└── MonitorServiceHealthAsync()

IRouteResolutionService
├── ResolveRouteAsync()
└── FindMatchingRouteAsync()

IMetricsCollectionService
├── GetTodayStatisticsAsync()
├── GetSlowRequestsAsync()
└── GetAverageResponseTimeAsync()

ICacheService
├── Set()
├── TryGetValue()
├── Remove()
└── GetCacheStatistics()

IValidationService
├── ValidateServiceAsync()
├── ValidateRouteAsync()
└── ValidateAuthTokenAsync()
```

### 3. **Domain Layer** (Models)

Core business entities with business logic:

```csharp
GatewayConfiguration     // Gateway settings
GrpcService             // Registered backend service
GatewayRoute            // Routing rule
RequestMetric           // Single request measurement
AuthenticationToken     // API access token
ServiceHealthReport     // Health check result
GatewayStatistics       // Aggregated daily stats
```

### 4. **Data Access Layer** (Repositories)

Repository pattern for data persistence:

```csharp
IGatewayRepository      // Gateway configuration storage
IServiceRegistry        // Service management
IRouteRepository        // Route storage
IMetricsRepository      // Metrics persistence
IUnitOfWork             // Transaction management
```

**Implementation**: PostgreSQL with Entity Framework Core

### 5. **Infrastructure Layer**

#### Middleware Pipeline

```
1. CORS Middleware              // Cross-origin requests
   ├── AllowAnyOrigin()
   ├── AllowAnyMethod()
   └── AllowAnyHeader()

2. Request Logging Middleware   // Log all requests/responses
   ├── Log request metadata
   ├── Measure latency
   └── Capture response status

3. Authentication Middleware    // Verify API tokens
   ├── Extract bearer token
   ├── Validate token
   └── Set user context

4. Rate Limiting Middleware     // Token bucket algorithm
   ├── Track client IP
   ├── Increment counter
   └── Return 429 if exceeded

5. Error Handling Middleware    // Catch exceptions
   ├── Format error response
   ├── Log stack trace
   └── Return 500 status

6. gRPC-Web Handler             // Protocol translation
   ├── Parse gRPC-Web request
   ├── Forward to gRPC service
   └── Convert response back
```

#### Support Services

**PerformanceMonitor**: Real-time latency tracking
- Maintains circular buffers for P50, P95, P99
- Updates every 1 second
- Minimal overhead

**StructuredLogger**: Consistent logging
- Serilog integration
- Structured key-value pairs
- Daily log rotation

**RequestContext**: Async-safe request correlation
- Correlation ID tracking
- User context propagation
- Scoped to request

## Design Patterns

### 1. **Dependency Injection**

All services use constructor injection:

```csharp
public class GatewayController : ControllerBase
{
    private readonly IGatewayService _service;
    
    public GatewayController(IGatewayService service)
    {
        _service = service;
    }
}
```

**Benefits**: Testability, loose coupling, easy mocking

### 2. **Repository Pattern**

Abstracts data access:

```csharp
public interface IGatewayRepository
{
    Task<GatewayService> GetByIdAsync(int id);
    Task<List<GatewayService>> GetAllAsync();
    Task AddAsync(GatewayService service);
    Task UpdateAsync(GatewayService service);
    Task DeleteAsync(int id);
}
```

**Benefits**: Database independence, easier testing, clean separation

### 3. **Unit of Work Pattern**

Manages transactions:

```csharp
public interface IUnitOfWork
{
    IGatewayRepository Gateway { get; }
    IServiceRegistry Services { get; }
    IRouteRepository Routes { get; }
    Task<int> SaveChangesAsync();
}
```

**Benefits**: Consistent transactions, atomicity

### 4. **Factory Pattern**

GrpcClientFactory creates gRPC clients:

```csharp
public interface IGrpcClientFactory
{
    TClient CreateClient<TClient>(GrpcService service) 
        where TClient : class;
}
```

**Benefits**: Centralized client creation, caching, pooling

### 5. **Observer Pattern (Events)**

EventPublisher implements pub-sub:

```csharp
public interface IEventPublisher
{
    void Publish<T>(T @event) where T : GatewayEvent;
    void Subscribe<T>(IEventHandler<T> handler) where T : GatewayEvent;
}
```

**Benefits**: Loose coupling, asynchronous notifications

### 6. **Strategy Pattern**

Route matching strategies:

```csharp
public enum RouteMatchType
{
    Exact,      // Exact string match
    Prefix,     // Wildcard prefix match
    Regex       // Regular expression match
}
```

**Benefits**: Flexible matching, easy extension

### 7. **Decorator Pattern**

Middleware wraps request processing:

```csharp
app.UseMiddleware<RequestLoggingMiddleware>();  // Logs
app.UseMiddleware<RateLimitingMiddleware>();    // Rate limits
app.UseMiddleware<AuthenticationMiddleware>();  // Auth
```

**Benefits**: Composable functionality, separation of concerns

## Data Model

### Service Registration Flow

```
RegisterService
    ↓
Validate Service
    ↓
Insert into Database
    ↓
Publish ServiceRegisteredEvent
    ↓
Start Health Check Timer
    ↓
Return Service ID
```

### Request Processing Flow

```
HTTP Request
    ↓
CORS Check
    ↓
Authentication
    ↓
Rate Limit Check
    ↓
Route Matching
    ↓
Check Cache
    ├─ Hit → Return cached response
    └─ Miss → Forward to gRPC Service
    ↓
gRPC Call
    ↓
Record Metrics
    ↓
Update Cache (if enabled)
    ↓
Return Response
```

### Health Check Flow

```
HealthCheckBackgroundService
    ├─ Every 30 seconds
    ├─ For each registered service
    ├─ Send gRPC health check
    ├─ Update service status
    ├─ Publish HealthCheckEvent
    └─ If failed 3x → Unregister service
```

## Performance Considerations

### Memory Optimization

- **Circular Buffers**: P50/P95/P99 use fixed-size buffers
- **Object Pooling**: Request/response objects reused
- **Streaming**: Large responses streamed, not buffered
- **Cache Limits**: Configurable cache size with LRU eviction

### CPU Optimization

- **Async/Await**: Non-blocking request handling
- **SIMD**: Protobuf serialization uses optimized libraries
- **Lazy Loading**: Services loaded on-demand
- **Compiled Regex**: Route patterns pre-compiled

### Network Optimization

- **Connection Pooling**: HTTP/2 for backend services
- **Compression**: gzip compression support
- **Keep-Alive**: TCP connection reuse
- **Batch Requests**: Multiple gRPC calls grouped

## Security Architecture

### Authentication Flow

```
Client Request
    ↓
Extract Bearer Token from Authorization Header
    ↓
Validate Token Signature
    ↓
Check Token Expiration
    ↓
Set User Principal
    ↓
Continue Pipeline
```

### Rate Limiting

Token bucket algorithm per IP:

```
For each request:
    bucket = GetBucketForIp(clientIp)
    if bucket.TokensAvailable >= 1:
        bucket.Consume(1)
        Allow request
    else:
        Return 429 Too Many Requests
```

### Authorization

Per-route authentication requirements:

```
Route { Pattern = "admin.*", RequiresAuthentication = true }
    ↓
If request matches pattern AND not authenticated
    ↓
Return 401 Unauthorized
```

## Scalability

### Horizontal Scaling

Deploy multiple gateway instances behind load balancer:

```
Load Balancer
    ├── Gateway Instance 1 (Pod 1)
    ├── Gateway Instance 2 (Pod 2)
    ├── Gateway Instance 3 (Pod 3)
    └── Gateway Instance 4 (Pod 4)
         ↓
    Shared PostgreSQL Database
```

**Key**: Stateless design allows unlimited scaling

### Vertical Scaling

Increase single instance resources:

- Increase `MaxConcurrentConnections`
- Tune JIT compilation
- Increase cache size
- Use SSD for logs

### Database Scaling

For high metrics volume:

- Partition metrics by date
- Archive old metrics
- Use read replicas for reporting
- Consider time-series database (TimescaleDB)

## Extension Points

### Adding Custom Middleware

```csharp
public class CustomMiddleware
{
    private readonly RequestDelegate _next;
    
    public CustomMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Before request
        await _next(context);
        // After response
    }
}

// Register: app.UseMiddleware<CustomMiddleware>();
```

### Adding Custom Event Handlers

```csharp
public class CustomEventHandler : IEventHandler<ServiceRegisteredEvent>
{
    public async Task HandleAsync(ServiceRegisteredEvent @event)
    {
        // React to service registration
    }
}

// Register: services.AddScoped<IEventHandler<ServiceRegisteredEvent>, CustomEventHandler>();
```

### Adding Custom Metrics

```csharp
public class CustomMetricsService
{
    private readonly IMetricsRepository _repo;
    
    public async Task RecordCustomMetricAsync(string name, double value)
    {
        // Store custom metric
    }
}
```

## Testing Strategy

### Unit Testing

Test individual services with mocked dependencies:

```csharp
[Test]
public async Task RegisterService_ValidInput_ReturnsServiceId()
{
    // Arrange
    var mockRepo = new Mock<IGatewayRepository>();
    var service = new GatewayService(mockRepo.Object);
    
    // Act
    var result = await service.RegisterServiceAsync(...);
    
    // Assert
    Assert.That(result.Id, Is.GreaterThan(0));
}
```

### Integration Testing

Test service interactions:

```csharp
[Test]
public async Task Route_ServiceRegistered_SuccessfullyRouted()
{
    // Start gateway
    // Register service
    // Make request
    // Verify response
}
```

### Performance Testing

Load testing:

```bash
dotnet run --project tests/DotNetGrpcGateway.LoadTests
```

## Deployment Architecture

### Development

Single-container setup with embedded database.

### Production

Multi-tier deployment:

```
Internet
    ↓
CDN (CloudFront/CloudFlare)
    ↓
Load Balancer
    ↓
Gateway Cluster (3-5 instances)
    ↓
Database Cluster (Primary + Replicas)
    ↓
Backend gRPC Services (Multiple instances)
```

See [DEPLOYMENT.md](DEPLOYMENT.md) for details.
