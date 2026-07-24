// Copyright (c) 2024
// Licensed under the MIT license.

using System;
using DotNetGrpcGateway.Domain;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class AuthenticationTokenExtensionsTests
{
    [Fact]
    public void HasScope_HappyPath_ReturnsTrueWhenScopeExists()
    {
        // Arrange
        var token = new AuthenticationToken
        {
            Scopes = new List<string> { "read", "write", "admin" }
        };

        // Act
        var hasScope = token.HasScope("read");

        // Assert
        Assert.True(hasScope);
    }

    [Fact]
    public void HasScope_HappyPath_ReturnsTrueWhenScopeExistsCaseInsensitive()
    {
        // Arrange
        var token = new AuthenticationToken
        {
            Scopes = new List<string> { "read", "write", "admin" }
        };

        // Act
        var hasScope = token.HasScope("READ");

        // Assert
        Assert.True(hasScope);
    }

    [Fact]
    public void HasScope_HappyPath_ReturnsFalseWhenScopeDoesNotExist()
    {
        // Arrange
        var token = new AuthenticationToken
        {
            Scopes = new List<string> { "read", "write", "admin" }
        };

        // Act
        var hasScope = token.HasScope("delete");

        // Assert
        Assert.False(hasScope);
    }

    [Fact]
    public void HasScope_EmptyScopes_ReturnsFalse()
    {
        // Arrange
        var token = new AuthenticationToken
        {
            Scopes = new List<string>()
        };

        // Act
        var hasScope = token.HasScope("read");

        // Assert
        Assert.False(hasScope);
    }

    [Fact]
    public void HasScope_NullToken_ThrowsArgumentNullException()
    {
        // Arrange
        AuthenticationToken? token = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => token!.HasScope("read"));
    }

    [Fact]
    public void HasScope_NullScope_ThrowsArgumentNullException()
    {
        // Arrange
        var token = new AuthenticationToken
        {
            Scopes = new List<string> { "read" }
        };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => token.HasScope(null!));
    }

    [Fact]
    public void HasScope_EmptyScope_ThrowsArgumentException()
    {
        // Arrange
        var token = new AuthenticationToken
        {
            Scopes = new List<string> { "read" }
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => token.HasScope(" "));
    }

    [Fact]
    public void IsExpired_HappyPath_ReturnsFalseWhenTokenNotExpired()
    {
        // Arrange
        var token = new AuthenticationToken
        {
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        // Act
        var isExpired = token.IsExpired();

        // Assert
        Assert.False(isExpired);
    }

    [Fact]
    public void IsExpired_HappyPath_ReturnsTrueWhenTokenIsExpired()
    {
        // Arrange
        var token = new AuthenticationToken
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        };

        // Act
        var isExpired = token.IsExpired();

        // Assert
        Assert.True(isExpired);
    }

    [Fact]
    public void IsExpired_NoExpiration_ReturnsFalse()
    {
        // Arrange
        var token = new AuthenticationToken
        {
            ExpiresAt = null
        };

        // Act
        var isExpired = token.IsExpired();

        // Assert
        Assert.False(isExpired);
    }

    [Fact]
    public void IsExpired_NullToken_ThrowsArgumentNullException()
    {
        // Arrange
        AuthenticationToken? token = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => token!.IsExpired());
    }

    [Fact]
    public void IsNearExpiry_HappyPath_ReturnsTrueWhenWithinThreshold()
    {
        // Arrange
        var token = new AuthenticationToken
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        // Act
        var isNearExpiry = token.IsNearExpiry(TimeSpan.FromMinutes(10));

        // Assert
        Assert.True(isNearExpiry);
    }

    [Fact]
    public void IsNearExpiry_HappyPath_ReturnsFalseWhenOutsideThreshold()
    {
        // Arrange
        var token = new AuthenticationToken
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(20)
        };

        // Act
        var isNearExpiry = token.IsNearExpiry(TimeSpan.FromMinutes(10));

        // Assert
        Assert.False(isNearExpiry);
    }

    [Fact]
    public void IsNearExpiry_ExactThreshold_ReturnsTrue()
    {
        // Arrange
        var expiresAt = DateTime.UtcNow.AddMinutes(5);
        var token = new AuthenticationToken
        {
            ExpiresAt = expiresAt
        };

        // Act
        var isNearExpiry = token.IsNearExpiry(TimeSpan.FromMinutes(5));

        // Assert
        Assert.True(isNearExpiry);
    }

    [Fact]
    public void IsNearExpiry_NoExpiration_ReturnsFalse()
    {
        // Arrange
        var token = new AuthenticationToken
        {
            ExpiresAt = null
        };

        // Act
        var isNearExpiry = token.IsNearExpiry(TimeSpan.FromMinutes(10));

        // Assert
        Assert.False(isNearExpiry);
    }

    [Fact]
    public void IsNearExpiry_NullToken_ThrowsArgumentNullException()
    {
        // Arrange
        AuthenticationToken? token = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => token!.IsNearExpiry(TimeSpan.FromMinutes(10)));
    }

    [Fact]
    public void IsNearExpiry_NegativeThreshold_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var token = new AuthenticationToken
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => token.IsNearExpiry(TimeSpan.FromMinutes(-1)));
    }

    [Fact]
    public void IsNearExpiry_ZeroThreshold_ReturnsFalse()
    {
        // Arrange
        var token = new AuthenticationToken
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        // Act
        var isNearExpiry = token.IsNearExpiry(TimeSpan.Zero);

        // Assert
        Assert.False(isNearExpiry);
    }
}