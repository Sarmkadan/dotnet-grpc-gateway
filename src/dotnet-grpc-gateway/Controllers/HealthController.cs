#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Infrastructure;
using DotNetGrpcGateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotNetGrpcGateway.Controllers;

/// <summary>
/// REST API controller for health status and readiness checks.
/// Provides detailed health information including database connectivity, circuit breaker states,
/// and service health metrics.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    private readonly IGatewayRepository _gatewayRepository;
    private readonly ICircuitBreakerRegistry _circuitBreakerRegistry;
    private readonly IMetricsCollectionService _metricsService;
    private readonly IServiceDiscoveryService _serviceDiscoveryService;
    private readonly IPerformanceMonitor? _performanceMonitor;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        IGatewayRepository gatewayRepository,
        ICircuitBreakerRegistry circuitBreakerRegistry,
        IMetricsCollectionService metricsService,
        IServiceDiscoveryService serviceDiscoveryService,
        ILogger<HealthController> logger,
        IPerformanceMonitor? performanceMonitor = null)
    {
        _gatewayRepository = gatewayRepository ?? throw new ArgumentNullException(nameof(gatewayRepository));
        _circuitBreakerRegistry = circuitBreakerRegistry ?? throw new ArgumentNullException(nameof(circuitBreakerRegistry));
        _metricsService = metricsService ?? throw new ArgumentNullException(nameof(metricsService));
        _serviceDiscoveryService = serviceDiscoveryService ?? throw new ArgumentNullException(nameof(serviceDiscoveryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _performanceMonitor = performanceMonitor;
    }

    /// <summary>
    /// Gets comprehensive health status including database connectivity, circuit breaker states,
    /// and service health metrics.
    /// </summary>
    /// <returns>Health status with detailed information</returns>
    [HttpGet("status")]
    [ProducesResponseType(typeof(HealthStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<HealthStatus>> GetHealthStatus()
    {
        const string endpointName = "status";
        _logger.LogDebug("Health check {Endpoint} started", endpointName);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Test database connectivity
            var dbAvailable = await CheckDatabaseConnectivityAsync();

            // Get circuit breaker states
            var circuitBreakerStates = _circuitBreakerRegistry.GetAllStates();
            var openCircuits = circuitBreakerStates.Count(x => x.Value == CircuitBreakerState.Open);
            var halfOpenCircuits = circuitBreakerStates.Count(x => x.Value == CircuitBreakerState.HalfOpen);
            var closedCircuits = circuitBreakerStates.Count(x => x.Value == CircuitBreakerState.Closed);

            if (openCircuits > 0)
            {
                _logger.LogWarning(
                    "Health check component {Component} unhealthy: {Reason}. Open circuits: {OpenCircuitCount}",
                    "Circuit Breaker Registry",
                    "One or more circuit breakers are open",
                    openCircuits);
            }

            // Get service health
            var serviceHealth = await _serviceDiscoveryService.GetAllServicesHealthAsync();
            var healthyServices = serviceHealth.Count(x => x.Value == DotNetGrpcGateway.Services.ServiceHealthStatus.Healthy);
            var totalServices = serviceHealth.Count;

            if (healthyServices != totalServices)
            {
                _logger.LogWarning(
                    "Health check component {Component} unhealthy: {Reason}. Unhealthy services: {UnhealthyServiceCount}; total services: {TotalServiceCount}",
                    "Service Discovery",
                    "One or more discovered services are unhealthy",
                    totalServices - healthyServices,
                    totalServices);
            }

            // Get performance metrics if available
            PerformanceMetrics? performanceMetrics = null;
            if (_performanceMonitor is not null)
            {
                performanceMetrics = await _performanceMonitor.GetMetricsAsync();
            }

            // Get today's statistics
            var todayStats = await _metricsService.GetTodayStatisticsAsync();
            var isGatewayHealthy = todayStats.IsGatewayHealthy(healthyThreshold: 0.8);

            var healthStatus = new HealthStatus
            {
                Status = dbAvailable && isGatewayHealthy ? "Healthy" : "Unhealthy",
                Timestamp = DateTime.UtcNow,
                Database = new DatabaseStatus
                {
                    Available = dbAvailable,
                    Status = dbAvailable ? "Connected" : "Disconnected"
                },
                CircuitBreakers = new CircuitBreakerStatus
                {
                    Total = circuitBreakerStates.Count,
                    Open = openCircuits,
                    HalfOpen = halfOpenCircuits,
                    Closed = closedCircuits,
                    State = openCircuits > 0 ? "Degraded" :
                            halfOpenCircuits > 0 ? "Recovering" :
                            "Operational"
                },
                Services = new ServiceStatus
                {
                    Total = totalServices,
                    Healthy = healthyServices,
                    Unhealthy = totalServices - healthyServices,
                    Status = totalServices == 0 ? "NoServices" :
                            healthyServices == totalServices ? "Healthy" :
                            healthyServices >= totalServices * 0.8 ? "Degraded" :
                            "Unhealthy"
                },
                Performance = performanceMetrics is null ? null : new PerformanceStatus
                {
                    TotalRequests = performanceMetrics.TotalRequests,
                    RequestsPerSecond = performanceMetrics.RequestsPerSecond,
                    AverageResponseTimeMs = performanceMetrics.AverageDurationMs,
                    P50DurationMs = performanceMetrics.P50DurationMs,
                    P95DurationMs = performanceMetrics.P95DurationMs,
                    P99DurationMs = performanceMetrics.P99DurationMs
                },
                Statistics = new StatisticsStatus
                {
                    TotalRequestsProcessed = todayStats.TotalRequestsProcessed,
                    SuccessfulRequests = todayStats.SuccessfulRequests,
                    FailedRequests = todayStats.FailedRequests,
                    SuccessRate = todayStats.SuccessRate,
                    AverageResponseTimeMs = todayStats.AverageResponseTimeMs,
                    ActiveConnections = todayStats.ActiveConnections,
                    PeakConnections = todayStats.PeakConnections,
                    CacheHitRate = todayStats.CacheHitRate
                },
                Checks = new List<HealthCheckResult>
                {
                    new HealthCheckResult
                    {
                        Name = "Database Connectivity",
                        Status = dbAvailable ? "Healthy" : "Unhealthy",
                        Details = dbAvailable ? "Successfully connected to database" : "Failed to connect to database"
                    },
                    new HealthCheckResult
                    {
                        Name = "Circuit Breakers",
                        Status = openCircuits == 0 ? "Healthy" : "Degraded",
                        Details = $"{closedCircuits} closed, {openCircuits} open, {halfOpenCircuits} half-open"
                    },
                    new HealthCheckResult
                    {
                        Name = "Service Health",
                        Status = healthyServices == totalServices ? "Healthy" :
                                healthyServices >= totalServices * 0.8 ? "Degraded" :
                                "Unhealthy",
                        Details = $"{healthyServices}/{totalServices} services healthy"
                    }
                }
            };

            stopwatch.Stop();
            _logger.LogInformation(
                "Health check {Endpoint} completed with status {Status} in {ElapsedMilliseconds} ms",
                endpointName,
                healthStatus.Status,
                stopwatch.ElapsedMilliseconds);
            return Ok(healthStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            stopwatch.Stop();
            _logger.LogInformation(
                "Health check {Endpoint} completed with status {Status} in {ElapsedMilliseconds} ms",
                endpointName,
                "Unhealthy",
                stopwatch.ElapsedMilliseconds);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                Status = "Unhealthy",
                Timestamp = DateTime.UtcNow,
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Liveness probe - simple endpoint that always returns Healthy if the application is running.
    /// Used by orchestrators like Kubernetes to determine if the pod should be restarted.
    /// </summary>
    /// <returns>Simple liveness status</returns>
    [HttpGet("liveness")]
    [ProducesResponseType(typeof(LivenessStatus), StatusCodes.Status200OK)]
    public IActionResult GetLiveness()
    {
        const string endpointName = "liveness";
        _logger.LogDebug("Health check {Endpoint} started", endpointName);
        var stopwatch = Stopwatch.StartNew();

        var livenessStatus = new LivenessStatus
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Application = "dotnet-grpc-gateway"
        };

        stopwatch.Stop();
        _logger.LogInformation(
            "Health check {Endpoint} completed with status {Status} in {ElapsedMilliseconds} ms",
            endpointName,
            livenessStatus.Status,
            stopwatch.ElapsedMilliseconds);
        return Ok(livenessStatus);
    }

    /// <summary>
    /// Readiness probe - returns Healthy only when the application is ready to serve traffic.
    /// Checks database connectivity and circuit breaker states.
    /// </summary>
    /// <returns>Readiness status</returns>
    [HttpGet("readiness")]
    [ProducesResponseType(typeof(ReadinessStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ReadinessStatus>> GetReadiness()
    {
        const string endpointName = "readiness";
        _logger.LogDebug("Health check {Endpoint} started", endpointName);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Check database connectivity
            var dbAvailable = await CheckDatabaseConnectivityAsync();

            // Check circuit breaker states
            var circuitBreakerStates = _circuitBreakerRegistry.GetAllStates();
            var openCircuits = circuitBreakerStates.Count(x => x.Value == CircuitBreakerState.Open);

            if (openCircuits > 0)
            {
                _logger.LogWarning(
                    "Health check component {Component} unhealthy: {Reason}. Open circuits: {OpenCircuitCount}",
                    "Circuit Breaker Registry",
                    "One or more circuit breakers are open",
                    openCircuits);
            }

            // Get service health
            var serviceHealth = await _serviceDiscoveryService.GetAllServicesHealthAsync();
            var healthyServices = serviceHealth.Count(x => x.Value == DotNetGrpcGateway.Services.ServiceHealthStatus.Healthy);
            var totalServices = serviceHealth.Count;

            if (healthyServices != totalServices)
            {
                _logger.LogWarning(
                    "Health check component {Component} unhealthy: {Reason}. Unhealthy services: {UnhealthyServiceCount}; total services: {TotalServiceCount}",
                    "Service Discovery",
                    "One or more discovered services are unhealthy",
                    totalServices - healthyServices,
                    totalServices);
            }

            // Get today's statistics
            var todayStats = await _metricsService.GetTodayStatisticsAsync();
            var isGatewayHealthy = todayStats.IsGatewayHealthy(healthyThreshold: 0.8);

            var readinessStatus = new ReadinessStatus
            {
                Status = dbAvailable && isGatewayHealthy && openCircuits == 0 ? "Ready" : "NotReady",
                Timestamp = DateTime.UtcNow,
                DatabaseAvailable = dbAvailable,
                CircuitBreakersOperational = openCircuits == 0,
                ServicesHealthy = healthyServices,
                ServicesTotal = totalServices,
                Message = dbAvailable && isGatewayHealthy && openCircuits == 0
                    ? "Gateway is ready to serve traffic"
                    : "Gateway is not ready to serve traffic"
            };

            stopwatch.Stop();
            _logger.LogInformation(
                "Health check {Endpoint} completed with status {Status} in {ElapsedMilliseconds} ms",
                endpointName,
                readinessStatus.Status,
                stopwatch.ElapsedMilliseconds);
            return Ok(readinessStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Readiness check failed");
            stopwatch.Stop();
            _logger.LogInformation(
                "Health check {Endpoint} completed with status {Status} in {ElapsedMilliseconds} ms",
                endpointName,
                "NotReady",
                stopwatch.ElapsedMilliseconds);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                Status = "NotReady",
                Timestamp = DateTime.UtcNow,
                Error = ex.Message
            });
        }
    }

    private async Task<bool> CheckDatabaseConnectivityAsync()
    {
        try
        {
            // Test database connectivity by attempting a simple query
            var count = await _gatewayRepository.CountAsync(CancellationToken.None);
            return count >= 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Health check component {Component} unhealthy: {Reason}",
                "Database",
                ex.Message);
            return false;
        }
    }
}

/// <summary>
/// Comprehensive health status response
/// </summary>
public class HealthStatus
{
    /// <summary>Overall health status</summary>
    public required string Status { get; set; }

    /// <summary>Timestamp when health was checked</summary>
    public required DateTime Timestamp { get; set; }

    /// <summary>Database connectivity status</summary>
    public required DatabaseStatus Database { get; set; }

    /// <summary>Circuit breaker states</summary>
    public required CircuitBreakerStatus CircuitBreakers { get; set; }

    /// <summary>Service health information</summary>
    public required ServiceStatus Services { get; set; }

    /// <summary>Performance metrics</summary>
    public PerformanceStatus? Performance { get; set; }

    /// <summary>Gateway statistics</summary>
    public required StatisticsStatus Statistics { get; set; }

    /// <summary>Detailed check results</summary>
    public required List<HealthCheckResult> Checks { get; set; }
}

/// <summary>Database connectivity status</summary>
public class DatabaseStatus
{
    /// <summary>Whether database is available</summary>
    public required bool Available { get; set; }

    /// <summary>Status description</summary>
    public required string Status { get; set; }
}

/// <summary>Circuit breaker states</summary>
public class CircuitBreakerStatus
{
    /// <summary>Total number of circuit breakers</summary>
    public required int Total { get; set; }

    /// <summary>Number of open circuits</summary>
    public required int Open { get; set; }

    /// <summary>Number of half-open circuits</summary>
    public required int HalfOpen { get; set; }

    /// <summary>Number of closed circuits</summary>
    public required int Closed { get; set; }

    /// <summary>Overall circuit breaker state</summary>
    public required string State { get; set; }
}

/// <summary>Service health information</summary>
public class ServiceStatus
{
    /// <summary>Total number of services</summary>
    public required int Total { get; set; }

    /// <summary>Number of healthy services</summary>
    public required int Healthy { get; set; }

    /// <summary>Number of unhealthy services</summary>
    public required int Unhealthy { get; set; }

    /// <summary>Overall service health status</summary>
    public required string Status { get; set; }
}

/// <summary>Performance metrics</summary>
public class PerformanceStatus
{
    /// <summary>Total number of requests</summary>
    public required long TotalRequests { get; set; }

    /// <summary>Current requests per second</summary>
    public required double RequestsPerSecond { get; set; }

    /// <summary>Average response time in milliseconds</summary>
    public required double AverageResponseTimeMs { get; set; }

    /// <summary>50th percentile response time</summary>
    public required double P50DurationMs { get; set; }

    /// <summary>95th percentile response time</summary>
    public required double P95DurationMs { get; set; }

    /// <summary>99th percentile response time</summary>
    public required double P99DurationMs { get; set; }
}

/// <summary>Gateway statistics</summary>
public class StatisticsStatus
{
    /// <summary>Total requests processed</summary>
    public required long TotalRequestsProcessed { get; set; }

    /// <summary>Successful requests</summary>
    public required long SuccessfulRequests { get; set; }

    /// <summary>Failed requests</summary>
    public required long FailedRequests { get; set; }

    /// <summary>Success rate percentage</summary>
    public required double SuccessRate { get; set; }

    /// <summary>Average response time in milliseconds</summary>
    public required double AverageResponseTimeMs { get; set; }

    /// <summary>Active connections</summary>
    public required int ActiveConnections { get; set; }

    /// <summary>Peak connections</summary>
    public required int PeakConnections { get; set; }

    /// <summary>Cache hit rate percentage</summary>
    public required double CacheHitRate { get; set; }
}

/// <summary>Individual health check result</summary>
public class HealthCheckResult
{
    /// <summary>Check name</summary>
    public required string Name { get; set; }

    /// <summary>Check status</summary>
    public required string Status { get; set; }

    /// <summary>Details about the check</summary>
    public required string Details { get; set; }
}

/// <summary>Simple liveness status</summary>
public class LivenessStatus
{
    /// <summary>Status</summary>
    public required string Status { get; set; }

    /// <summary>Timestamp</summary>
    public required DateTime Timestamp { get; set; }

    /// <summary>Application name</summary>
    public required string Application { get; set; }
}

/// <summary>Readiness status</summary>
public class ReadinessStatus
{
    /// <summary>Status</summary>
    public required string Status { get; set; }

    /// <summary>Timestamp</summary>
    public required DateTime Timestamp { get; set; }

    /// <summary>Whether database is available</summary>
    public required bool DatabaseAvailable { get; set; }

    /// <summary>Whether circuit breakers are operational</summary>
    public required bool CircuitBreakersOperational { get; set; }

    /// <summary>Number of healthy services</summary>
    public required int ServicesHealthy { get; set; }

    /// <summary>Total number of services</summary>
    public required int ServicesTotal { get; set; }

    /// <summary>Readiness message</summary>
    public required string Message { get; set; }
}
