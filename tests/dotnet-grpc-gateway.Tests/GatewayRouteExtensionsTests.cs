namespace DotNetGrpcGateway.Tests;

using Xunit;
using System;
using System.Collections.Generic;
using DotNetGrpcGateway.Domain;

public class GatewayRouteExtensionsTests
{
    [Fact]
    public void ShouldHandleRequest_ExactMatch_HappyPath_ReturnsTrue()
    {
        // Arrange
        var route = new GatewayRoute
        {
            Pattern = "UserService.GetUser"
        };
        var serviceName = "UserService";
        var methodName = "GetUser";

        // Act
        var result = route.ShouldHandleRequest(serviceName, methodName);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldHandleRequest_PrefixMatch_HappyPath_ReturnsTrue()
    {
        // Arrange
        var route = new GatewayRoute
        {
            Pattern = "UserService.",
            MatchType = RouteMatchType.Prefix
        };
        var serviceName = "UserService";
        var methodName = "GetUserById";

        // Act
        var result = route.ShouldHandleRequest(serviceName, methodName);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldHandleRequest_RegexMatch_HappyPath_ReturnsTrue()
    {
        // Arrange
        var route = new GatewayRoute
        {
            Pattern = @"UserService\.(Get|Create|Update)",
            MatchType = RouteMatchType.Regex
        };
        var serviceName = "UserService";
        var methodName = "GetUser";

        // Act
        var result = route.ShouldHandleRequest(serviceName, methodName);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldHandleRequest_NoMatch_ReturnsFalse()
    {
        // Arrange
        var route = new GatewayRoute
        {
            Pattern = "UserService.GetUser"
        };
        var serviceName = "ProductService";
        var methodName = "GetProduct";

        // Act
        var result = route.ShouldHandleRequest(serviceName, methodName);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldHandleRequest_NullServiceName_ThrowsArgumentNullException()
    {
        // Arrange
        var route = new GatewayRoute
        {
            Pattern = "UserService.GetUser"
        };
        var serviceName = (string)null!;
        var methodName = "GetUser";

        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => route.ShouldHandleRequest(serviceName, methodName));
    }

    [Fact]
    public void ShouldHandleRequest_EmptyServiceName_ThrowsArgumentException()
    {
        // Arrange
        var route = new GatewayRoute
        {
            Pattern = "UserService.GetUser"
        };
        var serviceName = "";
        var methodName = "GetUser";

        // Act and Assert
        Assert.Throws<ArgumentException>(() => route.ShouldHandleRequest(serviceName, methodName));
    }

    [Fact]
    public void ShouldHandleRequest_WhitespaceServiceName_ThrowsArgumentException()
    {
        // Arrange
        var route = new GatewayRoute
        {
            Pattern = "UserService.GetUser"
        };
        var serviceName = "   ";
        var methodName = "GetUser";

        // Act and Assert
        Assert.Throws<ArgumentException>(() => route.ShouldHandleRequest(serviceName, methodName));
    }

    [Fact]
    public void ShouldHandleRequest_NullMethodName_ThrowsArgumentNullException()
    {
        // Arrange
        var route = new GatewayRoute
        {
            Pattern = "UserService.GetUser"
        };
        var serviceName = "UserService";
        var methodName = (string)null!;

        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => route.ShouldHandleRequest(serviceName, methodName));
    }

    [Fact]
    public void ShouldHandleRequest_EmptyMethodName_ThrowsArgumentException()
    {
        // Arrange
        var route = new GatewayRoute
        {
            Pattern = "UserService.GetUser"
        };
        var serviceName = "UserService";
        var methodName = "";

        // Act and Assert
        Assert.Throws<ArgumentException>(() => route.ShouldHandleRequest(serviceName, methodName));
    }

    [Fact]
    public void GetEffectiveRateLimit_RouteHasRateLimit_ReturnsRouteRateLimit()
    {
        // Arrange
        var route = new GatewayRoute
        {
            RateLimitPerMinute = 500
        };
        var defaultRateLimit = 1000;

        // Act
        var result = route.GetEffectiveRateLimit(defaultRateLimit);

        // Assert
        Assert.Equal(500, result);
    }

    [Fact]
    public void GetEffectiveRateLimit_RouteHasZeroRateLimit_ReturnsDefaultRateLimit()
    {
        // Arrange
        var route = new GatewayRoute
        {
            RateLimitPerMinute = 0
        };
        var defaultRateLimit = 1000;

        // Act
        var result = route.GetEffectiveRateLimit(defaultRateLimit);

        // Assert
        Assert.Equal(defaultRateLimit, result);
    }

    [Fact]
    public void GetEffectiveRateLimit_RouteHasNegativeRateLimit_ReturnsDefaultRateLimit()
    {
        // Arrange
        var route = new GatewayRoute
        {
            RateLimitPerMinute = -100
        };
        var defaultRateLimit = 1000;

        // Act
        var result = route.GetEffectiveRateLimit(defaultRateLimit);

        // Assert
        Assert.Equal(defaultRateLimit, result);
    }

    [Fact]
    public void GetEffectiveRateLimit_RouteHasNoRateLimit_ReturnsDefaultRateLimit()
    {
        // Arrange
        var route = new GatewayRoute
        {
            RateLimitPerMinute = 0
        };
        var defaultRateLimit = 2000;

        // Act
        var result = route.GetEffectiveRateLimit(defaultRateLimit);

        // Assert
        Assert.Equal(defaultRateLimit, result);
    }

    [Fact]
    public void GetEffectiveRateLimit_DefaultNotSpecified_ReturnsDefaultOf1000()
    {
        // Arrange
        var route = new GatewayRoute();

        // Act
        var result = route.GetEffectiveRateLimit();

        // Assert
        Assert.Equal(1000, result);
    }

    [Fact]
    public void GetEffectiveRateLimit_NullRoute_ThrowsArgumentNullException()
    {
        // Arrange
        GatewayRoute route = null!;
        var defaultRateLimit = 1000;

        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => route.GetEffectiveRateLimit(defaultRateLimit));
    }

    [Fact]
    public void GetEffectiveCacheDuration_CachingEnabledWithValidDuration_ReturnsCacheDuration()
    {
        // Arrange
        var route = new GatewayRoute
        {
            EnableCaching = true,
            CacheDurationSeconds = 300
        };
        var defaultCacheDuration = 60;

        // Act
        var result = route.GetEffectiveCacheDuration(defaultCacheDuration);

        // Assert
        Assert.Equal(300, result);
    }

    [Fact]
    public void GetEffectiveCacheDuration_CachingEnabledWithZeroDuration_ReturnsDuration()
    {
        // Arrange
        var route = new GatewayRoute
        {
            EnableCaching = true,
            CacheDurationSeconds = 0
        };
        var defaultCacheDuration = 60;

        // Act
        var result = route.GetEffectiveCacheDuration(defaultCacheDuration);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetEffectiveCacheDuration_CachingEnabledWithNegativeDuration_ReturnsDefaultDuration()
    {
        // Arrange
        var route = new GatewayRoute
        {
            EnableCaching = true,
            CacheDurationSeconds = -10
        };
        var defaultCacheDuration = 60;

        // Act
        var result = route.GetEffectiveCacheDuration(defaultCacheDuration);

        // Assert
        Assert.Equal(defaultCacheDuration, result);
    }

    [Fact]
    public void GetEffectiveCacheDuration_CachingDisabled_ReturnsZero()
    {
        // Arrange
        var route = new GatewayRoute
        {
            EnableCaching = false,
            CacheDurationSeconds = 300
        };
        var defaultCacheDuration = 60;

        // Act
        var result = route.GetEffectiveCacheDuration(defaultCacheDuration);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetEffectiveCacheDuration_CachingNotSpecified_ReturnsZero()
    {
        // Arrange
        var route = new GatewayRoute();
        var defaultCacheDuration = 120;

        // Act
        var result = route.GetEffectiveCacheDuration(defaultCacheDuration);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetEffectiveCacheDuration_DefaultNotSpecified_ReturnsZero()
    {
        // Arrange
        var route = new GatewayRoute();

        // Act
        var result = route.GetEffectiveCacheDuration();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetEffectiveCacheDuration_NullRoute_ThrowsArgumentNullException()
    {
        // Arrange
        GatewayRoute route = null!;
        var defaultCacheDuration = 60;

        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => route.GetEffectiveCacheDuration(defaultCacheDuration));
    }

    [Fact]
    public void ToDiagnosticString_HappyPath_ReturnsFormattedString()
    {
        // Arrange
        var route = new GatewayRoute
        {
            Id = 1,
            Pattern = "UserService.GetUser",
            TargetServiceId = 10,
            Priority = 50,
            MatchType = RouteMatchType.ExactMatch,
            Description = "Get user by ID",
            RequiresAuthentication = true,
            AuthorizationPolicy = "BearerTokenPolicy",
            RateLimitPerMinute = 500,
            EnableCaching = true,
            CacheDurationSeconds = 300,
            EnableCompression = false,
            CreatedAt = new DateTime(2024, 1, 1, 10, 0, 0),
            ModifiedAt = new DateTime(2024, 1, 2, 11, 0, 0),
            IsActive = true
        };
        route.Headers.Add("X-Custom-Header", "value1");
        route.Metadata.Add("meta-key", "meta-value");

        // Act
        var result = route.ToDiagnosticString();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("GatewayRoute Diagnostic Information:", result);
        Assert.Contains("ID: 1", result);
        Assert.Contains("Pattern: UserService.GetUser", result);
        Assert.Contains("Target Service ID: 10", result);
        Assert.Contains("Priority: 50", result);
        Assert.Contains("Match Type: ExactMatch", result);
        Assert.Contains("Description: Get user by ID", result);
        Assert.Contains("Active: True", result);
        Assert.Contains("Requires Authentication: True", result);
        Assert.Contains("Rate Limit: 500 per minute", result);
        Assert.Contains("Caching: Enabled (300s)", result);
        Assert.Contains("Compression: Disabled", result);
        Assert.Contains("Created: 2024-01-01 10:00:00", result);
        Assert.Contains("Modified: 2024-01-02 11:00:00", result);
        Assert.Contains("X-Custom-Header: value1", result);
        Assert.Contains("meta-key: meta-value", result);
    }

    [Fact]
    public void ToDiagnosticString_NullDescription_ReturnsPlaceholder()
    {
        // Arrange
        var route = new GatewayRoute
        {
            Description = null
        };

        // Act
        var result = route.ToDiagnosticString();

        // Assert
        Assert.Contains("Description: (none)", result);
    }

    [Fact]
    public void ToDiagnosticString_EmptyHeadersAndMetadata_DoesNotIncludeSections()
    {
        // Arrange
        var route = new GatewayRoute
        {
            Id = 2,
            Pattern = "ProductService.GetProduct"
        };

        // Act
        var result = route.ToDiagnosticString();

        // Assert
        Assert.NotNull(result);
        Assert.DoesNotContain("Headers:", result);
        Assert.DoesNotContain("Metadata:", result);
    }

    [Fact]
    public void ToDiagnosticString_NullRoute_ThrowsArgumentNullException()
    {
        // Arrange
        GatewayRoute route = null!;

        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => route.ToDiagnosticString());
    }

    [Fact]
    public void RequiresAuth_RouteRequiresAuthentication_ReturnsTrue()
    {
        // Arrange
        var route = new GatewayRoute
        {
            RequiresAuthentication = true
        };

        // Act
        var result = route.RequiresAuth();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void RequiresAuth_RouteHasAuthorizationPolicy_ReturnsTrue()
    {
        // Arrange
        var route = new GatewayRoute
        {
            RequiresAuthentication = false,
            AuthorizationPolicy = "BearerTokenPolicy"
        };

        // Act
        var result = route.RequiresAuth();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void RequiresAuth_RouteHasBoth_ReturnsTrue()
    {
        // Arrange
        var route = new GatewayRoute
        {
            RequiresAuthentication = true,
            AuthorizationPolicy = "BearerTokenPolicy"
        };

        // Act
        var result = route.RequiresAuth();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void RequiresAuth_RouteRequiresNoAuth_ReturnsFalse()
    {
        // Arrange
        var route = new GatewayRoute
        {
            RequiresAuthentication = false,
            AuthorizationPolicy = null
        };

        // Act
        var result = route.RequiresAuth();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void RequiresAuth_RouteHasEmptyAuthorizationPolicy_ReturnsFalse()
    {
        // Arrange
        var route = new GatewayRoute
        {
            RequiresAuthentication = false,
            AuthorizationPolicy = ""
        };

        // Act
        var result = route.RequiresAuth();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void RequiresAuth_RouteHasWhitespaceAuthorizationPolicy_ReturnsFalse()
    {
        // Arrange
        var route = new GatewayRoute
        {
            RequiresAuthentication = false,
            AuthorizationPolicy = "   "
        };

        // Act
        var result = route.RequiresAuth();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void RequiresAuth_NullRoute_ThrowsArgumentNullException()
    {
        // Arrange
        GatewayRoute route = null!;

        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => route.RequiresAuth());
    }
}
