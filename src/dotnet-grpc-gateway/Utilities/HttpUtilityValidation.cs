#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace DotNetGrpcGateway.Utilities;

/// <summary>
/// Provides validation helpers for HTTP utility operations to ensure input parameters are valid.
/// </summary>
public static class HttpUtilityValidation
{
    /// <summary>
    /// Validates that a token is not null or empty for authorization header construction.
    /// </summary>
    /// <param name="token">The bearer token to validate.</param>
    /// <returns>An enumerable of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="token"/> is null.</exception>
    public static IReadOnlyList<string> Validate(string? token)
    {
        var problems = new List<string>();

        if (string.IsNullOrEmpty(token))
        {
            problems.Add("Token cannot be null or empty for authorization header construction.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates that an authorization header is properly formatted for token extraction.
    /// </summary>
    /// <param name="authorizationHeader">The authorization header to validate.</param>
    /// <returns>An enumerable of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateAuthorizationHeader(string? authorizationHeader)
    {
        var problems = new List<string>();

        if (string.IsNullOrEmpty(authorizationHeader))
        {
            problems.Add("Authorization header cannot be null or empty for token extraction.");
        }
        else if (!authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            problems.Add("Authorization header must use Bearer scheme for token extraction.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates that an accept header is properly formatted.
    /// </summary>
    /// <param name="acceptHeader">The Accept header to validate.</param>
    /// <returns>An enumerable of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateAcceptHeader(string? acceptHeader)
    {
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(acceptHeader))
        {
            problems.Add("Accept header cannot be null, empty, or whitespace.");
        }
        else if (acceptHeader.Length > 1024)
        {
            problems.Add("Accept header exceeds maximum length of 1024 characters.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates that a status code is within the valid HTTP range.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to validate.</param>
    /// <returns>An enumerable of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateStatusCode(int statusCode)
    {
        var problems = new List<string>();

        if (statusCode < 100 || statusCode >= 600)
        {
            problems.Add($"Status code must be in range 100-599, but was {statusCode}.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates that a content type string is not null or empty.
    /// </summary>
    /// <param name="contentType">The content type to validate.</param>
    /// <returns>An enumerable of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateContentType(string? contentType)
    {
        var problems = new List<string>();

        if (string.IsNullOrEmpty(contentType))
        {
            problems.Add("Content type cannot be null or empty.");
        }
        else if (contentType.Length > 256)
        {
            problems.Add("Content type exceeds maximum length of 256 characters.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the token is valid for authorization header construction.
    /// </summary>
    /// <param name="token">The bearer token to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(string? token) => Validate(token).Count == 0;

    /// <summary>
    /// Determines whether the authorization header is valid for token extraction.
    /// </summary>
    /// <param name="authorizationHeader">The authorization header to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidAuthorizationHeader(string? authorizationHeader) => ValidateAuthorizationHeader(authorizationHeader).Count == 0;

    /// <summary>
    /// Determines whether the accept header is valid.
    /// </summary>
    /// <param name="acceptHeader">The Accept header to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidAcceptHeader(string? acceptHeader) => ValidateAcceptHeader(acceptHeader).Count == 0;

    /// <summary>
    /// Determines whether the status code is valid.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidStatusCode(int statusCode) => ValidateStatusCode(statusCode).Count == 0;

    /// <summary>
    /// Determines whether the content type is valid.
    /// </summary>
    /// <param name="contentType">The content type to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidContentType(string? contentType) => ValidateContentType(contentType).Count == 0;
}