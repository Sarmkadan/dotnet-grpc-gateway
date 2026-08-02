#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DotNetGrpcGateway.Formatters;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class OutputFormatterFactoryTests
{
    // A minimal IOutputFormatter implementation for testing.
    private sealed class TestFormatter : IOutputFormatter
    {
        public string ContentType { get; }

        public TestFormatter(string contentType) => ContentType = contentType;

        // The real interface likely has a method to write output; we provide a no‑op implementation.
        public Task WriteAsync(object? value, Stream output) => Task.CompletedTask;
    }

    private OutputFormatterFactory CreateFactory()
        => new OutputFormatterFactory(NullLogger<OutputFormatterFactory>.Instance);

    [Fact]
    public void Constructor_RegistersDefaultFormatters()
    {
        var factory = CreateFactory();

        var types = new HashSet<string>(factory.GetAvailableContentTypes(), StringComparer.OrdinalIgnoreCase);
        Assert.Contains("application/json", types);
        Assert.Contains("text/csv", types);
        Assert.Contains("application/xml", types);
    }

    [Fact]
    public void RegisterFormatter_AddsNewFormatter_And_GetFormatterReturnsIt()
    {
        var factory = CreateFactory();
        var custom = new TestFormatter("application/custom");

        factory.RegisterFormatter(custom);

        var retrieved = factory.GetFormatter("application/custom");
        Assert.Same(custom, retrieved);
    }

    [Fact]
    public void GetFormatter_NullOrEmpty_ReturnsJsonFormatter()
    {
        var factory = CreateFactory();

        var jsonFormatter = factory.GetFormatter("application/json");
        var fromNull = factory.GetFormatter(null!);
        var fromEmpty = factory.GetFormatter(string.Empty);

        Assert.Same(jsonFormatter, fromNull);
        Assert.Same(jsonFormatter, fromEmpty);
    }

    [Fact]
    public void GetFormatter_PartialMatch_IgnoresParameters()
    {
        var factory = CreateFactory();

        var jsonFormatter = factory.GetFormatter("application/json;charset=utf-8");
        Assert.Same(jsonFormatter, factory.GetFormatter("application/json"));
    }

    [Fact]
    public void GetFormatter_UnknownContentType_ThrowsKeyNotFoundException()
    {
        var factory = CreateFactory();

        var unknown = "application/unknown";
        var ex = Assert.Throws<KeyNotFoundException>(() => factory.GetFormatter(unknown));
        Assert.Contains(unknown, ex.Message);
    }

    [Fact]
    public void IsSupported_ReturnsTrueForKnownAndNull_AndFalseForUnknown()
    {
        var factory = CreateFactory();

        // Null / empty are considered supported (defaults to JSON)
        Assert.True(factory.IsSupported(null));
        Assert.True(factory.IsSupported(string.Empty));

        // Known default formatter
        Assert.True(factory.IsSupported("application/json"));
        Assert.True(factory.IsSupported("text/csv"));

        // Unknown formatter
        Assert.False(factory.IsSupported("application/does-not-exist"));
    }

    [Fact]
    public void IsSupported_IsCaseInsensitive()
    {
        var factory = CreateFactory();

        Assert.True(factory.IsSupported("APPLICATION/JSON"));
        Assert.True(factory.IsSupported("Text/CSV"));
    }
}
