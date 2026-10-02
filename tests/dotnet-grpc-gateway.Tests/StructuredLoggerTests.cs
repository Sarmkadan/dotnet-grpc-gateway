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
            var loggerMock = new Mock<ILogger>();
            StructuredLogger.LogRequestStart(loggerMock.Object, "requestId", "/path", "GET", "192.168.1.1");

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request started")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogRequestComplete_HappyPath_LogsRequestComplete()
        {
            var loggerMock = new Mock<ILogger>();
            StructuredLogger.LogRequestComplete(loggerMock.Object, "requestId", "/path", 200, 100);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request completed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogServiceDiscovery_HappyPath_LogsServiceDiscovery()
        {
            var loggerMock = new Mock<ILogger>();
            StructuredLogger.LogServiceDiscovery(loggerMock.Object, 1, "serviceName", true);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Service discovery")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogCacheOperation_HappyPath_LogsCacheOperation()
        {
            var loggerMock = new Mock<ILogger>();
            StructuredLogger.LogCacheOperation(loggerMock.Object, "operation", "key", true);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cache operation")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogRouteResolution_HappyPath_LogsRouteResolution()
        {
            var loggerMock = new Mock<ILogger>();
            StructuredLogger.LogRouteResolution(loggerMock.Object, "/path", "routePattern", 1);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Route resolved")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogRateLimit_HappyPath_LogsRateLimit()
        {
            var loggerMock = new Mock<ILogger>();
            StructuredLogger.LogRateLimit(loggerMock.Object, "192.168.1.1", "/path", 100);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Rate limit")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogAuthentication_HappyPath_LogsAuthentication()
        {
            var loggerMock = new Mock<ILogger>();
            StructuredLogger.LogAuthentication(loggerMock.Object, "userId", true);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Authentication")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogCriticalError_HappyPath_LogsCriticalError()
        {
            var loggerMock = new Mock<ILogger>();
            var ex = new Exception("exception");
            StructuredLogger.LogCriticalError(loggerMock.Object, ex, "context");

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Critical error")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogPerformanceMetrics_HappyPath_LogsPerformanceMetrics()
        {
            var loggerMock = new Mock<ILogger>();
            StructuredLogger.LogPerformanceMetrics(loggerMock.Object, "operation", 100);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Performance")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogRequestStart_NullLogger_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => StructuredLogger.LogRequestStart(null!, "requestId", "/path", "GET", "192.168.1.1"));
        }

        [Fact]
        public void LogRequestComplete_NullLogger_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => StructuredLogger.LogRequestComplete(null!, "requestId", "/path", 200, 100));
        }
    }
}
