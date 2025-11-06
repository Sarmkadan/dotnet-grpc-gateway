// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Formatters;

/// <summary>
/// Interface for output formatters that convert objects to various text formats.
/// Supports both sync and async formatting operations.
/// </summary>
public interface IOutputFormatter
{
    /// <summary>
    /// Gets the MIME type for this formatter.
    /// </summary>
    string ContentType { get; }

    /// <summary>
    /// Formats an object to a string representation.
    /// </summary>
    string Format<T>(T? data, bool pretty = false);

    /// <summary>
    /// Asynchronously formats an object to a string representation.
    /// </summary>
    Task<string> FormatAsync<T>(T? data, bool pretty = false);
}
