using System;
using System.Collections.Generic;
using System.Linq;
using DotNetGrpcGateway.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetGrpcGateway.Tests
{
    public class StructuredLoggerValidationTests
    {
        private readonly Mock<ILogger> _mockLogger;

        public StructuredLoggerValidationTests()
        {
            _mockLogger = new Mock<ILogger>();
        }

        #region ValidateLogRequestStart Tests

        [Fact]
        public void ValidateLogRequestStart_WithValidParameters_ReturnsEmptyList()
        {
            // Act
            var result = StructuredLoggerValidation.ValidateLogRequestStart(
                _mockLogger.Object,
                "req123",
                "/api/test",
                "GET",
                "192.168.1.1");

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void ValidateLogRequestStart_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => StructuredLoggerValidation.ValidateLogRequestStart(
                null!,
                "req123",
                "/api/test",
                "GET",
                "192.168.1.1");

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }

        [Fact]
        public void ValidateLogRequestStart_WithNullRequestId_ThrowsArgumentException()
        {
            // Act
            Action act = () => StructuredLoggerValidation.ValidateLogRequestStart(
                _mockLogger.Object,
                null!,
                "/api/test",
                "GET",
                "192.168.1.1");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithParameterName("requestId");
        }

        [Fact]
        public void ValidateLogRequestStart_WithEmptyRequestId_ThrowsArgumentException()
        {
            // Act
            Action act = () => StructuredLoggerValidation.ValidateLogRequestStart(
                _mockLogger.Object,
                string.Empty,
                "/api/test",
                "GET",
                "192.168.1.1");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithParameterName("requestId");
        }

        [Fact]
        public void ValidateLogRequestStart_WithWhitespaceRequestId_ThrowsArgumentException()
        {
            // Act
            Action act = () => StructuredLoggerValidation.ValidateLogRequestStart(
                _mockLogger.Object,
                "   ",
                "/api/test",
                "GET",
                "192.168.1.1");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithParameterName("requestId");
        }

        [Fact]
        public void ValidateLogRequestStart_WithRequestIdOver100Chars_ReturnsError()
        {
            // Arrange
            var longRequestId = new string('a', 101);

            // Act
            var result = StructuredLoggerValidation.ValidateLogRequestStart(
                _mockLogger.Object,
                longRequestId,
                "/api/test",
                "GET",
                "192.168.1.1");

            // Assert
            result.Should().ContainSingle()
                .Which.Should().Be("RequestId exceeds maximum length of 100 characters");
        }

        [Fact]
        public void ValidateLogRequestStart_WithPathOver500Chars_ReturnsError()
        {
            // Arrange
            var longPath = new string('a', 501);

            // Act
            var result = StructuredLoggerValidation.ValidateLogRequestStart(
                _mockLogger.Object,
                "req123",
                longPath,
                "GET",
                "192.168.1.1");

            // Assert
            result.Should().ContainSingle()
                .Which.Should().Be("Path exceeds maximum length of 500 characters");
        }

        [Fact]
        public void ValidateLogRequestStart_WithMethodOver20Chars_ReturnsError()
        {
            // Arrange
            var longMethod = new string('a', 21);

            // Act
            var result = StructuredLoggerValidation.ValidateLogRequestStart(
                _mockLogger.Object,
                "req123",
                "/api/test",
                longMethod,
                "192.168.1.1");

            // Assert
            result.Should().ContainSingle()
                .Which.Should().Be("Method exceeds maximum length of 20 characters");
        }

        [Fact]
        public void ValidateLogRequestStart_WithClientIpOver45Chars_ReturnsError()
        {
            // Arrange
            var longClientIp = new string('a', 46);

            // Act
            var result = StructuredLoggerValidation.ValidateLogRequestStart(
                _mockLogger.Object,
                "req123",
                "/api/test",
                "GET",
                longClientIp);

            // Assert
            result.Should().ContainSingle()
                .Which.Should().Be("ClientIp exceeds maximum length of 45 characters");
        }

        [Fact]
        public void ValidateLogRequestStart_WithMultipleValidationErrors_ReturnsAllErrors()
        {
            // Arrange
            var longRequestId = new string('a', 101);
            var longPath = new string('a', 501);

            // Act
            var result = StructuredLoggerValidation.ValidateLogRequestStart(
                _mockLogger.Object,
                longRequestId,
                longPath,
                "GET",
                "192.168.1.1");

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain("RequestId exceeds maximum length of 100 characters");
            result.Should().Contain("Path exceeds maximum length of 500 characters");
        }

        #endregion

        #region ValidateLogRequestComplete Tests

        [Fact]
        public void ValidateLogRequestComplete_WithValidParameters_ReturnsEmptyList()
        {
            // Act
            var result = StructuredLoggerValidation.ValidateLogRequestComplete(
                _mockLogger.Object,
                "req123",
                "/api/test",
                200,
                150);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void ValidateLogRequestComplete_WithStatusCodeBelow100_ReturnsError()
        {
            // Act
            var result = StructuredLoggerValidation.ValidateLogRequestComplete(
                _mockLogger.Object,
                "req123",
                "/api/test",
                99,
                150);

            // Assert
            result.Should().ContainSingle()
                .Which.Should().Be("StatusCode must be between 100 and 999");
        }

        [Fact]
        public void ValidateLogRequestComplete_WithStatusCodeAbove999_ReturnsError()
        {
            // Act
            var result = StructuredLoggerValidation.ValidateLogRequestComplete(
                _mockLogger.Object,
                "req123",
                "/api/test",
                1000,
                150);

            // Assert
            result.Should().ContainSingle()
                .Which.Should().Be("StatusCode must be between 100 and 999");
        }

        [Fact]
        public void ValidateLogRequestComplete_WithNegativeDurationMs_ReturnsError()
        {
            // Act
            var result = StructuredLoggerValidation.ValidateLogRequestComplete(
                _mockLogger.Object,
                "req123",
                "/api/test",
                200,
                -1);

            // Assert
            result.Should().ContainSingle()
                .Which.Should().Be("DurationMs cannot be negative");
        }

        #endregion

        #region ValidateLogServiceDiscovery Tests

        [Fact]
        public void ValidateLogServiceDiscovery_WithValidParameters_ReturnsEmptyList()
        {
            // Act
            var result = StructuredLoggerValidation.ValidateLogServiceDiscovery(
                _mockLogger.Object,
                1,
                "TestService",
                true);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void ValidateLogServiceDiscovery_WithNonPositiveServiceId_ReturnsError()
        {
            // Act
            var result = StructuredLoggerValidation.ValidateLogServiceDiscovery(
                _mockLogger.Object,
                0,
                "TestService",
                true);

            // Assert
            result.Should().ContainSingle()
                .Which.Should().Be("ServiceId must be a positive integer");
        }

        [Fact]
        public void ValidateLogServiceDiscovery_WithServiceNameOver100Chars_ReturnsError()
        {
            // Arrange
            var longServiceName = new string('a', 101);

            // Act
            var result = StructuredLoggerValidation.ValidateLogServiceDiscovery(
                _mockLogger.Object,
                1,
                longServiceName,
                true);

            // Assert
            result.Should().ContainSingle()
                .Which.Should().Be("ServiceName exceeds maximum length of 100 characters");
        }

        #endregion

        #region ValidateLogCacheOperation Tests

        [Fact]
        public void ValidateLogCacheOperation_WithValidParameters_ReturnsEmptyList()
        {
            // Act
            var result = StructuredLoggerValidation.ValidateLogCacheOperation(
                _mockLogger.Object,
                "GET",
                "cache_key_123",
                true);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void ValidateLogCacheOperation_WithOperationOver50Chars_ReturnsError()
        {
            // Arrange
            var longOperation = new string('a', 51);

            // Act
            var result = StructuredLoggerValidation.ValidateLogCacheOperation(
                _mockLogger.Object,
                longOperation,
                "cache_key_123",
                true);

            // Assert
            result.Should().ContainSingle()
                .Which.Should().Be("Operation exceeds maximum length of 50 characters");
        }

        [Fact]
        public void ValidateLogCacheOperation_WithKeyOver200Chars_ReturnsError()
        {
            // Arrange
            var longKey = new string('a', 201);

            // Act
            var result = StructuredLoggerValidation.ValidateLogCacheOperation(
                _mockLogger.Object,
                "GET",
                longKey,
                true);

            // Assert
            result.Should().ContainSingle()
                .Which.Should().Be("Key exceeds maximum length of 200 characters");
        }

        #endregion

        #region ValidateLogRouteResolution Tests

        [Fact]
        public void ValidateLogRouteResolution_WithValidParameters_ReturnsEmptyList()
        {
            // Act
            var result = StructuredLoggerValidation.ValidateLogRouteResolution(
                _mockLogger.Object,
                "/api/users/{id}",
                "/api/users/{id}",
                123);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void ValidateLogRouteResolution_WithPathOver500Chars_ReturnsError()
        {
            // Arrange
            var longPath = new string('a', 501);

            // Act
            var result = StructuredLoggerValidation.ValidateLogRouteResolution(
                _mockLogger.Object,
                longPath,
                "/api/users/{id}",
                123);

            // Assert
            result.Should().ContainSingle()
                .Which.Should().Be("Path exceeds maximum length of 500 characters");
        }

        [Fact]
        public void ValidateLogRouteResolution_WithRoutePatternOver200Chars_ReturnsError()
        {
            // Arrange
            var longRoutePattern = new string('a', 201);

            // Act
            var result = StructuredLoggerValidation.ValidateLogRouteResolution(
                _mockLogger.Object,
                "/api/users/{id}",
                longRoutePattern,
                123);

            // Assert
            result.Should().ContainSingle()
                .Which.Should().Be("RoutePattern exceeds maximum length of 200 characters");
        }

        [Fact]
        public void ValidateLogRouteResolution_WithNonPositiveTargetServiceId_ReturnsError()
        {
            // Act
            var result = StructuredLoggerValidation.ValidateLogRouteResolution(
                _mockLogger.Object,
                "/api/users/{id}",
                "/api/users/{id}",
                0);

            // Assert
            result.Should().ContainSingle()
                .Which.Should().Be("TargetServiceId must be a positive integer");
        }

        #endregion

        #region ValidateLogRateLimit Tests

        [Fact]
        public void ValidateLogRateLimit_WithValidParameters_ReturnsEmptyList()
        {
            // Act
            var result = StructuredLoggerValidation.ValidateLogRateLimit(
                _mockLogger.Object,
                "192.168.1.1",
                "/api/test",
                100);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void ValidateLogRateLimit_WithClientIpOver45Chars_ReturnsError()
        {
            // Arrange
            var longClientIp = new string('a', 46);

            // Act
            var result = StructuredLoggerValidation.ValidateLogRateLimit(
                _mockLogger.Object,
                longClientIp,
                "/api/test",
                100);

            // Assert
            result.Should().ContainSingle()
                .Which.Should().Be("ClientIp exceeds maximum length of 45 characters");
        }

        [Fact]
        public void ValidateLogRateLimit_WithPathOver500Chars_ReturnsError()
        {
            // Arrange
            var longPath = new string('a', 501);

            // Act
            var result = StructuredLoggerValidation.ValidateLogRateLimit(
                _mockLogger.Object,
                "192.168.1.1",
                longPath,
                100);

            // Assert
            result.Should().ContainSingle()
                .Which.Should().Be("Path exceeds maximum length of 500 characters");
        }

        [Fact]
        public void ValidateLogRateLimit_WithNonPositiveLimit_ReturnsError()
        {
            // Act
            var result = StructuredLoggerValidation.ValidateLogRateLimit(
                _mockLogger.Object,
                "192.168.1.1",
                "/api/test",
                0);

            // Assert
            result.Should().ContainSingle()
                .Which.Should().Be("Limit must be a positive integer");
        }

        #endregion

        #region ValidateLogAuthentication Tests

        [Fact]
        public void ValidateLogAuthentication_WithValidParameters_ReturnsEmptyList()
        {
            // Act
            var result = StructuredLoggerValidation.ValidateLogAuthentication(
                _mockLogger.Object,
                "user123",
                true,
                "Success");

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void ValidateLogAuthentication_WithNullUserId_ReturnsEmptyList()
        {
            // Act
            var result = StructuredLoggerValidation.ValidateLogAuthentication(
                _mockLogger.Object,
                null,
                false,
                "Invalid credentials");

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void ValidateLogAuthentication_WithUserIdOver50Chars_ReturnsError()
        {
            // Arrange
            var longUserId = new string('a', 51);

            // Act
            var result = StructuredLoggerValidation.ValidateLogAuthentication(
                _mockLogger.Object,
                longUserId,
                false,
                "Invalid credentials");

            // Assert
            result.Should().ContainSingle()
                .Which.Should().Be("UserId exceeds maximum length of 50 characters");
        }

        [Fact]
        public void ValidateLogAuthentication_WithReasonOver200Chars_ReturnsError()
        {
            // Arrange
            var longReason = new string('a', 201);

            // Act
            var result = StructuredLoggerValidation.ValidateLogAuthentication(
                _mockLogger.Object,
                "user123",
                false,
                longReason);

            // Assert
            result.Should().ContainSingle()
                .Which.Should().Be("Reason exceeds maximum length of 200 characters");
        }

        #endregion

        #region ValidateLogCriticalError Tests

        [Fact]
        public void ValidateLogCriticalError_WithValidParameters_ReturnsEmptyList()
        {
            // Act
            var result = StructuredLoggerValidation.ValidateLogCriticalError(
                _mockLogger.Object,
                new InvalidOperationException("Test error"),
                "Processing request",
                null);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void ValidateLogCriticalError_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => StructuredLoggerValidation.ValidateLogCriticalError(
                null!,
                new InvalidOperationException("Test error"),
                "Processing request",
                null);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }

        [Fact]
        public void ValidateLogCriticalError_WithNullException_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => StructuredLoggerValidation.ValidateLogCriticalError(
                _mockLogger.Object,
                null!,
                "Processing request",
                null);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("ex");
        }

        [Fact]
        public void ValidateLogCriticalError_WithNullContext_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => StructuredLoggerValidation.ValidateLogCriticalError(
                _mockLogger.Object,
                new InvalidOperationException("Test error"),
                null!,
                null);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("context");
        }

        [Fact]
        public void ValidateLogCriticalError_WithContextOver200Chars_ReturnsError()
        {
            // Arrange
            var longContext = new string('a', 201);

            // Act
            var result = StructuredLoggerValidation.ValidateLogCriticalError(
                _mockLogger.Object,
                new InvalidOperationException("Test error"),
                longContext,
                null);

            // Assert
            result.Should().ContainSingle()
                .Which.Should().Be("Context exceeds maximum length of 200 characters");
        }

        [Fact]
        public void ValidateLogCriticalError_WithAdditionalDataOver20Entries_ReturnsError()
        {
            // Arrange
            var additionalData = Enumerable.Range(0, 21)
                .ToDictionary(i => $"Key{i}", i => (object)$"Value{i}");

            // Act
            var result = StructuredLoggerValidation.ValidateLogCriticalError(
                _mockLogger.Object,
                new InvalidOperationException("Test error"),
                "Processing request",
                additionalData);

            // Assert
            result.Should().ContainSingle()
                .Which.Should().Be("AdditionalData contains more than 20 entries");
        }

        [Fact]
        public void ValidateLogCriticalError_WithAdditionalDataKeyOver100Chars_ReturnsError()
        {
            // Arrange
            var additionalData = new Dictionary<string, object>
            {
                { new string('a', 101), "value" }
            };

            // Act
            var result = StructuredLoggerValidation.ValidateLogCriticalError(
                _mockLogger.Object,
                new InvalidOperationException("Test error"),
                "Processing request",
                additionalData);

            // Assert
            result.Should().ContainSingle()
                .Which.Should().Contain("AdditionalData key '")
                .And.Contain("exceeds maximum length of 100 characters");
        }

        [Fact]
        public void ValidateLogCriticalError_WithAdditionalDataValueOver500Chars_ReturnsError()
        {
            // Arrange
            var additionalData = new Dictionary<string, object>
            {
                { "key", new string('a', 501) }
            };

            // Act
            var result = StructuredLoggerValidation.ValidateLogCriticalError(
                _mockLogger.Object,
                new InvalidOperationException("Test error"),
                "Processing request",
                additionalData);

            // Assert
            result.Should().ContainSingle()
                .Which.Should().Contain("AdditionalData value for key 'key'")
                .And.Contain("exceeds maximum length of 500 characters");
        }

        #endregion

        #region ValidateLogPerformanceMetrics Tests

        [Fact]
        public void ValidateLogPerformanceMetrics_WithValidParameters_ReturnsEmptyList()
        {
            // Act
            var result = StructuredLoggerValidation.ValidateLogPerformanceMetrics(
                _mockLogger.Object,
                "DatabaseQuery",
                150,
                1000);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void ValidateLogPerformanceMetrics_WithOperationOver100Chars_ReturnsError()
        {
            // Arrange
            var longOperation = new string('a', 101);

            // Act
            var result = StructuredLoggerValidation.ValidateLogPerformanceMetrics(
                _mockLogger.Object,
                longOperation,
                150,
                1000);

            // Assert
            result.Should().ContainSingle()
                .Which.Should().Be("Operation exceeds maximum length of 100 characters");
        }

        [Fact]
        public void ValidateLogPerformanceMetrics_WithNegativeDurationMs_ReturnsError()
        {
            // Act
            var result = StructuredLoggerValidation.ValidateLogPerformanceMetrics(
                _mockLogger.Object,
                "DatabaseQuery",
                -1,
                1000);

            // Assert
            result.Should().ContainSingle()
                .Which.Should().Be("DurationMs cannot be negative");
        }

        [Fact]
        public void ValidateLogPerformanceMetrics_WithItemCountZero_ReturnsError()
        {
            // Act
            var result = StructuredLoggerValidation.ValidateLogPerformanceMetrics(
                _mockLogger.Object,
                "DatabaseQuery",
                150,
                0);

            // Assert
            result.Should().ContainSingle()
                .Which.Should().Be("ItemCount must be a positive integer between 1 and 1,000,000");
        }

        [Fact]
        public void ValidateLogPerformanceMetrics_WithItemCountOverOneMillion_ReturnsError()
        {
            // Act
            var result = StructuredLoggerValidation.ValidateLogPerformanceMetrics(
                _mockLogger.Object,
                "DatabaseQuery",
                150,
                1_000_001);

            // Assert
            result.Should().ContainSingle()
                .Which.Should().Be("ItemCount must be a positive integer between 1 and 1,000,000");
        }

        [Fact]
        public void ValidateLogPerformanceMetrics_WithNullItemCount_ReturnsEmptyList()
        {
            // Act
            var result = StructuredLoggerValidation.ValidateLogPerformanceMetrics(
                _mockLogger.Object,
                "DatabaseQuery",
                150,
                null);

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region IsValidLogRequestStart Tests

        [Fact]
        public void IsValidLogRequestStart_WithValidParameters_ReturnsTrue()
        {
            // Act
            var result = StructuredLoggerValidation.IsValidLogRequestStart(
                _mockLogger.Object,
                "req123",
                "/api/test",
                "GET",
                "192.168.1.1");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsValidLogRequestStart_WithInvalidRequestId_ReturnsFalse()
        {
            // Act
            var result = StructuredLoggerValidation.IsValidLogRequestStart(
                _mockLogger.Object,
                new string('a', 101), // Too long
                "/api/test",
                "GET",
                "192.168.1.1");

            // Assert
            result.Should().BeFalse();
        }

        #endregion
    }
}