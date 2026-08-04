#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using DotNetGrpcGateway.Services;
using DotNetGrpcGateway.Infrastructure;

namespace DotNetGrpcGateway.Tests;

public class CircuitBreakerRegistryTests
{
    private readonly CircuitBreakerRegistry _registry;

    public CircuitBreakerRegistryTests()
    {
        _registry = new CircuitBreakerRegistry(NullLoggerFactory.Instance);
    }

    [Fact]
    public void GetOrCreate_SameServiceId_ReturnsSameInstance()
    {
        var breaker1 = _registry.GetOrCreate(1);
        var breaker2 = _registry.GetOrCreate(1);

        Assert.Same(breaker1, breaker2);
    }

    [Fact]
    public void GetOrCreate_DifferentServiceIds_ReturnsDifferentInstances()
    {
        var breaker1 = _registry.GetOrCreate(1);
        var breaker2 = _registry.GetOrCreate(2);

        Assert.NotSame(breaker1, breaker2);
    }

    [Fact]
    public void GetOrCreate_ConcurrentCalls_ReturnsOneInstance()
    {
        const int serviceId = 1;
        var instances = new List<ICircuitBreaker>();
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 100 };

        Parallel.For(0, 1000, parallelOptions, _ =>
        {
            instances.Add(_registry.GetOrCreate(serviceId));
        });

        var distinctInstances = instances.Distinct().ToList();
        Assert.Single(distinctInstances);
    }

    [Fact]
    public void TryGet_ExistingServiceId_ReturnsBreaker()
    {
        _registry.GetOrCreate(1);
        var breaker = _registry.TryGet(1);

        Assert.NotNull(breaker);
    }

    [Fact]
    public void TryGet_NonExistentServiceId_ReturnsNull()
    {
        var breaker = _registry.TryGet(999);
        Assert.Null(breaker);
    }

    [Fact]
    public void Reset_ExistingServiceId_DoesNotThrow()
    {
        _registry.GetOrCreate(1);

        var exception = Record.Exception(() => _registry.Reset(1));

        Assert.Null(exception);
    }

    [Fact]
    public void Reset_NonExistentServiceId_DoesNotThrow()
    {
        var exception = Record.Exception(() => _registry.Reset(999));

        Assert.Null(exception);
    }

    [Fact]
    public void GetAllStates_ReturnsAllRegisteredBreakers()
    {
        _registry.GetOrCreate(1);
        _registry.GetOrCreate(2);

        var states = _registry.GetAllStates();

        Assert.Equal(2, states.Count);
        Assert.True(states.ContainsKey(1));
        Assert.True(states.ContainsKey(2));
    }
}
