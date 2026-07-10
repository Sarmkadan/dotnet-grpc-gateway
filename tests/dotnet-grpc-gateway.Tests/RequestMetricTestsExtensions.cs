using System;
using System.Collections.Generic;
using DotNetGrpcGateway.Domain;

namespace DotNetGrpcGateway.Tests
{
    public static class RequestMetricTestsExtensions
    {
        /// <summary>
        /// Creates a valid RequestMetric instance with default values for testing purposes.
        /// This extension method provides a convenient way to create test metrics without
        /// manually constructing RequestMetric objects in test setup.
        /// </summary>
        /// <param name="tests">The RequestMetricTests instance (unused, for extension method syntax)</param>
        /// <param name="serviceName">Service name to set</param>
        /// <param name="methodName">Method name to set</param>
        /// <param name="clientIpAddress">Client IP address to set</param>
        /// <returns>A new RequestMetric instance ready for testing</returns>
        public static RequestMetric CreateValidMetric(this RequestMetricTests tests,
            string serviceName = "TestService",
            string methodName = "TestMethod",
            string clientIpAddress = "192.168.1.100")
        {
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
        /// Creates an invalid RequestMetric instance with empty service name for negative testing.
        /// Useful for testing validation error handling scenarios.
        /// </summary>
        /// <param name="tests">The RequestMetricTests instance (unused, for extension method syntax)</param>
        /// <returns>A new RequestMetric instance with empty service name</returns>
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
        /// Creates a RequestMetric instance with negative duration for testing validation.
        /// Useful for testing duration validation scenarios.
        /// </summary>
        /// <param name="tests">The RequestMetricTests instance (unused, for extension method syntax)</param>
        /// <param name="durationMs">Negative duration value</param>
        /// <returns>A new RequestMetric instance with negative duration</returns>
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
        /// Creates a collection of RequestMetric instances with varying durations.
        /// Useful for performance testing and benchmarking scenarios.
        /// </summary>
        /// <param name="tests">The RequestMetricTests instance (unused, for extension method syntax)</param>
        /// <param name="count">Number of metrics to create</param>
        /// <param name="startDurationMs">Starting duration in milliseconds</param>
        /// <param name="durationIncrementMs">Increment between each metric</param>
        /// <returns>IEnumerable of RequestMetric instances</returns>
        public static IEnumerable<RequestMetric> CreateDurationSequence(this RequestMetricTests tests,
            int count,
            double startDurationMs = 0,
            double durationIncrementMs = 100)
        {
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
        /// Creates a RequestMetric instance that represents a slow request.
        /// Useful for testing slow request detection logic.
        /// </summary>
        /// <param name="tests">The RequestMetricTests instance (unused, for extension method syntax)</param>
        /// <param name="thresholdMs">Threshold that defines a slow request</param>
        /// <returns>A new RequestMetric instance with duration above threshold</returns>
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
        /// Creates a RequestMetric instance with error state for testing error handling.
        /// Useful for testing error recovery and monitoring scenarios.
        /// </summary>
        /// <param name="tests">The RequestMetricTests instance (unused, for extension method syntax)</param>
        /// <param name="errorMessage">Error message to set</param>
        /// <param name="includeStackTrace">Whether to include stack trace</param>
        /// <returns>A new RequestMetric instance with error state</returns>
        public static RequestMetric CreateErrorMetric(this RequestMetricTests tests,
            string errorMessage = "Connection timeout",
            bool includeStackTrace = true)
        {
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
        /// Creates a RequestMetric instance with retry tracking enabled.
        /// Useful for testing retry logic and circuit breaker patterns.
        /// </summary>
        /// <param name="tests">The RequestMetricTests instance (unused, for extension method syntax)</param>
        /// <param name="retryCount">Number of retries to simulate</param>
        /// <returns>A new RequestMetric instance with retry tracking</returns>
        public static RequestMetric CreateRetryMetric(this RequestMetricTests tests, int retryCount = 3)
        {
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