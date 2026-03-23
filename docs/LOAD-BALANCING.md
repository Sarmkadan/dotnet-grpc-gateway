# Load Balancing and Circuit Breaker Guide

This guide covers how to distribute traffic across multiple upstream instances and protect against cascading failures using dotnet-grpc-gateway.

## Load Balancing

### Overview

dotnet-grpc-gateway supports routing to multiple instances of the same gRPC service. Register each instance separately, then create routes with matching patterns to all of them using priority and health-aware selection.

### Registering Multiple Instances

```bash
# Instance 1
curl -X POST http://localhost:5000/api/gateway/services \
  -H "Content-Type: application/json" \
  -d '{
    "name": "OrderService-1",
    "serviceFullName": "order.OrderService",
    "host": "orders-pod-1",
    "port": 5001,
    "healthCheckIntervalSeconds": 15
  }'

# Instance 2
curl -X POST http://localhost:5000/api/gateway/services \
  -H "Content-Type: application/json" \
  -d '{
    "name": "OrderService-2",
    "serviceFullName": "order.OrderService",
    "host": "orders-pod-2",
    "port": 5001,
    "healthCheckIntervalSeconds": 15
  }'
```

### Priority-Based Routing (Active/Standby)

Use different `priority` values to create an active/standby pair. The route with the highest priority that leads to a healthy service is selected.

```bash
# Primary (higher priority)
curl -X POST http://localhost:5000/api/gateway/routes \
  -H "Content-Type: application/json" \
  -d '{
    "pattern": "order.OrderService.*",
    "targetServiceId": 1,
    "matchType": 1,
    "priority": 200
  }'

# Standby (lower priority, used when primary is down)
curl -X POST http://localhost:5000/api/gateway/routes \
  -H "Content-Type: application/json" \
  -d '{
    "pattern": "order.OrderService.*",
    "targetServiceId": 2,
    "matchType": 1,
    "priority": 100
  }'
```

When the primary service becomes unhealthy, the gateway automatically falls through to the standby route.

### Weighted Routing

Route a percentage of traffic to a canary or new version by adjusting `rateLimitPerMinute` relative to total expected traffic, or by using service-level routing with different patterns.

```bash
# Stable version — higher rate limit allows more traffic
curl -X POST http://localhost:5000/api/gateway/routes \
  -H "Content-Type: application/json" \
  -d '{
    "pattern": "order.OrderService.*",
    "targetServiceId": 1,
    "matchType": 1,
    "priority": 200,
    "rateLimitPerMinute": 900
  }'

# Canary version — constrained to a smaller share
curl -X POST http://localhost:5000/api/gateway/routes \
  -H "Content-Type: application/json" \
  -d '{
    "pattern": "order.OrderService.*",
    "targetServiceId": 3,
    "matchType": 1,
    "priority": 150,
    "rateLimitPerMinute": 100
  }'
```

### Health-Aware Routing

The gateway only dispatches to services that are currently healthy. A service is marked unhealthy after `FailureThreshold` consecutive failed health probes.

```json
{
  "Gateway": {
    "HealthCheck": {
      "IntervalSeconds": 15,
      "TimeoutMs": 3000,
      "FailureThreshold": 2
    }
  }
}
```

Lower the `FailureThreshold` and `IntervalSeconds` to detect failures and reroute traffic faster.

## Circuit Breaker Pattern

A circuit breaker prevents a failing upstream from overwhelming the gateway with retried calls. Implement it at the infrastructure level using one of the patterns below.

### Option 1: Rate Limiting as a Soft Circuit Breaker

Set `rateLimitPerMinute` to a low value on routes targeting a degraded service. Update the route dynamically via the API when a service starts showing high error rates.

```bash
# Throttle a degraded service
curl -X PUT http://localhost:5000/api/gateway/routes/2 \
  -H "Content-Type: application/json" \
  -d '{
    "pattern": "order.OrderService.*",
    "targetServiceId": 2,
    "matchType": 1,
    "priority": 100,
    "rateLimitPerMinute": 10
  }'
```

Restore the original limit once the service recovers.

### Option 2: Polly-Based Circuit Breaker (Recommended for Production)

Add [Polly](https://github.com/App-vNext/Polly) to your upstream gRPC client registration for resilience policies including circuit breaker, retry, and timeout.

#### Install Polly

```bash
dotnet add src/dotnet-grpc-gateway package Microsoft.Extensions.Http.Polly
```

#### Configure in Program.cs

```csharp
services.AddHttpClient<IGrpcClientFactory, GrpcClientFactory>()
    .AddTransientHttpErrorPolicy(policy =>
        policy.CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30)))
    .AddTransientHttpErrorPolicy(policy =>
        policy.WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(200 * attempt)))
    .AddTransientHttpErrorPolicy(policy =>
        policy.TimeoutAsync(TimeSpan.FromSeconds(5)));
```

#### Circuit Breaker States

```
    ┌──────────┐  5 failures   ┌──────────┐
    │  Closed  │ ────────────► │   Open   │
    │ (normal) │               │(blocking)│
    └──────────┘               └────┬─────┘
          ▲                         │ 30s timer
          │ success                 ▼
    ┌─────┴────────┐         ┌──────────────┐
    │   Half-Open  │ ◄────── │  Half-Open   │
    │  (1 trial)   │         │  (probing)   │
    └──────────────┘         └──────────────┘
```

| State | Behaviour |
|-------|-----------|
| **Closed** | All requests pass through normally |
| **Open** | Requests fail immediately without hitting the upstream |
| **Half-Open** | A single trial request is allowed; success closes the breaker |

### Option 3: Kubernetes-Native

When running on Kubernetes, combine the gateway's health-aware routing with Kubernetes liveness/readiness probes. The gateway exposes dedicated endpoints:

```yaml
livenessProbe:
  httpGet:
    path: /api/health/live
    port: 5000
  initialDelaySeconds: 5
  periodSeconds: 10

readinessProbe:
  httpGet:
    path: /api/health/ready
    port: 5000
  initialDelaySeconds: 10
  periodSeconds: 15
```

Kubernetes removes unhealthy pods from the service's endpoint slice, so the gateway only sees healthy upstreams.

## Request Timeout Configuration

Prevent slow upstreams from holding connections indefinitely:

```json
{
  "Gateway": {
    "RequestTimeoutMs": 10000
  }
}
```

Combine a tight gateway timeout with a Polly timeout policy to ensure both layers cut long-running calls.

## Complete Production Example

The following configuration sets up a highly-available pair of services with health checking, rate limiting, and a Polly circuit breaker:

```json
{
  "Gateway": {
    "RequestTimeoutMs": 8000,
    "MaxConcurrentConnections": 2000,
    "HealthCheck": {
      "IntervalSeconds": 10,
      "TimeoutMs": 2000,
      "FailureThreshold": 2
    }
  }
}
```

```bash
# Register two instances
curl -X POST http://localhost:5000/api/gateway/services \
  -d '{"name":"PaySvc-A","serviceFullName":"pay.PaymentService","host":"pay-a","port":5001,"healthCheckIntervalSeconds":10}'

curl -X POST http://localhost:5000/api/gateway/services \
  -d '{"name":"PaySvc-B","serviceFullName":"pay.PaymentService","host":"pay-b","port":5001,"healthCheckIntervalSeconds":10}'

# Primary and standby routes
curl -X POST http://localhost:5000/api/gateway/routes \
  -d '{"pattern":"pay.PaymentService.*","targetServiceId":1,"matchType":1,"priority":200,"rateLimitPerMinute":5000}'

curl -X POST http://localhost:5000/api/gateway/routes \
  -d '{"pattern":"pay.PaymentService.*","targetServiceId":2,"matchType":1,"priority":100,"rateLimitPerMinute":5000}'
```

## Monitoring Load Balancing

### View per-service metrics

```bash
# Request counts and error rates
curl http://localhost:5000/api/metrics/performance

# Slow requests (> 500ms)
curl "http://localhost:5000/api/metrics/slow?threshold=500"

# Average response time
curl http://localhost:5000/api/gateway/metrics/average-response-time
```

### Check service health distribution

```bash
curl http://localhost:5000/api/health/services
```

```json
{
  "1": "Healthy",
  "2": "Healthy"
}
```

## Troubleshooting

### All traffic goes to one instance

- Ensure both services have separate IDs in the registry
- Verify both services are marked healthy: `GET /api/health/services`
- Check that route `priority` values differ if you intend active/standby
- For equal-priority round-robin, confirm multiple routes exist with the same priority

### Circuit breaker tripping too aggressively

- Increase `handledEventsAllowedBeforeBreaking` in Polly config
- Increase `durationOfBreak` to give slow services more recovery time
- Review upstream service logs for root cause

### High latency during failover

- Reduce `HealthCheck.IntervalSeconds` so unhealthy services are detected faster
- Lower `FailureThreshold` to `2` to reduce the number of probes before failover
- Pre-warm the standby instance to reduce cold-start latency

### Rate limiting causing 429 errors unexpectedly

- Check `rateLimitPerMinute` on each route: `GET /api/gateway/routes`
- Rate limits apply per route, not per service instance
- Increase `rateLimitPerMinute` or split traffic across multiple routes
