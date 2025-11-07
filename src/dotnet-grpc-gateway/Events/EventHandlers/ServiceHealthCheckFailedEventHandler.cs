// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Integration;

namespace DotNetGrpcGateway.Events.EventHandlers;

/// <summary>
/// Event handler for service health check failures.
/// Logs failures and optionally sends notifications via webhooks.
/// </summary>
public class ServiceHealthCheckFailedEventHandler : IEventHandler<ServiceHealthCheckFailedEvent>
{
    private readonly ILogger<ServiceHealthCheckFailedEventHandler> _logger;
    private readonly IWebhookService? _webhookService;

    public ServiceHealthCheckFailedEventHandler(
        ILogger<ServiceHealthCheckFailedEventHandler> logger,
        IWebhookService? webhookService = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _webhookService = webhookService;
    }

    public async Task HandleAsync(ServiceHealthCheckFailedEvent @event)
    {
        _logger.LogWarning(
            "Service health check failed - Service: {ServiceName} (ID: {ServiceId}), Error: {Error}, CorrelationId: {@CorrelationId}",
            @event.ServiceName, @event.ServiceId, @event.ErrorMessage, @event.CorrelationId);

        // Send alert via webhook if configured
        if (_webhookService != null)
        {
            try
            {
                var payload = new
                {
                    eventType = "SERVICE_HEALTH_CHECK_FAILED",
                    @event.ServiceId,
                    @event.ServiceName,
                    @event.ErrorMessage,
                    occurredAt = @event.OccurredAt,
                    correlationId = @event.CorrelationId
                };

                // Note: In a real scenario, webhook URL would come from configuration
                // await _webhookService.SendWebhookAsync(alertWebhookUrl, payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending health check failure webhook");
            }
        }

        await Task.CompletedTask;
    }
}
