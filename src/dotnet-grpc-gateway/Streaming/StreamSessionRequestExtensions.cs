#nullable enable

using System;
using System.Collections.Generic;

namespace DotNetGrpcGateway.Streaming;

/// <summary>
/// Extension methods for <see cref="StreamSessionRequest"/> to enhance request processing and analysis.
/// </summary>
public static class StreamSessionRequestExtensions
{
    /// <summary>
    /// Determines whether the request contains a valid service and method name.
    /// </summary>
    /// <param name="request">The request to check.</param>
    /// <returns><see langword="true"/> if the request has a valid service and method name; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    public static bool HasValidServiceAndMethod(this StreamSessionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return !string.IsNullOrEmpty(request.ServiceName) && !string.IsNullOrEmpty(request.MethodName);
    }

    /// <summary>
    /// Gets a read-only dictionary of headers from the request, excluding any null or empty keys.
    /// </summary>
    /// <param name="request">The request to retrieve headers from.</param>
    /// <returns>A read-only dictionary of valid headers.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    public static IReadOnlyDictionary<string, string> GetValidHeaders(this StreamSessionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Headers);

        var validHeaders = new Dictionary<string, string>(request.Headers.Count);
        foreach (var header in request.Headers)
        {
            if (!string.IsNullOrEmpty(header.Key))
            {
                validHeaders[header.Key] = header.Value;
            }
        }
        return validHeaders;
    }

    /// <summary>
    /// Creates a human-readable summary of the request for logging or auditing purposes.
    /// </summary>
    /// <param name="request">The request to summarize.</param>
    /// <returns>A summary string containing service, method, route path, and header count.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    public static string ToRequestSummary(this StreamSessionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return $"Service: {request.ServiceName}, Method: {request.MethodName}, Route: {request.RoutePath ?? "default"}, Headers: {request.Headers.Count}";
    }
}
