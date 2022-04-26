using System;

namespace DotNetGrpcGateway.Domain;

/// <summary>
/// Provides extension methods for <see cref="ServiceHealthReport"/> to simplify health check analysis and reporting.
/// </summary>
public static class ServiceHealthReportExtensions
{
    /// <summary>
    /// Determines whether the service has been unhealthy for longer than the specified threshold.
    /// </summary>
    /// <param name="report">The health report to analyze. Cannot be null.</param>
    /// <param name="threshold">The time span that constitutes "too long".</param>
    /// <returns>True if the service is unhealthy and has been unhealthy for longer than the threshold; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="report"/> is null.</exception>
    public static bool IsUnhealthyForLongTime(this ServiceHealthReport report, TimeSpan threshold)
    {
        ArgumentNullException.ThrowIfNull(report);

        return !report.IsHealthy && report.LastCheckAt.Add(threshold) < DateTime.UtcNow;
    }

    /// <summary>
    /// Calculates the average response time across all health checks.
    /// </summary>
    /// <param name="report">The health report containing response time data. Cannot be null.</param>
    /// <returns>The average response time in milliseconds, or 0 if no health checks have been performed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="report"/> is null.</exception>
    public static double CalculateAverageResponseTime(this ServiceHealthReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        return report.TotalHealthChecks == 0 ? 0 : (double)report.ResponseTimeMs / report.TotalHealthChecks;
    }

    /// <summary>
    /// Generates a human-readable summary of the service health status.
    /// </summary>
    /// <param name="report">The health report to summarize. Cannot be null.</param>
    /// <returns>A formatted string describing the health status and relevant statistics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="report"/> is null.</exception>
    public static string GetHealthStatusSummary(this ServiceHealthReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        return report.IsHealthy
            ? $"Healthy ({report.SuccessfulHealthChecks} successful checks)"
            : $"Unhealthy ({report.FailedChecksInARow} failed checks in a row)";
    }
}