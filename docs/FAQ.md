# Frequently Asked Questions

## General Questions

### What is dotnet-grpc-gateway?

A lightweight, production-ready .NET gateway that bridges web clients and gRPC backend services. It eliminates the need for Envoy or complex proxies by translating HTTP/1.1 requests to gRPC, enabling browsers to communicate with gRPC services directly.

### Why should I use dotnet-grpc-gateway instead of Envoy?

- **Lighter**: 15-30MB vs Envoy's 50-100MB
- **Simpler**: No complex YAML configuration
- **Faster Setup**: 5 minutes vs 30+ minutes
- **Better Integration**: Native .NET, works with your stack
- **Configuration as Code**: C# instead of YAML
- **Built-in Health Checks**: No additional components
- **Cost**: Cheaper infrastructure for smaller deployments

### Is it production-ready?

Yes. The gateway includes:
- Comprehensive error handling
- Health monitoring and liveness probes
- Kubernetes-ready deployment manifests
- Rate limiting and authentication
- Detailed metrics and logging
- 30-day metrics retention

### What gRPC services does it support?

Any gRPC service following the standard gRPC protocol, including:
- Unary RPC
- Server streaming
- Client streaming
- Bidirectional streaming

### Can I use it with my existing gRPC infrastructure?

Yes. The gateway works with:
- Existing gRPC services (no code changes)
- Current databases and storage
- Kubernetes or traditional servers
- Load balancers and proxies

---

## Installation & Setup

### What are the minimum system requirements?

- **OS**: Linux, Windows, or macOS
- **.NET Runtime**: 10.0 or later
- **Database**: PostgreSQL 14 or later
- **Memory**: 256MB minimum
- **CPU**: 1 core minimum (2+ recommended)

### How do I install it?

**Easiest**: Docker Compose
```bash
git clone https://github.com/Sarmkadan/dotnet-grpc-gateway.git
cd dotnet-grpc-gateway
docker-compose up -d
```

### Can I run it without Docker?

Yes. See [GETTING-STARTED.md](GETTING-STARTED.md#manual-installation) for manual installation steps.

### What database does it require?

PostgreSQL 14+. The gateway stores:
- Gateway configuration
- Registered services
- Routes and rules
- Metrics and statistics
- Authentication tokens

### Can I use MySQL instead of PostgreSQL?

Not currently. PostgreSQL is required. Consider opening an issue if this is a blocker.

### How do I change the port?

**Via environment variable:**
```bash
export Gateway__Port=8080
```

**Via appsettings.json:**
```json
{
  "Gateway": {
    "Port": 8080
  }
}
```

---

## Configuration

### How do I configure rate limiting?

Per route:
```bash
curl -X POST http://localhost:5000/api/gateway/routes \
  -H "Content-Type: application/json" \
  -d '{
    "pattern": "user.*",
    "targetServiceId": 1,
    "rateLimitPerMinute": 1000
  }'
```

Or use the API to update existing routes.

### What rate limiting algorithm does it use?

Token bucket algorithm with per-IP tracking. Each IP gets a bucket refilled once per minute.

### How do I enable HTTPS/TLS?

In production, use a load balancer or Kubernetes ingress to handle TLS. Or configure in appsettings:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://0.0.0.0:5000",
        "Certificate": {
          "Path": "/secrets/tls.crt",
          "KeyPath": "/secrets/tls.key"
        }
      }
    }
  }
}
```

### How do I enable authentication?

1. Create a token:
```bash
curl -X POST http://localhost:5000/api/gateway/auth/tokens \
  -d '{"token": "secret-key", "expiresAt": "2025-12-31T23:59:59Z"}'
```

2. Require authentication on routes:
```bash
curl -X POST http://localhost:5000/api/gateway/routes \
  -d '{
    "pattern": "admin.*",
    "requiresAuthentication": true
  }'
```

3. Call with token:
```bash
curl http://localhost:5000/admin/endpoint \
  -H "Authorization: Bearer secret-key"
```

### How do I configure logging?

Edit `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "Serilog": {
    "MinimumLevel": "Information"
  }
}
```

Or set environment variable:
```bash
export Logging__LogLevel__Default=Debug
```

### How do I configure metrics retention?

```json
{
  "Gateway": {
    "Metrics": {
      "RetentionDays": 30,
      "CollectionIntervalSeconds": 60
    }
  }
}
```

Reduces memory by lowering `RetentionDays` from 30 to 7.

---

## Routing

### How do route patterns work?

Three match types:

```
Exact:  "user.UserService.GetUser" matches exactly
Prefix: "user.*" matches "user.UserService.GetUser"
Regex:  "^user\.(.*)" matches any service starting with "user."
```

### What happens if multiple routes match?

The route with highest priority wins. Example:

```
Route 1: Pattern = "user.*", Priority = 100
Route 2: Pattern = "user.UserService.*", Priority = 50

Request to "user.UserService.GetUser" matches Route 1 (higher priority)
```

### Can I have routes without rate limiting?

Yes. Set `rateLimitPerMinute: 0` to disable limits on a route.

### Can I have routes without caching?

Yes. Set `enableCaching: false` on the route.

---

## Health Checks

### Why is my service showing as "Unhealthy"?

Possible causes:
1. Service not running
2. Wrong host/port configuration
3. Firewall blocking access
4. Service not responding to gRPC health checks

**Verify**:
```bash
# Direct gRPC connection
grpcurl -plaintext <host>:<port> list

# Check gateway health for service
curl http://localhost:5000/api/health/services/1
```

### How often are health checks performed?

Every 30 seconds by default. Configure:

```json
{
  "Gateway": {
    "HealthCheck": {
      "IntervalSeconds": 30,
      "TimeoutMs": 5000,
      "FailureThreshold": 3
    }
  }
}
```

### What happens if a service fails health check?

After 3 consecutive failures (90 seconds default):
1. Service marked as unhealthy
2. Requests routed to other services if available
3. `ServiceHealthCheckFailedEvent` published
4. Webhooks triggered (if configured)

---

## Metrics & Performance

### What metrics are collected?

Per-request:
- Latency (milliseconds)
- Status code
- Service name
- Method name

Aggregated daily:
- Total requests
- Success/failure counts
- Error rate
- Response time percentiles (P50, P95, P99)

### How long are metrics retained?

30 days by default. Configure:

```json
{
  "Gateway": {
    "Metrics": {
      "RetentionDays": 30
    }
  }
}
```

Metrics older than retention period are automatically deleted.

### How do I export metrics?

```bash
# Today's statistics
curl http://localhost:5000/api/gateway/statistics/today

# Performance metrics
curl http://localhost:5000/api/metrics/performance

# Slow requests
curl http://localhost:5000/api/metrics/slow?threshold=1000
```

### How can I monitor the gateway?

1. **Health Endpoints**: `/api/health/status`, `/api/health/ready`
2. **Metrics API**: `/api/metrics/performance`
3. **Logs**: `logs/gateway-*.txt` or container logs
4. **Kubernetes**: `kubectl logs`, `kubectl top pod`

### What's a good baseline for performance?

- **P50 Latency**: < 50ms (median)
- **P99 Latency**: < 500ms (99th percentile)
- **Error Rate**: < 0.5%
- **Cache Hit Rate**: > 50% (if caching enabled)

---

## Scaling & Performance

### How many concurrent connections can it handle?

Default: 1000. Configure:

```json
{
  "Gateway": {
    "MaxConcurrentConnections": 5000
  }
}
```

Actual capacity depends on:
- Available memory
- CPU cores
- Backend service capacity
- Network bandwidth

### How do I scale the gateway?

**Horizontal**: Run multiple instances behind a load balancer
```bash
# Kubernetes
kubectl scale deployment grpc-gateway --replicas=5
```

**Vertical**: Increase resources for single instance
```json
{
  "Gateway": {
    "MaxConcurrentConnections": 10000
  }
}
```

### Will caching improve performance?

Yes, significantly for read-heavy workloads:
- Cache hit: 1-5ms (local cache)
- Cache miss: 50-500ms (backend service)

Enable on routes:
```bash
curl -X POST http://localhost:5000/api/gateway/routes \
  -d '{
    "enableCaching": true,
    "cacheDurationSeconds": 300
  }'
```

### How do I reduce memory usage?

1. Lower metrics retention: `RetentionDays: 7`
2. Lower cache size: Reduce `cacheDurationSeconds`
3. Reduce concurrent connections: `MaxConcurrentConnections: 500`
4. Increase collection interval: `CollectionIntervalSeconds: 120`

---

## Troubleshooting

### Gateway won't start

**Error**: `Connection string 'DefaultConnection' not found.`

**Solution**:
```bash
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=grpc_gateway;Username=postgres;Password=postgres"
```

### Service stays unhealthy

**Solutions**:
1. Check if service is running: `grpcurl -plaintext localhost:5001 list`
2. Verify host/port in service configuration
3. Check firewall rules
4. Look at gateway logs: `tail -f logs/gateway-*.txt`

### High memory usage

**Solutions**:
1. Reduce metrics retention: Change to 7 days
2. Lower `MaxConcurrentConnections`
3. Disable caching or reduce TTL
4. Check for memory leaks: Monitor over time

### Slow responses

**Solutions**:
1. Check slow requests: `curl http://localhost:5000/api/metrics/slow`
2. Verify backend service performance
3. Enable caching for read operations
4. Increase timeouts: `RequestTimeoutMs: 60000`

### Rate limiting not working

**Troubleshoot**:
1. Verify `rateLimitPerMinute` is set on route
2. Check middleware is enabled in pipeline
3. Ensure high enough limit for your workload
4. Test with high volume: 1000+ requests/minute

### Database connection pool exhausted

**Solutions**:
1. Increase pool size in connection string
2. Close idle connections faster
3. Monitor connection usage
4. Scale horizontally

---

## Deployment

### How do I deploy to Kubernetes?

```bash
kubectl apply -f k8s/
# See docs/DEPLOYMENT.md for full guide
```

### How do I deploy to AWS?

Use ECS or EKS:
1. Push image to ECR
2. Create ECS task definition or EKS deployment
3. Use RDS PostgreSQL
4. Put ALB in front for load balancing

### How do I deploy to Azure?

Use AKS or App Service:
1. Push image to ACR
2. Deploy to AKS or App Service
3. Use Azure Database for PostgreSQL
4. Use Application Gateway for load balancing

### Can I run it on a single machine?

Yes, for development/testing:
```bash
docker-compose up -d
```

Not recommended for production. Use Kubernetes or managed container service.

### How do I handle database backups?

```bash
# PostgreSQL backup
pg_dump grpc_gateway > backup.sql

# In Kubernetes
kubectl exec -it postgres-0 -- \
  pg_dump -U postgres grpc_gateway > backup.sql
```

---

## Development

### How do I contribute?

1. Fork repository
2. Create feature branch
3. Make changes
4. Add tests
5. Submit pull request

See README.md Contributing section.

### How do I run tests?

```bash
dotnet test
```

### How do I build from source?

```bash
dotnet build -c Release
```

### How do I add a custom middleware?

See [ARCHITECTURE.md](ARCHITECTURE.md#extension-points).

### How do I add custom metrics?

See [ARCHITECTURE.md](ARCHITECTURE.md#extension-points).

---

## Support & Community

### Where can I get help?

- 📚 Documentation: [GitHub Wiki](https://github.com/Sarmkadan/dotnet-grpc-gateway/wiki)
- 💬 Discussions: [GitHub Discussions](https://github.com/Sarmkadan/dotnet-grpc-gateway/discussions)
- 🐛 Issues: [GitHub Issues](https://github.com/Sarmkadan/dotnet-grpc-gateway/issues)
- 📧 Email: support@sarmkadan.com

### How do I report a bug?

1. Search existing issues
2. Create new issue with:
   - Reproducible steps
   - Expected vs actual behavior
   - Logs/error messages
   - Environment details

### How do I request a feature?

Open a GitHub Discussion or Issue with:
- Use case description
- Why it's needed
- Suggested implementation (optional)

---

## License & Legal

### What license is this under?

MIT License - See [LICENSE](../LICENSE)

### Can I use this commercially?

Yes. MIT license allows commercial use.

### Do you provide support SLA?

Community support only. For commercial support, contact support@sarmkadan.com
