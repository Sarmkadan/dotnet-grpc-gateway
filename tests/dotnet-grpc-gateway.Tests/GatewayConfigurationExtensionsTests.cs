using DotNetGrpcGateway.Domain;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class GatewayConfigurationExtensionsTests
{
    [Fact]
    public void GetEndpointUrl_HappyPath_ReturnsExpectedUrl()
    {
        // Arrange
        var configuration = new GatewayConfiguration { ListenAddress = "localhost", Port = 5000 };

        // Act
        var url = configuration.GetEndpointUrl();

        // Assert
        Assert.Equal("http://localhost:5000", url);
    }

    [Fact]
    public void EnableSsl_HappyPath_ReturnsExpectedValue()
    {
        // Arrange
        var configuration = new GatewayConfiguration { ListenAddress = "localhost", Port = 5000 };

        // Act
        var enabled = configuration.EnableSsl();

        // Assert
        Assert.False(enabled);
    }

    [Fact]
    public void GetMaxRequestSizeBytes_HappyPath_ReturnsExpectedValue()
    {
        // Arrange
        var configuration = new GatewayConfiguration { MaxMessageSize = 1024 };

        // Act
        var size = configuration.GetMaxRequestSizeBytes();

        // Assert
        Assert.Equal(1024, size);
    }

    [Fact]
    public void GetRequestTimeout_HappyPath_ReturnsExpectedValue()
    {
        // Arrange
        var configuration = new GatewayConfiguration { RequestTimeoutMs = 1000 };

        // Act
        var timeout = configuration.GetRequestTimeout();

        // Assert
        Assert.Equal(TimeSpan.FromMilliseconds(1000), timeout);
    }

    [Fact]
    public void CreateDefault_HappyPath_ReturnsExpectedConfiguration()
    {
        // Act
        var configuration = GatewayConfigurationExtensions.CreateDefault("Test");

        // Assert
        Assert.NotNull(configuration);
        Assert.Equal("Test", configuration.Name);
    }

    [Fact]
    public void Clone_HappyPath_ReturnsExpectedConfiguration()
    {
        // Arrange
        var configuration = new GatewayConfiguration { Name = "Test" };

        // Act
        var cloned = configuration.Clone();

        // Assert
        Assert.NotNull(cloned);
        Assert.Equal("Test", cloned.Name);
    }

    [Fact]
    public void ListensOnIp_HappyPath_ReturnsExpectedValue()
    {
        // Arrange
        var configuration = new GatewayConfiguration { ListenAddress = "localhost" };

        // Act
        var listens = configuration.ListensOnIp("localhost");

        // Assert
        Assert.True(listens);
    }

    [Fact]
    public void GetMonitoringConnectionString_HappyPath_ReturnsExpectedValue()
    {
        // Arrange
        var configuration = new GatewayConfiguration { ListenAddress = "localhost", Port = 5000 };

        // Act
        var connectionString = configuration.GetMonitoringConnectionString();

        // Assert
        Assert.Equal("grpc://localhost:5000", connectionString);
    }

    [Fact]
    public void GetEndpointUrl_NullConfiguration_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => ((GatewayConfiguration?)null).GetEndpointUrl());
    }
}
