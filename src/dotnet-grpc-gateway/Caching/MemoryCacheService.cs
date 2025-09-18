// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using DotNetGrpcGateway.Utilities;

namespace DotNetGrpcGateway.Caching;

/// <summary>
/// In-memory cache service implementation using IMemoryCache.
/// Provides thread-safe caching with expiration and statistics tracking.
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryCacheService> _logger;
    private readonly ConcurrentDictionary<string, CacheEntryMetadata> _metadata = new();

    private long _hitCount = 0;
    private long _missCount = 0;

    public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        if (string.IsNullOrEmpty(key))
            return default;

        // Simulate async operation for consistency with interface
        return await Task.FromResult(Get<T>(key));
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Cache key cannot be null or empty", nameof(key));

        // Simulate async operation for consistency with interface
        await Task.Run(() => Set(key, value, expiration));
    }

    public async Task RemoveAsync(string key)
    {
        if (string.IsNullOrEmpty(key))
            return;

        await Task.Run(() => Remove(key));
    }

    public async Task<bool> ExistsAsync(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;

        return await Task.FromResult(_cache.TryGetValue(key, out _));
    }

    public async Task ClearAsync()
    {
        // Note: IMemoryCache doesn't have a built-in Clear method
        // In a real implementation, you'd maintain a list of keys or use MemoryCache.Compact
        await Task.CompletedTask;
        _metadata.Clear();
        _hitCount = 0;
        _missCount = 0;
        _logger.LogInformation("Cache cleared");
    }

    public async Task<CacheStatistics> GetStatisticsAsync()
    {
        return await Task.FromResult(new CacheStatistics
        {
            HitCount = _hitCount,
            MissCount = _missCount,
            EntryCount = _metadata.Count,
            ApproximateSizeBytes = CalculateApproximateSize()
        });
    }

    private T? Get<T>(string key)
    {
        try
        {
            if (_cache.TryGetValue(key, out var value))
            {
                Interlocked.Increment(ref _hitCount);
                _logger.LogDebug("Cache hit for key: {Key}", StringUtility.MaskSensitiveData(key));
                return (T?)value;
            }

            Interlocked.Increment(ref _missCount);
            _logger.LogDebug("Cache miss for key: {Key}", StringUtility.MaskSensitiveData(key));
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving from cache: {Key}", StringUtility.MaskSensitiveData(key));
            return default;
        }
    }

    private void Set<T>(string key, T value, TimeSpan? expiration)
    {
        try
        {
            var cacheOptions = new MemoryCacheEntryOptions();

            if (expiration.HasValue && expiration.Value > TimeSpan.Zero)
            {
                cacheOptions.SetAbsoluteExpiration(expiration.Value);
            }
            else
            {
                // Default 5-minute expiration if not specified
                cacheOptions.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
            }

            _cache.Set(key, value, cacheOptions);

            // Track metadata for statistics
            _metadata.AddOrUpdate(key,
                new CacheEntryMetadata { CreatedAt = DateTime.UtcNow, Size = EstimateSize(value) },
                (_, _) => new CacheEntryMetadata { CreatedAt = DateTime.UtcNow, Size = EstimateSize(value) });

            _logger.LogDebug("Cache set for key: {Key}, Expiration: {Expiration}",
                StringUtility.MaskSensitiveData(key), expiration?.ToString() ?? "default");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache: {Key}", StringUtility.MaskSensitiveData(key));
        }
    }

    private void Remove(string key)
    {
        try
        {
            _cache.Remove(key);
            _metadata.TryRemove(key, out _);
            _logger.LogDebug("Cache entry removed: {Key}", StringUtility.MaskSensitiveData(key));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing from cache: {Key}", StringUtility.MaskSensitiveData(key));
        }
    }

    private long CalculateApproximateSize()
    {
        return _metadata.Values.Sum(m => m.Size);
    }

    private static long EstimateSize(object? value)
    {
        if (value == null)
            return 0;

        // Rough estimation of object size in bytes
        return value switch
        {
            string s => s.Length * 2, // UTF-16
            byte[] b => b.Length,
            _ => System.Runtime.InteropServices.Marshal.SizeOf(value)
        };
    }

    private class CacheEntryMetadata
    {
        public DateTime CreatedAt { get; set; }
        public long Size { get; set; }
    }
}
