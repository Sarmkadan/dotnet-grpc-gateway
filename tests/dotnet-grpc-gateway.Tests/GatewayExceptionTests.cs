using Xunit;
using DotNetGrpcGateway.Exceptions;

namespace DotNetGrpcGateway.Tests;

public class GatewayExceptionTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        // Arrange and Act
        var exception = new GatewayException("Test message", "TEST_ERROR", 400);

        // Assert
        Assert.Equal("Test message", exception.Message);
        Assert.Equal("TEST_ERROR", exception.ErrorCode);
        Assert.Equal(400, exception.HttpStatusCode);
    }

    [Fact]
    public void Constructor_WithInnerException_SetsProperties()
    {
        // Arrange
        var innerException = new Exception("Inner exception");

        // Act
        var exception = new GatewayException("Test message", innerException, "TEST_ERROR", 400);

        // Assert
        Assert.Equal("Test message", exception.Message);
        Assert.Equal("TEST_ERROR", exception.ErrorCode);
        Assert.Equal(400, exception.HttpStatusCode);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void AddDetail_AddsToDetailsDictionary()
    {
        // Arrange
        var exception = new GatewayException("Test message", "TEST_ERROR", 400);

        // Act
        exception.AddDetail("Test key", "Test value");

        // Assert
        Assert.NotNull(exception.Details);
        Assert.Single(exception.Details);
        Assert.Equal("Test value", exception.Details["Test key"]);
    }

    [Fact]
    public void ServiceNotFoundException_SetsProperties()
    {
        // Arrange and Act
        var exception = new ServiceNotFoundException("Test service");

        // Assert
        Assert.Equal("Service 'Test service' not found", exception.Message);
        Assert.Equal("SERVICE_NOT_FOUND", exception.ErrorCode);
        Assert.Equal(404, exception.HttpStatusCode);
        Assert.NotNull(exception.Details);
        Assert.Single(exception.Details);
        Assert.Equal("Test service", exception.Details["service_name"]);
    }

    [Fact]
    public void ServiceUnavailableException_SetsProperties()
    {
        // Arrange and Act
        var exception = new ServiceUnavailableException("Test service", "Test reason");

        // Assert
        Assert.Equal("Service 'Test service' is unavailable: Test reason", exception.Message);
        Assert.Equal("SERVICE_UNAVAILABLE", exception.ErrorCode);
        Assert.Equal(503, exception.HttpStatusCode);
        Assert.NotNull(exception.Details);
        Assert.Equal(2, exception.Details.Count);
        Assert.Equal("Test service", exception.Details["service_name"]);
        Assert.Equal("Test reason", exception.Details["reason"]);
    }
}
