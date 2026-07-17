#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// Extension methods for ReflectionController
// =============================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DotNetGrpcGateway.Domain;

namespace DotNetGrpcGateway.Controllers;

/// <summary>
/// Provides additional helper methods for <see cref="ReflectionController"/>.
/// </summary>
public static class ReflectionControllerExtensions
{
    /// <summary>
    /// Retrieves the <see cref="ReflectionHealthSummary"/> produced by the controller's
    /// <c>GetReflectionHealth</c> endpoint.
    /// </summary>
    /// <param name="controller">The <see cref="ReflectionController"/> instance.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    /// <returns>The health summary returned by the controller.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The controller returned an unexpected result type.</exception>
    public static async Task<ReflectionHealthSummary> GetHealthSummaryAsync(
        this ReflectionController controller,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(controller);
        var result = await controller.GetReflectionHealth(cancellationToken);

        return result.Result switch
        {
            OkObjectResult ok => (ReflectionHealthSummary)ok.Value!,
            ObjectResult { StatusCode: StatusCodes.Status503ServiceUnavailable } obj => (ReflectionHealthSummary)obj.Value!,
            _ => throw new InvalidOperationException("Unexpected result type from GetReflectionHealth endpoint")
        };
    }

    /// <summary>
    /// Returns the number of services that currently have a reachable reflection endpoint.
    /// </summary>
    /// <param name="controller">The <see cref="ReflectionController"/> instance.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    /// <returns>The count of available services.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/>.</exception>
    public static async Task<int> GetAvailableServiceCountAsync(
        this ReflectionController controller,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(controller);
        return (await controller.GetHealthSummaryAsync(cancellationToken)).AvailableServiceCount;
    }

    /// <summary>
    /// Returns <c>true</c> if at least one service reports a reachable reflection endpoint.
    /// </summary>
    /// <param name="controller">The <see cref="ReflectionController"/> instance.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    /// <returns><c>true</c> when any service is available; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/>.</exception>
    public static async Task<bool> IsAnyServiceAvailableAsync(
        this ReflectionController controller,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(controller);
        return (await controller.GetHealthSummaryAsync(cancellationToken)).IsAvailable;
    }
}
