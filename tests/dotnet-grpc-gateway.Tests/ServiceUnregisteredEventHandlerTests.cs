using System;
using System.Threading.Tasks;
using DotNetGrpcGateway.Events;
using DotNetGrpcGateway.Events.EventHandlers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetGrpcGateway.Tests;

/// <summary>
/// Tests for ServiceUnregisteredEventHandler to ensure error paths are handled gracefully.
/// </summary>
public class ServiceUnregisteredEventHandlerTests
{
    private readonly Mock<ILogger<ServiceUnregisteredEventHandler>> _mockLogger = new();

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ServiceUnregisteredEventHandler(null!));
    }

    [Fact]
    public async Task HandleAsync_WithNullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        var handler = new ServiceUnregisteredEventHandler(_mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.HandleAsync(null!));
    }

    [Fact]
    public async Task HandleAsync_WithDefaultServiceUnregisteredEvent_DoesNotThrow()
    {
        // Arrange
        var handler = new ServiceUnregisteredEventHandler(_mockLogger.Object);
        var @event = new ServiceUnregisteredEvent(); // Default values

        // Act & Assert - should not throw even with default/null values
        var exception = await Record.ExceptionAsync(() => handler.HandleAsync(@event));
        Assert.Null(exception);
    }

    [Fact]
    public async Task HandleAsync_WithNullServiceName_DoesNotThrow()
    {
        // Arrange
        var handler = new ServiceUnregisteredEventHandler(_mockLogger.Object);
        var @event = new ServiceUnregisteredEvent(123, null!); // null ServiceName

        // Act & Assert - should handle null ServiceName gracefully
        var exception = await Record.ExceptionAsync(() => handler.HandleAsync(@event));
        Assert.Null(exception);
    }

    [Fact]
    public async Task HandleAsync_WithZeroServiceId_DoesNotThrow()
    {
        // Arrange
        var handler = new ServiceUnregisteredEventHandler(_mockLogger.Object);
        var @event = new ServiceUnregisteredEvent(0, "TestService"); // Zero ServiceId

        // Act & Assert - should handle zero ServiceId gracefully
        var exception = await Record.ExceptionAsync(() => handler.HandleAsync(@event));
        Assert.Null(exception);
    }

    [Fact]
    public async Task HandleAsync_WithNegativeServiceId_DoesNotThrow()
    {
        // Arrange
        var handler = new ServiceUnregisteredEventHandler(_mockLogger.Object);
        var @event = new ServiceUnregisteredEvent(-1, "TestService"); // Negative ServiceId

        // Act & Assert - should handle negative ServiceId gracefully
        var exception = await Record.ExceptionAsync(() => handler.HandleAsync(@event));
        Assert.Null(exception);
    }

    [Fact]
    public async Task HandleAsync_LogsWarningWithEventDetails()
    {
        // Arrange
        var handler = new ServiceUnregisteredEventHandler(_mockLogger.Object);
        var @event = new ServiceUnregisteredEvent(42, "MyService");

        // Act
        await handler.HandleAsync(@event);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Service unregistered")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNullServiceName_LogsWarningWithoutNullReference()
    {
        // Arrange
        var handler = new ServiceUnregisteredEventHandler(_mockLogger.Object);
        var @event = new ServiceUnregisteredEvent(123, null!);

        // Act
        await handler.HandleAsync(@event);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ServiceId") && v.ToString()!.Contains("123")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyServiceName_LogsWarning()
    {
        // Arrange
        var handler = new ServiceUnregisteredEventHandler(_mockLogger.Object);
        var @event = new ServiceUnregisteredEvent(456, string.Empty);

        // Act
        await handler.HandleAsync(@event);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Service unregistered")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithCorrelationId_LogsWithCorrelationContext()
    {
        // Arrange
        var handler = new ServiceUnregisteredEventHandler(_mockLogger.Object);
        var @event = new ServiceUnregisteredEvent(789, "AnotherService")
        {
            CorrelationId = "test-correlation-123"
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
    public async Task HandleAsync_MultipleTimes_HandlesIdempotently()
    {
        // Arrange
        var handler = new ServiceUnregisteredEventHandler(_mockLogger.Object);
        var @event = new ServiceUnregisteredEvent(999, "IdempotentService");

        // Act - call multiple times (at-least-once delivery semantics)
        await handler.HandleAsync(@event);
        await handler.HandleAsync(@event);
        await handler.HandleAsync(@event);

        // Assert - should not throw and should log each time
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Service unregistered")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task HandleAsync_WithVeryLargeServiceId_DoesNotThrow()
    {
        // Arrange
        var handler = new ServiceUnregisteredEventHandler(_mockLogger.Object);
        var @event = new ServiceUnregisteredEvent(int.MaxValue, "LargeService");

        // Act & Assert - should handle large ServiceId
        var exception = await Record.ExceptionAsync(() => handler.HandleAsync(@event));
        Assert.Null(exception);
    }

    [Fact]
    public async Task HandleAsync_WithSpecialCharactersInServiceName_DoesNotThrow()
    {
        // Arrange
        var handler = new ServiceUnregisteredEventHandler(_mockLogger.Object);
        var @event = new ServiceUnregisteredEvent(111, "Service-With_Special.Chars");

        // Act & Assert - should handle special characters in service name
        var exception = await Record.ExceptionAsync(() => handler.HandleAsync(@event));
        Assert.Null(exception);
    }
}
