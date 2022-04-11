#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using DotNetGrpcGateway.Utilities;
using Xunit;

/// <summary>
/// Tests for the StringUtility class.
/// </summary>
public class StringUtilityTests
{
    /// <summary>
    /// Tests the Truncate method.
    /// </summary>
    [Fact]
    public void Truncate_ValueExceedsMaxLength_ReturnsEllipsisSuffix()
    {
        var input = "This is a very long string that exceeds the limit";

        var result = StringUtility.Truncate(input, 20);

        result.Should().HaveLength(20);
        result.Should().EndWith("...");
        result.Should().Be("This is a very lo...");
    }

    /// <summary>
    /// Tests the Truncate method with a null input.
    /// </summary>
    [Fact]
    public void Truncate_NullInput_ReturnsEmptyString()
    {
        var result = StringUtility.Truncate(null, 50);

        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests the Truncate method with a value shorter than the max length.
    /// </summary>
    [Fact]
    public void Truncate_ValueShorterThanMaxLength_ReturnsOriginal()
    {
        var input = "short";

        var result = StringUtility.Truncate(input, 100);

        result.Should().Be(input);
    }

    /// <summary>
    /// Tests the MaskSensitiveData method.
    /// </summary>
    [Fact]
    public void MaskSensitiveData_ValueLongerThanShowChars_MasksRemainder()
    {
        var token = "secret-api-key-xyz";

        var result = StringUtility.MaskSensitiveData(token, 4);

        result.Should().StartWith("secr");
        result.Should().HaveLength(token.Length);
        result.Should().MatchRegex(@"^secr\*+$");
    }

    /// <summary>
    /// Tests the MatchesWildcardPattern method.
    /// </summary>
    [Fact]
    public void MatchesWildcardPattern_PatternWithStar_MatchesBroadly()
    {
        var result = StringUtility.MatchesWildcardPattern("UserService.GetUser", "UserService.*");

        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests the ToKebabCase method.
    /// </summary>
    [Fact]
    public void ToKebabCase_PascalCaseInput_InsertsHyphensBeforeUppercase()
    {
        var result = StringUtility.ToKebabCase("GatewayRouteManager");

        result.Should().Be("gateway-route-manager");
    }

    /// <summary>
    /// Tests the ToSlug method.
    /// </summary>
    [Fact]
    public void ToSlug_StringWithSpecialChars_ReturnsAlphanumericHyphenated()
    {
        var result = StringUtility.ToSlug("Hello World! gRPC Gateway");

        result.Should().Be("hello-world-grpc-gateway");
        result.Should().MatchRegex(@"^[a-z0-9\-]+$");
    }
}
