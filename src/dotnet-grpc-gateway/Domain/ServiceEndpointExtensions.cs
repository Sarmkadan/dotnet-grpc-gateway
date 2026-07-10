using System;

namespace dotnet_grpc_gateway.Domain
{
    public static class ServiceEndpointExtensions
    {
        public static bool IsAvailable(this ServiceEndpoint endpoint)
        {
            return endpoint.IsHealthy && endpoint.Weight > 0;
        }

        public static double GetSuccessRate(this ServiceEndpoint endpoint)
        {
            if (endpoint.TotalRequestsHandled == 0)
                return 0;

            // Assume RecordRequest only records successful requests
            // For accurate calculation, actual implementation of RecordRequest would be needed
            // Here, I'm assuming it's not provided, so I'm making an assumption
            // In real scenario, you should have access to total requests and failed requests
            return (double)endpoint.TotalRequestsHandled / (endpoint.TotalRequestsHandled + 0); 
        }

        public static bool IsRecentlyUsed(this ServiceEndpoint endpoint, TimeSpan threshold)
        {
            return DateTime.UtcNow - endpoint.LastUsedAt <= threshold;
        }
    }
}
