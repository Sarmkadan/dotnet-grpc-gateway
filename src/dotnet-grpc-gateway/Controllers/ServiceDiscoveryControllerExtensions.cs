#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
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
    /// <exception cref="InvalidOperationException">Thrown when service retrieval fails.</exception>
    public static async Task<IReadOnlyList<ServiceInfo>> GetAllActiveServicesAsync(this ServiceDiscoveryController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var servicesResult = await controller.GetAllServices();
        return servicesResult.Result switch
        {
            OkObjectResult okResult => okResult.Value switch
            {
                List<ServiceInfo> services => services.Where(s => s.IsActive).ToList().AsReadOnly(),
                _ => throw new InvalidOperationException("Failed to retrieve services - invalid response type")
            },
            _ => throw new InvalidOperationException("Failed to retrieve services")
        };
    }

    /// <summary>
    /// Finds a service by its name.
    /// </summary>
    /// <param name="controller">The <see cref="ServiceDiscoveryController"/> instance.</param>
    /// <param name="serviceName">The name of the service to find.</param>
    /// <returns>The service info if found; otherwise, <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="serviceName"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when service retrieval fails.</exception>
    public static async Task<ServiceInfo?> FindServiceByNameAsync(this ServiceDiscoveryController controller, string serviceName)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentException.ThrowIfNullOrEmpty(serviceName);

        var servicesResult = await controller.GetAllServices();
        return servicesResult.Result switch
        {
            OkObjectResult okResult => okResult.Value switch
            {
                List<ServiceInfo> services => services.FirstOrDefault(s => s.Name == serviceName),
                _ => throw new InvalidOperationException("Failed to retrieve services - invalid response type")
            },
            _ => throw new InvalidOperationException("Failed to retrieve services")
        };
    }

    /// <summary>
    /// Gets routes for a specific service by its name.
    /// </summary>
    /// <param name="controller">The <see cref="ServiceDiscoveryController"/> instance.</param>
    /// <param name="serviceName">The name of the service.</param>
    /// <returns>A read-only list of routes for the service, or an empty list if not found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="serviceName"/> is <see langword="null"/> or empty.</exception>
    public static async Task<IReadOnlyList<GatewayRoute>> GetServiceRoutesByNameAsync(this ServiceDiscoveryController controller, string serviceName)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentException.ThrowIfNullOrEmpty(serviceName);

        var serviceResult = await controller.FindServiceByNameAsync(serviceName);
        return serviceResult is null
            ? Array.Empty<GatewayRoute>()
            : await controller.GetServiceRoutes(serviceResult.Id) is var routesResult
                && routesResult.Result is OkObjectResult okResult
                && okResult.Value is List<GatewayRoute> routes
                ? routes.AsReadOnly()
                : Array.Empty<GatewayRoute>();
    }
}
