#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace DotNetGrpcGateway.Examples;

/// <summary>
/// Extension methods for <see cref="HealthChecksAndMonitoringExample"/> providing additional
/// monitoring and health check functionality.
/// </summary>
public static class HealthChecksAndMonitoringExampleExtensions
{
    /// <summary>
    /// Check health status of multiple services by their IDs and return a summary report.
    /// </summary>
    /// <param name="example">The <see cref="HealthChecksAndMonitoringExample"/> instance</param>
    /// <param name="serviceIds">List of service IDs to check</param>
    /// <returns>Dictionary mapping service ID to health status</returns>
    /// <exception cref="ArgumentNullException"><paramref name="example"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="serviceIds"/> is <see langword="null"/></exception>
    public static async Task<Dictionary<int, bool>> CheckMultipleServicesHealthAsync(this HealthChecksAndMonitoringExample example, IEnumerable<int> serviceIds)
    {
        ArgumentNullException.ThrowIfNull(example);
        ArgumentNullException.ThrowIfNull(serviceIds);

        var results = new Dictionary<int, bool>();

        foreach (var serviceId in serviceIds)
        {
            var response = await example.CheckServiceHealthAsync(serviceId);
            results[serviceId] = response.IsHealthy;
        }

        return results;
    }

    /// <summary>
    /// Get formatted health status report as a string for logging or display purposes.
    /// </summary>
    /// <param name="example">The <see cref="HealthChecksAndMonitoringExample"/> instance</param>
    /// <param name="includeMetrics">Whether to include detailed metrics in the report</param>
    /// <returns>Formatted health status report string</returns>
    /// <exception cref="ArgumentNullException"><paramref name="example"/> is <see langword="null"/></exception>
    public static async Task<string> GetHealthReportAsync(this HealthChecksAndMonitoringExample example, bool includeMetrics = true)
    {
        ArgumentNullException.ThrowIfNull(example);

        var report = new System.Text.StringBuilder();
        report.AppendLine("=== Gateway Health Report ===");

        // Gateway health
        var gatewayHealth = await example.CheckGatewayHealthAsync();
        report.AppendLine($"Gateway Status: {(gatewayHealth ? "✓ Healthy" : "✗ Unhealthy")}");

        if (includeMetrics)
        {
            var detailedStatus = await example.GetDetailedHealthStatusAsync();
            report.AppendLine($"Uptime: {detailedStatus.Uptime}");
            report.AppendLine($"Requests Processed: {detailedStatus.RequestsProcessed}");
            report.AppendLine($"Active Connections: {detailedStatus.ActiveConnections}");
            report.AppendLine($"Cache Hit Rate: {detailedStatus.CacheHitRate:P2}");
        }

        return report.ToString();
    }

    /// <summary>
    /// Check if all specified services are healthy.
    /// </summary>
    /// <param name="example">The <see cref="HealthChecksAndMonitoringExample"/> instance</param>
    /// <param name="serviceIds">List of service IDs to check</param>
    /// <returns>True if all services are healthy, false otherwise</returns>
    /// <exception cref="ArgumentNullException"><paramref name="example"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="serviceIds"/> is <see langword="null"/></exception>
    public static async Task<bool> AreAllServicesHealthyAsync(this HealthChecksAndMonitoringExample example, IEnumerable<int> serviceIds)
    {
        ArgumentNullException.ThrowIfNull(example);
        ArgumentNullException.ThrowIfNull(serviceIds);

        var results = await example.CheckMultipleServicesHealthAsync(serviceIds);
        return results.Values.All(isHealthy => isHealthy);
    }

    /// <summary>
    /// Get detailed health status with structured data.
    /// </summary>
    /// <param name="example">The <see cref="HealthChecksAndMonitoringExample"/> instance</param>
    /// <returns>Structured health status data</returns>
    /// <exception cref="ArgumentNullException"><paramref name="example"/> is <see langword="null"/></exception>
    /// <exception cref="HttpRequestException">Failed to get detailed health status</exception>
    /// <exception cref="JsonException">Response is not valid JSON</exception>
    public static async Task<HealthStatus> GetDetailedHealthStatusAsync(this HealthChecksAndMonitoringExample example)
    {
        ArgumentNullException.ThrowIfNull(example);

        var response = await example.GetHttpClient().GetAsync("http://localhost:5000/api/health/status");

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException("Failed to get detailed health status");
        }

        var result = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;

        return new HealthStatus
        {
            Status = root.GetProperty("status").GetString(),
            Uptime = root.GetProperty("uptime").GetString(),
            RequestsProcessed = root.GetProperty("requestsProcessed").GetInt64(),
            ActiveConnections = root.GetProperty("activeConnections").GetInt32(),
            CacheHitRate = root.GetProperty("cacheHitRate").GetDouble()
        };
    }

    /// <summary>
    /// Check health status of a specific service by ID.
    /// </summary>
    /// <param name="example">The <see cref="HealthChecksAndMonitoringExample"/> instance</param>
    /// <param name="serviceId">Service ID to check</param>
    /// <returns>Service health status</returns>
    /// <exception cref="ArgumentNullException"><paramref name="example"/> is <see langword="null"/></exception>
    /// <exception cref="HttpRequestException">Failed to get service health status</exception>
    /// <exception cref="JsonException">Response is not valid JSON</exception>
    public static async Task<ServiceHealth> CheckServiceHealthAsync(this HealthChecksAndMonitoringExample example, int serviceId)
    {
        ArgumentNullException.ThrowIfNull(example);

        var response = await example.GetHttpClient().GetAsync($"http://localhost:5000/api/health/services/{serviceId}");

        if (!response.IsSuccessStatusCode)
        {
            return new ServiceHealth
            {
                ServiceId = serviceId,
                IsHealthy = false,
                ServiceName = "Unknown",
                ResponseTimeMs = -1,
                LastCheckAt = DateTime.MinValue.ToString()
            };
        }

        var result = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;

        return new ServiceHealth
        {
            ServiceId = serviceId,
            ServiceName = root.GetProperty("serviceName").GetString(),
            IsHealthy = root.GetProperty("isHealthy").GetBoolean(),
            ResponseTimeMs = root.GetProperty("responseTimeMs").GetInt32(),
            LastCheckAt = root.GetProperty("lastCheckAt").GetString(),
            ConsecutiveFailures = root.GetProperty("consecutiveFailures").GetInt32(),
            FailureThreshold = root.GetProperty("failureThreshold").GetInt32()
        };
    }

    /// <summary>
    /// Get the HttpClient instance used by the example for health checks.
    /// </summary>
    /// <param name="example">The <see cref="HealthChecksAndMonitoringExample"/> instance</param>
    /// <returns>HttpClient instance</returns>
    /// <exception cref="ArgumentNullException"><paramref name="example"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">Failed to access HttpClient via reflection</exception>
    public static HttpClient GetHttpClient(this HealthChecksAndMonitoringExample example)
    {
        ArgumentNullException.ThrowIfNull(example);

        // Use reflection to access the private _httpClient field
        var field = typeof(HealthChecksAndMonitoringExample).GetField(
            "_httpClient",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
        );

        return field?.GetValue(example) as HttpClient
            ?? throw new InvalidOperationException("Failed to access HttpClient via reflection");
    }
}

/// <summary>
/// Represents detailed health status data.
/// </summary>
public sealed class HealthStatus
{
    public string? Status { get; set; }
    public string? Uptime { get; set; }
    public long RequestsProcessed { get; set; }
    public int ActiveConnections { get; set; }
    public double CacheHitRate { get; set; }
}

/// <summary>
/// Represents service health status data.
/// </summary>
public sealed class ServiceHealth
{
    public int ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public bool IsHealthy { get; set; }
    public int ResponseTimeMs { get; set; }
    public string? LastCheckAt { get; set; }
    public int ConsecutiveFailures { get; set; }
    public int FailureThreshold { get; set; }
}