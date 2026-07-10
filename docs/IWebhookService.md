# IWebhookService
The `IWebhookService` interface is designed to provide a standardized way of interacting with webhooks, allowing for the sending and tracking of webhook deliveries. It offers a simple and efficient API for integrating webhooks into applications, enabling features such as real-time notifications and event-driven processing.

## API
* `bool Success`: Indicates whether the webhook delivery was successful.
* `int? StatusCode`: The HTTP status code returned by the webhook endpoint, if applicable.
* `string? Message`: A message associated with the webhook delivery, if any.
* `DateTime DeliveredAt`: The timestamp when the webhook was delivered.
* `long DurationMs`: The duration of the webhook delivery in milliseconds.
* `string Url`: The URL of the webhook endpoint.
* `WebhookService`: The instance of the webhook service.
* `async Task<WebhookResult> SendWebhookAsync`: Sends a webhook asynchronously. Returns a `WebhookResult` object containing the outcome of the delivery. May throw exceptions related to network errors or webhook endpoint issues.
* `async Task<List<WebhookDelivery>> GetDeliveryHistoryAsync`: Retrieves the history of webhook deliveries asynchronously. Returns a list of `WebhookDelivery` objects representing past deliveries. May throw exceptions related to data retrieval or storage issues.
* `string? ErrorMessage`: An error message associated with the webhook delivery, if any.

## Usage
The following examples demonstrate how to use the `IWebhookService` interface:
```csharp
// Example 1: Sending a webhook
var webhookService = new WebhookService();
var webhookResult = await webhookService.SendWebhookAsync("https://example.com/webhook", new { message = "Hello, world!" });
if (webhookResult.Success)
{
    Console.WriteLine("Webhook sent successfully.");
}
else
{
    Console.WriteLine($"Error sending webhook: {webhookResult.ErrorMessage}");
}

// Example 2: Retrieving delivery history
var deliveryHistory = await webhookService.GetDeliveryHistoryAsync();
foreach (var delivery in deliveryHistory)
{
    Console.WriteLine($"Webhook delivered at {delivery.DeliveredAt} with status code {delivery.StatusCode}");
}
```

## Notes
When using the `IWebhookService` interface, consider the following:
* Webhook deliveries are asynchronous, and the `SendWebhookAsync` method may return before the delivery is complete.
* The `GetDeliveryHistoryAsync` method may return a large amount of data, depending on the number of past deliveries.
* The `IWebhookService` interface is designed to be thread-safe, allowing for concurrent access and usage.
* Error handling is crucial when working with webhooks, as network errors or endpoint issues may occur.
* Implementations of the `IWebhookService` interface should ensure proper handling of webhook endpoint URLs, authentication, and data serialization.
