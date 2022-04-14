#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Domain;

/// <summary>
/// Provides validation helpers for <see cref="ServiceHealthReport"/> instances
/// </summary>
public static class ServiceHealthReportValidation
{
    /// <summary>
    /// Validates the specified health report and returns a list of validation problems.
    /// Returns an empty list if the report is valid.
    /// </summary>
    /// <param name="value">The health report to validate</param>
    /// <returns>List of human-readable validation problems</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> ValidateReport(this ServiceHealthReport value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate required properties
        if (value.ServiceId <= 0)
            problems.Add("Service ID must be a positive integer");

        if (string.IsNullOrWhiteSpace(value.HealthStatus))
            problems.Add("Health status must be specified");
        else if (value.HealthStatus.Length > 50)
            problems.Add("Health status must not exceed 50 characters");

        // Validate numeric ranges
        if (value.ResponseTimeMs < 0)
            problems.Add("Response time cannot be negative");

        if (value.HttpStatusCode < 0 || value.HttpStatusCode > 999)
            problems.Add("HTTP status code must be between 0 and 999");

        if (value.SuccessfulChecksInARow < 0)
            problems.Add("Successful checks in a row cannot be negative");

        if (value.FailedChecksInARow < 0)
            problems.Add("Failed checks in a row cannot be negative");

        if (value.TotalHealthChecks < 0)
            problems.Add("Total health checks cannot be negative");

        if (value.SuccessfulHealthChecks < 0)
            problems.Add("Successful health checks cannot be negative");

        if (value.TotalHealthChecks > 0 && value.SuccessfulHealthChecks > value.TotalHealthChecks)
            problems.Add("Successful health checks cannot exceed total health checks");

        // Validate success rate
        if (value.HealthCheckSuccessRate < 0 || value.HealthCheckSuccessRate > 100)
            problems.Add("Health check success rate must be between 0 and 100");

        if (value.TotalHealthChecks > 0)
        {
            var calculatedRate = (double)value.SuccessfulHealthChecks / value.TotalHealthChecks * 100;
            if (Math.Abs(value.HealthCheckSuccessRate - calculatedRate) > 0.001)
                problems.Add("Health check success rate does not match calculated value from checks");
        }

        // Validate dates
        var now = DateTime.UtcNow;
        if (value.LastCheckAt == default)
            problems.Add("Last check timestamp must be specified");
        else if (value.LastCheckAt > now.AddMinutes(5))
            problems.Add("Last check timestamp cannot be in the future");
        else if (value.LastCheckAt < now.AddYears(-1))
            problems.Add("Last check timestamp cannot be more than one year in the past");

        if (value.NextCheckScheduledAt == default)
            problems.Add("Next check scheduled timestamp must be specified");
        else if (value.NextCheckScheduledAt < now.AddMinutes(-5))
            problems.Add("Next check cannot be scheduled in the past");
        else if (value.NextCheckScheduledAt > now.AddYears(1))
            problems.Add("Next check cannot be scheduled more than one year in the future");

        if (value.NextCheckScheduledAt < value.LastCheckAt)
            problems.Add("Next check cannot be scheduled before last check");

        if (value.ReportedAt == default)
            problems.Add("Reported timestamp must be specified");
        else if (value.ReportedAt > now.AddMinutes(5))
            problems.Add("Reported timestamp cannot be in the future");
        else if (value.ReportedAt < now.AddYears(-1))
            problems.Add("Reported timestamp cannot be more than one year in the past");

        // Validate endpoint if present
        if (!string.IsNullOrWhiteSpace(value.HealthCheckEndpoint))
        {
            if (value.HealthCheckEndpoint.Length > 500)
                problems.Add("Health check endpoint must not exceed 500 characters");

            if (!Uri.IsWellFormedUriString(value.HealthCheckEndpoint, UriKind.Absolute) &&
                !Uri.IsWellFormedUriString(value.HealthCheckEndpoint, UriKind.Relative))
                problems.Add("Health check endpoint must be a valid URI");
        }

        // Validate error message and stack trace consistency
        if (value.IsHealthy && !string.IsNullOrEmpty(value.ErrorMessage))
            problems.Add("Healthy services should not have error messages");

        if (value.IsHealthy && !string.IsNullOrEmpty(value.StackTrace))
            problems.Add("Healthy services should not have stack traces");

        if (!value.IsHealthy && string.IsNullOrEmpty(value.ErrorMessage))
            problems.Add("Unhealthy services must have error messages");

        // Validate diagnostic messages
        if (value.DiagnosticMessages.Count > 10)
            problems.Add("Diagnostic messages cannot exceed 10 entries");

        foreach (var message in value.DiagnosticMessages)
        {
            if (string.IsNullOrWhiteSpace(message))
                problems.Add("Diagnostic messages cannot be null or empty");
            else if (message.Length > 200)
                problems.Add("Diagnostic messages cannot exceed 200 characters");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified health report is valid.
    /// </summary>
    /// <param name="value">The health report to check</param>
    /// <returns>True if valid; otherwise false</returns>
    public static bool IsValid(this ServiceHealthReport value)
    {
        return !value.ValidateReport().Any();
    }

    /// <summary>
    /// Validates the specified health report and throws an <see cref="ArgumentException"/>
    /// if it contains validation problems.
    /// </summary>
    /// <param name="value">The health report to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails with detailed problem list</exception>
    public static void EnsureValid(this ServiceHealthReport value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.ValidateReport();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Service health report validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }
}