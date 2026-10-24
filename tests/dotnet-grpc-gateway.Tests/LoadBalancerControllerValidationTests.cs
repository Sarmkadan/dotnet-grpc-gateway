#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using DotNetGrpcGateway.Controllers;
using DotNetGrpcGateway.Domain;
using FluentAssertions;
using Xunit;

namespace DotNetGrpcGateway.Tests;

/// <summary>
/// Contains unit tests for <see cref="LoadBalancerControllerValidation"/> covering endpoint, ID,
/// strategy name and draining-state validation rules.
/// </summary>
public class LoadBalancerControllerValidationTests
{
    [Fact]
    public void Validate_ValidEndpoint_ReturnsNoErrors()
    {
        // Arrange
        var endpoint = CreateValidEndpoint();

        // Act & Assert
        LoadBalancerControllerValidation.Validate(endpoint).Should().BeEmpty();
        LoadBalancerControllerValidation.IsValid(endpoint).Should().BeTrue();
        Action act = () => LoadBalancerControllerValidation.EnsureValid(endpoint);
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_NullInputs_ThrowExpectedExceptions()
    {
        // Act & Assert
        Action nullEndpoint = () => LoadBalancerControllerValidation.Validate((ServiceEndpoint)null!);
        nullEndpoint.Should().ThrowExactly<ArgumentNullException>();

        Action nullStrategy = () => LoadBalancerControllerValidation.Validate((string)null!);
        nullStrategy.Should().ThrowExactly<ArgumentNullException>();

        Action emptyStrategy = () => LoadBalancerControllerValidation.Validate(string.Empty);
        emptyStrategy.Should().ThrowExactly<ArgumentException>();

        var invalid = CreateValidEndpoint();
        invalid.Host = " ";
        Action ensureInvalid = () => LoadBalancerControllerValidation.EnsureValid(invalid);
        ensureInvalid.Should().ThrowExactly<ArgumentException>()
            .WithMessage("*Host is required*");
    }

    [Theory]
    [InlineData(0, 1, false)]
    [InlineData(1, 1, true)]
    [InlineData(65535, 100, true)]
    [InlineData(65536, 1, false)]
    [InlineData(5000, 0, false)]
    [InlineData(5000, 101, false)]
    public void Validate_PortAndWeight_BoundariesAreInclusive(int port, int weight, bool expectedValid)
    {
        // Arrange
        var endpoint = CreateValidEndpoint();
        endpoint.Port = port;
        endpoint.Weight = weight;

        // Act
        var errors = LoadBalancerControllerValidation.Validate(endpoint);

        // Assert
        errors.Should().HaveCount(expectedValid ? 0 : 1);
        LoadBalancerControllerValidation.IsValid(endpoint).Should().Be(expectedValid);
    }

    [Fact]
    public void Validate_Host_RequiredAndMaxLengthIs253()
    {
        // Arrange
        var atLimit = CreateValidEndpoint();
        atLimit.Host = new string('a', 253);

        var overLimit = CreateValidEndpoint();
        overLimit.Host = new string('a', 254);

        var blank = CreateValidEndpoint();
        blank.Host = "   ";

        var missing = CreateValidEndpoint();
        missing.Host = null!;

        // Act & Assert
        LoadBalancerControllerValidation.IsValid(atLimit).Should().BeTrue();
        LoadBalancerControllerValidation.Validate(overLimit)
            .Should().ContainSingle().Which.Should().Be("Host cannot exceed 253 characters.");
        LoadBalancerControllerValidation.Validate(blank)
            .Should().ContainSingle().Which.Should().Be("Host is required and cannot be empty or whitespace.");
        LoadBalancerControllerValidation.Validate(missing)
            .Should().ContainSingle().Which.Should().Be("Host is required and cannot be empty or whitespace.");
    }

    [Fact]
    public void Validate_Timestamps_RejectDefaultFutureAndReversedDates()
    {
        // Arrange
        var now = DateTime.UtcNow;

        var defaults = CreateValidEndpoint();
        defaults.RegisteredAt = default;
        defaults.LastUsedAt = default;

        var future = CreateValidEndpoint();
        future.RegisteredAt = now.AddMinutes(5);
        future.LastUsedAt = now.AddMinutes(5);

        var reversed = CreateValidEndpoint();
        reversed.RegisteredAt = now.AddHours(-1);
        reversed.LastUsedAt = now.AddHours(-2);

        var withinSkew = CreateValidEndpoint();
        withinSkew.RegisteredAt = now.AddSeconds(30);
        withinSkew.LastUsedAt = now.AddSeconds(45);

        // Act & Assert
        LoadBalancerControllerValidation.Validate(defaults)
            .Should().Contain("RegisteredAt must be a valid date.")
            .And.Contain("LastUsedAt must be a valid date.");
        LoadBalancerControllerValidation.Validate(future)
            .Should().Contain("RegisteredAt cannot be in the future.")
            .And.Contain("LastUsedAt cannot be in the future.");
        LoadBalancerControllerValidation.Validate(reversed)
            .Should().ContainSingle().Which.Should().Be("LastUsedAt cannot be earlier than RegisteredAt.");
        LoadBalancerControllerValidation.IsValid(withinSkew).Should().BeTrue();
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(1, true)]
    [InlineData(int.MaxValue, true)]
    public void ValidateIds_BelowOne_IsInvalid(int id, bool expectedValid)
    {
        // Act & Assert
        LoadBalancerControllerValidation.IsValidServiceId(id).Should().Be(expectedValid);
        LoadBalancerControllerValidation.IsValidEndpointId(id).Should().Be(expectedValid);
        LoadBalancerControllerValidation.ValidateServiceId(id).Should().HaveCount(expectedValid ? 0 : 1);
        LoadBalancerControllerValidation.ValidateEndpointId(id).Should().HaveCount(expectedValid ? 0 : 1);
    }

    [Theory]
    [InlineData("RoundRobin", true)]
    [InlineData("random", true)]
    [InlineData("LEASTCONNECTIONS", true)]
    [InlineData("Weighted", false)]
    public void Validate_StrategyName_MatchesEnumCaseInsensitively(string strategy, bool expectedValid)
    {
        // Act
        var errors = LoadBalancerControllerValidation.Validate(strategy);

        // Assert
        errors.Should().HaveCount(expectedValid ? 0 : 1);
        LoadBalancerControllerValidation.IsValid(strategy).Should().Be(expectedValid);
        if (!expectedValid)
            errors[0].Should().Contain("Valid values: RoundRobin, Random, LeastConnections.");
    }

    [Fact]
    public void Validate_DrainingState_AlwaysValid()
    {
        // Act & Assert
        LoadBalancerControllerValidation.Validate(true).Should().BeEmpty();
        LoadBalancerControllerValidation.Validate(false).Should().BeEmpty();
        LoadBalancerControllerValidation.IsValid(true).Should().BeTrue();
        LoadBalancerControllerValidation.IsValid(false).Should().BeTrue();
        Action act = () => LoadBalancerControllerValidation.EnsureValid(false);
        act.Should().NotThrow();
    }

    private static ServiceEndpoint CreateValidEndpoint() => new()
    {
        Host = "localhost",
        Port = 5000,
        Weight = 1,
        RegisteredAt = DateTime.UtcNow.AddHours(-1),
        LastUsedAt = DateTime.UtcNow.AddMinutes(-5),
    };
}
