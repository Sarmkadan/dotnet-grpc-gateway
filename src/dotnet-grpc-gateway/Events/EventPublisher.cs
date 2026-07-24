#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Collections.Concurrent;
using System.Reflection;

namespace DotNetGrpcGateway.Events;

/// <summary>
/// In-memory event publisher implementing pub-sub pattern.
/// Manages subscriptions and routes events to appropriate handlers asynchronously.
/// </summary>
public class EventPublisher : IEventPublisher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventPublisher> _logger;
    private readonly ConcurrentDictionary<Type, List<Type>> _subscriptions = new();
    private EventHandlerFailurePolicy _failurePolicy;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventPublisher"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    /// <param name="logger">The logger for diagnostic messages.</param>
    /// <param name="failurePolicy">Optional failure policy for handling handler exceptions. Defaults to ContinueOnFailure.</param>
    /// <exception cref="ArgumentNullException">Thrown if serviceProvider or logger is null.</exception>
    public EventPublisher(
        IServiceProvider serviceProvider,
        ILogger<EventPublisher> logger,
        EventHandlerFailurePolicy? failurePolicy = null)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _failurePolicy = failurePolicy ?? EventHandlerFailurePolicy.ContinueOnFailure;

        DiscoverAndRegisterHandlers();
    }

    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : GatewayEvent
    {
        ArgumentNullException.ThrowIfNull(@event);

        var eventType = typeof(TEvent);

        _logger.LogInformation("Publishing event: {EventType} [{EventId}]", eventType.Name, @event.EventId);

        if (!_subscriptions.TryGetValue(eventType, out var handlerTypes))
        {
            _logger.LogDebug("No handlers registered for event type: {EventType}", eventType.Name);
            return;
        }

        var failedHandlers = new List<(Type HandlerType, Exception Exception)>();
        var tasks = new List<Task>();

        foreach (var handlerType in handlerTypes)
        {
            try
            {
                // Get or create handler instance from DI container
                var handler = _serviceProvider.GetService(handlerType);
                if (handler is null)
                {
                    _logger.LogWarning("Could not resolve handler: {HandlerType}", handlerType.Name);
                    continue;
                }

                // Get the HandleAsync method and invoke it
                var method = handlerType.GetMethod(
                    "HandleAsync",
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new[] { typeof(TEvent) },
                    null);

                if (method is not null)
                {
                    var task = (Task?)method.Invoke(handler, new object[] { @event });
                    if (task is not null)
                    {
                        tasks.Add(HandleHandlerTaskAsync(handlerType, task, failedHandlers));
                    }
                }
            }
            catch (Exception ex) when (LogHandlerException(handlerType, eventType, ex))
            {
                // Exception is already logged by LogHandlerException
            }
        }

        // Execute all handlers concurrently
        try
        {
            await Task.WhenAll(tasks);

            if (failedHandlers.Count > 0)
            {
                HandleFailedHandlers(@event, failedHandlers);
            }
            else
            {
                _logger.LogInformation("Event published successfully: {EventType} [{EventId}]", eventType.Name, @event.EventId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event: {EventType} [{EventId}]", eventType.Name, @event.EventId);
            throw;
        }
    }

    /// <summary>
    /// Wraps handler execution to capture failures without breaking the pipeline.
    /// </summary>
    private async Task HandleHandlerTaskAsync(Type handlerType, Task task, List<(Type, Exception)> failedHandlers)
    {
        try
        {
            await task;
        }
        catch (Exception ex)
        {
            failedHandlers.Add((handlerType, ex));
            // Exception is logged in the calling method
        }
    }

    /// <summary>
    /// Logs handler exceptions and returns true to continue execution.
    /// </summary>
    private bool LogHandlerException(Type handlerType, Type eventType, Exception ex)
    {
        _logger.LogError(
            ex,
            "Handler failed: {HandlerType} for event {EventType} [{EventId}] - {FailureMessage}",
            handlerType.Name,
            eventType.Name,
            ex.Message);
        return true;
    }

    /// <summary>
    /// Handles failed handlers based on the configured failure policy.
    /// </summary>
    private void HandleFailedHandlers(GatewayEvent @event, List<(Type HandlerType, Exception Exception)> failedHandlers)
    {
        switch (_failurePolicy)
        {
            case EventHandlerFailurePolicy.ContinueOnFailure:
                _logger.LogWarning(
                    "Event published with {FailedHandlerCount} failed handlers: {EventType} [{EventId}]",
                    failedHandlers.Count,
                    @event.GetType().Name,
                    @event.EventId);
                break;

            case EventHandlerFailurePolicy.RetryThenContinue:
                RetryFailedHandlers(@event, failedHandlers);
                break;

            case EventHandlerFailurePolicy.DeadLetterOnFailure:
                DeadLetterEvent(@event, failedHandlers);
                break;
        }
    }

    /// <summary>
    /// Retries failed handlers once before continuing.
    /// </summary>
    private void RetryFailedHandlers(GatewayEvent @event, List<(Type HandlerType, Exception Exception)> failedHandlers)
    {
        _logger.LogInformation("Retrying {FailedHandlerCount} failed handlers for event {EventType}", failedHandlers.Count, @event.GetType().Name);

        var retryTasks = new List<Task>();
        var remainingFailed = new List<(Type HandlerType, Exception Exception)>();

        foreach (var (handlerType, exception) in failedHandlers)
        {
            try
            {
                var handler = _serviceProvider.GetService(handlerType);
                if (handler is null)
                {
                    _logger.LogWarning("Could not resolve handler during retry: {HandlerType}", handlerType.Name);
                    continue;
                }

                var method = handlerType.GetMethod(
                    "HandleAsync",
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new[] { @event.GetType() },
                    null);

                if (method is not null)
                {
                    var task = (Task?)method.Invoke(handler, new object[] { @event });
                    if (task is not null)
                    {
                        retryTasks.Add(HandleHandlerTaskAsync(handlerType, task, remainingFailed));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Retry failed for handler {HandlerType} on event {EventType}: {FailureMessage}",
                    handlerType.Name,
                    @event.GetType().Name,
                    ex.Message);
                remainingFailed.Add((handlerType, ex));
            }
        }

        Task.WhenAll(retryTasks).GetAwaiter().GetResult();

        if (remainingFailed.Count > 0)
        {
            _logger.LogWarning(
                "Event published after retry with {RemainingFailedCount} still-failing handlers: {EventType} [{EventId}]",
                remainingFailed.Count,
                @event.GetType().Name,
                @event.EventId);
        }
        else
        {
            _logger.LogInformation(
                "Event published successfully after retry: {EventType} [{EventId}]",
                @event.GetType().Name,
                @event.EventId);
        }
    }

    /// <summary>
    /// Routes failed events to a dead-letter queue/topic for later processing.
    /// </summary>
    private void DeadLetterEvent(GatewayEvent @event, List<(Type HandlerType, Exception Exception)> failedHandlers)
    {
        _logger.LogError(
            "Dead-lettering event with {FailedHandlerCount} failed handlers: {EventType} [{EventId}]",
            failedHandlers.Count,
            @event.GetType().Name,
            @event.EventId);

        // In a real implementation, this would publish to a dead-letter queue/topic
        // For now, we just log the failures
        foreach (var (handlerType, exception) in failedHandlers)
        {
            _logger.LogError(
                exception,
                "Dead-lettered handler failure - {HandlerType} on {EventType}: {FailureMessage}",
                handlerType.Name,
                @event.GetType().Name,
                exception.Message);
        }
    }

    /// <summary>
    /// Gets the current event handler failure policy.
    /// </summary>
    /// <returns>The failure policy.</returns>
    public EventHandlerFailurePolicy GetFailurePolicy()
    {
        return _failurePolicy;
    }

    /// <summary>
    /// Sets the event handler failure policy.
    /// </summary>
    /// <param name="policy">The failure policy to set.</param>
    public void SetFailurePolicy(EventHandlerFailurePolicy policy)
    {
        _failurePolicy = policy;
        _logger.LogInformation("Event handler failure policy set to: {Policy}", policy);
    }

    private void DiscoverAndRegisterHandlers()
    {
        // Find all event handler implementations using reflection
        var handlerInterfaceType = typeof(IEventHandler<>);
        var assembly = Assembly.GetExecutingAssembly();

        var handlerTypes = assembly.GetTypes()
        .Where(t => !t.IsAbstract && !t.IsInterface &&
            t.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == handlerInterfaceType));

        foreach (var handlerType in handlerTypes)
        {
            // Get the event type from IEventHandler<TEvent>
            var eventHandlerInterface = handlerType.GetInterfaces()
            .First(i => i.IsGenericType &&
                i.GetGenericTypeDefinition() == handlerInterfaceType);

            var eventType = eventHandlerInterface.GetGenericArguments()[0];

            _subscriptions.AddOrUpdate(eventType,
            new List<Type> { handlerType },
            (_, list) =>
            {
                if (!list.Contains(handlerType))
                    list.Add(handlerType);
                return list;
            });

            _logger.LogDebug("Registered event handler: {HandlerType} for {EventType}", handlerType.Name, eventType.Name);
        }
    }
}

/// <summary>
/// Defines the failure handling policy for event handlers.
/// </summary>
public enum EventHandlerFailurePolicy
{
    /// <summary>
    /// Continue processing other handlers even if some fail.
    /// Logs errors but does not throw or affect the event publishing outcome.
    /// </summary>
    ContinueOnFailure,

    /// <summary>
    /// Retry failed handlers once, then continue with remaining failures logged.
    /// </summary>
    RetryThenContinue,

    /// <summary>
    /// Route events with handler failures to a dead-letter queue/topic.
    /// </summary>
    DeadLetterOnFailure
}