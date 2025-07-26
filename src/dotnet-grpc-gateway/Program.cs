// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Caching;
using DotNetGrpcGateway.Configuration;
using DotNetGrpcGateway.Events;
using DotNetGrpcGateway.Events.EventHandlers;
using DotNetGrpcGateway.Formatters;
using DotNetGrpcGateway.Infrastructure;
using DotNetGrpcGateway.Integration;
using DotNetGrpcGateway.Middleware;
using DotNetGrpcGateway.Services;
using Serilog;

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

    // Request context
    services.AddScoped<RequestContextAccessor>();

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
                .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding");
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

    app.UseRouting();

    // Phase 2: Add new middleware
    app.UseMiddleware<RequestLoggingMiddleware>();
    app.UseMiddleware<RateLimitingMiddleware>();
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseCors("GrpcWebPolicy");
    app.UseGrpcWeb();

    // Exception handling middleware
    app.UseMiddleware<ErrorHandlingMiddleware>();

    app.MapControllers();
    app.MapHealthChecks("/health");

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
