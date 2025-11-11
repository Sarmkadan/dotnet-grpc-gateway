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
/// Example 4: Authentication & Security
///
/// This example demonstrates:
/// - Creating authentication tokens
/// - Protecting routes with authentication
/// - Making authenticated requests
/// - Managing token expiration
/// </summary>
public class AuthenticationAndSecurityExample
{
    private readonly string _gatewayUrl = "http://localhost:5000/api";
    private readonly HttpClient _httpClient;

    public AuthenticationAndSecurityExample()
    {
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Create a new API authentication token.
    /// </summary>
    public async Task<string> CreateAuthenticationTokenAsync(string tokenValue, string description)
    {
        var tokenData = new
        {
            token = tokenValue,
            description = description,
            expiresAt = DateTime.UtcNow.AddYears(1)
        };

        var content = new StringContent(
            JsonSerializer.Serialize(tokenData),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync(
            $"{_gatewayUrl}/gateway/auth/tokens",
            content
        );

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"✓ Token created: {description}");
            return tokenValue;
        }
        else
        {
            Console.WriteLine($"✗ Error creating token: {response.StatusCode}");
            return null;
        }
    }

    /// <summary>
    /// Create a protected route that requires authentication.
    /// </summary>
    public async Task CreateProtectedRouteAsync(int serviceId, string pattern, string description)
    {
        var routeData = new
        {
            pattern = pattern,
            targetServiceId = serviceId,
            matchType = 1,
            priority = 150,
            requiresAuthentication = true,
            rateLimitPerMinute = 100,
            enableCaching = false
        };

        var content = new StringContent(
            JsonSerializer.Serialize(routeData),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync(
            $"{_gatewayUrl}/gateway/routes",
            content
        );

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"✓ Protected route created: {pattern} ({description})");
        }
        else
        {
            Console.WriteLine($"✗ Error creating protected route: {response.StatusCode}");
        }
    }

    /// <summary>
    /// Make an authenticated request to a protected endpoint.
    /// </summary>
    public async Task MakeAuthenticatedRequestAsync(string token, string endpoint)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Add("Authorization", $"Bearer {token}");

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"✓ Authenticated request succeeded");
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            Console.WriteLine($"✗ Unauthorized: Invalid or missing token");
        }
        else
        {
            Console.WriteLine($"✗ Request failed: {response.StatusCode}");
        }
    }

    /// <summary>
    /// Try to access a protected endpoint without authentication (should fail).
    /// </summary>
    public async Task TryUnauthorizedAccessAsync(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            Console.WriteLine($"✓ Protected endpoint correctly blocked unauthorized access");
        }
        else if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"✗ Endpoint is NOT protected (should require auth)");
        }
        else
        {
            Console.WriteLine($"✗ Unexpected response: {response.StatusCode}");
        }
    }

    /// <summary>
    /// Create rate-limited routes for different user tiers.
    /// </summary>
    public async Task ConfigureTieredAccessAsync(int serviceId)
    {
        var tiers = new[]
        {
            new { name = "Free", pattern = "free.*", limit = 100 },
            new { name = "Pro", pattern = "pro.*", limit = 1000 },
            new { name = "Enterprise", pattern = "enterprise.*", limit = 10000 }
        };

        Console.WriteLine("\n=== Configuring Tiered Access ===");

        foreach (var tier in tiers)
        {
            var routeData = new
            {
                pattern = tier.pattern,
                targetServiceId = serviceId,
                matchType = 1,
                priority = 100,
                rateLimitPerMinute = tier.limit,
                requiresAuthentication = true
            };

            var content = new StringContent(
                JsonSerializer.Serialize(routeData),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(
                $"{_gatewayUrl}/gateway/routes",
                content
            );

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"✓ {tier.name} tier configured: {tier.limit} req/min");
            }
        }
    }

    /// <summary>
    /// List all authentication tokens (metadata only).
    /// </summary>
    public async Task DisplayTokensAsync()
    {
        var response = await _httpClient.GetAsync($"{_gatewayUrl}/gateway/auth/tokens");

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine("\n=== Registered Tokens ===");
            Console.WriteLine(result);
        }
    }

    // Main entry point
    public static async Task Main(string[] args)
    {
        var example = new AuthenticationAndSecurityExample();

        Console.WriteLine("=== Authentication & Security Example ===\n");

        try
        {
            // Step 1: Create authentication tokens
            Console.WriteLine("Step 1: Creating authentication tokens...");
            var clientToken = await example.CreateAuthenticationTokenAsync(
                "client-token-12345",
                "Client Application"
            );
            var adminToken = await example.CreateAuthenticationTokenAsync(
                "admin-token-67890",
                "Admin User"
            );
            await Task.Delay(1000);

            // Step 2: Create protected routes
            Console.WriteLine("\nStep 2: Creating protected routes...");
            await example.CreateProtectedRouteAsync(1, "admin.*", "Admin operations");
            await example.CreateProtectedRouteAsync(1, "api.secure.*", "Secure API endpoints");
            await Task.Delay(1000);

            // Step 3: Configure tiered access
            Console.WriteLine("\nStep 3: Configuring tiered access...");
            await example.ConfigureTieredAccessAsync(1);
            await Task.Delay(1000);

            // Step 4: Display tokens
            Console.WriteLine("\nStep 4: Displaying tokens...");
            await example.DisplayTokensAsync();
            await Task.Delay(1000);

            // Step 5: Test unauthorized access
            Console.WriteLine("\nStep 5: Testing security...");
            await example.TryUnauthorizedAccessAsync("http://localhost:5000/admin/endpoint");
            await Task.Delay(1000);

            // Step 6: Test authenticated access
            if (clientToken != null)
            {
                Console.WriteLine("\nStep 6: Making authenticated request...");
                await example.MakeAuthenticatedRequestAsync(
                    clientToken,
                    "http://localhost:5000/api/secure/data"
                );
            }

            Console.WriteLine("\n=== Example complete ===");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Make sure the gateway is running at http://localhost:5000");
        }
    }
}
