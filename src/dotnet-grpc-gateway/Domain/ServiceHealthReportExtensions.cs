using System;
using System.Collections.Generic;
using System.Linq;

namespace dotnet_grpc_gateway.Domain;

public static class ServiceHealthReportExtensions
{
    public static bool IsUnhealthyForLongTime(this ServiceHealthReport report, TimeSpan threshold)
    {
        return report.IsHealthy == false && report.LastCheckAt.Add(threshold) < DateTime.UtcNow;
    }

    public static double CalculateAverageResponseTime(this ServiceHealthReport report)
    {
        if (report.TotalHealthChecks == 0) 
            return 0;

        return report.ResponseTimeMs / report.TotalHealthChecks;
    }

    public static string GetHealthStatusSummary(this ServiceHealthReport report)
    {
        if (report.IsHealthy) 
            return $"Healthy ({report.SuccessfulHealthChecks} successful checks)";

        return $"Unhealthy ({report.FailedChecksInARow} failed checks in a row)";
    }
}
