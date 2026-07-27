namespace DotNetGrpcGateway.Tests;

using System;
using System.Collections.Generic;
using System.Text.Json;
using DotNetGrpcGateway.Utilities;
using Xunit;

public class GatewayUtilitiesTests
{
    [Fact]
    public void GenerateRequestId_Returns32CharHexString()
    {
        var id = GatewayUtilities.GenerateRequestId();

        Assert.False(string.IsNullOrWhiteSpace(id));
        Assert.Equal(32, id.Length);
        // Should be valid hex characters
        Assert.Matches("^[0-9a-fA-F]{32}$", id);
    }

    [Fact]
    public void ToJson_ReturnsEmptyString_WhenObjectIsNull()
    {
        string json = GatewayUtilities.ToJson<object>(null);
        Assert.Equal(string.Empty, json);
    }

    private class SampleDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    [Fact]
    public void ToJson_SerializesObject_WithCamelCaseAndIndent()
    {
        var obj = new SampleDto { Id = 1, Name = "Test" };
        string json = GatewayUtilities.ToJson(obj);

        // Verify that the JSON is indented and uses camelCase property names
        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("id", out var idProp));
        Assert.Equal(1, idProp.GetInt32());
        Assert.True(doc.RootElement.TryGetProperty("name", out var nameProp));
        Assert.Equal("Test", nameProp.GetString());
        Assert.Contains("\n", json); // indented output contains line breaks
    }

    [Fact]
    public void FromJson_ReturnsDefault_WhenInputIsNullOrWhiteSpace()
    {
        SampleDto? result1 = GatewayUtilities.FromJson<SampleDto>(null);
        SampleDto? result2 = GatewayUtilities.FromJson<SampleDto>("   ");

        Assert.Null(result1);
        Assert.Null(result2);
    }

    [Fact]
    public void FromJson_DeserializesValidJson()
    {
        var json = "{\"id\":2,\"name\":\"Bob\"}";
        var obj = GatewayUtilities.FromJson<SampleDto>(json);

        Assert.NotNull(obj);
        Assert.Equal(2, obj!.Id);
        Assert.Equal("Bob", obj.Name);
    }

    [Fact]
    public void FromJson_ReturnsDefault_WhenJsonIsInvalid()
    {
        var obj = GatewayUtilities.FromJson<SampleDto>("{invalid json}");
        Assert.Null(obj);
    }

    [Fact]
    public void GetElapsedTime_ReturnsAbsoluteDifference()
    {
        var from = new DateTime(2023, 1, 1, 12, 0, 0);
        var toLater = from.AddMinutes(5);
        var toEarlier = from.AddMinutes(-3);

        var diff1 = GatewayUtilities.GetElapsedTime(from, toLater);
        var diff2 = GatewayUtilities.GetElapsedTime(toEarlier, from);

        Assert.Equal(TimeSpan.FromMinutes(5), diff1);
        Assert.Equal(TimeSpan.FromMinutes(3), diff2);
    }

    [Theory]
    [InlineData(500, "500.00ms")]
    [InlineData(1500, "1.50s")]
    [InlineData(120000, "2.00m")]
    public void FormatDuration_FormatsCorrectly(double ms, string expected)
    {
        var result = GatewayUtilities.FormatDuration(ms);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(500L, "500.00 B")]
    [InlineData(2048L, "2.00 KB")]
    [InlineData(5L * 1024 * 1024, "5.00 MB")]
    [InlineData(3L * 1024 * 1024 * 1024, "3.00 GB")]
    public void FormatBytes_ConvertsToHumanReadable(long bytes, string expected)
    {
        var result = GatewayUtilities.FormatBytes(bytes);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void NormalizeServiceName_TrimAndReplaceSpaces()
    {
        var input = "  My Service Name  ";
        var normalized = GatewayUtilities.NormalizeServiceName(input);
        Assert.Equal("My.Service.Name", normalized);
    }

    [Fact]
    public void NormalizeServiceName_RemovesInvalidCharacters()
    {
        var input = "Service@#%Name!";
        var normalized = GatewayUtilities.NormalizeServiceName(input);
        Assert.Equal("ServiceName", normalized);
    }

    [Fact]
    public void NormalizeServiceName_Throws_WhenResultIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => GatewayUtilities.NormalizeServiceName("@@@"));
    }

    [Fact]
    public void NormalizeServiceName_Throws_WhenInputIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => GatewayUtilities.NormalizeServiceName(string.Empty));
    }

    [Fact]
    public void ComputeSha256Hash_KnownInputProducesExpectedHash()
    {
        var hash = GatewayUtilities.ComputeSha256Hash("test");
        // SHA256 of "test" in uppercase hex
        const string expected = "9F86D081884C7D659A2FEAA0C55AD015A3BF4F1B2B0B822CD15D6C15B0F00A08";
        Assert.Equal(expected, hash);
    }

    [Fact]
    public void GenerateRandomToken_ReturnsBase64String_AndIsDifferentOnSubsequentCalls()
    {
        var token1 = GatewayUtilities.GenerateRandomToken(16);
        var token2 = GatewayUtilities.GenerateRandomToken(16);

        Assert.False(string.IsNullOrWhiteSpace(token1));
        Assert.False(string.IsNullOrWhiteSpace(token2));
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void SafeGetValue_ReturnsDefault_WhenDictionaryIsNull()
    {
        Dictionary<string, int>? dict = null;
        var result = GatewayUtilities.SafeGetValue(dict, "key");
        Assert.Equal(default(int), result);
    }

    [Fact]
    public void SafeGetValue_ReturnsDefault_WhenKeyMissing()
    {
        var dict = new Dictionary<string, int> { { "a", 1 } };
        var result = GatewayUtilities.SafeGetValue(dict, "b");
        Assert.Equal(default(int), result);
    }

    [Fact]
    public void SafeGetValue_ReturnsValue_WhenKeyExists()
    {
        var dict = new Dictionary<string, int> { { "a", 42 } };
        var result = GatewayUtilities.SafeGetValue(dict, "a");
        Assert.Equal(42, result);
    }
}
