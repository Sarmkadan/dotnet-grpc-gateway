#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Mvc;

namespace DotNetGrpcGateway.Controllers;

/// <summary>
/// Extension methods for <see cref="CircuitBreakerController"/> that provide additional functionality
/// for managing and monitoring circuit breakers.
/// </summary>
public static class CircuitBreakerControllerExtensions
{
    /// <summary>
    /// Gets the status of circuit breakers for multiple services in a single call.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="serviceIds">Collection of service IDs to query.</param>
    /// <returns>Dictionary mapping service IDs to their status objects.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> or <paramref name="serviceIds"/> is null.</exception>
    public static ActionResult<IReadOnlyDictionary<int, object>> GetStatuses(
        this CircuitBreakerController controller,
        IEnumerable<int> serviceIds)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(serviceIds);

        var result = new Dictionary<int, object>();
        var registry = controller.GetRegistry();

        foreach (var serviceId in serviceIds)
        {
            var breaker = registry.TryGet(serviceId);
            if (breaker is not null)
            {
                result[serviceId] = new
                {
                    serviceId,
                    state = breaker.State.ToString(),
                    consecutiveFailures = breaker.ConsecutiveFailures,
                    openedAt = breaker.OpenedAt
                };
            }
        }

        return controller.Ok((IReadOnlyDictionary<int, object>)result);
    }

    /// <summary>
    /// Gets all circuit breakers with additional statistics and metrics.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <returns>Dictionary mapping service IDs to enhanced status objects with metrics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> is null.</exception>
    public static ActionResult<IReadOnlyDictionary<int, object>> GetAllWithMetrics(
        this CircuitBreakerController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var states = controller.GetRegistry().GetAllStates();
        var result = new Dictionary<int, object>();

        foreach (var (serviceId, breakerState) in states)
        {
            var breaker = controller.GetRegistry().TryGet(serviceId);
            if (breaker is not null)
            {
                result[serviceId] = new
                {
                    serviceId,
                    state = breaker.State.ToString(),
                    consecutiveFailures = breaker.ConsecutiveFailures,
                    openedAt = breaker.OpenedAt,
                    isHalfOpen = breaker.State == Infrastructure.CircuitBreakerState.HalfOpen
                };
            }
        }

        return controller.Ok((IReadOnlyDictionary<int, object>)result);
    }

    /// <summary>
    /// Resets multiple circuit breakers in a single operation.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="serviceIds">Collection of service IDs to reset.</param>
    /// <returns>Dictionary mapping service IDs to reset results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> or <paramref name="serviceIds"/> is null.</exception>
    public static ActionResult<IReadOnlyDictionary<int, object>> ResetMultiple(
        this CircuitBreakerController controller,
        IEnumerable<int> serviceIds)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(serviceIds);

        var result = new Dictionary<int, object>();
        var registry = controller.GetRegistry();

        foreach (var serviceId in serviceIds)
    {
        var breaker = registry.TryGet(serviceId);
        if (breaker is not null)
        {
            registry.Reset(serviceId);
            result[serviceId] = new { serviceId, state = Infrastructure.CircuitBreakerState.Closed.ToString() };
        }
        else
        {
            result[serviceId] = new { serviceId, error = $"No circuit breaker registered for service {serviceId}" };
        }
    }

        return controller.Ok((IReadOnlyDictionary<int, object>)result);
    }

    /// <summary>
    /// Gets circuit breakers that are currently in a faulted state.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <returns>Dictionary of faulted circuit breakers.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> is null.</exception>
    public static ActionResult<IReadOnlyDictionary<int, object>> GetFaulted(
        this CircuitBreakerController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var states = controller.GetRegistry().GetAllStates();
        var result = new Dictionary<int, object>();

        foreach (var (serviceId, breakerState) in states)
        {
            var breaker = controller.GetRegistry().TryGet(serviceId);
            if (breaker?.State == Infrastructure.CircuitBreakerState.Open)
            {
                result[serviceId] = new
                {
                    serviceId,
                    state = breaker.State.ToString(),
                    consecutiveFailures = breaker.ConsecutiveFailures,
                    openedAt = breaker.OpenedAt
                };
            }
        }

        return controller.Ok((IReadOnlyDictionary<int, object>)result);
    }

    private static Services.ICircuitBreakerRegistry GetRegistry(this CircuitBreakerController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);
        return controller.HttpContext.RequestServices.GetRequiredService<Services.ICircuitBreakerRegistry>();
    }
}