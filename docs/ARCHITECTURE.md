# Architecture

This document describes how dotnet-grpc-gateway is actually built - the layout of the
single ASP.NET Core project, the request pipeline, the main abstractions, and the
decisions behind them (including the compromises). If something here disagrees with
the code, the code wins; fix the doc.

## What it is

A self-hosted gRPC-Web gateway: browsers speak gRPC-Web over HTTP/1.1 to this
service, which manages a registry of backend gRPC services and routes, and exposes a
REST management/observability API on top. It replaces the "run Envoy in front of
your gRPC services" setup with a single .NET process.

Everything lives in one project: `src/dotnet-grpc-gateway` (`net10.0`, root namespace
`DotNetGrpcGateway`). There is no separate Domain/Application/Infrastructure project
split - the layering is by folder, which is deliberate: the deployable unit is one
gateway process, and a multi-project split would buy compile-time enforcement of
layering at the cost of ceremony that a project this size does not need yet.

## Project layout

```
src/dotnet-grpc-gateway/
├── Program.cs            # composition root: DI, middleware pipeline, Serilog
├── Configuration/        # ServiceCollectionExtensions (AddGatewayServices etc.)
├── Controllers/          # REST management API (gateway, metrics, health, ...)
├── Domain/               # entities: GrpcService, GatewayRoute, RequestMetric, ...
├── Services/             # application services + hosted background services
├── Infrastructure/       # repositories, circuit breaker, perf monitor, logging
├── Middleware/           # auth, rate limiting, logging, gRPC-Web helpers
├── Events/               # in-process pub-sub (IEventPublisher + handlers)
├── Caching/              # ICacheService over IMemoryCache
├── Formatters/           # JSON/CSV/XML output formatters + factory
├── Streaming/            # streaming session abstractions
├── Integration/          # webhooks, HttpClient provider
├── Options/              # DotnetGrpcGatewayOptions (bound from configuration)
├── Exceptions/           # GatewayException hierarchy
└── Utilities/, Extensions/, Constants/
```

Tests live in `tests/dotnet-grpc-gateway.Tests`, benchmarks (BenchmarkDotNet) in
`benchmarks/`, runnable examples in `examples/`.

## Composition root and pipeline

`Program.cs` is the single composition root. DI registrations are grouped into
extension methods in `Configuration/ServiceCollectionExtensions.cs`
(`AddGatewayServices`, `AddGatewayConfiguration`, `AddGatewayHealthChecks`,
`AddGatewayReflection`) so Program.cs stays a readable table of contents.

The middleware pipeline, in order:

```
ErrorHandlingMiddleware              # outermost - catches everything below
UseRouting
RequestLoggingMiddleware
RateLimitingMiddleware
UseAuthentication / UseAuthorization # "ApiKey" scheme
UseCors("GrpcWebPolicy")
UseGrpcWeb                           # Grpc.AspNetCore.Web protocol translation
GrpcWebCompressionMiddleware
GrpcWebTrailerForwardingMiddleware
RequestResponseCapturingMiddleware
MapControllers
MapHealthChecks("/health")
```

Rationale for the ordering:

- **Error handling is first** so it wraps every other middleware. It converts the
  `GatewayException` hierarchy into structured error responses and is the only place
  that turns unhandled exceptions into HTTP 500s.
- **Rate limiting runs before authentication** on purpose: rejecting a flooded
  client should not cost a token lookup per request.
- **CORS before UseGrpcWeb**: the browser's preflight has to be answered before any
  protocol translation happens, and the policy explicitly exposes the
  `Grpc-Status` / `Grpc-Message` / trailer headers gRPC-Web clients need.
- The two custom gRPC-Web middlewares (compression, trailer forwarding) exist
  because the stock `UseGrpcWeb` does not compress gRPC-Web bodies and some
  HTTP/1.1 clients lose trailers.

Logging is Serilog (console + daily rolling file under `logs/`), configured before
the host builds so startup failures are captured too.

## Core abstractions

Registered in `AddGatewayServices` (see `ServiceCollectionExtensions.cs`):

| Abstraction | Role |
|---|---|
| `IGatewayService` | orchestrates register/unregister services, add/remove routes |
| `IServiceRegistry`, `IRouteRepository`, `IGatewayRepository`, `IMetricsRepository` | persistence of services / routes / gateway config / metrics |
| `IUnitOfWork` | groups the four repositories behind a transaction-shaped API |
| `IRouteResolutionService` | matches an incoming path to a `GatewayRoute` (exact / prefix / regex, priority-ordered) |
| `IServiceDiscoveryService` | health-checks registered backends |
| `IReflectionService` | discovers backend service surface via gRPC Server Reflection |
| `IGrpcClientFactory` | creates/caches gRPC channels per backend (typed HttpClient) |
| `ILoadBalancerService` | picks an endpoint per service; strategies incl. RoundRobin (default), LeastConnections, Random, Weighted |
| `ICircuitBreakerRegistry` / `ICircuitBreaker` | per-service circuit breaker (closed/open/half-open, tunables in `CircuitBreakerOptions`) |
| `IMetricsCollectionService`, `IRequestMetricsAnalyzerService` | record and analyze `RequestMetric`s |
| `IRequestLogService` | fixed-size ring buffer (10 000 entries) of request/response captures |
| `ICacheService` | thin wrapper over `IMemoryCache` with stats |
| `IEventPublisher` | in-process pub-sub for `GatewayEvent`s |
| `IValidationService` | FluentValidation-based input validation |
| `IStreamingGatewayService` | bidirectional streaming sessions with backpressure (see `Streaming/`) |

The REST surface (`Controllers/`) sits on top of these: `GatewayController`
(services/routes/config CRUD), `MetricsController`, `ServiceDiscoveryController`,
`ReflectionController`, `LoadBalancerController`, `CircuitBreakerController`,
`RequestLogsController`, `HealthController`. Responses can be shaped as JSON, CSV
or XML via `OutputFormatterFactory`.

### Background services

Four `IHostedService` workers, all with deliberate startup delays so they never
compete with request warm-up:

- `HealthCheckBackgroundService` - `PeriodicTimer`, interval from
  `DotnetGrpcGatewayOptions.HealthCheck.IntervalSeconds`; pings each registered
  backend and publishes `ServiceHealthCheckFailedEvent` on failure.
- `MetricsAggregationBackgroundService` - rolls raw `RequestMetric`s into
  `GatewayStatistics`.
- `CacheExpirationBackgroundService` - sweeps the cache every 10 minutes.
- `ServiceCleanupBackgroundService` - hourly, removes long-dead registrations.

Background singletons reach scoped services through DI scopes - they never capture
scoped instances directly.

### Event system

`IEventPublisher` is a simple in-process dispatcher; handlers implement
`IEventHandler<TEvent>` and are resolved from DI (one handler class per event type
under `Events/EventHandlers/`). Events cover service registration/unregistration,
route add/remove, health-check failures, config updates and throttling. This is
observer-pattern decoupling, not a message bus: publishing is in-process and
best-effort. If you need durable or cross-instance events, that is an extension
point - do not bolt it onto `EventPublisher`.

## Persistence: the honest part

**The repositories are in-memory.** `GatewayRepository`, `ServiceRegistry`,
`RouteRepository` and `MetricsRepository` each keep a `Dictionary<int, T>` and hand
out `Task.FromResult`. They take an `IConnectionStringProvider` in the constructor,
and the csproj references Npgsql + Dapper, but no SQL is executed anywhere yet.
`UnitOfWork.CommitAsync` / `RollbackAsync` are likewise no-ops shaped like a
transaction API.

Consequences you must know:

- **State does not survive a restart.** Registered services and routes are gone
  when the process dies. `docker-compose.yml` provisions PostgreSQL and passes a
  connection string, but today that database stores nothing.
- The repositories are registered as **singletons** precisely because they are
  dictionary-backed - a scoped lifetime would give each request a fresh empty
  store. When a Dapper/Npgsql implementation lands, switch them back to scoped and
  give `UnitOfWork` a real `NpgsqlTransaction`; the interfaces were shaped for
  that from the start, which is why `IConnectionStringProvider` is already
  threaded through.
- Multi-instance deployment behind a load balancer is **not** currently viable for
  the management plane: each instance would have its own registry. The data plane
  (proxying) works if all instances are configured identically.

This is a known, accepted trade-off: the interfaces define the contract, the
storage engine is swappable, and in-memory was the cheapest way to make the whole
gateway testable end-to-end without a database.

## Data flow

Proxied request (gRPC-Web from a browser):

```
client → ErrorHandling → RequestLogging → RateLimiting → Auth → CORS
       → gRPC-Web translation (+compression, trailer forwarding)
       → RouteResolutionService (exact/prefix/regex, priority)
       → LoadBalancerService picks endpoint
       → CircuitBreakerRegistry gate
       → GrpcClientFactory channel → backend gRPC service
       → RequestResponseCapturing stores entry in ring buffer
       → MetricsCollectionService records RequestMetric
```

Management request (REST): same pipeline until routing, then a controller calls the
matching application service, which goes through `IUnitOfWork` / repositories and
may publish a `GatewayEvent` (e.g. `RouteAddedEvent`) whose handlers do
logging/webhook side effects.

## Cross-cutting decisions and trade-offs

- **API-key authentication only** (`AddApiKeyAuthentication`, "ApiKey" scheme).
  Chosen over JWT/OIDC because the management API is meant to sit behind an
  internal network edge; tokens are modeled by `AuthenticationToken` in Domain.
  Trade-off: no per-user identity or claims - add a proper OIDC scheme if the API
  is ever exposed publicly.
- **In-process rate limiting** (`RateLimitingMiddleware`, per-client counters).
  Fine for one instance; not coordinated across instances (see persistence note).
- **10 MB gRPC message caps** in both directions - a guardrail against a browser
  client accidentally streaming a file through the gateway. Raise consciously.
- **`PerformanceMonitor` keeps latency percentiles in fixed-size in-memory
  buffers** - constant memory, but percentiles reflect the recent window only.
- **`RequestLogService` is a bounded ring buffer (10 000 entries)**, not a log
  store: it exists for "what just happened" debugging via `RequestLogsController`,
  not for audit. Old entries are overwritten by design.
- **Explicit DI registration over assembly scanning**: registrations are grouped
  in extension methods, explicit and greppable; the cost is remembering to add new
  services by hand.

## Extension points

- **Storage**: implement the four repository interfaces against PostgreSQL
  (Npgsql/Dapper are already referenced), register them scoped, give `UnitOfWork`
  a real transaction. No caller changes needed.
- **Load balancing**: add a member to `LoadBalancingStrategy` and a case in
  `LoadBalancerService`.
- **Events**: implement `IEventHandler<TEvent>` and register it; the publisher
  resolves all handlers for the event type.
- **Output formats**: implement `IOutputFormatter` and extend
  `OutputFormatterFactory`.
- **Middleware**: standard ASP.NET Core `UseMiddleware<T>` - register anything
  that must have its failures translated *after* `ErrorHandlingMiddleware`.
- **Discovery**: `IReflectionService` wraps gRPC Server Reflection; alternative
  discovery (Consul, DNS) belongs behind `IServiceDiscoveryService`.

## Known limitations

- No durable storage (see above) - a restart wipes registry, routes and metrics.
- Single-instance management plane; no distributed rate limiting or shared
  circuit-breaker state.
- Health checking is poll-based (no push/watch), interval-configurable.
- `UnitOfWork` transactions are advisory no-ops until real persistence lands.
- The `docs/` folder contains generated per-type reference pages of varying
  quality; this file plus `GETTING-STARTED.md` and `DEPLOYMENT.md` are the curated
  entry points.
