# Getting Started with dotnet-grpc-gateway

This guide will help you set up and run dotnet-grpc-gateway in under 5 minutes.

## Prerequisites

- **.NET 10 SDK** or later ([download](https://dotnet.microsoft.com/download))
- **PostgreSQL 14+** ([download](https://www.postgresql.org/download))
- **Docker** (optional, for containerized setup)
- **Git** for version control
- **curl** or Postman for testing APIs

## Installation

### Quick Start (Docker Compose)

The fastest way to get started is using Docker Compose:

```bash
# 1. Clone repository
git clone https://github.com/Sarmkadan/dotnet-grpc-gateway.git
cd dotnet-grpc-gateway

# 2. Start services
docker-compose up -d

# 3. Verify installation
curl http://localhost:5000/health
```

The gateway is now running at `http://localhost:5000` with PostgreSQL at `localhost:5432`.

### Manual Installation

If you prefer to install components separately:

```bash
# 1. Clone repository
git clone https://github.com/Sarmkadan/dotnet-grpc-gateway.git
cd dotnet-grpc-gateway

# 2. Restore NuGet packages
dotnet restore

# 3. Build the project
dotnet build -c Release

# 4. Create PostgreSQL database
createdb grpc_gateway

# 5. Set connection string
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=grpc_gateway;Username=postgres;Password=postgres"

# 6. Start the gateway
dotnet run --project src/dotnet-grpc-gateway -c Release
```

## Configuration

### Basic Configuration

Edit `src/dotnet-grpc-gateway/appsettings.json`:

```json
{
  "Gateway": {
    "Port": 5000,
    "MaxConcurrentConnections": 1000,
    "RequestTimeoutMs": 30000,
    "HealthCheck": {
      "IntervalSeconds": 30,
      "TimeoutMs": 5000
    }
  }
}
```

### Environment Variables

```bash
# Set custom port
export Gateway__Port=8080

# Set production database
export ConnectionStrings__DefaultConnection="Host=prod-db.example.com;..."

# Enable verbose logging
export Logging__LogLevel__Default=Debug
```

## Your First Service Registration

### Step 1: Verify Gateway is Running

```bash
curl http://localhost:5000/health
# Response: {"status":"Healthy"}
```

### Step 2: Register a gRPC Service

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

**Response:**
```json
{
  "id": 1,
  "name": "UserService",
  "host": "localhost",
  "port": 5001,
  "isHealthy": false
}
```

Note: Service shows as unhealthy because it's not running yet.

### Step 3: Verify Service Registration

```bash
# List all services
curl http://localhost:5000/api/gateway/services

# Get specific service
curl http://localhost:5000/api/gateway/services/1

# Check health of all services
curl http://localhost:5000/api/health/services
```

### Step 4: Add a Route

Routes determine how requests are matched and forwarded to services.

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

**Parameters:**
- `pattern`: Route pattern (wildcard supported)
- `targetServiceId`: ID of service to route to
- `matchType`: 0=Exact, 1=Prefix, 2=Regex
- `priority`: Higher number = higher priority
- `rateLimitPerMinute`: Rate limit for this route
- `enableCaching`: Enable response caching
- `cacheDurationSeconds`: Cache TTL

### Step 5: Test the Route

```bash
# List all routes
curl http://localhost:5000/api/gateway/routes

# Test route matching
curl -X POST http://localhost:5000/api/servicediscovery/route-match \
  -H "Content-Type: application/json" \
  -d '{
    "serviceName": "user.UserService",
    "methodName": "GetUser"
  }'
```

## Running a Local gRPC Service

To test the gateway with a real gRPC service, here's a minimal example:

### Create a Test Service

```bash
# Create new gRPC project
dotnet new grpc -n TestGrpcService
cd TestGrpcService

# Update Program.cs to listen on port 5001
# Change: app.Urls.Add("http://localhost:5001");
```

### Test End-to-End

```bash
# Terminal 1: Start gateway
cd dotnet-grpc-gateway
docker-compose up -d

# Terminal 2: Start your gRPC service
cd TestGrpcService
dotnet run

# Terminal 3: Test the flow
# Service should now show as healthy
curl http://localhost:5000/api/gateway/services/1
```

## Monitoring and Debugging

### Check Gateway Logs

```bash
# View logs
tail -f logs/gateway-*.txt

# Or through Docker
docker logs dotnet-grpc-gateway
```

### Performance Metrics

```bash
# Get performance statistics
curl http://localhost:5000/api/metrics/performance

# Get slow requests
curl http://localhost:5000/api/metrics/slow?threshold=500

# Average response time
curl http://localhost:5000/api/gateway/metrics/average-response-time
```

### Service Health Details

```bash
# Overall health
curl http://localhost:5000/api/health/status

# Per-service health
curl http://localhost:5000/api/health/services

# Readiness (for load balancers)
curl http://localhost:5000/api/health/ready

# Liveness (for Kubernetes)
curl http://localhost:5000/api/health/live
```

## Common Tasks

### Enable Authentication

```bash
# Create authentication token
curl -X POST http://localhost:5000/api/gateway/auth/tokens \
  -H "Content-Type: application/json" \
  -d '{
    "token": "my-secret-key-12345",
    "description": "API Client",
    "expiresAt": "2025-12-31T23:59:59Z"
  }'

# Add protected route
curl -X POST http://localhost:5000/api/gateway/routes \
  -H "Content-Type: application/json" \
  -d '{
    "pattern": "admin.*",
    "targetServiceId": 1,
    "matchType": 1,
    "priority": 200,
    "requiresAuthentication": true,
    "rateLimitPerMinute": 100
  }'

# Call protected endpoint with token
curl http://localhost:5000/admin/sensitive \
  -H "Authorization: Bearer my-secret-key-12345"
```

### Export Metrics

```bash
# Export today's statistics as JSON
curl http://localhost:5000/api/gateway/statistics/today \
  -H "Accept: application/json"

# Export as CSV (if configured)
curl http://localhost:5000/api/gateway/statistics/today \
  -H "Accept: text/csv"
```

### Backup Configuration

```bash
# Backup all services and routes
curl http://localhost:5000/api/gateway/services > services.json
curl http://localhost:5000/api/gateway/routes > routes.json

# Backup PostgreSQL
pg_dump grpc_gateway > backup.sql
```

## Troubleshooting

### Gateway won't start

**Error**: `Connection string 'DefaultConnection' not found.`

**Fix**: Set environment variable:
```bash
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=grpc_gateway;Username=postgres;Password=postgres"
```

### Service stays unhealthy

**Check**:
1. Is the gRPC service running?
2. Can you reach it directly?
   ```bash
   grpcurl -plaintext localhost:5001 list
   ```
3. Check firewall rules
4. Verify host/port configuration

### High memory usage

**Solutions**:
1. Reduce metrics retention: Change `RetentionDays` from 30 to 7
2. Lower `MaxConcurrentConnections`
3. Reduce cache size

### Rate limiting not working

**Check**:
1. Is `RateLimitPerMinute` set on the route?
2. Is RateLimitingMiddleware added to pipeline?
3. Are you hitting the rate limit? (needs 1000+ requests/minute for default limit)

## Next Steps

1. **Read the Architecture Guide**: [ARCHITECTURE.md](ARCHITECTURE.md)
2. **Explore API Reference**: [API-REFERENCE.md](API-REFERENCE.md)
3. **Deploy to Production**: [DEPLOYMENT.md](DEPLOYMENT.md)
4. **Check Examples**: See `examples/` directory

## Getting Help

- 📚 Read the [FAQ](FAQ.md)
- 🐛 Report issues: [GitHub Issues](https://github.com/Sarmkadan/dotnet-grpc-gateway/issues)
- 💬 Discuss: [GitHub Discussions](https://github.com/Sarmkadan/dotnet-grpc-gateway/discussions)
