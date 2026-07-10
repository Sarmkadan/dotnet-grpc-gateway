# MemoryCacheService

`MemoryCacheService` provides an in-memory, asynchronous cache store for objects of any type. It offers basic CRUD operations, existence checks, bulk clearing, and statistics retrieval. The service is intended for lightweight caching scenarios where data volatility and memory pressure are acceptable.

## API

### `MemoryCacheService()`

Initializes a new instance of the cache service. The cache starts empty.  
**Parameters:** none.  
**Returns:** nothing.  
**Throws:** nothing.

### `public async Task<T?> GetAsync<T>(...)`

Retrieves a cached value by its key.  
**Parameters:**  
- `key` (string) – The unique identifier for the cached item.  

**Returns:**  
- `Task<T?>` – The cached value if found and of type `T`; otherwise `default(T?)`.  

**Throws:**  
- `ArgumentNullException` if `key` is `null`.  
- `InvalidCastException` if the stored value cannot be cast to `T`.

### `public async Task SetAsync<T>(...)`

Stores a value in the cache, overwriting any existing entry with the same key.  
**Parameters:**  
- `key` (string) – The unique identifier for the cached item.  
- `value` (T) – The value to cache.  

**Returns:**  
- `Task` – Completes when the value has been stored.  

**Throws:**  
- `ArgumentNullException` if `key` is `null`.

### `public async Task RemoveAsync(...)`

Removes a single cached entry by its key.  
**Parameters:**  
- `key` (string) – The key of the entry to remove.  

**Returns:**  
- `Task` – Completes when the entry has been removed (no-op if the key does not exist).  

**Throws:**  
- `ArgumentNullException` if `key` is `null`.

### `public async Task<bool> ExistsAsync(...)`

Checks whether a key is present in the cache.  
**Parameters:**  
- `key` (string) – The key to check.  

**Returns:**  
- `Task<bool>` – `true` if the key exists; otherwise `false`.  

**Throws:**  
- `ArgumentNullException` if `key` is `null`.

### `public async Task ClearAsync()`

Removes all entries from the cache.  
**Parameters:** none.  
**Returns:**  
- `Task` – Completes when the cache is empty.  

**Throws:** nothing.

### `public async Task<CacheStatistics> GetStatisticsAsync()`

Returns a snapshot of cache usage statistics.  
**Parameters:** none.  
**Returns:**  
- `Task<CacheStatistics>` – An object containing current cache metrics (e.g., entry count, hit/miss counts).  

**Throws:** nothing.

### `public DateTime CreatedAt`

The date and time (UTC) when this `MemoryCacheService` instance was created.  
**Type:** `DateTime` (read-only).

### `public long Size`

The current number of entries stored in the cache.  
**Type:** `long` (read-only).

## Usage

### Basic caching of a user profile

```csharp
public class UserService
{
    private readonly MemoryCacheService _cache = new();

    public async Task<UserProfile> GetUserProfileAsync(int userId)
    {
        string key = $"user:{userId}";
        var cached = await _cache.GetAsync<UserProfile>(key);
        if (cached != null)
            return cached;

        // Simulate expensive database call
        var profile = await FetchFromDatabaseAsync(userId);
        await _cache.SetAsync(key, profile);
        return profile;
    }
}
```

### Monitoring cache health

```csharp
public class CacheMonitor
{
    private readonly MemoryCacheService _cache;

    public CacheMonitor(MemoryCacheService cache)
    {
        _cache = cache;
    }

    public async Task ReportAsync()
    {
        var stats = await _cache.GetStatisticsAsync();
        Console.WriteLine($"Cache created at: {_cache.CreatedAt:O}");
        Console.WriteLine($"Current size: {_cache.Size}");
        Console.WriteLine($"Hits: {stats.Hits}, Misses: {stats.Misses}");
    }
}
```

## Notes

- All public methods are thread-safe. Concurrent calls to `GetAsync`, `SetAsync`, `RemoveAsync`, `ClearAsync`, and `ExistsAsync` will not corrupt internal state.
- `Size` reflects the number of entries at the moment it is read; it may change immediately after due to concurrent operations.
- `CreatedAt` is set once at construction and does not change.
- Passing a `null` key to any method that accepts a key parameter will throw `ArgumentNullException`.
- The cache does not enforce any expiration policy. Entries remain until explicitly removed or the service is garbage-collected.
- `GetAsync<T>` returns `default(T?)` when the key is missing. For value types, this is `null` (if nullable) or the default value (e.g., `0` for `int`). Use `ExistsAsync` to distinguish between a missing key and a stored default value.
- `CacheStatistics` is a separate type; its exact properties depend on the implementation. Common members include `Hits`, `Misses`, and `EntryCount`.
