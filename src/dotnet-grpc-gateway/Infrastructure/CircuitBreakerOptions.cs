#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Infrastructure;

/// <summary>
/// Configuration for an individual circuit breaker instance.
/// </summary>
public class CircuitBreakerOptions
{
    /// <summary>Number of consecutive failures required to open the circuit.</summary>
    /// <value>The failure threshold count.</value>
    public int FailureThreshold { get; set; } = 3;

    /// <summary>Duration the circuit stays open before entering the half-open state.</summary>
    /// <value>The open duration as a TimeSpan.</value>
    public TimeSpan OpenDuration { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Number of successful calls in half-open state required to close the circuit.</summary>
    /// <value>The half-open success threshold count.</value>
    public int HalfOpenSuccessThreshold { get; set; } = 2;

    public override string ToString() => $"CircuitBreakerOptions {{ FailureThreshold = {FailureThreshold}, OpenDuration = {OpenDuration}, HalfOpenSuccessThreshold = {HalfOpenSuccessThreshold} }}";
}