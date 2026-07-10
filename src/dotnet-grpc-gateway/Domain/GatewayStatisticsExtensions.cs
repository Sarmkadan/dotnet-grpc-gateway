using System;
using System.Collections.Generic;
using System.Linq;

namespace DotnetGrpcGateway.Domain
{
    public static class GatewayStatisticsExtensions
    {
        /// <summary>
        /// Calculates the total number of errors across all error types
        /// </summary>
        public static int GetTotalErrors(this GatewayStatistics statistics)
        {
            return statistics.ErrorsByType.Values.Sum();
        }

        /// <summary>
        /// Returns the top N services by request count
        /// </summary>
        public static List<KeyValuePair<string, long>> GetTopServicesByRequestCount(
            this GatewayStatistics statistics, 
            int topN = 5)
        {
            return statistics.RequestsByService
                .OrderByDescending(kvp => kvp.Value)
                .Take(topN)
                .ToList();
        }

        /// <summary>
        /// Formats total data processed into human-readable format
        /// </summary>
        public static string GetFormattedDataProcessed(this GatewayStatistics statistics)
        {
            if (statistics.TotalDataProcessedBytes < 1024)
                return $"{statistics.TotalDataProcessedBytes} B";
            
            var kb = statistics.TotalDataProcessedBytes / 1024;
            if (kb < 1024)
                return $"{kb:F1} KB";
                
            var mb = kb / 1024;
            if (mb < 1024)
                return $"{mb:F1} MB";
                
            var gb = mb / 1024;
            return $"{gb:F1} GB";
        }

        /// <summary>
        /// Determines if gateway is healthy based on service health ratio
        /// </summary>
        public static bool IsGatewayHealthy(this GatewayStatistics statistics, double healthyThreshold = 0.9)
        {
            if (statistics.TotalServices == 0)
                return true;
                
            return (double)statistics.HealthyServices / statistics.TotalServices >= healthyThreshold;
        }
    }
}
