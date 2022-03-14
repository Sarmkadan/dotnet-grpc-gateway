# HttpContextExtensions
The `HttpContextExtensions` class provides a set of extension methods for the `HttpContext` class, allowing developers to easily access and manipulate HTTP request data in a gRPC gateway context. These extensions enable the retrieval of client IP addresses, request headers, authorization tokens, and request IDs, as well as the detection of gRPC requests and web requests.

## API
* `public static string GetClientIpAddress`: Retrieves the IP address of the client making the request. This method returns a string representing the client's IP address and does not throw any exceptions.
* `public static string? GetHeader`: Retrieves the value of a specific header from the HTTP request. This method takes no parameters and returns a nullable string, indicating that the header may not be present in the request. It does not throw any exceptions.
* `public static string? GetAuthorizationToken`: Retrieves the authorization token from the HTTP request. This method returns a nullable string, indicating that the token may not be present in the request. It does not throw any exceptions.
* `public static string GetRequestId`: Retrieves the ID of the current request. This method returns a string representing the request ID and does not throw any exceptions.
* `public static bool IsGrpcRequest`: Determines whether the current request is a gRPC request. This method returns a boolean value indicating whether the request is a gRPC request and does not throw any exceptions.
* `public static bool IsGrpcWebRequest`: Determines whether the current request is a gRPC web request. This method returns a boolean value indicating whether the request is a gRPC web request and does not throw any exceptions.

## Usage
The following examples demonstrate how to use the `HttpContextExtensions` class:
```csharp
// Example 1: Retrieving client IP address and request ID
var ipAddress = HttpContext.Current.GetClientIpAddress();
var requestId = HttpContext.Current.GetRequestId();
Console.WriteLine($"Client IP address: {ipAddress}, Request ID: {requestId}");

// Example 2: Checking if a request is a gRPC request and retrieving the authorization token
if (HttpContext.Current.IsGrpcRequest())
{
    var token = HttpContext.Current.GetAuthorizationToken();
    Console.WriteLine($"gRPC request detected, Authorization token: {token}");
}
```

## Notes
When using the `GetHeader` and `GetAuthorizationToken` methods, be aware that they return nullable strings, indicating that the header or token may not be present in the request. This can occur if the client does not provide the header or token, or if the request is not a gRPC request. Additionally, the `IsGrpcRequest` and `IsGrpcWebRequest` methods are thread-safe, as they only access the current `HttpContext` instance. However, it is essential to ensure that the `HttpContext` instance is properly initialized and accessible before calling these methods to avoid any potential issues.
