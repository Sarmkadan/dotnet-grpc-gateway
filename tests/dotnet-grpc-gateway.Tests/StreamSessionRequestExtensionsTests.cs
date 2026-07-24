namespace DotNetGrpcGateway.Tests;

using Xunit;
using System;
using System.Collections.Generic;
using DotNetGrpcGateway.Streaming;

public class StreamSessionRequestExtensionsTests
{
    [Fact]
    public void HasValidServiceAndMethod_ReturnsTrue_WhenServiceAndMethodAreValid()
    {
        // Arrange
        var request = new StreamSessionRequest
        {
            ServiceName = "Service1",
            MethodName = "Method1"
        };

        // Act
        var result = request.HasValidServiceAndMethod();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasValidServiceAndMethod_ReturnsFalse_WhenServiceIsInvalid()
    {
        // Arrange
        var request = new StreamSessionRequest
        {
            ServiceName = null,
            MethodName = "Method1"
        };

        // Act
        var result = request.HasValidServiceAndMethod();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasValidServiceAndMethod_ReturnsFalse_WhenMethodIsInvalid()
    {
        // Arrange
        var request = new StreamSessionRequest
        {
            ServiceName = "Service1",
            MethodName = null
        };

        // Act
        var result = request.HasValidServiceAndMethod();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetValidHeaders_ReturnsEmptyDictionary_WhenHeadersAreNull()
    {
        // Arrange
        var request = new StreamSessionRequest
        {
            Headers = null
        };

        // Act
        var result = request.GetValidHeaders();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetValidHeaders_ReturnsEmptyDictionary_WhenHeadersAreEmpty()
    {
        // Arrange
        var request = new StreamSessionRequest
        {
            Headers = new Dictionary<string, string>()
        };

        // Act
        var result = request.GetValidHeaders();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetValidHeaders_ReturnsValidHeaders_WhenHeadersAreValid()
    {
        // Arrange
        var request = new StreamSessionRequest
        {
            Headers = new Dictionary<string, string>
            {
                {"Header1", "Value1"},
                {"Header2", "Value2"}
            }
        };

        // Act
        var result = request.GetValidHeaders();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("Header1", result.Keys);
        Assert.Contains("Header2", result.Keys);
    }

    [Fact]
    public void ToRequestSummary_ReturnsSummaryString_WhenRequestIsValid()
    {
        // Arrange
        var request = new StreamSessionRequest
        {
            ServiceName = "Service1",
            MethodName = "Method1",
            RoutePath = "Route1",
            Headers = new Dictionary<string, string>
            {
                {"Header1", "Value1"},
                {"Header2", "Value2"}
            }
        };

        // Act
        var result = request.ToRequestSummary();

        // Assert
        Assert.Equal("Service: Service1, Method: Method1, Route: Route1, Headers: 2", result);
    }

    [Fact]
    public void ToRequestSummary_ReturnsSummaryString_WhenRequestHasNoHeaders()
    {
        // Arrange
        var request = new StreamSessionRequest
        {
            ServiceName = "Service1",
            MethodName = "Method1",
            RoutePath = "Route1"
        };

        // Act
        var result = request.ToRequestSummary();

        // Assert
        Assert.Equal("Service: Service1, Method: Method1, Route: Route1, Headers: 0", result);
    }
}
