# GatewayStatisticsTests

Unit tests for the `GatewayStatistics` class, verifying validation logic and metrics aggregation behavior for gRPC gateway statistics. The test suite ensures correct handling of request counts, success rates, response times, service health, and caching metrics.

## API

### `void Validate_ValidStatistics_DoesNotThrow()`
Verifies that a `GatewayStatistics` object with valid metrics (non-negative counts, success rate between 0 and 100, and logical response time ranges) does not throw exceptions during validation.

### `void Validate_NegativeTotalRequests_ThrowsInvalidOperationException()`
Ensures that setting a negative `TotalRequests` value throws an `InvalidOperationException` during validation.

### `void Validate_NegativeSuccessfulRequests_ThrowsInvalidOperationException()`
Ensures that setting a negative `SuccessfulRequests` value throws an `InvalidOperationException` during validation.

### `void Validate_NegativeFailedRequests_ThrowsInvalidOperationException()`
Ensures that setting a negative `FailedRequests` value throws an `InvalidOperationException` during validation.

### `void Validate_SuccessRateBelowZero_ThrowsInvalidOperationException()`
Ensures that setting a success rate below 0 throws an `InvalidOperationException` during validation.

### `void Validate_SuccessRateAbove100_ThrowsInvalidOperationException()`
Ensures that setting a success rate above 100 throws an `InvalidOperationException` during validation.

### `void Validate_NegativeAverageResponseTime_ThrowsInvalidOperationException()`
Ensures that setting a negative `AverageResponseTime` value throws an `InvalidOperationException` during validation.

### `void Validate_MaxLessThanMinResponseTime_ThrowsInvalidOperationException()`
Ensures that setting a `MaxResponseTime` value less than `MinResponseTime` throws an `InvalidOperationException` during validation.

### `void RecordRequest_FirstRequest_SetsInitialValues()`
Validates that recording the first request initializes all statistics fields with correct default and calculated values.

### `void RecordRequest_MultipleRequests_CalculatesCorrectAverages()`
Ensures that after recording multiple requests, the average response time, success rate, and other metrics are computed accurately.

### `void RecordRequest_ZeroRequests_DoesNotCalculateAverage()`
Confirms that no average metrics are calculated when no requests have been recorded.

### `void RecordServiceRequest_AddsToServiceDictionary()`
Verifies that calling `RecordServiceRequest` increments the count for the specified service in the internal service dictionary.

### `void RecordMethodCall_AddsToMethodDictionary()`
Verifies that calling `RecordMethodCall` increments the count for the specified method in the internal method dictionary.

### `void RecordError_AddsToErrorDictionary()`
Ensures that calling `RecordError` increments the error count for the specified service in the internal error dictionary.

### `void RecordCacheHit_UpdatesCacheMetrics()`
Validates that recording a cache hit updates the cache hit count and cache hit rate accordingly.

### `void RecordCacheHit_NoRequests_CalculatesZeroRate()`
Confirms that the cache hit rate is reported as zero when no requests have been recorded.

### `void UpdateServiceHealth_SetsHealthCounts()`
Ensures that calling `UpdateServiceHealth` updates the health status counts (healthy, degraded, unhealthy) correctly.

### `void DefaultConstructor_SetsDefaultValues()`
Validates that the default constructor initializes all statistics fields with expected default values (e.g., zero counts, null or empty collections).

## Usage

### Example 1: Validating Statistics
