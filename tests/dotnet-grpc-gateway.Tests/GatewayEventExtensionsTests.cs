using Xunit;
using DotNetGrpcGateway.Events;
using FluentAssertions;

namespace DotNetGrpcGateway.Tests;

public class GatewayEventExtensionsTests
{
    [Fact]
    public void MaybeServiceRelated_ServiceRegisteredEvent_ReturnsTrue()
    {
        // Arrange
        var evt = new ServiceRegisteredEvent(1, "TestService", "Test.Service.V1", "localhost", 50051);

        // Act
        var result = evt.MaybeServiceRelated();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void MaybeServiceRelated_ServiceUnregisteredEvent_ReturnsTrue()
    {
        // Arrange
        var evt = new ServiceUnregisteredEvent(1, "TestService");

        // Act
        var result = evt.MaybeServiceRelated();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void MaybeServiceRelated_OtherEventType_ReturnsFalse()
    {
        // Arrange
        var evt = new RouteAddedEvent(1, "/api/test", 2);

        // Act
        var result = evt.MaybeServiceRelated();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void MaybeServiceRelated_NullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        GatewayEvent? evt = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => evt!.MaybeServiceRelated());
    }

    [Fact]
    public void GetServiceNameIfAvailable_ServiceRegisteredEvent_ReturnsServiceName()
    {
        // Arrange
        var serviceName = "MyService";
        var evt = new ServiceRegisteredEvent(1, serviceName, "My.Service.V1", "localhost", 50051);

        // Act
        var result = evt.GetServiceNameIfAvailable();

        // Assert
        result.Should().Be(serviceName);
    }

    [Fact]
    public void GetServiceNameIfAvailable_ServiceUnregisteredEvent_ReturnsServiceName()
    {
        // Arrange
        var serviceName = "MyService";
        var evt = new ServiceUnregisteredEvent(1, serviceName);

        // Act
        var result = evt.GetServiceNameIfAvailable();

        // Assert
        result.Should().Be(serviceName);
    }

    [Fact]
    public void GetServiceNameIfAvailable_NonServiceEvent_ReturnsNull()
    {
        // Arrange
        var evt = new RouteAddedEvent(1, "/api/test", 2);

        // Act
        var result = evt.GetServiceNameIfAvailable();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetServiceNameIfAvailable_NullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        GatewayEvent? evt = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => evt!.GetServiceNameIfAvailable());
    }

    [Fact]
    public void ToEventSummary_ServiceRegisteredEvent_ReturnsCorrectFormat()
    {
        // Arrange
        var serviceName = "TestService";
        var evt = new ServiceRegisteredEvent(1, serviceName, "Test.Service.V1", "localhost", 50051);
        evt.CorrelationId = "corr-123";

        // Act
        var result = evt.ToEventSummary();

        // Assert
        result.Should().Contain("ServiceRegisteredEvent");
        result.Should().Contain(evt.EventId);
        result.Should().Contain(evt.OccurredAt.ToString("O"));
        result.Should().Contain("corr-123");
    }

    [Fact]
    public void ToEventSummary_ServiceUnregisteredEvent_ReturnsCorrectFormat()
    {
        // Arrange
        var evt = new ServiceUnregisteredEvent(1, "TestService");
        evt.CorrelationId = "corr-456";

        // Act
        var result = evt.ToEventSummary();

        // Assert
        result.Should().Contain("ServiceUnregisteredEvent");
        result.Should().Contain(evt.EventId);
        result.Should().Contain(evt.OccurredAt.ToString("O"));
        result.Should().Contain("corr-456");
    }

    [Fact]
    public void ToEventSummary_EventWithoutCorrelationId_ReturnsNoneForCorrelation()
    {
        // Arrange
        var evt = new RouteAddedEvent(1, "/api/test", 2);

        // Act
        var result = evt.ToEventSummary();

        // Assert
        result.Should().Contain("RouteAddedEvent");
        result.Should().Contain("Correlation: none");
    }

    [Fact]
    public void ToEventSummary_NullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        GatewayEvent? evt = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => evt!.ToEventSummary());
    }

    [Fact]
    public void MaybeServiceRelated_ServiceHealthCheckFailedEvent_ReturnsFalse()
    {
        // Arrange
        var evt = new ServiceHealthCheckFailedEvent(1, "TestService", "Health check failed");

        // Act
        var result = evt.MaybeServiceRelated();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ToEventSummary_ConfigurationUpdatedEvent_ReturnsCorrectFormat()
    {
        // Arrange
        var changes = new Dictionary<string, object?> { { "key1", "value1" } };
        var evt = new ConfigurationUpdatedEvent(changes);
        evt.CorrelationId = "config-corr";

        // Act
        var result = evt.ToEventSummary();

        // Assert
        result.Should().Contain("ConfigurationUpdatedEvent");
        result.Should().Contain(evt.EventId);
    }

    [Fact]
    public void ToEventSummary_RequestThrottledEvent_ReturnsCorrectFormat()
    {
        // Arrange
        var evt = new RequestThrottledEvent("192.168.1.1", "/api/resource", 100);
        evt.CorrelationId = "throttle-corr";

        // Act
        var result = evt.ToEventSummary();

        // Assert
        result.Should().Contain("RequestThrottledEvent");
        result.Should().Contain(evt.EventId);
    }
}