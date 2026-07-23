#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Unit tests for AuthenticationToken domain class
// =============================================================================

using System;
using System.Collections.Generic;
using DotNetGrpcGateway.Domain;
using FluentAssertions;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class AuthenticationTokenTests
{
    private AuthenticationToken CreateValidToken()
    {
        return new AuthenticationToken
        {
            Id = 1,
            TokenHash = "valid-hash-value",
            ClientName = "Test Client",
            ClientId = "test-client-id",
            ClientSecret = "test-secret",
            TokenType = "Bearer",
            Scopes = new List<string> { "read", "write" },
            AllowedServiceIds = new List<int> { 100, 200 },
            AllowAllServices = false,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            LastUsedAt = DateTime.UtcNow.AddMinutes(-5),
            UsageCount = 42,
            IsRevoked = false,
            RevokedReason = null,
            IsActive = true,
            IpWhitelistCsv = "192.168.1.1,10.0.0.0/24",
            UserAgent = "Test-Agent/1.0"
        };
    }

    [Fact]
    public void Constructor_DefaultValues_InitializesCorrectly()
    {
        var token = new AuthenticationToken();

        token.Id.Should().Be(0);
        token.TokenHash.Should().BeNull();
        token.ClientName.Should().BeNull();
        token.ClientId.Should().BeNull();
        token.ClientSecret.Should().BeNull();
        token.TokenType.Should().Be("Bearer");
        token.Scopes.Should().NotBeNull().And.BeEmpty();
        token.AllowedServiceIds.Should().NotBeNull().And.BeEmpty();
        token.AllowAllServices.Should().BeFalse();
        token.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        token.ExpiresAt.Should().BeNull();
        token.LastUsedAt.Should().BeNull();
        token.UsageCount.Should().Be(0);
        token.IsRevoked.Should().BeFalse();
        token.RevokedReason.Should().BeNull();
        token.IsActive.Should().BeTrue();
        token.IpWhitelistCsv.Should().BeNull();
        token.UserAgent.Should().BeNull();
    }

    [Fact]
    public void Id_GetSet_RoundtripsValue()
    {
        var token = new AuthenticationToken { Id = 42 };
        token.Id.Should().Be(42);
    }

    [Fact]
    public void TokenHash_GetSet_RoundtripsValue()
    {
        var token = new AuthenticationToken { TokenHash = "test-hash" };
        token.TokenHash.Should().Be("test-hash");
    }

    [Fact]
    public void ClientName_GetSet_RoundtripsValue()
    {
        var token = new AuthenticationToken { ClientName = "My Client" };
        token.ClientName.Should().Be("My Client");
    }

    [Fact]
    public void ClientId_GetSet_RoundtripsValue()
    {
        var token = new AuthenticationToken { ClientId = "client-123" };
        token.ClientId.Should().Be("client-123");
    }

    [Fact]
    public void ClientSecret_GetSet_RoundtripsValue()
    {
        var token = new AuthenticationToken { ClientSecret = "secret-456" };
        token.ClientSecret.Should().Be("secret-456");
    }

    [Fact]
    public void ClientSecret_CanBeNull()
    {
        var token = new AuthenticationToken { ClientSecret = null };
        token.ClientSecret.Should().BeNull();
    }

    [Fact]
    public void TokenType_GetSet_RoundtripsValue()
    {
        var token = new AuthenticationToken { TokenType = "JWT" };
        token.TokenType.Should().Be("JWT");
    }

    [Fact]
    public void TokenType_DefaultValue_IsBearer()
    {
        var token = new AuthenticationToken();
        token.TokenType.Should().Be("Bearer");
    }

    [Fact]
    public void Scopes_GetSet_RoundtripsValue()
    {
        var scopes = new List<string> { "read", "write", "admin" };
        var token = new AuthenticationToken { Scopes = scopes };
        token.Scopes.Should().BeEquivalentTo(scopes);
    }

    [Fact]
    public void Scopes_DefaultValue_IsEmptyList()
    {
        var token = new AuthenticationToken();
        token.Scopes.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void AllowedServiceIds_GetSet_RoundtripsValue()
    {
        var serviceIds = new List<int> { 1, 2, 3 };
        var token = new AuthenticationToken { AllowedServiceIds = serviceIds };
        token.AllowedServiceIds.Should().BeEquivalentTo(serviceIds);
    }

    [Fact]
    public void AllowedServiceIds_DefaultValue_IsEmptyList()
    {
        var token = new AuthenticationToken();
        token.AllowedServiceIds.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void AllowAllServices_GetSet_RoundtripsValue()
    {
        var token = new AuthenticationToken { AllowAllServices = true };
        token.AllowAllServices.Should().BeTrue();
    }

    [Fact]
    public void AllowAllServices_DefaultValue_IsFalse()
    {
        var token = new AuthenticationToken();
        token.AllowAllServices.Should().BeFalse();
    }

    [Fact]
    public void CreatedAt_GetSet_RoundtripsValue()
    {
        var now = DateTime.UtcNow;
        var token = new AuthenticationToken { CreatedAt = now };
        token.CreatedAt.Should().Be(now);
    }

    [Fact]
    public void CreatedAt_DefaultValue_IsUtcNow()
    {
        var token = new AuthenticationToken();
        // CreatedAt has a default value of DateTime.UtcNow
        token.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ExpiresAt_GetSet_RoundtripsValue()
    {
        var future = DateTime.UtcNow.AddHours(1);
        var token = new AuthenticationToken { ExpiresAt = future };
        token.ExpiresAt.Should().Be(future);
    }

    [Fact]
    public void ExpiresAt_CanBeNull()
    {
        var token = new AuthenticationToken { ExpiresAt = null };
        token.ExpiresAt.Should().BeNull();
    }

    [Fact]
    public void LastUsedAt_GetSet_RoundtripsValue()
    {
        var past = DateTime.UtcNow.AddMinutes(-30);
        var token = new AuthenticationToken { LastUsedAt = past };
        token.LastUsedAt.Should().Be(past);
    }

    [Fact]
    public void LastUsedAt_CanBeNull()
    {
        var token = new AuthenticationToken { LastUsedAt = null };
        token.LastUsedAt.Should().BeNull();
    }

    [Fact]
    public void UsageCount_GetSet_RoundtripsValue()
    {
        var token = new AuthenticationToken { UsageCount = 100 };
        token.UsageCount.Should().Be(100);
    }

    [Fact]
    public void UsageCount_DefaultValue_IsZero()
    {
        var token = new AuthenticationToken();
        token.UsageCount.Should().Be(0);
    }

    [Fact]
    public void IsRevoked_GetSet_RoundtripsValue()
    {
        var token = new AuthenticationToken { IsRevoked = true };
        token.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public void IsRevoked_DefaultValue_IsFalse()
    {
        var token = new AuthenticationToken();
        token.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public void RevokedReason_GetSet_RoundtripsValue()
    {
        var token = new AuthenticationToken { RevokedReason = "Compromised" };
        token.RevokedReason.Should().Be("Compromised");
    }

    [Fact]
    public void RevokedReason_CanBeNull()
    {
        var token = new AuthenticationToken { RevokedReason = null };
        token.RevokedReason.Should().BeNull();
    }

    [Fact]
    public void IsActive_GetSet_RoundtripsValue()
    {
        var token = new AuthenticationToken { IsActive = false };
        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_DefaultValue_IsTrue()
    {
        var token = new AuthenticationToken();
        token.IsActive.Should().BeTrue();
    }

    [Fact]
    public void IpWhitelistCsv_GetSet_RoundtripsValue()
    {
        var token = new AuthenticationToken { IpWhitelistCsv = "192.168.1.1,10.0.0.1" };
        token.IpWhitelistCsv.Should().Be("192.168.1.1,10.0.0.1");
    }

    [Fact]
    public void IpWhitelistCsv_CanBeNull()
    {
        var token = new AuthenticationToken { IpWhitelistCsv = null };
        token.IpWhitelistCsv.Should().BeNull();
    }

    [Fact]
    public void UserAgent_GetSet_RoundtripsValue()
    {
        var token = new AuthenticationToken { UserAgent = "Postman/1.0" };
        token.UserAgent.Should().Be("Postman/1.0");
    }

    [Fact]
    public void UserAgent_CanBeNull()
    {
        var token = new AuthenticationToken { UserAgent = null };
        token.UserAgent.Should().BeNull();
    }

    [Fact]
    public void Validate_WithValidToken_DoesNotThrow()
    {
        var token = CreateValidToken();
        var act = () => token.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithNullTokenHash_ThrowsInvalidOperationException()
    {
        var token = CreateValidToken();
        token.TokenHash = null!;
        Action act = () => token.Validate();
        act.Should().Throw<InvalidOperationException>("Token hash is required");
    }

    [Fact]
    public void Validate_WithEmptyTokenHash_ThrowsInvalidOperationException()
    {
        var token = CreateValidToken();
        token.TokenHash = "   ";
        Action act = () => token.Validate();
        act.Should().Throw<InvalidOperationException>("Token hash is required");
    }

    [Fact]
    public void Validate_WithNullClientName_ThrowsInvalidOperationException()
    {
        var token = CreateValidToken();
        token.ClientName = null!;
        Action act = () => token.Validate();
        act.Should().Throw<InvalidOperationException>("Client name is required");
    }

    [Fact]
    public void Validate_WithEmptyClientName_ThrowsInvalidOperationException()
    {
        var token = CreateValidToken();
        token.ClientName = "   ";
        Action act = () => token.Validate();
        act.Should().Throw<InvalidOperationException>("Client name is required");
    }

    [Fact]
    public void Validate_WithNullClientId_ThrowsInvalidOperationException()
    {
        var token = CreateValidToken();
        token.ClientId = null!;
        Action act = () => token.Validate();
        act.Should().Throw<InvalidOperationException>("Client ID is required");
    }

    [Fact]
    public void Validate_WithEmptyClientId_ThrowsInvalidOperationException()
    {
        var token = CreateValidToken();
        token.ClientId = "   ";
        Action act = () => token.Validate();
        act.Should().Throw<InvalidOperationException>("Client ID is required");
    }

    [Fact]
    public void Validate_WithNoAllowedServicesAndAllowAllFalse_ThrowsInvalidOperationException()
    {
        var token = CreateValidToken();
        token.AllowedServiceIds = new List<int>();
        token.AllowAllServices = false;
        Action act = () => token.Validate();
        act.Should().Throw<InvalidOperationException>("Token must allow all services or specify allowed services");
    }

    [Fact]
    public void Validate_WithPastExpirationDate_ThrowsInvalidOperationException()
    {
        var token = CreateValidToken();
        token.ExpiresAt = DateTime.UtcNow.AddHours(-1);
        Action act = () => token.Validate();
        act.Should().Throw<InvalidOperationException>("Token expiration date cannot be in the past");
    }

    [Fact]
    public void IsExpired_WithFutureExpiration_ReturnsFalse()
    {
        var token = CreateValidToken();
        token.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WithPastExpiration_ReturnsTrue()
    {
        var token = CreateValidToken();
        token.ExpiresAt = DateTime.UtcNow.AddHours(-1);
        token.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WithNoExpiration_ReturnsFalse()
    {
        var token = CreateValidToken();
        token.ExpiresAt = null;
        token.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithActiveNonRevokedNonExpiredToken_ReturnsTrue()
    {
        var token = CreateValidToken();
        token.IsValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithInactiveToken_ReturnsFalse()
    {
        var token = CreateValidToken();
        token.IsActive = false;
        token.IsValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithRevokedToken_ReturnsFalse()
    {
        var token = CreateValidToken();
        token.IsRevoked = true;
        token.IsValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithExpiredToken_ReturnsFalse()
    {
        var token = CreateValidToken();
        token.ExpiresAt = DateTime.UtcNow.AddHours(-1);
        token.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CanAccessService_WithAllowAllServices_ReturnsTrue()
    {
        var token = CreateValidToken();
        token.AllowAllServices = true;
        token.CanAccessService(999).Should().BeTrue();
    }

    [Fact]
    public void CanAccessService_WithMatchingServiceId_ReturnsTrue()
    {
        var token = CreateValidToken();
        token.CanAccessService(100).Should().BeTrue();
    }

    [Fact]
    public void CanAccessService_WithNonMatchingServiceId_ReturnsFalse()
    {
        var token = CreateValidToken();
        token.CanAccessService(999).Should().BeFalse();
    }

    [Fact]
    public void CanAccessService_WithEmptyAllowedListAndAllowAllFalse_ReturnsFalse()
    {
        var token = CreateValidToken();
        token.AllowedServiceIds = new List<int>();
        token.AllowAllServices = false;
        token.CanAccessService(100).Should().BeFalse();
    }

    [Fact]
    public void RecordUsage_UpdatesUsageCountAndLastUsedAt()
    {
        var token = CreateValidToken();
        var beforeCount = token.UsageCount;
        var beforeLastUsed = token.LastUsedAt;

        token.RecordUsage("New-Agent/2.0");

        token.UsageCount.Should().Be(beforeCount + 1);
        token.LastUsedAt.Should().NotBe(beforeLastUsed);
        token.UserAgent.Should().Be("New-Agent/2.0");
    }

    [Fact]
    public void RecordUsage_WithNullUserAgent_DoesNotUpdateUserAgent()
    {
        var token = CreateValidToken();
        token.UserAgent = "Old-Agent/1.0";

        token.RecordUsage(null);

        token.UserAgent.Should().Be("Old-Agent/1.0");
    }

    [Fact]
    public void RecordUsage_WithEmptyUserAgent_DoesNotUpdateUserAgent()
    {
        var token = CreateValidToken();
        token.UserAgent = "Old-Agent/1.0";

        token.RecordUsage("");

        token.UserAgent.Should().Be("Old-Agent/1.0");
    }

    [Fact]
    public void Revoke_WithReason_SetsRevokedProperties()
    {
        var token = CreateValidToken();
        var beforeRevokedAt = token.RevokedAt;

        token.Revoke("Compromised key");

        token.IsRevoked.Should().BeTrue();
        token.RevokedReason.Should().Be("Compromised key");
        token.RevokedAt.Should().NotBe(beforeRevokedAt);
    }

    [Fact]
    public void Revoke_WithoutReason_SetsRevokedProperties()
    {
        var token = CreateValidToken();
        var beforeRevokedAt = token.RevokedAt;

        token.Revoke();

        token.IsRevoked.Should().BeTrue();
        token.RevokedReason.Should().BeNull();
        token.RevokedAt.Should().NotBe(beforeRevokedAt);
    }

    [Fact]
    public void IsIpAllowed_WithNullIpWhitelist_ReturnsTrue()
    {
        var token = CreateValidToken();
        token.IpWhitelistCsv = null;
        token.IsIpAllowed("192.168.1.100").Should().BeTrue();
    }

    [Fact]
    public void IsIpAllowed_WithEmptyIpWhitelist_ReturnsTrue()
    {
        var token = CreateValidToken();
        token.IpWhitelistCsv = "";
        token.IsIpAllowed("192.168.1.100").Should().BeTrue();
    }

    [Fact]
    public void IsIpAllowed_WithWhitelistedIp_ReturnsTrue()
    {
        var token = CreateValidToken();
        token.IsIpAllowed("192.168.1.1").Should().BeTrue();
    }

    [Fact]
    public void IsIpAllowed_WithNonWhitelistedIp_ReturnsFalse()
    {
        var token = CreateValidToken();
        token.IsIpAllowed("192.168.2.1").Should().BeFalse();
    }

    [Fact]
    public void IsIpAllowed_WithExactIpMatch_ReturnsTrue()
    {
        var token = CreateValidToken();
        token.IsIpAllowed("192.168.1.1").Should().BeTrue();
    }

    [Fact]
    public void IsIpAllowed_WithNonMatchingIp_ReturnsFalse()
    {
        var token = CreateValidToken();
        token.IsIpAllowed("192.168.2.1").Should().BeFalse();
    }
}