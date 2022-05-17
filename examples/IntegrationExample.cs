using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

/// <summary>
/// Integration example for dotnet-grpc-gateway.
/// This demonstrates how to wire up gateway clients into an ASP.NET DI container.
/// </summary>
public class IntegrationExample
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Assuming the gateway provides a client factory or registration extension
        // builder.Services.AddGrpcGatewayClient(options => {
        //     options.GatewayAddress = "http://localhost:5000";
        // });

        // Example of using a service that relies on the gateway
        builder.Services.AddSingleton<IGatewayConsumer, GatewayConsumer>();

        using var host = builder.Build();
        var consumer = host.Services.GetRequiredService<IGatewayConsumer>();

        Console.WriteLine("--- Integration Example ---");
        await consumer.ProcessDataAsync();
    }
}

/// <summary>
/// Interface for a gateway consumer.
/// </summary>
public interface IGatewayConsumer
{
    /// <summary>
    /// Processes data asynchronously via the gateway.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ProcessDataAsync();
}

/// <summary>
/// A gateway consumer that uses an HTTP client to process data.
/// </summary>
public class GatewayConsumer : IGatewayConsumer
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GatewayConsumer"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    public GatewayConsumer(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("GatewayClient");
    }

    /// <summary>
    /// Processes data asynchronously via the gateway.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ProcessDataAsync()
    {
        Console.WriteLine("Consuming data via Gateway from ASP.NET service...");
        // var response = await _httpClient.GetAsync("/user/GetUser?userId=1");
        await Task.CompletedTask;
    }
}
