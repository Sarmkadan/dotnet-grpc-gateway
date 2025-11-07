// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net.Http.Headers;

namespace DotNetGrpcGateway.Utilities;

/// <summary>
/// HTTP-related utilities for request/response handling and header manipulation.
/// Simplifies common HTTP operations like header parsing and content negotiation.
/// </summary>
public static class HttpUtility
{
    /// <summary>
    /// Determines the appropriate content type based on Accept header and available formatters.
    /// </summary>
    public static string GetAcceptedContentType(string? acceptHeader, string defaultType = "application/json")
    {
        if (string.IsNullOrEmpty(acceptHeader))
            return defaultType;

        // Parse Accept header and return first supported type
        var types = acceptHeader.Split(',')
            .Select(t => t.Split(';')[0].Trim())
            .Where(t => !string.IsNullOrEmpty(t));

        var supportedTypes = new[] { "application/json", "application/xml", "text/csv" };

        return types.FirstOrDefault(t => supportedTypes.Contains(t, StringComparer.OrdinalIgnoreCase)) ?? defaultType;
    }

    /// <summary>
    /// Builds an authorization header value for Bearer token authentication.
    /// </summary>
    public static string BuildAuthorizationHeader(string token)
    {
        if (string.IsNullOrEmpty(token))
            throw new ArgumentException("Token cannot be null or empty", nameof(token));

        return $"Bearer {token}";
    }

    /// <summary>
    /// Extracts the Bearer token from an Authorization header value.
    /// </summary>
    public static string? ExtractBearerToken(string? authorizationHeader)
    {
        if (string.IsNullOrEmpty(authorizationHeader))
            return null;

        const string bearerScheme = "Bearer ";
        if (!authorizationHeader.StartsWith(bearerScheme, StringComparison.OrdinalIgnoreCase))
            return null;

        return authorizationHeader[bearerScheme.Length..].Trim();
    }

    /// <summary>
    /// Creates appropriate headers for gRPC-Web requests.
    /// </summary>
    public static HttpRequestHeaders AddGrpcWebHeaders(this HttpRequestHeaders headers)
    {
        headers.Add("x-grpc-web", "1");
        headers.Add("x-user-agent", "grpc-web-dotnet/1.0");
        return headers;
    }

    /// <summary>
    /// Checks if response indicates a successful status (2xx).
    /// </summary>
    public static bool IsSuccessStatusCode(int statusCode) => statusCode >= 200 && statusCode < 300;

    /// <summary>
    /// Checks if response indicates a client error (4xx).
    /// </summary>
    public static bool IsClientError(int statusCode) => statusCode >= 400 && statusCode < 500;

    /// <summary>
    /// Checks if response indicates a server error (5xx).
    /// </summary>
    public static bool IsServerError(int statusCode) => statusCode >= 500 && statusCode < 600;

    /// <summary>
    /// Gets the category name for an HTTP status code.
    /// </summary>
    public static string GetStatusCodeCategory(int statusCode)
    {
        return statusCode switch
        {
            >= 100 and < 200 => "Informational",
            >= 200 and < 300 => "Success",
            >= 300 and < 400 => "Redirection",
            >= 400 and < 500 => "Client Error",
            >= 500 and < 600 => "Server Error",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Checks if the content type indicates JSON.
    /// </summary>
    public static bool IsJsonContentType(string? contentType)
    {
        if (string.IsNullOrEmpty(contentType))
            return false;

        return contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase) ||
               contentType.Contains("application/ld+json", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the content type indicates XML.
    /// </summary>
    public static bool IsXmlContentType(string? contentType)
    {
        if (string.IsNullOrEmpty(contentType))
            return false;

        return contentType.Contains("application/xml", StringComparison.OrdinalIgnoreCase) ||
               contentType.Contains("text/xml", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the content type indicates form data.
    /// </summary>
    public static bool IsFormContentType(string? contentType)
    {
        if (string.IsNullOrEmpty(contentType))
            return false;

        return contentType.Contains("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) ||
               contentType.Contains("multipart/form-data", StringComparison.OrdinalIgnoreCase);
    }
}
