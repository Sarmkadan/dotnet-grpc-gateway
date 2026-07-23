#nullable enable
using System;
using DotNetGrpcGateway.Domain;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class GrpcServiceTests
{
    private GrpcService CreateValidService()
    {
        return new GrpcService
        {
            Id = 1,
            Name = "TestService",
            ServiceFullName = "Test.Namespace.TestService",
            Host = "localhost",
            Port = 5000,
            UseTls = false,
            Description = "A test service",
            ProtoPackage = "test.package",
            HealthCheckIntervalSeconds = 30,
            MaxRetries = 5,
            IsHealthy = true,
            RegisteredAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    [Fact]
    public void Validate_WithValidProperties_DoesNotThrow()
    {
        var service = CreateValidService();

        var exception = Record.Exception(() => service.Validate());

        Assert.Null(exception);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_NameIsNullOrWhiteSpace_ThrowsInvalidOperationException(string? name)
    {
        var service = CreateValidService();
        service.Name = name!;

        Assert.Throws<InvalidOperationException>(() => service.Validate());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ServiceFullNameIsNullOrWhiteSpace_ThrowsInvalidOperationException(string? fullName)
    {
        var service = CreateValidService();
        service.ServiceFullName = fullName!;

        Assert.Throws<InvalidOperationException>(() => service.Validate());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_HostIsNullOrWhiteSpace_ThrowsInvalidOperationException(string? host)
    {
        var service = CreateValidService();
        service.Host = host!;

        Assert.Throws<InvalidOperationException>(() => service.Validate());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(70000)]
    public void Validate_PortOutOfRange_ThrowsInvalidOperationException(int port)
    {
        var service = CreateValidService();
        service.Port = port;

        Assert.Throws<InvalidOperationException>(() => service.Validate());
    }

    [Fact]
    public void Validate_HealthCheckIntervalSecondsLessThanOne_ThrowsInvalidOperationException()
    {
        var service = CreateValidService();
        service.HealthCheckIntervalSeconds = 0;

        Assert.Throws<InvalidOperationException>(() => service.Validate());
    }

    [Fact]
    public void Validate_MaxRetriesNegative_ThrowsInvalidOperationException()
    {
        var service = CreateValidService();
        service.MaxRetries = -1;

        Assert.Throws<InvalidOperationException>(() => service.Validate());
    }

    [Fact]
    public void GetEndpointUri_UseTlsTrue_ReturnsHttps()
    {
        var service = CreateValidService();
        service.UseTls = true;
        service.Host = "example.com";
        service.Port = 443;

        var uri = service.GetEndpointUri();

        Assert.Equal("https://example.com:443", uri);
    }

    [Fact]
    public void GetEndpointUri_UseTlsFalse_ReturnsHttp()
    {
        var service = CreateValidService();
        service.UseTls = false;
        service.Host = "example.com";
        service.Port = 80;

        var uri = service.GetEndpointUri();

        Assert.Equal("http://example.com:80", uri);
    }

    [Fact]
    public void UpdateHealthStatus_UpdatesProperties()
    {
        var service = CreateValidService();

        var before = DateTime.UtcNow;
        service.UpdateHealthStatus(false, "network error");
        var after = DateTime.UtcNow;

        Assert.False(service.IsHealthy);
        Assert.Equal("network error", service.LastHealthCheckError);
        Assert.InRange(service.LastHealthCheckAt, before, after);
    }

    [Fact]
    public void RecordRequestMetric_SuccessfulRequest_UpdatesMetrics()
    {
        var service = CreateValidService();

        var before = service.ModifiedAt;
        service.RecordRequestMetric(123.4, true);

        Assert.Equal(1, service.TotalRequestsProcessed);
        Assert.Equal(0, service.FailedRequestsCount);
        Assert.Equal(123.4, service.AverageResponseTimeMs, 3);
        Assert.True(service.ModifiedAt > before);
    }

    [Fact]
    public void RecordRequestMetric_MultipleRequests_CalculatesRunningAverage()
    {
        var service = CreateValidService();

        service.RecordRequestMetric(100, true);   // first request
        service.RecordRequestMetric(200, false); // second request (failed)

        Assert.Equal(2, service.TotalRequestsProcessed);
        Assert.Equal(1, service.FailedRequestsCount);
        Assert.Equal(150, service.AverageResponseTimeMs, 3);
    }
}
