using System;
using System.Collections.Generic;
using DotNetGrpcGateway.Domain;

namespace DotNetGrpcGateway.Tests
{
    /// <summary>
    /// Provides extension methods for creating test instances of <see cref="RequestMetric"/> for unit testing scenarios.
    /// </summary>
    public static class RequestMetricTestsExtensions
    {
        /// <summary>
        /// Creates a valid <see cref="RequestMetric"/> instance with default values for testing purposes.
        /// This extension method provides a convenient way to create test metrics without
        /// manually constructing <see cref="RequestMetric"/> objects in test setup.
        /// </summary>
        /// <param name="tests">The <see cref="RequestMetricTests"/> instance (unused, for extension method syntax).</param>
        /// <param name="serviceName">Service name to set. Cannot be null or empty.</param>
        /// <param name="methodName">Method name to set. Cannot be null or empty.</param>
        /// <param name="clientIpAddress">Client IP address to set. Cannot be null or empty.</param>
        /// <returns>A new <see cref="RequestMetric"/> instance ready for testing.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="serviceName"/>, <paramref name="methodName"/>, or <paramref name="clientIpAddress"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="serviceName"/>, <paramref name="methodName"/>, or <paramref name="clientIpAddress"/> is empty.</exception>
        public static RequestMetric CreateValidMetric(
            this RequestMetricTests tests,
            string serviceName = "TestService",
            string methodName = "TestMethod",
            string clientIpAddress = "192.168.1.100")
        {
            ArgumentNullException.ThrowIfNull(serviceName);
            ArgumentNullException.ThrowIfNull(methodName);
            ArgumentNullException.ThrowIfNull(clientIpAddress);

            ArgumentException.ThrowIfNullOrEmpty(serviceName);
            ArgumentException.ThrowIfNullOrEmpty(methodName);
            ArgumentException.ThrowIfNullOrEmpty(clientIpAddress);

            return new RequestMetric
            {
                ServiceName = serviceName,
                MethodName = methodName,
                ClientIpAddress = clientIpAddress,
                DurationMs = 150.5,
                RequestSizeBytes = 1024,
                ResponseSizeBytes = 2048,
                HttpStatusCode = 200,
                IsSuccessful = true
            };
        }

        /// <summary>
        /// Creates an invalid <see cref="RequestMetric"/> instance with empty service name for negative testing.
        /// Useful for testing validation error handling scenarios.
        /// </summary>
        /// <param name="tests">The <see cref="RequestMetricTests"/> instance (unused, for extension method syntax).</param>
        /// <returns>A new <see cref="RequestMetric"/> instance with empty service name.</returns>
        public static RequestMetric CreateInvalidServiceNameMetric(this RequestMetricTests tests)
        {
            return new RequestMetric
            {
                ServiceName = "",
                MethodName = "GetUser",
                ClientIpAddress = "192.168.1.100"
            };
        }

        /// <summary>
        /// Creates a <see cref="RequestMetric"/> instance with negative duration for testing validation.
        /// Useful for testing duration validation scenarios.
        /// </summary>
        /// <param name="tests">The <see cref="RequestMetricTests"/> instance (unused, for extension method syntax).</param>
        /// <param name="durationMs">Negative duration value.</param>
        /// <returns>A new <see cref="RequestMetric"/> instance with negative duration.</returns>
        public static RequestMetric CreateNegativeDurationMetric(this RequestMetricTests tests, double durationMs = -100)
        {
            return new RequestMetric
            {
                ServiceName = "TestService",
                MethodName = "GetUser",
                ClientIpAddress = "192.168.1.100",
                DurationMs = durationMs
            };
        }

        /// <summary>
        /// Creates a collection of <see cref="RequestMetric"/> instances with varying durations.
        /// Useful for performance testing and benchmarking scenarios.
        /// </summary>
        /// <param name="tests">The <see cref="RequestMetricTests"/> instance (unused, for extension method syntax).</param>
        /// <param name="count">Number of metrics to create. Must be non-negative.</param>
        /// <param name="startDurationMs">Starting duration in milliseconds.</param>
        /// <param name="durationIncrementMs">Increment between each metric.</param>
        /// <returns>IEnumerable of <see cref="RequestMetric"/> instances.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="count"/> is negative.</exception>
        public static IEnumerable<RequestMetric> CreateDurationSequence(
            this RequestMetricTests tests,
            int count,
            double startDurationMs = 0,
            double durationIncrementMs = 100)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be non-negative.");
            }

            for (int i = 0; i < count; i++)
            {
                yield return new RequestMetric
                {
                    ServiceName = "PerformanceTestService",
                    MethodName = "PerformanceTestMethod",
                    ClientIpAddress = "192.168.1.100",
                    DurationMs = startDurationMs + (i * durationIncrementMs),
                    RequestSizeBytes = 1024 + (i * 512),
                    ResponseSizeBytes = 2048 + (i * 1024)
                };
            }
        }

        /// <summary>
        /// Creates a <see cref="RequestMetric"/> instance that represents a slow request.
        /// Useful for testing slow request detection logic.
        /// </summary>
        /// <param name="tests">The <see cref="RequestMetricTests"/> instance (unused, for extension method syntax).</param>
        /// <param name="thresholdMs">Threshold that defines a slow request.</param>
        /// <returns>A new <see cref="RequestMetric"/> instance with duration above threshold.</returns>
        public static RequestMetric CreateSlowRequest(this RequestMetricTests tests, double thresholdMs = 1000)
        {
            return new RequestMetric
            {
                ServiceName = "SlowService",
                MethodName = "SlowMethod",
                ClientIpAddress = "192.168.1.100",
                DurationMs = thresholdMs + 500,
                RequestSizeBytes = 2048,
                ResponseSizeBytes = 4096
            };
        }

        /// <summary>
        /// Creates a <see cref="RequestMetric"/> instance with error state for testing error handling.
        /// Useful for testing error recovery and monitoring scenarios.
        /// </summary>
        /// <param name="tests">The <see cref="RequestMetricTests"/> instance (unused, for extension method syntax).</param>
        /// <param name="errorMessage">Error message to set. Cannot be null or empty.</param>
        /// <param name="includeStackTrace">Whether to include stack trace.</param>
        /// <returns>A new <see cref="RequestMetric"/> instance with error state.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="errorMessage"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="errorMessage"/> is empty.</exception>
        public static RequestMetric CreateErrorMetric(
            this RequestMetricTests tests,
            string errorMessage = "Connection timeout",
            bool includeStackTrace = true)
        {
            ArgumentNullException.ThrowIfNull(errorMessage);
            ArgumentException.ThrowIfNullOrEmpty(errorMessage);

            var metric = new RequestMetric
            {
                ServiceName = "ErrorService",
                MethodName = "ErrorMethod",
                ClientIpAddress = "192.168.1.100",
                DurationMs = 500,
                RequestSizeBytes = 1024,
                ResponseSizeBytes = 2048,
                IsSuccessful = false
            };

            if (includeStackTrace)
            {
                metric.RecordError(errorMessage, "at line 42 in ErrorService.cs");
            }
            else
            {
                metric.RecordError(errorMessage);
            }

            return metric;
        }

        /// <summary>
        /// Creates a <see cref="RequestMetric"/> instance with retry tracking enabled.
        /// Useful for testing retry logic and circuit breaker patterns.
        /// </summary>
        /// <param name="tests">The <see cref="RequestMetricTests"/> instance (unused, for extension method syntax).</param>
        /// <param name="retryCount">Number of retries to simulate. Must be non-negative.</param>
        /// <returns>A new <see cref="RequestMetric"/> instance with retry tracking.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="retryCount"/> is negative.</exception>
        public static RequestMetric CreateRetryMetric(this RequestMetricTests tests, int retryCount = 3)
        {
            if (retryCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(retryCount), "Retry count must be non-negative.");
            }

            var metric = new RequestMetric
            {
                ServiceName = "RetryService",
                MethodName = "RetryMethod",
                ClientIpAddress = "192.168.1.100",
                DurationMs = 250,
                RequestSizeBytes = 1024,
                ResponseSizeBytes = 2048,
                IsSuccessful = true
            };

            for (int i = 0; i < retryCount; i++)
            {
                metric.RecordRetry();
            }

            return metric;
        }
    }
}