#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using FluentAssertions;
using DotNetGrpcGateway.Domain;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class GatewayRouteTests
{
    [Fact]
    public void Validate_ValidRoute_DoesNotThrow()
    {
        var route = new GatewayRoute
        {
            Pattern = "UserService.GetUser",
            TargetServiceId = 1,
            Priority = 100,
            RateLimitPerMinute = 1000,
            CacheDurationSeconds = 60,
            IsActive = true
        };

        var act = () => route.Validate();

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_EmptyPattern_ThrowsInvalidOperationException()
    {
        var route = new GatewayRoute
        {
            Pattern = "",
            TargetServiceId = 1
        };

        var act = () => route.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Route pattern is required*");
    }

    [Fact]
    public void Validate_InvalidTargetServiceId_ThrowsInvalidOperationException()
    {
        var route = new GatewayRoute
        {
            Pattern = "UserService.GetUser",
            TargetServiceId = 0
        };

        var act = () => route.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Target service ID must be valid*");
    }

    [Fact]
    public void Validate_NegativePriority_ThrowsInvalidOperationException()
    {
        var route = new GatewayRoute
        {
            Pattern = "UserService.GetUser",
            TargetServiceId = 1,
            Priority = -1
        };

        var act = () => route.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Priority must be between 0 and 1000*");
    }

    [Fact]
    public void Validate_TooHighPriority_ThrowsInvalidOperationException()
    {
        var route = new GatewayRoute
        {
            Pattern = "UserService.GetUser",
            TargetServiceId = 1,
            Priority = 1001
        };

        var act = () => route.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Priority must be between 0 and 1000*");
    }

    [Fact]
    public void Validate_ZeroRateLimit_ThrowsInvalidOperationException()
    {
        var route = new GatewayRoute
        {
            Pattern = "UserService.GetUser",
            TargetServiceId = 1,
            RateLimitPerMinute = 0
        };

        var act = () => route.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Rate limit must be greater than 0*");
    }

    [Fact]
    public void Validate_NegativeCacheDuration_ThrowsInvalidOperationException()
    {
        var route = new GatewayRoute
        {
            Pattern = "UserService.GetUser",
            TargetServiceId = 1,
            CacheDurationSeconds = -10
        };

        var act = () => route.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cache duration cannot be negative*");
    }

    [Fact]
    public void MatchesRequest_ExactMatch_ReturnsTrue()
    {
        var route = new GatewayRoute
        {
            Pattern = "UserService.GetUser",
            MatchType = RouteMatchType.ExactMatch
        };

        route.MatchesRequest("UserService", "GetUser").Should().BeTrue();
        route.MatchesRequest("UserService", "GetUserById").Should().BeFalse();
        route.MatchesRequest("OrderService", "GetUser").Should().BeFalse();
    }

    [Fact]
    public void MatchesRequest_PrefixMatch_ReturnsTrue()
    {
        var route = new GatewayRoute
        {
            Pattern = "UserService.",
            MatchType = RouteMatchType.Prefix
        };

        route.MatchesRequest("UserService", "GetUser").Should().BeTrue();
        route.MatchesRequest("UserService", "GetUserById").Should().BeTrue();
        route.MatchesRequest("UserService", "CreateUser").Should().BeTrue();
        route.MatchesRequest("OrderService", "GetUser").Should().BeFalse();
    }

    [Fact]
    public void MatchesRequest_RegexMatch_ReturnsTrue()
    {
        var route = new GatewayRoute
        {
            Pattern = "UserService\\.(Get|Create)User",
            MatchType = RouteMatchType.Regex
        };

        route.MatchesRequest("UserService", "GetUser").Should().BeTrue();
        route.MatchesRequest("UserService", "CreateUser").Should().BeTrue();
        route.MatchesRequest("UserService", "UpdateUser").Should().BeFalse();
        route.MatchesRequest("OrderService", "GetUser").Should().BeFalse();
    }

    [Fact]
    public void MatchesRequest_InvalidMatchType_ReturnsFalse()
    {
        var route = new GatewayRoute
        {
            Pattern = "UserService.GetUser",
            MatchType = (RouteMatchType)999
        };

        route.MatchesRequest("UserService", "GetUser").Should().BeFalse();
    }

    [Fact]
    public void UpdateModifiedDate_UpdatesModifiedAt()
    {
        var route = new GatewayRoute
        {
            Pattern = "UserService.GetUser",
            TargetServiceId = 1
        };

        var originalModifiedAt = route.ModifiedAt;

        System.Threading.Thread.Sleep(10);
        route.UpdateModifiedDate();

        route.ModifiedAt.Should().BeAfter(originalModifiedAt);
    }

    [Fact]
    public void DefaultConstructor_SetsDefaultValues()
    {
        var route = new GatewayRoute();

        route.Id.Should().Be(0);
        route.Pattern.Should().BeNull();
        route.TargetServiceId.Should().Be(0);
        route.Priority.Should().Be(100);
        route.MatchType.Should().Be(RouteMatchType.ExactMatch);
        route.Description.Should().BeNull();
        route.Headers.Should().BeEmpty();
        route.Metadata.Should().BeEmpty();
        route.RequiresAuthentication.Should().BeFalse();
        route.AuthorizationPolicy.Should().BeNull();
        route.RateLimitPerMinute.Should().Be(1000);
        route.EnableCaching.Should().BeFalse();
        route.CacheDurationSeconds.Should().Be(60);
        route.RequestTransformationScript.Should().BeNull();
        route.ResponseTransformationScript.Should().BeNull();
        route.EnableCompression.Should().BeTrue();
        route.ChannelOptions.Should().BeNull();
        route.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        route.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        route.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Validate_DefaultRoute_DoesNotThrow()
    {
        var route = new GatewayRoute();

        var act = () => route.Validate();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Validate_ValidRouteWithAllProperties_SetCorrectly()
    {
        var route = new GatewayRoute
        {
            Pattern = "UserService.GetUser",
            TargetServiceId = 1,
            Priority = 500,
            Description = "Get user by ID",
            Headers = new Dictionary<string, string> { { "Authorization", "Bearer" } },
            Metadata = new Dictionary<string, string> { { "version", "1.0" } },
            RequiresAuthentication = true,
            AuthorizationPolicy = "BearerToken",
            RateLimitPerMinute = 500,
            EnableCaching = true,
            CacheDurationSeconds = 120,
            RequestTransformationScript = "transformRequest",
            ResponseTransformationScript = "transformResponse",
            EnableCompression = false,
            IsActive = false
        };

        var act = () => route.Validate();

        act.Should().NotThrow();
        route.Priority.Should().Be(500);
        route.RateLimitPerMinute.Should().Be(500);
        route.CacheDurationSeconds.Should().Be(120);
        route.EnableCaching.Should().BeTrue();
        route.EnableCompression.Should().BeFalse();
        route.IsActive.Should().BeFalse();
    }
}