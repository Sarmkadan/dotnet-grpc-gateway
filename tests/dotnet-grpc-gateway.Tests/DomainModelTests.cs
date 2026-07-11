#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using FluentAssertions;
using DotNetGrpcGateway.Domain;
using Xunit;

/// <summary>
/// Contains unit tests for the GrpcService class.
/// </summary>
namespace DotNetGrpcGateway.Tests;

public class GrpcServiceTests
{
    /// <summary>
    /// Creates a new instance of GrpcService with valid properties.
    /// </summary>
    /// <returns>A new instance of GrpcService.</returns>
    private static GrpcService ValidService() => new()
    {
        Name = "UserService",
        ServiceFullName = "myapp.UserService",
        Host = "localhost",
        Port = 5001
    };

    /// <summary>
    /// Verifies that the GetEndpointUri method returns the correct URI when TLS is enabled.
    /// </summary>
    [Fact]
    public void GetEndpointUri_TlsEnabled_ReturnsHttpsScheme()
    {
        var service = ValidService();
        service.UseTls = true;

        var uri = service.GetEndpointUri();

        uri.Should().StartWith("https://");
        uri.Should().Be("https://localhost:5001");
    }

    /// <summary>
    /// Verifies that the GetEndpointUri method returns the correct URI when TLS is disabled.
    /// </summary>
    [Fact]
    public void GetEndpointUri_TlsDisabled_ReturnsHttpScheme()
    {
        var service = ValidService();
        service.UseTls = false;

        var uri = service.GetEndpointUri();

        uri.Should().Be("http://localhost:5001");
    }

    /// <summary>
    /// Verifies that the RecordRequestMetric method maintains the running average of response times.
    /// </summary>
    [Fact]
    public void RecordRequestMetric_MultipleRequests_MaintainsRunningAverage()
    {
        var service = ValidService();

        service.RecordRequestMetric(100, success: true);
        service.RecordRequestMetric(200, success: true);

        service.TotalRequestsProcessed.Should().Be(2);
        service.FailedRequestsCount.Should().Be(0);
        service.AverageResponseTimeMs.Should().BeApproximately(150, 0.001);
    }

    /// <summary>
    /// Verifies that the RecordRequestMetric method increments the failure count when a request fails.
    /// </summary>
    [Fact]
    public void RecordRequestMetric_FailedRequest_IncrementsFailureCount()
    {
        var service = ValidService();

        service.RecordRequestMetric(500, success: false);

        service.TotalRequestsProcessed.Should().Be(1);
        service.FailedRequestsCount.Should().Be(1);
    }

    /// <summary>
    /// Verifies that the Validate method throws an InvalidOperationException when the port is invalid.
    /// </summary>
    [Fact]
    public void Validate_InvalidPort_ThrowsInvalidOperationException()
    {
        var service = ValidService();
        service.Port = 0;

        var act = () => service.Validate();

        act.Should().Throw<InvalidOperationException>()
        .WithMessage("*port*");
    }
}
