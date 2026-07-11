# RequestContextExtensions
The `RequestContextExtensions` class provides a set of extension methods for working with request contexts in a gRPC gateway application. It offers properties and methods to access and manipulate information about the current request, such as user IDs, client information, and request start times.

## API
* `public static bool HasUserId`: Indicates whether a user ID is present in the current request context. Returns `true` if a user ID is available, `false` otherwise. This property does not throw any exceptions.
* `public static string GetClientInfo`: Retrieves client information from the current request context. Returns a string containing client information or an empty string if no client information is available. This property does not throw any exceptions.
* `public static void SetStartTime`: Sets the start time of the current request. This method does not throw any exceptions but may have unintended consequences if called multiple times or outside the context of a request.
* `public static DateTime? GetStartTime`: Retrieves the start time of the current request. Returns a `DateTime?` representing the start time or `null` if no start time has been set. This property does not throw any exceptions.

## Usage
The following examples demonstrate how to use the `RequestContextExtensions` class in a gRPC gateway application:
```csharp
// Example 1: Accessing user ID and client information
if (RequestContextExtensions.HasUserId)
{
    var clientId = RequestContextExtensions.GetClientInfo;
    Console.WriteLine($"User ID present, client info: {clientId}");
}
else
{
    Console.WriteLine("No user ID present");
}

// Example 2: Setting and retrieving request start time
RequestContextExtensions.SetStartTime();
var startTime = RequestContextExtensions.GetStartTime;
if (startTime.HasValue)
{
    Console.WriteLine($"Request started at {startTime.Value}");
}
else
{
    Console.WriteLine("No start time set");
}
```

## Notes
When using the `RequestContextExtensions` class, consider the following edge cases and thread-safety remarks:
* The `HasUserId` property and `GetClientInfo` method are thread-safe, as they only access existing information in the request context.
* The `SetStartTime` method is not thread-safe, as it modifies the request context. Calling this method multiple times or from different threads may lead to unexpected behavior.
* The `GetStartTime` method is thread-safe, as it only retrieves existing information from the request context. However, its return value may be `null` if `SetStartTime` has not been called or if the start time has been reset.
