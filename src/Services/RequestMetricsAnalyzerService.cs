using System;
using System.Threading.Tasks;

namespace dotnet_grpc_gateway.Services
{
    public class RequestMetricsAnalyzerService
    {
        public async Task<EndpointHealthScore> AnalyzeEndpointHealthAsync(string path)
        {
            // ... other code
            return new EndpointHealthScore { HealthScore = 0.5 };
        }
    }
}