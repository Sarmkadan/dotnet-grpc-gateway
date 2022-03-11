# IHttpClientProvider
The `IHttpClientProvider` interface is designed to provide a flexible way to manage instances of `HttpClient`, allowing for customization of various settings such as timeouts, retries, and default headers. This interface is useful in scenarios where multiple clients need to be created with different configurations, or where the client configuration needs to be changed dynamically.

## API
* `Timeout`: Gets the timeout value for the client, represented as a `TimeSpan`.
* `MaxRetries`: Gets the maximum number of retries for the client, represented as an `int`.
* `AllowAutoRedirect`: Gets a boolean value indicating whether the client allows auto-redirects.
* `MaxConnectionsPerServer`: Gets the maximum number of connections per server, represented as an `int`.
* `DefaultHeaders`: Gets a dictionary of default headers for the client, represented as a `Dictionary<string, string>`.
* `HttpClientProvider`: Not applicable, as this is the interface itself.
* `CreateClient`: Creates a new instance of `HttpClient` with the specified configuration.
* `GetClient`: Retrieves an existing instance of `HttpClient` with the specified configuration, or creates a new one if it does not exist.
* `RemoveClient`: Removes an existing instance of `HttpClient` from the provider.

## Usage
The following examples demonstrate how to use the `IHttpClientProvider` interface:
```csharp
// Example 1: Creating a client with custom configuration
var provider = new HttpClientProvider();
var client = provider.CreateClient();
client.Timeout = TimeSpan.FromSeconds(30);
client.DefaultRequestHeaders.Add("Accept", "application/json");

// Example 2: Retrieving an existing client and modifying its configuration
var existingClient = provider.GetClient();
existingClient.MaxRetries = 3;
existingClient.AllowAutoRedirect = false;
```

## Notes
When using the `IHttpClientProvider` interface, it is essential to consider the following edge cases and thread-safety remarks:
* The `CreateClient` and `GetClient` methods may return the same instance of `HttpClient` if the configuration is identical.
* The `RemoveClient` method may throw an exception if the client is still in use.
* The `DefaultHeaders` dictionary is shared among all clients created by the provider, so modifications to it will affect all clients.
* The `IHttpClientProvider` interface is not inherently thread-safe, so access to its members should be synchronized if used in a multi-threaded environment.
