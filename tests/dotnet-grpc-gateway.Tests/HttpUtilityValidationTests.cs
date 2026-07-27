#nullable enable

using System;
using System.Collections.Generic;
using DotNetGrpcGateway.Utilities;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class HttpUtilityValidationTests
{
    // ---------- Validate (token) ----------
    [Fact]
    public void Validate_TokenIsValid_ReturnsEmpty()
    {
        // Arrange
        string token = "valid-token";

        // Act
        IReadOnlyList<string> result = HttpUtilityValidation.Validate(token);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_TokenIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => HttpUtilityValidation.Validate(null));
    }

    [Fact]
    public void Validate_TokenIsEmpty_ReturnsProblem()
    {
        // Act
        IReadOnlyList<string> result = HttpUtilityValidation.Validate(string.Empty);

        // Assert
        Assert.Single(result);
        Assert.Contains("Token cannot be null or empty", result[0]);
    }

    // ---------- ValidateAuthorizationHeader ----------
    [Fact]
    public void ValidateAuthorizationHeader_ValidBearer_ReturnsEmpty()
    {
        // Arrange
        string header = "Bearer abc123";

        // Act
        IReadOnlyList<string> result = HttpUtilityValidation.ValidateAuthorizationHeader(header);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateAuthorizationHeader_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => HttpUtilityValidation.ValidateAuthorizationHeader(null));
    }

    [Fact]
    public void ValidateAuthorizationHeader_InvalidScheme_ReturnsProblem()
    {
        // Arrange
        string header = "Basic abc123";

        // Act
        IReadOnlyList<string> result = HttpUtilityValidation.ValidateAuthorizationHeader(header);

        // Assert
        Assert.Single(result);
        Assert.Contains("Bearer scheme", result[0]);
    }

    // ---------- ValidateAcceptHeader ----------
    [Fact]
    public void ValidateAcceptHeader_Valid_ReturnsEmpty()
    {
        string header = "application/json";

        IReadOnlyList<string> result = HttpUtilityValidation.ValidateAcceptHeader(header);

        Assert.Empty(result);
    }

    [Fact]
    public void ValidateAcceptHeader_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => HttpUtilityValidation.ValidateAcceptHeader(null));
    }

    [Fact]
    public void ValidateAcceptHeader_TooLong_ReturnsProblem()
    {
        string header = new string('a', 1025); // 1 char over limit

        IReadOnlyList<string> result = HttpUtilityValidation.ValidateAcceptHeader(header);

        Assert.Single(result);
        Assert.Contains("maximum length of 1024", result[0]);
    }

    // ---------- ValidateStatusCode ----------
    [Theory]
    [InlineData(100)]
    [InlineData(200)]
    [InlineData(599)]
    public void ValidateStatusCode_ValidRange_ReturnsEmpty(int statusCode)
    {
        IReadOnlyList<string> result = HttpUtilityValidation.ValidateStatusCode(statusCode);
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(99)]
    [InlineData(600)]
    public void ValidateStatusCode_InvalidRange_ReturnsProblem(int statusCode)
    {
        IReadOnlyList<string> result = HttpUtilityValidation.ValidateStatusCode(statusCode);
        Assert.Single(result);
        Assert.Contains("Status code must be in range", result[0]);
    }

    // ---------- ValidateContentType ----------
    [Fact]
    public void ValidateContentType_Valid_ReturnsEmpty()
    {
        string ct = "application/json";

        IReadOnlyList<string> result = HttpUtilityValidation.ValidateContentType(ct);

        Assert.Empty(result);
    }

    [Fact]
    public void ValidateContentType_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => HttpUtilityValidation.ValidateContentType(null));
    }

    [Fact]
    public void ValidateContentType_TooLong_ReturnsProblem()
    {
        string ct = new string('a', 257); // 1 char over limit

        IReadOnlyList<string> result = HttpUtilityValidation.ValidateContentType(ct);

        Assert.Single(result);
        Assert.Contains("maximum length of 256", result[0]);
    }

    // ---------- IsValid helpers ----------
    [Fact]
    public void IsValid_TokenValid_ReturnsTrue()
    {
        Assert.True(HttpUtilityValidation.IsValid("token"));
    }

    [Fact]
    public void IsValid_TokenInvalid_ReturnsFalse()
    {
        Assert.False(HttpUtilityValidation.IsValid(string.Empty));
    }

    [Fact]
    public void IsValidAuthorizationHeader_Valid_ReturnsTrue()
    {
        Assert.True(HttpUtilityValidation.IsValidAuthorizationHeader("Bearer xyz"));
    }

    [Fact]
    public void IsValidAuthorizationHeader_Invalid_ReturnsFalse()
    {
        Assert.False(HttpUtilityValidation.IsValidAuthorizationHeader("Basic xyz"));
    }

    [Fact]
    public void IsValidAcceptHeader_Valid_ReturnsTrue()
    {
        Assert.True(HttpUtilityValidation.IsValidAcceptHeader("text/plain"));
    }

    [Fact]
    public void IsValidAcceptHeader_Invalid_ReturnsFalse()
    {
        Assert.False(HttpUtilityValidation.IsValidAcceptHeader(new string('a', 1025)));
    }

    [Fact]
    public void IsValidStatusCode_Valid_ReturnsTrue()
    {
        Assert.True(HttpUtilityValidation.IsValidStatusCode(200));
    }

    [Fact]
    public void IsValidStatusCode_Invalid_ReturnsFalse()
    {
        Assert.False(HttpUtilityValidation.IsValidStatusCode(99));
    }

    [Fact]
    public void IsValidContentType_Valid_ReturnsTrue()
    {
        Assert.True(HttpUtilityValidation.IsValidContentType("application/xml"));
    }

    [Fact]
    public void IsValidContentType_Invalid_ReturnsFalse()
    {
        Assert.False(HttpUtilityValidation.IsValidContentType(new string('a', 257)));
    }
}
