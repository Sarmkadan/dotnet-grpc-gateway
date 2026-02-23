# Migration Guide: v1.x to v2.0

This guide covers breaking changes and required steps when upgrading from v1.x to v2.0.

## Breaking Changes

### Port Change: 5000 -> 8080

The default listening port has changed from `5000` to `8080` to align with container runtime conventions (non-root, unprivileged ports).

**Action required:**
- Update any reverse proxy configs (Caddy, nginx, Traefik) pointing to port 5000
- Update Docker port mappings if using custom compose overrides
- Update health check URLs if monitored externally
- Update `ASPNETCORE_URLS` if set explicitly in your environment

```yaml
# Before (v1.x)
ports:
  - "5000:5000"

# After (v2.0)
ports:
  - "8080:8080"
```

### Streaming Gateway Service

v2.0 introduces `IStreamingGatewayService` - a new abstraction for bidirectional gRPC streaming support. If you have custom gateway implementations that override core interfaces, you may need to implement this new interface.

### Docker Compose

- Removed deprecated `version` field (Docker Compose v2+ no longer requires it)
- PostgreSQL upgraded from `15-alpine` to `16-alpine`
- Database password now uses `${POSTGRES_PASSWORD:-postgres}` env variable (defaults to `postgres` for backward compatibility)
- New `Gateway__EnableStreaming` environment variable (defaults to `true`)

## Upgrade Steps

### 1. Update source

```bash
git pull origin main
```

### 2. Rebuild

```bash
dotnet build -c Release
```

### 3. Update Docker (if using containers)

```bash
docker compose down
docker compose build --no-cache
docker compose up -d
```

### 4. Update external references

Replace any hardcoded references to port `5000` with `8080`:

```bash
# Find references in your config files
grep -r "5000" /path/to/your/config/
```

### 5. Verify

```bash
curl http://localhost:8080/health
```

## New Features in v2.0

- **gRPC Streaming**: Bidirectional streaming support via `IStreamingGatewayService`
- **Improved Docker setup**: Non-root user, unprivileged port, env-based DB password
- **PostgreSQL 16**: Upgraded from 15 for performance improvements

## Rollback

If you need to roll back to v1.x:

```bash
git checkout v1.0.0
docker compose down
docker compose build --no-cache
docker compose up -d
```

Ensure you revert any port changes in your reverse proxy configuration.
