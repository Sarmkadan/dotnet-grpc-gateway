using System;
using System.Collections.Generic;
using DotNetGrpcGateway.Domain;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public sealed class RouteChannelOptionsTests
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var options = new RouteChannelOptions();

        Assert.Null(options.CallTimeout);
        Assert.Null(options.MaxReceiveMessageSize);
        Assert.Null(options.MaxSendMessageSize);
        Assert.NotNull(options.AdditionalHeaders);
        Assert.Empty(options.AdditionalHeaders);
        Assert.False(options.SkipTlsVerification);
        Assert.Null(options.TlsTargetName);
    }

    [Fact]
    public void SettingProperties_ShouldPersistValues()
    {
        var options = new RouteChannelOptions
        {
            CallTimeout = TimeSpan.FromSeconds(30),
            MaxReceiveMessageSize = 1024 * 1024,
            MaxSendMessageSize = 512 * 1024,
            SkipTlsVerification = true,
            TlsTargetName = "example.com"
        };

        options.AdditionalHeaders["Authorization"] = "Bearer token";

        Assert.Equal(TimeSpan.FromSeconds(30), options.CallTimeout);
        Assert.Equal(1024 * 1024, options.MaxReceiveMessageSize);
        Assert.Equal(512 * 1024, options.MaxSendMessageSize);
        Assert.True(options.SkipTlsVerification);
        Assert.Equal("example.com", options.TlsTargetName);
        Assert.Single(options.AdditionalHeaders);
        Assert.Equal("Bearer token", options.AdditionalHeaders["Authorization"]);
    }

    [Fact]
    public void AdditionalHeaders_ShouldBeIndependentPerInstance()
    {
        var first = new RouteChannelOptions();
        var second = new RouteChannelOptions();

        first.AdditionalHeaders["X-First"] = "value1";
        second.AdditionalHeaders["X-Second"] = "value2";

        Assert.Single(first.AdditionalHeaders);
        Assert.Single(second.AdditionalHeaders);
        Assert.Equal("value1", first.AdditionalHeaders["X-First"]);
        Assert.Equal("value2", second.AdditionalHeaders["X-Second"]);
    }

    [Fact]
    public void SettingNullValues_ShouldAllowNull()
    {
        var options = new RouteChannelOptions
        {
            CallTimeout = TimeSpan.FromSeconds(10),
            MaxReceiveMessageSize = 2048,
            MaxSendMessageSize = 4096
        };

        options.CallTimeout = null;
        options.MaxReceiveMessageSize = null;
        options.MaxSendMessageSize = null;

        Assert.Null(options.CallTimeout);
        Assert.Null(options.MaxReceiveMessageSize);
        Assert.Null(options.MaxSendMessageSize);
    }

    [Fact]
    public void BoundaryValues_ShouldAcceptLargeIntegers()
    {
        var options = new RouteChannelOptions
        {
            MaxReceiveMessageSize = int.MaxValue,
            MaxSendMessageSize = int.MaxValue
        };

        Assert.Equal(int.MaxValue, options.MaxReceiveMessageSize);
        Assert.Equal(int.MaxValue, options.MaxSendMessageSize);
    }

    [Fact]
    public void SkipTlsVerification_ShouldToggleCorrectly()
    {
        var options = new RouteChannelOptions();

        Assert.False(options.SkipTlsVerification);

        options.SkipTlsVerification = true;
        Assert.True(options.SkipTlsVerification);

        options.SkipTlsVerification = false;
        Assert.False(options.SkipTlsVerification);
    }
}
