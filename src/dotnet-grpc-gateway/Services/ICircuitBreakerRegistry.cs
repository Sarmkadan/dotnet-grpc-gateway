#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using DotNetGrpcGateway.Events;
using DotNetGrpcGateway.Infrastructure;

namespace DotNetGrpcGateway.Services;

/// <summary>
/// Manages a registry of circuit breakers, one per registered service.
/// Provides lifecycle operations and aggregate status queries.
/// </summary>
public interface ICircuitBreakerRegistry
{
    /// <summary>Returns the circuit breaker for the given service, creating it on first access.</summary>
    ICircuitBreaker GetOrCreate(int serviceId);

    /// <summary>Returns the circuit breaker for a service, or <see langword="null"/> if none exists.</summary>
    ICircuitBreaker? TryGet(int serviceId);

    /// <summary>Resets the circuit breaker for a service to the closed state.</summary>
    void Reset(int serviceId);

    /// <summary>Returns a snapshot of all circuit breaker states.</summary>
    IReadOnlyDictionary<int, CircuitBreakerState> GetAllStates();
}

/// <summary>
/// Thread-safe, in-process circuit breaker registry.
/// </summary>
public class CircuitBreakerRegistry : ICircuitBreakerRegistry
{
    private readonly ConcurrentDictionary<int, ICircuitBreaker> _breakers = new();
    private readonly CircuitBreakerOptions _defaultOptions;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IEventPublisher? _eventPublisher;

    public CircuitBreakerRegistry(
        ILoggerFactory loggerFactory,
        CircuitBreakerOptions? defaultOptions = null,
        IEventPublisher? eventPublisher = null)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _defaultOptions = defaultOptions ?? new CircuitBreakerOptions();
        _eventPublisher = eventPublisher;
    }

    /// <inheritdoc/>
    public ICircuitBreaker GetOrCreate(int serviceId)
    {
        return _breakers.GetOrAdd(
            serviceId,
            id => new CircuitBreaker(id, _defaultOptions, _loggerFactory.CreateLogger<CircuitBreaker>(), _eventPublisher));
    }

    /// <inheritdoc/>
    public ICircuitBreaker? TryGet(int serviceId)
    {
        _breakers.TryGetValue(serviceId, out var breaker);
        return breaker;
    }

    /// <inheritdoc/>
    public void Reset(int serviceId)
    {
        if (_breakers.TryGetValue(serviceId, out var breaker))
            breaker.Reset();
    }

    /// <inheritdoc/>
    public IReadOnlyDictionary<int, CircuitBreakerState> GetAllStates()
    {
        return _breakers.ToDictionary(pair => pair.Key, pair => pair.Value.State);
    }
}
