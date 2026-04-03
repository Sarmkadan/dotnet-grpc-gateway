#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using DotNetGrpcGateway.Infrastructure;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class CircuitBreakerTests
{
    private readonly Mock<ILogger<CircuitBreaker>> _logger = new();

    private CircuitBreaker CreateSut(int failureThreshold = 3, int openDurationSeconds = 30) =>
        new(
            serviceId: 1,
            new CircuitBreakerOptions
            {
                FailureThreshold = failureThreshold,
                OpenDuration = TimeSpan.FromSeconds(openDurationSeconds),
                HalfOpenSuccessThreshold = 2
            },
            _logger.Object);

    [Fact]
    public void InitialState_IsClosed()
    {
        var sut = CreateSut();

        sut.State.Should().Be(CircuitBreakerState.Closed);
        sut.AllowRequest().Should().BeTrue();
    }

    [Fact]
    public void RecordFailure_BelowThreshold_RemainsClosedAndAllowsRequests()
    {
        var sut = CreateSut(failureThreshold: 3);
        sut.RecordFailure();
        sut.RecordFailure();

        sut.State.Should().Be(CircuitBreakerState.Closed);
        sut.AllowRequest().Should().BeTrue();
    }

    [Fact]
    public void RecordFailure_ReachesThreshold_OpensCircuit()
    {
        var sut = CreateSut(failureThreshold: 3);
        sut.RecordFailure();
        sut.RecordFailure();
        sut.RecordFailure();

        sut.State.Should().Be(CircuitBreakerState.Open);
        sut.AllowRequest().Should().BeFalse();
    }

    [Fact]
    public void RecordSuccess_WhenHalfOpen_ClosesCircuitAfterThreshold()
    {
        var sut = CreateSut(failureThreshold: 1, openDurationSeconds: 0);
        sut.RecordFailure();

        sut.AllowRequest().Should().BeTrue("open duration is 0s, should transition to HalfOpen");
        sut.State.Should().Be(CircuitBreakerState.HalfOpen);

        sut.RecordSuccess();
        sut.RecordSuccess();

        sut.State.Should().Be(CircuitBreakerState.Closed);
    }

    [Fact]
    public void RecordFailure_WhenHalfOpen_ReOpensCircuit()
    {
        var sut = CreateSut(failureThreshold: 1, openDurationSeconds: 0);
        sut.RecordFailure();

        sut.AllowRequest();
        sut.RecordFailure();

        sut.State.Should().Be(CircuitBreakerState.Open);
    }

    [Fact]
    public void Reset_OpenCircuit_ClosesAndClearsCounters()
    {
        var sut = CreateSut(failureThreshold: 2);
        sut.RecordFailure();
        sut.RecordFailure();
        sut.State.Should().Be(CircuitBreakerState.Open);

        sut.Reset();

        sut.State.Should().Be(CircuitBreakerState.Closed);
        sut.ConsecutiveFailures.Should().Be(0);
        sut.OpenedAt.Should().BeNull();
    }
}
