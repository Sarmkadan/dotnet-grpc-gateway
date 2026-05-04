// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Exceptions;

namespace DotNetGrpcGateway.Services;

/// <summary>
/// Service discovery and health checking for registered gRPC services
/// </summary>
public interface IServiceDiscoveryService
{
    Task<ServiceHealthReport> PerformHealthCheckAsync(int serviceId);
    Task<ServiceHealthReport> GetLatestHealthReportAsync(int serviceId);
    Task UpdateServiceHealthAsync(int serviceId, bool isHealthy, string? errorMessage = null);
    Task<List<GrpcService>> DiscoverAvailableServicesAsync();
    Task<Dictionary<int, ServiceHealthStatus>> GetAllServicesHealthAsync();
}

public class ServiceDiscoveryService : IServiceDiscoveryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly Dictionary<int, ServiceHealthReport> _healthReports = new();
    private readonly ILogger<ServiceDiscoveryService> _logger;

    public ServiceDiscoveryService(IUnitOfWork unitOfWork, ILogger<ServiceDiscoveryService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ServiceHealthReport> PerformHealthCheckAsync(int serviceId)
    {
        try
        {
            var service = await _unitOfWork.Services.GetByIdAsync(serviceId);
            if (service == null)
                throw new ServiceNotFoundException("unknown");

            var healthReport = new ServiceHealthReport
            {
                ServiceId = serviceId,
                HealthCheckEndpoint = service.GetEndpointUri() + "/health",
                NextCheckScheduledAt = DateTime.UtcNow.AddSeconds(service.HealthCheckIntervalSeconds)
            };

            // Simulate health check (in real implementation, would make HTTP/gRPC call)
            var startTime = DateTime.UtcNow;
            try
            {
                var isHealthy = Random.Shared.Next(0, 100) > 5; // 95% success rate
                var responseTime = Random.Shared.Next(10, 500);

                healthReport.RecordCheckResult(isHealthy, responseTime);
                healthReport.HttpStatusCode = isHealthy ? 200 : 503;

                service.UpdateHealthStatus(healthReport.IsHealthy, healthReport.ErrorMessage);
                await _unitOfWork.Services.UpdateAsync(service);

                _healthReports[serviceId] = healthReport;
                await _unitOfWork.Metrics.UpdateStatisticsAsync(new GatewayStatistics());

                _logger.LogInformation(
                    "Health check for service {ServiceId} completed with status {Status}",
                    serviceId,
                    healthReport.HealthStatus);

                return healthReport;
            }
            catch (Exception ex)
            {
                var responseTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
                healthReport.RecordCheckResult(false, responseTime, ex.Message);
                healthReport.AddDiagnosticMessage($"Health check failed: {ex.Message}");

                _logger.LogError(ex, "Health check failed for service {ServiceId}", serviceId);
                throw new ServiceUnavailableException(service.Name, ex.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing health check for service {ServiceId}", serviceId);
            throw;
        }
    }

    public async Task<ServiceHealthReport> GetLatestHealthReportAsync(int serviceId)
    {
        if (_healthReports.TryGetValue(serviceId, out var report))
            return report;

        var newReport = new ServiceHealthReport
        {
            ServiceId = serviceId,
            HealthStatus = "Unknown",
            IsHealthy = false
        };

        return newReport;
    }

    public async Task UpdateServiceHealthAsync(int serviceId, bool isHealthy, string? errorMessage = null)
    {
        var service = await _unitOfWork.Services.GetByIdAsync(serviceId);
        if (service == null)
            throw new ServiceNotFoundException("unknown");

        service.UpdateHealthStatus(isHealthy, errorMessage);
        await _unitOfWork.Services.UpdateAsync(service);

        _logger.LogInformation(
            "Service {ServiceName} health status updated to {Status}",
            service.Name,
            isHealthy ? "Healthy" : "Unhealthy");
    }

    public async Task<List<GrpcService>> DiscoverAvailableServicesAsync()
    {
        return await _unitOfWork.Services.GetActiveAsync();
    }

    public async Task<Dictionary<int, ServiceHealthStatus>> GetAllServicesHealthAsync()
    {
        var services = await _unitOfWork.Services.GetAllAsync();
        var health = new Dictionary<int, ServiceHealthStatus>();

        foreach (var service in services)
        {
            var status = service.IsHealthy
                ? ServiceHealthStatus.Healthy
                : ServiceHealthStatus.Unhealthy;

            health[service.Id] = status;
        }

        return health;
    }
}

public enum ServiceHealthStatus
{
    Unknown,
    Healthy,
    Degraded,
    Unhealthy,
    Maintenance
}
