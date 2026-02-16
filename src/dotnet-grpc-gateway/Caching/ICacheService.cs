// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Caching;

/// <summary>
/// Interface for cache service abstraction. Allows swapping between different caching implementations.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a value from cache by key.
    /// </summary>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// Sets a value in cache with optional expiration.
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

    /// <summary>
    /// Removes a value from cache.
    /// </summary>
    Task RemoveAsync(string key);

    /// <summary>
    /// Checks if a key exists in cache.
    /// </summary>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// Clears all cache entries.
    /// </summary>
    Task ClearAsync();

    /// <summary>
    /// Gets cache statistics (hit rate, size, etc.).
    /// </summary>
    Task<CacheStatistics> GetStatisticsAsync();
}

/// <summary>
/// Cache statistics information.
/// </summary>
public class CacheStatistics
{
    public long HitCount { get; set; }
    public long MissCount { get; set; }
    public int EntryCount { get; set; }
    public long ApproximateSizeBytes { get; set; }

    public double HitRate => HitCount + MissCount > 0
        ? (double)HitCount / (HitCount + MissCount)
        : 0;
}

/// <summary>
/// Cache policy configuration.
/// </summary>
public class CachePolicy
{
    public string Name { get; set; } = null!;
    public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(5);
    public int MaxSize { get; set; } = 1000;
    public bool AbsoluteExpiration { get; set; } = false;
    public bool SlidingExpiration { get; set; } = true;
}
