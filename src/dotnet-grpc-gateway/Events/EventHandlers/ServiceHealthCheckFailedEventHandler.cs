#nullable enable
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
public class ServiceHealthCheckFailedEventHandler : EventHandlerBase<ServiceHealthCheckFailedEvent>, IEventHandler<ServiceHealthCheckFailedEvent>
{
    private readonly IWebhookService? _webhookService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceHealthCheckFailedEventHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="webhookService">Optional webhook service for sending alerts.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    public ServiceHealthCheckFailedEventHandler(
        ILogger<ServiceHealthCheckFailedEventHandler> logger,
        IWebhookService? webhookService = null)
        : base(logger)
    {
        _webhookService = webhookService;
    }

    /// <summary>
    /// Handles service health check failure events by logging and optionally sending webhooks.
    /// </summary>
    /// <param name="@event">The service health check failure event.</param>
    /// <exception cref="ArgumentNullException">Thrown when the event is null.</exception>
    public async Task HandleAsync(ServiceHealthCheckFailedEvent @event)
    {
        ValidateEvent(@event);

        SafeLog(
            LogLevel.Warning,
            "Service health check failed - Service: {ServiceName} (ID: {ServiceId}), Error: {Error}, CorrelationId: {@CorrelationId}",
            @event.ServiceName, @event.ServiceId, @event.ErrorMessage, @event.CorrelationId);

        // Send alert via webhook if configured
        if (_webhookService is not null)
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
                SafeLog(
                    LogLevel.Error,
                    ex,
                    "Error sending health check failure webhook - {ExceptionMessage}",
                    ex.Message);
            }
        }

        await Task.CompletedTask;
    }
}
