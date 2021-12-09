// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace DotNetGrpcGateway.Examples;

/// <summary>
/// Example 5: Health Checks & Monitoring
///
/// This example demonstrates:
/// - Gateway health status
/// - Service health monitoring
/// - Readiness probes (for load balancers)
/// - Liveness probes (for Kubernetes)
/// - Health status polling
/// </summary>
public class HealthChecksAndMonitoringExample
{
    private readonly string _gatewayUrl = "http://localhost:5000";
    private readonly HttpClient _httpClient;

    public HealthChecksAndMonitoringExample()
    {
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Check basic gateway health status.
    /// </summary>
    public async Task CheckGatewayHealthAsync()
    {
        var response = await _httpClient.GetAsync($"{_gatewayUrl}/health");

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(result);
            var status = doc.RootElement.GetProperty("status").GetString();

            Console.WriteLine($"Gateway Health: {status}");
            return;
        }

        Console.WriteLine("✗ Gateway is not responding");
    }

    /// <summary>
    /// Get detailed health status including uptime and metrics.
    /// </summary>
    public async Task DisplayDetailedHealthStatusAsync()
    {
        var response = await _httpClient.GetAsync($"{_gatewayUrl}/api/health/status");

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(result);
            var root = doc.RootElement;

            Console.WriteLine("\n=== Detailed Gateway Health ===");
            Console.WriteLine($"Status: {root.GetProperty("status").GetString()}");
            Console.WriteLine($"Uptime: {root.GetProperty("uptime").GetString()}");
            Console.WriteLine($"Requests Processed: {root.GetProperty("requestsProcessed").GetInt64()}");
            Console.WriteLine($"Active Connections: {root.GetProperty("activeConnections").GetInt32()}");
            Console.WriteLine($"Cache Hit Rate: {root.GetProperty("cacheHitRate").GetDouble():P2}");
        }
    }

    /// <summary>
    /// Get health status of all registered services.
    /// </summary>
    public async Task DisplayAllServicesHealthAsync()
    {
        var response = await _httpClient.GetAsync($"{_gatewayUrl}/api/health/services");

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(result);

            Console.WriteLine("\n=== Services Health Status ===");
            Console.WriteLine($"{"Service ID",-12} {"Service Name",-30} {"Status",-12} {"Response Time",-15}");
            Console.WriteLine(new string('-', 70));

            foreach (var element in doc.RootElement.EnumerateArray())
            {
                var serviceId = element.GetProperty("serviceId").GetInt32();
                var serviceName = element.GetProperty("serviceName").GetString();
                var isHealthy = element.GetProperty("isHealthy").GetBoolean();
                var responseTime = "N/A";

                if (element.TryGetProperty("responseTimeMs", out var rtElement))
                {
                    responseTime = $"{rtElement.GetInt32()}ms";
                }

                var status = isHealthy ? "✓ Healthy" : "✗ Unhealthy";

                Console.WriteLine(
                    $"{serviceId,-12} {serviceName,-30} {status,-12} {responseTime,-15}"
                );
            }
        }
    }

    /// <summary>
    /// Get detailed health information for a specific service.
    /// </summary>
    public async Task DisplayServiceHealthDetailAsync(int serviceId)
    {
        var response = await _httpClient.GetAsync($"{_gatewayUrl}/api/health/services/{serviceId}");

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(result);
            var root = doc.RootElement;

            Console.WriteLine($"\n=== Service {serviceId} Health Details ===");
            Console.WriteLine($"Service: {root.GetProperty("serviceName").GetString()}");
            Console.WriteLine($"Status: {(root.GetProperty("isHealthy").GetBoolean() ? "Healthy" : "Unhealthy")}");
            Console.WriteLine($"Last Check: {root.GetProperty("lastCheckAt").GetString()}");
            Console.WriteLine($"Response Time: {root.GetProperty("responseTimeMs").GetInt32()}ms");
            Console.WriteLine($"Consecutive Failures: {root.GetProperty("consecutiveFailures").GetInt32()}");
            Console.WriteLine($"Failure Threshold: {root.GetProperty("failureThreshold").GetInt32()}");
        }
        else
        {
            Console.WriteLine($"Service {serviceId} not found");
        }
    }

    /// <summary>
    /// Check if gateway is ready to serve traffic (used by load balancers).
    /// </summary>
    public async Task CheckReadinessAsync()
    {
        var response = await _httpClient.GetAsync($"{_gatewayUrl}/api/health/ready");

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(result);
            var isReady = doc.RootElement.GetProperty("ready").GetBoolean();

            Console.WriteLine($"\nReadiness Probe: {(isReady ? "✓ Ready" : "✗ Not Ready")}");
            Console.WriteLine($"Reason: {doc.RootElement.GetProperty("reason").GetString()}");
        }
    }

    /// <summary>
    /// Check if gateway process is alive (used by Kubernetes).
    /// </summary>
    public async Task CheckLivenessAsync()
    {
        var response = await _httpClient.GetAsync($"{_gatewayUrl}/api/health/live");

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(result);
            var isAlive = doc.RootElement.GetProperty("alive").GetBoolean();

            Console.WriteLine($"Liveness Probe: {(isAlive ? "✓ Alive" : "✗ Dead")}");
        }
    }

    /// <summary>
    /// Poll health status over a period of time (for monitoring).
    /// </summary>
    public async Task PollHealthStatusAsync(int intervalSeconds = 5, int durationSeconds = 30)
    {
        Console.WriteLine($"\n=== Health Status Polling ({durationSeconds}s, every {intervalSeconds}s) ===");

        var endTime = DateTime.Now.AddSeconds(durationSeconds);
        var pollCount = 0;

        while (DateTime.Now < endTime)
        {
            pollCount++;
            await CheckGatewayHealthAsync();
            await Task.Delay(intervalSeconds * 1000);
        }

        Console.WriteLine($"Polling completed: {pollCount} checks");
    }

    // Main entry point
    public static async Task Main(string[] args)
    {
        var example = new HealthChecksAndMonitoringExample();

        Console.WriteLine("=== Health Checks & Monitoring Example ===");

        try
        {
            // Step 1: Basic health check
            Console.WriteLine("\nStep 1: Checking gateway health...");
            await example.CheckGatewayHealthAsync();
            await Task.Delay(1000);

            // Step 2: Detailed health status
            Console.WriteLine("\nStep 2: Getting detailed health status...");
            await example.DisplayDetailedHealthStatusAsync();
            await Task.Delay(1000);

            // Step 3: All services health
            Console.WriteLine("\nStep 3: Checking all services health...");
            await example.DisplayAllServicesHealthAsync();
            await Task.Delay(1000);

            // Step 4: Specific service health
            Console.WriteLine("\nStep 4: Getting service 1 health details...");
            await example.DisplayServiceHealthDetailAsync(1);
            await Task.Delay(1000);

            // Step 5: Readiness probe
            Console.WriteLine("\nStep 5: Checking readiness probe...");
            await example.CheckReadinessAsync();
            await Task.Delay(1000);

            // Step 6: Liveness probe
            Console.WriteLine("\nStep 6: Checking liveness probe...");
            await example.CheckLivenessAsync();
            await Task.Delay(1000);

            // Step 7: Poll health status
            Console.WriteLine("\nStep 7: Polling health status for 15 seconds...");
            await example.PollHealthStatusAsync(intervalSeconds: 3, durationSeconds: 15);

            Console.WriteLine("\n=== Example complete ===");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Make sure the gateway is running at http://localhost:5000");
        }
    }
}
