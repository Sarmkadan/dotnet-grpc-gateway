using Xunit;
using DotNetGrpcGateway.Extensions;

namespace DotNetGrpcGateway.Tests;

public class StringExtensionsTests
{
    [Fact]
    public void ToSha256Hash_HappyPath_ReturnsHash()
    {
        // Arrange
        var input = "Hello, World!";

        // Act
        var result = input.ToSha256Hash();

        // Assert
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ToSha256Hash_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => ("Hello, World!".ToSha256Hash()));
    }

    [Fact]
    public void ToSlug_HappyPath_ReturnsSlug()
    {
        // Arrange
        var input = "Hello, World!";

        // Act
        var result = input.ToSlug();

        // Assert
        Assert.Equal("hello-world", result);
    }

    [Fact]
    public void ToSlug_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => ("Hello, World!".ToSlug()));
    }

    [Fact]
    public void Truncate_HappyPath_ReturnsTruncatedString()
    {
        // Arrange
        var input = "Hello, World!";
        var maxLength = 10;

        // Act
        var result = input.Truncate(maxLength);

        // Assert
        Assert.Equal("Hello, Wo...", result);
    }

    [Fact]
    public void Truncate_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => ("Hello, World!".Truncate(10)));
    }

    [Fact]
    public void IsValidIpAddress_HappyPath_ReturnsTrue()
    {
        // Arrange
        var input = "192.168.1.1";

        // Act
        var result = input.IsValidIpAddress();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidIpAddress_NullInput_ReturnsFalse()
    {
        // Act
        var result = ("Hello, World!".IsValidIpAddress());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void MatchesPattern_HappyPath_ReturnsTrue()
    {
        // Arrange
        var input = "Hello, World!";
        var pattern = "Hello, *";

        // Act
        var result = input.MatchesPattern(pattern);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesPattern_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => ("Hello, World!".MatchesPattern("Hello, *")));
    }

    [Fact]
    public void MatchesPattern_NullPattern_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => ("Hello, World!".MatchesPattern(null)));
    }
}
