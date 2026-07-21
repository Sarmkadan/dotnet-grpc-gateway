#nullable enable

using System.Security.Claims;
using DotNetGrpcGateway.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Text.Encodings.Web;
using Xunit;

namespace DotNetGrpcGateway.Tests;

/// <summary>
/// Tests for ApiKeyAuthenticationHandler authentication behavior.
/// </summary>
public class ApiKeyAuthenticationHandlerTests
{
    private readonly Mock<IOptionsMonitor<AuthenticationSchemeOptions>> _optionsMonitorMock;
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly Mock<ILogger<ApiKeyAuthenticationHandler>> _loggerMock;
    private readonly Mock<UrlEncoder> _urlEncoderMock;
    private readonly ApiKeyAuthenticationHandler _handler;

    public ApiKeyAuthenticationHandlerTests()
    {
        _optionsMonitorMock = new Mock<IOptionsMonitor<AuthenticationSchemeOptions>>();
        _loggerMock = new Mock<ILogger<ApiKeyAuthenticationHandler>>();
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _urlEncoderMock = new Mock<UrlEncoder>();

        _loggerFactoryMock
            .Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(_loggerMock.Object);

        _optionsMonitorMock
            .Setup(x => x.Get(It.IsAny<string>()))
            .Returns(new AuthenticationSchemeOptions());

        _handler = new ApiKeyAuthenticationHandler(
            _optionsMonitorMock.Object,
            _loggerFactoryMock.Object,
            _urlEncoderMock.Object);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_MissingAuthorizationHeader_ReturnsFailResult()
    {
        // Arrange
        var context = new DefaultHttpContext();
        await _handler.InitializeAsync(
            new AuthenticationScheme("ApiKey", "ApiKey", typeof(ApiKeyAuthenticationHandler)),
            context);

        // Act
        var result = await _handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failure.Should().NotBeNull();
        result.Failure?.Message.Should().Be("Missing Authorization header");
    }

    [Theory]
    [InlineData("Bearer")]
    [InlineData("bearer")]
    [InlineData("BEARER")]
    public async Task HandleAuthenticateAsync_ValidBearerToken_ReturnsSuccessWithClaims(string scheme)
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = $"{scheme} {token}";

        await _handler.InitializeAsync(
            new AuthenticationScheme("ApiKey", "ApiKey", typeof(ApiKeyAuthenticationHandler)),
            context);

        // Act
        var result = await _handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Failure.Should().BeNull();

        var principal = result.Principal;
        principal.Should().NotBeNull();
        principal.Identity.Should().NotBeNull();
        principal.Identity.IsAuthenticated.Should().BeTrue();
        principal.Identity.AuthenticationType.Should().Be("ApiKey");

        var claims = principal.Claims.ToList();
        claims.Should().Contain(x => x.Type == ClaimTypes.NameIdentifier && x.Value == token);
        claims.Should().Contain(x => x.Type == "token_type" && x.Value == "api_key");
    }

    [Theory]
    [InlineData("Bearer ")] // Empty token after space
    [InlineData("Bearer   ")] // Whitespace only after space
    public async Task HandleAuthenticateAsync_MissingToken_ReturnsFailResult(string authHeader)
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = authHeader;

        await _handler.InitializeAsync(
            new AuthenticationScheme("ApiKey", "ApiKey", typeof(ApiKeyAuthenticationHandler)),
            context);

        // Act
        var result = await _handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failure.Should().NotBeNull();
        result.Failure?.Message.Should().Be("Missing token");
    }

    [Theory]
    [InlineData("Basic abc123")]
    [InlineData("Token abc123")]
    [InlineData("ApiKey abc123")]
    [InlineData("Digest abc123")]
    public async Task HandleAuthenticateAsync_InvalidScheme_ReturnsFailResult(string authHeader)
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = authHeader;

        await _handler.InitializeAsync(
            new AuthenticationScheme("ApiKey", "ApiKey", typeof(ApiKeyAuthenticationHandler)),
            context);

        // Act
        var result = await _handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failure.Should().NotBeNull();
        result.Failure?.Message.Should().Be("Invalid Authorization header format");
    }

    [Theory]
    [InlineData("Bearer not-a-guid")]
    [InlineData("Bearer 12345")]
    [InlineData("Bearer abc")]
    public async Task HandleAuthenticateAsync_InvalidTokenFormat_ReturnsFailResult(string authHeader)
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = authHeader;

        await _handler.InitializeAsync(
            new AuthenticationScheme("ApiKey", "ApiKey", typeof(ApiKeyAuthenticationHandler)),
            context);

        // Act
        var result = await _handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failure.Should().NotBeNull();
        result.Failure?.Message.Should().Be("Invalid token format");
    }

    [Fact]
    public async Task HandleAuthenticateAsync_HealthCheckEndpoint_SkipsAuthentication()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/health";

        await _handler.InitializeAsync(
            new AuthenticationScheme("ApiKey", "ApiKey", typeof(ApiKeyAuthenticationHandler)),
            context);

        // Act
        var result = await _handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failure.Should().BeNull();
        result.Ticket.Should().BeNull();
    }

    [Fact]
    public async Task HandleAuthenticateAsync_CaseInsensitiveHeaderName_ReturnsSuccess()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var context = new DefaultHttpContext();
        context.Request.Headers["authorization"] = $"Bearer {token}";

        await _handler.InitializeAsync(
            new AuthenticationScheme("ApiKey", "ApiKey", typeof(ApiKeyAuthenticationHandler)),
            context);

        // Act
        var result = await _handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Failure.Should().BeNull();

        var principal = result.Principal;
        principal.Should().NotBeNull();
        principal.Identity.Should().NotBeNull();
        principal.Identity.IsAuthenticated.Should().BeTrue();
    }
}
