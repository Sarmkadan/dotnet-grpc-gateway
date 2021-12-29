# dotnet-grpc-gateway

A lightweight gRPC-Web gateway for .NET that enables browser-compatible gRPC communication without complex infrastructure like Envoy.

![Build](https://github.com/sarmkadan/dotnet-grpc-gateway/actions/workflows/build.yml/badge.svg)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Overview

The **dotnet-grpc-gateway** is a minimal, production-ready gateway that bridges web clients and gRPC backend services. It translates HTTP/1.1 gRPC-Web requests to standard gRPC calls, enabling direct browser access to gRPC services without deploying complex proxy infrastructure.

## Features

- ✅ **gRPC-Web Support** - Browser-compatible HTTP/1.1 translation
- ✅ **Service Discovery** - Automatic service registration and health monitoring
- ✅ **Request Routing** - Pattern-based routing with wildcards and priorities
- ✅ **Metrics & Analytics** - Latency tracking, error rates, and request volumes
- ✅ **Security** - Bearer token authentication and rate limiting
- ✅ **Load Balancing** - Multiple strategies (RoundRobin, Random, LeastConnections)
- ✅ **Circuit Breaker** - Per-service failure protection
- ✅ **Request Logging** - Structured request/response logging
- ✅ **Caching** - In-memory response caching with TTL

## Quick Start

### 1. Start the Gateway (Docker)

```bash
git clone https://github.com/sarmkadan/dotnet-grpc-gateway.git
cd dotnet-grpc-gateway
docker-compose up -d
# Gateway available at http://localhost:5000
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

### 3. Add a Routing Rule

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
# REST API call
curl http://localhost:5000/user/GetUser?userId=123

# gRPC-Web call (from browser)
fetch('http://localhost:5000/user.UserService/GetUser', {
  method: 'POST',
  headers: { 'Content-Type': 'application/grpc-web+proto' },
  body: /* protobuf binary */
})
```

## Examples

The repository includes a variety of practical examples to help you get started:

- `examples/BasicUsage.cs`: Minimal setup and registration example.
- `examples/AdvancedUsage.cs`: Configuration, routing, and error handling.
- `examples/IntegrationExample.cs`: Wiring into ASP.NET dependency injection.
- `examples/`: Additional service-specific examples and Docker deployment guides.

## Installation

### Option 1: Docker Compose (Recommended)

```bash
git clone https://github.com/sarmkadan/dotnet-grpc-gateway.git
cd dotnet-grpc-gateway
docker-compose up -d
```

Gateway will be available at `http://localhost:5000`
PostgreSQL at `localhost:5432`


### Option 2: Manual Installation

**Prerequisites:**
- .NET 10 SDK or later
- PostgreSQL 14 or later
- Git

**Steps:**

```bash
git clone https://github.com/sarmkadan/dotnet-grpc-gateway.git
cd dotnet-grpc-gateway

dotnet restore
dotnet build -c Release

export CONNECTION_STRING="Host=localhost;Port=5432;Database=grpc_gateway;Username=postgres;Password=postgres"
dotnet run --project src/dotnet-grpc-gateway -- migrate
dotnet run --project src/dotnet-grpc-gateway -c Release
```

### Option 3: From Source

```bash
git clone https://github.com/sarmkadan/dotnet-grpc-gateway.git
cd dotnet-grpc-gateway

dotnet test  # Run tests
dotnet run --project src/dotnet-grpc-gateway
```

## Configuration

The gateway is configured via `appsettings.json`:

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
    "RequestTimeoutMs": 30000
  }
}
```

## API Reference

### Service Management
- `GET /api/gateway/services` - List all services
- `POST /api/gateway/services` - Register new service
- `GET /api/gateway/services/{id}` - Get service details
- `DELETE /api/gateway/services/{id}` - Unregister service

### Route Management
- `GET /api/gateway/routes` - List all routes
- `POST /api/gateway/routes` - Add new route
- `DELETE /api/gateway/routes/{id}` - Remove route

### Health & Metrics
- `GET /health` - Overall health status
- `GET /api/health/services` - Per-service health
- `GET /api/metrics/performance` - Performance metrics
- `GET /api/metrics/today` - Today's statistics

## Development

### Prerequisites
- .NET 10 SDK
- PostgreSQL 14+
- Git

### Build & Test

```bash
dotnet restore
dotnet build -c Release
dotnet test
```

### Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for contribution guidelines.

## License

MIT License - Copyright (c) 2026 Vladyslav Zaiets

See [LICENSE](LICENSE) for full details.
