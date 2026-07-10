# IServiceRegistry

Central registry for managing gRPC service definitions within the dotnet-grpc-gateway system. Provides CRUD operations and discovery capabilities for registered gRPC services.

## API

### `ServiceRegistry`

The concrete implementation of `IServiceRegistry` that maintains the collection of registered gRPC services. Thread-safe for concurrent reads; writes require synchronization.

### `Task<GrpcService> GetByIdAsync(Guid id)`

Retrieves a gRPC service by its unique identifier.

- **Parameters**:
  - `id`: The unique identifier of the service to retrieve.
- **Returns**: A `Task<GrpcService>` resolving to the requested service.
- **Throws**:
  - `KeyNotFoundException` if no service with the specified `id` exists.

### `Task<GrpcService?> GetByNameAsync(string name)`

Attempts to retrieve a gRPC service by its simple name (e.g., `Greeter`).

- **Parameters**:
  - `name`: The simple name of the service to locate.
- **Returns**: A `Task<GrpcService?>` resolving to the service if found, otherwise `null`.
- **Throws**: None.

### `Task<GrpcService?> GetByFullNameAsync(string fullName)`

Attempts to retrieve a gRPC service by its fully-qualified name (e.g., `example.v1.Greeter`).

- **Parameters**:
  - `fullName`: The fully-qualified name of the service to locate.
- **Returns**: A `Task<GrpcService?>` resolving to the service if found, otherwise `null`.
- **Throws**: None.

### `Task<List<GrpcService>> GetAllAsync()`

Retrieves all registered gRPC services.

- **Returns**: A `Task<List<GrpcService>>` containing all registered services.
- **Throws**: None.

### `Task<List<GrpcService>> GetActiveAsync()`

Retrieves all registered gRPC services that are currently active (enabled for routing).

- **Returns**: A `Task<List<GrpcService>>` containing all active services.
- **Throws**: None.

### `Task<GrpcService> RegisterAsync(GrpcService service)`

Registers a new gRPC service definition.

- **Parameters**:
  - `service`: The service definition to register.
- **Returns**: A `Task<GrpcService>` resolving to the registered service.
- **Throws**:
  - `ArgumentException` if `service` is `null`.
  - `InvalidOperationException` if a service with the same `Id` or `FullName` already exists.

### `Task UpdateAsync(GrpcService service)`

Updates an existing gRPC service definition.

- **Parameters**:
  - `service`: The updated service definition.
- **Returns**: A `Task` representing the asynchronous operation.
- **Throws**:
  - `ArgumentException` if `service` is `null`.
  - `KeyNotFoundException` if no service with the specified `Id` exists.
  - `InvalidOperationException` if the update would conflict with an existing service’s `FullName`.

### `Task UnregisterAsync(Guid id)`

Removes a gRPC service from the registry by its unique identifier.

- **Parameters**:
  - `id`: The unique identifier of the service to remove.
- **Returns**: A `Task` representing the asynchronous operation.
- **Throws**:
  - `KeyNotFoundException` if no service with the specified `id` exists.

### `Task<List<GrpcService>> FindByHostAsync(string host)`

Retrieves all gRPC services associated with a specific host (e.g., `localhost:5001`).

- **Parameters**:
  - `host`: The host address to filter services by.
- **Returns**: A `Task<List<GrpcService>>` containing all services associated with the specified host.
- **Throws**: None.

## Usage

### Registering and retrieving a service
