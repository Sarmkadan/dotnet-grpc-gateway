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
    Task<WebhookResult> SendWebhookAsync(string url, object payload, CancellationToken cancellationToken = default);
    Task<List<WebhookDelivery>> GetDeliveryHistoryAsync(string url);
}

/// <summary>
/// Webhook delivery result information.
/// </summary>
public class WebhookResult
{
    public bool Success { get; set; }
    public int? StatusCode { get; set; }
    public string? Message { get; set; }
    public DateTime DeliveredAt { get; set; }
    public long DurationMs { get; set; }
}

/// <summary>
/// Webhook delivery history record.
/// </summary>
public class WebhookDelivery
{
    public string Url { get; set; } = null!;
    public DateTime DeliveredAt { get; set; }
    public bool Success { get; set; }
    public int? StatusCode { get; set; }
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

    public WebhookService(HttpClient httpClient, ILogger<WebhookService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<WebhookResult> SendWebhookAsync(string url, object payload, CancellationToken cancellationToken = default)
    {
        if (!ValidationUtility.IsValidUri(url))
        {
            _logger.LogWarning("Invalid webhook URL: {Url}", StringUtility.MaskSensitiveData(url));
            return new WebhookResult { Success = false, Message = "Invalid URL" };
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
                if ((int)response.StatusCode < 500)
                {
                    break;
                }

                if (attempt < MaxRetries)
                {
                    var delayMs = (int)Math.Pow(2, attempt - 1) * 1000; // Exponential backoff
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
                    await Task.Delay(1000, cancellationToken);
                    continue;
                }

                result = new WebhookResult
                {
                    Success = false,
                    Message = "Request timeout",
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
                    await Task.Delay(1000, cancellationToken);
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
        if (result != null)
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
            if (_history.Count > 1000)
                _history.RemoveRange(0, 100);
        }

        return result ?? new WebhookResult { Success = false, Message = "Unknown error" };
    }

    public async Task<List<WebhookDelivery>> GetDeliveryHistoryAsync(string url)
    {
        return await Task.FromResult(
            _history.Where(h => h.Url.Equals(url, StringComparison.OrdinalIgnoreCase))
                   .OrderByDescending(h => h.DeliveredAt)
                   .ToList()
        );
    }
}
