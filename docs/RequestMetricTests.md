# RequestMetricTests

Overview of the test suite for the `RequestMetric` type. This class contains unit tests that verify the validation logic, state‑transition helpers, and default construction behavior of `RequestMetric` instances used throughout the dotnet‑grpc‑gateway project.

## API

| Member | Purpose | Parameters | Return Value | Throws |
|--------|---------|------------|--------------|--------|
| `Validate_ValidMetric_DoesNotThrow` | Confirms that a correctly populated `RequestMetric` passes validation without throwing. | none | `void` | Does not throw under valid conditions. |
| `Validate_EmptyServiceName_ThrowsInvalidOperationException` | Verifies that validation throws when the `ServiceName` property is empty. | none | `void` | Throws `System.InvalidOperationException` when `ServiceName` is `string.Empty`. |
| `Validate_NullMethodName_ThrowsInvalidOperationException` | Verifies that validation throws when the `MethodName` property is `null`. | none | `void` | Throws `System.InvalidOperationException` when `MethodName` is `null`. |
| `Validate_EmptyClientIpAddress_ThrowsInvalidOperationException` | Verifies that validation throws when the `ClientIpAddress` property is empty. | none | `void` | Throws `System.InvalidOperationException` when `ClientIpAddress` is `string.Empty`. |
| `Validate_NegativeDuration_ThrowsInvalidOperationException` | Verifies that validation throws when the `Duration` property is negative. | none | `void` | Throws `System.InvalidOperationException` when `Duration` < `0`. |
| `Validate_NegativeRequestSize_ThrowsInvalidOperationException` | Verifies that validation throws when the `RequestSize` property is negative. | none | `void` | Throws `System.InvalidOperationException` when `RequestSize` < `0`. |
| `Validate_NullClientIpAddress_ThrowsInvalidOperationException` | Verifies that validation throws when the `ClientIpAddress` property is `null`. | none | `void` | Throws `System.InvalidOperationException` when `ClientIpAddress` is `null`. |
| `Validate_NegativeResponseSize_ThrowsInvalidOperationException` | Verifies that validation throws when the `ResponseSize` property is negative. | none | `void` | Throws `System.InvalidOperationException` when `ResponseSize` < `0`. |
| `IsSlowRequest_BelowThreshold_ReturnsFalse` | Checks that `IsSlowRequest` returns `false` when the duration is below the configured slow‑request threshold. | none | `void` | Does not throw; asserts the return value is `false`. |
| `IsSlowRequest_AboveThreshold_ReturnsTrue` | Checks that `IsSlowRequest` returns `true` when the duration meets or exceeds the slow‑request threshold. | none | `void` | Does not throw; asserts the return value is `true`. |
| `RecordError_SetsErrorProperties` | Ensures that calling `RecordError` populates the error‑related fields (`HasError`, `ErrorMessage`, `ErrorStackTrace`). | none | `void` | Does not throw; asserts the properties are set as expected. |
| `RecordRetry_IncrementsRetryCount` | Confirms that each call to `RecordRetry` increments the `RetryCount` property by one. | none | `void` | Does not throw; asserts the increment behavior. |
| `RecordError_WithoutStackTrace_SetsErrorProperties` | Verifies that `RecordError` can be invoked with a null stack trace and still sets `HasError` and `ErrorMessage` while leaving `ErrorStackTrace` null. | none | `void` | Does not throw; asserts the appropriate property states. |
| `SetCacheStatus_SetsCacheHitStatus` | Validates that `SetCacheStatus` correctly updates the `CacheHit` flag based on the supplied boolean argument. | none | `void` | Does not throw; asserts the flag reflects the input. |
| `DefaultConstructor_SetsDefaultValues` | Ensures that a newly constructed `RequestMetric` instance has sensible default values for all members (e.g., zero counters, false flags, empty strings). | none | `void` | Does not throw; asserts each default value. |

## Usage

The test class is intended to be executed by a unit‑test runner (e.g., xUnit, NUnit, MSTest). Below are two typical ways to invoke the tests.

```csharp
using Xunit;
using DotNetGrpcGateway.Tests; // namespace containing RequestMetricTests

public class RequestMetricTestsFixture
{
    [Fact]
    public void RunAllValidationTests()
    {
        var sut = new RequestMetricTests();

        // Valid metric should not throw
        sut.Validate_ValidMetric_DoesNotThrow();

        // Each invalid case is expected to throw
        Assert.Throws<InvalidOperationException>(() => sut.Validate_EmptyServiceName_ThrowsInvalidOperationException());
        Assert.Throws<InvalidOperationException>(() => sut.Validate_NullMethodName_ThrowsInvalidOperationException());
        Assert.Throws<InvalidOperationException>(() => sut.Validate_EmptyClientIpAddress_ThrowsInvalidOperationException());
        Assert.Throws<InvalidOperationException>(() => sut.Validate_NegativeDuration_ThrowsInvalidOperationException());
        Assert.Throws<InvalidOperationException>(() => sut.Validate_NegativeRequestSize_ThrowsInvalidOperationException());
        Assert.Throws<InvalidOperationException>(() => sut.Validate_NullClientIpAddress_ThrowsInvalidOperationException());
        Assert.Throws<InvalidOperationException>(() => sut.Validate_NegativeResponseSize_ThrowsInvalidOperationException());
    }
}
```

```csharp
using NUnit.Framework;
using DotNetGrpcGateway.Tests;

[TestFixture]
public class RequestMetricBehaviorTests
{
    private RequestMetricTests _tests;

    [SetUp]
    public void SetUp() => _tests = new RequestMetricTests();

    [Test]
    public void SlowRequestDetection()
    {
        _tests.IsSlowRequest_BelowThreshold_ReturnsFalse();
        _tests.IsSlowRequest_AboveThreshold_ReturnsTrue();
    }

    [Test]
    public void ErrorAndRetryRecording()
    {
        _tests.RecordError_SetsErrorProperties();
        _tests.RecordRetry_IncrementsRetryCount();
        _tests.RecordError_WithoutStackTrace_SetsErrorProperties();
    }

    [Test]
    public void CacheAndConstruction()
    {
        _tests.SetCacheStatus_SetsCacheHitStatus();
        _tests.DefaultConstructor_SetsDefaultValues();
    }
}
```

## Notes

- All test methods are stateless; they create any required `RequestMetric` instances internally, so invoking them from multiple threads concurrently does not produce shared‑mutable state and is therefore thread‑safe.
- The validation tests deliberately check for `InvalidOperationException`; any change in the exception type thrown by `RequestMetric.Validate` would cause these tests to fail, providing a safeguard against accidental API changes.
- Edge cases covered include empty strings, `null` references, and negative numeric values for duration and size fields. No other invalid inputs (e.g., excessively large values) are asserted by the current test suite.
- The `IsSlowRequest` tests rely on the internal threshold defined in `RequestMetric`; if the threshold is altered, the corresponding tests must be updated to reflect the new boundary.
- No external resources (files, network, etc.) are accessed by these tests, making them suitable for fast execution in continuous integration pipelines.
