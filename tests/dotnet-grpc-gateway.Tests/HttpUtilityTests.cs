#nullable enable

using System.Net.Http;
using System.Net.Http.Headers;
using Xunit;
using DotNetGrpcGateway.Utilities;

namespace DotNetGrpcGateway.Tests;

public class HttpUtilityTests
{
    [Theory]
    [InlineData(null, "application/json", "application/json")]
    [InlineData("text/csv, application/json", "application/json", "text/csv")]
    [InlineData("application/xml", "application/json", "application/xml")]
    public void GetAcceptedContentType_ShouldReturnExpectedType(string? acceptHeader, string defaultType, string expected)
    {
        var result = HttpUtility.GetAcceptedContentType(acceptHeader, defaultType);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void BuildAuthorizationHeader_ValidToken_ReturnsBearerString()
    {
        var token = "mytoken";
        var result = HttpUtility.BuildAuthorizationHeader(token);
        Assert.Equal("Bearer mytoken", result);
    }

    [Fact]
    public void BuildAuthorizationHeader_EmptyToken_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => HttpUtility.BuildAuthorizationHeader(""));
    }

    [Fact]
    public void ExtractBearerToken_ValidHeader_ReturnsToken()
    {
        var header = "Bearer mytoken";
        var result = HttpUtility.ExtractBearerToken(header);
        Assert.Equal("mytoken", result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Basic mytoken")]
    [InlineData("mytoken")]
    public void ExtractBearerToken_InvalidHeader_ReturnsNull(string? header)
    {
        var result = HttpUtility.ExtractBearerToken(header);
        Assert.Null(result);
    }

    [Fact]
    public void AddGrpcWebHeaders_AddsCorrectHeaders()
    {
        using var request = new HttpRequestMessage();
        var headers = request.Headers;
        
        HttpUtility.AddGrpcWebHeaders(headers);
        
        Assert.True(headers.Contains("x-grpc-web"));
        Assert.Equal("1", headers.GetValues("x-grpc-web").First());
        Assert.True(headers.Contains("x-user-agent"));
        Assert.Equal("grpc-web-dotnet/1.0", headers.GetValues("x-user-agent").First());
    }

    [Theory]
    [InlineData(200, true, false, false, "Success")]
    [InlineData(404, false, true, false, "Client Error")]
    [InlineData(500, false, false, true, "Server Error")]
    public void HttpStatusCodeChecks_ReturnExpectedResults(int statusCode, bool isSuccess, bool isClientError, bool isServerError, string category)
    {
        Assert.Equal(isSuccess, HttpUtility.IsSuccessStatusCode(statusCode));
        Assert.Equal(isClientError, HttpUtility.IsClientError(statusCode));
        Assert.Equal(isServerError, HttpUtility.IsServerError(statusCode));
        Assert.Equal(category, HttpUtility.GetStatusCodeCategory(statusCode));
    }

    [Theory]
    [InlineData("application/json", true, false)]
    [InlineData("application/ld+json", true, false)]
    [InlineData("application/xml", false, true)]
    [InlineData("text/xml", false, true)]
    [InlineData("text/plain", false, false)]
    public void ContentTypeChecks_ReturnExpectedResults(string contentType, bool isJson, bool isXml)
    {
        Assert.Equal(isJson, HttpUtility.IsJsonContentType(contentType));
        Assert.Equal(isXml, HttpUtility.IsXmlContentType(contentType));
    }
}
