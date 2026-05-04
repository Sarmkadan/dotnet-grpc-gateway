// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections;
using System.Reflection;
using System.Text;

namespace DotNetGrpcGateway.Formatters;

/// <summary>
/// CSV output formatter. Converts collections of objects to CSV format.
/// Works with IEnumerable types, extracting public properties as columns.
/// </summary>
public class CsvFormatter : IOutputFormatter
{
    public string ContentType => "text/csv";

    public string Format<T>(T? data, bool pretty = false)
    {
        if (data == null)
            return string.Empty;

        // Handle IEnumerable types (collections)
        if (data is IEnumerable enumerable && !(data is string))
        {
            return FormatCollection(enumerable.Cast<object>());
        }

        // Handle single objects
        return FormatSingleObject(data);
    }

    public async Task<string> FormatAsync<T>(T? data, bool pretty = false)
    {
        return await Task.FromResult(Format(data, pretty));
    }

    private string FormatCollection(IEnumerable<object> items)
    {
        var list = items.ToList();
        if (list.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        var firstItem = list[0];
        var properties = GetProperties(firstItem.GetType());

        // Write header
        sb.AppendLine(string.Join(",", properties.Select(p => EscapeCsvField(p.Name))));

        // Write rows
        foreach (var item in list)
        {
            var values = properties.Select(p => EscapeCsvField(GetPropertyValue(item, p)?.ToString() ?? ""));
            sb.AppendLine(string.Join(",", values));
        }

        return sb.ToString();
    }

    private string FormatSingleObject(object obj)
    {
        var properties = GetProperties(obj.GetType());
        var sb = new StringBuilder();

        // Write property names and values
        foreach (var prop in properties)
        {
            var value = GetPropertyValue(obj, prop);
            sb.AppendLine($"{prop.Name},{EscapeCsvField(value?.ToString() ?? "")}");
        }

        return sb.ToString();
    }

    private PropertyInfo[] GetProperties(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.IgnoreCase)
            .Where(p => p.CanRead)
            .ToArray();
    }

    private object? GetPropertyValue(object obj, PropertyInfo property)
    {
        try
        {
            return property.GetValue(obj);
        }
        catch
        {
            return null;
        }
    }

    private string EscapeCsvField(string? field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;

        // Escape quotes and wrap field if it contains special characters
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }
}
