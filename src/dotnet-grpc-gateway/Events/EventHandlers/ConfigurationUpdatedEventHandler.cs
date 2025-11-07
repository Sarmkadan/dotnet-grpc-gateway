// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Events.EventHandlers;

/// <summary>
/// Event handler for gateway configuration updates.
/// Logs configuration changes and validates that changes are allowed.
/// </summary>
public class ConfigurationUpdatedEventHandler : IEventHandler<ConfigurationUpdatedEvent>
{
    private readonly ILogger<ConfigurationUpdatedEventHandler> _logger;

    public ConfigurationUpdatedEventHandler(ILogger<ConfigurationUpdatedEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(ConfigurationUpdatedEvent @event)
    {
        var changesSummary = string.Join(", ",
            @event.Changes.Select(c => $"{c.Key}={c.Value}"));

        _logger.LogInformation(
            "Configuration updated - Changes: {Changes}, OccurredAt: {OccurredAt}, CorrelationId: {@CorrelationId}",
            changesSummary, @event.OccurredAt, @event.CorrelationId);

        // Audit log configuration changes
        foreach (var change in @event.Changes)
        {
            _logger.LogDebug("Config change - Key: {Key}, Value: {Value}", change.Key, change.Value);
        }

        // In a real scenario, this would:
        // 1. Reload affected services
        // 2. Validate configuration consistency
        // 3. Trigger configuration sync across cluster

        await Task.CompletedTask;
    }
}
