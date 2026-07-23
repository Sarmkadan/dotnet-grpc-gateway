using System;
using System.Collections.Generic;
using System.Linq;
using DotnetGrpcGateway.Domain;
using Xunit;

namespace DotnetGrpcGateway.Tests;

public class ServiceReflectionInfoExtensionsTests
{
    private static ServiceReflectionInfo CreateInfo(
        string serviceName = "TestService",
        string serviceFullName = "Test.Namespace.TestService",
        IEnumerable<ServiceMethodDescriptor>? methods = null,
        bool isAvailable = true,
        DateTime? reflectedAt = null,
        string? errorMessage = null)
    {
        return new ServiceReflectionInfo
        {
            ServiceName = serviceName,
            ServiceFullName = serviceFullName,
            Methods = (methods ?? Enumerable.Empty<ServiceMethodDescriptor>()).ToList(),
            IsAvailable = isAvailable,
            ReflectedAt = reflectedAt ?? DateTime.UtcNow,
            ErrorMessage = errorMessage
        };
    }

    private static ServiceMethodDescriptor CreateMethod(string name) =>
        new ServiceMethodDescriptor { Name = name };

    [Fact]
    public void ToSummaryString_ReturnsFormattedString()
    {
        // Arrange
        var info = CreateInfo(
            methods: new[]
            {
                CreateMethod("MethodA"),
                CreateMethod("MethodB")
            },
            reflectedAt: new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc));

        // Act
        var result = info.ToSummaryString();

        // Assert
        Assert.Contains(info.ServiceName, result);
        Assert.Contains(info.ServiceFullName, result);
        Assert.Contains("Methods: 2", result);
        Assert.Contains($"Available: {info.IsAvailable}", result);
        Assert.Contains("ReflectedAt: 2023-01-01 12:00:00Z", result);
    }

    [Fact]
    public void ToSummaryString_NullInfo_ThrowsArgumentNullException()
    {
        // Arrange
        ServiceReflectionInfo? info = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => info!.ToSummaryString());
    }

    [Fact]
    public void GetMethodNames_ReturnsOnlyNonEmptyNames()
    {
        // Arrange
        var info = CreateInfo(
            methods: new[]
            {
                CreateMethod("Alpha"),
                CreateMethod(string.Empty),
                CreateMethod(null!),
                CreateMethod("Beta")
            });

        // Act
        var names = info.GetMethodNames().ToList();

        // Assert
        Assert.Equal(2, names.Count);
        Assert.Contains("Alpha", names);
        Assert.Contains("Beta", names);
    }

    [Fact]
    public void GetMethodNames_NullInfo_ThrowsArgumentNullException()
    {
        ServiceReflectionInfo? info = null;
        Assert.Throws<ArgumentNullException>(() => info!.GetMethodNames());
    }

    [Fact]
    public void IsHealthy_ReturnsTrueWhenAvailableAndNoError()
    {
        var info = CreateInfo(isAvailable: true, errorMessage: null);
        Assert.True(info.IsHealthy());
    }

    [Fact]
    public void IsHealthy_ReturnsFalseWhenNotAvailable()
    {
        var info = CreateInfo(isAvailable: false, errorMessage: null);
        Assert.False(info.IsHealthy());
    }

    [Fact]
    public void IsHealthy_ReturnsFalseWhenErrorMessagePresent()
    {
        var info = CreateInfo(isAvailable: true, errorMessage: "boom");
        Assert.False(info.IsHealthy());
    }

    [Fact]
    public void GetMethodByName_Found_ReturnsDescriptor()
    {
        var method = CreateMethod("TargetMethod");
        var info = CreateInfo(methods: new[] { method });

        var result = info.GetMethodByName("TargetMethod");

        Assert.NotNull(result);
        Assert.Same(method, result);
    }

    [Fact]
    public void GetMethodByName_NotFound_ReturnsNull()
    {
        var info = CreateInfo(methods: new[] { CreateMethod("Other") });

        var result = info.GetMethodByName("Missing");

        Assert.Null(result);
    }

    [Fact]
    public void GetMethodByName_NullInfo_ThrowsArgumentNullException()
    {
        ServiceReflectionInfo? info = null;
        Assert.Throws<ArgumentNullException>(() => info!.GetMethodByName("any"));
    }

    [Fact]
    public void GetMethodByName_NullOrEmptyName_ThrowsArgumentException()
    {
        var info = CreateInfo();

        Assert.Throws<ArgumentException>(() => info.GetMethodByName(null!));
        Assert.Throws<ArgumentException>(() => info.GetMethodByName(string.Empty));
    }
}
