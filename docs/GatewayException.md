# GatewayException

`GatewayException` is the base exception type used within the `dotnet-grpc-gateway` project to represent errors encountered during request processing. It encapsulates error details such as an error code, HTTP status code, and additional context via a dictionary of key-value pairs. Derived exception types provide specific error scenarios, enabling structured error handling and propagation.

## API

### `public string ErrorCode`
Gets the machine-readable error code associated with the exception. This code is intended for programmatic consumption and categorization of errors.

### `public int? HttpStatusCode`
Gets the HTTP status code that should be returned to the client when this exception is thrown. This property may be `null` if the exception does not map to an HTTP status code.

### `public Dictionary<string, object>? Details`
Gets a dictionary containing additional context or metadata about the error. This property may be `null` if no additional details are provided. Keys are strings, and values can be of any type.

### `public GatewayException()`
Initializes a new instance of the `GatewayException` class with default values. The `ErrorCode` will be set to a generic value, and `HttpStatusCode` and `Details` will be `null`.

### `public GatewayException(string errorCode)`
Initializes a new instance of the `GatewayException` class with the specified error code.
- **Parameters**:
  - `errorCode`: The machine-readable error code associated with the exception.

### `public GatewayException(string errorCode, int httpStatusCode)`
Initializes a new instance of the `GatewayException` class with the specified error code and HTTP status code.
- **Parameters**:
  - `errorCode`: The machine-readable error code associated with the exception.
  - `httpStatusCode`: The HTTP status code that should be returned to the client.

### `public GatewayException(string errorCode, int httpStatusCode, Dictionary<string, object> details)`
Initializes a new instance of the `GatewayException` class with the specified error code, HTTP status code, and additional details.
- **Parameters**:
  - `errorCode`: The machine-readable error code associated with the exception.
  - `httpStatusCode`: The HTTP status code that should be returned to the client.
  - `details`: A dictionary containing additional context or metadata about the error.

### `public void AddDetail(string key, object value)`
Adds a key-value pair to the `Details` dictionary. If the `Details` dictionary is `null`, it will be initialized.
- **Parameters**:
  - `key`: The key of the detail entry.
  - `value`: The value associated with the key.
- **Exceptions**:
  - `ArgumentNullException`: Thrown if `key` is `null`.

### `public class ServiceNotFoundException : GatewayException`
Represents an error indicating that the requested gRPC service was not found.
- **Constructors**:
  - `ServiceNotFoundException()`: Initializes a new instance with a default error code.
  - `ServiceNotFoundException(string errorCode)`: Initializes a new instance with the specified error code.
  - `ServiceNotFoundException(string errorCode, int httpStatusCode)`: Initializes a new instance with the specified error code and HTTP status code.
  - `ServiceNotFoundException(string errorCode, int httpStatusCode, Dictionary<string, object> details)`: Initializes a new instance with the specified error code, HTTP status code, and details.

### `public class ServiceUnavailableException : GatewayException`
Represents an error indicating that the requested gRPC service is unavailable.
- **Constructors**:
  - `ServiceUnavailableException()`: Initializes a new instance with a default error code.
  - `ServiceUnavailableException(string errorCode)`: Initializes a new instance with the specified error code.
  - `ServiceUnavailableException(string errorCode, int httpStatusCode)`: Initializes a new instance with the specified error code and HTTP status code.
  - `ServiceUnavailableException(string errorCode, int httpStatusCode, Dictionary<string, object> details)`: Initializes a new instance with the specified error code, HTTP status code, and details.

### `public class RouteResolutionException : GatewayException`
Represents an error indicating that the requested route could not be resolved.
- **Constructors**:
  - `RouteResolutionException()`: Initializes a new instance with a default error code.
  - `RouteResolutionException(string errorCode)`: Initializes a new instance with the specified error code.
  - `RouteResolutionException(string errorCode, int httpStatusCode)`: Initializes a new instance with the specified error code and HTTP status code.
  - `RouteResolutionException(string errorCode, int httpStatusCode, Dictionary<string, object> details)`: Initializes a new instance with the specified error code, HTTP status code, and details.

### `public class AuthenticationException : GatewayException`
Represents an error indicating that the request failed authentication.
- **Constructors**:
  - `AuthenticationException()`: Initializes a new instance with a default error code.
  - `AuthenticationException(string errorCode)`: Initializes a new instance with the specified error code.
  - `AuthenticationException(string errorCode, int httpStatusCode)`: Initializes a new instance with the specified error code and HTTP status code.
  - `AuthenticationException(string errorCode, int httpStatusCode, Dictionary<string, object> details)`: Initializes a new instance with the specified error code, HTTP status code, and details.

### `public class AuthorizationException : GatewayException`
Represents an error indicating that the request failed authorization.
- **Constructors**:
  - `AuthorizationException()`: Initializes a new instance with a default error code.
  - `AuthorizationException(string errorCode)`: Initializes a new instance with the specified error code.
  - `AuthorizationException(string errorCode, int httpStatusCode)`: Initializes a new instance with the specified error code and HTTP status code.
  - `AuthorizationException(string errorCode, int httpStatusCode, Dictionary<string, object> details)`: Initializes a new instance with the specified error code, HTTP status code, and details.

### `public class ConfigurationException : GatewayException`
Represents an error indicating that the gateway encountered a configuration issue.
- **Constructors**:
  - `ConfigurationException()`: Initializes a new instance with a default error code.
  - `ConfigurationException(string errorCode)`: Initializes a new instance with the specified error code.
  - `ConfigurationException(string errorCode, int httpStatusCode)`: Initializes a new instance with the specified error code and HTTP status code.
  - `ConfigurationException(string errorCode, int httpStatusCode, Dictionary<string, object> details)`: Initializes a new instance with the specified error code, HTTP status code, and details.

### `public class DataAccessException : GatewayException`
Represents an error indicating that the gateway encountered an issue accessing data.
- **Constructors**:
  - `DataAccessException()`: Initializes a new instance with a default error code.
  - `DataAccessException(string errorCode)`: Initializes a new instance with the specified error code.
  - `DataAccessException(string errorCode, int httpStatusCode)`: Initializes a new instance with the specified error code and HTTP status code.
  - `DataAccessException(string errorCode, int httpStatusCode, Dictionary<string, object> details)`: Initializes a new instance with the specified error code, HTTP status code, and details.

### `public class RateLimitException : GatewayException`
Represents an error indicating that the request was rate-limited.
- **Constructors**:
  - `RateLimitException()`: Initializes a new instance with a default error code.
  - `RateLimitException(string errorCode)`: Initializes a new instance with the specified error code.
  - `RateLimitException(string errorCode, int httpStatusCode)`: Initializes a new instance with the specified error code and HTTP status code.
  - `RateLimitException(string errorCode, int httpStatusCode, Dictionary<string, object> details)`: Initializes a new instance with the specified error code, HTTP status code, and details.

### `public class TimeoutException : GatewayException`
Represents an error indicating that the request timed out.
- **Constructors**:
  - `TimeoutException()`: Initializes a new instance with a default error code.
  - `TimeoutException(string errorCode)`: Initializes a new instance with the specified error code.
  - `TimeoutException(string errorCode, int httpStatusCode)`: Initializes a new instance with the specified error code and HTTP status code.
  - `TimeoutException(string errorCode, int httpStatusCode, Dictionary<string, object> details)`: Initializes a new instance with the specified error code, HTTP status code, and details.

## Usage

### Example 1: Throwing a `GatewayException` with Details
