// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using DotNetGrpcGateway.Domain;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class GatewayRouteTests
{
    private static GatewayRoute ValidRoute() => new()
    {
        Pattern = "UserService.GetUser",
        TargetServiceId = 1,
        Priority = 100,
        RateLimitPerMinute = 500
    };

    [Fact]
    public void Validate_EmptyPattern_ThrowsInvalidOperationException()
    {
        var route = ValidRoute();
        route.Pattern = string.Empty;

        var act = () => route.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*pattern*");
    }

    [Fact]
    public void Validate_PriorityOutOfBounds_ThrowsInvalidOperationException()
    {
        var route = ValidRoute();
        route.Priority = 1001;

        var act = () => route.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Priority*");
    }

    [Fact]
    public void MatchesRequest_ExactMatch_ReturnsTrueOnlyForIdenticalPath()
    {
        var route = ValidRoute();
        route.Pattern = "UserService.GetUser";
        route.MatchType = RouteMatchType.ExactMatch;

        route.MatchesRequest("UserService", "GetUser").Should().BeTrue();
        route.MatchesRequest("UserService", "ListUsers").Should().BeFalse();
    }

    [Fact]
    public void MatchesRequest_PrefixMatch_ReturnsTrueForRoutesStartingWithPattern()
    {
        var route = ValidRoute();
        route.Pattern = "UserService";
        route.MatchType = RouteMatchType.Prefix;

        route.MatchesRequest("UserService", "GetUser").Should().BeTrue();
        route.MatchesRequest("OrderService", "GetOrder").Should().BeFalse();
    }
}

public class GrpcServiceTests
{
    private static GrpcService ValidService() => new()
    {
        Name = "UserService",
        ServiceFullName = "myapp.UserService",
        Host = "localhost",
        Port = 5001
    };

    [Fact]
    public void GetEndpointUri_TlsEnabled_ReturnsHttpsScheme()
    {
        var service = ValidService();
        service.UseTls = true;

        var uri = service.GetEndpointUri();

        uri.Should().StartWith("https://");
        uri.Should().Be("https://localhost:5001");
    }

    [Fact]
    public void GetEndpointUri_TlsDisabled_ReturnsHttpScheme()
    {
        var service = ValidService();
        service.UseTls = false;

        var uri = service.GetEndpointUri();

        uri.Should().Be("http://localhost:5001");
    }

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

    [Fact]
    public void RecordRequestMetric_FailedRequest_IncrementsFailureCount()
    {
        var service = ValidService();

        service.RecordRequestMetric(500, success: false);

        service.TotalRequestsProcessed.Should().Be(1);
        service.FailedRequestsCount.Should().Be(1);
    }

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
