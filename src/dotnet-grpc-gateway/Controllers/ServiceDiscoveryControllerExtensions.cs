#nullable enable
// =============================================================================
// Author: [Your Name]
// =============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;

namespace DotNetGrpcGateway.Controllers;

/// <summary>
/// Extension methods for <see cref="ServiceDiscoveryController"/> to enhance service discovery operations.
/// </summary>
public static class ServiceDiscoveryControllerExtensions
{
    /// <summary>
    /// Gets all active services with their metadata.
    /// </summary>
    /// <param name="controller">The <see cref="ServiceDiscoveryController"/> instance.</param>
    /// <returns>A read-only list of active service infos.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/>.</exception>
    public static async Task<IReadOnlyList<ServiceInfo>> GetAllActiveServicesAsync(this ServiceDiscoveryController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);
        var servicesResult = await controller.GetAllServices();
        if (servicesResult.Result is OkObjectResult okResult)
        {
            var services = (List<ServiceInfo>)okResult.Value!;
            return services.Where(s => s.IsActive).ToList();
        }
        else
        {
            throw new InvalidOperationException("Failed to retrieve services");
        }
    }

    /// <summary>
    /// Finds a service by its name.
    /// </summary>
    /// <param name="controller">The <see cref="ServiceDiscoveryController"/> instance.</param>
    /// <param name="serviceName">The name of the service to find.</param>
    /// <returns>The service info if found; otherwise, <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/>.</exception>
    public static async Task<ServiceInfo?> FindServiceByNameAsync(this ServiceDiscoveryController controller, string serviceName)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentException.ThrowIfNullOrEmpty(serviceName);
        var servicesResult = await controller.GetAllServices();
        if (servicesResult.Result is OkObjectResult okResult)
        {
            var services = (List<ServiceInfo>)okResult.Value!;
            return services.FirstOrDefault(s => s.Name == serviceName);
        }
        else
        {
            throw new InvalidOperationException("Failed to retrieve services");
        }
    }

    /// <summary>
    /// Gets routes for a specific service by its name.
    /// </summary>
    /// <param name="controller">The <see cref="ServiceDiscoveryController"/> instance.</param>
    /// <param name="serviceName">The name of the service.</param>
    /// <returns>A read-only list of routes for the service.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/>.</exception>
    public static async Task<IReadOnlyList<GatewayRoute>> GetServiceRoutesByNameAsync(this ServiceDiscoveryController controller, string serviceName)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentException.ThrowIfNullOrEmpty(serviceName);
        var serviceResult = await controller.FindServiceByNameAsync(serviceName);
        if (serviceResult is not null)
        {
            var routesResult = await controller.GetServiceRoutes(serviceResult.Id);
            if (routesResult.Result is OkObjectResult okResult)
            {
                return (List<GatewayRoute>)okResult.Value!;
            }
        }
        return Array.Empty<GatewayRoute>();
    }
}
