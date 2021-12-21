// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Infrastructure;
using FluentAssertions;

namespace DotNetGrpcGateway.Tests;

public class RequestContextTests
{
    [Fact]
    public void Constructor_GeneratesNonEmptyRequestId()
    {
        var ctx = new RequestContext();
        ctx.RequestId.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Constructor_CorrelationIdDefaultsToRequestId()
    {
        var ctx = new RequestContext();
        ctx.CorrelationId.Should().Be(ctx.RequestId);
    }

    [Fact]
    public void Constructor_StartTimeIsSetToApproximatelyNow()
    {
        var before = DateTime.UtcNow;
        var ctx = new RequestContext();
        ctx.StartTime.Should().BeOnOrAfter(before);
        ctx.StartTime.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void SetProperty_NullKey_DoesNotThrow()
    {
        var ctx = new RequestContext();
        var act = () => ctx.SetProperty(null!, "value");
        act.Should().NotThrow();
    }

    [Fact]
    public void SetProperty_EmptyKey_DoesNotAddToProperties()
    {
        var ctx = new RequestContext();
        ctx.SetProperty("", "value");
        ctx.Properties.Should().BeEmpty();
    }

    [Fact]
    public void SetProperty_NullValue_RemovesExistingProperty()
    {
        var ctx = new RequestContext();
        ctx.SetProperty("key", "value");
        ctx.Properties.Should().ContainKey("key");

        ctx.SetProperty("key", null);
        ctx.Properties.Should().NotContainKey("key");
    }

    [Fact]
    public void SetProperty_ValidKeyAndValue_StoresInProperties()
    {
        var ctx = new RequestContext();
        ctx.SetProperty("retryCount", 3);
        ctx.Properties["retryCount"].Should().Be(3);
    }

    [Fact]
    public void GetProperty_ExistingKey_ReturnsTypedValue()
    {
        var ctx = new RequestContext();
        ctx.SetProperty("timeout", 5000);
        ctx.GetProperty<int>("timeout").Should().Be(5000);
    }

    [Fact]
    public void GetProperty_NonExistentKey_ReturnsDefault()
    {
        var ctx = new RequestContext();
        ctx.GetProperty<int>("missing").Should().Be(0);
        ctx.GetProperty<string>("missing").Should().BeNull();
    }

    [Fact]
    public void GetProperty_EmptyKey_ReturnsDefault()
    {
        var ctx = new RequestContext();
        ctx.GetProperty<string>("").Should().BeNull();
    }

    [Fact]
    public void Elapsed_ReturnsPositiveTimeSpan()
    {
        var ctx = new RequestContext();
        Thread.Sleep(10);
        ctx.Elapsed.TotalMilliseconds.Should().BeGreaterThan(0);
    }

    [Fact]
    public void TwoRequestContexts_HaveDifferentRequestIds()
    {
        var ctx1 = new RequestContext();
        var ctx2 = new RequestContext();
        ctx1.RequestId.Should().NotBe(ctx2.RequestId);
    }
}

public class RequestContextAccessorTests
{
    [Fact]
    public void Current_DefaultsToNull()
    {
        var accessor = new RequestContextAccessor();
        accessor.Current.Should().BeNull();
    }

    [Fact]
    public void Current_SetAndGet_ReturnsSameInstance()
    {
        var accessor = new RequestContextAccessor();
        var ctx = new RequestContext();
        accessor.Current = ctx;
        accessor.Current.Should().BeSameAs(ctx);
    }

    [Fact]
    public async Task Current_IsolatedAcrossAsyncFlows()
    {
        var accessor = new RequestContextAccessor();
        var ctx1 = new RequestContext { Path = "/task1" };
        var ctx2 = new RequestContext { Path = "/task2" };

        RequestContext? captured1 = null;
        RequestContext? captured2 = null;

        var task1 = Task.Run(() =>
        {
            accessor.Current = ctx1;
            Thread.Sleep(50);
            captured1 = accessor.Current;
        });

        var task2 = Task.Run(() =>
        {
            accessor.Current = ctx2;
            Thread.Sleep(50);
            captured2 = accessor.Current;
        });

        await Task.WhenAll(task1, task2);

        captured1?.Path.Should().Be("/task1");
        captured2?.Path.Should().Be("/task2");
    }
}
