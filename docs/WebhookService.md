# WebhookService

`WebhookService` (`src/dotnet-grpc-gateway/Integration/WebhookService.cs`) sends JSON webhook payloads to external HTTP endpoints. It retries transient failures with a fixed or exponential delay, enforces a per-attempt timeout, and keeps an in-memory delivery history per instance.

Public types in the same file:

- `IWebhookService`: `SendWebhookAsync` and `GetDeliveryHistoryAsync`.
- `WebhookResult`: outcome of a single `SendWebhookAsync` call.
- `WebhookDelivery`: one entry in the delivery history.

See also [IWebhookService](IWebhookService.md).

## Registration

`Program.cs` registers the service as a typed client:

```csharp
services.AddHttpClient<IWebhookService, WebhookService>();
```

Typed clients registered this way are transient, so each resolution of `IWebhookService` gets a new `WebhookService` instance with its own `HttpClient` and its own history list. See [Limitations](#limitations).

The service has no configuration options. Its constants are fixed in code:

| Constant | Value | Meaning |
|----------|-------|---------|
| `MaxRetries` | 3 | Maximum number of attempts per call |
| `TimeoutSeconds` | 10 | Timeout applied to each attempt |
| `ServerErrorStatusCode` | 500 | Responses with this status or higher are retried |
| `RetryDelayMs` | 1000 | Fixed delay after a timeout or exception |
| `BackoffBase` / `BackoffBaseMs` | 2 / 1000 | Base of the exponential delay after a 5xx response |
| `MaxHistoryEntries` | 1000 | History size that triggers trimming |
| `HistoryTrimCount` | 100 | Number of oldest entries removed when trimming |

## Delivery mechanics

`SendWebhookAsync(url, payload, cancellationToken)` runs in this order:

1. `url` and `payload` must be non-null, otherwise `ArgumentNullException` is thrown.
2. The URL is checked with `ValidationUtility.IsValidUri`, which requires an absolute URI. If it fails, the method logs a warning (with the URL masked) and returns `Success = false`, `Message = "Invalid URL"` immediately. Nothing is sent and nothing is added to the history.
3. The payload is sent with `HttpClient.PostAsJsonAsync` in a loop of up to three attempts. Each attempt has its own 10-second timeout, linked to the caller's `cancellationToken`.
4. The outcome of the last attempt is recorded in the history (see [Delivery history](#delivery-history)) and returned.

An attempt succeeds when the response status is 2xx. The response body is not read.

## Retry policy

| Outcome of an attempt | Retried? | Delay before the next attempt |
|-----------------------|----------|-------------------------------|
| 2xx | No | — (delivery succeeds) |
| 3xx or 4xx | No | — (returned as a failure) |
| 5xx | Yes, until attempt 3 | Exponential: 1000 ms after attempt 1, 2000 ms after attempt 2 |
| Timeout (10 s per attempt) | Yes, until attempt 3 | Fixed: 1000 ms |
| Other exception (for example, a network error) | Yes, until attempt 3 | Fixed: 1000 ms |

Notes:

- The backoff formula is `2^(attempt - 1) * 1000 ms`, applied only to 5xx responses. Timeouts and exceptions always wait 1000 ms.
- There is no delay after the third attempt.
- The worst case for one call is three timeouts (30 s) plus two 1-second delays, about 32 seconds. A 5xx-only sequence adds at most 3 seconds of delay on top of three attempts.
- Responses with status below 500 that are not 2xx (such as 404 or 429) are not retried. The 429 case is not special-cased.

### Final result

If all attempts fail, the last attempt determines the returned result:

- A 5xx response returns `Success = false`, `StatusCode` set to that response, and `Message` set to the response's reason phrase.
- A timeout on the last attempt returns `Success = false` with `Message = "Request timeout"`.
- An exception on the last attempt returns `Success = false` with `Message` set to the exception message.

A 4xx response is returned right away with its status code and reason phrase, and no retry takes place.

### Cancellation

The caller's `cancellationToken` is combined with the per-attempt timeout. The two cases behave differently:

- If the caller cancels while an attempt is in flight, the attempt throws `OperationCanceledException`. Unless it was the last attempt, the method then calls `Task.Delay` with the cancelled token, which throws. The exception propagates to the caller, and nothing is added to the history.
- If the caller cancels on the last attempt, the method does not throw. It records and returns a result with `Message = "Request timeout"`.

Callers should treat `OperationCanceledException` as a possible result of `SendWebhookAsync`, not only as a failure value.

## Payload format

The `payload` argument is `object`. It is serialized as-is with `HttpClient.PostAsJsonAsync` and no options. The service does not add an envelope, metadata, headers, or a signature.

Each request has:

- Method: `POST`
- URL: the `url` argument
- `Content-Type: application/json; charset=utf-8`
- Body: the payload serialized with System.Text.Json defaults

With the default options, the serializer:

- Writes property names in camelCase (`ServiceId` becomes `serviceId`).
- Writes `null` values explicitly. They are not omitted.
- Writes `DateTime` values in ISO 8601 form, for example `"1970-01-01T00:00:00Z"`.

Example, using the shape of the payload that `ServiceHealthCheckFailedEventHandler` builds:

```json
{
  "eventType": "SERVICE_HEALTH_CHECK_FAILED",
  "serviceId": 42,
  "serviceName": "orders",
  "errorMessage": "Connection refused",
  "occurredAt": "2026-10-25T10:15:30Z",
  "correlationId": "7b1e2d4a-0c3f-4f7e-9a6e-2f1d3c4b5a69"
}
```

The handler builds this payload, but the `SendWebhookAsync` call is commented out in the current code. Nothing in the current code path sends it.

## Result object

`WebhookResult` is returned by `SendWebhookAsync`:

| Property | Meaning |
|----------|---------|
| `Success` | `true` only for a 2xx response |
| `StatusCode` | Status of the last response received, or `null` if no response was received |
| `Message` | Reason phrase of the last response, `"Request timeout"`, the exception message, or `"Invalid URL"` |
| `DeliveredAt` | Completion time in UTC |
| `DurationMs` | Total time from the start of the call to the end, including retries and delays |

`DurationMs` is measured from the start of the call, so it includes time spent on earlier attempts and backoff delays. It is not the duration of the successful attempt.

## Delivery history

On success or final failure, `SendWebhookAsync` adds a `WebhookDelivery` to the instance's history. The entry stores `Url`, `DeliveredAt`, `Success`, `StatusCode`, and `ErrorMessage` (copied from `Message`).

- When the history exceeds 1000 entries, the 100 oldest are removed.
- Invalid-URL rejections and calls that end with a thrown exception are not recorded.
- `GetDeliveryHistoryAsync(url)` returns the entries whose `Url` matches, compared case-insensitively with `StringComparison.OrdinalIgnoreCase`, sorted by `DeliveredAt` descending. It returns a new list, so changes to the list do not affect the history.

## Logging

| Event | Level | URL in log |
|-------|-------|------------|
| Invalid URL | Warning | Masked |
| Success | Information | Masked |
| 5xx, before a retry | Warning | Not logged (status and delay only) |
| Timeout | Warning | Not logged (duration and attempt only) |
| Exception | Error | Masked |

`StringUtility.MaskSensitiveData` keeps the first four characters and replaces the rest with `*`.

## Limitations

- **History is per instance.** Because `AddHttpClient` registers the service as transient, `GetDeliveryHistoryAsync` only sees deliveries sent through the same instance. A caller that resolves a new `IWebhookService` will not see earlier deliveries.
- **History is not thread-safe.** It is a plain `List<WebhookDelivery>` with no locking. Concurrent calls on the same instance can race on it. Avoid sharing one instance across concurrent senders until this is changed.
- **History is lost on restart.** It is held in memory only.
- **No authentication or signing.** The service sends no `Authorization` header and no HMAC signature. Receivers that need to verify the source must rely on the URL and a secret that is part of it.
- **Retries are not idempotent-aware.** A 5xx or timeout may still have been processed by the receiver. Receivers should de-duplicate, for example with the `correlationId` in the payload.
