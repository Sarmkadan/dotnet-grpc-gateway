# RouteChannelOptions

Configuration options for customizing the behavior of gRPC client channels in the dotnet-grpc-gateway project. These options allow fine-tuning of channel settings such as timeouts, message size limits, TLS behavior, and additional HTTP headers.

## API

### `CallTimeout`
- **Purpose**: Specifies the maximum duration allowed for a single gRPC call before timing out.
- **Type**: `TimeSpan?`
- **Default**: `null` (no timeout enforced)
- **Usage**: Set to a non-negative value to enforce call duration limits. Exceeding this duration results in a `RpcException` with status `DeadlineExceeded`.

### `MaxReceiveMessageSize`
- **Purpose**: Limits the maximum size (in bytes) of messages the client will accept from the server.
- **Type**: `int?`
- **Default**: `null` (no limit enforced)
- **Usage**: Set to a positive integer to prevent excessively large payloads. Exceeding this size results in a `RpcException` with status `ResourceExhausted`.

### `MaxSendMessageSize`
- **Purpose**: Limits the maximum size (in bytes) of messages the client will send to the server.
- **Type**: `int?`
- **Default**: `null` (no limit enforced)
- **Usage**: Set to a positive integer to cap outgoing message sizes. Exceeding this size results in a `RpcException` with status `InvalidArgument`.

### `AdditionalHeaders`
- **Purpose**: Provides a dictionary of custom HTTP headers to include in gRPC requests.
- **Type**: `Dictionary<string, string>`
- **Default**: Empty dictionary
- **Usage**: Add key-value pairs to inject headers (e.g., authentication tokens). Headers are case-insensitive and applied to all requests.

### `SkipTlsVerification`
- **Purpose**: Disables TLS certificate validation for the channel.
- **Type**: `bool`
- **Default**: `false`
- **Usage**: Set to `true` to bypass certificate checks (e.g., for development with self-signed certificates). **Warning**: Disabling validation exposes the channel to man-in-the-middle attacks.

### `TlsTargetName`
- **Purpose**: Overrides the target name used for TLS certificate validation.
- **Type**: `string?`
- **Default**: `null` (uses the hostname from the channel address)
- **Usage**: Set to a specific name (e.g., a Subject Alternative Name) when the server's certificate does not match the hostname. Useful in load-balanced or proxy scenarios.
