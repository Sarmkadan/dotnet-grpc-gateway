using System.Diagnostics.CodeAnalysis;

namespace DotNetGrpcGateway.Formatters;

/// <summary>
/// Extension methods for <see cref="OutputFormatterFactory"/> that provide additional convenience functionality.
/// </summary>
public static class OutputFormatterFactoryExtensions
{
    /// <summary>
    /// Gets a formatter by content type, or returns null if not found instead of defaulting to JSON.
    /// </summary>
    /// <param name="factory">The formatter factory instance.</param>
    /// <param name="contentType">The content type to find a formatter for.</param>
    /// <returns>The formatter if found, otherwise null.</returns>
    public static IOutputFormatter? GetFormatterOrDefault(this OutputFormatterFactory factory, string? contentType)
    {
        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        if (string.IsNullOrEmpty(contentType))
        {
            return null;
        }

        // Try exact match first
        if (factory.GetAvailableContentTypes().Contains(contentType, StringComparer.OrdinalIgnoreCase))
        {
            return factory.GetFormatter(contentType);
        }

        // Try partial match (e.g., "application/json;charset=utf-8" -> "application/json")
        var baseType = contentType.Split(';')[0].Trim();
        if (factory.GetAvailableContentTypes().Contains(baseType, StringComparer.OrdinalIgnoreCase))
        {
            return factory.GetFormatter(baseType);
        }

        return null;
    }

    /// <summary>
    /// Attempts to get a formatter by content type.
    /// </summary>
    /// <param name="factory">The formatter factory instance.</param>
    /// <param name="contentType">The content type to find a formatter for.</param>
    /// <param name="formatter">When this method returns, contains the formatter if found, otherwise null.</param>
    /// <returns>True if the formatter was found, otherwise false.</returns>
    public static bool TryGetFormatter(this OutputFormatterFactory factory, string? contentType, [NotNullWhen(true)] out IOutputFormatter? formatter)
    {
        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        formatter = factory.GetFormatterOrDefault(contentType);
        return formatter != null;
    }

    /// <summary>
    /// Formats the specified data using the formatter for the given content type.
    /// </summary>
    /// <typeparam name="T">The type of data to format.</typeparam>
    /// <param name="factory">The formatter factory instance.</param>
    /// <param name="data">The data to format.</param>
    /// <param name="contentType">The content type to determine which formatter to use.</param>
    /// <param name="pretty">Whether to format with pretty printing (if supported by the formatter).</param>
    /// <returns>The formatted string representation of the data.</returns>
    public static string Format<T>(this OutputFormatterFactory factory, T? data, string? contentType, bool pretty = false)
    {
        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        var formatter = factory.GetFormatter(contentType ?? "application/json");
        return formatter.Format(data, pretty);
    }

    /// <summary>
    /// Asynchronously formats the specified data using the formatter for the given content type.
    /// </summary>
    /// <typeparam name="T">The type of data to format.</typeparam>
    /// <param name="factory">The formatter factory instance.</param>
    /// <param name="data">The data to format.</param>
    /// <param name="contentType">The content type to determine which formatter to use.</param>
    /// <param name="pretty">Whether to format with pretty printing (if supported by the formatter).</param>
    /// <returns>A task that represents the asynchronous formatting operation. The task result contains the formatted string.</returns>
    public static async Task<string> FormatAsync<T>(this OutputFormatterFactory factory, T? data, string? contentType, bool pretty = false)
    {
        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        var formatter = factory.GetFormatter(contentType ?? "application/json");
        return await formatter.FormatAsync(data, pretty).ConfigureAwait(false);
    }
}
