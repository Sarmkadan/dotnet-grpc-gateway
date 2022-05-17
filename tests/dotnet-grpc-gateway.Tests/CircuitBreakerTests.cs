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

/// <summary>
/// Tests for the <see cref="CircuitBreaker"/> class.
/// </summary>
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

    /// <summary>
    /// Verifies that the initial state of the circuit breaker is closed.
    /// </summary>
    [Fact]
    public void InitialState_IsClosed()
    {
        var sut = CreateSut();

        sut.State.Should().Be(CircuitBreakerState.Closed);
        sut.AllowRequest().Should().BeTrue();
    }

    /// <summary>
    /// Ensures that recording failures below the threshold keeps the circuit closed and allows requests.
    /// </summary>
    [Fact]
    public void RecordFailure_BelowThreshold_RemainsClosedAndAllowsRequests()
    {
        var sut = CreateSut(failureThreshold: 3);
        sut.RecordFailure();
        sut.RecordFailure();

        sut.State.Should().Be(CircuitBreakerState.Closed);
        sut.AllowRequest().Should().BeTrue();
    }

    /// <summary>
    /// Confirms that recording failures up to the threshold opens the circuit.
    /// </summary>
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

    /// <summary>
    /// Tests that a half‑open circuit closes after the configured number of successful requests.
    /// </summary>
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

    /// <summary>
    /// Verifies that a failure during a half‑open state re‑opens the circuit.
    /// </summary>
    [Fact]
    public void RecordFailure_WhenHalfOpen_ReOpensCircuit()
    {
        var sut = CreateSut(failureThreshold: 1, openDurationSeconds: 0);
        sut.RecordFailure();

        sut.AllowRequest();
        sut.RecordFailure();

        sut.State.Should().Be(CircuitBreakerState.Open);
    }

    /// <summary>
    /// Ensures that resetting an open circuit closes it and clears counters.
    /// </summary>
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
