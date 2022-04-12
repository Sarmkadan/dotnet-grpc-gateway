# GatewayExceptionExtensions

Provides a set of extension methods for inspecting and extracting information from gateway‑related exceptions, enabling concise handling of common error conditions such as rate limiting and missing detail payloads.

## API

### `public static bool IsRateLimitExceeded(this GatewayException ex)`
- **Purpose**: Determines whether the exception indicates that a rate‑limit threshold has been exceeded.
- **Parameters**: `ex` – the `GatewayException` instance to evaluate.
- **Return value**: `true` if the exception carries a rate‑limit error code; otherwise `false`.
- **Exceptions**: Throws `ArgumentNullException` if `ex` is `null`.

### `public static bool HasDetails(this GatewayException ex)`
- **Purpose**: Checks whether the exception contains any detail metadata.
- **Parameters**: `ex` – the `GatewayException` instance to inspect.
- **Return value**: `true` when the exception’s `Details` collection is non‑empty; otherwise `false`.
- **Exceptions**: Throws `ArgumentNullException` if `ex` is `null`.

### `public static string GetErrorCodeOrDefault(this GatewayException ex)`
- **Purpose**: Retrieves the error code associated with the exception, falling back to a default string when none is present.
- **Parameters**: `ex` – the `GatewayException` instance to query.
- **Return value**: The error code string if available; otherwise `"UNKNOWN"`.
- **Exceptions**: Throws `ArgumentNullException` if `ex` is `null`.

### `public static int GetHttpStatusCodeOrDefault(this GatewayException ex)`
- **Purpose**: Obtains the HTTP status code mapped from the exception, providing a fallback value when the mapping is unavailable.
- **Parameters**: `ex` – the `GatewayException` instance to evaluate.
- **Return value**: The HTTP status code (e.g., 429 for rate limit) if determinable; otherwise `500`.
- **Exceptions**: Throws `ArgumentNullException` if `ex` is `null`.

## Usage

```csharp
try
{
    await client.SomeRpcAsync(request);
}
catch (GatewayException gex)
{
    if (gex.IsRateLimitExceeded())
    {
        // Apply back‑off or retry logic specific to rate limits.
        Log.Warn("Rate limit exceeded: {Code}", gex.GetErrorCodeOrDefault());
    }
    else if (gex.HasDetails())
    {
        // Process rich error details returned by the service.
        Log.Error("Service error: {Details}", string.Join(", ", gex.Details));
    }
    else
    {
        Log.Error("Unexpected gateway failure: {Status}", gex.GetHttpStatusCodeOrDefault());
    }
}
```

```csharp
public static async Task HandleResponseAsync(CallInvoker invoker)
{
    var response = await invoker.BlockingUnaryCall(
        method: MyService.Methods.GetData,
        request: new GetDataRequest(),
        options: new CallOptions());

    // If the call fails as a GatewayException, surface a user‑friendly message.
}
catch (GatewayException ex) when (ex.IsRateLimitExceeded())
{
    throw new InvalidOperationException(
        "The service is temporarily unavailable due to rate limiting. Please try again later.", ex);
}
```

## Notes

- All extension methods are pure; they do not modify the exception instance and rely only on its read‑only state, making them safe to invoke concurrently from multiple threads.
- Passing a `null` reference results in an `ArgumentNullException`; callers should ensure the exception is non‑null before invoking these helpers.
- The default values returned by `GetErrorCodeOrDefault` (`"UNKNOWN"`) and `GetHttpStatusCodeOrDefault` (`500`) are chosen to represent an unspecified error while still allowing downstream code to distinguish between known and unknown failure modes.
- If the underlying `GatewayException` implementation changes its internal representation of error codes or details, the behavior of these methods will adapt accordingly, but their contracts (return types and null‑checking) remain stable.
