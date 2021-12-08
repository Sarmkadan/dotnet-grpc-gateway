// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading.Tasks;
using DotNetGrpcGateway;

namespace DotNetGrpcGateway.Examples.V2BasicUsage;

/// <summary>
/// Example: Basic Usage of dotnet-grpc-gateway v2.0
///
/// This example demonstrates the minimal setup required to use the gateway in v2.0.
/// It covers:
/// - Basic configuration
/// - Service registration
/// - Simple request routing
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== dotnet-grpc-gateway v2.0 Basic Usage ===\n");

        // Create a new gateway instance with default configuration
        var gateway = new GatewayBuilder()
            .WithDefaultConfiguration()
            .Build();

        Console.WriteLine("Gateway created successfully!");
        Console.WriteLine("Configuration:");
        Console.WriteLine($"  Port: {gateway.Configuration.Port}");
        Console.WriteLine($"  Host: {gateway.Configuration.Host}");
        Console.WriteLine($"  Max Request Size: {gateway.Configuration.MaxRequestSizeMB} MB");
        Console.WriteLine($"  Enable CORS: {gateway.Configuration.EnableCors}");
        Console.WriteLine($"  Enable Compression: {gateway.Configuration.EnableCompression}");

        // Start the gateway
        Console.WriteLine("\nStarting gateway...");
        await gateway.StartAsync();

        Console.WriteLine("Gateway started successfully!");
        Console.WriteLine("Press Ctrl+C to stop the gateway...");

        // Keep the application running
        await Task.Delay(-1);
    }
}
