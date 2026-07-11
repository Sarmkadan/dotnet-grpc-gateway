#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using FluentAssertions;
using DotNetGrpcGateway.Domain;
using Xunit;

/// <summary>
/// Tests for the RequestLogService.
/// </summary>
namespace DotNetGrpcGateway.Tests;

public class RequestLogServiceTests
{
    /// <summary>
    /// Tests that a valid request creates a log entry with the correct information.
    /// </summary>
    [Fact]
    public void LogRequest_ValidRequest_CreatesLogEntry()
    {
        var logEntry = new RequestLogEntry
        {
            RequestId = Guid.NewGuid().ToString(),
            ServiceName = "UserService",
            MethodName = "GetUser",
            ClientIp = "192.168.1.1",
            RequestSizeBytes = 1024,
            ResponseSizeBytes = 2048,
            DurationMs = 150,
            HttpStatusCode = 200,
            IsSuccessful = true,
            Timestamp = DateTime.UtcNow
        };

        logEntry.LogLevel.Should().Be("INFO");
        logEntry.Message.Should().Contain("Request completed");
        logEntry.Message.Should().Contain("UserService.GetUser");
        logEntry.Message.Should().Contain("200");
    }

    /// <summary>
    /// Tests that a failed request creates a log entry with the correct information.
    /// </summary>
    [Fact]
    public void LogRequest_FailedRequest_CreatesErrorLogEntry()
    {
        var logEntry = new RequestLogEntry
        {
            RequestId = Guid.NewGuid().ToString(),
            ServiceName = "UserService",
            MethodName = "GetUser",
            ClientIp = "192.168.1.1",
            RequestSizeBytes = 1024,
            ResponseSizeBytes = 0,
            DurationMs = 500,
            HttpStatusCode = 500,
            IsSuccessful = false,
            ErrorMessage = "Internal server error",
            Timestamp = DateTime.UtcNow
        };

        logEntry.LogLevel.Should().Be("ERROR");
        logEntry.Message.Should().Contain("Request failed");
        logEntry.Message.Should().Contain("500");
        logEntry.Message.Should().Contain("Internal server error");
    }

    /// <summary>
    /// Tests that a slow request creates a log entry with the correct information.
    /// </summary>
    [Fact]
    public void LogRequest_SlowRequest_CreatesWarningLogEntry()
    {
        var logEntry = new RequestLogEntry
        {
            RequestId = Guid.NewGuid().ToString(),
            ServiceName = "UserService",
            MethodName = "GetUser",
            ClientIp = "192.168.1.1",
            RequestSizeBytes = 1024,
            ResponseSizeBytes = 2048,
            DurationMs = 2500,
            HttpStatusCode = 200,
            IsSuccessful = true,
            Timestamp = DateTime.UtcNow
        };

        logEntry.LogLevel.Should().Be("WARN");
        logEntry.Message.Should().Contain("Slow request");
    }

    /// <summary>
    /// Tests that a large request creates a log entry with the correct information.
    /// </summary>
    [Fact]
    public void LogRequest_LargeRequest_CreatesWarningLogEntry()
    {
        var logEntry = new RequestLogEntry
        {
            RequestId = Guid.NewGuid().ToString(),
            ServiceName = "UserService",
            MethodName = "UploadFile",
            ClientIp = "192.168.1.1",
            RequestSizeBytes = 10 * 1024 * 1024,
            ResponseSizeBytes = 5 * 1024 * 1024,
            DurationMs = 150,
            HttpStatusCode = 200,
            IsSuccessful = true,
            Timestamp = DateTime.UtcNow
        };

        logEntry.LogLevel.Should().Be("WARN");
        logEntry.Message.Should().Contain("Large request/response");
    }

    /// <summary>
    /// Tests that a request with a cache hit creates a log entry with the correct information.
    /// </summary>
    [Fact]
    public void LogRequest_WithCacheHit_CreatesInfoLogEntry()
    {
        var logEntry = new RequestLogEntry
        {
            RequestId = Guid.NewGuid().ToString(),
            ServiceName = "UserService",
            MethodName = "GetUser",
            ClientIp = "192.168.1.1",
            RequestSizeBytes = 1024,
            ResponseSizeBytes = 2048,
            DurationMs = 50,
            HttpStatusCode = 200,
            IsSuccessful = true,
            CacheHit = true,
            Timestamp = DateTime.UtcNow
        };

        logEntry.LogLevel.Should().Be("INFO");
        logEntry.Message.Should().Contain("Cache HIT");
    }

    /// <summary>
    /// Tests that a request with a cache miss creates a log entry with the correct information.
    /// </summary>
    [Fact]
    public void LogRequest_WithCacheMiss_CreatesInfoLogEntry()
    {
        var logEntry = new RequestLogEntry
        {
            RequestId = Guid.NewGuid().ToString(),
            ServiceName = "UserService",
            MethodName = "GetUser",
            ClientIp = "192.168.1.1",
            RequestSizeBytes = 1024,
            ResponseSizeBytes = 2048,
            DurationMs = 150,
            HttpStatusCode = 200,
            IsSuccessful = true,
            CacheHit = false,
            Timestamp = DateTime.UtcNow
        };

        logEntry.LogLevel.Should().Be("INFO");
        logEntry.Message.Should().Contain("Cache MISS");
    }

    /// <summary>
    /// Tests that a request with retries creates a log entry with the correct information.
    /// </summary>
    [Fact]
    public void LogRequest_WithRetries_CreatesWarningLogEntry()
    {
        var logEntry = new RequestLogEntry
        {
            RequestId = Guid.NewGuid().ToString(),
            ServiceName = "UserService",
            MethodName = "GetUser",
            ClientIp = "192.168.1.1",
            RequestSizeBytes = 1024,
            ResponseSizeBytes = 2048,
            DurationMs = 300,
            HttpStatusCode = 200,
            IsSuccessful = true,
            RetryCount = 3,
            Timestamp = DateTime.UtcNow
        };

        logEntry.LogLevel.Should().Be("WARN");
        logEntry.Message.Should().Contain("Retry count: 3");
    }

    /// <summary>
    /// Tests that the default constructor sets default values.
    /// </summary>
    [Fact]
    public void DefaultConstructor_SetsDefaultValues()
    {
        var logEntry = new RequestLogEntry();

        logEntry.RequestId.Should().NotBeNullOrEmpty();
        logEntry.ServiceName.Should().BeNull();
        logEntry.MethodName.Should().BeNull();
        logEntry.ClientIp.Should().BeNull();
        logEntry.RequestSizeBytes.Should().Be(0);
        logEntry.ResponseSizeBytes.Should().Be(0);
        logEntry.DurationMs.Should().Be(0);
        logEntry.HttpStatusCode.Should().Be(0);
        logEntry.IsSuccessful.Should().BeTrue();
        logEntry.ErrorMessage.Should().BeNull();
        logEntry.StackTrace.Should().BeNull();
        logEntry.CacheHit.Should().BeFalse();
        logEntry.RetryCount.Should().Be(0);
        logEntry.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
