// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Configuration;
using DotNetGrpcGateway.Infrastructure;
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

    // Background services
    services.AddHostedService<HealthCheckBackgroundService>();

    // gRPC and web services
    services.AddGrpc(options =>
    {
        options.MaxReceiveMessageSize = 10 * 1024 * 1024; // 10MB
        options.MaxSendMessageSize = 10 * 1024 * 1024;
    });

    services.AddGrpcWeb(options =>
    {
        options.GrpcWebEnabled = true;
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
    app.UseCors("GrpcWebPolicy");
    app.UseGrpcWeb();

    // Exception handling middleware
    app.UseMiddleware<ErrorHandlingMiddleware>();

    app.MapControllers();
    app.MapGrpcReflectionService();
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
