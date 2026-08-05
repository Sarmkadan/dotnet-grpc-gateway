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

        _logger.LogInformation("OutputFormatterFactory initialized with {Count} default formatters", _formatterFactories.Count);
    }

    /// <summary>
    /// Registers a formatter for a specific content type.
    /// </summary>
    public void RegisterFormatter(IOutputFormatter formatter)
    {
        if (formatter is null)
            throw new ArgumentNullException(nameof(formatter));

        _logger.LogInformation("Registering formatter for content type: {ContentType}", formatter.ContentType);

        // Store a factory that returns the formatter instance.
        _formatterFactories[formatter.ContentType] = () => formatter;

        _logger.LogInformation("Formatter registered for content type: {ContentType}", formatter.ContentType);
    }

    /// <summary>
    /// Gets a formatter by content type. Returns JSON formatter as default if not found.
    /// </summary>
    public IOutputFormatter GetFormatter(string contentType)
    {
        _logger.LogInformation("Getting formatter for content type: {ContentType}", contentType);

        if (string.IsNullOrEmpty(contentType))
        {
            _logger.LogInformation("Content type is null or empty, returning default JSON formatter");
            return _formatterFactories["application/json"]();
        }

        // Try exact match first
        if (_formatterFactories.TryGetValue(contentType, out var factory))
        {
            _logger.LogInformation("Found exact match for content type: {ContentType}", contentType);
            return factory();
        }

        // Try partial match (e.g., "application/json;charset=utf-8" -> "application/json")
        var baseType = contentType.Split(';')[0].Trim();
        if (!baseType.Equals(contentType, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Exact match not found, trying base type: {BaseType}", baseType);
        }

        if (_formatterFactories.TryGetValue(baseType, out var baseFactory))
        {
            _logger.LogInformation("Found formatter for base type: {BaseType}", baseType);
            return baseFactory();
        }

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
        _logger.LogInformation("Getting all available content types");
        var keys = _formatterFactories.Keys;
        _logger.LogInformation("Found {Count} available content types", keys.Count);
        return keys;
    }

    /// <summary>
    /// Checks if a content type is supported.
    /// </summary>
    public bool IsSupported(string contentType)
    {
        _logger.LogInformation("Checking if content type '{ContentType}' is supported", contentType);

        if (string.IsNullOrEmpty(contentType))
        {
            _logger.LogInformation("Content type is null or empty, considered supported");
            return true;
        }

        bool isSupported = _formatterFactories.ContainsKey(contentType) ||
                           _formatterFactories.ContainsKey(contentType.Split(';')[0].Trim());

        _logger.LogInformation("Content type '{ContentType}' is supported: {IsSupported}", contentType, isSupported);
        return isSupported;
    }
}