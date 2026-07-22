#nullable enable
using DotNetGrpcGateway.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace DotNetGrpcGateway.Tests
{
    public class StructuredLoggerTests
    {
        [Fact]
        public void LogRequestStart_HappyPath_LogsRequestStart()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var requestId = "requestId";
            var path = "/path";
            var method = "GET";
            var clientIp = "192.168.1.1";

            // Act
            StructuredLogger.LogRequestStart(loggerMock.Object, requestId, path, method, clientIp);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), requestId, path, method, clientIp), Times.Once);
        }

        [Fact]
        public void LogRequestComplete_HappyPath_LogsRequestComplete()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var requestId = "requestId";
            var path = "/path";
            var statusCode = 200;
            var durationMs = 100;

            // Act
            StructuredLogger.LogRequestComplete(loggerMock.Object, requestId, path, statusCode, durationMs);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), requestId, path, statusCode, durationMs), Times.Once);
        }

        [Fact]
        public void LogServiceDiscovery_HappyPath_LogsServiceDiscovery()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var serviceId = 1;
            var serviceName = "serviceName";
            var healthy = true;

            // Act
            StructuredLogger.LogServiceDiscovery(loggerMock.Object, serviceId, serviceName, healthy);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), serviceId, serviceName, healthy), Times.Once);
        }

        [Fact]
        public void LogCacheOperation_HappyPath_LogsCacheOperation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var operation = "operation";
            var key = "key";
            var hit = true;

            // Act
            StructuredLogger.LogCacheOperation(loggerMock.Object, operation, key, hit);

            // Assert
            loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), operation, key, hit), Times.Once);
        }

        [Fact]
        public void LogRouteResolution_HappyPath_LogsRouteResolution()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var path = "/path";
            var routePattern = "routePattern";
            var targetServiceId = 1;

            // Act
            StructuredLogger.LogRouteResolution(loggerMock.Object, path, routePattern, targetServiceId);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), path, routePattern, targetServiceId), Times.Once);
        }

        [Fact]
        public void LogRateLimit_HappyPath_LogsRateLimit()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clientIp = "192.168.1.1";
            var path = "/path";
            var limit = 100;

            // Act
            StructuredLogger.LogRateLimit(loggerMock.Object, clientIp, path, limit);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), clientIp, path, limit), Times.Once);
        }

        [Fact]
        public void LogAuthentication_HappyPath_LogsAuthentication()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var userId = "userId";
            var success = true;

            // Act
            StructuredLogger.LogAuthentication(loggerMock.Object, userId, success);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), userId, success, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void LogCriticalError_HappyPath_LogsCriticalError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var ex = new Exception("exception");
            var context = "context";

            // Act
            StructuredLogger.LogCriticalError(loggerMock.Object, ex, context);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), context, It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void LogPerformanceMetrics_HappyPath_LogsPerformanceMetrics()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var operation = "operation";
            var durationMs = 100;

            // Act
            StructuredLogger.LogPerformanceMetrics(loggerMock.Object, operation, durationMs);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), operation, durationMs), Times.Once);
        }

        [Fact]
        public void LogRequestStart_NullLogger_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => StructuredLogger.LogRequestStart(null, "requestId", "/path", "GET", "192.168.1.1"));
        }

        [Fact]
        public void LogRequestComplete_NullLogger_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => StructuredLogger.LogRequestComplete(null, "requestId", "/path", 200, 100));
        }
    }
}
