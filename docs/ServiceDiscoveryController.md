# ServiceDiscoveryController
The `ServiceDiscoveryController` class is a crucial component in the `dotnet-grpc-gateway` project, responsible for managing service discovery and routing. It provides a set of APIs to retrieve information about available services, their routes, and conflicting routes, as well as to find matching routes. This controller is essential for enabling efficient and scalable service discovery and routing in a microservices architecture.

## API
The `ServiceDiscoveryController` class exposes the following public members:
* `GetAllServices`: Retrieves a list of all available services. Returns an `ActionResult` containing a `List<ServiceInfo>`. Throws if an error occurs during service discovery.
* `GetServiceRoutes`: Retrieves a list of routes for a specific service. Returns an `ActionResult` containing a `List<GatewayRoute>`. Throws if an error occurs during route retrieval.
* `FindMatchingRoute`: Finds a matching route based on the provided parameters. Returns an `ActionResult` containing a `RouteMatchResult`. Throws if an error occurs during route matching.
* `GetConflictingRoutes`: Retrieves a list of conflicting routes. Returns an `ActionResult` containing a `List<GatewayRoute>`. Throws if an error occurs during conflict detection.
* `Id`: Gets the identifier of the controller.
* `Name`: Gets the name of the controller.
* `ServiceFullName`: Gets the full name of the service.
* `Host`: Gets the host of the service.
* `Port`: Gets the port of the service.
* `UseTls`: Gets a value indicating whether to use TLS.
* `IsActive`: Gets a value indicating whether the controller is active.
* `Path`: Gets the path of the route.
* `RouteId`: Gets the identifier of the route.
* `Pattern`: Gets the pattern of the route.
* `ServiceId`: Gets the identifier of the service.
* `ServiceName`: Gets the name of the service.
* `Priority`: Gets the priority of the route.

## Usage
Here are two examples of using the `ServiceDiscoveryController` class:
```csharp
// Example 1: Retrieving all available services
var controller = new ServiceDiscoveryController();
var services = await controller.GetAllServices();
foreach (var service in services.Value)
{
    Console.WriteLine($"Service Name: {service.Name}, Service Full Name: {service.FullName}");
}

// Example 2: Finding a matching route
var controller = new ServiceDiscoveryController();
var routeMatchResult = await controller.FindMatchingRoute();
if (routeMatchResult.Value.IsMatch)
{
    Console.WriteLine($"Matching Route Found: {routeMatchResult.Value.Route}");
}
else
{
    Console.WriteLine("No matching route found");
}
```

## Notes
When using the `ServiceDiscoveryController` class, consider the following edge cases and thread-safety remarks:
* The `GetAllServices`, `GetServiceRoutes`, `FindMatchingRoute`, and `GetConflictingRoutes` methods are asynchronous and may throw exceptions if errors occur during service discovery, route retrieval, or conflict detection.
* The `Id`, `Name`, `ServiceFullName`, `Host`, `Port`, `UseTls`, `IsActive`, `Path`, `RouteId`, `Pattern`, `ServiceId`, `ServiceName`, and `Priority` properties are read-only and may be accessed concurrently by multiple threads.
* The `ServiceDiscoveryController` class is not designed to be thread-safe for modification. If you need to modify the controller's state, ensure that you synchronize access to the controller instance.
* The `ServiceDiscoveryController` class may cache results from previous service discovery and route retrieval operations. If you need to ensure that the latest information is retrieved, consider using the `GetAllServices` or `GetServiceRoutes` methods with a cache-busting mechanism.
