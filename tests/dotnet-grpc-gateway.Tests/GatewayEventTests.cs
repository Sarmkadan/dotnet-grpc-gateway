using Xunit;
using DotNetGrpcGateway.Events;

namespace DotNetGrpcGateway.Tests;

public class GatewayEventTests
{
    [Fact]
    public void ServiceRegisteredEvent_DefaultConstructor_InitializesBaseProperties()
    {
        // Act
        var evt = new ServiceRegisteredEvent();

        // Assert
        Assert.NotNull(evt.EventId);
        Assert.NotEmpty(evt.EventId);
        Assert.True(DateTime.UtcNow.Subtract(evt.OccurredAt).TotalSeconds < 1);
        Assert.Null(evt.CorrelationId);
        Assert.Null(evt.CausedBy);
    }

    [Fact]
    public void ServiceRegisteredEvent_ParameterizedConstructor_SetsSpecificProperties()
    {
        // Arrange
        int serviceId = 101;
        string serviceName = "TestService";
        string serviceFullName = "Test.Service.V1";
        string host = "grpc.example.com";
        int port = 50051;

        // Act
        var evt = new ServiceRegisteredEvent(serviceId, serviceName, serviceFullName, host, port);

        // Assert
        Assert.Equal(serviceId, evt.ServiceId);
        Assert.Equal(serviceName, evt.ServiceName);
        Assert.Equal(serviceFullName, evt.ServiceFullName);
        Assert.Equal(host, evt.Host);
        Assert.Equal(port, evt.Port);
    }

    [Fact]
    public void GatewayEvent_SetCorrelationIdAndCausedBy_UpdatesProperties()
    {
        // Arrange
        var evt = new ServiceRegisteredEvent();
        string correlationId = "correlation-123";
        string causedBy = "system-trigger";

        // Act
        evt.CorrelationId = correlationId;
        evt.CausedBy = causedBy;

        // Assert
        Assert.Equal(correlationId, evt.CorrelationId);
        Assert.Equal(causedBy, evt.CausedBy);
    }

    [Fact]
    public void ConfigurationUpdatedEvent_DefaultConstructor_InitializesEmptyDictionary()
    {
        // Act
        var evt = new ConfigurationUpdatedEvent();

        // Assert
        Assert.NotNull(evt.Changes);
        Assert.Empty(evt.Changes);
    }

    [Fact]
    public void ConfigurationUpdatedEvent_ParameterizedConstructor_SetsChanges()
    {
        // Arrange
        var changes = new Dictionary<string, object?>
        {
            { "key1", "value1" },
            { "key2", 100 }
        };

        // Act
        var evt = new ConfigurationUpdatedEvent(changes);

        // Assert
        Assert.Equal(2, evt.Changes.Count);
        Assert.Equal("value1", evt.Changes["key1"]);
        Assert.Equal(100, evt.Changes["key2"]);
    }

    [Fact]
    public void RequestThrottledEvent_Constructor_SetsProperties()
    {
        // Arrange
        string ip = "192.168.1.1";
        string path = "/api/v1/resource";
        int limit = 100;

        // Act
        var evt = new RequestThrottledEvent(ip, path, limit);

        // Assert
        Assert.Equal(ip, evt.ClientIp);
        Assert.Equal(path, evt.RequestPath);
        Assert.Equal(limit, evt.RateLimitPerWindow);
    }
}
