// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Formatters;

/// <summary>
/// Factory for creating output formatters based on content type.
/// Manages available formatters and provides appropriate formatter for requested format.
/// </summary>
public class OutputFormatterFactory
{
    private readonly Dictionary<string, IOutputFormatter> _formatters = new(StringComparer.OrdinalIgnoreCase);
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
        if (formatter == null)
            throw new ArgumentNullException(nameof(formatter));

        _formatters[formatter.ContentType] = formatter;
        _logger.LogInformation("Registered formatter for content type: {ContentType}", formatter.ContentType);
    }

    /// <summary>
    /// Gets a formatter by content type. Returns JSON formatter as default if not found.
    /// </summary>
    public IOutputFormatter GetFormatter(string contentType)
    {
        if (string.IsNullOrEmpty(contentType))
            return _formatters["application/json"];

        // Try exact match first
        if (_formatters.TryGetValue(contentType, out var formatter))
            return formatter;

        // Try partial match (e.g., "application/json;charset=utf-8" -> "application/json")
        var baseType = contentType.Split(';')[0].Trim();
        if (_formatters.TryGetValue(baseType, out var baseFormatter))
            return baseFormatter;

        _logger.LogWarning("No formatter found for content type '{ContentType}', using default JSON formatter", contentType);
        return _formatters["application/json"];
    }

    /// <summary>
    /// Gets all registered content types.
    /// </summary>
    public IEnumerable<string> GetAvailableContentTypes()
    {
        return _formatters.Keys;
    }

    /// <summary>
    /// Checks if a content type is supported.
    /// </summary>
    public bool IsSupported(string contentType)
    {
        if (string.IsNullOrEmpty(contentType))
            return true;

        return _formatters.ContainsKey(contentType) ||
               _formatters.ContainsKey(contentType.Split(';')[0].Trim());
    }
}
