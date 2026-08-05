using System;
using DotNetGrpcGateway.Options;
using FluentAssertions;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class DotnetGrpcGatewayOptionsExtensionsTests
{
    [Fact]
    public void UseLocalhost_SetsListenAddressToLoopback()
    {
        var options = new DotnetGrpcGatewayOptions();

        var result = options.UseLocalhost();

        result.Should().BeSameAs(options);
        result.ListenAddress.Should().Be("127.0.0.1");
    }

    [Fact]
    public void UseAllInterfaces_SetsListenAddressToAllInterfaces()
    {
        var options = new DotnetGrpcGatewayOptions();

        var result = options.UseAllInterfaces();

        result.Should().BeSameAs(options);
        result.ListenAddress.Should().Be("0.0.0.0");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UseAddress_ThrowsArgumentException_WhenAddressIsNullOrWhitespace(string? address)
    {
        var options = new DotnetGrpcGatewayOptions();

        var act = () => options.UseAddress(address!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UseAddress_SetsListenAddress_WhenValid()
    {
        var options = new DotnetGrpcGatewayOptions();

        var result = options.UseAddress("192.168.1.10");

        result.ListenAddress.Should().Be("192.168.1.10");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(65536)]
    [InlineData(-1)]
    public void UsePort_ThrowsArgumentOutOfRangeException_WhenPortIsOutOfRange(int port)
    {
        var options = new DotnetGrpcGatewayOptions();

        var act = () => options.UsePort(port);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5000)]
    [InlineData(65535)]
    public void UsePort_SetsPort_WhenWithinValidRange(int port)
    {
        var options = new DotnetGrpcGatewayOptions();

        var result = options.UsePort(port);

        result.Port.Should().Be(port);
    }

    [Fact]
    public void DisableReflection_SetsEnableReflectionToFalse()
    {
        var options = new DotnetGrpcGatewayOptions();

        var result = options.DisableReflection();

        result.EnableReflection.Should().BeFalse();
    }

    [Fact]
    public void DisableMetrics_DisablesBothTopLevelAndNestedMetrics()
    {
        var options = new DotnetGrpcGatewayOptions();

        var result = options.DisableMetrics();

        result.EnableMetrics.Should().BeFalse();
        result.Metrics.EnableMetrics.Should().BeFalse();
    }

    [Fact]
    public void ConfigureHealthCheck_AppliesConfigureAction()
    {
        var options = new DotnetGrpcGatewayOptions();

        var result = options.ConfigureHealthCheck(hc => hc.FailureThreshold = 7);

        result.HealthCheck.FailureThreshold.Should().Be(7);
    }

    [Fact]
    public void ConfigureHealthCheck_ThrowsArgumentNullException_WhenConfigureIsNull()
    {
        var options = new DotnetGrpcGatewayOptions();

        var act = () => options.ConfigureHealthCheck(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ConfigureMetrics_AppliesConfigureAction()
    {
        var options = new DotnetGrpcGatewayOptions();

        var result = options.ConfigureMetrics(m => m.RetentionDays = 90);

        result.Metrics.RetentionDays.Should().Be(90);
    }

    [Fact]
    public void ConfigureRequestLogging_AppliesConfigureAction()
    {
        var options = new DotnetGrpcGatewayOptions();

        var result = options.ConfigureRequestLogging(rl => rl.Enabled = false);

        result.RequestLogging.Enabled.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void SetLogLevel_ThrowsArgumentException_WhenLogLevelIsNullOrWhitespace(string? logLevel)
    {
        var options = new DotnetGrpcGatewayOptions();

        var act = () => options.SetLogLevel(logLevel!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetLogLevel_SetsLogLevel_WhenValid()
    {
        var options = new DotnetGrpcGatewayOptions();

        var result = options.SetLogLevel("Debug");

        result.LogLevel.Should().Be("Debug");
    }

    [Fact]
    public void UseLocalhost_ThrowsArgumentNullException_WhenOptionsIsNull()
    {
        DotnetGrpcGatewayOptions options = null!;

        var act = () => options.UseLocalhost();

        act.Should().Throw<ArgumentNullException>();
    }
}
