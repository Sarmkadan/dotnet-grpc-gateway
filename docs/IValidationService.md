# IValidationService

Central service for validating configuration, runtime parameters, and security tokens used by the gRPC Gateway. It ensures that gateway routes, gRPC services, and client requests comply with configured policies before processing.

## API

### `ValidationService`

Constructs a new instance of the validation service. This service is designed to be injected and reused across the gateway runtime.

### `ValidateGatewayConfiguration`
