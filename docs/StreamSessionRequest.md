# StreamSessionRequest

Represents a single message in a bidirectional gRPC‑gateway stream, encapsulating metadata, payload, and flow‑control information required to forward data between the HTTP client and the gRPC backend.

## API

| Member | Type | Purpose | Return / Value | Throws |
|--------|------|---------|----------------|--------|
| **ServiceName** | `string` | Fully qualified name of the gRPC service being invoked. | The service name as set by the caller. | Setting to `null` may throw `ArgumentNullException` if the implementation requires a non‑null value. |
| **MethodName** | `string` | Name of the method within `ServiceName` to call. | The method name as set by the caller. | Setting to `null` may throw `ArgumentNullException` if the implementation requires a non‑null value. |
| **RoutePath** | `string?` | Optional HTTP route path that triggered the stream (e.g., from a gateway mapping). | The route path or `null` when not applicable. | None; accepting `null` is valid. |
| **Headers** | `Dictionary<string,string>` | Collection of HTTP header key‑value pairs to be forwarded with the message. | The header dictionary; modifications affect the outgoing request. | Setting the property to `null` may throw `ArgumentNullException`. Adding a `null` key or value to the dictionary may throw `ArgumentNullException`. |
| **Payload** | `byte[]` | Binary payload of the message (e.g., protobuf‑encoded request/response). | The byte array containing the message data. | Setting to `null` may throw `ArgumentNullException` if the implementation expects a non‑null buffer. |
| **EndOfStream** | `bool` | Indicates whether this message terminates the stream (no further frames will follow). | `true` if this is the final frame; otherwise `false`. | None. |
| **ContentType** | `string?` | MIME type of the payload (e.g., `application/protobuf`). | The content type string or `null` when unknown. | None; accepting `null` is valid. |
| **InitialWindowSizeBytes** | `int` | Initial flow‑control window size advertised to the peer for this stream. | Window size in bytes; must be non‑negative. | Setting a negative value may throw `ArgumentOutOfRangeException`. |
| **ChannelCapacity** | `int` | Maximum number of bytes the underlying channel can buffer before applying back‑pressure. | Capacity in bytes; must be non‑negative. | Setting a negative value may throw `ArgumentOutOfRangeException`. |
| **WriteTimeout** | `TimeSpan` | Maximum time to wait for a write operation to complete before timing out. | Timeout interval; must be non‑negative. | Setting a negative `TimeSpan` may throw `ArgumentOutOfRangeException`. |
| **InitialSize** | `int` | Starting credit count for the stream’s flow‑control mechanism. | Initial credit value; must be non‑negative. | Setting a negative value may throw `ArgumentOutOfRangeException`. |
| **AvailableCredits** | `int` | Current number of credits available for sending more data. | Credits remaining; must be non‑negative and ≤ `InitialSize` plus any increments. | Setting a negative value may throw `ArgumentOutOfRangeException`. |
| **IsThrottled** | `bool` | Flag indicating whether the stream is currently throttled due to flow‑control limits. | `true` when throttled; otherwise `false`. | None. |
| **PendingBytes** | `int` | Number of bytes that have been written but not yet acknowledged by the peer. | Byte count awaiting acknowledgment; must be non‑negative. | Setting a negative value may throw `ArgumentOutOfRangeException`. |
| **Capacity** | `int` | Total buffer capacity available for the stream (often synonymous with `ChannelCapacity`). | Buffer capacity in bytes; must be non‑negative. | Setting a negative value may throw `ArgumentOutOfRangeException`. |
| **UtilisationPct** | `double` | Percentage of the buffer capacity currently utilized (0.0–100.0). | Utilization ratio; values outside 0‑100 may be clamped or cause an exception depending on implementation. | Setting a value < 0 or > 100 may throw `ArgumentOutOfRangeException`. |
| **SessionId** | `string` | Identifier uniquely associating all frames belonging to the same logical stream session. | The session identifier string. | Setting to `null` may throw `ArgumentNullException` if the implementation requires a non‑null value. |
| **State** | `StreamState` | Current lifecycle state of the stream (e.g., `Open`, `HalfClosed`, `Closed`). | Enum value reflecting the stream’s state. | None. |
| **FramesRead** | `long` | Cumulative count of frames that have been read from the stream since its creation. | Number of frames processed; monotonically increasing. | None. |

## Usage

### Creating a request frame for a unary call

```csharp
var request = new StreamSessionRequest
{
    ServiceName = "example.MyService",
    MethodName  = "UnaryMethod",
    RoutePath   = "/example.MyService/UnaryMethod",
    Headers     = new Dictionary<string, string>
    {
        { "content-type", "application/grpc" },
        { "authorization", "Bearer abc123" }
    },
    Payload          = ProtobufSerializer.Serialize(myRequestMessage),
    EndOfStream      = true,
    ContentType      = "application/grpc",
    InitialWindowSizeBytes = 64 * 1024,
    ChannelCapacity        = 256 * 1024,
    WriteTimeout           = TimeSpan.FromSeconds(30),
    InitialSize            = 100,
    AvailableCredits       = 100,
    IsThrottled = false,
    PendingBytes           = 0,
    Capacity               = 256 * 1024,
    UtilisationPct         = 0.0,
    SessionId              = Guid.NewGuid().ToString(),
    State                  = StreamState.Open,
    FramesRead             = 0
};

// Pass `request` to the gateway's outbound queue for transmission.
await gateway.EnqueueOutboundAsync(request);
```

### Processing an inbound streaming frame

```csharp
// Assume `incoming` is a StreamSessionRequest received from the gRPC backend.
if (incoming.EndOfStream)
{
    // No more data expected; complete the client‑side response.
    await responseWriter.CompleteAsync();
    return;
}

// Forward payload to the HTTP client.
await responseWriter.WriteAsync(incoming.Payload, 0, incoming.Payload.Length);

// Update local flow‑control bookkeeping.
interceptor.ApplyCredits(incoming.AvailableCredits);
if (incoming.IsThrottled)
{
    // Apply back‑pressure to the upstream gRPC call.
    await upstream.PauseReadsAsync();
}
```

## Notes

- **Mutability**: All members are mutable; concurrent reads or writes from multiple threads without external synchronization can lead to race conditions. It is recommended to treat a `StreamSessionRequest` instance as confined to a single thread or to protect access with a lock/`ReaderWriterLockSlim` when shared.
- **Headers dictionary**: The `Headers` property returns the actual dictionary instance. Modifying this dictionary after the request has been queued may affect the message in flight; therefore, alterations should be completed before handing the request to the gateway.
- **Payload nullability**: While the type allows `null`, the gateway implementation typically expects a non‑null byte array for frames that carry data. Supplying `null` for a data frame may cause the underlying transport to treat the frame as a control frame only.
- **Flow‑control fields**: `InitialWindowSizeBytes`, `ChannelCapacity`, `InitialSize`, `AvailableCredits`, `PendingBytes`, `Capacity`, and `UtilisationPct` are interdependent. Setting them to inconsistent values (e.g., `AvailableCredits` > `InitialSize` + increments) may trigger protocol‑level errors; callers should maintain the invariants expected by the gRPC‑gateway flow‑control algorithm.
- **Enum `StreamState`**: The `State` property should be updated only by the gateway’s internal state machine. Arbitrary changes by user code can corrupt session tracking.
- **SessionId uniqueness**: Duplicate `SessionId` values across concurrent streams can cause the gateway to mis‑associate frames, resulting in incorrect demultiplexing. Generating a new GUID per stream, as shown in the usage example, is a safe practice.
- **WriteTimeout**: A `WriteTimeout` of `TimeSpan.Zero` indicates an immediate timeout; negative values are invalid and will throw if validated by the implementation.
