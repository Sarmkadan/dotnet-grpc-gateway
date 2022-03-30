# OutputFormatterFactory
The `OutputFormatterFactory` class is a central component in the `dotnet-grpc-gateway` project, responsible for managing and providing output formatters for various content types. It allows registration of custom formatters, retrieval of available content types, and determination of whether a specific content type is supported. This enables flexible and extensible handling of different output formats in gRPC gateway applications.

## API
* `public OutputFormatterFactory`: The constructor for creating an instance of the `OutputFormatterFactory` class.
* `public void RegisterFormatter`: Registers a custom output formatter for a specific content type. This method takes in an `IOutputFormatter` instance and associates it with a particular content type, allowing for the handling of custom output formats.
* `public IOutputFormatter GetFormatter`: Retrieves an output formatter for a given content type. This method returns an `IOutputFormatter` instance that can be used to format output in the specified content type.
* `public IEnumerable<string> GetAvailableContentTypes`: Returns a collection of content types that are currently supported by the factory. This method provides a way to discover the available content types without having to attempt to retrieve a formatter for each one.
* `public bool IsSupported`: Determines whether a specific content type is supported by the factory. This method returns a boolean value indicating whether the factory can provide a formatter for the given content type.

## Usage
The following examples demonstrate how to use the `OutputFormatterFactory` class:
```csharp
// Example 1: Registering a custom formatter and retrieving it
var factory = new OutputFormatterFactory();
factory.RegisterFormatter(new CustomJsonFormatter(), "application/json");
var formatter = factory.GetFormatter("application/json");
// Use the formatter to format output
```

```csharp
// Example 2: Checking available content types and using a formatter
var factory = new OutputFormatterFactory();
var availableContentTypes = factory.GetAvailableContentTypes();
if (factory.IsSupported("text/plain"))
{
    var formatter = factory.GetFormatter("text/plain");
    // Use the formatter to format output in plain text
}
```

## Notes
When using the `OutputFormatterFactory` class, it is essential to consider the following edge cases and thread-safety remarks:
* The `RegisterFormatter` method may throw an exception if a formatter is already registered for the same content type.
* The `GetFormatter` method may return `null` if no formatter is registered for the specified content type.
* The `GetAvailableContentTypes` method returns a snapshot of the currently available content types and may not reflect changes made after the method call.
* The `IsSupported` method is thread-safe, but the `RegisterFormatter` and `GetFormatter` methods are not. Therefore, it is recommended to synchronize access to these methods when using the factory in a multi-threaded environment.
