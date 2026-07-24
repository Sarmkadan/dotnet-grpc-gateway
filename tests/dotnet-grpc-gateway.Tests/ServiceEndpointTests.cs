namespace DotNetGrpcGateway.Tests;

using Xunit;
using System;
using DotNetGrpcGateway.Domain;

public class ServiceEndpointTests
{
    [Fact]
    public void Constructor_WithValidParameters_SetsAllProperties()
    {
        // Arrange
        var endpoint = new ServiceEndpoint
        {
            Id = 1,
            ServiceId = 10,
            Host = "localhost",
            Port = 5001,
            UseTls = true,
            IsHealthy = false,
            Draining = true,
            Weight = 5,
            TotalRequestsHandled = 100,
            FailedRequestsCount = 10,
            AverageResponseTimeMs = 42.5,
            ActiveConnections = 3
        };

        // Assert
        Assert.Equal(1, endpoint.Id);
        Assert.Equal(10, endpoint.ServiceId);
        Assert.Equal("localhost", endpoint.Host);
        Assert.Equal(5001, endpoint.Port);
        Assert.True(endpoint.UseTls);
        Assert.False(endpoint.IsHealthy);
        Assert.True(endpoint.Draining);
        Assert.Equal(5, endpoint.Weight);
        Assert.Equal(100, endpoint.TotalRequestsHandled);
        Assert.Equal(10, endpoint.FailedRequestsCount);
        Assert.Equal(42.5, endpoint.AverageResponseTimeMs);
        Assert.Equal(3, endpoint.ActiveConnections);
    }

    [Fact]
    public void DefaultConstructor_SetsDefaultValues()
    {
        // Arrange & Act
        var endpoint = new ServiceEndpoint();

        // Assert
        Assert.Equal(0, endpoint.Id);
        Assert.Equal(0, endpoint.ServiceId);
        Assert.Equal("http://:5000", endpoint.GetUri());
        Assert.False(endpoint.UseTls);
        Assert.True(endpoint.IsHealthy);
        Assert.False(endpoint.Draining);
        Assert.Equal(1, endpoint.Weight);
        Assert.Equal(0, endpoint.TotalRequestsHandled);
        Assert.Equal(0, endpoint.FailedRequestsCount);
        Assert.Equal(0, endpoint.ActiveConnections);
        Assert.True(endpoint.RegisteredAt <= DateTime.UtcNow);
        Assert.True(endpoint.LastUsedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void GetUri_WithHttp_ReturnsCorrectUri()
    {
        // Arrange
        var endpoint = new ServiceEndpoint
        {
            Host = "api.example.com",
            Port = 8080,
            UseTls = false
        };

        // Act
        var uri = endpoint.GetUri();

        // Assert
        Assert.Equal("http://api.example.com:8080", uri);
    }

    [Fact]
    public void GetUri_WithHttps_ReturnsCorrectUri()
    {
        // Arrange
        var endpoint = new ServiceEndpoint
        {
            Host = "secure.example.com",
            Port = 443,
            UseTls = true
        };

        // Act
        var uri = endpoint.GetUri();

        // Assert
        Assert.Equal("https://secure.example.com:443", uri);
    }


    [Fact]
    public void RecordRequest_WithSuccess_IncrementsCounters()
    {
        // Arrange
        var endpoint = new ServiceEndpoint
        {
            TotalRequestsHandled = 10,
            FailedRequestsCount = 2,
            AverageResponseTimeMs = 50.0
        };

        // Act
        endpoint.RecordRequest(125.5, true);

        // Assert
        Assert.Equal(11, endpoint.TotalRequestsHandled);
        Assert.Equal(2, endpoint.FailedRequestsCount);
        Assert.Equal(56.863636363636367, endpoint.AverageResponseTimeMs);
        Assert.True(endpoint.LastUsedAt >= DateTime.UtcNow.AddSeconds(-1));
    }

    [Fact]
    public void RecordRequest_WithFailure_IncrementsCounters()
    {
        // Arrange
        var endpoint = new ServiceEndpoint
        {
            TotalRequestsHandled = 10,
            FailedRequestsCount = 2,
            AverageResponseTimeMs = 50.0
        };

        // Act
        endpoint.RecordRequest(75.25, false);

        // Assert
        Assert.Equal(11, endpoint.TotalRequestsHandled);
        Assert.Equal(3, endpoint.FailedRequestsCount);
        Assert.Equal(52.295454545454547, endpoint.AverageResponseTimeMs);
        Assert.True(endpoint.LastUsedAt >= DateTime.UtcNow.AddSeconds(-1));
    }

    [Fact]
    public void RecordRequest_WithZeroRequests_CalculatesAverageCorrectly()
    {
        // Arrange
        var endpoint = new ServiceEndpoint();

        // Act
        endpoint.RecordRequest(100.0, true);

        // Assert
        Assert.Equal(1, endpoint.TotalRequestsHandled);
        Assert.Equal(0, endpoint.FailedRequestsCount);
        Assert.Equal(100.0, endpoint.AverageResponseTimeMs);
    }

    [Fact]
    public void RecordRequest_UpdatesLastUsedAt()
    {
        // Arrange
        var endpoint = new ServiceEndpoint
        {
            LastUsedAt = DateTime.UtcNow.AddHours(-1)
        };
        var before = endpoint.LastUsedAt;

        // Act
        endpoint.RecordRequest(50.0, true);

        // Assert
        Assert.True(endpoint.LastUsedAt > before);
        Assert.True(endpoint.LastUsedAt >= DateTime.UtcNow.AddSeconds(-1));
    }

    [Fact]
    public void Weight_DefaultValue_Is1()
    {
        // Arrange & Act
        var endpoint = new ServiceEndpoint();

        // Assert
        Assert.Equal(1, endpoint.Weight);
    }

    [Fact]
    public void IsHealthy_DefaultValue_IsTrue()
    {
        // Arrange & Act
        var endpoint = new ServiceEndpoint();

        // Assert
        Assert.True(endpoint.IsHealthy);
    }

    [Fact]
    public void Draining_DefaultValue_IsFalse()
    {
        // Arrange & Act
        var endpoint = new ServiceEndpoint();

        // Assert
        Assert.False(endpoint.Draining);
    }
}