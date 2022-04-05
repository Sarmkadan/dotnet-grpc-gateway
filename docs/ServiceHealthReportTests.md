# ServiceHealthReportTests

`ServiceHealthReportTests` is a test fixture class designed to validate the behavior and correctness of the `ServiceHealthReport` type within the `dotnet-grpc-gateway` project. It ensures that health reports are properly validated, state transitions occur as expected, and diagnostic messages are managed correctly under various conditions.

## API

### Validate_ValidReport_DoesNotThrow
- **Purpose**: Verifies that a valid `ServiceHealthReport` instance passes validation without throwing exceptions.
- **Parameters**: None.
- **Return Value**: `void`.
- **Exceptions**: Does not throw any exceptions when the report is valid.

### Validate_InvalidServiceId_ThrowsInvalidOperationException
- **Purpose**: Ensures that an invalid service ID (e.g., empty or malformed) triggers an `InvalidOperationException`.
- **Parameters**: None.
- **Return Value**: `void`.
- **Exceptions**: Throws `InvalidOperationException` if the service ID is invalid.

### Validate_NegativeSuccessRate_ThrowsInvalidOperationException
- **Purpose**: Confirms that a negative success rate value causes an `InvalidOperationException`.
- **Parameters**: None.
- **Return Value**: `void`.
- **Exceptions**: Throws `InvalidOperationException` if the success rate is negative.

### Validate_SuccessRateAbove100_ThrowsInvalidOperationException
- **Purpose**: Validates that a success rate exceeding 100% results in an `InvalidOperationException`.
- **Parameters**: None.
- **Return Value**: `void`.
- **Exceptions**: Throws `InvalidOperationException` if the success rate is greater than 100.

### Validate_NullHealthStatus_ThrowsInvalidOperationException
- **Purpose**: Checks that a null health status value throws an `InvalidOperationException`.
- **Parameters**: None.
- **Return Value**: `void`.
- **Exceptions**: Throws `InvalidOperationException` if the health status is null.

### Validate_NegativeResponseTime_ThrowsInvalidOperationException
- **Purpose**: Ensures that a negative response time value triggers an `InvalidOperationException`.
- **Parameters**: None.
- **Return Value**: `void`.
- **Exceptions**: Throws `InvalidOperationException` if the response time is negative.

### RecordCheckResult_SuccessfulCheck_IncrementsSuccessCounters
- **Purpose**: Verifies that recording a successful health check increments the success counters appropriately.
- **Parameters**: None.
- **Return Value**: `void`.

### RecordCheckResult_FailedCheck_IncrementsFailureCounters
- **Purpose**: Confirms that recording a failed health check increments the failure counters.
- **Parameters**: None.
- **Return Value**: `void`.

### RecordCheckResult_MultipleFailures_EventuallyMarksUnhealthy
- **Purpose**: Ensures that after multiple consecutive failures, the service is marked as unhealthy.
- **Parameters**: None.
- **Return Value**: `void`.

### RecordCheckResult_CalculatesSuccessRateCorrectly
- **Purpose**: Validates that the success rate is calculated accurately based on recorded check results.
- **Parameters**: None.
- **Return Value**: `void`.

### AddDiagnosticMessage_AddsMessageToList
- **Purpose**: Confirms that a diagnostic message is added to the internal list.
- **Parameters**: None.
- **Return Value**: `void`.

### AddDiagnosticMessage_MoreThan10Messages_RemovesOldest
- **Purpose**: Ensures that when more than 10 diagnostic messages are added, the oldest messages are removed to maintain the limit.
- **Parameters**: None.
- **Return Value**: `void`.

### ShouldBeMarkedUnhealthy_AfterThreeFailures_ReturnsTrue
- **Purpose**: Verifies that the service is marked unhealthy after three consecutive failures.
- **Parameters**: None.
- **Return Value**: `void`.

### ShouldBeMarkedUnhealthy_BeforeThreeFailures_ReturnsFalse
- **Purpose**: Confirms that the service remains healthy before three consecutive failures.
- **Parameters**: None.
- **Return Value**: `void`.

### GetAvailabilityPercentage_ReturnsCorrectPercentage
- **Purpose**: Validates that the availability percentage is computed correctly based on success and failure counts.
- **Parameters**: None.
- **Return Value**: `void`.

### DefaultConstructor_SetsDefaultValues
- **Purpose**: Ensures that the default constructor initializes the `ServiceHealthReport` with expected default values.
- **Parameters**: None.
- **Return Value**: `void`.

### Validate_ValidReportWithAllProperties_SetCorrectly
- **Purpose**: Confirms that all properties of a valid report are set correctly during initialization.
- **Parameters**: None.
- **Return Value**: `void`.

## Usage

```csharp
[Fact]
public void Validate_ValidReport_DoesNotThrow()
{
    var report = new ServiceHealthReport
    {
        ServiceId = "valid-service",
        SuccessRate = 95.5,
        ResponseTime = TimeSpan.FromMilliseconds(100),
        HealthStatus = HealthStatus.Healthy
    };

    var validator = new ServiceHealthReportValidator();
    Action act = () => validator.Validate(report);

    act.Should().NotThrow();
}
```

```csharp
[Fact]
public void AddDiagnosticMessage_MoreThan10Messages_RemovesOldest()
{
    var report = new ServiceHealthReport();

    for (int i = 0; i < 12; i++)
    {
        report.AddDiagnosticMessage($"Message {i}");
    }

    report.Diagnostics.Should().HaveCount(10);
    report.Diagnostics.First().Should().Be("Message 2");
}
```

## Notes

- The `ServiceHealthReportTests` class is intended solely for unit testing and does not require thread-safety considerations in its implementation.
- Diagnostic messages are capped at 10 entries; older messages are automatically removed when the limit is exceeded.
- A service is marked unhealthy after three consecutive failed health checks, as determined by the `ShouldBeMarkedUnhealthy` logic.
- The success rate must be between 0 and 100 inclusive; values outside this range trigger validation errors.
- Null or malformed service identifiers, response times, and health statuses are explicitly validated to prevent invalid state transitions.
