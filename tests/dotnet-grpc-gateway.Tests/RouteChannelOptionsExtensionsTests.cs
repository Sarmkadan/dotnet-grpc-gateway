namespace DotNetGrpcGateway.Tests;

using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using DotNetGrpcGateway.Domain;

public class RouteChannelOptionsExtensionsTests
{
    [Fact]
    public void WithCallTimeoutMs_HappyPath_ReturnsConfiguredOptions()
    {
        // Arrange
        var options = new RouteChannelOptions();
        var timeoutMs = 1000;

        // Act
        var result = options.WithCallTimeoutMs(timeoutMs);

        // Assert
        Assert.Equal(timeoutMs, result.CallTimeout.Value.TotalMilliseconds);
    }

    [Fact]
    public void WithMaxReceiveMessageSize_HappyPath_ReturnsConfiguredOptions()
    {
        // Arrange
        var options = new RouteChannelOptions();
        var maxSize = 1024;

        // Act
        var result = options.WithMaxReceiveMessageSize(maxSize);

        // Assert
        Assert.Equal(maxSize, result.MaxReceiveMessageSize);
    }

    [Fact]
    public void WithMaxSendMessageSize_HappyPath_ReturnsConfiguredOptions()
    {
        // Arrange
        var options = new RouteChannelOptions();
        var maxSize = 1024;

        // Act
        var result = options.WithMaxSendMessageSize(maxSize);

        // Assert
        Assert.Equal(maxSize, result.MaxSendMessageSize);
    }

    [Fact]
    public void WithHeader_HappyPath_ReturnsConfiguredOptions()
    {
        // Arrange
        var options = new RouteChannelOptions();
        var name = "header-name";
        var value = "header-value";

        // Act
        var result = options.WithHeader(name, value);

        // Assert
        Assert.Equal(value, result.AdditionalHeaders[name]);
    }

    [Fact]
    public void WithTlsTargetName_HappyPath_ReturnsConfiguredOptions()
    {
        // Arrange
        var options = new RouteChannelOptions();
        var targetName = "tls-target-name";

        // Act
        var result = options.WithTlsTargetName(targetName);

        // Assert
        Assert.Equal(targetName, result.TlsTargetName);
    }

    [Fact]
    public void WithSkipTlsVerification_HappyPath_ReturnsConfiguredOptions()
    {
        // Arrange
        var options = new RouteChannelOptions();
        var skip = true;

        // Act
        var result = options.WithSkipTlsVerification(skip);

        // Assert
        Assert.Equal(skip, result.SkipTlsVerification);
    }

    [Fact]
    public void UpdateFrom_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        var options = new RouteChannelOptions();

        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => options.UpdateFrom(null));
    }

    [Fact]
    public void UpdateFrom_EmptySource_DoesNotThrow()
    {
        // Arrange
        var options = new RouteChannelOptions();
        var source = new RouteChannelOptions();

        // Act and Assert
        options.UpdateFrom(source);
    }
}
