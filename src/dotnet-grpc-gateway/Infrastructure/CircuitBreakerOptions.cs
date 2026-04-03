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
    public int FailureThreshold { get; set; } = 5;

    /// <summary>Duration the circuit stays open before entering the half-open state.</summary>
    public TimeSpan OpenDuration { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Number of successful calls in half-open state required to close the circuit.</summary>
    public int HalfOpenSuccessThreshold { get; set; } = 2;
}
