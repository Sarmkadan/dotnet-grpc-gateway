using System.Net.Http.Json;
using System.Text.Json;

// Basic usage example for dotnet-grpc-gateway
// This demonstrates how to register a service and send a request

var gatewayUrl = "http://localhost:5000";

Console.WriteLine("--- Basic Usage Example ---");

// 1. Register a service
var servicePayload = new
{
    name = "ProductService",
    serviceFullName = "product.ProductService",
    host = "localhost",
    port = 5002,
    useTls = false,
    healthCheckIntervalSeconds = 30
};

using var httpClient = new HttpClient();
var response = await httpClient.PostAsJsonAsync($"{gatewayUrl}/api/gateway/services", servicePayload);
response.EnsureSuccessStatusCode();
Console.WriteLine("Service 'ProductService' registered successfully.");

// 2. Add a routing rule
var routePayload = new
{
    pattern = "product.ProductService.*",
    targetServiceId = 1, // Assumes ID 1
    matchType = 1,
    priority = 100
};

response = await httpClient.PostAsJsonAsync($"{gatewayUrl}/api/gateway/routes", routePayload);
response.EnsureSuccessStatusCode();
Console.WriteLine("Route for 'ProductService' added successfully.");

// 3. Make a request (simplified representation)
Console.WriteLine("Ready to forward requests to ProductService.");
