#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Xunit;
using DotNetGrpcGateway.Utilities;

namespace DotNetGrpcGateway.Tests;

public class ConfigurationUtilityTests
{
    private IConfiguration BuildConfiguration(IEnumerable<KeyValuePair<string, string?>>? data = null)
    {
        var builder = new ConfigurationBuilder();
        if (data != null)
        {
            builder.AddInMemoryCollection(data);
        }
        return builder.Build();
    }

    private class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = string.Empty;
        public string ContentRootPath { get; set; } = string.Empty;
        public Microsoft.Extensions.FileProviders.IFileProvider? ContentRootFileProvider { get; set; }
        public string? WebRootPath { get; set; }
        public Microsoft.Extensions.FileProviders.IFileProvider? WebRootFileProvider { get; set; }
    }

    [Fact]
    public void GetConfigValue_ReturnsTypedValue_WhenPresent()
    {
        var config = BuildConfiguration(new[]
        {
            new KeyValuePair<string, string?>("MyInt", "42")
        });

        int result = ConfigurationUtility.GetConfigValue(config, "MyInt", 0);
        Assert.Equal(42, result);
    }

    [Fact]
    public void GetConfigValue_ReturnsDefault_WhenMissingOrInvalid()
    {
        var config = BuildConfiguration(new[]
        {
            new KeyValuePair<string, string?>("BadInt", "not-an-int")
        });

        // missing key
        int missing = ConfigurationUtility.GetConfigValue(config, "Missing", -1);
        Assert.Equal(-1, missing);

        // invalid conversion
        int invalid = ConfigurationUtility.GetConfigValue(config, "BadInt", -2);
        Assert.Equal(-2, invalid);
    }

    [Fact]
    public void GetBoolValue_ParsesCorrectly_AndFallsBack()
    {
        var config = BuildConfiguration(new[]
        {
            new KeyValuePair<string, string?>("FlagTrue", "true"),
            new KeyValuePair<string, string?>("FlagFalse", "false"),
            new KeyValuePair<string, string?>("FlagBad", "notabool")
        });

        Assert.True(ConfigurationUtility.GetBoolValue(config, "FlagTrue"));
        Assert.False(ConfigurationUtility.GetBoolValue(config, "FlagFalse"));
        // invalid value falls back to default (true supplied)
        Assert.True(ConfigurationUtility.GetBoolValue(config, "FlagBad", true));
        // missing key falls back to default (false supplied)
        Assert.False(ConfigurationUtility.GetBoolValue(config, "MissingKey", false));
    }

    [Fact]
    public void GetIntValue_ParsesAndDefaults()
    {
        var config = BuildConfiguration(new[]
        {
            new KeyValuePair<string, string?>("Number", "123"),
            new KeyValuePair<string, string?>("BadNumber", "abc")
        });

        Assert.Equal(123, ConfigurationUtility.GetIntValue(config, "Number"));
        Assert.Equal(0, ConfigurationUtility.GetIntValue(config, "Missing"));
        Assert.Equal(5, ConfigurationUtility.GetIntValue(config, "BadNumber", 5));
    }

    [Fact]
    public void GetTimeSpanValue_ParsesVariousFormats()
    {
        var config = BuildConfiguration(new[]
        {
            new KeyValuePair<string, string?>("Span1", "00:01:30"),
            new KeyValuePair<string, string?>("Span2", "90"),
            new KeyValuePair<string, string?>("BadSpan", "notatime")
        });

        Assert.Equal(TimeSpan.FromMinutes(1.5), ConfigurationUtility.GetTimeSpanValue(config, "Span1", TimeSpan.Zero));
        Assert.Equal(TimeSpan.FromSeconds(90), ConfigurationUtility.GetTimeSpanValue(config, "Span2", TimeSpan.Zero));
        // fallback to default when parsing fails
        var defaultSpan = TimeSpan.FromHours(1);
        Assert.Equal(defaultSpan, ConfigurationUtility.GetTimeSpanValue(config, "BadSpan", defaultSpan));
    }

    private class SampleSection
    {
        public string? Name { get; set; }
        public int Value { get; set; }
    }

    [Fact]
    public void GetSection_BindsToObject_WhenSectionExists()
    {
        var config = BuildConfiguration(new[]
        {
            new KeyValuePair<string, string?>("Sample:Name", "Test"),
            new KeyValuePair<string, string?>("Sample:Value", "7")
        });

        var result = ConfigurationUtility.GetSection<SampleSection>(config, "Sample");
        Assert.NotNull(result);
        Assert.Equal("Test", result!.Name);
        Assert.Equal(7, result.Value);
    }

    [Fact]
    public void GetSection_ReturnsNull_WhenSectionMissing()
    {
        var config = BuildConfiguration();
        var result = ConfigurationUtility.GetSection<SampleSection>(config, "Missing");
        Assert.Null(result);
    }

    [Fact]
    public void ValidateRequiredKey_ReturnsExpectedValues()
    {
        var config = BuildConfiguration(new[]
        {
            new KeyValuePair<string, string?>("Present", "value"),
            new KeyValuePair<string, string?>("Empty", "")
        });

        Assert.True(ConfigurationUtility.ValidateRequiredKey(config, "Present"));
        Assert.False(ConfigurationUtility.ValidateRequiredKey(config, "Empty"));
        Assert.False(ConfigurationUtility.ValidateRequiredKey(config, "Missing"));
    }

    [Fact]
    public void GetAllKeys_And_GetKeysMatchingPattern_WorkCorrectly()
    {
        var config = BuildConfiguration(new[]
        {
            new KeyValuePair<string, string?>("Parent:Child1", "a"),
            new KeyValuePair<string, string?>("Parent:Child2", "b"),
            new KeyValuePair<string, string?>("Other", "c")
        });

        var allKeys = ConfigurationUtility.GetAllKeys(config).ToList();
        Assert.Contains("Parent:Child1", allKeys);
        Assert.Contains("Parent:Child2", allKeys);
        Assert.Contains("Other", allKeys);
        Assert.Contains("Parent", allKeys); // top‑level key

        var matching = ConfigurationUtility.GetKeysMatchingPattern(config, "child").ToList();
        Assert.Contains("Parent:Child1", matching, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("Parent:Child2", matching, StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain("Other", matching);
    }

    [Fact]
    public void MergeConfigurations_RespectsPriority_Order()
    {
        var source1 = new Dictionary<string, string?> { ["A"] = "1", ["B"] = "2" };
        var source2 = new Dictionary<string, string?> { ["B"] = "override", ["C"] = "3" };
        var merged = ConfigurationUtility.MergeConfigurations(source1, source2);

        // source1 has higher priority (first argument)
        Assert.Equal("1", merged["A"]);
        Assert.Equal("2", merged["B"]);
        Assert.Equal("3", merged["C"]);
    }

    [Fact]
    public void IsDevelopment_And_IsProduction_ReturnCorrectValues()
    {
        var devEnv = new TestWebHostEnvironment { EnvironmentName = Environments.Development };
        var prodEnv = new TestWebHostEnvironment { EnvironmentName = Environments.Production };
        var unknownEnv = new TestWebHostEnvironment { EnvironmentName = "Staging" };

        Assert.True(ConfigurationUtility.IsDevelopment(devEnv));
        Assert.False(ConfigurationUtility.IsDevelopment(prodEnv));
        Assert.False(ConfigurationUtility.IsDevelopment(unknownEnv));

        Assert.True(ConfigurationUtility.IsProduction(prodEnv));
        Assert.False(ConfigurationUtility.IsProduction(devEnv));
        Assert.False(ConfigurationUtility.IsProduction(unknownEnv));
    }

    [Fact]
    public void Methods_Throw_OnNullConfiguration()
    {
        IConfiguration? nullConfig = null;

        Assert.Throws<NullReferenceException>(() => ConfigurationUtility.GetConfigValue<int>(nullConfig!, "key", 0));
        Assert.Throws<NullReferenceException>(() => ConfigurationUtility.GetBoolValue(nullConfig!, "key"));
        Assert.Throws<NullReferenceException>(() => ConfigurationUtility.GetIntValue(nullConfig!, "key"));
        Assert.Throws<NullReferenceException>(() => ConfigurationUtility.GetTimeSpanValue(nullConfig!, "key", TimeSpan.Zero));
        Assert.Throws<NullReferenceException>(() => ConfigurationUtility.GetSection<object>(nullConfig!, "section"));
        Assert.Throws<NullReferenceException>(() => ConfigurationUtility.ValidateRequiredKey(nullConfig!, "key"));
        Assert.Throws<NullReferenceException>(() => ConfigurationUtility.GetKeysMatchingPattern(nullConfig!, "pattern"));
        Assert.Throws<NullReferenceException>(() => ConfigurationUtility.GetAllKeys(nullConfig!));
    }
}
