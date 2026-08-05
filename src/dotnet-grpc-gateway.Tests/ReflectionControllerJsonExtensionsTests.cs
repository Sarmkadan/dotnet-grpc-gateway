using System;
using DotNetGrpcGateway.Controllers;
using DotNetGrpcGateway.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class ReflectionControllerJsonExtensionsTests
{
    private static ReflectionController CreateController()
    {
        var reflectionServiceMock = new Mock<IReflectionService>();
        var logger = NullLogger<ReflectionController>.Instance;
        return new ReflectionController(reflectionServiceMock.Object, logger);
    }

    [Fact]
    public void ToJson_Returns_ValidJson()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var json = controller.ToJson();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(json));
        // The JSON should start with an object brace
        Assert.StartsWith("{", json);
    }

    [Fact]
    public void ToJson_WithIndent_Produces_IndentedJson()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var indentedJson = controller.ToJson(indented: true);
        var nonIndentedJson = controller.ToJson(indented: false);

        // Assert
        Assert.NotEqual(nonIndentedJson, indentedJson);
        // Indented JSON contains line breaks (environment new line)
        Assert.Contains(Environment.NewLine, indentedJson);
        // Non‑indented JSON should not contain line breaks
        Assert.DoesNotContain(Environment.NewLine, nonIndentedJson);
    }

    [Fact]
    public void FromJson_ValidJson_Returns_Object()
    {
        // Arrange
        var controller = CreateController();
        var json = controller.ToJson();

        // Act
        var result = ReflectionControllerJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
        // The deserialized instance should be of the correct type
        Assert.IsType<ReflectionController>(result);
    }

    [Fact]
    public void FromJson_NullOrEmpty_Throws_ArgumentException()
    {
        // Null
        Assert.Throws<ArgumentException>(() => ReflectionControllerJsonExtensions.FromJson(null!));

        // Empty
        Assert.Throws<ArgumentException>(() => ReflectionControllerJsonExtensions.FromJson(string.Empty));
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrue_AndSetsValue()
    {
        // Arrange
        var controller = CreateController();
        var json = controller.ToJson();

        // Act
        var success = ReflectionControllerJsonExtensions.TryFromJson(json, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
        Assert.IsType<ReflectionController>(result);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse_AndSetsNull()
    {
        // Arrange
        var invalidJson = "{ this is not valid json }";

        // Act
        var success = ReflectionControllerJsonExtensions.TryFromJson(invalidJson, out var result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_NullOrEmpty_Throws_ArgumentException()
    {
        // Null
        Assert.Throws<ArgumentException>(() => ReflectionControllerJsonExtensions.TryFromJson(null!, out _));

        // Empty
        Assert.Throws<ArgumentException>(() => ReflectionControllerJsonExtensions.TryFromJson(string.Empty, out _));
    }
}
