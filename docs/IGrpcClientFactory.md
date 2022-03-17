# IGrpcClientFactory

`IGrpcClientFactory` is the core abstraction for creating and managing gRPC client instances within the `dotnet-grpc-gateway` project. It decouples client construction from consumption, enabling centralized configuration of HTTP channels, automatic service invocation with gateway-compatible metadata propagation, and built-in client lifetime management. Implementations handle channel pooling, address resolution, and serialization concerns so that callers only need to specify the service contract and optional per-call parameters.

## API

### GrpcClientFactory

The concrete implementation of the factory. It is instantiated with configuration that governs how underlying `HttpClient` instances are created, how service addresses are resolved, and how client proxies are cached. The constructor is not defined on the interface itself but is the entry point for obtaining a working factory instance.

### HttpClient CreateHttpClient(string name)

Creates or retrieves a configured `HttpClient` instance identified by the logical name. The returned client is pre-configured with the base address, timeout, and message handler pipeline associated with that name in the factory’s configuration.

- **Parameters:**
  - `name` — The logical name used to look up the client configuration (e.g., from an `IHttpClientFactory` integration or internal registry).
- **Returns:** An `HttpClient` ready for use with gRPC invocations.
- **Throws:** `InvalidOperationException` if no configuration exists for the given name. May propagate underlying handler creation exceptions.

### HttpClient CreateHttpClient(Uri baseAddress)

Creates or retrieves an `HttpClient` configured for a specific base address. This overload bypasses named configurations and directly targets the supplied URI.

- **Parameters:**
  - `baseAddress` — The absolute base URI that will be used for all requests made by the returned client.
- **Returns:** An `HttpClient` bound to the specified base address.
- **Throws:** `ArgumentNullException` if `baseAddress` is null. `UriFormatException` if the URI is not absolute.

### async Task\<T\> InvokeAsync\<T\>(string clientName, Func\<HttpClient, Task\<T\>\> invocation, CancellationToken cancellationToken = default)

Executes a strongly-typed gRPC call using a client obtained by name. The factory resolves the `HttpClient`, invokes the caller-supplied delegate, and returns the deserialized response. Metadata propagation and error translation are handled internally.

- **Type Parameters:**
  - `T` — The response type, typically a generated gRPC message class.
- **Parameters:**
  - `clientName` — The logical name of the client configuration to use.
  - `invocation` — A delegate that receives the pre-configured `HttpClient` and performs the actual gRPC call (e.g., `client.SayHelloAsync(request)`).
  - `cancellationToken` — Optional token to cancel the underlying HTTP request.
- **Returns:** The response object of type `T`.
- **Throws:** `InvalidOperationException` when the client name is not configured. `RpcException` or derived exceptions on gRPC-level failures. `TaskCanceledException` if the token is signaled.

### async Task\<Stream\> InvokeStreamingAsync(string clientName, Func\<HttpClient, Task\<Stream\>\> invocation, CancellationToken cancellationToken = default)

Executes a server-streaming or client-streaming gRPC call and returns the raw response stream. The caller is responsible for reading and deserializing the stream contents.

- **Parameters:**
  - `clientName` — The logical name of the client configuration to use.
  - `invocation` — A delegate that receives the `HttpClient` and returns a `Task<Stream>` representing the streaming response.
  - `cancellationToken` — Optional token to cancel the streaming call.
- **Returns:** A readable `Stream` containing the streaming response data.
- **Throws:** `InvalidOperationException` when the client name is not configured. `RpcException` on call failures. `TaskCanceledException` if cancellation is requested.

### void ClearClientCache()

Removes all cached `HttpClient` instances and client proxies held by the factory. Subsequent calls will trigger fresh client creation based on the current configuration. This is useful when configuration changes at runtime (e.g., endpoint rotation) need to take effect without restarting the process.

- **Parameters:** None.
- **Returns:** Void.
- **Throws:** No exceptions are thrown directly. Any in-flight operations using previously cached clients are unaffected; they continue to use the old instances until completion.

## Usage

### Example 1: Basic unary call with named client

```csharp
// Assume factory is injected via DI
IGrpcClientFactory factory = serviceProvider.GetRequiredService<IGrpcClientFactory>();

// Define the invocation delegate using the generated gRPC client
var request = new HelloRequest { Name = "World" };
HelloReply reply = await factory.InvokeAsync(
    "greeter-service",
    async httpClient =>
    {
        var greeterClient = new Greeter.GreeterClient(httpClient);
        return await greeterClient.SayHelloAsync(request);
    });

Console.WriteLine(reply.Message);
```

### Example 2: Server-streaming call with stream processing

```csharp
IGrpcClientFactory factory = serviceProvider.GetRequiredService<IGrpcClientFactory>();

var request = new SubscribeRequest { Topic = "events" };
Stream responseStream = await factory.InvokeStreamingAsync(
    "pubsub-service",
    async httpClient =>
    {
        var subscriberClient = new Subscriber.SubscriberClient(httpClient);
        var call = subscriberClient.Subscribe(request);
        // Return the response stream for external processing
        return await Task.FromResult(call.ResponseStream);
    });

// Read and deserialize messages from the stream
using var reader = new StreamReader(responseStream);
while (!reader.EndOfStream)
{
    var line = await reader.ReadLineAsync();
    Console.WriteLine($"Received: {line}");
}
```

## Notes

- **Client caching and lifetime:** The factory internally caches `HttpClient` instances keyed by name or base address. Calling `ClearClientCache` discards all cached entries, but existing references held by callers remain valid until they go out of scope. This means cache clearing is safe to call concurrently with in-flight requests.
- **Thread safety:** `InvokeAsync`, `InvokeStreamingAsync`, and `ClearClientCache` are thread-safe. Multiple threads may call these methods concurrently without corrupting internal state. The underlying `HttpClient` instances are themselves thread-safe for concurrent requests.
- **Delegate execution:** The invocation delegate passed to `InvokeAsync` and `InvokeStreamingAsync` executes on the caller’s synchronization context. Long-running or CPU-intensive work inside the delegate may block the calling thread; offload such work if necessary.
- **Stream disposal:** The `Stream` returned by `InvokeStreamingAsync` is owned by the caller. It must be disposed after use to release network resources. Failure to dispose may lead to connection leaks.
- **Configuration changes:** After updating the factory’s backing configuration (e.g., changing a base address), call `ClearClientCache` to ensure new clients reflect the changes. Without clearing, cached clients continue using the old configuration indefinitely.
- **Error propagation:** gRPC status codes are surfaced as `RpcException` instances. Transient network failures may be wrapped in `HttpRequestException` before being translated. Callers should implement retry policies at the invocation level rather than relying on the factory to handle retries internally.
