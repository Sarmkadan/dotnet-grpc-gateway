#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net;
using DotNetGrpcGateway.Caching;
using DotNetGrpcGateway.Configuration;
using DotNetGrpcGateway.Events;
using DotNetGrpcGateway.Events.EventHandlers;
using DotNetGrpcGateway.Formatters;
using DotNetGrpcGateway.Infrastructure;
using DotNetGrpcGateway.Integration;
using DotNetGrpcGateway.Middleware;
using DotNetGrpcGateway.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using DotNetGrpcGateway.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for structured logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/gateway-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    var services = builder.Services;

    // Configuration
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                          throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    services.AddSingleton<IConnectionStringProvider>(
        new ConnectionStringProvider(connectionString));

    // Add gateway services using extension methods
    services.AddGatewayServices();
    services.AddGatewayConfiguration(builder.Configuration);
    services.AddGatewayHealthChecks();
    services.AddGatewayReflection();

    // Phase 2: Add new services and infrastructure

    // Caching layer
    services.AddMemoryCache();
    services.AddSingleton<ICacheService, MemoryCacheService>();

    // Performance monitoring
    services.AddSingleton<IPerformanceMonitor, PerformanceMonitor>();

    // Event system
    services.AddSingleton<IEventPublisher, EventPublisher>();
    services.AddScoped<IEventHandler<ServiceRegisteredEvent>, ServiceRegisteredEventHandler>();
    services.AddScoped<IEventHandler<ServiceUnregisteredEvent>, ServiceUnregisteredEventHandler>();
    services.AddScoped<IEventHandler<RouteAddedEvent>, RouteAddedEventHandler>();
    services.AddScoped<IEventHandler<RouteRemovedEvent>, RouteRemovedEventHandler>();
    services.AddScoped<IEventHandler<ServiceHealthCheckFailedEvent>, ServiceHealthCheckFailedEventHandler>();
    services.AddScoped<IEventHandler<ConfigurationUpdatedEvent>, ConfigurationUpdatedEventHandler>();
    services.AddScoped<IEventHandler<RequestThrottledEvent>, RequestThrottledEventHandler>();

    // Output formatters
    services.AddSingleton<OutputFormatterFactory>();

    // Integration services
    services.AddHttpClient<IWebhookService, WebhookService>();
    services.AddSingleton<IHttpClientProvider, HttpClientProvider>();

    // Advanced route management
    services.AddScoped<IRouteManagementService, RouteManagementService>();

    // Metrics analysis
    services.AddScoped<IRequestMetricsAnalyzerService, RequestMetricsAnalyzerService>();

    // Load balancing
    services.AddSingleton<ILoadBalancerService, LoadBalancerService>();

    // Circuit breaker registry
    services.AddSingleton<ICircuitBreakerRegistry, CircuitBreakerRegistry>();

// Circuit breaker service for protected gRPC invocations
services.AddSingleton<ICircuitBreakerService, CircuitBreakerService>();

    // Request/response log store (10 000 entry ring buffer)
    services.AddSingleton<IRequestLogService>(new RequestLogService(10_000));

    // Request context

    // Authentication
    services.AddAuthentication("ApiKey").AddApiKeyAuthentication();

    // Background services
    services.AddHostedService<HealthCheckBackgroundService>();
    services.AddHostedService<MetricsAggregationBackgroundService>();
    services.AddHostedService<CacheExpirationBackgroundService>();
    services.AddHostedService<ServiceCleanupBackgroundService>();

    // gRPC and web services
    services.AddGrpc(options =>
    {
        options.MaxReceiveMessageSize = 10 * 1024 * 1024; // 10MB
        options.MaxSendMessageSize = 10 * 1024 * 1024;
    });

    services.AddCors(options =>
    {
        options.AddPolicy("GrpcWebPolicy", builder =>
        {
            builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithExposedHeaders(
                    "Grpc-Status",
                    "Grpc-Message",
                    "Grpc-Encoding",
                    "Grpc-Accept-Encoding",
                    "Grpc-Status-Details-Bin",
                    "Trailer");
        });
    });

    services.AddControllers();
    services.AddEndpointsApiExplorer();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    // Exception handling first, so it wraps the whole pipeline - previously it sat
    // at the end and could not catch exceptions thrown by the middleware above it.
    app.UseMiddleware<ErrorHandlingMiddleware>();

    app.UseRouting();

// Configure ForwardedHeadersMiddleware with explicit KnownProxies/KnownNetworks for security
// This middleware processes X-Forwarded-For, X-Forwarded-Proto, X-Forwarded-Host headers
// and sets Connection.RemoteIpAddress to the correct client IP when headers come from trusted sources
// SECURITY: It is CRITICAL to configure KnownProxies/KnownNetworks with your actual proxy IPs or CIDR ranges
// in production environments. Without proper configuration, client IP spoofing is possible.
// Example for common reverse proxy setups:
// KnownProxies = { IPAddress.Parse("10.0.0.1"), IPAddress.Parse("192.168.1.100") }
// OR for cloud providers:
// KnownNetworks = { IPNetwork.Parse("10.0.0.0/8"), IPNetwork.Parse("172.16.0.0/12"), IPNetwork.Parse("192.168.0.0/16") }
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost,
    // SECURITY: Configure these with your actual proxy IPs or CIDR ranges
    // For development/testing, only localhost is trusted
    // For production, add your reverse proxy IPs here
    KnownProxies = { IPAddress.Parse("127.0.0.1"), IPAddress.Parse("::1") },
    KnownNetworks = { }
});

// Initialize request context early in the pipeline so it's available to all middleware
app.UseMiddleware<RequestContextMiddleware>();

// Sanitize forwarded headers and strip hop-by-hop headers before processing
app.UseMiddleware<ForwardedHeadersSanitizationMiddleware>();

    app.UseMiddleware<RequestLoggingMiddleware>();
    app.UseMiddleware<RateLimitingMiddleware>();
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseCors("GrpcWebPolicy");
    app.UseGrpcWeb();
    app.UseMiddleware<GrpcWebCompressionMiddleware>();
    app.UseMiddleware<GrpcWebTrailerForwardingMiddleware>();
    app.UseMiddleware<RequestResponseCapturingMiddleware>();

    app.MapControllers();

// Map health endpoints
app.MapHealthChecks("/health");
app.MapPrometheusMetrics();

    Log.Information("Gateway starting on {ListenAddress}:{Port}", "0.0.0.0", 5000);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
