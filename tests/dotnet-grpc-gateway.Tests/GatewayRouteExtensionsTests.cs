// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using DotNetGrpcGateway.Domain;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class GatewayRouteExtensionsTests
{
    [Fact]
    public void MatchesPath_ValidPath_ReturnsTrueForExactMatch()
    {
        var route = new GatewayRoute
        {
            Pattern = "UserService.GetUser",
            MatchType = RouteMatchType.ExactMatch
        };

        route.MatchesPath("UserService.GetUser").Should().BeTrue();
        route.MatchesPath("UserService.CreateUser").Should().BeFalse();
    }

    [Fact]
    public void MatchesPath_ValidPath_ReturnsTrueForPrefixMatch()
    {
        var route = new GatewayRoute
        {
            Pattern = "UserService.",
            MatchType = RouteMatchType.Prefix
        };

        route.MatchesPath("UserService.GetUser").Should().BeTrue();
        route.MatchesPath("UserService.CreateUser").Should().BeTrue();
        route.MatchesPath("OrderService.GetOrder").Should().BeFalse();
    }

    [Fact]
    public void MatchesPath_InvalidPathFormat_ReturnsFalse()
    {
        var route = new GatewayRoute
        {
            Pattern = "UserService.GetUser",
            MatchType = RouteMatchType.ExactMatch
        };

        route.MatchesPath("InvalidPath").Should().BeFalse();
        route.MatchesPath("").Should().BeFalse();
    }

    [Fact]
    public void ToDisplayString_ReturnsExpectedFormat()
    {
        var route = new GatewayRoute
        {
            Id = 1,
            Pattern = "UserService.GetUser",
            MatchType = RouteMatchType.ExactMatch,
            TargetServiceId = 10
        };

        var displayString = route.ToDisplayString();
        displayString.Should().Be("[1] UserService.GetUser (ExactMatch) -> Service ID: 10");
    }
}
