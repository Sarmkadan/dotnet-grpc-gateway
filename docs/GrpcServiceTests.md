# GrpcServiceTests

Unit tests for verifying gRPC service endpoint URI generation and request metric tracking functionality in the `dotnet-grpc-gateway` project. This class tests the behavior of endpoint URI construction based on TLS configuration and validates the metrics collection system for request performance and failure tracking.

## API

### `GetEndpointUri_TlsEnabled_ReturnsHttpsScheme()`

Verifies that when TLS is enabled, the endpoint URI scheme is set to `https`.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: No exceptions

### `GetEndpointUri_TlsDisabled_ReturnsHttpScheme()`

Verifies that when TLS is disabled, the endpoint URI scheme is set to `http`.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: No exceptions

### `RecordRequestMetric_MultipleRequests_MaintainsRunningAverage()`

Ensures that recording multiple request metrics correctly maintains a running average of request durations.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: No exceptions

### `RecordRequestMetric_FailedRequest_IncrementsFailureCount()`

Validates that recording a failed request increments the failure count metric.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: No exceptions

### `Validate_InvalidPort_ThrowsInvalidOperationException()`

Confirms that attempting to validate an invalid port throws an `InvalidOperationException`.

- **Parameters**: None
- **Return value**: `void`
- **Throws**:
  - `InvalidOperationException` if the port is invalid

## Usage
