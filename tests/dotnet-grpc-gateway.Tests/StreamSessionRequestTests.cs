using Xunit;
using DotNetGrpcGateway.Streaming;

namespace DotNetGrpcGateway.Tests;

public class StreamSessionRequestTests
{
    [Fact]
    public void DefaultConstructor_PropertiesAreNullOrEmpty()
    {
        // Arrange & Act
        var request = new StreamSessionRequest();

        // Assert
        Assert.Null(request.ServiceName);
        Assert.Null(request.MethodName);
        Assert.Null(request.RoutePath);
        Assert.NotNull(request.Headers);
        Assert.Empty(request.Headers);
    }

    [Fact]
    public void SetServiceNameAndMethodName_ValuesAreStored()
    {
        // Arrange
        var request = new StreamSessionRequest();

        // Act
        request.ServiceName = "TestService";
        request.MethodName = "TestMethod";

        // Assert
        Assert.Equal("TestService", request.ServiceName);
        Assert.Equal("TestMethod", request.MethodName);
    }

    [Fact]
    public void SetRoutePath_ValuesAreStored()
    {
        // Arrange
        var request = new StreamSessionRequest();

        // Act
        request.RoutePath = "/test/path";

        // Assert
        Assert.Equal("/test/path", request.RoutePath);
    }

    [Fact]
    public void SetRoutePathToNull_IsNull()
    {
        // Arrange
        var request = new StreamSessionRequest();

        // Act
        request.RoutePath = null;

        // Assert
        Assert.Null(request.RoutePath);
    }

    [Fact]
    public void SetHeaders_ValuesAreStored()
    {
        // Arrange
        var request = new StreamSessionRequest();
        var headers = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };

        // Act
        request.Headers = headers;

        // Assert
        Assert.Equal(2, request.Headers.Count);
        Assert.Equal("value1", request.Headers["key1"]);
        Assert.Equal("value2", request.Headers["key2"]);
    }

    [Fact]
    public void SetHeadersToNull_IsNull()
    {
        // Arrange
        var request = new StreamSessionRequest();

        // Act
        request.Headers = null;

        // Assert
        Assert.Null(request.Headers);
    }

    [Fact]
    public void HeadersDefaultIsEmptyDictionary()
    {
        // Arrange
        var request = new StreamSessionRequest();

        // Assert
        Assert.NotNull(request.Headers);
        Assert.Empty(request.Headers);
    }

    [Fact]
    public void SetServiceNameToNull_IsNull()
    {
        // Arrange
        var request = new StreamSessionRequest();

        // Act
        request.ServiceName = null;

        // Assert
        Assert.Null(request.ServiceName);
    }

    [Fact]
    public void SetMethodNameToNull_IsNull()
    {
        // Arrange
        var request = new StreamSessionRequest();

        // Act
        request.MethodName = null;

        // Assert
        Assert.Null(request.MethodName);
    }
}