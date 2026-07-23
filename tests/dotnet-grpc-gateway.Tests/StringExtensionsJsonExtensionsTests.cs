using Xunit;
using DotNetGrpcGateway.Extensions;

namespace DotNetGrpcGateway.Tests;

public class StringExtensionsJsonExtensionsTests
{
    [Fact]
    public void ToJsonString_HappyPath_ReturnsJsonString()
    {
        // Arrange
        var value = "Hello, World!";
        var expected = "{\"value\":\"Hello, World!\"}";

        // Act
        var result = StringExtensionsJsonExtensions.ToJsonString(value);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToJsonString_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => StringExtensionsJsonExtensions.ToJsonString(null));
    }

    [Fact]
    public void FromJsonString_HappyPath_ReturnsDeserializedValue()
    {
        // Arrange
        var json = "{\"value\":\"Hello, World!\"}";
        var expected = "Hello, World!";

        // Act
        var result = StringExtensionsJsonExtensions.FromJsonString(json);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void FromJsonString_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => StringExtensionsJsonExtensions.FromJsonString(null));
    }

    [Fact]
    public void FromJsonString_EmptyString_ReturnsNull()
    {
        // Act
        var result = StringExtensionsJsonExtensions.FromJsonString("");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJsonString_HappyPath_ReturnsTrue()
    {
        // Arrange
        var json = "{\"value\":\"Hello, World!\"}";
        var expected = "Hello, World!";

        // Act
        var result = StringExtensionsJsonExtensions.TryFromJsonString(json, out var value);

        // Assert
        Assert.True(result);
        Assert.Equal(expected, value);
    }

    [Fact]
    public void TryFromJsonString_NullInput_ReturnsFalse()
    {
        // Act
        var result = StringExtensionsJsonExtensions.TryFromJsonString(null, out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TryFromJsonString_EmptyString_ReturnsFalse()
    {
        // Act
        var result = StringExtensionsJsonExtensions.TryFromJsonString("", out _);

        // Assert
        Assert.False(result);
    }
}
