using System;
using System.Collections.Generic;
using DotNetGrpcGateway.Domain;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class GatewayConfigurationValidationTests
{
    private static GatewayConfiguration CreateValidConfiguration()
    {
        return new GatewayConfiguration
        {
            Id = 0,
            Name = "ValidName",
            Description = "A valid description",
            ListenAddress = "0.0.0.0",
            Port = 5000,
            MaxConcurrentConnections = 10,
            RequestTimeoutMs = 5000,
            MaxMessageSize = 1024 * 1024,
            EnableCorsPolicy = false,
            CorsOrigins = null,
            EnableCompressionByDefault = false,
            CompressionAlgorithm = null,
            LogLevel = "Information",
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public void Validate_ValidConfiguration_ReturnsEmptyList()
    {
        // Arrange
        var config = CreateValidConfiguration();

        // Act
        IReadOnlyList<string> errors = GatewayConfigurationValidation.Validate(config);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void IsValid_ValidConfiguration_ReturnsTrue()
    {
        // Arrange
        var config = CreateValidConfiguration();

        // Act
        bool isValid = GatewayConfigurationValidation.IsValid(config);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void EnsureValid_ValidConfiguration_DoesNotThrow()
    {
        // Arrange
        var config = CreateValidConfiguration();

        // Act / Assert
        var exception = Record.Exception(() => GatewayConfigurationValidation.EnsureValid(config));
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_NullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => GatewayConfigurationValidation.Validate(null!));
    }

    [Fact]
    public void EnsureValid_NullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => GatewayConfigurationValidation.EnsureValid(null!));
    }

    [Fact]
    public void Validate_InvalidConfiguration_ReturnsExpectedErrors()
    {
        // Arrange: create a configuration that violates several rules
        var config = new GatewayConfiguration
        {
            Id = -1,                                   // invalid
            Name = "",                                 // invalid
            Description = new string('x', 3000),      // too long
            ListenAddress = "",                       // invalid
            Port = 70000,                              // out of range
            MaxConcurrentConnections = 0,             // invalid
            RequestTimeoutMs = 50,                    // too low
            MaxMessageSize = 500,                     // too low
            EnableCorsPolicy = true,
            CorsOrigins = null,                       // required when CORS enabled
            EnableCompressionByDefault = true,
            CompressionAlgorithm = null,              // required when compression enabled
            LogLevel = "",                             // invalid
            CreatedAt = default,                      // invalid
            ModifiedAt = default                       // invalid
        };

        // Act
        IReadOnlyList<string> errors = GatewayConfigurationValidation.Validate(config);

        // Assert
        var expectedMessages = new[]
        {
            "Id must be a non-negative integer, but was -1",
            "Name is required and cannot be null or whitespace",
            "Description cannot exceed 2048 characters",
            "ListenAddress is required and cannot be null or whitespace",
            "Port must be between 1 and 65535",
            "MaxConcurrentConnections must be greater than 0",
            "RequestTimeoutMs must be at least 100 milliseconds",
            "MaxMessageSize must be at least 1024 bytes (1KB)",
            "CorsOrigins is required when EnableCorsPolicy is true",
            "CompressionAlgorithm is required when EnableCompressionByDefault is true",
            "LogLevel is required and cannot be null or whitespace",
            "CreatedAt must be set to a valid DateTime",
            "ModifiedAt must be set to a valid DateTime"
        };

        Assert.Equal(expectedMessages.Length, errors.Count);
        foreach (var expected in expectedMessages)
        {
            Assert.Contains(expected, errors);
        }
    }

    [Fact]
    public void EnsureValid_InvalidConfiguration_ThrowsArgumentException()
    {
        // Arrange
        var config = new GatewayConfiguration
        {
            Id = -5,
            Name = null!,
            ListenAddress = "addr",
            Port = 0,
            MaxConcurrentConnections = 0,
            RequestTimeoutMs = 0,
            MaxMessageSize = 0,
            EnableCorsPolicy = false,
            EnableCompressionByDefault = false,
            LogLevel = null!,
            CreatedAt = default,
            ModifiedAt = default
        };

        // Act
        var ex = Assert.Throws<ArgumentException>(() => GatewayConfigurationValidation.EnsureValid(config));

        // Assert
        Assert.Contains("GatewayConfiguration validation failed", ex.Message);
        // At least a few specific error messages should be present
        Assert.Contains("Id must be a non-negative integer", ex.Message);
        Assert.Contains("Name is required and cannot be null or whitespace", ex.Message);
        Assert.Contains("Port must be between 1 and 65535", ex.Message);
    }
}
