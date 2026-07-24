#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Events.EventHandlers;

/// <summary>
/// Event handler for gateway configuration updates.
/// Logs configuration changes for audit trails.
/// </summary>
public class ConfigurationUpdatedEventHandler : EventHandlerBase<ConfigurationUpdatedEvent>, IEventHandler<ConfigurationUpdatedEvent>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationUpdatedEventHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    public ConfigurationUpdatedEventHandler(ILogger<ConfigurationUpdatedEventHandler> logger)
        : base(logger)
    {
    }

    /// <summary>
    /// Handles configuration update events by logging the changes.
    /// </summary>
    /// <param name="@event">The configuration update event.</param>
    /// <exception cref="ArgumentNullException">Thrown when the event is null.</exception>
    public async Task HandleAsync(ConfigurationUpdatedEvent @event)
    {
        ValidateEvent(@event);

        var changesSummary = string.Join(", ",
            @event.Changes.Select(c => $"{c.Key}={c.Value}"));

        SafeLog(
            LogLevel.Information,
            "Configuration updated - Changes: {Changes}, OccurredAt: {OccurredAt}, CorrelationId: {@CorrelationId}",
            changesSummary, @event.OccurredAt, @event.CorrelationId);

        // Audit log configuration changes
        foreach (var change in @event.Changes)
        {
            SafeLog(
                LogLevel.Debug,
                "Config change - Key: {Key}, Value: {Value}",
                change.Key, change.Value);
        }

        // In a real scenario, this would:
        // 1. Reload affected services
        // 2. Validate configuration consistency
        // 3. Trigger configuration sync across cluster

        await Task.CompletedTask;
    }
}
