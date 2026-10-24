#nullable enable

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using DotNetGrpcGateway.Caching;

namespace DotNetGrpcGateway.Tests;

public class MemoryCacheServiceTests : IDisposable
{
    private readonly MemoryCache _memoryCache = new(new MemoryCacheOptions());

    public void Dispose() => _memoryCache.Dispose();

    private MemoryCacheService CreateSut() =>
        new(_memoryCache, NullLogger<MemoryCacheService>.Instance);

    [Fact]
    public void Constructor_NullArguments_ThrowsArgumentNullException()
    {
        Action nullCache = () => new MemoryCacheService(null!, NullLogger<MemoryCacheService>.Instance);
        Action nullLogger = () => new MemoryCacheService(_memoryCache, null!);

        nullCache.Should().Throw<ArgumentNullException>().WithParameterName("cache");
        nullLogger.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public async Task SetAsync_ThenGetAsync_ReturnsValueAndRecordsHit()
    {
        var sut = CreateSut();
        await sut.SetAsync("user:1", new SampleItem("alice"));

        var result = await sut.GetAsync<SampleItem>("user:1");

        result.Should().NotBeNull();
        result!.Name.Should().Be("alice");
        var stats = await sut.GetStatisticsAsync();
        stats.HitCount.Should().Be(1);
        stats.MissCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAsync_MissingKeyOrWrongType_ReturnsDefaultWithoutThrowing()
    {
        var sut = CreateSut();
        await sut.SetAsync("number", 42);

        (await sut.GetAsync<string>("absent")).Should().BeNull();
        (await sut.GetAsync<string>("number")).Should().BeNull();

        var stats = await sut.GetStatisticsAsync();
        stats.MissCount.Should().Be(1);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task NullOrEmptyKey_ReadsReturnDefaults_WriteThrows(string? key)
    {
        var sut = CreateSut();

        (await sut.GetAsync<string>(key!)).Should().BeNull();
        (await sut.ExistsAsync(key!)).Should().BeFalse();
        await sut.RemoveAsync(key!);

        Func<Task> set = () => sut.SetAsync(key!, "value");
        await set.Should().ThrowAsync<ArgumentException>().WithParameterName("key");

        (await sut.GetStatisticsAsync()).MissCount.Should().Be(0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SetAsync_NonPositiveExpiration_StoresEntryWithDefaultTtl(int seconds)
    {
        var sut = CreateSut();

        await sut.SetAsync("key", "value", TimeSpan.FromSeconds(seconds));

        (await sut.ExistsAsync("key")).Should().BeTrue();
    }

    [Fact]
    public async Task RemoveAsync_ExistingKey_RemovesOnlyThatEntry()
    {
        var sut = CreateSut();
        await sut.SetAsync("a", "1");
        await sut.SetAsync("b", "2");

        await sut.RemoveAsync("a");

        (await sut.ExistsAsync("a")).Should().BeFalse();
        (await sut.ExistsAsync("b")).Should().BeTrue();
        (await sut.GetStatisticsAsync()).EntryCount.Should().Be(1);
    }

    [Fact]
    public async Task ClearAsync_RemovesAllEntriesAndResetsCounters()
    {
        var sut = CreateSut();
        await sut.SetAsync("a", "1");
        await sut.SetAsync("b", "2");
        await sut.GetAsync<string>("a");
        await sut.GetAsync<string>("missing");

        await sut.ClearAsync();

        (await sut.ExistsAsync("a")).Should().BeFalse();
        var stats = await sut.GetStatisticsAsync();
        stats.EntryCount.Should().Be(0);
        stats.ApproximateSizeBytes.Should().Be(0);
        stats.HitCount.Should().Be(0);
        stats.MissCount.Should().Be(0);
    }

    [Fact]
    public async Task GetStatisticsAsync_OverwrittenKey_ReportsSizeOfLatestValue()
    {
        var sut = CreateSut();
        await sut.SetAsync("k", "abc");                 // 3 chars * 2 bytes (UTF-16)
        await sut.SetAsync("bytes", new byte[10]);      // 10 bytes

        var stats = await sut.GetStatisticsAsync();
        stats.EntryCount.Should().Be(2);
        stats.ApproximateSizeBytes.Should().Be(16);

        await sut.SetAsync("k", "abcdefgh");            // 8 chars * 2 bytes = 16
        stats = await sut.GetStatisticsAsync();
        stats.EntryCount.Should().Be(2);
        stats.ApproximateSizeBytes.Should().Be(26);
    }

    private sealed record SampleItem(string Name);
}
