# dotnet-grpc-gateway

A high-performance gRPC gateway for .NET that routes requests to multiple backend services with load balancing, caching, authentication, and real-time monitoring.

## Features

- **Dynamic Routing**: Route requests to multiple gRPC services based on URL patterns
- **Load Balancing**: Distribute traffic across service instances
- **Caching**: Cache responses for improved performance
- **Authentication**: JWT-based authentication and authorization
- **Monitoring**: Real-time metrics, latency percentiles, and health checks
- **Service Discovery**: Automatic service registration and health monitoring
- **Rate Limiting**: Tiered rate limiting by subscription level
- **Multi-Protocol**: gRPC, REST, and GraphQL support

## Quick Start

```bash
# Clone the repository
git clone https://github.com/Sarmkadan/dotnet-grpc-gateway.git
cd dotnet-grpc-gateway

# Start the gateway
docker-compose up -d

# Register a service
cd examples
dotnet run --project Example1_BasicServiceRegistration.cs
```

## Architecture

The gateway consists of several key components:

- **Gateway Core**: Routes incoming requests to appropriate services
- **Service Discovery**: Tracks registered services and their health
- **Performance Monitor**: Collects and analyzes request metrics
- **Authentication Service**: Manages API keys and authentication tokens
- **Health Monitoring**: Tracks service availability and readiness

## Documentation

- [Examples](examples/README.md) - Complete runnable examples
- [API Reference](docs/API-REFERENCE.md) - REST API documentation
- [Architecture Guide](docs/ARCHITECTURE.md) - System architecture
- [Deployment Guide](docs/DEPLOYMENT.md) - Production deployment instructions

## Projects

### Main Projects

- `src/dotnet-grpc-gateway` - Core gateway implementation
- `src/dotnet-grpc-gateway.Client` - Client library for service registration
- `src/dotnet-grpc-gateway.Tests` - Unit and integration tests

### Example Projects

See [examples/README.md](examples/README.md) for detailed examples.

## IRouteRepository

The `IRouteRepository` interface defines a contract for managing GatewayRoute entities. It provides methods for retrieving, creating, updating, and deleting routes, as well as filtering routes by service ID or pattern.

### Example Usage:

```csharp
public class Program
{
  public static async Task Main(string[] args)
  {
    var repository = new RouteRepository(new InMemoryConnectionStringProvider());

    var route = await repository.CreateAsync(new GatewayRoute
    {
      Pattern = "/api/users",
      TargetServiceId = 1,
      IsActive = true,
      Priority = 1
    });

    var activeRoutes = await repository.GetActiveAsync();
    Console.WriteLine($"Active routes: {activeRoutes.Count}");

    var routeById = await repository.GetByIdAsync(route.Id);
    Console.WriteLine($"Route by ID: {routeById.Pattern}");

    var routesByServiceId = await repository.GetByServiceIdAsync(1);
    Console.WriteLine($"Routes by service ID: {routesByServiceId.Count}");

    var routesByPattern = await repository.GetByPatternAsync("/api/users");
    Console.WriteLine($"Routes by pattern: {routesByPattern.Count}");

    await repository.UpdateAsync(route);
    await repository.DeleteAsync(route.Id);
  }
}
```

## IPerformanceMonitor

The `IPerformanceMonitor` interface provides real-time performance tracking for gRPC gateway services. It records request durations, calculates throughput metrics, and tracks latency percentiles (P50, P95, P99) to monitor SLA compliance and system health.

### Example Usage:

```csharp
public class Program
{
  public static async Task Main(string[] args)
  {
    // Create performance monitor (typically injected via DI)
    var performanceMonitor = new PerformanceMonitor();

    // Record request durations for different endpoints
    performanceMonitor.RecordRequestDuration("/api/users", 45);
    performanceMonitor.RecordRequestDuration("/api/products", 78);
    performanceMonitor.RecordRequestDuration("/api/users", 32);
    performanceMonitor.RecordRequestDuration("/api/products", 92);

    // Retrieve current performance metrics
    var metrics = await performanceMonitor.GetMetricsAsync();

    Console.WriteLine($"Total requests: {metrics.TotalRequests}");
    Console.WriteLine($"Average duration: {metrics.AverageDurationMs:F2} ms");
    Console.WriteLine($"P50 (median): {metrics.P50DurationMs:F2} ms");
    Console.WriteLine($"P95: {metrics.P95DurationMs:F2} ms");
    Console.WriteLine($"P99: {metrics.P99DurationMs:F2} ms");
    Console.WriteLine($"Requests per second: {metrics.RequestsPerSecond:F2}");
    Console.WriteLine($"Min duration: {metrics.MinDurationMs} ms");
    Console.WriteLine($"Max duration: {metrics.MaxDurationMs} ms");

    // Reset metrics for a fresh monitoring cycle
    await performanceMonitor.ResetAsync();
  }
}
```

## IUnitOfWork

The `IUnitOfWork` interface defines a contract for transaction management in the gRPC gateway. It provides methods for executing operations within transactions, committing or rolling back changes, and accessing gateway repositories through a unified interface. The Unit of Work pattern ensures that all operations either complete successfully together or fail together, maintaining data consistency.

### Example Usage:

```csharp
public class Program
{
  public static async Task Main(string[] args)
  {
    // Create repositories (typically injected via DI)
    var gateways = new GatewayRepository(new InMemoryConnectionStringProvider());
    var services = new ServiceRegistry();
    var routes = new RouteRepository(new InMemoryConnectionStringProvider());
    var metrics = new MetricsRepository();

    // Create unit of work (typically injected via DI)
    var unitOfWork = new UnitOfWork(gateways, services, routes, metrics);

    // Execute operations within a transaction
    var gatewayCount = await unitOfWork.ExecuteInTransactionAsync(async () =>
    {
      // Add a new gateway
      var gateway = await gateways.CreateAsync(new Gateway
      {
        Name = "Production Gateway",
        Address = "https://gateway.example.com",
        IsActive = true,
        Priority = 1
      });

      // Add a service
      var service = await services.RegisterServiceAsync(new ServiceRegistration
      {
        ServiceId = "user-service",
        Endpoint = "https://user-service.example.com:5001",
        HealthCheckEndpoint = "/health",
        IsActive = true
      });

      return await gateways.CountAsync();
    });

    Console.WriteLine($"Gateway count after transaction: {gatewayCount}");

    // Execute multiple operations in a single transaction
    await unitOfWork.ExecuteInTransactionAsync(async () =>
    {
      var route = await routes.CreateAsync(new GatewayRoute
      {
        Pattern = "/api/users",
        TargetServiceId = "user-service",
        IsActive = true,
        Priority = 1
      });

      // Multiple operations that should succeed or fail together
      await routes.UpdateAsync(route);
    });

    // Manually commit a transaction
    await unitOfWork.CommitAsync();

    // Manually rollback if needed
    // await unitOfWork.RollbackAsync();
  }
}
```
