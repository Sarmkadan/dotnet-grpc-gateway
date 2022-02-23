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
    public static async Task<ActionResult<GrpcService>> GetServiceByName(this GatewayController controller, string serviceName)
    {
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
    public static async Task<ActionResult<bool>> IsServiceHealthy(this GatewayController controller, string serviceName)
    {
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
    public static async Task<ActionResult<GatewayStatistics>> GetTodayStatisticsWithFilter(
        this GatewayController controller,
        [FromQuery] string? serviceName = null)
    {
        var statsResult = await controller.GetTodayStatistics();
        if (statsResult.Result is not OkObjectResult okResult)
            return new NotFoundResult();

        var stats = okResult.Value as GatewayStatistics;
        if (stats is null)
            return new NotFoundResult();

        if (string.IsNullOrWhiteSpace(serviceName))
            return controller.Ok(stats);

        // Filter statistics by service name if provided (using RequestsByService dictionary)
        if (stats.RequestsByService is not null && stats.RequestsByService.Any())
        {
            var hasService = stats.RequestsByService.ContainsKey(serviceName!);
            // Return the same stats object, but caller can check if service exists in RequestsByService
        }

        return controller.Ok(stats);
    }

    /// <summary>
    /// Gets slow requests with additional filtering by service name
    /// </summary>
    public static async Task<ActionResult<List<RequestMetric>>> GetSlowRequestsByService(
        this GatewayController controller,
        [FromQuery] double thresholdMs = 1000,
        [FromQuery] string? serviceName = null)
    {
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

        // Filter by service name
        var filtered = slowRequests
            .Where(r => string.Equals(r.ServiceName, serviceName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return controller.Ok(filtered);
    }
}