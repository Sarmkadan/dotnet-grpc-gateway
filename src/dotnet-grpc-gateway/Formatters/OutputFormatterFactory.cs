#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace DotNetGrpcGateway.Formatters;

/// <summary>
/// Factory for creating output formatters based on content type.
/// Manages available formatters and provides appropriate formatter for requested format.
/// </summary>
public class OutputFormatterFactory
{
    // Stores factory functions that return formatter instances.
    private readonly Dictionary<string, Func<IOutputFormatter>> _formatterFactories = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger<OutputFormatterFactory> _logger;

    public OutputFormatterFactory(ILogger<OutputFormatterFactory> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Register default formatters
        RegisterFormatter(new JsonFormatter());
        RegisterFormatter(new CsvFormatter());
        RegisterFormatter(new XmlFormatter());
    }

    /// <summary>
    /// Registers a formatter for a specific content type.
    /// </summary>
    public void RegisterFormatter(IOutputFormatter formatter)
    {
        if (formatter is null)
            throw new ArgumentNullException(nameof(formatter));

        // Store a factory that returns the formatter instance.
        _formatterFactories[formatter.ContentType] = () => formatter;
        _logger.LogInformation("Registered formatter for content type: {ContentType}", formatter.ContentType);
    }

    /// <summary>
    /// Gets a formatter by content type. Returns JSON formatter as default if not found.
    /// </summary>
    public IOutputFormatter GetFormatter(string contentType)
    {
        if (string.IsNullOrEmpty(contentType))
            return _formatterFactories["application/json"]();

        // Try exact match first
        if (_formatterFactories.TryGetValue(contentType, out var factory))
            return factory();

        // Try partial match (e.g., "application/json;charset=utf-8" -> "application/json")
        var baseType = contentType.Split(';')[0].Trim();
        if (_formatterFactories.TryGetValue(baseType, out var baseFactory))
            return baseFactory();

        // No formatter found – throw an exception listing supported formats.
        var supported = string.Join(", ", _formatterFactories.Keys);
        _logger.LogWarning("No formatter found for content type '{ContentType}', supported formats: {Supported}", contentType, supported);
        throw new KeyNotFoundException($"No formatter found for content type '{contentType}'. Supported formats: {supported}");
    }

    /// <summary>
    /// Gets all registered content types.
    /// </summary>
    public IEnumerable<string> GetAvailableContentTypes()
    {
        return _formatterFactories.Keys;
    }

    /// <summary>
    /// Checks if a content type is supported.
    /// </summary>
    public bool IsSupported(string contentType)
    {
        if (string.IsNullOrEmpty(contentType))
            return true;

        return _formatterFactories.ContainsKey(contentType) ||
               _formatterFactories.ContainsKey(contentType.Split(';')[0].Trim());
    }
}
