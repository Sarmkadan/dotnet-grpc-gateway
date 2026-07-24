#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Infrastructure;

/// <summary>
/// Configuration for retrying transient repository/upstream failures with
/// exponential backoff and jitter. Sits alongside <see cref="CircuitBreakerOptions"/>
/// as shared resilience configuration.
/// </summary>
public class RetryPolicyOptions
{
    /// <summary>Maximum number of attempts, including the initial one, before giving up.</summary>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>Base delay used to compute the exponential backoff for the first retry.</summary>
    public TimeSpan BaseDelay { get; set; } = TimeSpan.FromMilliseconds(100);

    /// <summary>Upper bound applied to the computed backoff delay for any single attempt.</summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>Maximum wall-clock time allowed across all attempts before the operation is abandoned.</summary>
    public TimeSpan TotalTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Maximum random jitter fraction (0.0-1.0) applied on top of each computed backoff delay.</summary>
    public double JitterFactor { get; set; } = 0.25;
}
