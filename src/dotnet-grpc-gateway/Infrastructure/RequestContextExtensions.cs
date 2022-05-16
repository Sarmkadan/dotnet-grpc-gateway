using System;
using System.Collections.Generic;

namespace DotNetGrpcGateway.Infrastructure;

/// <summary>
/// Provides extension methods for <see cref="RequestContext"/> to simplify common operations
/// such as checking for user identity, formatting client information, and managing timing metadata.
/// </summary>
public static class RequestContextExtensions
{
    /// <summary>
    /// Determines whether the request context contains a non-empty user identifier.
    /// </summary>
    /// <param name="requestContext">The request context to check. Cannot be null.</param>
    /// <returns>True if the context has a non-empty user ID; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="requestContext"/> is null.</exception>
    /// <remarks>This method checks if the user ID is not null and not empty.</remarks>
    public static bool HasUserId(this RequestContext requestContext)
    {
        ArgumentNullException.ThrowIfNull(requestContext);
        return !string.IsNullOrEmpty(requestContext.UserId);
    }

    /// <summary>
    /// Formats client information into a human-readable string including client IP and user identifier.
    /// </summary>
    /// <param name="requestContext">The request context containing client information. Cannot be null.</param>
    /// <returns>A formatted string with client IP and user ID information.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="requestContext"/> is null.</exception>
    /// <remarks>This method returns a string in the format "Client IP: {clientIp}, User ID: {userId}."</remarks>
    public static string GetClientInfo(this RequestContext requestContext)
    {
        ArgumentNullException.ThrowIfNull(requestContext);
        return $"Client IP: {requestContext.ClientIp}, User ID: {requestContext.UserId ?? "Unknown"}";
    }

    /// <summary>
    /// Sets the start time of the request in the context properties.
    /// </summary>
    /// <param name="requestContext">The request context. Cannot be null.</param>
    /// <param name="startTime">The start time to record.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="requestContext"/> is null.</exception>
    /// <remarks>This method sets the start time in the context properties under the key "StartTime".</remarks>
    public static void SetStartTime(this RequestContext requestContext, DateTime startTime)
    {
        ArgumentNullException.ThrowIfNull(requestContext);
        requestContext.Properties["StartTime"] = startTime;
    }

    /// <summary>
    /// Gets the start time of the request from the context properties.
    /// </summary>
    /// <param name="requestContext">The request context. Cannot be null.</param>
    /// <returns>The start time if found; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="requestContext"/> is null.</exception>
    /// <remarks>This method retrieves the start time from the context properties under the key "StartTime".</remarks>
    public static DateTime? GetStartTime(this RequestContext requestContext)
    {
        ArgumentNullException.ThrowIfNull(requestContext);

        if (requestContext.Properties.TryGetValue("StartTime", out var startTime) && startTime is DateTime dateTime)
        {
            return dateTime;
        }

        return null;
    }
}
