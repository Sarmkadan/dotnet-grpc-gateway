# RouteManagementServiceTests

Unit tests for the `RouteManagementService` class, focusing on route validation and retrieval scenarios. These tests verify the behavior of route pattern validation, duplicate detection, and service-specific route filtering.

## API

### `ValidateRouteAsync_EmptyPattern_ReturnsFalse`

Ensures that a route with an empty pattern is rejected during validation.

- **Parameters**: None
- **Return value**: `Task` completing with `false` when the route pattern is empty.
- **Exceptions**: Throws `ArgumentException` if the route pattern is null.

### `ValidateRouteAsync_EmptyPattern_LogsWarning`

Verifies that an empty route pattern triggers a warning log during validation.

- **Parameters**: None
- **Return value**: `Task` completing when the warning has been logged.
- **Exceptions**: Throws `ArgumentException` if the route pattern is null.

### `ValidateRouteAsync_DuplicatePatternInRepository_ReturnsFalse`

Confirms that a route with a pattern already present in the repository is rejected.

- **Parameters**: None
- **Return value**: `Task` completing with `false` when a duplicate pattern is detected.
- **Exceptions**: Throws `ArgumentException` if the route pattern is null or empty.

### `ValidateRouteAsync_UniqueValidRoute_ReturnsTrue`

Validates that a route with a unique, well-formed pattern is accepted.

- **Parameters**: None
- **Return value**: `Task` completing with `true` when the route pattern is valid and unique.
- **Exceptions**: Throws `ArgumentException` if the route pattern is null or empty.

### `GetRoutesByServiceAsync_MultipleServices_ReturnsOnlyMatchingRoutes`

Ensures that route retrieval filters results to only those matching the specified service.

- **Parameters**: None
- **Return value**: `Task` completing with a collection of routes filtered by the target service.
- **Exceptions**: Throws `ArgumentNullException` if the service identifier is null.

## Usage
