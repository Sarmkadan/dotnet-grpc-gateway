# ServiceReflectionInfo

`ServiceReflectionInfo` is a data transfer object used in the `dotnet-grpc-gateway` project to encapsulate metadata about a gRPC service that has been reflected from a server. It provides essential details about the service and its methods, enabling clients or tooling to discover and interact with the service dynamically.

## API

### `ServiceId`
- **Purpose**: A unique identifier for the service within the reflection context.
- **Type**: `int`
- **Remarks**: This value is assigned during reflection and remains constant for the lifetime of the `ServiceReflectionInfo` instance.

### `ServiceName`
- **Purpose**: The short name of the service as defined in the `.proto` file.
- **Type**: `string`
- **Remarks**: This is typically the unqualified name (e.g., `"Greeter"`).

### `ServiceFullName`
- **Purpose**: The fully qualified name of the service, including its package path.
- **Type**: `string`
- **Remarks**: Useful for disambiguation when multiple services share the same short name across different packages (e.g., `".example.v1.Greeter"`).

### `Methods`
- **Purpose**: A list of descriptors detailing each method exposed by the service.
- **Type**: `List<ServiceMethodDescriptor>`
- **Remarks**: The list is populated during reflection and is immutable after construction. If reflection fails, this list may be empty.

### `ReflectedAt`
- **Purpose**: The timestamp when the service reflection was performed.
- **Type**: `DateTime`
- **Remarks**: This is set to `DateTime.UtcNow` at the moment of reflection.

### `IsAvailable`
- **Purpose**: Indicates whether the service is currently available for invocation.
- **Type**: `bool`
- **Remarks**: This flag is set based on runtime checks (e.g., server health) and may change over time.

### `ErrorMessage`
- **Purpose**: Contains an error message if reflection failed for this service.
- **Type**: `string?`
- **Remarks**: `null` if reflection succeeded; otherwise, a non-empty string describing the failure reason.

### `Name`
- **Purpose**: Alias for `ServiceName`; provided for consistency with other reflection types.
- **Type**: `string`
- **Remarks**: Identical to `ServiceName` in value and behavior.

### `RequestType`
- **Purpose**: The fully qualified name of the request message type for the service.
- **Type**: `string`
- **Remarks**: Only relevant if the service has a single request type (e.g., for unary methods). Empty if the service uses multiple request types.

### `ResponseType`
- **Purpose**: The fully qualified name of the response message type for the service.
- **Type**: `string`
- **Remarks**: Only relevant if the service has a single response type. Empty if the service uses multiple response types.

### `IsClientStreaming`
- **Purpose**: Indicates whether the service supports client-streaming RPCs.
- **Type**: `bool`
- **Remarks**: Derived from the reflected method descriptors. `false` if no client-streaming methods exist.

### `IsServerStreaming`
- **Purpose**: Indicates whether the service supports server-streaming RPCs.
- **Type**: `bool`
- **Remarks**: Derived from the reflected method descriptors. `false` if no server-streaming methods exist.

## Usage

### Example 1: Discovering a Service
