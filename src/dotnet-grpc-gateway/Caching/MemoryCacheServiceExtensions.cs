#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading.Tasks;

namespace DotNetGrpcGateway.Caching;

/// <summary>
/// Extension methods for <see cref="MemoryCacheService"/> providing additional cache functionality.
/// </summary>
public static class MemoryCacheServiceExtensions
{
    /// <summary>
    /// Attempts to get the value associated with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the value to get.</typeparam>
    /// <param name="cacheService">The cache service.</param>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>A tuple containing a boolean indicating if the key exists and the value (or default if not found).</returns>
    public static async Task<(bool Exists, T? Value)> TryGetValueAsync<T>(this MemoryCacheService cacheService, string key)
    {
        if (cacheService is null)
        {
            throw new ArgumentNullException(nameof(cacheService));
        }

        if (string.IsNullOrEmpty(key))
        {
            return (false, default);
        }

        var result = await cacheService.GetAsync<T>(key);
        return (result is not null, result);
    }

    /// <summary>
    /// Gets the value associated with the specified key or creates it using the provided factory function.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="cacheService">The cache service.</param>
    /// <param name="key">The key whose value to get or create.</param>
    /// <param name="valueFactory">The function used to create the value if it doesn't exist.</param>
    /// <param name="expiration">Optional expiration time for the cache entry.</param>
    /// <returns>The value from cache or newly created value.</returns>
    public static async Task<T> GetOrCreateAsync<T>(
        this MemoryCacheService cacheService,
        string key,
        Func<Task<T>> valueFactory,
        TimeSpan? expiration = null)
    {
        if (cacheService is null)
        {
            throw new ArgumentNullException(nameof(cacheService));
        }

        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Cache key cannot be null or empty", nameof(key));
        }

        if (valueFactory is null)
        {
            throw new ArgumentNullException(nameof(valueFactory));
        }

        // Try to get from cache first
        var cachedValue = await cacheService.GetAsync<T>(key);
        if (cachedValue is not null)
        {
            return cachedValue;
        }

        // Create new value
        var value = await valueFactory();
        await cacheService.SetAsync(key, value, expiration);
        return value;
    }

    /// <summary>
    /// Gets multiple values from cache in a single operation.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="cacheService">The cache service.</param>
    /// <param name="keys">The collection of keys to retrieve.</param>
    /// <returns>A dictionary containing the found key-value pairs.</returns>
    public static async Task<Dictionary<string, T>> GetManyAsync<T>(
        this MemoryCacheService cacheService,
        IEnumerable<string> keys)
    {
        if (cacheService is null)
        {
            throw new ArgumentNullException(nameof(cacheService));
        }

        if (keys is null)
        {
            throw new ArgumentNullException(nameof(keys));
        }

        var result = new Dictionary<string, T>();
        foreach (var key in keys)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var value = await cacheService.GetAsync<T>(key);
                if (value is not null)
                {
                    result[key] = value;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Sets multiple values in cache in a single operation.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="cacheService">The cache service.</param>
    /// <param name="items">The key-value pairs to set in cache.</param>
    /// <param name="expiration">Optional expiration time for all cache entries.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task SetManyAsync<T>(
        this MemoryCacheService cacheService,
        IEnumerable<KeyValuePair<string, T>> items,
        TimeSpan? expiration = null)
    {
        if (cacheService is null)
        {
            throw new ArgumentNullException(nameof(cacheService));
        }

        if (items is null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        foreach (var item in items)
        {
            if (!string.IsNullOrEmpty(item.Key))
            {
                await cacheService.SetAsync(item.Key, item.Value, expiration);
            }
        }
    }

    /// <summary>
    /// Gets cache statistics including hit/miss ratio.
    /// </summary>
    /// <param name="cacheService">The cache service.</param>
    /// <returns>A tuple containing cache statistics and hit/miss ratio.</returns>
    public static async Task<(CacheStatistics Statistics, double HitRatio)> GetStatisticsWithRatioAsync(
        this MemoryCacheService cacheService)
    {
        if (cacheService is null)
        {
            throw new ArgumentNullException(nameof(cacheService));
        }

        var stats = await cacheService.GetStatisticsAsync();
        var hitRatio = stats.HitCount + stats.MissCount > 0
            ? (double)stats.HitCount / (stats.HitCount + stats.MissCount)
            : 0;
        return (stats, hitRatio);
    }

    /// <summary>
    /// Removes multiple cache entries in a single operation.
    /// </summary>
    /// <param name="cacheService">The cache service.</param>
    /// <param name="keys">The keys to remove from cache.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task RemoveManyAsync(
        this MemoryCacheService cacheService,
        IEnumerable<string> keys)
    {
        if (cacheService is null)
        {
            throw new ArgumentNullException(nameof(cacheService));
        }

        if (keys is null)
        {
            throw new ArgumentNullException(nameof(keys));
        }

        foreach (var key in keys)
        {
            if (!string.IsNullOrEmpty(key))
            {
                await cacheService.RemoveAsync(key);
            }
        }
    }

    /// <summary>
    /// Checks if any of the specified keys exist in cache.
    /// </summary>
    /// <param name="cacheService">The cache service.</param>
    /// <param name="keys">The keys to check.</param>
    /// <returns>A dictionary mapping keys to their existence status.</returns>
    public static async Task<Dictionary<string, bool>> ExistsManyAsync(
        this MemoryCacheService cacheService,
        IEnumerable<string> keys)
    {
        if (cacheService is null)
        {
            throw new ArgumentNullException(nameof(cacheService));
        }

        if (keys is null)
        {
            throw new ArgumentNullException(nameof(keys));
        }

        var result = new Dictionary<string, bool>();
        foreach (var key in keys)
        {
            result[key] = await cacheService.ExistsAsync(key);
        }

        return result;
    }
}