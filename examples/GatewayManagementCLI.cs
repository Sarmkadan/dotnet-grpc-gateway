// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DotNetGrpcGateway.Examples;

/// <summary>
/// Gateway Management CLI
///
/// A simple command-line interface for managing the gateway.
/// Demonstrates all major API operations.
/// </summary>
public class GatewayManagementCLI
{
    private readonly string _gatewayUrl = "http://localhost:5000/api";
    private readonly HttpClient _httpClient;
    private bool _running = true;

    public GatewayManagementCLI()
    {
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Display main menu.
    /// </summary>
    private void DisplayMenu()
    {
        Console.WriteLine("\n=== Gateway Management CLI ===");
        Console.WriteLine("1. List services");
        Console.WriteLine("2. Register service");
        Console.WriteLine("3. Check service health");
        Console.WriteLine("4. List routes");
        Console.WriteLine("5. Create route");
        Console.WriteLine("6. View metrics");
        Console.WriteLine("7. Check gateway health");
        Console.WriteLine("8. View configuration");
        Console.WriteLine("9. Exit");
        Console.Write("\nSelect option: ");
    }

    /// <summary>
    /// List all registered services.
    /// </summary>
    private async Task ListServicesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_gatewayUrl}/gateway/services");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(result);

                Console.WriteLine("\n=== Registered Services ===");
                Console.WriteLine($"{"ID",-5} {"Name",-25} {"Host",-20} {"Port",-6} {"Status",-12}");
                Console.WriteLine(new string('-', 70));

                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var service in doc.RootElement.EnumerateArray())
                    {
                        var id = service.GetProperty("id").GetInt32();
                        var name = service.GetProperty("name").GetString();
                        var host = service.GetProperty("host").GetString();
                        var port = service.GetProperty("port").GetInt32();
                        var isHealthy = service.GetProperty("isHealthy").GetBoolean();

                        Console.WriteLine(
                            $"{id,-5} {name,-25} {host,-20} {port,-6} {(isHealthy ? "Healthy" : "Unhealthy"),-12}"
                        );
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Register a new service.
    /// </summary>
    private async Task RegisterServiceAsync()
    {
        Console.Write("Service name: ");
        var name = Console.ReadLine();

        Console.Write("Full service name (e.g., user.UserService): ");
        var fullName = Console.ReadLine();

        Console.Write("Host: ");
        var host = Console.ReadLine();

        Console.Write("Port: ");
        if (!int.TryParse(Console.ReadLine(), out var port))
        {
            Console.WriteLine("Invalid port");
            return;
        }

        Console.Write("Use TLS? (y/n): ");
        var useTls = Console.ReadLine()?.ToLower() == "y";

        try
        {
            var serviceData = new
            {
                name,
                serviceFullName = fullName,
                host,
                port,
                useTls,
                healthCheckIntervalSeconds = 30
            };

            var content = new StringContent(
                JsonSerializer.Serialize(serviceData),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync($"{_gatewayUrl}/gateway/services", content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("✓ Service registered successfully");
                await ListServicesAsync();
            }
            else
            {
                Console.WriteLine($"✗ Error: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Check health of a specific service.
    /// </summary>
    private async Task CheckServiceHealthAsync()
    {
        Console.Write("Service ID: ");
        if (!int.TryParse(Console.ReadLine(), out var serviceId))
        {
            Console.WriteLine("Invalid service ID");
            return;
        }

        try
        {
            var response = await _httpClient.GetAsync($"{_gatewayUrl}/health/services/{serviceId}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(result);
                var root = doc.RootElement;

                Console.WriteLine("\n=== Service Health ===");
                Console.WriteLine($"Service: {root.GetProperty("serviceName").GetString()}");
                Console.WriteLine($"Status: {(root.GetProperty("isHealthy").GetBoolean() ? "Healthy" : "Unhealthy")}");
                Console.WriteLine($"Last Check: {root.GetProperty("lastCheckAt").GetString()}");
                Console.WriteLine($"Response Time: {root.GetProperty("responseTimeMs").GetInt32()}ms");
                Console.WriteLine($"Consecutive Failures: {root.GetProperty("consecutiveFailures").GetInt32()}");
            }
            else
            {
                Console.WriteLine($"Service not found");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// List all routes.
    /// </summary>
    private async Task ListRoutesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_gatewayUrl}/gateway/routes");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(result);

                Console.WriteLine("\n=== Routes ===");
                Console.WriteLine($"{"ID",-5} {"Pattern",-30} {"Priority",-10} {"Rate Limit",-12}");
                Console.WriteLine(new string('-', 60));

                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var route in doc.RootElement.EnumerateArray())
                    {
                        var id = route.GetProperty("id").GetInt32();
                        var pattern = route.GetProperty("pattern").GetString();
                        var priority = route.GetProperty("priority").GetInt32();
                        var rateLimit = route.GetProperty("rateLimitPerMinute").GetInt32();

                        Console.WriteLine(
                            $"{id,-5} {pattern,-30} {priority,-10} {rateLimit,-12}"
                        );
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Create a new route.
    /// </summary>
    private async Task CreateRouteAsync()
    {
        Console.Write("Pattern (e.g., user.*): ");
        var pattern = Console.ReadLine();

        Console.Write("Target Service ID: ");
        if (!int.TryParse(Console.ReadLine(), out var serviceId))
        {
            Console.WriteLine("Invalid service ID");
            return;
        }

        Console.Write("Priority (0-200): ");
        if (!int.TryParse(Console.ReadLine(), out var priority))
        {
            Console.WriteLine("Invalid priority");
            return;
        }

        Console.Write("Rate limit per minute (0=unlimited): ");
        if (!int.TryParse(Console.ReadLine(), out var rateLimit))
        {
            Console.WriteLine("Invalid rate limit");
            return;
        }

        try
        {
            var routeData = new
            {
                pattern,
                targetServiceId = serviceId,
                matchType = 1,
                priority,
                rateLimitPerMinute = rateLimit,
                enableCaching = false
            };

            var content = new StringContent(
                JsonSerializer.Serialize(routeData),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync($"{_gatewayUrl}/gateway/routes", content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("✓ Route created successfully");
                await ListRoutesAsync();
            }
            else
            {
                Console.WriteLine($"✗ Error: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Display gateway metrics.
    /// </summary>
    private async Task ViewMetricsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_gatewayUrl}/metrics/performance");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(result);
                var root = doc.RootElement;

                Console.WriteLine("\n=== Gateway Metrics ===");
                Console.WriteLine($"Throughput: {root.GetProperty("throughputRequestsPerSecond").GetDouble():F2} req/s");
                Console.WriteLine($"Average Latency: {root.GetProperty("averageLatencyMs").GetDouble():F2} ms");
                Console.WriteLine($"P50 Latency: {root.GetProperty("p50LatencyMs").GetDouble():F2} ms");
                Console.WriteLine($"P95 Latency: {root.GetProperty("p95LatencyMs").GetDouble():F2} ms");
                Console.WriteLine($"P99 Latency: {root.GetProperty("p99LatencyMs").GetDouble():F2} ms");
                Console.WriteLine($"Total Requests: {root.GetProperty("totalRequests").GetInt64()}");
                Console.WriteLine($"Error Rate: {root.GetProperty("errorRate").GetDouble():P2}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Check gateway health.
    /// </summary>
    private async Task CheckGatewayHealthAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"http://localhost:5000/health");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(result);
                var status = doc.RootElement.GetProperty("status").GetString();

                Console.WriteLine($"\nGateway Status: {status}");
            }
            else
            {
                Console.WriteLine("✗ Gateway is not responding");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Display gateway configuration.
    /// </summary>
    private async Task ViewConfigurationAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_gatewayUrl}/gateway/configuration");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine("\n=== Gateway Configuration ===");
                Console.WriteLine(result);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Main CLI loop.
    /// </summary>
    public async Task RunAsync()
    {
        Console.WriteLine("dotnet-grpc-gateway Management CLI");
        Console.WriteLine("Make sure gateway is running at http://localhost:5000\n");

        while (_running)
        {
            DisplayMenu();
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await ListServicesAsync();
                    break;
                case "2":
                    await RegisterServiceAsync();
                    break;
                case "3":
                    await CheckServiceHealthAsync();
                    break;
                case "4":
                    await ListRoutesAsync();
                    break;
                case "5":
                    await CreateRouteAsync();
                    break;
                case "6":
                    await ViewMetricsAsync();
                    break;
                case "7":
                    await CheckGatewayHealthAsync();
                    break;
                case "8":
                    await ViewConfigurationAsync();
                    break;
                case "9":
                    _running = false;
                    break;
                default:
                    Console.WriteLine("Invalid option");
                    break;
            }
        }

        Console.WriteLine("Goodbye!");
    }

    // Main entry point
    public static async Task Main(string[] args)
    {
        var cli = new GatewayManagementCLI();
        await cli.RunAsync();
    }
}
