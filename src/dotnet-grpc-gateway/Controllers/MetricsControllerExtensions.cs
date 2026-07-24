#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Infrastructure;
using DotNetGrpcGateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotNetGrpcGateway.Controllers;

/// <summary>
/// Extension methods for MetricsController to add Prometheus-compatible endpoints
/// </summary>
public static class MetricsControllerExtensions
{
    /// <summary>
    /// Adds Prometheus-compatible metrics endpoints to the application
    /// </summary>
    /// <param name="app">The WebApplication instance</param>
    /// <returns>The configured WebApplication</returns>
    public static WebApplication MapPrometheusMetrics(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // Map Prometheus-compatible metrics endpoint
        app.MapGet("/metrics", async (
            IGatewayRepository gatewayRepository,
            ICircuitBreakerRegistry circuitBreakerRegistry,
            IMetricsCollectionService metricsService,
            IServiceDiscoveryService serviceDiscoveryService,
            IPerformanceMonitor? performanceMonitor,
            ILogger<MetricsController> logger) =>
        {
            try
            {
                var metrics = await GeneratePrometheusMetricsAsync(
                    gatewayRepository,
                    circuitBreakerRegistry,
                    metricsService,
                    serviceDiscoveryService,
                    performanceMonitor,
                    logger);

                return Results.Text(metrics, "text/plain; version=0.0.4; charset=utf-8");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to generate Prometheus metrics");
                return Results.Problem("Failed to generate metrics");
            }
        });

        return app;
    }

    private static async Task<string> GeneratePrometheusMetricsAsync(
        IGatewayRepository gatewayRepository,
        ICircuitBreakerRegistry circuitBreakerRegistry,
        IMetricsCollectionService metricsService,
        IServiceDiscoveryService serviceDiscoveryService,
        IPerformanceMonitor? performanceMonitor,
        ILogger logger)
    {
        var metrics = new System.Text.StringBuilder();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Header
        metrics.AppendLine("# HELP dotnet_grpc_gateway_info Gateway information");
        metrics.AppendLine("# TYPE dotnet_grpc_gateway_info gauge");
        metrics.AppendLine($"dotnet_grpc_gateway_info{{version=\"1.0.0\"}} 1 {timestamp}");
        metrics.AppendLine();

        // Database metrics
        try
        {
            var dbCount = await gatewayRepository.CountAsync();
            metrics.AppendLine("# HELP dotnet_grpc_gateway_database_records Total records in database");
            metrics.AppendLine("# TYPE dotnet_grpc_gateway_database_records gauge");
            metrics.AppendLine($"dotnet_grpc_gateway_database_records {dbCount} {timestamp}");
            metrics.AppendLine();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to get database metrics");
        }

        // Circuit breaker metrics
        var circuitBreakerStates = circuitBreakerRegistry.GetAllStates();
        var openCircuits = circuitBreakerStates.Count(x => x.Value == CircuitBreakerState.Open);
        var halfOpenCircuits = circuitBreakerStates.Count(x => x.Value == CircuitBreakerState.HalfOpen);
        var closedCircuits = circuitBreakerStates.Count(x => x.Value == CircuitBreakerState.Closed);

        metrics.AppendLine("# HELP dotnet_grpc_gateway_circuit_breakers_total Total circuit breakers");
        metrics.AppendLine("# TYPE dotnet_grpc_gateway_circuit_breakers_total gauge");
        metrics.AppendLine($"dotnet_grpc_gateway_circuit_breakers_total {circuitBreakerStates.Count} {timestamp}");
        
        metrics.AppendLine("# HELP dotnet_grpc_gateway_circuit_breakers_open Number of open circuits");
        metrics.AppendLine("# TYPE dotnet_grpc_gateway_circuit_breakers_open gauge");
        metrics.AppendLine($"dotnet_grpc_gateway_circuit_breakers_open {openCircuits} {timestamp}");
        
        metrics.AppendLine("# HELP dotnet_grpc_gateway_circuit_breakers_half_open Number of half-open circuits");
        metrics.AppendLine("# TYPE dotnet_grpc_gateway_circuit_breakers_half_open gauge");
        metrics.AppendLine($"dotnet_grpc_gateway_circuit_breakers_half_open {halfOpenCircuits} {timestamp}");
        
        metrics.AppendLine("# HELP dotnet_grpc_gateway_circuit_breakers_closed Number of closed circuits");
        metrics.AppendLine("# TYPE dotnet_grpc_gateway_circuit_breakers_closed gauge");
        metrics.AppendLine($"dotnet_grpc_gateway_circuit_breakers_closed {closedCircuits} {timestamp}");
        metrics.AppendLine();

        // Service health metrics
        var serviceHealth = await serviceDiscoveryService.GetAllServicesHealthAsync();
        var healthyServices = serviceHealth.Count(x => x.Value == DotNetGrpcGateway.Services.ServiceHealthStatus.Healthy);
        var totalServices = serviceHealth.Count;

        metrics.AppendLine("# HELP dotnet_grpc_gateway_services_total Total services");
        metrics.AppendLine("# TYPE dotnet_grpc_gateway_services_total gauge");
        metrics.AppendLine($"dotnet_grpc_gateway_services_total {totalServices} {timestamp}");
        
        metrics.AppendLine("# HELP dotnet_grpc_gateway_services_healthy Number of healthy services");
        metrics.AppendLine("# TYPE dotnet_grpc_gateway_services_healthy gauge");
        metrics.AppendLine($"dotnet_grpc_gateway_services_healthy {healthyServices} {timestamp}");
        
        metrics.AppendLine("# HELP dotnet_grpc_gateway_services_unhealthy Number of unhealthy services");
        metrics.AppendLine("# TYPE dotnet_grpc_gateway_services_unhealthy gauge");
        metrics.AppendLine($"dotnet_grpc_gateway_services_unhealthy {totalServices - healthyServices} {timestamp}");
        metrics.AppendLine();

        // Performance metrics
        if (performanceMonitor is not null)
        {
            var performanceMetrics = await performanceMonitor.GetMetricsAsync();
            
            metrics.AppendLine("# HELP dotnet_grpc_gateway_performance_requests_total Total requests processed");
            metrics.AppendLine("# TYPE dotnet_grpc_gateway_performance_requests_total counter");
            metrics.AppendLine($"dotnet_grpc_gateway_performance_requests_total {performanceMetrics.TotalRequests} {timestamp}");
            
            metrics.AppendLine("# HELP dotnet_grpc_gateway_performance_requests_per_second Current requests per second");
            metrics.AppendLine("# TYPE dotnet_grpc_gateway_performance_requests_per_second gauge");
            metrics.AppendLine($"dotnet_grpc_gateway_performance_requests_per_second {performanceMetrics.RequestsPerSecond:F2} {timestamp}");
            
            metrics.AppendLine("# HELP dotnet_grpc_gateway_performance_duration_avg Average response time in milliseconds");
            metrics.AppendLine("# TYPE dotnet_grpc_gateway_performance_duration_avg gauge");
            metrics.AppendLine($"dotnet_grpc_gateway_performance_duration_avg {performanceMetrics.AverageDurationMs:F2} {timestamp}");
            
            metrics.AppendLine("# HELP dotnet_grpc_gateway_performance_duration_min Minimum response time in milliseconds");
            metrics.AppendLine("# TYPE dotnet_grpc_gateway_performance_duration_min gauge");
            metrics.AppendLine($"dotnet_grpc_gateway_performance_duration_min {performanceMetrics.MinDurationMs} {timestamp}");
            
            metrics.AppendLine("# HELP dotnet_grpc_gateway_performance_duration_max Maximum response time in milliseconds");
            metrics.AppendLine("# TYPE dotnet_grpc_gateway_performance_duration_max gauge");
            metrics.AppendLine($"dotnet_grpc_gateway_performance_duration_max {performanceMetrics.MaxDurationMs} {timestamp}");
            
            metrics.AppendLine("# HELP dotnet_grpc_gateway_performance_duration_p50 50th percentile response time");
            metrics.AppendLine("# TYPE dotnet_grpc_gateway_performance_duration_p50 gauge");
            metrics.AppendLine($"dotnet_grpc_gateway_performance_duration_p50 {performanceMetrics.P50DurationMs:F2} {timestamp}");
            
            metrics.AppendLine("# HELP dotnet_grpc_gateway_performance_duration_p95 95th percentile response time");
            metrics.AppendLine("# TYPE dotnet_grpc_gateway_performance_duration_p95 gauge");
            metrics.AppendLine($"dotnet_grpc_gateway_performance_duration_p95 {performanceMetrics.P95DurationMs:F2} {timestamp}");
            
            metrics.AppendLine("# HELP dotnet_grpc_gateway_performance_duration_p99 99th percentile response time");
            metrics.AppendLine("# TYPE dotnet_grpc_gateway_performance_duration_p99 gauge");
            metrics.AppendLine($"dotnet_grpc_gateway_performance_duration_p99 {performanceMetrics.P99DurationMs:F2} {timestamp}");
            metrics.AppendLine();
        }

        // Gateway statistics metrics
        var todayStats = await metricsService.GetTodayStatisticsAsync();
        
        metrics.AppendLine("# HELP dotnet_grpc_gateway_statistics_requests_total Total requests processed");
        metrics.AppendLine("# TYPE dotnet_grpc_gateway_statistics_requests_total counter");
        metrics.AppendLine($"dotnet_grpc_gateway_statistics_requests_total {todayStats.TotalRequestsProcessed} {timestamp}");
        
        metrics.AppendLine("# HELP dotnet_grpc_gateway_statistics_requests_successful Successful requests");
        metrics.AppendLine("# TYPE dotnet_grpc_gateway_statistics_requests_successful counter");
        metrics.AppendLine($"dotnet_grpc_gateway_statistics_requests_successful {todayStats.SuccessfulRequests} {timestamp}");
        
        metrics.AppendLine("# HELP dotnet_grpc_gateway_statistics_requests_failed Failed requests");
        metrics.AppendLine("# TYPE dotnet_grpc_gateway_statistics_requests_failed counter");
        metrics.AppendLine($"dotnet_grpc_gateway_statistics_requests_failed {todayStats.FailedRequests} {timestamp}");
        
        metrics.AppendLine("# HELP dotnet_grpc_gateway_statistics_success_rate Success rate percentage");
        metrics.AppendLine("# TYPE dotnet_grpc_gateway_statistics_success_rate gauge");
        metrics.AppendLine($"dotnet_grpc_gateway_statistics_success_rate {todayStats.SuccessRate:F2} {timestamp}");
        
        metrics.AppendLine("# HELP dotnet_grpc_gateway_statistics_duration_avg Average response time in milliseconds");
        metrics.AppendLine("# TYPE dotnet_grpc_gateway_statistics_duration_avg gauge");
        metrics.AppendLine($"dotnet_grpc_gateway_statistics_duration_avg {todayStats.AverageResponseTimeMs:F2} {timestamp}");
        
        metrics.AppendLine("# HELP dotnet_grpc_gateway_statistics_connections_active Active connections");
        metrics.AppendLine("# TYPE dotnet_grpc_gateway_statistics_connections_active gauge");
        metrics.AppendLine($"dotnet_grpc_gateway_statistics_connections_active {todayStats.ActiveConnections} {timestamp}");
        
        metrics.AppendLine("# HELP dotnet_grpc_gateway_statistics_connections_peak Peak connections");
        metrics.AppendLine("# TYPE dotnet_grpc_gateway_statistics_connections_peak gauge");
        metrics.AppendLine($"dotnet_grpc_gateway_statistics_connections_peak {todayStats.PeakConnections} {timestamp}");
        
        metrics.AppendLine("# HELP dotnet_grpc_gateway_statistics_cache_hit_rate Cache hit rate percentage");
        metrics.AppendLine("# TYPE dotnet_grpc_gateway_statistics_cache_hit_rate gauge");
        metrics.AppendLine($"dotnet_grpc_gateway_statistics_cache_hit_rate {todayStats.CacheHitRate:F2} {timestamp}");
        metrics.AppendLine();

        // Service-specific metrics
        foreach (var service in todayStats.RequestsByService.OrderByDescending(x => x.Value))
        {
            var escapedServiceName = EscapeMetricLabel(service.Key);
            metrics.AppendLine($"# HELP dotnet_grpc_gateway_service_requests_total Requests for service {service.Key}");
            metrics.AppendLine("# TYPE dotnet_grpc_gateway_service_requests_total counter");
            metrics.AppendLine($"dotnet_grpc_gateway_service_requests_total{{service=\"{escapedServiceName}\"}} {service.Value} {timestamp}");
        }
        
        if (todayStats.RequestsByService.Count > 0)
        {
            metrics.AppendLine();
        }

        // Error metrics
        foreach (var error in todayStats.ErrorsByType.OrderByDescending(x => x.Value))
        {
            var escapedErrorType = EscapeMetricLabel(error.Key);
            metrics.AppendLine($"# HELP dotnet_grpc_gateway_errors_total Error count for {error.Key}");
            metrics.AppendLine("# TYPE dotnet_grpc_gateway_errors_total counter");
            metrics.AppendLine($"dotnet_grpc_gateway_errors_total{{error_type=\"{escapedErrorType}\"}} {error.Value} {timestamp}");
        }

        return metrics.ToString();
    }

    private static string EscapeMetricLabel(string label)
    {
        return label
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }
}
