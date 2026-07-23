using Xunit;
using DotNetGrpcGateway.Domain;

namespace DotNetGrpcGateway.Tests;

public class GrpcServiceJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsJson()
    {
        // Arrange
        var grpcService = new GrpcService { Host = "localhost", Port = 8080, UseTls = true };

        // Act
        var json = grpcService.ToJson();

        // Assert
        Assert.NotEmpty(json);
    }

    [Fact]
    public void ToJson_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => (null as GrpcService).ToJson());
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsGrpcService()
    {
        // Arrange
        var json = "{\"Host\":\"localhost\",\"Port\":8080,\"UseTls\":true}";

        // Act
        var grpcService = GrpcServiceJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(grpcService);
        Assert.Equal("localhost", grpcService.Host);
        Assert.Equal(8080, grpcService.Port);
        Assert.True(grpcService.UseTls);
    }

    [Fact]
    public void FromJson_NullInput_ReturnsNull()
    {
        // Act
        var grpcService = GrpcServiceJsonExtensions.FromJson(null);

        // Assert
        Assert.Null(grpcService);
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrue()
    {
        // Arrange
        var json = "{\"Host\":\"localhost\",\"Port\":8080,\"UseTls\":true}";

        // Act
        var success = GrpcServiceJsonExtensions.TryFromJson(json, out var grpcService);

        // Assert
        Assert.True(success);
        Assert.NotNull(grpcService);
        Assert.Equal("localhost", grpcService.Host);
        Assert.Equal(8080, grpcService.Port);
        Assert.True(grpcService.UseTls);
    }

    [Fact]
    public void TryFromJson_NullInput_ReturnsFalse()
    {
        // Act
        var success = GrpcServiceJsonExtensions.TryFromJson(null, out var grpcService);

        // Assert
        Assert.False(success);
        Assert.Null(grpcService);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse()
    {
        // Arrange
        var json = "{\"Host\":\"localhost\",\"Port\":8080,\"UseTls\":true\"}";

        // Act
        var success = GrpcServiceJsonExtensions.TryFromJson(json, out var grpcService);

        // Assert
        Assert.False(success);
        Assert.Null(grpcService);
    }
}
