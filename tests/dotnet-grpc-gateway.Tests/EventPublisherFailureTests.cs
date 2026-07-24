using System;
using System.Threading.Tasks;
using DotNetGrpcGateway.Events;
using DotNetGrpcGateway.Events.EventHandlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetGrpcGateway.Tests;

/// <summary>
/// Tests for event handler failure semantics to ensure poison events don't kill the pipeline.
/// </summary>
public class EventPublisherFailureTests
{
    private readonly Mock<ILogger<EventPublisher>> _mockLogger = new();
    private readonly Mock<ILogger<FailingServiceRegisteredEventHandler>> _mockFailingLogger = new();
    private readonly Mock<ILogger<SecondServiceRegisteredEventHandler>> _mockSecondLogger = new();

    private class TestServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, object> _services = new();

        public TestServiceProvider(IEventHandler<ServiceRegisteredEvent> handler1, IEventHandler<ServiceRegisteredEvent> handler2)
        {
            _services[typeof(IEventHandler<ServiceRegisteredEvent>)] = handler1;
            _services[typeof(FailingServiceRegisteredEventHandler)] = handler1;
            _services[typeof(SecondServiceRegisteredEventHandler)] = handler2;
        }

        public object? GetService(Type serviceType)
        {
            return _services.TryGetValue(serviceType, out var service) ? service : null;
        }
    }

    [Fact]
    public async Task PublishAsync_FirstHandlerThrows_SecondHandlerStillExecutes()
    {
        // Arrange
        var failingHandler = new FailingServiceRegisteredEventHandler(_mockFailingLogger.Object);
        var secondHandler = new SecondServiceRegisteredEventHandler(_mockSecondLogger.Object);
        var serviceProvider = new TestServiceProvider(failingHandler, secondHandler);
        var publisher = new EventPublisher(serviceProvider, _mockLogger.Object);

        var @event = new ServiceRegisteredEvent(1, "TestService", "Test.Service.V1", "localhost", 50051);

        // Act
        await publisher.PublishAsync(@event);

        // Assert
        // Verify that the second handler was called despite the first throwing
        _mockSecondLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Second handler executed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_HandlerThrows_EventPublishingSucceeds()
    {
        // Arrange
        var failingHandler = new FailingServiceRegisteredEventHandler(_mockFailingLogger.Object);
        var serviceProvider = new TestServiceProvider(failingHandler, failingHandler);
        var publisher = new EventPublisher(serviceProvider, _mockLogger.Object);

        var @event = new ServiceRegisteredEvent(1, "TestService", "Test.Service.V1", "localhost", 50051);

        // Act & Assert - should not throw
        var exception = await Record.ExceptionAsync(() => publisher.PublishAsync(@event));
        Assert.Null(exception);
    }

    [Fact]
    public async Task PublishAsync_AllHandlersThrow_NoExceptionThrown()
    {
        // Arrange
        var failingHandler = new FailingServiceRegisteredEventHandler(_mockFailingLogger.Object);
        var serviceProvider = new TestServiceProvider(failingHandler, failingHandler);
        var publisher = new EventPublisher(serviceProvider, _mockLogger.Object);

        var @event = new ServiceRegisteredEvent(1, "TestService", "Test.Service.V1", "localhost", 50051);

        // Act & Assert - should not throw even when all handlers fail
        var exception = await Record.ExceptionAsync(() => publisher.PublishAsync(@event));
        Assert.Null(exception);
    }

    [Fact]
    public async Task PublishAsync_WithContinueOnFailurePolicy_LogsFailuresButContinues()
    {
        // Arrange
        var failingHandler = new FailingServiceRegisteredEventHandler(_mockFailingLogger.Object);
        var secondHandler = new SecondServiceRegisteredEventHandler(_mockSecondLogger.Object);
        var serviceProvider = new TestServiceProvider(failingHandler, secondHandler);
        var publisher = new EventPublisher(serviceProvider, _mockLogger.Object, EventHandlerFailurePolicy.ContinueOnFailure);

        var @event = new ServiceRegisteredEvent(1, "TestService", "Test.Service.V1", "localhost", 50051);

        // Act
        await publisher.PublishAsync(@event);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("failed handlers")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WithRetryThenContinuePolicy_RetriesFailedHandlers()
    {
        // Arrange
        var failingHandler = new FailingServiceRegisteredEventHandler(_mockFailingLogger.Object);
        var secondHandler = new SecondServiceRegisteredEventHandler(_mockSecondLogger.Object);
        var serviceProvider = new TestServiceProvider(failingHandler, secondHandler);
        var publisher = new EventPublisher(serviceProvider, _mockLogger.Object, EventHandlerFailurePolicy.RetryThenContinue);

        var @event = new ServiceRegisteredEvent(1, "TestService", "Test.Service.V1", "localhost", 50051);

        // Act
        await publisher.PublishAsync(@event);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retrying")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WithDeadLetterPolicy_LogsDeadLetter()
    {
        // Arrange
        var failingHandler = new FailingServiceRegisteredEventHandler(_mockFailingLogger.Object);
        var serviceProvider = new TestServiceProvider(failingHandler, failingHandler);
        var publisher = new EventPublisher(serviceProvider, _mockLogger.Object, EventHandlerFailurePolicy.DeadLetterOnFailure);

        var @event = new ServiceRegisteredEvent(1, "TestService", "Test.Service.V1", "localhost", 50051);

        // Act
        await publisher.PublishAsync(@event);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Dead-lettering")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void GetFailurePolicy_ReturnsConfiguredPolicy()
    {
        // Arrange
        var failingHandler = new FailingServiceRegisteredEventHandler(_mockFailingLogger.Object);
        var serviceProvider = new TestServiceProvider(failingHandler, failingHandler);
        var publisher = new EventPublisher(serviceProvider, _mockLogger.Object, EventHandlerFailurePolicy.RetryThenContinue);

        // Act
        var policy = publisher.GetFailurePolicy();

        // Assert
        Assert.Equal(EventHandlerFailurePolicy.RetryThenContinue, policy);
    }

    [Fact]
    public void SetFailurePolicy_UpdatesPolicy()
    {
        // Arrange
        var failingHandler = new FailingServiceRegisteredEventHandler(_mockFailingLogger.Object);
        var serviceProvider = new TestServiceProvider(failingHandler, failingHandler);
        var publisher = new EventPublisher(serviceProvider, _mockLogger.Object, EventHandlerFailurePolicy.ContinueOnFailure);

        // Act
        publisher.SetFailurePolicy(EventHandlerFailurePolicy.DeadLetterOnFailure);

        // Assert
        var policy = publisher.GetFailurePolicy();
        Assert.Equal(EventHandlerFailurePolicy.DeadLetterOnFailure, policy);
    }

    [Fact]
    public void WithFailurePolicy_ExtensionMethod_ReturnsPublisher()
    {
        // Arrange
        var failingHandler = new FailingServiceRegisteredEventHandler(_mockFailingLogger.Object);
        var serviceProvider = new TestServiceProvider(failingHandler, failingHandler);
        var publisher = new EventPublisher(serviceProvider, _mockLogger.Object);

        // Act
        var result = publisher.WithFailurePolicy(EventHandlerFailurePolicy.RetryThenContinue);

        // Assert
        Assert.Equal(publisher, result);
        Assert.Equal(EventHandlerFailurePolicy.RetryThenContinue, publisher.GetFailurePolicy());
    }

    [Fact]
    public void GetFailurePolicy_ExtensionMethod_ReturnsPolicy()
    {
        // Arrange
        var failingHandler = new FailingServiceRegisteredEventHandler(_mockFailingLogger.Object);
        var serviceProvider = new TestServiceProvider(failingHandler, failingHandler);
        var publisher = new EventPublisher(serviceProvider, _mockLogger.Object, EventHandlerFailurePolicy.DeadLetterOnFailure);

        // Act
        var policy = publisher.GetFailurePolicy();

        // Assert
        Assert.Equal(EventHandlerFailurePolicy.DeadLetterOnFailure, policy);
    }

    [Fact]
    public void GetFailurePolicy_ExtensionMethod_WithNullPublisher_Throws()
    {
        // Arrange
        IEventPublisher? publisher = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => publisher!.GetFailurePolicy());
    }

    [Fact]
    public void WithFailurePolicy_ExtensionMethod_WithNullPublisher_Throws()
    {
        // Arrange
        IEventPublisher? publisher = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => publisher!.WithFailurePolicy(EventHandlerFailurePolicy.ContinueOnFailure));
    }
}

/// <summary>
/// Test handler that always throws an exception.
/// </summary>
public class FailingServiceRegisteredEventHandler : IEventHandler<ServiceRegisteredEvent>
{
    private readonly ILogger<FailingServiceRegisteredEventHandler> _logger;

    public FailingServiceRegisteredEventHandler(ILogger<FailingServiceRegisteredEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(ServiceRegisteredEvent @event)
    {
        _logger.LogInformation("First handler executed - throwing exception");
        throw new InvalidOperationException("Simulated handler failure for testing");
    }
}

/// <summary>
/// Second test handler that should execute even when first handler throws.
/// </summary>
public class SecondServiceRegisteredEventHandler : IEventHandler<ServiceRegisteredEvent>
{
    private readonly ILogger<SecondServiceRegisteredEventHandler> _logger;

    public SecondServiceRegisteredEventHandler(ILogger<SecondServiceRegisteredEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(ServiceRegisteredEvent @event)
    {
        _logger.LogInformation("Second handler executed successfully");
        await Task.CompletedTask;
    }
}
