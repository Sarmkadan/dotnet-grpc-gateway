// ... (rest of the file remains the same)

## IRouteRepository

The `IRouteRepository` interface defines a contract for managing GatewayRoute entities. It provides methods for retrieving, creating, updating, and deleting routes, as well as filtering routes by service ID or pattern.

### Example Usage:

```csharp
public class Program
{
  public static async Task Main(string[] args)
  {
    var repository = new RouteRepository(new InMemoryConnectionStringProvider());

    var route = await repository.CreateAsync(new GatewayRoute
    {
      Pattern = "/api/users",
      TargetServiceId = 1,
      IsActive = true,
      Priority = 1
    });

    var activeRoutes = await repository.GetActiveAsync();
    Console.WriteLine($"Active routes: {activeRoutes.Count}");

    var routeById = await repository.GetByIdAsync(route.Id);
    Console.WriteLine($"Route by ID: {routeById.Pattern}");

    var routesByServiceId = await repository.GetByServiceIdAsync(1);
    Console.WriteLine($"Routes by service ID: {routesByServiceId.Count}");

    var routesByPattern = await repository.GetByPatternAsync("/api/users");
    Console.WriteLine($"Routes by pattern: {routesByPattern.Count}");

    await repository.UpdateAsync(route);
    await repository.DeleteAsync(route.Id);
  }
}
```
