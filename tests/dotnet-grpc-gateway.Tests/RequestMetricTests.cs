#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using DotNetGrpcGateway.Domain;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class RequestMetricTests
{
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
