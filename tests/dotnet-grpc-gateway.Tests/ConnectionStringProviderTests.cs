using DotNetGrpcGateway.Infrastructure;
using FluentAssertions;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class ConnectionStringProviderTests
{
    private const string TestSecret = "Test-Only-Secret";

    private static ConnectionStringProvider CreateSut(string connectionString, string? secretStoreIdentifier = null) =>
        new(connectionString, secretStoreIdentifier);

    [Fact]
    public void GetRedactedConnectionString_PlainPassword_ReplacesPasswordOnly()
    {
        // Arrange
        var sut = CreateSut($"Server=db;Database=gateway;Password={TestSecret}");

        // Act
        var redacted = sut.GetRedactedConnectionString();

        // Assert
        redacted.Should().Be("server=db;database=gateway;password=[REDACTED]");
    }

    [Fact]
    public void GetRedactedConnectionString_QuotedPasswordContainingSemicolon_DoesNotLeakRemainder()
    {
        // Arrange
        var sut = CreateSut($"Server=db;Password=\"{TestSecret};tail\"");

        // Act
        var redacted = sut.GetRedactedConnectionString();

        // Assert
        redacted.Should().NotContain("tail").And.NotContain(TestSecret);
        redacted.Should().Be("server=db;password=[REDACTED]");
    }

    [Fact]
    public void GetRedactedConnectionString_InjectedKeyValuePairInsideQuotedValue_StaysPartOfValue()
    {
        // Arrange
        const string injected = "x;Trusted_Connection=True;";
        var sut = CreateSut($"Server=db;Password=\"{injected}\"");

        // Act
        var redacted = sut.GetRedactedConnectionString();

        // Assert
        redacted.Should().NotContain("Trusted_Connection").And.NotContain("x;");
        redacted.Should().Be("server=db;password=[REDACTED]");
    }

    [Fact]
    public void GetRedactedConnectionString_MalformedConnectionString_ReturnsFullyRedacted()
    {
        // Arrange
        var sut = CreateSut($"Server=db;Password=\"{TestSecret}");

        // Act
        var redacted = sut.GetRedactedConnectionString();

        // Assert
        redacted.Should().Be("[REDACTED]");
    }

    [Fact]
    public void ToString_WithSecret_NeverContainsConnectionString()
    {
        // Arrange
        var sut = CreateSut($"Server=db;Password={TestSecret}", "vault://gateway");

        // Act
        var text = sut.ToString();

        // Assert
        text.Should().NotContain(TestSecret);
        text.Should().Contain("vault://gateway");
    }
}
