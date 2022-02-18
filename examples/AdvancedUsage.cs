using System.Net.Http.Json;
using System.Text.Json;

// Advanced usage example for dotnet-grpc-gateway
// This demonstrates custom configuration, routing, and error handling

var gatewayUrl = "http://localhost:5000";

Console.WriteLine("--- Advanced Usage Example ---");

// 1. Register a service with custom options
var advancedServicePayload = new
{
    name = "OrderService",
    serviceFullName = "order.OrderService",
    host = "orders-backend",
    port = 8080,
    useTls = true,
    healthCheckIntervalSeconds = 10 // More frequent checks
};

using var httpClient = new HttpClient();
var response = await httpClient.PostAsJsonAsync($"{gatewayUrl}/api/gateway/services", advancedServicePayload);
if (!response.IsSuccessStatusCode)
{
    var error = await response.Content.ReadAsStringAsync();
    Console.WriteLine($"Failed to register service: {error}");
    return;
}

// 2. Add complex routing rule with caching and rate limiting
var advancedRoutePayload = new
{
    pattern = "order.OrderService/GetOrderDetails",
    targetServiceId = 2,
    matchType = 0, // Exact match
    priority = 200,
    rateLimitPerMinute = 500,
    enableCaching = true,
    cacheDurationSeconds = 60 // 1 minute cache
};

response = await httpClient.PostAsJsonAsync($"{gatewayUrl}/api/gateway/routes", advancedRoutePayload);
if (response.IsSuccessStatusCode)
{
    Console.WriteLine("Advanced route added with caching and rate limits.");
}
else
{
    Console.WriteLine("Failed to add advanced route.");
}
