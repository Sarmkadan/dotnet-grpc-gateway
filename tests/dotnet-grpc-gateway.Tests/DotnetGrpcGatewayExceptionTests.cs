#nullable enable
using System;
using DotNetGrpcGateway.Exceptions;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class DotnetGrpcGatewayExceptionTests
{
    [Fact]
    public void DefaultConstructor_ShouldCreateException_WithDefaultMessage()
    {
        // Act
        var ex = new DotnetGrpcGatewayException();

        // Assert
        Assert.NotNull(ex);
        // The default Exception message contains the full type name.
        Assert.Contains(nameof(DotnetGrpcGatewayException), ex.Message);
    }

    [Fact]
    public void MessageConstructor_ShouldSetMessage()
    {
        // Arrange
        var message = "Custom error message";

        // Act
        var ex = new DotnetGrpcGatewayException(message);

        // Assert
        Assert.Equal(message, ex.Message);
        Assert.Null(ex.InnerException);
    }

    [Fact]
    public void MessageConstructor_NullMessage_ShouldResultInNullMessage()
    {
        // Act
        var ex = new DotnetGrpcGatewayException((string?)null);

        // Assert
        Assert.Null(ex.Message);
        Assert.Null(ex.InnerException);
    }

    [Fact]
    public void MessageAndInnerExceptionConstructor_ShouldSetBoth()
    {
        // Arrange
        var message = "Outer exception";
        var inner = new InvalidOperationException("Inner exception");

        // Act
        var ex = new DotnetGrpcGatewayException(message, inner);

        // Assert
        Assert.Equal(message, ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void MessageAndInnerExceptionConstructor_NullInnerException_ShouldSetMessageOnly()
    {
        // Arrange
        var message = "Only outer message";

        // Act
        var ex = new DotnetGrpcGatewayException(message, null);

        // Assert
        Assert.Equal(message, ex.Message);
        Assert.Null(ex.InnerException);
    }
}
