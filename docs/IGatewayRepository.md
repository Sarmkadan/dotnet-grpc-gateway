# IGatewayRepository

The `IGatewayRepository` interface defines the contract for data persistence operations regarding `GatewayConfiguration` entities within the `dotnet-grpc-gateway` system. It provides asynchronous methods for managing the lifecycle of gateway configurations, enabling CRUD operations and specialized retrieval of active configurations.

## API

### GatewayRepository
The `GatewayRepository` is the primary implementation class for the `IGatewayRepository` interface. It typically requires a database context or similar persistence mechanism provided via dependency injection.

### GetByIdAsync
Retrieves a specific gateway configuration by its unique identifier.

- **Parameters:** `Guid id` - The unique identifier of the gateway.
- **Returns:** `Task<GatewayConfiguration>` - The gateway configuration matching the specified ID, or `null` if not found.
- **Throws:** May throw an exception if the underlying data store connection fails.

### GetAllAsync
Retrieves all stored gateway configurations.

- **Returns:** `Task<List<GatewayConfiguration>>` - A list containing all available gateway configurations.

### GetActiveAsync
Retrieves all gateway configurations that are currently marked as active.

- **Returns:** `Task<List<GatewayConfiguration>>` - A list of active gateway configurations.

### CreateAsync
Persists a new gateway configuration to the data store.

- **Parameters:** `GatewayConfiguration configuration` - The new gateway configuration object to persist.
- **Returns:** `Task<GatewayConfiguration>` - The persisted gateway configuration, including any server-assigned identifiers.

### UpdateAsync
Updates an existing gateway configuration in the data store.

- **Parameters:** `GatewayConfiguration configuration` - The modified gateway configuration to update.
- **Returns:** `Task` - A task representing the asynchronous update operation.

### DeleteAsync
Removes a gateway configuration from the data store by its identifier.

- **Parameters:** `Guid id` - The unique identifier of the gateway configuration to remove.
- **Returns:** `Task` - A task representing the asynchronous deletion operation.

### CountAsync
Retrieves the total number of gateway configurations in the data store.

- **Returns:** `Task<int>` - The total count of stored gateway configurations.

## Usage

### Retrieving Active Gateways
This example demonstrates injecting the repository into a service and retrieving all currently active gateway configurations.

```csharp
public class GatewayService
{
    private readonly IGatewayRepository _repository;

    public GatewayService(IGatewayRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<GatewayConfiguration>> GetActiveConfigurationsAsync()
    {
        return await _repository.GetActiveAsync();
    }
}
```

### Creating and Managing a Gateway
This example demonstrates how to create a new gateway configuration and update its properties.

```csharp
public async Task ProcessGatewayAsync(IGatewayRepository repository)
{
    var newGateway = new GatewayConfiguration { Name = "PrimaryGateway", IsActive = true };
    var created = await repository.CreateAsync(newGateway);

    created.IsActive = false;
    await repository.UpdateAsync(created);
}
```

## Notes

- **Thread Safety:** Implementations of `IGatewayRepository` are generally intended to be thread-safe when used as scoped services in dependency injection containers (e.g., backed by an Entity Framework Core `DbContext`).
- **Asynchronous Execution:** All methods are asynchronous and should be awaited. Blocking calls on these methods may lead to thread pool starvation in high-concurrency environments.
- **Data Validation:** This repository performs persistence operations. Input validation for `GatewayConfiguration` properties should be performed by the calling layer prior to invocation to ensure data integrity.
- **Entity Identification:** `GetByIdAsync` and `DeleteAsync` assume the `GatewayConfiguration` uses a `Guid` as the primary key.
