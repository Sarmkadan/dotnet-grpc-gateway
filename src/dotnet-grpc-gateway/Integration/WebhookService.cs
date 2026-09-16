#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net.Http.Json;
using DotNetGrpcGateway.Utilities;

namespace DotNetGrpcGateway.Integration;

/// <summary>
/// Service for sending webhooks to external endpoints.
/// Handles retries, timeout management, and failure tracking.
/// </summary>
public interface IWebhookService
{
    /// <summary>
    /// Sends a webhook payload to the specified URL.
    /// </summary>
    /// <param name="url">The URL to which the webhook is sent.</param>
    /// <param name="payload">The payload to send.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation and contains the delivery result.</returns>
    Task<WebhookResult> SendWebhookAsync(string url, object payload, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the delivery history for the specified webhook URL.
    /// </summary>
    /// <param name="url">The webhook URL whose delivery history is retrieved.</param>
    /// <returns>A task that represents the asynchronous operation and contains the delivery records.</returns>
    Task<List<WebhookDelivery>> GetDeliveryHistoryAsync(string url);
}

/// <summary>
/// Webhook delivery result information.
/// </summary>
public class WebhookResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the webhook was delivered successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status code returned by the endpoint, if available.
    /// </summary>
    public int? StatusCode { get; set; }

    /// <summary>
    /// Gets or sets a message describing the delivery result.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the date and time at which the delivery completed.
    /// </summary>
    public DateTime DeliveredAt { get; set; }

    /// <summary>
    /// Gets or sets the delivery duration, in milliseconds.
    /// </summary>
    public long DurationMs { get; set; }
}

/// <summary>
/// Webhook delivery history record.
/// </summary>
public class WebhookDelivery
{
    /// <summary>
    /// Gets or sets the webhook URL.
    /// </summary>
    public string Url { get; set; } = null!;

    /// <summary>
    /// Gets or sets the date and time at which the delivery completed.
    /// </summary>
    public DateTime DeliveredAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the webhook was delivered successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status code returned by the endpoint, if available.
    /// </summary>
    public int? StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the error message associated with the delivery, if any.
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Implementation of webhook service with retry logic.
/// </summary>
public class WebhookService : IWebhookService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebhookService> _logger;
    private readonly List<WebhookDelivery> _history = new();
    private const int MaxRetries = 3;
    private const int TimeoutSeconds = 10;
    private const int ServerErrorStatusCode = 500;
    private const int RetryDelayMs = 1000;
    private const int BackoffBase = 2;
    private const int BackoffBaseMs = 1000;
    private const int MaxHistoryEntries = 1000;
    private const int HistoryTrimCount = 100;
    private const string InvalidUrlMessage = "Invalid URL";
    private const string RequestTimeoutMessage = "Request timeout";
    private const string UnknownErrorMessage = "Unknown error";

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to send webhook requests.</param>
    /// <param name="logger">The logger used to record webhook delivery activity.</param>
    public WebhookService(HttpClient httpClient, ILogger<WebhookService> logger)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(logger);

        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<WebhookResult> SendWebhookAsync(string url, object payload, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(payload);

        if (!ValidationUtility.IsValidUri(url))
        {
            _logger.LogWarning("Invalid webhook URL: {Url}", StringUtility.MaskSensitiveData(url));
            return new WebhookResult { Success = false, Message = InvalidUrlMessage };
        }

        var startTime = DateTime.UtcNow;
        int attempt = 0;
        WebhookResult? result = null;

        while (attempt < MaxRetries)
        {
            attempt++;

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(TimeoutSeconds));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);

                var response = await _httpClient.PostAsJsonAsync(url, payload, linkedCts.Token);
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                result = new WebhookResult
                {
                    Success = response.IsSuccessStatusCode,
                    StatusCode = (int)response.StatusCode,
                    Message = response.ReasonPhrase,
                    DeliveredAt = DateTime.UtcNow,
                    DurationMs = (long)duration
                };

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Webhook delivered successfully to {Url} (attempt {Attempt}, {DurationMs}ms)",
                        StringUtility.MaskSensitiveData(url), attempt, duration);
                    break;
                }

                // Retry on server errors (5xx) but not client errors (4xx)
                if ((int)response.StatusCode < ServerErrorStatusCode)
                {
                    break;
                }

                if (attempt < MaxRetries)
                {
                    var delayMs = (int)Math.Pow(BackoffBase, attempt - 1) * BackoffBaseMs; // Exponential backoff
                    _logger.LogWarning("Webhook delivery failed with {StatusCode}, retrying in {DelayMs}ms",
                        response.StatusCode, delayMs);
                    await Task.Delay(delayMs, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.LogWarning("Webhook request timed out after {DurationMs}ms on attempt {Attempt}",
                    duration, attempt);

                if (attempt < MaxRetries)
                {
                    await Task.Delay(RetryDelayMs, cancellationToken);
                    continue;
                }

                result = new WebhookResult
                {
                    Success = false,
                    Message = RequestTimeoutMessage,
                    DeliveredAt = DateTime.UtcNow,
                    DurationMs = (long)duration
                };
                break;
            }
            catch (Exception ex)
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.LogError(ex, "Error sending webhook to {Url} on attempt {Attempt}",
                    StringUtility.MaskSensitiveData(url), attempt);

                if (attempt < MaxRetries)
                {
                    await Task.Delay(RetryDelayMs, cancellationToken);
                    continue;
                }

                result = new WebhookResult
                {
                    Success = false,
                    Message = ex.Message,
                    DeliveredAt = DateTime.UtcNow,
                    DurationMs = (long)duration
                };
                break;
            }
        }

        // Record delivery in history
        if (result is not null)
        {
            _history.Add(new WebhookDelivery
            {
                Url = url,
                DeliveredAt = result.DeliveredAt,
                Success = result.Success,
                StatusCode = result.StatusCode,
                ErrorMessage = result.Message
            });

            // Keep only recent history (last 1000 deliveries)
            if (_history.Count > MaxHistoryEntries)
                _history.RemoveRange(0, HistoryTrimCount);
        }

        return result ?? new WebhookResult { Success = false, Message = UnknownErrorMessage };
    }

    /// <inheritdoc/>
    public async Task<List<WebhookDelivery>> GetDeliveryHistoryAsync(string url)
    {
        ArgumentNullException.ThrowIfNull(url);

        return await Task.FromResult(
            _history.Where(h => h.Url.Equals(url, StringComparison.OrdinalIgnoreCase))
                   .OrderByDescending(h => h.DeliveredAt)
                   .ToList()
        );
    }
}
