// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Xml.Serialization;
using System.Text;

namespace DotNetGrpcGateway.Formatters;

/// <summary>
/// XML output formatter. Converts objects to XML strings using XmlSerializer.
/// </summary>
public class XmlFormatter : IOutputFormatter
{
    public string ContentType => "application/xml";

    public string Format<T>(T? data, bool pretty = false)
    {
        if (data == null)
            return "<root></root>";

        try
        {
            var serializer = new XmlSerializer(typeof(T));
            using var stream = new StringWriter();
            serializer.Serialize(stream, data);

            var xml = stream.ToString();

            // Optionally format for readability
            if (pretty)
            {
                return PrettyPrintXml(xml);
            }

            return xml;
        }
        catch (Exception ex)
        {
            return $"<error>{System.Net.WebUtility.HtmlEncode(ex.Message)}</error>";
        }
    }

    public async Task<string> FormatAsync<T>(T? data, bool pretty = false)
    {
        return await Task.FromResult(Format(data, pretty));
    }

    private string PrettyPrintXml(string xml)
    {
        try
        {
            var doc = new System.Xml.XmlDocument();
            doc.LoadXml(xml);

            var sb = new StringBuilder();
            var settings = new System.Xml.XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\n",
                Encoding = new UTF8Encoding(false)
            };

            using var writer = System.Xml.XmlWriter.Create(sb, settings);
            doc.WriteTo(writer);

            return sb.ToString();
        }
        catch
        {
            return xml;
        }
    }
}
