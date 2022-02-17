#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class RequestLogServiceTests
{
    private static RequestLogService CreateSut(int capacity = 1000) => new(capacity);

    private static RequestLogEntry BuildEntry(
        string method = "helloworld.Greeter/SayHello",
        int statusCode = 200,
        long durationMs = 50) => new()
    {
        GrpcMethod = method,
        Method = "POST",
        Path = "/" + method,
        StatusCode = statusCode,
        DurationMs = durationMs,
        Timestamp = DateTime.UtcNow
    };

    [Fact]
    public void Append_AddsEntryToStore()
    {
        var sut = CreateSut();
        sut.Append(BuildEntry());

        sut.GetRecent(10).Should().HaveCount(1);
    }

    [Fact]
    public void GetRecent_ReturnsNewestFirst()
    {
        var sut = CreateSut();
        var older = BuildEntry();
        older.Timestamp = DateTime.UtcNow.AddSeconds(-5);
        var newer = BuildEntry();
        newer.Timestamp = DateTime.UtcNow;

        sut.Append(older);
        sut.Append(newer);

        var result = sut.GetRecent(10);
        result[0].Timestamp.Should().BeOnOrAfter(result[1].Timestamp);
    }

    [Fact]
    public void Search_FilterByMethod_ReturnsMatchingEntries()
    {
        var sut = CreateSut();
        sut.Append(BuildEntry(method: "UserService/GetUser"));
        sut.Append(BuildEntry(method: "OrderService/PlaceOrder"));

        var results = sut.Search(methodFilter: "UserService");

        results.Should().HaveCount(1);
        results[0].GrpcMethod.Should().Contain("UserService");
    }

    [Fact]
    public void Search_FilterByStatusCode_ReturnsOnlyMatchingStatusCodes()
    {
        var sut = CreateSut();
        sut.Append(BuildEntry(statusCode: 200));
        sut.Append(BuildEntry(statusCode: 500));
        sut.Append(BuildEntry(statusCode: 200));

        var errorEntries = sut.Search(statusCode: 500);

        errorEntries.Should().HaveCount(1);
        errorEntries[0].StatusCode.Should().Be(500);
    }

    [Fact]
    public void Append_OverCapacity_EvictsOldestEntries()
    {
        var sut = CreateSut(capacity: 3);
        for (var i = 0; i < 5; i++)
            sut.Append(BuildEntry());

        sut.GetRecent(100).Count.Should().BeLessThanOrEqualTo(3);
    }

    [Fact]
    public void GetSummary_ReflectsAggregates()
    {
        var sut = CreateSut();
        sut.Append(BuildEntry(statusCode: 200, durationMs: 100));
        sut.Append(BuildEntry(statusCode: 500, durationMs: 200));

        var summary = sut.GetSummary();

        summary.TotalEntries.Should().Be(2);
        summary.SuccessCount.Should().Be(1);
        summary.ErrorCount.Should().Be(1);
        summary.AverageDurationMs.Should().Be(150);
    }

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        var sut = CreateSut();
        sut.Append(BuildEntry());
        sut.Append(BuildEntry());
        sut.Clear();

        sut.GetRecent(100).Should().BeEmpty();
    }
}
