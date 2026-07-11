#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using DotNetGrpcGateway.Domain;
using Xunit;

namespace DotNetGrpcGateway.Tests;

/// <summary>
/// Contains unit tests for the <see cref="RequestMetric"/> class, verifying its
/// validation logic, helper methods, and default state.
/// </summary>
public class RequestMetricTests
{
    /// <summary>
    /// Validates that a fully populated <see cref="RequestMetric"/> does not
    /// throw any exception when <c>Validate</c> is called.
    /// </summary>
    [Fact]
    public void Validate_ValidMetric_DoesNotThrow()
    {
        var metric = new RequestMetric
        {
            ServiceName = "UserService",
            MethodName = "GetUser",
            ClientIpAddress = "192.168.1.1",
            DurationMs = 150.5,
            RequestSizeBytes = 1024,
            ResponseSizeBytes = 2048,
            HttpStatusCode = 200,
            IsSuccessful = true
        };

        var act = () => metric.Validate();

        act.Should().NotThrow();
    }

    /// <summary>
    /// Ensures that validation throws <see cref="InvalidOperationException"/>
    /// when <c>ServiceName</c> is empty.
    /// </summary>
    [Fact]
    public void Validate_EmptyServiceName_ThrowsInvalidOperationException()
    {
        var metric = new RequestMetric
        {
            ServiceName = "",
            MethodName = "GetUser",
            ClientIpAddress = "192.168.1.1"
        };

        var act = () => metric.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Service name is required*");
    }

    /// <summary>
    /// Ensures that validation throws <see cref="InvalidOperationException"/>
    /// when <c>MethodName</c> is null.
    /// </summary>
    [Fact]
    public void Validate_NullMethodName_ThrowsInvalidOperationException()
    {
        var metric = new RequestMetric
        {
            ServiceName = "UserService",
            MethodName = null!,
            ClientIpAddress = "192.168.1.1"
        };

        var act = () => metric.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Method name is required*");
    }

    /// <summary>
    /// Ensures that validation throws <see cref="InvalidOperationException"/>
    /// when <c>ClientIpAddress</c> is empty.
    /// </summary>
    [Fact]
    public void Validate_EmptyClientIpAddress_ThrowsInvalidOperationException()
    {
        var metric = new RequestMetric
        {
            ServiceName = "UserService",
            MethodName = "GetUser",
            ClientIpAddress = ""
        };

        var act = () => metric.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Client IP address is required*");
    }

    /// <summary>
    /// Ensures that validation throws <see cref="InvalidOperationException"/>
    /// when <c>DurationMs</c> is negative.
    /// </summary>
    [Fact]
    public void Validate_NegativeDuration_ThrowsInvalidOperationException()
    {
        var metric = new RequestMetric
        {
            ServiceName = "UserService",
            MethodName = "GetUser",
            ClientIpAddress = "192.168.1.1",
            DurationMs = -10
        };

        var act = () => metric.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Duration cannot be negative*");
    }

    /// <summary>
    /// Ensures that validation throws <see cref="InvalidOperationException"/>
    /// when <c>RequestSizeBytes</c> is negative.
    /// </summary>
    [Fact]
    public void Validate_NegativeRequestSize_ThrowsInvalidOperationException()
    {
        var metric = new RequestMetric
        {
            ServiceName = "UserService",
            MethodName = "GetUser",
            ClientIpAddress = "192.168.1.1",
            DurationMs = 100,
            RequestSizeBytes = -100
        };

        var act = () => metric.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Message sizes cannot be negative*");
    }

    /// <summary>
    /// Ensures that validation throws <see cref="InvalidOperationException"/>
    /// when <c>ClientIpAddress</c> is null.
    /// </summary>
    [Fact]
    public void Validate_NullClientIpAddress_ThrowsInvalidOperationException()
    {
        var metric = new RequestMetric
        {
            ServiceName = "UserService",
            MethodName = "GetUser",
            ClientIpAddress = null!
        };

        var act = () => metric.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Client IP address is required*");
    }

    /// <summary>
    /// Ensures that validation throws <see cref="InvalidOperationException"/>
    /// when <c>ResponseSizeBytes</c> is negative.
    /// </summary>
    [Fact]
    public void Validate_NegativeResponseSize_ThrowsInvalidOperationException()
    {
        var metric = new RequestMetric
        {
            ServiceName = "UserService",
            MethodName = "GetUser",
            ClientIpAddress = "192.168.1.1",
            DurationMs = 100,
            RequestSizeBytes = 100,
            ResponseSizeBytes = -100
        };

        var act = () => metric.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Message sizes cannot be negative*");
    }

    /// <summary>
    /// Verifies that <see cref="RequestMetric.IsSlowRequest"/> returns <c>false</c>
    /// when the duration is below the supplied threshold.
    /// </summary>
    [Fact]
    public void IsSlowRequest_BelowThreshold_ReturnsFalse()
    {
        var metric = new RequestMetric
        {
            ServiceName = "UserService",
            MethodName = "GetUser",
            ClientIpAddress = "192.168.1.1",
            DurationMs = 500
        };

        metric.IsSlowRequest(slowThresholdMs: 1000).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="RequestMetric.IsSlowRequest"/> returns <c>true</c>
    /// when the duration exceeds the supplied threshold.
    /// </summary>
    [Fact]
    public void IsSlowRequest_AboveThreshold_ReturnsTrue()
    {
        var metric = new RequestMetric
        {
            ServiceName = "UserService",
            MethodName = "GetUser",
            ClientIpAddress = "192.168.1.1",
            DurationMs = 1500
        };

        metric.IsSlowRequest(slowThresholdMs: 1000).Should().BeTrue();
    }

    /// <summary>
    /// Checks that <see cref="RequestMetric.RecordError(string,string)"/> sets the
    /// error-related properties and marks the metric as unsuccessful.
    /// </summary>
    [Fact]
    public void RecordError_SetsErrorProperties()
    {
        var metric = new RequestMetric
        {
            ServiceName = "UserService",
            MethodName = "GetUser",
            ClientIpAddress = "192.168.1.1"
        };

        metric.RecordError("Connection timeout", "Stack trace here");

        metric.IsSuccessful.Should().BeFalse();
        metric.ErrorMessage.Should().Be("Connection timeout");
        metric.StackTrace.Should().Be("Stack trace here");
    }

    /// <summary>
    /// Ensures that calling <see cref="RequestMetric.RecordRetry"/> increments the
    /// retry count and sets <c>WasRetried</c> to true.
    /// </summary>
    [Fact]
    public void RecordRetry_IncrementsRetryCount()
    {
        var metric = new RequestMetric
        {
            ServiceName = "UserService",
            MethodName = "GetUser",
            ClientIpAddress = "192.168.1.1"
        };

        metric.RecordRetry();
        metric.RecordRetry();

        metric.RetryCount.Should().Be(2);
        metric.WasRetried.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="RequestMetric.RecordError(string)"/> without a stack
    /// trace sets the error message and leaves <c>StackTrace</c> null.
    /// </summary>
    [Fact]
    public void RecordError_WithoutStackTrace_SetsErrorProperties()
    {
        var metric = new RequestMetric
        {
            ServiceName = "UserService",
            MethodName = "GetUser",
            ClientIpAddress = "192.168.1.1"
        };

        metric.RecordError("Connection failed");

        metric.IsSuccessful.Should().BeFalse();
        metric.ErrorMessage.Should().Be("Connection failed");
        metric.StackTrace.Should().BeNull();
    }

    /// <summary>
    /// Checks that <see cref="RequestMetric.SetCacheStatus(string)"/> correctly
    /// records the cache hit status.
    /// </summary>
    [Fact]
    public void SetCacheStatus_SetsCacheHitStatus()
    {
        var metric = new RequestMetric
        {
            ServiceName = "UserService",
            MethodName = "GetUser",
            ClientIpAddress = "192.168.1.1"
        };

        metric.SetCacheStatus("HIT");

        metric.CacheHitStatus.Should().Be("HIT");
    }

    /// <summary>
    /// Verifies that the default constructor initializes all properties with their
    /// expected default values.
    /// </summary>
    [Fact]
    public void DefaultConstructor_SetsDefaultValues()
    {
        var metric = new RequestMetric();

        metric.Id.Should().Be(0);
        metric.RequestId.Should().NotBeNullOrEmpty();
        metric.ServiceName.Should().BeNull();
        metric.MethodName.Should().BeNull();
        metric.ClientIpAddress.Should().BeNull();
        metric.RouteId.Should().Be(0);
        metric.DurationMs.Should().Be(0);
        metric.RequestSizeBytes.Should().Be(0);
        metric.ResponseSizeBytes.Should().Be(0);
        metric.HttpStatusCode.Should().Be(200);
        metric.GrpcStatusCode.Should().BeNull();
        metric.IsSuccessful.Should().BeTrue();
        metric.ErrorMessage.Should().BeNull();
        metric.StackTrace.Should().BeNull();
        metric.CacheHitStatus.Should().BeNull();
        metric.WasRetried.Should().BeFalse();
        metric.RetryCount.Should().Be(0);
        metric.RecordedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
