// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DotNetGrpcGateway.Examples;

/// <summary>
/// Example 1: Basic Service Registration
///
/// This example demonstrates how to register a gRPC service with the gateway
/// using the REST API. It covers the complete lifecycle from registration to
/// health checking.
/// </summary>
public class BasicServiceRegistrationExample
{
    private readonly string _gatewayUrl = "http://localhost:5000/api";
    private readonly HttpClient _httpClient;

    public BasicServiceRegistrationExample()
    {
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Register a new gRPC service with the gateway.
    /// </summary>
    public async Task RegisterServiceAsync()
    {
        var serviceData = new
        {
            name = "UserService",
            serviceFullName = "user.UserService",
            host = "api.example.com",
            port = 5001,
            useTls = true,
            healthCheckIntervalSeconds = 30
        };

        var content = new StringContent(
            JsonSerializer.Serialize(serviceData),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync(
            $"{_gatewayUrl}/gateway/services",
            content
        );

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Service registered successfully: {result}");
        }
        else
        {
            Console.WriteLine($"Error registering service: {response.StatusCode}");
        }
    }

    /// <summary>
    /// Verify the service was registered by retrieving all services.
    /// </summary>
    public async Task VerifyServiceRegistrationAsync()
    {
        var response = await _httpClient.GetAsync($"{_gatewayUrl}/gateway/services");

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Registered services: {result}");
        }
    }

    /// <summary>
    /// Check health status of a registered service.
    /// </summary>
    public async Task CheckServiceHealthAsync(int serviceId)
    {
        var response = await _httpClient.GetAsync(
            $"{_gatewayUrl}/health/services/{serviceId}"
        );

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(result);
            var isHealthy = jsonDoc.RootElement.GetProperty("isHealthy").GetBoolean();

            Console.WriteLine($"Service {serviceId} health: {(isHealthy ? "Healthy" : "Unhealthy")}");
        }
    }

    /// <summary>
    /// Unregister a service from the gateway.
    /// </summary>
    public async Task UnregisterServiceAsync(int serviceId)
    {
        var response = await _httpClient.DeleteAsync(
            $"{_gatewayUrl}/gateway/services/{serviceId}"
        );

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Service {serviceId} unregistered successfully");
        }
        else
        {
            Console.WriteLine($"Error unregistering service: {response.StatusCode}");
        }
    }

    // Main entry point for running this example
    public static async Task Main(string[] args)
    {
        var example = new BasicServiceRegistrationExample();

        Console.WriteLine("=== Basic Service Registration Example ===\n");

        // Step 1: Register a service
        Console.WriteLine("Step 1: Registering UserService...");
        await example.RegisterServiceAsync();
        await Task.Delay(1000);

        // Step 2: Verify registration
        Console.WriteLine("\nStep 2: Verifying service registration...");
        await example.VerifyServiceRegistrationAsync();
        await Task.Delay(1000);

        // Step 3: Check health
        Console.WriteLine("\nStep 3: Checking service health (service ID: 1)...");
        await example.CheckServiceHealthAsync(1);
        await Task.Delay(1000);

        // Step 4: Unregister (optional)
        // await example.UnregisterServiceAsync(1);

        Console.WriteLine("\n=== Example complete ===");
    }
}
