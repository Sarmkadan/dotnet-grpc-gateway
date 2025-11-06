// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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

    public EventPublisher(IServiceProvider serviceProvider, ILogger<EventPublisher> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        DiscoverAndRegisterHandlers();
    }

    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : GatewayEvent
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        var eventType = typeof(TEvent);

        _logger.LogInformation("Publishing event: {EventType} [{EventId}]", eventType.Name, @event.EventId);

        if (!_subscriptions.TryGetValue(eventType, out var handlerTypes))
        {
            _logger.LogDebug("No handlers registered for event type: {EventType}", eventType.Name);
            return;
        }

        var tasks = new List<Task>();

        foreach (var handlerType in handlerTypes)
        {
            try
            {
                // Get or create handler instance from DI container
                var handler = _serviceProvider.GetService(handlerType);
                if (handler == null)
                {
                    _logger.LogWarning("Could not resolve handler: {HandlerType}", handlerType.Name);
                    continue;
                }

                // Get the HandleAsync method and invoke it
                var method = handlerType.GetMethod("HandleAsync",
                    new[] { typeof(TEvent) });

                if (method != null)
                {
                    var task = (Task?)method.Invoke(handler, new object[] { @event });
                    if (task != null)
                        tasks.Add(task);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invoking handler {HandlerType} for event {EventType}",
                    handlerType.Name, eventType.Name);
            }
        }

        // Execute all handlers concurrently
        try
        {
            await Task.WhenAll(tasks);
            _logger.LogInformation("Event published successfully: {EventType} [{EventId}]", eventType.Name, @event.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event: {EventType} [{EventId}]", eventType.Name, @event.EventId);
            throw;
        }
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

            _logger.LogDebug("Registered event handler: {HandlerType} for {EventType}",
                handlerType.Name, eventType.Name);
        }
    }
}
