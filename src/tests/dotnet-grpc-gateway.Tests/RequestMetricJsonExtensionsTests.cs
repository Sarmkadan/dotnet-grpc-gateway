// SPDX-License-Identifier: MIT
// Tests for RequestMetricJsonExtensions
// ------------------------------------------------
// These tests target the public extension methods defined in
// DotNetGrpcGateway.Domain.RequestMetricJsonExtensions.
// ------------------------------------------------

using System;
using System.Text;
using System.Text.Json;
using DotNetGrpcGateway.Domain;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public sealed class RequestMetricJsonExtensionsTests
{
    private static RequestMetric CreateValidMetric()
        => new RequestMetric
        {
            ServiceName = "TestService",
            MethodName = "TestMethod",
            ClientIpAddress = "127.0.0.1",
            // optional fields – populate with plausible values to keep validation happy
            DurationMs = 123,
            Timestamp = DateTimeOffset.UtcNow,
            // any other properties can be left at their defaults
        };

    [Fact]
    public void ToJson_WithValidMetric_ReturnsJson()
    {
        // Arrange
        var metric = CreateValidMetric();

        // Act
        var json = metric.ToJson();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(json));
        Assert.Contains("\"serviceName\":\"TestService\"", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"methodName\":\"TestMethod\"", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"clientIpAddress\":\"127.0.0.1\"", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ToJson_WithIndentation_ProducesIndentedJson()
    {
        // Arrange
        var metric = CreateValidMetric();

        // Act
        var json = metric.ToJson(indented: true);

        // Assert
        // Indented JSON contains line breaks
        Assert.Contains(Environment.NewLine, json);
    }

    [Fact]
    public void ToJson_NullMetric_ThrowsArgumentNullException()
    {
        // Arrange
        RequestMetric? metric = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => metric!.ToJson());
    }

    [Fact]
    public void FromJson_WithValidJson_ReturnsMetric()
    {
        // Arrange
        var original = CreateValidMetric();
        var json = original.ToJson();

        // Act
        var deserialized = RequestMetricJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.ServiceName, deserialized!.ServiceName);
        Assert.Equal(original.MethodName, deserialized.MethodName);
        Assert.Equal(original.ClientIpAddress, deserialized.ClientIpAddress);
    }

    [Fact]
    public void FromJson_NullOrWhiteSpace_ReturnsNull()
    {
        // Act
        var result1 = RequestMetricJsonExtensions.FromJson(null!);
        var result2 = RequestMetricJsonExtensions.FromJson(string.Empty);
        var result3 = RequestMetricJsonExtensions.FromJson("   ");

        // Assert
        Assert.Null(result1);
        Assert.Null(result2);
        Assert.Null(result3);
    }

    [Fact]
    public void FromJson_ExceedsMaxSize_ThrowsJsonException()
    {
        // Arrange: create a JSON string just over the 1 MB limit
        var oversizedPayload = new string('a', RequestMetricJsonExtensionsTestsHelper.MaxJsonPayloadSizeBytes + 1);
        var json = $"{{\"serviceName\":\"{oversizedPayload}\"}}";

        // Act & Assert
        Assert.Throws<JsonException>(() => RequestMetricJsonExtensions.FromJson(json));
    }

    [Fact]
    public void TryFromJson_WithValidJson_ReturnsTrueAndMetric()
    {
        // Arrange
        var original = CreateValidMetric();
        var json = original.ToJson();

        // Act
        var success = RequestMetricJsonExtensions.TryFromJson(json, out var metric);

        // Assert
        Assert.True(success);
        Assert.NotNull(metric);
        Assert.Equal(original.ServiceName, metric!.ServiceName);
    }

    [Fact]
    public void TryFromJson_EmptyString_ReturnsFalse()
    {
        // Act
        var success = RequestMetricJsonExtensions.TryFromJson(string.Empty, out var metric);

        // Assert
        Assert.False(success);
        Assert.Null(metric);
    }

    [Fact]
    public void TryFromJson_ExceedsMaxSize_ReturnsFalse()
    {
        // Arrange: payload just over the limit
        var oversizedPayload = new string('b', RequestMetricJsonExtensionsTestsHelper.MaxJsonPayloadSizeBytes + 1);
        var json = $"{{\"serviceName\":\"{oversizedPayload}\"}}";

        // Act
        var success = RequestMetricJsonExtensions.TryFromJson(json, out var metric);

        // Assert
        Assert.False(success);
        Assert.Null(metric);
    }
}

// Helper class to expose the internal constant for test calculations.
// The constant is private in the production code; mirroring its value here
// keeps the test independent from the implementation details while still
// exercising the same boundary.
internal static class RequestMetricJsonExtensionsTestsHelper
{
    public const int MaxJsonPayloadSizeBytes = 1_048_576; // 1 MB
}
