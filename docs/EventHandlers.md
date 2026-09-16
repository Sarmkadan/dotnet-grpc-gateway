# Event Handlers

The event handler pattern in `dotnet-grpc-gateway` provides a decoupled mechanism for responding to system events through the publish-subscribe model. Handlers implement the `IEventHandler<TEvent>` interface to process specific event types asynchronously.

## Pattern Overview

Event handlers follow a consistent structure:
- Inherit from `EventHandlerBase<TEvent>` for common validation and logging
- Implement `IEventHandler<TEvent>` interface for the specific event type
- Use dependency injection for logger and optional services
- Perform null validation and safe logging through base class methods
- Return completed tasks as handlers are primarily for side effects (logging, notifications)

## Base Class: EventHandlerBase

All handlers inherit from `EventHandlerBase<TEvent>` which provides:

### Features
- **Null Validation**: `ValidateEvent(TEvent @event)` method throws `ArgumentNullException` for null events
- **Safe Logging**: `SafeLog` methods with fallback to console if logger fails
- **Logger Access**: Protected `Logger` property for derived classes
- **Exception Handling**: Consistent try/catch patterns around logging operations

### Methods
- `ValidateEvent(TEvent @event)` - Validates event parameter is not null
- `SafeLog(LogLevel logLevel, string message, params object?[] args)` - Safe logging without exceptions
- `SafeLog(LogLevel logLevel, Exception exception, string message, params object?[] args)` - Safe logging with exception context

## Handler Registration

Handlers are automatically discovered and registered by `EventPublisher` through reflection during initialization. The publisher scans for all implementations of `IEventHandler<TEvent>` and maps them to their corresponding event types.

## Event Handler List

### ConfigurationUpdatedEventHandler
- **Processes**: `ConfigurationUpdatedEvent`
- **Purpose**: Logs configuration changes for audit trails
- **Key Actions**:
  - Logs summary of all configuration changes at Information level
  - Logs individual key-value changes at Debug level
  - Provides extension points for reloading services and validating consistency

### ServiceRegisteredEventHandler
- **Processes**: `ServiceRegisteredEvent`
- **Purpose**: Logs service registrations and updates service discovery cache
- **Key Actions**:
  - Logs service registration details (name, host, port, full name) at Information level
  - Extension point for updating service discovery cache or registry

### ServiceUnregisteredEventHandler
- **Processes**: `ServiceUnregisteredEvent`
- **Purpose**: Logs service removals and cleans up associated routes
- **Key Actions**:
  - Logs service unregistration at Warning level
  - Extension points for removing routes, clearing cached data, and notifying monitoring systems

### RouteAddedEventHandler
- **Processes**: `RouteAddedEvent`
- **Purpose**: Logs route additions and optionally triggers cache invalidation
- **Key Actions**:
  - Logs route addition details (ID, pattern, target service) at Information level
  - Extension points for invalidating route matching cache and updating load balancer configuration

### RouteRemovedEventHandler
- **Processes**: `RouteRemovedEvent`
- **Purpose**: Logs route removals and triggers related cleanup operations
- **Key Actions**:
  - Logs route removal details (ID, pattern) at Information level
  - Extension points for invalidating cache, closing connections, and updating load balancer

### ServiceHealthCheckFailedEventHandler
- **Processes**: `ServiceHealthCheckFailedEvent`
- **Purpose**: Logs failures and optionally sends notifications via webhooks
- **Key Actions**:
  - Logs health check failure details at Warning level
  - Sends alert via configured webhook service (when available)
  - Includes error handling for webhook failures with Error level logging

### CircuitBreakerStateChangedEventHandler
- **Processes**: `CircuitBreakerStateChangedEvent`
- **Purpose**: Logs state transitions and can be extended for monitoring/alerting
- **Key Actions**:
  - Logs circuit breaker state changes with descriptive messages at Information level
  - Additional Warning level logging when circuit breaker opens
  - Tracks consecutive failures and open duration for monitoring

### RequestThrottledEventHandler
- **Processes**: `RequestThrottledEvent`
- **Purpose**: Tracks throttled requests and logs excessive throttling patterns
- **Key Actions**:
  - Logs throttling events at Warning level
  - Tracks per-IP throttling counters to detect abuse patterns
  - Logs Error level alerts when same IP exceeds throttling threshold (>5 times)

## Implementation Details

### Thread Safety
- Handlers are designed to be thread-safe for concurrent execution
- State within handlers (like RequestThrottledEventHandler's counter) uses appropriate locking mechanisms
- EventPublisher executes handlers concurrently but safely

### Error Handling
- All handlers inherit safe logging from base class that falls back to console
- Individual handlers may add specific error handling (e.g., webhook failures)
- EventPublisher implements failure policies (ContinueOnFailure, RetryThenContinue, DeadLetterOnFailure)

### Extensibility
- Handlers are designed with clear extension points marked by comments
- New handlers can be added by implementing `IEventHandler<TEvent>` for new event types
- Automatic discovery means no manual registration required

## Usage Example

Creating a new event handler follows this pattern:

```csharp
public class NewEventHandler : EventHandlerBase<NewEvent>, IEventHandler<NewEvent>
{
    private readonly IOtherService _otherService;
    
    public NewEventHandler(ILogger<NewEventHandler> logger, IOtherService otherService)
        : base(logger)
    {
        _otherService = otherService;
    }
    
    public async Task HandleAsync(NewEvent @event)
    {
        ValidateEvent(@event);
        
        SafeLog(LogLevel.Information, "Processing new event: {Property}", @event.SomeProperty);
        
        // Handle the event - typically side effects like logging, notifications, cache updates
        await _otherService.DoSomethingAsync(@event);
        
        await Task.CompletedTask;
    }
}
```

## Convention Summary

1. **Inheritance**: All handlers inherit from `EventHandlerBase<TEvent>`
2. **Interface**: Implement `IEventHandler<TEvent>` for the specific event type
3. **Constructor**: Accept `ILogger<THandler>` and optional dependencies via DI
4. **Validation**: Call `ValidateEvent(@event)` at start of HandleAsync
5. **Logging**: Use `SafeLog` methods from base class for consistent logging
6. **Return**: Return `await Task.CompletedTask` as handlers focus on side effects
7. **Naming**: Handler name matches `{EventName}EventHandler` pattern