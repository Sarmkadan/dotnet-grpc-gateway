using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Integration example for dotnet-grpc-gateway
// This demonstrates how to wire up gateway clients into an ASP.NET DI container

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

public interface IGatewayConsumer
{
    Task ProcessDataAsync();
}

public class GatewayConsumer : IGatewayConsumer
{
    private readonly HttpClient _httpClient;

    public GatewayConsumer(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("GatewayClient");
    }

    public async Task ProcessDataAsync()
    {
        Console.WriteLine("Consuming data via Gateway from ASP.NET service...");
        // var response = await _httpClient.GetAsync("/user/GetUser?userId=1");
        await Task.CompletedTask;
    }
}
