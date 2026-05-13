# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-10-08

### Added
- **Comprehensive Documentation**: Full API reference, deployment guide, architecture documentation
- **Docker Support**: Multi-stage Dockerfile with health checks and security hardening
- **Docker Compose**: Development setup with PostgreSQL integration
- **CI/CD Pipeline**: GitHub Actions workflow with build, test, lint, and security scanning
- **Kubernetes Manifests**: Production-ready deployment configurations with HPA
- **Examples**: 8 complete example applications demonstrating common patterns
- **FAQ**: 50+ frequently asked questions and answers
- **EditorConfig**: Code style configuration for consistency across IDEs
- **Advanced Route Management**: Route conflict detection and analysis
- **Webhook Integration**: Send notifications on events with retry logic
- **Performance Monitoring**: Real-time P50/P95/P99 latency tracking
- **Event System**: Pub-sub message bus for decoupled events
- **Output Formatters**: JSON, CSV, XML output format support
- **Request Correlation**: Async-safe request tracking with correlation IDs
- **Background Services**:
  - Health check background service (30-second intervals)
  - Metrics aggregation service (60-second intervals)
  - Cache expiration monitor
  - Service cleanup service
- **Middleware Suite**:
  - Request logging with response time tracking
  - Rate limiting with token bucket algorithm
  - Authentication with bearer tokens

### Changed
- Updated README with comprehensive guide
- Improved error messages with more context
- Enhanced logging with structured fields
- Optimized Docker image size (multi-stage build)
- Refactored service registration to use event-driven approach
- Improved health check reliability with configurable thresholds
- Enhanced metrics storage with date-partitioned queries

### Fixed
- Fixed memory leak in metrics collection for long-running instances
- Fixed race condition in health check updates
- Fixed potential null reference in webhook service
- Fixed connection pool exhaustion under high load
- Fixed metrics calculation for concurrent requests
- Fixed race condition in route matching

### Known Limitations
- Single-region deployment (multi-region support planned)
- PostgreSQL only (MySQL support planned)
- In-memory caching only (Redis support planned)

---

## [0.5.0] - 2025-08-12

### Added
- **REST API Endpoints** (20+ endpoints)
  - Service management
  - Route configuration
  - Metrics and statistics
  - Health checks
  - Readiness/liveness probes
- **Infrastructure**
  - Structured logging with Serilog
  - Global error handling middleware
  - Configuration management
  - Dependency injection setup
- **Utilities**
  - String manipulation helpers
  - HTTP header parsing
  - DateTime utilities
  - Validation utilities
  - JSON serialization helpers
  - Configuration utilities

### Changed
- Stabilized public API surface for 1.0.0 release
- Improved request validation error messages
- Expanded test coverage to >80%

### Fixed
- Fixed edge case in wildcard route matching with nested patterns
- Fixed incorrect status code mapping for upstream gRPC errors

---

## [0.4.0] - 2025-06-19

### Added
- **Security Features**
  - Bearer token authentication
  - Per-route authorization policies
  - Rate limiting per IP address
  - CORS support
- **Performance Features**
  - In-memory response caching with TTL
  - Connection pooling
  - Configurable request timeouts
- Cache statistics endpoint

### Changed
- Rate limiting moved to dedicated middleware
- Authentication token validation extracted to service

### Fixed
- Fixed token expiry not being checked on each request
- Fixed CORS preflight handling for gRPC-Web requests

---

## [0.3.0] - 2025-04-30

### Added
- **Service Discovery**
  - Automatic service registration and deregistration
  - Real-time health checks with configurable intervals
  - Per-service health reports
- **Metrics & Analytics**
  - Per-request metrics collection
  - Latency percentiles (P50, P95, P99)
  - Error rate tracking
  - 30-day retention
- **Data Persistence**
  - PostgreSQL repository layer
  - Dapper-based data access
  - Unit of Work pattern for transactions

### Changed
- Health check interval made configurable per service (default 30 s)
- Metrics storage schema revised for efficient date-range queries

### Fixed
- Fixed service not marked unhealthy after consecutive failures
- Fixed duplicate metric rows on concurrent requests

---

## [0.2.0] - 2025-03-10

### Added
- **Core Gateway Functionality**
  - gRPC-Web protocol support for browser clients
  - HTTP/1.1 to gRPC translation
  - Request routing with pattern matching
  - Priority-based route selection
  - Dynamic route updates without restart

### Changed
- Project renamed from `grpc-proxy` to `dotnet-grpc-gateway`
- Restructured into layered architecture (Domain / Services / Infrastructure)

### Fixed
- Fixed protocol framing error for gRPC-Web trailers
- Fixed route priority not respected when multiple patterns matched

---

## [0.1.0] - 2025-01-27

### Added
- Initial project structure
- Basic gRPC gateway skeleton
- Initial domain models (GrpcService, GatewayRoute, GatewayConfiguration)
- PostgreSQL integration setup
- Solution file and project references

---

## Unreleased

### Planned Features
- [ ] Redis caching backend
- [ ] MySQL/MariaDB support
- [ ] Multi-region federation
- [ ] GraphQL to gRPC translation
- [ ] OpenAPI/Swagger documentation
- [ ] Distributed tracing (Jaeger/Zipkin)
- [ ] Metrics export (Prometheus)
- [ ] Advanced authorization (OIDC/OAuth2)
- [ ] Circuit breaker pattern
- [ ] Request/response transformers
- [ ] WebSocket support
- [ ] gRPC streaming optimization

### Under Consideration
- [ ] Web UI for configuration management
- [ ] Automatic route discovery from reflection
- [ ] Service mesh integration

---

## Migration Guide

### From 0.5.0 to 1.0.0

No breaking changes. Update is safe:

```bash
git pull origin main
dotnet build -c Release
# Restart the gateway
```

### From 0.4.0 to 0.5.0

Minor database schema changes for new fields:

```bash
# Apply migrations
dotnet run --project src/dotnet-grpc-gateway -- migrate

# Restart the gateway
```

### From 0.1.0 to 0.2.0

Breaking changes:
- Configuration format updated
- Some API endpoints renamed
- Database schema changes

See [MIGRATION.md](docs/MIGRATION.md) for detailed steps.

---

## Contributors

Special thanks to all contributors:
- [Vladyslav Zaiets](https://github.com/Sarmkadan) - Original author and maintainer

## Support

For issues and questions:
- [Documentation](https://github.com/Sarmkadan/dotnet-grpc-gateway)
- [Issue Tracker](https://github.com/Sarmkadan/dotnet-grpc-gateway/issues)
- [Discussions](https://github.com/Sarmkadan/dotnet-grpc-gateway/discussions)
