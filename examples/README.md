# dotnet-grpc-gateway Examples

Complete, runnable examples demonstrating common patterns and use cases for dotnet-grpc-gateway.

## Quick Start

All examples require the gateway to be running:

```bash
# Terminal 1: Start the gateway
cd ../
docker-compose up -d
# or: make docker-run

# Terminal 2: Run any example
cd examples
dotnet run --project Example1_BasicServiceRegistration.cs
```

---

## Example 1: Basic Service Registration

**File**: `Example1_BasicServiceRegistration.cs`

**Demonstrates**:
- Registering a gRPC service with the gateway
- Verifying service registration
- Checking service health
- Unregistering a service

**Run**:
```bash
dotnet run --project Example1_BasicServiceRegistration.cs
```

**Key APIs**:
- `POST /api/gateway/services` - Register service
- `GET /api/gateway/services` - List services
- `GET /api/gateway/services/{id}` - Get service details
- `DELETE /api/gateway/services/{id}` - Unregister service

**Use Case**: Initial setup when deploying a new gRPC backend service

---

## Example 2: Dynamic Route Configuration

**File**: `Example2_DynamicRouteConfiguration.cs`

**Demonstrates**:
- Creating routes with different patterns (prefix, exact, regex)
- Setting up priority-based matching
- Configuring rate limits and caching
- Testing route matching
- Detecting route conflicts

**Run**:
```bash
dotnet run --project Example2_DynamicRouteConfiguration.cs
```

**Key APIs**:
- `POST /api/gateway/routes` - Create route
- `GET /api/gateway/routes` - List routes
- `POST /api/servicediscovery/route-match` - Test route matching
- `POST /api/servicediscovery/route-conflicts` - Detect conflicts

**Route Matching Types**:
- `0` = Exact match: `user.UserService.GetUser`
- `1` = Prefix match: `user.UserService.*`
- `2` = Regex match: `^user\..*`

**Use Case**: Setting up complex routing rules for multiple services

---

## Example 3: Metrics & Monitoring

**File**: `Example3_MetricsAndMonitoring.cs`

**Demonstrates**:
- Collecting performance metrics
- Analyzing latency percentiles (P50, P95, P99)
- Identifying slow requests
- Tracking error distribution
- Finding top endpoints by volume
- Resetting metrics

**Run**:
```bash
dotnet run --project Example3_MetricsAndMonitoring.cs
```

**Key APIs**:
- `GET /api/metrics/performance` - Performance stats
- `GET /api/metrics/slow` - Slow requests
- `GET /api/metrics/errors` - Error distribution
- `GET /api/metrics/endpoints` - Top endpoints
- `GET /api/gateway/statistics/today` - Daily statistics
- `POST /api/metrics/reset` - Reset metrics

**Performance Baselines**:
- P50 Latency: < 50ms (healthy)
- P99 Latency: < 500ms (good)
- Error Rate: < 0.5% (acceptable)
- Cache Hit Rate: > 50% (if caching enabled)

**Use Case**: Production monitoring and performance optimization

---

## Example 4: Authentication & Security

**File**: `Example4_AuthenticationAndSecurity.cs`

**Demonstrates**:
- Creating API authentication tokens
- Creating protected routes (requiring authentication)
- Making authenticated requests
- Testing unauthorized access
- Setting up tiered access (Free/Pro/Enterprise)
- Managing token expiration

**Run**:
```bash
dotnet run --project Example4_AuthenticationAndSecurity.cs
```

**Key APIs**:
- `POST /api/gateway/auth/tokens` - Create token
- `GET /api/gateway/auth/tokens` - List tokens
- `POST /api/gateway/routes` - Create protected route (with `requiresAuthentication: true`)

**Authentication Headers**:
```
Authorization: Bearer <token>
```

**Use Case**: API security and rate limiting by subscription tier

---

## Example 5: Health Checks & Monitoring

**File**: `Example5_HealthChecksAndMonitoring.cs`

**Demonstrates**:
- Checking gateway overall health
- Monitoring individual service health
- Using readiness probes (for load balancers)
- Using liveness probes (for Kubernetes)
- Polling health status over time
- Tracking consecutive failures

**Run**:
```bash
dotnet run --project Example5_HealthChecksAndMonitoring.cs
```

**Key Endpoints**:
- `GET /health` - Basic health check
- `GET /api/health/status` - Detailed status
- `GET /api/health/services` - All services health
- `GET /api/health/services/{id}` - Service details
- `GET /api/health/ready` - Readiness probe
- `GET /api/health/live` - Liveness probe

**Health Probe Timeouts**:
- Readiness: 3-5 second timeout
- Liveness: 5-10 second timeout
- Initial Delay: 10-30 seconds

**Use Case**: Kubernetes deployments and load balancer health checks

---

## Example 6: Logging & Request Tracing (Optional)

**Demonstrates**:
- Structured logging setup
- Request correlation IDs
- Response time tracking
- Error logging and analysis

**Example Code**:
```csharp
// Check logs
docker logs dotnet-grpc-gateway
# or
tail -f logs/gateway-*.txt
```

---

## Example 7: Webhook Integration (Optional)

**Demonstrates**:
- Sending webhooks on service events
- Webhook retry logic
- Event-driven architecture

---

## Common Patterns

### Pattern 1: Gradual Rollout

```csharp
// Route percentage of traffic to new service
var newRoute = new { pattern = "user.*", priority = 150 };  // Gets priority
var oldRoute = new { pattern = "user.*", priority = 100 };  // Fallback
```

### Pattern 2: Circuit Breaker

Monitor service health and automatically disable route if it fails:

```csharp
// Threshold: 3 consecutive health check failures
var failureThreshold = 3;
// Service automatically marked unhealthy after failures
```

### Pattern 3: Rate Limiting by Tier

```csharp
// Free tier: 100 req/min
var freeRoute = new { rateLimitPerMinute = 100 };

// Pro tier: 1000 req/min
var proRoute = new { rateLimitPerMinute = 1000 };

// Enterprise: 10000 req/min
var enterpriseRoute = new { rateLimitPerMinute = 10000 };
```

### Pattern 4: Cache Strategy

```csharp
// Read operations: Cache for 5 minutes
var listRoute = new { enableCaching = true, cacheDurationSeconds = 300 };

// Write operations: Never cache
var createRoute = new { enableCaching = false };
```

### Pattern 5: Multi-Service Failover

```csharp
// Register multiple instances of same service
// Create routes with different priorities
// Automatically route to healthy instance
```

---

## Running in Batch

Run all examples in sequence:

```bash
for example in Example*.cs; do
    echo "Running $example..."
    dotnet run --project "$example"
    sleep 5
done
```

---

## Integration with CI/CD

### GitHub Actions

```yaml
- name: Run Examples
  run: |
    cd examples
    dotnet run --project Example1_BasicServiceRegistration.cs
    dotnet run --project Example2_DynamicRouteConfiguration.cs
```

### Local Testing

```bash
make test  # Runs unit tests
make examples  # Runs all examples (if configured)
```

---

## Troubleshooting

### "Connection refused"

Gateway is not running:
```bash
docker-compose up -d
# or
make docker-run
```

### "Service not found"

Register the service first:
```bash
dotnet run --project Example1_BasicServiceRegistration.cs
```

### Port already in use

Change gateway port in docker-compose.yml or use:
```bash
lsof -i :5000
kill -9 <PID>
```

---

## Next Steps

1. Modify examples for your services
2. Create production routes based on patterns
3. Set up monitoring and alerts
4. Deploy to Kubernetes
5. Monitor metrics and adjust limits

---

## Support

- 📚 [Full Documentation](../README.md)
- 🏗️ [Architecture Guide](../docs/ARCHITECTURE.md)
- 📖 [API Reference](../docs/API-REFERENCE.md)
- 🐛 [Issue Tracker](https://github.com/Sarmkadan/dotnet-grpc-gateway/issues)
- 💬 [Discussions](https://github.com/Sarmkadan/dotnet-grpc-gateway/discussions)
