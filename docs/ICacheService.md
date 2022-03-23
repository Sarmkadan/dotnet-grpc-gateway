# ICacheService

The `ICacheService` interface defines a contract for a cache instance used within the gRPC gateway infrastructure. It exposes read-only metrics and configuration properties that allow consumers to monitor cache performance (hit/miss counts, entry count, approximate memory usage) and inspect the cache’s behavior (name, duration, maximum size, expiration policies). Implementations of this interface are expected to provide a concrete caching mechanism while exposing these standard observability and configuration points.

## API

- **`HitCount`** (`long`)  
  Total number of cache hits since the cache was created or last reset. Does not throw.

- **`MissCount`** (`long`)  
  Total number of cache misses since the cache was created or last reset. Does not throw.

- **`EntryCount`** (`int`)  
  Current number of entries stored in the cache. Does not throw.

- **`ApproximateSizeBytes`** (`long`)  
  An approximation of the total memory consumed by the cached entries, in bytes. The value is not guaranteed to be exact and may be zero if the implementation does not track size. Does not throw.

- **`Name`** (`string`)  
  A human-readable name assigned to this cache instance. The value is never `null`; it may be an empty string if not explicitly configured. Does not throw.

- **`Duration`** (`TimeSpan`)  
  The time-to-live (TTL) for cache entries. A value of `TimeSpan.Zero` indicates that no default duration is set (entries may live indefinitely or rely on other expiration mechanisms). Does not throw.

- **`MaxSize`** (`int`)  
  The maximum number of entries the cache can hold. A value of `0` means the cache has no hard limit on entry count. Does not throw.

- **`AbsoluteExpiration`** (`bool`)  
  When `true`, the cache uses absolute expiration: entries expire at a fixed point in time relative to their creation. When `false`, absolute expiration is not active. Does not throw.

- **`SlidingExpiration`** (`bool`)  
  When `true`, the cache uses sliding expiration: an entry’s lifetime is reset each time it is accessed. When `false`, sliding expiration is not active. Does not throw.

## Usage

The following examples demonstrate how to consume `ICacheService` in a typical application. Assume the interface is injected via dependency injection.

### Example 1: Logging cache statistics

```csharp
public class CacheMonitor
{
    private readonly ICacheService _cache;

    public CacheMonitor(ICacheService cache)
    {
        _cache = cache;
    }

    public void Report()
    {
        double hitRate = _cache.HitCount + _cache.MissCount > 0
            ? (double)_cache.HitCount / (_cache.HitCount + _cache.MissCount) * 100
            : 0;

        Console.WriteLine($"[{_cache.Name}]");
        Console.WriteLine($"  Entries: {_cache.EntryCount}");
        Console.WriteLine($"  Approx. size: {_cache.ApproximateSizeBytes} bytes");
        Console.WriteLine($"  Hits: {_cache.HitCount}, Misses: {_cache.MissCount}");
        Console.WriteLine($"  Hit rate: {hitRate:F2}%");
    }
}
```

### Example 2: Inspecting expiration policy

```csharp
public class CachePolicyInspector
{
    private readonly ICacheService _cache;

    public CachePolicyInspector(ICacheService cache)
    {
        _cache = cache;
    }

    public void DescribePolicy()
    {
        Console.WriteLine($"Cache '{_cache.Name}' policy:");
        Console.WriteLine($"  Duration: {_cache.Duration}");
        Console.WriteLine($"  Max size: {(_cache.MaxSize == 0 ? "unlimited" : _cache.MaxSize.ToString())}");
        Console.WriteLine($"  Absolute expiration: {_cache.AbsoluteExpiration}");
        Console.WriteLine($"  Sliding expiration: {_cache.SlidingExpiration}");

        if (_cache.AbsoluteExpiration && _cache.SlidingExpiration)
        {
            Console.WriteLine("  Warning: Both expiration modes are enabled; behavior is implementation-defined.");
        }
    }
}
```

## Notes

- **Edge cases**  
  - `HitCount` and `MissCount` are `long` values and may theoretically overflow after an extremely large number of operations, though this is unlikely in practice.  
  - `EntryCount` can be zero when the cache is empty.  
  - `ApproximateSizeBytes` may be zero if the implementation does not track memory usage or if no entries are present.  
  - `Duration` of `TimeSpan.Zero` indicates no default TTL; entries may live indefinitely unless evicted by size limits or explicit removal.  
  - `MaxSize` of `0` means no limit on the number of entries; the cache may grow unboundedly.  
  - `AbsoluteExpiration` and `SlidingExpiration` are not mutually exclusive by contract; when both are `true`, the behavior depends on the implementation (typically absolute expiration takes precedence, or both are applied).

- **Thread safety**  
  The `ICacheService` interface does not mandate thread safety, but all well-behaved implementations should be safe for concurrent reads of these properties. Values such as `HitCount`, `MissCount`, `EntryCount`, and `ApproximateSizeBytes` are snapshots and may change between reads. No property throws exceptions under normal operation.
