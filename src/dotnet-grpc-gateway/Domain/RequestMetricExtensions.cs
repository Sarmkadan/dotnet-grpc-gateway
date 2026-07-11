using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetGrpcGateway.Domain
{
    /// <summary>
    /// Provides extension methods for <see cref="RequestMetric"/> to format and analyze request metrics.
    /// </summary>
    public static class RequestMetricExtensions
    {
        /// <summary>
        /// Generates a concise summary string for the request metric
        /// </summary>
        /// <param name="metric">The request metric to format.</param>
        /// <returns>A formatted summary string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="metric"/> is null.</exception>
        public static string ToSummaryString(this RequestMetric metric)
        {
            ArgumentNullException.ThrowIfNull(metric);

            return $"[{metric.RecordedAt:HH:mm:ss}] {metric.ServiceName}/{metric.MethodName} " +
                   $"({metric.DurationMs:F2}ms | {metric.RequestSizeBytes}B → {metric.ResponseSizeBytes}B | " +
                   $"{metric.HttpStatusCode} {(string.IsNullOrEmpty(metric.GrpcStatusCode) ? "" : $"[{metric.GrpcStatusCode}]")})";
        }

        /// <summary>
        /// Calculates the average data transfer rate in bytes per second
        /// </summary>
        /// <param name="metric">The request metric to analyze.</param>
        /// <returns>The throughput in bytes per second, or 0 if duration is invalid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="metric"/> is null.</exception>
        public static double CalculateThroughput(this RequestMetric metric)
        {
            ArgumentNullException.ThrowIfNull(metric);

            if (metric.DurationMs <= 0)
                return 0;

            double totalBytes = metric.RequestSizeBytes + metric.ResponseSizeBytes;
            return totalBytes / (metric.DurationMs / 1000.0);
        }

        /// <summary>
        /// Creates a formatted error report if the request failed
        /// </summary>
        /// <param name="metric">The request metric to analyze.</param>
        /// <returns>A formatted error report, or null if the request was successful.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="metric"/> is null.</exception>
        public static string? GetErrorReport(this RequestMetric metric)
        {
            ArgumentNullException.ThrowIfNull(metric);

            if (metric.IsSuccessful)
                return null;

            var sb = new StringBuilder();
            sb.AppendLine($"ERROR in {metric.ServiceName}.{metric.MethodName} (ID: {metric.RequestId})");
            sb.AppendLine($"Status: {metric.HttpStatusCode} {metric.GrpcStatusCode}");
            sb.AppendLine($"Duration: {metric.DurationMs:F2}ms");
            sb.AppendLine($"Client: {metric.ClientIpAddress}");
            sb.AppendLine($"Route: {metric.RouteId}");

            if (!string.IsNullOrEmpty(metric.ErrorMessage))
            {
                sb.AppendLine("\nError Details:");
                sb.AppendLine(metric.ErrorMessage);

                if (!string.IsNullOrEmpty(metric.StackTrace))
                {
                    sb.AppendLine("\nStack Trace:");
                    sb.AppendLine(metric.StackTrace);
                }
            }

            return sb.ToString();
        }
    }
}