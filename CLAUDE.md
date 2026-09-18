# CLAUDE.md

## Overview
gRPC-Web gateway for .NET 10 (ASP.NET Core): exposes backend gRPC services to browsers without Envoy, with service discovery, load balancing, circuit breakers, metrics and request logging (PostgreSQL via Npgsql + Dapper).

## Build
```bash
dotnet restore dotnet-grpc-gateway.sln
dotnet build dotnet-grpc-gateway.sln -c Release --no-restore   # or: make build
dotnet run --project src/dotnet-grpc-gateway/dotnet-grpc-gateway.csproj   # or: make run-dev
docker build -t dotnet-grpc-gateway:latest . && docker-compose up -d       # or: make docker-run
```
Requires connection string `ConnectionStrings:DefaultConnection` (see `src/dotnet-grpc-gateway/appsettings.example.json`).

## Test
```bash
dotnet test dotnet-grpc-gateway.sln -c Release          # or: make test
dotnet test dotnet-grpc-gateway.sln --collect:"XPlat Code Coverage"   # make coverage
dotnet test --filter "FullyQualifiedName~CircuitBreakerTests"           # single class
```
xUnit + FluentAssertions + Moq. Benchmarks: `dotnet run -c Release --project benchmarks/dotnet-grpc-gateway.Benchmarks`.

## Lint / format
```bash
dotnet format dotnet-grpc-gateway.sln                       # make format
dotnet format dotnet-grpc-gateway.sln --verify-no-changes   # make lint
dotnet build /p:EnforceCodeStyleInBuild=true                # make analyze
```
Style rules live in `.editorconfig` (4-space indent, `var` when type apparent, expression-bodied properties, pattern matching preferred).

## Key directories
- `src/dotnet-grpc-gateway/` - main project (`DotNetGrpcGateway` namespace, `Program.cs` is the entry point; DI wiring via `Configuration/ServiceCollectionExtensions.cs`)
  - `Controllers/` - REST management API (gateway, health, metrics, load balancer, circuit breaker, reflection, service discovery, request logs)
  - `Domain/` - models (`GatewayRoute`, `GrpcService`, `ServiceEndpoint`, `RequestMetric`, ...)
  - `Services/` - service interfaces + background services (health checks, metrics aggregation/persistence, cache expiration, cleanup)
  - `Infrastructure/` - circuit breaker, retry policy, repositories/UoW interfaces, `RequestContext`, `StructuredLogger`, `ErrorHandlingMiddleware`
  - `Middleware/` - auth, rate limiting, gRPC-Web compression/trailer forwarding, request logging
  - `Caching/`, `Events/`, `Formatters/`, `Streaming/`, `Options/`, `Extensions/`, `Utilities/`, `Constants/`
  - `GlobalUsings.cs` - global usings for Domain/Services/Infrastructure/Exceptions/Constants
- `tests/dotnet-grpc-gateway.Tests/` - unit tests (flat, one file per type under test)
- `benchmarks/dotnet-grpc-gateway.Benchmarks/` - BenchmarkDotNet
- `examples/` - usage samples and docker deployment
- `docs/` - ARCHITECTURE.md, LOAD-BALANCING.md, per-type reference docs
- `.github/workflows/` - ci, build, codeql, docker, nuget-publish, release

## Conventions
- Every `.cs` file starts with `#nullable enable` and the author header comment block; file-scoped namespaces.
- Nullable and ImplicitUsings enabled; Serilog for logging (`ILogger<T>` injected).
- Naming: interfaces `I*`, private fields `_camelCase`, background services `*BackgroundService`, controllers `*Controller`.
- Helper logic is split into partial-style companion files: `Foo.cs`, `FooExtensions.cs`, `FooValidation.cs`, `FooJsonExtensions.cs`.
- Tests: class `FooTests`, methods `Method_Scenario_Expected` or `State_Expected`, `CreateSut()` factory, `[Fact]`/`[Theory]`, FluentAssertions `.Should()`.
- Commits follow Conventional Commits (`feat:`, `fix:`, `docs:`, `chore:`); see CONTRIBUTING.md.
