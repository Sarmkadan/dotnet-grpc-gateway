using System;
using System.Threading.Tasks;
using DotNetGrpcGateway.Events;
using DotNetGrpcGateway.Events.EventHandlers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetGrpcGateway.Tests;

/// <summary>
/// Tests for RequestThrottledEventHandler to ensure error paths are handled gracefully.
/// </summary>
public class RequestThrottledEventHandlerTests
{
    private readonly Mock<ILogger<RequestThrottledEventHandler>> _mockLogger = new();

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new RequestThrottledEventHandler(null!));
    }

    [Fact]
    public async Task HandleAsync_WithNullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        var handler = new RequestThrottledEventHandler(_mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.HandleAsync(null!));
    }

    [Fact]
    public async Task HandleAsync_WithDefaultRequestThrottledEvent_DoesNotThrow()
    {
        // Arrange
        var handler = new RequestThrottledEventHandler(_mockLogger.Object);
        var @event = new RequestThrottledEvent(); // Default values

        // Act & Assert - should not throw even with default/null values
        var exception = await Record.ExceptionAsync(() => handler.HandleAsync(@event));
        Assert.Null(exception);
    }

    [Fact]
    public async Task HandleAsync_WithNullClientIp_DoesNotThrow()
    {
        // Arrange
        var handler = new RequestThrottledEventHandler(_mockLogger.Object);
        var @event = new RequestThrottledEvent(null!, "/api/test", 100); // null ClientIp

        // Act & Assert - should handle null ClientIp gracefully
        var exception = await Record.ExceptionAsync(() => handler.HandleAsync(@event));
        Assert.Null(exception);
    }

    [Fact]
    public async Task HandleAsync_WithNullRequestPath_DoesNotThrow()
    {
        // Arrange
        var handler = new RequestThrottledEventHandler(_mockLogger.Object);
        var @event = new RequestThrottledEvent("192.168.1.1", null!, 100); // null RequestPath

        // Act & Assert - should handle null RequestPath gracefully
        var exception = await Record.ExceptionAsync(() => handler.HandleAsync(@event));
        Assert.Null(exception);
    }

    [Fact]
    public async Task HandleAsync_WithZeroRateLimit_DoesNotThrow()
    {
        // Arrange
        var handler = new RequestThrottledEventHandler(_mockLogger.Object);
        var @event = new RequestThrottledEvent("192.168.1.1", "/api/test", 0); // Zero rate limit

        // Act & Assert - should handle zero rate limit gracefully
        var exception = await Record.ExceptionAsync(() => handler.HandleAsync(@event));
        Assert.Null(exception);
    }

    [Fact]
    public async Task HandleAsync_WithNegativeRateLimit_DoesNotThrow()
    {
        // Arrange
        var handler = new RequestThrottledEventHandler(_mockLogger.Object);
        var @event = new RequestThrottledEvent("192.168.1.1", "/api/test", -10); // Negative rate limit

        // Act & Assert - should handle negative rate limit gracefully
        var exception = await Record.ExceptionAsync(() => handler.HandleAsync(@event));
        Assert.Null(exception);
    }

    [Fact]
    public async Task HandleAsync_LogsWarningWithEventDetails()
    {
        // Arrange
        var handler = new RequestThrottledEventHandler(_mockLogger.Object);
        var @event = new RequestThrottledEvent("10.0.0.1", "/api/users", 100);

        // Act
        await handler.HandleAsync(@event);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request throttled")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNullClientIp_LogsWarningWithoutNullReference()
    {
        // Arrange
        var handler = new RequestThrottledEventHandler(_mockLogger.Object);
        var @event = new RequestThrottledEvent(null!, "/api/test", 100);

        // Act
        await handler.HandleAsync(@event);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ClientIp") && v.ToString()!.Contains("null")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyRequestPath_LogsWarning()
    {
        // Arrange
        var handler = new RequestThrottledEventHandler(_mockLogger.Object);
        var @event = new RequestThrottledEvent("10.0.0.1", string.Empty, 100);

        // Act
        await handler.HandleAsync(@event);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request throttled")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithCorrelationId_LogsWithCorrelationContext()
    {
        // Arrange
        var handler = new RequestThrottledEventHandler(_mockLogger.Object);
        var @event = new RequestThrottledEvent("10.0.0.1", "/api/test", 100)
        {
            CorrelationId = "test-correlation-456"
        };

        // Act
        await handler.HandleAsync(@event);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("correlation")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_MultipleTimesFromSameIp_HandlesIdempotently()
    {
        // Arrange
        var handler = new RequestThrottledEventHandler(_mockLogger.Object);
        var @event = new RequestThrottledEvent("192.168.1.100", "/api/endpoint", 50);

        // Act - call multiple times (at-least-once delivery semantics)
        await handler.HandleAsync(@event);
        await handler.HandleAsync(@event);
        await handler.HandleAsync(@event);

        // Assert - should not throw and should log each time
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request throttled")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task HandleAsync_MultipleTimesFromDifferentIps_TracksEachIpSeparately()
    {
        // Arrange
        var handler = new RequestThrottledEventHandler(_mockLogger.Object);
        var @event1 = new RequestThrottledEvent("192.168.1.1", "/api/endpoint", 50);
        var @event2 = new RequestThrottledEvent("192.168.1.2", "/api/endpoint", 50);
        var @event3 = new RequestThrottledEvent("192.168.1.1", "/api/endpoint", 50); // Same IP as event1

        // Act
        await handler.HandleAsync(@event1);
        await handler.HandleAsync(@event2);
        await handler.HandleAsync(@event3); // Should increment count for IP 192.168.1.1

        // Assert - should log all three events
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request throttled")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task HandleAsync_WithVeryLargeRateLimit_DoesNotThrow()
    {
        // Arrange
        var handler = new RequestThrottledEventHandler(_mockLogger.Object);
        var @event = new RequestThrottledEvent("10.0.0.1", "/api/test", int.MaxValue);

        // Act & Assert - should handle large rate limit
        var exception = await Record.ExceptionAsync(() => handler.HandleAsync(@event));
        Assert.Null(exception);
    }

    [Fact]
    public async Task HandleAsync_WithSpecialCharactersInRequestPath_DoesNotThrow()
    {
        // Arrange
        var handler = new RequestThrottledEventHandler(_mockLogger.Object);
        var @event = new RequestThrottledEvent("10.0.0.1", "/api/v1/users/123?expand=profile&fields=id,name", 100);

        // Act & Assert - should handle special characters in request path
        var exception = await Record.ExceptionAsync(() => handler.HandleAsync(@event));
        Assert.Null(exception);
    }

    [Fact]
    public async Task HandleAsync_WithIpv6Address_DoesNotThrow()
    {
        // Arrange
        var handler = new RequestThrottledEventHandler(_mockLogger.Object);
        var @event = new RequestThrottledEvent("2001:0db8:85a3:0000:0000:8a2e:0370:7334", "/api/test", 100);

        // Act & Assert - should handle IPv6 addresses
        var exception = await Record.ExceptionAsync(() => handler.HandleAsync(@event));
        Assert.Null(exception);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyStringClientIp_DoesNotThrow()
    {
        // Arrange
        var handler = new RequestThrottledEventHandler(_mockLogger.Object);
        var @event = new RequestThrottledEvent(string.Empty, "/api/test", 100);

        // Act & Assert - should handle empty string ClientIp
        var exception = await Record.ExceptionAsync(() => handler.HandleAsync(@event));
        Assert.Null(exception);
    }
}
