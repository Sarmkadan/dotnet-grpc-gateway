# RequestLogServiceTests

`RequestLogServiceTests` is the unit test suite for the `RequestLogService` component in the `dotnet-grpc-gateway` project. It validates the service’s behavior when processing gRPC gateway requests under various conditions—success, failure, latency, payload size, cache interaction, and retry scenarios—and ensures that the default constructor initializes internal state correctly. Each test method targets a specific logging outcome, verifying that the service produces the appropriate log entry type and content.

## API

### public void LogRequest_ValidRequest_CreatesLogEntry
Validates that a well-formed, successful request results in a standard log entry being created. The test arranges a request context with no errors, normal latency, and typical payload size, then asserts that the service emits a log entry with the expected informational content and no error or warning markers.

- **Parameters:** none (test method)
- **Return value:** void
- **Throws:** assertion failures if the log entry is missing, has incorrect severity, or contains unexpected data

### public void LogRequest_FailedRequest_CreatesErrorLogEntry
Verifies that a request that terminates with an error (e.g., gRPC status code other than OK) causes the service to create an error-level log entry. The test supplies a request context carrying a failure status and confirms that the resulting log entry is marked as an error and includes failure details.

- **Parameters:** none (test method)
- **Return value:** void
- **Throws:** assertion failures if the log entry is not at error level or omits error information

### public void LogRequest_SlowRequest_CreatesWarningLogEntry
Ensures that a request whose processing time exceeds the configured latency threshold triggers a warning-level log entry. The test constructs a request context with an artificially high elapsed time and asserts that the service produces a warning log entry highlighting the latency.

- **Parameters:** none (test method)
- **Return value:** void
- **Throws:** assertion failures if the log entry is not at warning level or lacks latency details

### public void LogRequest_LargeRequest_CreatesWarningLogEntry
Confirms that a request with a payload size above the configured threshold results in a warning log entry. The test provides a request context with an oversized body and checks that the service emits a warning entry that references the payload size.

- **Parameters:** none (test method)
- **Return value:** void
- **Throws:** assertion failures if the log entry is not at warning level or omits size information

### public void LogRequest_WithCacheHit_CreatesInfoLogEntry
Tests that when a request is served from cache (cache hit), the service creates an informational log entry indicating the cache hit. The test arranges a request context with a cache-hit flag set and verifies the log entry’s level and message content.

- **Parameters:** none (test method)
- **Return value:** void
- **Throws:** assertion failures if the log entry does not reflect a cache hit or has incorrect severity

### public void LogRequest_WithCacheMiss_CreatesInfoLogEntry
Tests that when a request results in a cache miss, the service creates an informational log entry noting the miss. The test supplies a request context with a cache-miss indicator and asserts the log entry’s properties.

- **Parameters:** none (test method)
- **Return value:** void
- **Throws:** assertion failures if the log entry does not reflect a cache miss or has incorrect severity

### public void LogRequest_WithRetries_CreatesWarningLogEntry
Validates that a request that required one or more retries before completion causes the service to emit a warning-level log entry. The test provides a request context with a retry count greater than zero and confirms that the log entry is a warning that includes retry information.

- **Parameters:** none (test method)
- **Return value:** void
- **Throws:** assertion failures if the log entry is not at warning level or lacks retry details

### public void DefaultConstructor_SetsDefaultValues
Verifies that instantiating `RequestLogService` using the default constructor initializes all internal fields to their expected default values (e.g., empty log store, default thresholds, no pre-existing entries). The test creates a new instance and asserts the initial state.

- **Parameters:** none (test method)
- **Return value:** void
- **Throws:** assertion failures if any internal field deviates from its documented default

## Usage

### Example 1: Running all tests in a CI pipeline
```csharp
using Xunit;

public class CiTestRunner
{
    public void RunRequestLogServiceTests()
    {
        var testSuite = new RequestLogServiceTests();

        // Constructor and basic behavior
        testSuite.DefaultConstructor_SetsDefaultValues();
        testSuite.LogRequest_ValidRequest_CreatesLogEntry();

        // Error and warning conditions
        testSuite.LogRequest_FailedRequest_CreatesErrorLogEntry();
        testSuite.LogRequest_SlowRequest_CreatesWarningLogEntry();
        testSuite.LogRequest_LargeRequest_CreatesWarningLogEntry();

        // Cache and retry scenarios
        testSuite.LogRequest_WithCacheHit_CreatesInfoLogEntry();
        testSuite.LogRequest_WithCacheMiss_CreatesInfoLogEntry();
        testSuite.LogRequest_WithRetries_CreatesWarningLogEntry();
    }
}
```

### Example 2: Selective execution during local development
```csharp
using Xunit;

public class LocalDebugRunner
{
    public void ValidateRetryAndCacheLogging()
    {
        var tests = new RequestLogServiceTests();

        // Focus on retry and cache paths only
        tests.LogRequest_WithRetries_CreatesWarningLogEntry();
        tests.LogRequest_WithCacheHit_CreatesInfoLogEntry();
        tests.LogRequest_WithCacheMiss_CreatesInfoLogEntry();
    }
}
```

## Notes

- **Edge cases:** Tests for slow and large requests rely on threshold values configured in the service under test. If thresholds are changed, these tests may fail until the arranged context values are adjusted accordingly. The retry test assumes a retry count greater than zero triggers a warning; a retry count of zero should not produce a warning, though this boundary is not explicitly tested by the listed members.
- **Thread safety:** These are unit tests and are not designed to be thread-safe. They should be executed sequentially within a single test runner context. The underlying `RequestLogService` may have its own thread-safety guarantees, but the test methods themselves do not exercise concurrent access patterns.
- **Test isolation:** Each test method is expected to operate on a fresh instance of `RequestLogService` or a cleanly reset state. Shared state between tests (e.g., static log stores) can cause cross-test interference and flaky results. The `DefaultConstructor_SetsDefaultValues` test serves as a baseline to confirm a clean starting state.
