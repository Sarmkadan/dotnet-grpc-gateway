# GatewayRouteTests
The `GatewayRouteTests` class is designed to test the functionality of gateway routes in the `dotnet-grpc-gateway` project. It provides a set of test methods to validate the behavior of gateway routes under various conditions, ensuring that they are correctly configured and behave as expected.

## API
The `GatewayRouteTests` class includes the following public members:
* `Validate_ValidRoute_DoesNotThrow`: Validates that a valid route does not throw any exceptions.
* `Validate_EmptyPattern_ThrowsInvalidOperationException`: Validates that an empty pattern throws an `InvalidOperationException`.
* `Validate_InvalidTargetServiceId_ThrowsInvalidOperationException`: Validates that an invalid target service ID throws an `InvalidOperationException`.
* `Validate_NegativePriority_ThrowsInvalidOperationException`: Validates that a negative priority throws an `InvalidOperationException`.
* `Validate_TooHighPriority_ThrowsInvalidOperationException`: Validates that a priority that is too high throws an `InvalidOperationException`.
* `Validate_ZeroRateLimit_ThrowsInvalidOperationException`: Validates that a zero rate limit throws an `InvalidOperationException`.
* `Validate_NegativeCacheDuration_ThrowsInvalidOperationException`: Validates that a negative cache duration throws an `InvalidOperationException`.
* `MatchesRequest_ExactMatch_ReturnsTrue`: Validates that an exact match returns `true`.
* `MatchesRequest_PrefixMatch_ReturnsTrue`: Validates that a prefix match returns `true`.
* `MatchesRequest_RegexMatch_ReturnsTrue`: Validates that a regex match returns `true`.
* `MatchesRequest_InvalidMatchType_ReturnsFalse`: Validates that an invalid match type returns `false`.
* `UpdateModifiedDate_UpdatesModifiedAt`: Updates the modified date and verifies that the `ModifiedAt` property is updated.
* `DefaultConstructor_SetsDefaultValues`: Verifies that the default constructor sets the default values.
* `Validate_DefaultRoute_DoesNotThrow`: Validates that a default route does not throw any exceptions.
* `Validate_ValidRouteWithAllProperties_SetCorrectly`: Validates that a valid route with all properties is set correctly.

## Usage
Here are two examples of using the `GatewayRouteTests` class:
```csharp
// Example 1: Validating a route
var route = new GatewayRoute { Pattern = "/test", TargetServiceId = "test-service" };
GatewayRouteTests tests = new GatewayRouteTests();
tests.Validate_ValidRoute_DoesNotThrow();

// Example 2: Updating the modified date
var route2 = new GatewayRoute { Pattern = "/test2", TargetServiceId = "test-service2" };
GatewayRouteTests tests2 = new GatewayRouteTests();
tests2.UpdateModifiedDate_UpdatesModifiedAt();
```

## Notes
When using the `GatewayRouteTests` class, note that the `Validate_*` methods will throw an `InvalidOperationException` if the route is invalid. Additionally, the `MatchesRequest_*` methods will return `false` if the match type is invalid. The `UpdateModifiedDate_UpdatesModifiedAt` method will update the `ModifiedAt` property, but it does not throw any exceptions. The `GatewayRouteTests` class is designed to be thread-safe, but it is recommended to use a new instance for each test to ensure isolation.
