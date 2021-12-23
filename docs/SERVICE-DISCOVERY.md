# Service Discovery Integration Guide

This guide explains how dotnet-grpc-gateway discovers, registers, and health-checks upstream gRPC services at runtime.

## Overview

The gateway uses a **pull-based service registry** backed by PostgreSQL. Services register themselves via the REST API; the gateway continuously health-checks them and removes stale entries automatically.

```
┌─────────────────────────────────────────────────────┐
│  Client (browser / app)                             │
└──────────────────────┬──────────────────────────────┘
                       │ gRPC-Web
          ┌────────────▼──────────────┐
          │   dotnet-grpc-gateway     │
          │  ┌─────────────────────┐  │
          │  │  Service Registry   │  │
          │  │  (PostgreSQL)       │  │
          │  └─────────────────────┘  │
          └─┬──────────┬──────────────┘
            │          │
    ┌───────▼─┐   ┌────▼────┐
    │Service A│   │Service B│  ...
    └─────────┘   └─────────┘
```

## Registering a Service

### REST API

```bash
curl -X POST http://localhost:5000/api/gateway/services \
  -H "Content-Type: application/json" \
  -d '{
    "name": "OrderService",
    "serviceFullName": "order.OrderService",
    "host": "orders-svc",
    "port": 5001,
    "useTls": false,
    "healthCheckIntervalSeconds": 30
  }'
```

**Response:**
```json
{
  "id": 1,
  "name": "OrderService",
  "host": "orders-svc",
  "port": 5001,
  "isHealthy": false,
  "createdAt": "2025-01-01T00:00:00Z"
}
```

### Required Fields

| Field | Type | Description |
|-------|------|-------------|
| `name` | string | Human-readable service name |
| `serviceFullName` | string | Protobuf fully-qualified service name (e.g. `package.Service`) |
| `host` | string | Hostname or IP of the gRPC server |
| `port` | int | Port number |

### Optional Fields

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `useTls` | bool | `false` | Enable TLS for upstream connection |
| `healthCheckIntervalSeconds` | int | `30` | How often to probe `/health` |
| `tags` | string[] | `[]` | Metadata tags for filtering |

## Adding Routes

Once a service is registered, add a route to direct gRPC calls to it:

```bash
curl -X POST http://localhost:5000/api/gateway/routes \
  -H "Content-Type: application/json" \
  -d '{
    "pattern": "order.OrderService.*",
    "targetServiceId": 1,
    "matchType": 1,
    "priority": 100,
    "rateLimitPerMinute": 500
  }'
```

### Match Types

| Value | Name | Example Pattern |
|-------|------|-----------------|
| `0` | Exact | `order.OrderService/CreateOrder` |
| `1` | Prefix | `order.OrderService.*` |
| `2` | Regex | `^order\\..*` |

Prefix match is recommended for routing all methods of a service to the same backend.

## Health Checking

The background service `HealthCheckBackgroundService` runs health checks on every registered service at the configured interval.

### Configuration

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

| Setting | Description |
|---------|-------------|
| `IntervalSeconds` | Interval between health probes |
| `TimeoutMs` | Timeout for a single probe |
| `FailureThreshold` | Consecutive failures before marking unhealthy |

### Querying Health Status

```bash
# All services
curl http://localhost:5000/api/health/services

# Single service
curl http://localhost:5000/api/gateway/services/1

# Gateway-level health (for load balancers)
curl http://localhost:5000/api/health/ready
```

**Example response:**
```json
{
  "serviceId": 1,
  "isHealthy": true,
  "healthStatus": "Healthy",
  "responseTimeMs": 12,
  "lastCheckedAt": "2025-01-01T00:00:10Z"
}
```

### Manually Triggering a Health Check

```bash
curl -X POST http://localhost:5000/api/health/services/1/check
```

## Dynamic Service Discovery

The gateway exposes a route-matching endpoint so external tooling can verify that a particular gRPC call will be dispatched correctly:

```bash
curl -X POST http://localhost:5000/api/servicediscovery/route-match \
  -H "Content-Type: application/json" \
  -d '{
    "serviceName": "order.OrderService",
    "methodName": "CreateOrder"
  }'
```

**Response:**
```json
{
  "matched": true,
  "routeId": 1,
  "targetService": {
    "id": 1,
    "name": "OrderService",
    "host": "orders-svc",
    "port": 5001,
    "isHealthy": true
  }
}
```

## Complete Example: Multi-Service Setup

```bash
# 1. Register services
curl -X POST http://localhost:5000/api/gateway/services \
  -H "Content-Type: application/json" \
  -d '{"name":"UserService","serviceFullName":"user.UserService","host":"users-svc","port":5001}'

curl -X POST http://localhost:5000/api/gateway/services \
  -H "Content-Type: application/json" \
  -d '{"name":"OrderService","serviceFullName":"order.OrderService","host":"orders-svc","port":5002}'

# 2. Add routes
curl -X POST http://localhost:5000/api/gateway/routes \
  -H "Content-Type: application/json" \
  -d '{"pattern":"user.UserService.*","targetServiceId":1,"matchType":1,"priority":100}'

curl -X POST http://localhost:5000/api/gateway/routes \
  -H "Content-Type: application/json" \
  -d '{"pattern":"order.OrderService.*","targetServiceId":2,"matchType":1,"priority":100}'

# 3. Verify routing
curl http://localhost:5000/api/gateway/routes
curl http://localhost:5000/api/health/services
```

## Kubernetes Integration

For Kubernetes deployments, register services using the internal cluster DNS name:

```bash
curl -X POST http://localhost:5000/api/gateway/services \
  -H "Content-Type: application/json" \
  -d '{
    "name": "UserService",
    "serviceFullName": "user.UserService",
    "host": "user-service.default.svc.cluster.local",
    "port": 5001,
    "useTls": false,
    "healthCheckIntervalSeconds": 15
  }'
```

Use a Kubernetes `Job` or init container in your CI/CD pipeline to register services on deployment.

## Removing a Service

```bash
# Deregister a service (also removes its routes)
curl -X DELETE http://localhost:5000/api/gateway/services/1
```

The `ServiceCleanupBackgroundService` automatically removes services that have been unhealthy longer than the configured threshold.

## Troubleshooting

### Service stays unhealthy

1. Verify the gRPC service is listening: `grpcurl -plaintext <host>:<port> list`
2. Check network connectivity from the gateway container
3. Review gateway logs: `tail -f logs/gateway-*.txt`
4. Lower `FailureThreshold` in config for faster recovery detection

### Route not matching

1. Use the route-match endpoint to debug pattern matching
2. Check `matchType` — `Prefix` requires the pattern to end with `.*`
3. Verify `priority` — higher value wins when multiple routes match

### Service not reachable after registration

The first health check runs within `IntervalSeconds` of registration. The service will show `isHealthy: false` until the first successful probe. Use `POST /api/health/services/{id}/check` to trigger an immediate check.
