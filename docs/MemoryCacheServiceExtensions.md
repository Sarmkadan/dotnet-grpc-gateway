# MemoryCacheServiceExtensions

Extension methods for `IMemoryCache` that provide asynchronous, batch-oriented operations for common caching scenarios. Designed for scenarios where cache misses trigger expensive operations (e.g., database queries, external API calls) and where bulk operations improve throughput.

## API

### `TryGetValueAsync<T>`

Attempts to retrieve a value from the cache asynchronously. Returns a tuple indicating whether the value exists and the value itself if found.

- **Parameters**:
  - `cache`: The `IMemoryCache` instance.
  - `key`: The cache key to look up.
- **Return value**: A tuple `(Exists, Value)` where `Exists` is `true` if the key was found, and `Value` is the cached value of type `T` (or `null` if not found).
- **Exceptions**: Throws `ArgumentNullException` if `cache` or `key` is `null`.

### `GetOrCreateAsync<T>`

Retrieves a value from the cache or creates and stores it if missing. The creation function is invoked only when the key is absent.

- **Parameters**:
  - `cache`: The `IMemoryCache` instance.
  - `key`: The cache key to look up or create.
  - `factory`: An asynchronous function to create the value if the key is missing.
- **Return value**: The cached or newly created value of type `T`.
- **Exceptions**:
  - Throws `ArgumentNullException` if `cache`, `key`, or `factory` is `null`.
  - Propagates any exception thrown by `factory`.

### `GetManyAsync<T>`

Retrieves multiple values from the cache asynchronously in a single operation. Missing keys are omitted from the result.

- **Parameters**:
  - `cache`: The `IMemoryCache` instance.
  - `keys`: An enumerable of cache keys to look up.
- **Return value**: A `Dictionary<string, T>` mapping each found key to its cached value. Only keys present in the cache are included.
- **Exceptions**: Throws `ArgumentNullException` if `cache` or `keys` is `null`.

### `SetManyAsync<T>`

Stores multiple values in the cache asynchronously in a single operation.

- **Parameters**:
  - `cache`: The `IMemoryCache` instance.
  - `entries`: A dictionary mapping cache keys to values to store.
- **Return value**: None (`Task`).
- **Exceptions**:
  - Throws `ArgumentNullException` if `cache` or `entries` is `null`.
  - Throws `ArgumentException` if `entries` contains a `null` key.

### `GetStatisticsWithRatioAsync`

Retrieves cache statistics and calculates the hit ratio based on total operations and cache hits.

- **Parameters**:
  - `cache`: The `IMemoryCache` instance.
- **Return value**: A tuple `(Statistics, HitRatio)` where `Statistics` contains cache metrics (e.g., hit count, miss count) and `HitRatio` is the ratio of hits to total operations (a value between 0.0 and 1.0).
- **Exceptions**: Throws `ArgumentNullException` if `cache` is `null`.

### `RemoveManyAsync`

Removes multiple cache entries asynchronously in a single operation.

- **Parameters**:
  - `cache`: The `IMemoryCache` instance.
  - `keys`: An enumerable of cache keys to remove.
- **Return value**: None (`Task`).
- **Exceptions**: Throws `ArgumentNullException` if `cache` or `keys` is `null`.

### `ExistsManyAsync`

Checks the existence of multiple cache keys asynchronously in a single operation.

- **Parameters**:
  - `cache`: The `IMemoryCache` instance.
  - `keys`: An enumerable of cache keys to check.
- **Return value**: A `Dictionary<string, bool>` mapping each key to `true` if present, `false` otherwise.
- **Exceptions**: Throws `ArgumentNullException` if `cache` or `keys` is `null`.

## Usage

### Example 1: Bulk retrieval with fallback creation
