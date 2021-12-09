# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2026-05-04

### Added
- **Comprehensive Documentation**: Full API reference, deployment guide, architecture documentation
- **Docker Support**: Multi-stage Dockerfile with health checks and security hardening
- **Docker Compose**: Development setup with PostgreSQL integration
- **CI/CD Pipeline**: GitHub Actions workflow with build, test, lint, and security scanning
- **Kubernetes Manifests**: Production-ready deployment configurations with HPA
- **Examples**: 8 complete example applications demonstrating common patterns
- **FAQ**: 50+ frequently asked questions and answers
- **EditorConfig**: Code style configuration for consistency across IDEs

### Changed
- Updated README with comprehensive 2000+ word guide
- Improved error messages with more context
- Enhanced logging with structured fields
- Optimized Docker image size (multi-stage build)

### Fixed
- Fixed memory leak in metrics collection for long-running instances
- Fixed race condition in health check updates
- Fixed potential null reference in webhook service

## [1.1.0] - 2026-04-20

### Added
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
- Refactored service registration to use event-driven approach
- Improved health check reliability with configurable thresholds
- Enhanced metrics storage with date-partitioned queries
- Updated domain models with richer metadata

### Fixed
- Fixed connection pool exhaustion under high load
- Fixed metrics calculation for concurrent requests
- Fixed race condition in route matching

## [1.0.0] - 2026-03-15

### Added
- **Core Gateway Functionality**
  - gRPC-Web protocol support for browser clients
  - HTTP/1.1 to gRPC translation
  - Request routing with pattern matching
  - Dynamic service registration and deregistration
  - Real-time health monitoring
  - Metrics collection and statistics
  
- **Security Features**
  - Bearer token authentication
  - Per-route authorization
  - Rate limiting per IP address
  - CORS support
  
- **Performance Features**
  - In-memory response caching with TTL
  - Connection pooling
  - Configurable request timeouts
  
- **Data Persistence**
  - PostgreSQL repository layer
  - Entity Framework Core integration
  - Unit of Work pattern for transactions
  
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

### Known Limitations
- Single-region deployment (multi-region support planned)
- PostgreSQL only (MySQL support planned)
- In-memory caching only (Redis support planned)

---

## [0.1.0] - 2026-02-01

### Added
- Initial project structure
- Basic gRPC gateway skeleton
- Initial domain models
- PostgreSQL integration setup

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
- [ ] Machine learning-based anomaly detection
- [ ] Automatic route discovery from reflection
- [ ] Service mesh integration
- [ ] DynamoDB/CosmosDB support

---

## Migration Guide

### From 1.1.0 to 1.2.0

No breaking changes. Update is safe:

```bash
git pull origin main
dotnet build -c Release
# Restart the gateway
```

### From 1.0.0 to 1.1.0

Minor database schema changes for new fields:

```bash
# Apply migrations
dotnet run --project src/dotnet-grpc-gateway -- migrate

# Restart the gateway
```

### From 0.1.0 to 1.0.0

Breaking changes:
- Configuration format updated
- Some API endpoints renamed
- Database schema changes

See [MIGRATION.md](docs/MIGRATION.md) for detailed steps.

---

## Release Schedule

- **Major Releases** (v2.0.0, v3.0.0): Twice yearly (June, December)
- **Minor Releases** (v1.1.0, v1.2.0): Monthly
- **Patch Releases** (v1.0.1, v1.0.2): As needed for critical bugs

---

## Contributors

Special thanks to all contributors:
- [Vladyslav Zaiets](https://github.com/Sarmkadan) - Original author and maintainer

## Support

For issues and questions:
- 📚 [Documentation](https://github.com/Sarmkadan/dotnet-grpc-gateway)
- 🐛 [Issue Tracker](https://github.com/Sarmkadan/dotnet-grpc-gateway/issues)
- 💬 [Discussions](https://github.com/Sarmkadan/dotnet-grpc-gateway/discussions)
