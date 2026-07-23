using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetGrpcGateway.Domain
{
    /// <summary>
    /// Provides extension methods for <see cref="GatewayStatistics"/> to calculate and format statistics.
    /// </summary>
    public static class GatewayStatisticsExtensions
    {
        /// <summary>
        /// Calculates the total number of errors across all error types.
        /// </summary>
        /// <param name="statistics">The gateway statistics instance.</param>
        /// <returns>The sum of all error counts.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="statistics"/> is null.</exception>
        public static int GetTotalErrors(this GatewayStatistics statistics)
        {
            ArgumentNullException.ThrowIfNull(statistics);

            return statistics.ErrorsByType.Values.Sum();
        }

        /// <summary>
        /// Returns the top N services by request count.
        /// </summary>
        /// <param name="statistics">The gateway statistics instance.</param>
        /// <param name="topN">The number of top services to return. Defaults to 5.</param>
        /// <returns>A list of key-value pairs representing service names and their request counts, ordered by count descending.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="statistics"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="topN"/> is less than 0.</exception>
        public static List<KeyValuePair<string, long>> GetTopServicesByRequestCount(
            this GatewayStatistics statistics,
            int topN = 5)
        {
            ArgumentNullException.ThrowIfNull(statistics);
            ArgumentOutOfRangeException.ThrowIfNegative(topN);

            return statistics.RequestsByService
                .OrderByDescending(kvp => kvp.Value)
                .Take(topN)
                .ToList();
        }

        /// <summary>
        /// Formats total data processed into human-readable format (B, KB, MB, GB).
        /// </summary>
        /// <param name="statistics">The gateway statistics instance.</param>
        /// <returns>A formatted string representing the data size with appropriate unit.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="statistics"/> is null.</exception>
        public static string GetFormattedDataProcessed(this GatewayStatistics statistics)
        {
            ArgumentNullException.ThrowIfNull(statistics);

            if (statistics.TotalDataProcessedBytes < 1024)
                return $"{statistics.TotalDataProcessedBytes} B";

            var kb = statistics.TotalDataProcessedBytes / 1024.0;
            if (kb < 1024)
                return $"{kb:F1} KB";

            var mb = kb / 1024.0;
            if (mb < 1024)
                return $"{mb:F1} MB";

            var gb = mb / 1024.0;
            return $"{gb:F1} GB";
        }

        /// <summary>
        /// Determines if gateway is healthy based on service health ratio.
        /// </summary>
        /// <param name="statistics">The gateway statistics instance.</param>
        /// <param name="healthyThreshold">The minimum healthy service ratio threshold (0.0 to 1.0). Defaults to 0.9.</param>
        /// <returns>True if the gateway is healthy; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="statistics"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="healthyThreshold"/> is outside the valid range (0.0 to 1.0).</exception>
        public static bool IsGatewayHealthy(this GatewayStatistics statistics, double healthyThreshold = 0.9)
        {
            ArgumentNullException.ThrowIfNull(statistics);
            ArgumentOutOfRangeException.ThrowIfLessThan(healthyThreshold, 0.0);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(healthyThreshold, 1.0);

            if (statistics.TotalServices == 0)
                return true;

            return (double)statistics.HealthyServices / statistics.TotalServices >= healthyThreshold;
        }
    }
}
