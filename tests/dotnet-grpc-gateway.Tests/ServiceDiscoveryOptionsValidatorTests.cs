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

public class ServiceDiscoveryOptionsValidatorTests
{
    [Fact]
    public void Validate_WithValidConfiguration_Succeeds()
    {
        // Arrange
        var sut = CreateSut();
        var options = CreateValidOptions();

        // Act
        var result = sut.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithProviderAndNoEndpoints_Succeeds()
    {
        // Arrange
        var sut = CreateSut();
        var options = CreateValidOptions();
        options.Endpoints.Clear();
        options.Provider = "consul";

        // Act
        var result = sut.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithNoEndpointsAndNoProvider_Fails()
    {
        // Arrange
        var sut = CreateSut();
        var options = CreateValidOptions();
        options.Endpoints.Clear();

        // Act
        var result = sut.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("ServiceDiscovery:Endpoints");
        result.FailureMessage.Should().Contain("ServiceDiscovery:Provider");
    }

    [Fact]
    public void Validate_WithWhitespaceProviderAndNoEndpoints_Fails()
    {
        // Arrange
        var sut = CreateSut();
        var options = CreateValidOptions();
        options.Endpoints.Clear();
        options.Provider = "   ";

        // Act
        var result = sut.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("ServiceDiscovery:Endpoints");
    }

    [Theory]
    [InlineData("localhost:5001")]
    [InlineData("/relative/path")]
    [InlineData("ftp://backend.local")]
    [InlineData("grpc://backend.local:5001")]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithInvalidEndpoint_FailsNamingEndpointIndex(string endpoint)
    {
        // Arrange
        var sut = CreateSut();
        var options = CreateValidOptions();
        options.Endpoints = ["http://backend-a:5001", endpoint];

        // Act
        var result = sut.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("ServiceDiscovery:Endpoints[1]");
    }

    [Theory]
    [InlineData("http://backend.local:5001")]
    [InlineData("https://backend.local")]
    public void Validate_WithHttpOrHttpsEndpoint_Succeeds(string endpoint)
    {
        // Arrange
        var sut = CreateSut();
        var options = CreateValidOptions();
        options.Endpoints = [endpoint];

        // Act
        var result = sut.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(3600)]
    [InlineData(7200)]
    public void Validate_WithRefreshIntervalOutOfRange_FailsNamingKey(int refreshIntervalSeconds)
    {
        // Arrange
        var sut = CreateSut();
        var options = CreateValidOptions();
        options.RefreshIntervalSeconds = refreshIntervalSeconds;

        // Act
        var result = sut.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("ServiceDiscovery:RefreshIntervalSeconds");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-500)]
    public void Validate_WithNonPositiveConnectTimeout_FailsNamingKey(int connectTimeoutMs)
    {
        // Arrange
        var sut = CreateSut();
        var options = CreateValidOptions();
        options.ConnectTimeoutMs = connectTimeoutMs;

        // Act
        var result = sut.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("ServiceDiscovery:ConnectTimeoutMs");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithNonPositiveRequestTimeout_FailsNamingKey(int requestTimeoutMs)
    {
        // Arrange
        var sut = CreateSut();
        var options = CreateValidOptions();
        options.RequestTimeoutMs = requestTimeoutMs;

        // Act
        var result = sut.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("ServiceDiscovery:RequestTimeoutMs");
    }

    [Fact]
    public void Validate_WithMultipleInvalidKeys_ReportsEveryFailure()
    {
        // Arrange
        var sut = CreateSut();
        var options = CreateValidOptions();
        options.Endpoints = ["not-a-uri"];
        options.RefreshIntervalSeconds = 0;
        options.RequestTimeoutMs = 0;

        // Act
        var result = sut.Validate(null, options);

        // Assert
        result.Failures.Should().HaveCount(3);
        result.FailureMessage.Should().Contain("ServiceDiscovery:Endpoints[0]");
        result.FailureMessage.Should().Contain("ServiceDiscovery:RefreshIntervalSeconds");
        result.FailureMessage.Should().Contain("ServiceDiscovery:RequestTimeoutMs");
    }

    [Fact]
    public async Task StartAsync_WithInvalidServiceDiscoveryOptions_ThrowsNamingKey()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ServiceDiscovery:Endpoints:0"] = "http://backend.local:5001",
                ["ServiceDiscovery:RefreshIntervalSeconds"] = "3600"
            })
            .Build();

        using var host = new HostBuilder()
            .ConfigureServices(services => services.AddGatewayConfiguration(configuration))
            .Build();

        // Act
        var act = () => host.StartAsync();

        // Assert
        var exception = await Assert.ThrowsAsync<OptionsValidationException>(act);
        exception.Message.Should().Contain("ServiceDiscovery:RefreshIntervalSeconds");
    }

    private static ServiceDiscoveryOptionsValidator CreateSut() => new();

    private static ServiceDiscoveryOptions CreateValidOptions() => new()
    {
        Endpoints = ["http://backend-a.local:5001", "https://backend-b.local:5002"],
        RefreshIntervalSeconds = 30,
        ConnectTimeoutMs = 2000,
        RequestTimeoutMs = 5000
    };
}
