#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using FluentAssertions;
using DotNetGrpcGateway.Domain;
using Xunit;

namespace DotNetGrpcGateway.Tests;

/// <summary>
/// Tests for the <see cref="GatewayRoute"/> class, covering validation,
/// matching logic, and default property values.
/// </summary>
public class GatewayRouteTests
{
    /// <summary>
    /// Verifies that a route with valid properties does not throw an exception
    /// when <see cref="GatewayRoute.Validate"/> is called.
    /// </summary>
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

    /// <summary>
    /// Ensures that a route with an empty <c>Pattern</c> throws an
    /// <see cref="InvalidOperationException"/> with an appropriate message.
    /// </summary>
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

    /// <summary>
    /// Ensures that a route with a non‑positive <c>TargetServiceId</c> throws an
    /// <see cref="InvalidOperationException"/> with an appropriate message.
    /// </summary>
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

    /// <summary>
    /// Verifies that a negative <c>Priority</c> value causes validation to fail.
    /// </summary>
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

    /// <summary>
    /// Verifies that a priority value greater than the allowed maximum causes
    /// validation to fail.
    /// </summary>
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

    /// <summary>
    /// Ensures that a zero <c>RateLimitPerMinute</c> triggers a validation error.
    /// </summary>
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

    /// <summary>
    /// Ensures that a negative cache duration triggers a validation error.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="GatewayRoute.MatchesRequest"/> returns <c>true</c>
    /// for an exact match and <c>false</c> for non‑matching service or method names.
    /// </summary>
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

    /// <summary>
    /// Tests that a prefix match returns <c>true</c> for method names that start
    /// with the specified prefix and <c>false</c> for other services.
    /// </summary>
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

    /// <summary>
    /// Tests that a regular‑expression match works as expected for allowed patterns.
    /// </summary>
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

    /// <summary>
    /// Verifies that an undefined <c>MatchType</c> value results in a non‑match.
    /// </summary>
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

    /// <summary>
    /// Confirms that <see cref="GatewayRoute.UpdateModifiedDate"/> updates the
    /// <c>ModifiedAt</c> timestamp to a later value.
    /// </summary>
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

    /// <summary>
    /// Checks that the default constructor initializes all properties with their
    /// expected default values.
    /// </summary>
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

    /// <summary>
    /// Validates that calling <see cref="GatewayRoute.Validate"/> on a default
    /// instance throws an <see cref="InvalidOperationException"/>.
    /// </summary>
    [Fact]
    public void Validate_DefaultRoute_DoesNotThrow()
    {
        var route = new GatewayRoute();

        var act = () => route.Validate();

        act.Should().Throw<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that a fully populated route passes validation and that all
    /// explicitly set properties retain their assigned values.
    /// </summary>
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
