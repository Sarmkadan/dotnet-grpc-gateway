# RequestContext
The `RequestContext` type is designed to capture and manage information about an incoming request in a gRPC gateway application. It provides a centralized location for storing and retrieving request-specific data, such as the request ID, client IP, user ID, and other custom properties. This allows developers to easily access and utilize this information throughout the request processing pipeline.

## API
* `RequestId`: A string representing the unique identifier of the request.
* `CorrelationId`: A string representing the correlation ID of the request.
* `ClientIp`: A string representing the IP address of the client making the request.
* `UserId`: A nullable string representing the ID of the user making the request.
* `Path`: A string representing the path of the request.
* `Method`: A string representing the HTTP method of the request.
* `StartTime`: A `DateTime` representing the start time of the request.
* `Properties`: A dictionary of string-object pairs representing custom properties associated with the request.
* `RequestContext()`: The constructor for creating a new instance of `RequestContext`.
* `SetProperty(string key, object value)`: Sets a custom property with the specified key and value. The `key` parameter is the name of the property, and the `value` parameter is the value to be stored.
* `GetProperty<T>(string key)`: Retrieves the value of a custom property with the specified key. The `key` parameter is the name of the property, and the method returns the value as an instance of type `T`, or `null` if the property does not exist.

## Usage
The following examples demonstrate how to use the `RequestContext` type in a gRPC gateway application:
```csharp
// Example 1: Creating a new RequestContext instance and setting custom properties
var context = new RequestContext();
context.SetProperty("CustomProperty1", "Value1");
context.SetProperty("CustomProperty2", 123);

// Example 2: Retrieving custom properties from a RequestContext instance
var customProperty1 = context.GetProperty<string>("CustomProperty1");
var customProperty2 = context.GetProperty<int>("CustomProperty2");
Console.WriteLine($"CustomProperty1: {customProperty1}, CustomProperty2: {customProperty2}");
```

## Notes
When using the `RequestContext` type, it is essential to consider the following edge cases and thread-safety remarks:
* The `Properties` dictionary is not thread-safe, so access to it should be synchronized when used in a multi-threaded environment.
* The `SetProperty` and `GetProperty` methods do not perform any validation on the key or value parameters, so it is the responsibility of the developer to ensure that the keys are unique and the values are of the correct type.
* The `GetProperty` method returns `null` if the property does not exist, so developers should be prepared to handle this case when retrieving properties.
* The `RequestContext` instance should be properly disposed of when it is no longer needed to avoid memory leaks.
