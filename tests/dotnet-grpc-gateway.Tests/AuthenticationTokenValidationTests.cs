using System;
using System.Collections.Generic;
using System.Linq;
using DotNetGrpcGateway.Domain;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class AuthenticationTokenValidationTests
{
    private AuthenticationToken CreateValidToken()
    {
        return new AuthenticationToken
        {
            Id = 1,
            TokenHash = "hash",
            ClientName = "client",
            ClientId = "client-id",
            TokenType = "Bearer",
            ClientSecret = "secret",
            Scopes = new List<string> { "scope1", "scope2" },
            AllowedServiceIds = new List<int> { 10, 20 },
            AllowAllServices = false,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            LastUsedAt = DateTime.UtcNow.AddMinutes(-5),
            RevokedAt = null,
            UsageCount = 0,
            IpWhitelistCsv = "192.168.1.1,10.0.0.0/24",
            IsRevoked = false,
            RevokedReason = null,
            IsActive = true
        };
    }

    [Fact]
    public void GetValidationErrors_HappyPath_ReturnsEmptyList()
    {
        var token = CreateValidToken();
        var errors = token.GetValidationErrors();
        Assert.Empty(errors);
    }

    [Fact]
    public void GetValidationErrors_InvalidToken_ReturnsExpectedErrors()
    {
        var token = new AuthenticationToken
        {
            Id = 0, // invalid
            TokenHash = "", // invalid
            ClientName = null, // invalid
            ClientId = "   ", // invalid
            TokenType = null, // invalid
            ClientSecret = "", // invalid
            Scopes = new List<string> { "", "valid" }, // contains whitespace
            AllowedServiceIds = new List<int> { -5, 0 }, // non‑positive
            AllowAllServices = false,
            CreatedAt = default, // invalid
            ExpiresAt = DateTime.UtcNow.AddHours(-1), // past
            LastUsedAt = DateTime.UtcNow.AddHours(1), // future
            RevokedAt = DateTime.UtcNow.AddHours(-2), // earlier than CreatedAt
            UsageCount = -1, // negative
            IpWhitelistCsv = "", // empty
            IsRevoked = true,
            RevokedReason = null, // missing
            IsActive = false
        };

        var errors = token.GetValidationErrors();
        Assert.Contains("Id must be a positive integer", errors);
        Assert.Contains("TokenHash is required and cannot be null or whitespace", errors);
        Assert.Contains("ClientName is required and cannot be null or whitespace", errors);
        Assert.Contains("ClientId is required and cannot be null or whitespace", errors);
        Assert.Contains("TokenType is required and cannot be null or whitespace", errors);
        Assert.Contains("ClientSecret cannot be an empty string", errors);
        Assert.Contains("Scopes collection cannot contain null or whitespace values", errors);
        Assert.Contains("AllowedServiceIds collection cannot contain non-positive integers", errors);
        Assert.Contains("CreatedAt must be set to a valid date", errors);
        Assert.Contains("ExpiresAt cannot be in the past", errors);
        Assert.Contains("LastUsedAt cannot be in the future", errors);
        Assert.Contains("RevokedAt cannot be earlier than CreatedAt", errors);
        Assert.Contains("UsageCount cannot be negative", errors);
        Assert.Contains("IpWhitelistCsv must contain at least one valid IP address or CIDR range", errors);
        Assert.Contains("Revoked tokens must have RevokedReason", errors);
        Assert.Contains("Inactive tokens should be marked as revoked", errors);
    }

    [Fact]
    public void IsValid_HappyPath_ReturnsTrue()
    {
        var token = CreateValidToken();
        Assert.True(token.IsValid());
    }

    [Fact]
    public void IsValid_InvalidToken_ReturnsFalse()
    {
        var token = new AuthenticationToken { Id = 0, TokenHash = "" };
        Assert.False(token.IsValid());
    }

    [Fact]
    public void EnsureValid_ValidToken_DoesNotThrow()
    {
        var token = CreateValidToken();
        var exception = Record.Exception(() => token.EnsureValid());
        Assert.Null(exception);
    }

    [Fact]
    public void EnsureValid_InvalidToken_ThrowsArgumentException()
    {
        var token = new AuthenticationToken { Id = 0, TokenHash = "" };
        var ex = Assert.Throws<ArgumentException>(() => token.EnsureValid());
        Assert.Contains("Authentication token is invalid", ex.Message);
        Assert.Contains("Id must be a positive integer", ex.Message);
        Assert.Contains("TokenHash is required and cannot be null or whitespace", ex.Message);
    }

    [Fact]
    public void GetValidationErrors_NullToken_ThrowsArgumentNullException()
    {
        AuthenticationToken? token = null;
        Assert.Throws<ArgumentNullException>(() => token.GetValidationErrors());
    }

    [Fact]
    public void IsValid_NullToken_ThrowsArgumentNullException()
    {
        AuthenticationToken? token = null;
        Assert.Throws<ArgumentNullException>(() => token.IsValid());
    }

    [Fact]
    public void EnsureValid_NullToken_ThrowsArgumentNullException()
    {
        AuthenticationToken? token = null;
        Assert.Throws<ArgumentNullException>(() => token.EnsureValid());
    }
}
