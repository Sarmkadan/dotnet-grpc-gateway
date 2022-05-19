// ... (rest of the file remains the same)

## MemoryCacheServiceExtensions

The `MemoryCacheServiceExtensions` class provides utility methods for working with the `MemoryCacheService`. It allows you to easily interact with the cache, including getting, setting, and removing values.

### Example Usage:

```csharp
var cacheService = new MemoryCacheService();

var value = await cacheService.GetOrCreateAsync("myKey", async () => 
{
    // Simulate an expensive operation
    await Task.Delay(100);
    return "Cached Value";
});

var exists = await cacheService.TryGetValueAsync<string>("myKey");
Console.WriteLine($"Exists: {exists.Exists}, Value: {exists.Value}");

var multipleValues = await cacheService.GetManyAsync<string>(new[] { "myKey", "anotherKey" });
Console.WriteLine($"Multiple Values: {string.Join(", ", multipleValues.Select(x => x.Value))}");

await cacheService.SetManyAsync(new[] 
{
    new KeyValuePair<string, string>("key1", "value1"),
    new KeyValuePair<string, string>("key2", "value2"),
});

var statistics = await cacheService.GetStatisticsWithRatioAsync();
Console.WriteLine($"Hit Ratio: {statistics.HitRatio}");

await cacheService.RemoveManyAsync(new[] { "key1", "key2" });

var existsMany = await cacheService.ExistsManyAsync(new[] { "key1", "key2" });
Console.WriteLine($"Exists Many: {string.Join(", ", existsMany.Select(x => $"{x.Key}: {x.Value}"))}");
``` 

// ... (rest of the file remains the same)
