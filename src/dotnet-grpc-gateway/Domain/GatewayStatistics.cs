// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Domain;

/// <summary>
/// Aggregated statistics for the gateway and its services
/// </summary>
public class GatewayStatistics
{
    public int Id { get; set; }

    public DateTime StatisticsDate { get; set; } = DateTime.UtcNow.Date;

    public long TotalRequestsProcessed { get; set; } = 0;

    public long SuccessfulRequests { get; set; } = 0;

    public long FailedRequests { get; set; } = 0;

    public double SuccessRate { get; set; } = 0.0;

    public double AverageResponseTimeMs { get; set; } = 0.0;

    public double MinResponseTimeMs { get; set; } = 0.0;

    public double MaxResponseTimeMs { get; set; } = 0.0;

    public long TotalDataProcessedBytes { get; set; } = 0;

    public int ActiveConnections { get; set; } = 0;

    public int PeakConnections { get; set; } = 0;

    public Dictionary<string, long> RequestsByService { get; set; } = new();

    public Dictionary<string, long> RequestsByMethod { get; set; } = new();

    public Dictionary<string, int> ErrorsByType { get; set; } = new();

    public int HealthyServices { get; set; } = 0;

    public int UnhealthyServices { get; set; } = 0;

    public int TotalServices { get; set; } = 0;

    public double CacheHitRate { get; set; } = 0.0;

    public int CacheHits { get; set; } = 0;

    public int CacheMisses { get; set; } = 0;

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public void Validate()
    {
        if (TotalRequestsProcessed < 0)
            throw new InvalidOperationException("Total requests cannot be negative");

        if (SuccessfulRequests < 0 || FailedRequests < 0)
            throw new InvalidOperationException("Request counts cannot be negative");

        if (SuccessRate < 0 || SuccessRate > 100)
            throw new InvalidOperationException("Success rate must be between 0 and 100");

        if (AverageResponseTimeMs < 0)
            throw new InvalidOperationException("Average response time cannot be negative");

        if (MaxResponseTimeMs < MinResponseTimeMs)
            throw new InvalidOperationException("Max response time cannot be less than min");
    }

    public void RecordRequest(bool success, double responseTimeMs, long dataBytes)
    {
        TotalRequestsProcessed++;
        TotalDataProcessedBytes += dataBytes;

        if (success)
            SuccessfulRequests++;
        else
            FailedRequests++;

        SuccessRate = TotalRequestsProcessed > 0
            ? (double)SuccessfulRequests / TotalRequestsProcessed * 100
            : 0;

        // Update average, min, max response times
        if (responseTimeMs > 0)
        {
            AverageResponseTimeMs = (AverageResponseTimeMs * (TotalRequestsProcessed - 1) + responseTimeMs) / TotalRequestsProcessed;
            MinResponseTimeMs = MinResponseTimeMs == 0 ? responseTimeMs : Math.Min(MinResponseTimeMs, responseTimeMs);
            MaxResponseTimeMs = Math.Max(MaxResponseTimeMs, responseTimeMs);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordServiceRequest(string serviceName)
    {
        if (!RequestsByService.ContainsKey(serviceName))
            RequestsByService[serviceName] = 0;

        RequestsByService[serviceName]++;
    }

    public void RecordMethodCall(string methodName)
    {
        if (!RequestsByMethod.ContainsKey(methodName))
            RequestsByMethod[methodName] = 0;

        RequestsByMethod[methodName]++;
    }

    public void RecordError(string errorType)
    {
        if (!ErrorsByType.ContainsKey(errorType))
            ErrorsByType[errorType] = 0;

        ErrorsByType[errorType]++;
    }

    public void RecordCacheHit(bool isHit)
    {
        if (isHit)
            CacheHits++;
        else
            CacheMisses++;

        int totalCacheRequests = CacheHits + CacheMisses;
        CacheHitRate = totalCacheRequests > 0 ? (double)CacheHits / totalCacheRequests * 100 : 0;
    }

    public void UpdateServiceHealth(int healthyCount, int unhealthyCount)
    {
        HealthyServices = healthyCount;
        UnhealthyServices = unhealthyCount;
        TotalServices = healthyCount + unhealthyCount;
    }
}
