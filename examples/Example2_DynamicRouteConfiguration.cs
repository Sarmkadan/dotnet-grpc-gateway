// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DotNetGrpcGateway.Examples;

/// <summary>
/// Example 2: Dynamic Route Configuration
///
/// This example shows how to configure routes dynamically, including:
/// - Pattern-based routing (exact, prefix, regex)
/// - Priority-based matching
/// - Rate limiting
/// - Response caching
/// </summary>
public class DynamicRouteConfigurationExample
{
    private readonly string _gatewayUrl = "http://localhost:5000/api";
    private readonly HttpClient _httpClient;

    public DynamicRouteConfigurationExample()
    {
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Create routes for different patterns with varying priorities and strategies.
    /// </summary>
    public async Task ConfigureRoutesAsync(int serviceId)
    {
        var routes = new List<object>
        {
            // High-priority admin routes (no caching, tight rate limit)
            new
            {
                pattern = "admin.*",
                targetServiceId = serviceId,
                matchType = 1,  // Prefix match
                priority = 200,
                rateLimitPerMinute = 100,
                enableCaching = false,
                description = "Admin operations"
            },

            // Medium-priority write operations (no caching)
            new
            {
                pattern = "user.UserService.Create*",
                targetServiceId = serviceId,
                matchType = 1,
                priority = 100,
                rateLimitPerMinute = 500,
                enableCaching = false,
                description = "Create operations"
            },

            // Low-priority read operations (with caching)
            new
            {
                pattern = "user.UserService.Get*",
                targetServiceId = serviceId,
                matchType = 1,
                priority = 50,
                rateLimitPerMinute = 1000,
                enableCaching = true,
                cacheDurationSeconds = 300,
                description = "Get/List operations"
            },

            // Product service routes
            new
            {
                pattern = "product.*",
                targetServiceId = serviceId,
                matchType = 1,
                priority = 75,
                rateLimitPerMinute = 2000,
                enableCaching = true,
                cacheDurationSeconds = 600,
                description = "Product service routes"
            }
        };

        foreach (var route in routes)
        {
            await CreateRouteAsync(route);
        }
    }

    /// <summary>
    /// Create a single route via API.
    /// </summary>
    private async Task CreateRouteAsync(object route)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(route),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync(
            $"{_gatewayUrl}/gateway/routes",
            content
        );

        if (response.IsSuccessStatusCode)
        {
            var routeObj = (dynamic)route;
            Console.WriteLine($"✓ Route created: {routeObj.pattern} (priority: {routeObj.priority})");
        }
        else
        {
            Console.WriteLine($"✗ Error creating route: {response.StatusCode}");
        }
    }

    /// <summary>
    /// List all configured routes.
    /// </summary>
    public async Task DisplayAllRoutesAsync()
    {
        var response = await _httpClient.GetAsync($"{_gatewayUrl}/gateway/routes");

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(result);

            Console.WriteLine("\n=== Configured Routes ===");
            Console.WriteLine($"{"ID",-5} {"Pattern",-30} {"Priority",-10} {"Rate Limit",-12} {"Caching",-10}");
            Console.WriteLine(new string('-', 70));

            foreach (var element in doc.RootElement.EnumerateArray())
            {
                var id = element.GetProperty("id").GetInt32();
                var pattern = element.GetProperty("pattern").GetString();
                var priority = element.GetProperty("priority").GetInt32();
                var rateLimit = element.GetProperty("rateLimitPerMinute").GetInt32();
                var caching = element.GetProperty("enableCaching").GetBoolean();

                Console.WriteLine(
                    $"{id,-5} {pattern,-30} {priority,-10} {rateLimit,-12} {(caching ? "Yes" : "No"),-10}"
                );
            }
        }
    }

    /// <summary>
    /// Test route matching for a specific request.
    /// </summary>
    public async Task TestRouteMatchingAsync(string serviceName, string methodName)
    {
        var testData = new { serviceName, methodName };

        var content = new StringContent(
            JsonSerializer.Serialize(testData),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync(
            $"{_gatewayUrl}/servicediscovery/route-match",
            content
        );

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"\nRoute matched for {serviceName}/{methodName}:");
            Console.WriteLine(result);
        }
        else
        {
            Console.WriteLine($"No route matched for {serviceName}/{methodName}");
        }
    }

    /// <summary>
    /// Detect potential route conflicts.
    /// </summary>
    public async Task DetectConflictsAsync()
    {
        var conflictTest = new[]
        {
            new { pattern = "user.*", priority = 100 },
            new { pattern = "user.UserService.*", priority = 50 },
            new { pattern = "user.UserService.Get*", priority = 25 }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(conflictTest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync(
            $"{_gatewayUrl}/servicediscovery/route-conflicts",
            content
        );

        var result = await response.Content.ReadAsStringAsync();
        Console.WriteLine("\n=== Conflict Analysis ===");
        Console.WriteLine(result);
    }

    // Main entry point
    public static async Task Main(string[] args)
    {
        var example = new DynamicRouteConfigurationExample();

        Console.WriteLine("=== Dynamic Route Configuration Example ===\n");

        // Step 1: Configure routes
        Console.WriteLine("Step 1: Creating routes...");
        await example.ConfigureRoutesAsync(1);
        await Task.Delay(1000);

        // Step 2: Display all routes
        Console.WriteLine("\nStep 2: Displaying all routes...");
        await example.DisplayAllRoutesAsync();
        await Task.Delay(1000);

        // Step 3: Test route matching
        Console.WriteLine("\nStep 3: Testing route matching...");
        await example.TestRouteMatchingAsync("user.UserService", "GetUser");
        await Task.Delay(1000);

        // Step 4: Detect conflicts
        Console.WriteLine("\nStep 4: Detecting potential conflicts...");
        await example.DetectConflictsAsync();

        Console.WriteLine("\n=== Example complete ===");
    }
}
