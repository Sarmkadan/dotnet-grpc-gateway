using Xunit;
using DotNetGrpcGateway.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;

namespace DotNetGrpcGateway.Tests;

public class HttpContextExtensionsTests
{
    private readonly Mock<HttpContext> _mockContext;
    private readonly DefaultHttpContext _defaultContext;

    public HttpContextExtensionsTests()
    {
        _mockContext = new Mock<HttpContext>();
        _defaultContext = new DefaultHttpContext();
    }

    [Fact]
    public void GetClientIpAddress_HappyPath_WithRemoteIpAddress_ReturnsIpAddress()
    {
        // Arrange
        var connection = new Mock<ConnectionInfo>();
        connection.Setup(c => c.RemoteIpAddress).Returns(System.Net.IPAddress.Parse("192.168.1.100"));
        _mockContext.Setup(c => c.Connection).Returns(connection.Object);
        _mockContext.Setup(c => c.Request.Headers).Returns(new HeaderDictionary());

        // Act
        var result = _mockContext.Object.GetClientIpAddress();

        // Assert
        Assert.Equal("192.168.1.100", result);
    }

    [Fact]
    public void GetClientIpAddress_HappyPath_WithXForwardedFor_ReturnsFirstIp()
    {
        // Arrange
        var connection = new Mock<ConnectionInfo>();
        connection.Setup(c => c.RemoteIpAddress).Returns(System.Net.IPAddress.Parse("10.0.0.1"));
        _mockContext.Setup(c => c.Connection).Returns(connection.Object);
        _mockContext.Setup(c => c.Request.Headers).Returns(new HeaderDictionary
        {
            ["X-Forwarded-For"] = new StringValues("203.0.113.1, 198.51.100.2, 10.0.0.1")
        });

        // Act
        var result = _mockContext.Object.GetClientIpAddress();

        // Assert
        Assert.Equal("203.0.113.1", result);
    }

    [Fact]
    public void GetClientIpAddress_HappyPath_NoIpAddress_ReturnsUnknown()
    {
        // Arrange
        var connection = new Mock<ConnectionInfo>();
        connection.Setup(c => c.RemoteIpAddress).Returns((System.Net.IPAddress?)null);
        _mockContext.Setup(c => c.Connection).Returns(connection.Object);
        _mockContext.Setup(c => c.Request.Headers).Returns(new HeaderDictionary());

        // Act
        var result = _mockContext.Object.GetClientIpAddress();

        // Assert
        Assert.Equal("unknown", result);
    }

    [Fact]
    public void GetClientIpAddress_NullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => HttpContextExtensions.GetClientIpAddress(null!));
    }

    [Fact]
    public void GetHeader_HappyPath_WithExistingHeader_ReturnsHeaderValue()
    {
        // Arrange
        _defaultContext.Request.Headers["X-Custom-Header"] = "test-value";

        // Act
        var result = _defaultContext.GetHeader("X-Custom-Header");

        // Assert
        Assert.Equal("test-value", result);
    }

    [Fact]
    public void GetHeader_NullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => HttpContextExtensions.GetHeader(null!, "header"));
    }

    [Fact]
    public void GetHeader_NullHeaderName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _defaultContext.GetHeader(null!));
    }

    [Fact]
    public void GetAuthorizationToken_HappyPath_WithValidBearerToken_ReturnsToken()
    {
        // Arrange
        _defaultContext.Request.Headers["Authorization"] = "Bearer my-secret-token-123";

        // Act
        var result = _defaultContext.GetAuthorizationToken();

        // Assert
        Assert.Equal("my-secret-token-123", result);
    }

    [Fact]
    public void GetAuthorizationToken_HappyPath_WithMissingAuthorizationHeader_ReturnsNull()
    {
        // Arrange
        // No Authorization header set

        // Act
        var result = _defaultContext.GetAuthorizationToken();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetAuthorizationToken_NullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => HttpContextExtensions.GetAuthorizationToken(null!));
    }

    [Fact]
    public void GetRequestId_HappyPath_WithXRequestIdHeader_ReturnsHeaderValue()
    {
        // Arrange
        _defaultContext.Request.Headers["X-Request-ID"] = "request-123";

        // Act
        var result = _defaultContext.GetRequestId();

        // Assert
        Assert.Equal("request-123", result);
    }

    [Fact]
    public void GetRequestId_HappyPath_WithoutXRequestIdHeader_ReturnsTraceIdentifier()
    {
        // Arrange
        // No X-Request-ID header set

        // Act
        var result = _defaultContext.GetRequestId();

        // Assert
        Assert.Equal(_defaultContext.TraceIdentifier, result);
    }

    [Fact]
    public void GetRequestId_NullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => HttpContextExtensions.GetRequestId(null!));
    }

    [Fact]
    public void IsGrpcRequest_HappyPath_WithGrpcContentType_ReturnsTrue()
    {
        // Arrange
        _defaultContext.Request.ContentType = "application/grpc";

        // Act
        var result = _defaultContext.IsGrpcRequest();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsGrpcRequest_HappyPath_WithoutGrpcContentType_ReturnsFalse()
    {
        // Arrange
        _defaultContext.Request.ContentType = "application/json";

        // Act
        var result = _defaultContext.IsGrpcRequest();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsGrpcRequest_NullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => HttpContextExtensions.IsGrpcRequest(null!));
    }

    [Fact]
    public void IsGrpcWebRequest_HappyPath_WithGrpcWebContentType_ReturnsTrue()
    {
        // Arrange
        _defaultContext.Request.ContentType = "application/grpc-web";

        // Act
        var result = _defaultContext.IsGrpcWebRequest();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsGrpcWebRequest_HappyPath_WithGrpcContentType_ReturnsFalse()
    {
        // Arrange
        _defaultContext.Request.ContentType = "application/grpc";

        // Act
        var result = _defaultContext.IsGrpcWebRequest();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsGrpcWebRequest_NullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => HttpContextExtensions.IsGrpcWebRequest(null!));
    }
}
