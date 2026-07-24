using Xunit;
using DotNetGrpcGateway.Domain;

namespace DotNetGrpcGateway.Tests;

public class GatewayConfigurationTests
{
    [Fact]
    public void Validate_HappyPath_NoException()
    {
        // Arrange
        var config = new GatewayConfiguration
        {
            Name = "Test Gateway",
            Port = 5000,
            MaxConcurrentConnections = 100,
            RequestTimeoutMs = 30000,
            MaxMessageSize = 10 * 1024 * 1024
        };

        // Act and Assert
        config.Validate();
    }

    [Fact]
    public void Validate_InvalidName_ThrowsException()
    {
        // Arrange
        var config = new GatewayConfiguration
        {
            Name = string.Empty,
            Port = 5000,
            MaxConcurrentConnections = 100,
            RequestTimeoutMs = 30000,
            MaxMessageSize = 10 * 1024 * 1024
        };

        // Act and Assert
        Assert.Throws<InvalidOperationException>(() => config.Validate());
    }

    [Fact]
    public void Validate_InvalidPort_ThrowsException()
    {
        // Arrange
        var config = new GatewayConfiguration
        {
            Name = "Test Gateway",
            Port = 0,
            MaxConcurrentConnections = 100,
            RequestTimeoutMs = 30000,
            MaxMessageSize = 10 * 1024 * 1024
        };

        // Act and Assert
        Assert.Throws<InvalidOperationException>(() => config.Validate());
    }

    [Fact]
    public void Validate_InvalidMaxConcurrentConnections_ThrowsException()
    {
        // Arrange
        var config = new GatewayConfiguration
        {
            Name = "Test Gateway",
            Port = 5000,
            MaxConcurrentConnections = 0,
            RequestTimeoutMs = 30000,
            MaxMessageSize = 10 * 1024 * 1024
        };

        // Act and Assert
        Assert.Throws<InvalidOperationException>(() => config.Validate());
    }

    [Fact]
    public void Validate_InvalidRequestTimeoutMs_ThrowsException()
    {
        // Arrange
        var config = new GatewayConfiguration
        {
            Name = "Test Gateway",
            Port = 5000,
            MaxConcurrentConnections = 100,
            RequestTimeoutMs = 0,
            MaxMessageSize = 10 * 1024 * 1024
        };

        // Act and Assert
        Assert.Throws<InvalidOperationException>(() => config.Validate());
    }

    [Fact]
    public void UpdateModifiedDate_ModifiesModifiedDate()
    {
        // Arrange
        var config = new GatewayConfiguration
        {
            Name = "Test Gateway",
            Port = 5000,
            MaxConcurrentConnections = 100,
            RequestTimeoutMs = 30000,
            MaxMessageSize = 10 * 1024 * 1024
        };

        // Act
        config.UpdateModifiedDate();

        // Assert
        Assert.True(config.ModifiedAt > config.CreatedAt);
    }
}
