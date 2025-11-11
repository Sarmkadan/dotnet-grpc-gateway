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
/// Example 3: Metrics & Monitoring
///
/// This example demonstrates how to collect and analyze metrics including:
/// - Performance statistics
/// - Slow request detection
/// - Error tracking
/// - Real-time latency percentiles
/// </summary>
public class MetricsAndMonitoringExample
{
    private readonly string _gatewayUrl = "http://localhost:5000/api";
    private readonly HttpClient _httpClient;

    public MetricsAndMonitoringExample()
    {
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Get overall performance metrics for the gateway.
    /// </summary>
    public async Task DisplayPerformanceMetricsAsync()
    {
        var response = await _httpClient.GetAsync($"{_gatewayUrl}/metrics/performance");

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(result);
            var root = doc.RootElement;

            Console.WriteLine("\n=== Performance Metrics ===");
            Console.WriteLine($"Throughput: {root.GetProperty("throughputRequestsPerSecond").GetDouble():F2} req/s");
            Console.WriteLine($"Average Latency: {root.GetProperty("averageLatencyMs").GetDouble():F2} ms");
            Console.WriteLine($"P50 Latency: {root.GetProperty("p50LatencyMs").GetDouble():F2} ms");
            Console.WriteLine($"P95 Latency: {root.GetProperty("p95LatencyMs").GetDouble():F2} ms");
            Console.WriteLine($"P99 Latency: {root.GetProperty("p99LatencyMs").GetDouble():F2} ms");
            Console.WriteLine($"Total Requests: {root.GetProperty("totalRequests").GetInt64()}");
            Console.WriteLine($"Error Count: {root.GetProperty("errorCount").GetInt32()}");
            Console.WriteLine($"Error Rate: {root.GetProperty("errorRate").GetDouble():P2}");
        }
    }

    /// <summary>
    /// Get today's statistics.
    /// </summary>
    public async Task DisplayTodayStatisticsAsync()
    {
        var response = await _httpClient.GetAsync($"{_gatewayUrl}/gateway/statistics/today");

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(result);
            var root = doc.RootElement;

            Console.WriteLine("\n=== Today's Statistics ===");
            Console.WriteLine($"Date: {root.GetProperty("date").GetString()}");
            Console.WriteLine($"Total Requests: {root.GetProperty("totalRequests").GetInt32()}");
            Console.WriteLine($"Successful: {root.GetProperty("successfulRequests").GetInt32()}");
            Console.WriteLine($"Failed: {root.GetProperty("failedRequests").GetInt32()}");
            Console.WriteLine($"Error Rate: {root.GetProperty("errorRate").GetDouble():P2}");
            Console.WriteLine($"Average Latency: {root.GetProperty("averageLatencyMs").GetDouble():F2} ms");
            Console.WriteLine($"P99 Latency: {root.GetProperty("p99LatencyMs").GetDouble():F2} ms");
        }
    }

    /// <summary>
    /// Get slow requests exceeding latency threshold.
    /// </summary>
    public async Task DisplaySlowRequestsAsync(int thresholdMs = 1000)
    {
        var response = await _httpClient.GetAsync(
            $"{_gatewayUrl}/metrics/slow?threshold={thresholdMs}"
        );

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(result);

            Console.WriteLine($"\n=== Slow Requests (threshold: {thresholdMs}ms) ===");
            Console.WriteLine($"{"Endpoint",-40} {"Latency",-12} {"Time",-25}");
            Console.WriteLine(new string('-', 80));

            foreach (var element in doc.RootElement.EnumerateArray())
            {
                var endpoint = element.GetProperty("endpoint").GetString();
                var latency = element.GetProperty("latencyMs").GetInt32();
                var timestamp = element.GetProperty("timestamp").GetString();

                Console.WriteLine($"{endpoint,-40} {latency,-12} {timestamp,-25}");
            }
        }
    }

    /// <summary>
    /// Get error distribution by status code.
    /// </summary>
    public async Task DisplayErrorDistributionAsync()
    {
        var response = await _httpClient.GetAsync($"{_gatewayUrl}/metrics/errors");

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(result);
            var root = doc.RootElement;

            Console.WriteLine("\n=== Error Distribution ===");
            Console.WriteLine($"{"Status Code",-15} {"Count",-10} {"Percentage",-15}");
            Console.WriteLine(new string('-', 40));

            foreach (var element in root.GetProperty("errors").EnumerateArray())
            {
                var statusCode = element.GetProperty("statusCode").GetInt32();
                var count = element.GetProperty("count").GetInt32();
                var percentage = element.GetProperty("percentage").GetDouble();

                Console.WriteLine($"{statusCode,-15} {count,-10} {percentage:F1}%");
            }

            Console.WriteLine($"\nTotal Errors: {root.GetProperty("totalErrors").GetInt32()}");
        }
    }

    /// <summary>
    /// Get top endpoints by request volume.
    /// </summary>
    public async Task DisplayTopEndpointsAsync()
    {
        var response = await _httpClient.GetAsync($"{_gatewayUrl}/metrics/endpoints?limit=10");

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(result);

            Console.WriteLine("\n=== Top Endpoints by Volume ===");
            Console.WriteLine($"{"Endpoint",-40} {"Requests",-12} {"Avg Latency",-12} {"Success %",-12}");
            Console.WriteLine(new string('-', 80));

            var count = 0;
            foreach (var element in doc.RootElement.EnumerateArray())
            {
                if (count >= 5) break;

                var endpoint = element.GetProperty("endpoint").GetString();
                var requests = element.GetProperty("requestCount").GetInt32();
                var avgLatency = element.GetProperty("avgLatencyMs").GetDouble();
                var successRate = element.GetProperty("successRate").GetDouble();

                Console.WriteLine(
                    $"{endpoint,-40} {requests,-12} {avgLatency:F2}ms{"",-4} {successRate:P2}{"","",-5}"
                );

                count++;
            }
        }
    }

    /// <summary>
    /// Reset all metrics (careful: this clears all data).
    /// </summary>
    public async Task ResetMetricsAsync()
    {
        var response = await _httpClient.PostAsync(
            $"{_gatewayUrl}/metrics/reset",
            null
        );

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("✓ Metrics reset successfully");
        }
    }

    // Main entry point
    public static async Task Main(string[] args)
    {
        var example = new MetricsAndMonitoringExample();

        Console.WriteLine("=== Metrics & Monitoring Example ===");

        try
        {
            // Step 1: Display performance metrics
            Console.WriteLine("\nStep 1: Retrieving performance metrics...");
            await example.DisplayPerformanceMetricsAsync();
            await Task.Delay(500);

            // Step 2: Display today's statistics
            Console.WriteLine("\nStep 2: Retrieving today's statistics...");
            await example.DisplayTodayStatisticsAsync();
            await Task.Delay(500);

            // Step 3: Display slow requests
            Console.WriteLine("\nStep 3: Retrieving slow requests...");
            await example.DisplaySlowRequestsAsync(500);
            await Task.Delay(500);

            // Step 4: Display error distribution
            Console.WriteLine("\nStep 4: Retrieving error distribution...");
            await example.DisplayErrorDistributionAsync();
            await Task.Delay(500);

            // Step 5: Display top endpoints
            Console.WriteLine("\nStep 5: Retrieving top endpoints...");
            await example.DisplayTopEndpointsAsync();

            Console.WriteLine("\n=== Example complete ===");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Make sure the gateway is running at http://localhost:5000");
        }
    }
}
