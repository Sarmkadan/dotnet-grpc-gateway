// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using DotNetGrpcGateway.Utilities;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class StringUtilityTests
{
    [Fact]
    public void Truncate_ValueExceedsMaxLength_ReturnsEllipsisSuffix()
    {
        var input = "This is a very long string that exceeds the limit";

        var result = StringUtility.Truncate(input, 20);

        result.Should().HaveLength(20);
        result.Should().EndWith("...");
        result.Should().Be("This is a very lo...");
    }

    [Fact]
    public void Truncate_NullInput_ReturnsEmptyString()
    {
        var result = StringUtility.Truncate(null, 50);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Truncate_ValueShorterThanMaxLength_ReturnsOriginal()
    {
        var input = "short";

        var result = StringUtility.Truncate(input, 100);

        result.Should().Be(input);
    }

    [Fact]
    public void MaskSensitiveData_ValueLongerThanShowChars_MasksRemainder()
    {
        var token = "secret-api-key-xyz";

        var result = StringUtility.MaskSensitiveData(token, 4);

        result.Should().StartWith("secr");
        result.Should().HaveLength(token.Length);
        result.Should().MatchRegex(@"^secr\*+$");
    }

    [Fact]
    public void MatchesWildcardPattern_PatternWithStar_MatchesBroadly()
    {
        var result = StringUtility.MatchesWildcardPattern("UserService.GetUser", "UserService.*");

        result.Should().BeTrue();
    }

    [Fact]
    public void ToKebabCase_PascalCaseInput_InsertsHyphensBeforeUppercase()
    {
        var result = StringUtility.ToKebabCase("GatewayRouteManager");

        result.Should().Be("gateway-route-manager");
    }

    [Fact]
    public void ToSlug_StringWithSpecialChars_ReturnsAlphanumericHyphenated()
    {
        var result = StringUtility.ToSlug("Hello World! gRPC Gateway");

        result.Should().Be("hello-world-grpc-gateway");
        result.Should().MatchRegex(@"^[a-z0-9\-]+$");
    }
}
