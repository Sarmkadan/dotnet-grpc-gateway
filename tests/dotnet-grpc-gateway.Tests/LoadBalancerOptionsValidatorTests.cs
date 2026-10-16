#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Configuration;
using DotNetGrpcGateway.Options;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class LoadBalancerOptionsValidatorTests
{
    [Fact]
    public void Validate_WithValidConfiguration_Succeeds()
    {
        // Arrange
        var sut = CreateSut(endpointCount: 2);
        var options = CreateValidOptions();

        // Act
        var result = sut.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData("RoundRobin")]
    [InlineData("LeastConnections")]
    [InlineData("Random")]
    [InlineData("random")]
    [InlineData("LEASTCONNECTIONS")]
    public void Validate_WithKnownPolicy_Succeeds(string policy)
    {
        // Arrange
        var sut = CreateSut(endpointCount: 2);
        var options = CreateValidOptions();
        options.Policy = policy;

        // Act
        var result = sut.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData("WeightedRoundRobin")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("2")]
    public void Validate_WithUnknownPolicy_FailsListingValidValues(string policy)
    {
        // Arrange
        var sut = CreateSut(endpointCount: 2);
        var options = CreateValidOptions();
        options.Policy = policy;

        // Act
        var result = sut.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("LoadBalancer:Policy");
        result.FailureMessage.Should().Contain("RoundRobin").And.Contain("Random").And.Contain("LeastConnections");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(301)]
    public void Validate_WithHealthCheckIntervalOutOfRange_FailsNamingKey(int seconds)
    {
        // Arrange
        var sut = CreateSut(endpointCount: 2);
        var options = CreateValidOptions();
        options.HealthCheckInterval = TimeSpan.FromSeconds(seconds);

        // Act
        var result = sut.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("LoadBalancer:HealthCheckInterval");
    }

    [Fact]
    public void Validate_WithHealthCheckIntervalExactlyFiveMinutes_Succeeds()
    {
        // Arrange
        var sut = CreateSut(endpointCount: 2);
        var options = CreateValidOptions();
        options.HealthCheckInterval = TimeSpan.FromMinutes(5);

        // Act
        var result = sut.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public void Validate_WithMaxRetriesOutOfRange_FailsNamingKey(int maxRetries)
    {
        // Arrange
        var sut = CreateSut(endpointCount: 2);
        var options = CreateValidOptions();
        options.MaxRetries = maxRetries;

        // Act
        var result = sut.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("LoadBalancer:MaxRetries");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void Validate_WithMaxRetriesAtBounds_Succeeds(int maxRetries)
    {
        // Arrange
        var sut = CreateSut(endpointCount: 2);
        var options = CreateValidOptions();
        options.MaxRetries = maxRetries;

        // Act
        var result = sut.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithNoWeights_Succeeds()
    {
        // Arrange
        var sut = CreateSut(endpointCount: 2);
        var options = CreateValidOptions();
        options.Weights = new List<int>();

        // Act
        var result = sut.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithWeightCountMismatch_FailsNamingKey()
    {
        // Arrange
        var sut = CreateSut(endpointCount: 2);
        var options = CreateValidOptions();
        options.Weights = new List<int> { 3 };

        // Act
        var result = sut.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("LoadBalancer:Weights").And.Contain("ServiceDiscovery:Endpoints");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public void Validate_WithNonPositiveWeight_FailsNamingIndex(int weight)
    {
        // Arrange
        var sut = CreateSut(endpointCount: 2);
        var options = CreateValidOptions();
        options.Weights = new List<int> { 5, weight };

        // Act
        var result = sut.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("LoadBalancer:Weights[1]");
    }

    [Fact]
    public void Validate_WithMultipleInvalidKeys_ReportsEveryFailure()
    {
        // Arrange
        var sut = CreateSut(endpointCount: 2);
        var options = CreateValidOptions();
        options.Policy = "Bogus";
        options.MaxRetries = 99;
        options.HealthCheckInterval = TimeSpan.Zero;

        // Act
        var result = sut.Validate(null, options);

        // Assert
        result.Failures.Should().HaveCount(3);
        result.FailureMessage.Should().Contain("LoadBalancer:Policy");
        result.FailureMessage.Should().Contain("LoadBalancer:HealthCheckInterval");
        result.FailureMessage.Should().Contain("LoadBalancer:MaxRetries");
    }

    [Theory]
    [InlineData("LoadBalancer:Policy", "Bogus", "LoadBalancer:Policy")]
    [InlineData("LoadBalancer:HealthCheckInterval", "00:00:00", "LoadBalancer:HealthCheckInterval")]
    [InlineData("LoadBalancer:HealthCheckInterval", "00:05:01", "LoadBalancer:HealthCheckInterval")]
    [InlineData("LoadBalancer:MaxRetries", "-1", "LoadBalancer:MaxRetries")]
    [InlineData("LoadBalancer:MaxRetries", "11", "LoadBalancer:MaxRetries")]
    public async Task StartAsync_WithInvalidLoadBalancerOptions_ThrowsNamingKey(string key, string value, string expectedKey)
    {
        // Arrange
        using var host = BuildHost(new Dictionary<string, string?> { [key] = value });

        // Act
        var act = () => host.StartAsync();

        // Assert
        var exception = await Assert.ThrowsAsync<OptionsValidationException>(act);
        exception.Message.Should().Contain(expectedKey);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    public async Task StartAsync_WithNonPositiveWeight_ThrowsNamingIndex(string weight)
    {
        // Arrange
        using var host = BuildHost(new Dictionary<string, string?>
        {
            ["LoadBalancer:Weights:0"] = "5",
            ["LoadBalancer:Weights:1"] = weight
        });

        // Act
        var act = () => host.StartAsync();

        // Assert
        var exception = await Assert.ThrowsAsync<OptionsValidationException>(act);
        exception.Message.Should().Contain("LoadBalancer:Weights[1]");
    }

    [Fact]
    public async Task StartAsync_WithWeightCountMismatch_ThrowsOptionsValidationException()
    {
        // Arrange
        using var host = BuildHost(new Dictionary<string, string?>
        {
            ["LoadBalancer:Weights:0"] = "1",
            ["LoadBalancer:Weights:1"] = "2",
            ["LoadBalancer:Weights:2"] = "3"
        });

        // Act
        var act = () => host.StartAsync();

        // Assert
        var exception = await Assert.ThrowsAsync<OptionsValidationException>(act);
        exception.Message.Should().Contain("LoadBalancer:Weights");
    }

    [Fact]
    public async Task StartAsync_WithValidLoadBalancerOptions_StartsHost()
    {
        // Arrange
        using var host = BuildHost(new Dictionary<string, string?>
        {
            ["LoadBalancer:Policy"] = "LeastConnections",
            ["LoadBalancer:HealthCheckInterval"] = "00:01:00",
            ["LoadBalancer:MaxRetries"] = "5",
            ["LoadBalancer:Weights:0"] = "3",
            ["LoadBalancer:Weights:1"] = "1"
        });

        // Act
        var act = () => host.StartAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    private static IHost BuildHost(Dictionary<string, string?> overrides)
    {
        var values = new Dictionary<string, string?>
        {
            ["ServiceDiscovery:Endpoints:0"] = "http://backend-a.local:5001",
            ["ServiceDiscovery:Endpoints:1"] = "http://backend-b.local:5002"
        };
        foreach (var (key, value) in overrides)
        {
            values[key] = value;
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        return new HostBuilder()
            .ConfigureServices(services => services.AddGatewayConfiguration(configuration))
            .Build();
    }

    private static LoadBalancerOptionsValidator CreateSut(int endpointCount)
    {
        var discovery = new ServiceDiscoveryOptions
        {
            Endpoints = Enumerable.Range(0, endpointCount).Select(i => $"http://backend-{i}.local:5001").ToList()
        };

        return new LoadBalancerOptionsValidator(Microsoft.Extensions.Options.Options.Create(discovery));
    }

    private static LoadBalancerOptions CreateValidOptions() => new()
    {
        Policy = "RoundRobin",
        HealthCheckInterval = TimeSpan.FromSeconds(30),
        MaxRetries = 3,
        Weights = new List<int> { 2, 1 }
    };
}
