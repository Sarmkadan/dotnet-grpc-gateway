#nullable enable

using DotNetGrpcGateway.Domain;
using Microsoft.AspNetCore.Mvc;

namespace DotNetGrpcGateway.Controllers;

/// <summary>
/// Extension methods for GatewayController providing additional convenience methods
/// </summary>
public static class GatewayControllerExtensions
{
    /// <summary>
    /// Gets a specific service by name (convenience method)
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="serviceName">Name of the service to retrieve.</param>
    /// <returns>Action result containing the service if found, otherwise NotFound.</returns>
    /// <exception cref="ArgumentNullException">Thrown if controller is null.</exception>
    public static async Task<ActionResult<GrpcService>> GetServiceByName(this GatewayController controller, string serviceName)
    {
        ArgumentNullException.ThrowIfNull(controller);

        if (string.IsNullOrWhiteSpace(serviceName))
            return controller.BadRequest("Service name is required");

        var services = await controller.GetServices();
        if (services.Result is not OkObjectResult okResult)
            return new NotFoundResult();

        var allServices = okResult.Value as List<GrpcService>;
        var service = allServices?.FirstOrDefault(s => string.Equals(s.Name, serviceName, StringComparison.OrdinalIgnoreCase));

        return service is null
            ? controller.NotFound($"Service '{serviceName}' not found")
            : controller.Ok(service);
    }

    /// <summary>
    /// Checks if a service is healthy by name (convenience method)
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="serviceName">Name of the service to check.</param>
    /// <returns>Action result containing true if the service is healthy, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if controller is null.</exception>
    public static async Task<ActionResult<bool>> IsServiceHealthy(this GatewayController controller, string serviceName)
    {
        ArgumentNullException.ThrowIfNull(controller);

        if (string.IsNullOrWhiteSpace(serviceName))
            return controller.BadRequest("Service name is required");

        var healthyServices = await controller.GetHealthyServices();
        if (healthyServices.Result is not OkObjectResult okResult)
            return false;

        var healthy = okResult.Value as List<GrpcService>;
        return controller.Ok(healthy?.Any(s => string.Equals(s.Name, serviceName, StringComparison.OrdinalIgnoreCase)) ?? false);
    }

    /// <summary>
    /// Gets statistics for today with filtering options
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="serviceName">Optional service name to filter statistics by.</param>
    /// <returns>Action result containing the statistics, optionally filtered by service name.</returns>
    /// <exception cref="ArgumentNullException">Thrown if controller is null.</exception>
    public static async Task<ActionResult<GatewayStatistics>> GetTodayStatisticsWithFilter(
        this GatewayController controller,
        [FromQuery] string? serviceName = null)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var statsResult = await controller.GetTodayStatistics();
        if (statsResult.Result is not OkObjectResult okResult)
            return new NotFoundResult();

        var stats = okResult.Value as GatewayStatistics;
        if (stats is null)
            return new NotFoundResult();

        if (string.IsNullOrWhiteSpace(serviceName))
            return controller.Ok(stats);

        // Filter statistics by service name if provided
        if (stats.RequestsByService?.Any() == true && stats.RequestsByService.ContainsKey(serviceName!))
        {
            // Return a new statistics object with only the requested service's data
            var filteredStats = new GatewayStatistics
            {
                Id = stats.Id,
                StatisticsDate = stats.StatisticsDate,
                TotalRequestsProcessed = stats.RequestsByService[serviceName!],
                SuccessfulRequests = stats.SuccessfulRequests,
                FailedRequests = stats.FailedRequests,
                SuccessRate = stats.SuccessRate,
                AverageResponseTimeMs = stats.AverageResponseTimeMs,
                MinResponseTimeMs = stats.MinResponseTimeMs,
                MaxResponseTimeMs = stats.MaxResponseTimeMs,
                TotalDataProcessedBytes = stats.TotalDataProcessedBytes,
                ActiveConnections = stats.ActiveConnections,
                PeakConnections = stats.PeakConnections,
                RequestsByService = new Dictionary<string, long> { { serviceName!, stats.RequestsByService[serviceName!] } },
                RequestsByMethod = new Dictionary<string, long>(),
                ErrorsByType = new Dictionary<string, int>(),
                HealthyServices = stats.HealthyServices,
                UnhealthyServices = stats.UnhealthyServices,
                TotalServices = stats.TotalServices,
                CacheHitRate = stats.CacheHitRate,
                CacheHits = stats.CacheHits,
                CacheMisses = stats.CacheMisses,
                RecordedAt = stats.RecordedAt,
                UpdatedAt = stats.UpdatedAt
            };
            return controller.Ok(filteredStats);
        }

        // Service not found in statistics - return empty statistics
        return controller.NotFound();
    }

    /// <summary>
    /// Gets slow requests with additional filtering by service name
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="thresholdMs">Threshold in milliseconds for what constitutes a slow request.</param>
    /// <param name="serviceName">Optional service name to filter by.</param>
    /// <returns>Action result containing slow requests, optionally filtered by service name.</returns>
    /// <exception cref="ArgumentNullException">Thrown if controller is null.</exception>
    public static async Task<ActionResult<List<RequestMetric>>> GetSlowRequestsByService(
        this GatewayController controller,
        [FromQuery] double thresholdMs = 1000,
        [FromQuery] string? serviceName = null)
    {
        ArgumentNullException.ThrowIfNull(controller);

        if (thresholdMs <= 0)
            return controller.BadRequest("Threshold must be greater than 0");

        var slowRequestsResult = await controller.GetSlowRequests(thresholdMs);
        if (slowRequestsResult.Result is not OkObjectResult okResult)
            return new NotFoundResult();

        var slowRequests = okResult.Value as List<RequestMetric>;
        if (slowRequests is null || !slowRequests.Any())
            return controller.Ok(new List<RequestMetric>());

        if (string.IsNullOrWhiteSpace(serviceName))
            return controller.Ok(slowRequests);

        // Filter by service name using pattern matching for efficiency
        var filtered = slowRequests
            .Where(r => r.ServiceName is not null && string.Equals(r.ServiceName, serviceName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return controller.Ok(filtered);
    }
}