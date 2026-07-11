#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Services;

namespace DotNetGrpcGateway.Controllers;

/// <summary>
/// Provides validation helpers for <see cref="LoadBalancerController"/> parameters.
/// </summary>
public static class LoadBalancerControllerValidation
{
    private const int MinPort = 1;
    private const int MaxPort = 65535;
    private const int MinWeight = 1;
    private const int MaxWeight = 100;
    private const int MinServiceId = 1;
    private const int MinEndpointId = 1;

    /// <summary>
    /// Validates a <see cref="ServiceEndpoint"/> instance.
    /// </summary>
    /// <param name="value">The endpoint to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of human-readable validation errors.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ServiceEndpoint value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(value.Host))
            errors.Add("Host is required and cannot be empty or whitespace.");
        else if (value.Host.Length > 253)
            errors.Add("Host cannot exceed 253 characters.");

        if (value.Port < MinPort || value.Port > MaxPort)
            errors.Add($"Port must be between {MinPort} and {MaxPort}.");

        if (value.Weight < MinWeight || value.Weight > MaxWeight)
            errors.Add($"Weight must be between {MinWeight} and {MaxWeight}.");

        if (value.RegisteredAt == default)
            errors.Add("RegisteredAt must be a valid date.");

        if (value.LastUsedAt == default)
            errors.Add("LastUsedAt must be a valid date.");

        if (value.RegisteredAt > DateTime.UtcNow.AddMinutes(1))
            errors.Add("RegisteredAt cannot be in the future.");

        if (value.LastUsedAt > DateTime.UtcNow.AddMinutes(1))
            errors.Add("LastUsedAt cannot be in the future.");

        if (value.RegisteredAt > value.LastUsedAt)
            errors.Add("LastUsedAt cannot be earlier than RegisteredAt.");

        return errors;
    }

    /// <summary>
    /// Validates a service ID parameter.
    /// </summary>
    /// <param name="serviceId">The service identifier.</param>
    /// <returns>An empty list if valid; otherwise, a list of human-readable validation errors.</returns>
    public static IReadOnlyList<string> ValidateServiceId(int serviceId)
    {
        var errors = new List<string>();

        if (serviceId < MinServiceId)
            errors.Add($"Service ID must be at least {MinServiceId}.");

        return errors;
    }

    /// <summary>
    /// Validates an endpoint ID parameter.
    /// </summary>
    /// <param name="endpointId">The endpoint identifier.</param>
    /// <returns>An empty list if valid; otherwise, a list of human-readable validation errors.</returns>
    public static IReadOnlyList<string> ValidateEndpointId(int endpointId)
    {
        var errors = new List<string>();

        if (endpointId < MinEndpointId)
            errors.Add($"Endpoint ID must be at least {MinEndpointId}.");

        return errors;
    }

    /// <summary>
    /// Validates a load balancing strategy name.
    /// </summary>
    /// <param name="strategyName">The strategy name to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of human-readable validation errors.</returns>
    public static IReadOnlyList<string> Validate(string strategyName)
    {
        ArgumentException.ThrowIfNullOrEmpty(strategyName);

        var errors = new List<string>();

        if (!Enum.TryParse<LoadBalancingStrategy>(strategyName, true, out _))
        {
            var validValues = string.Join(", ", Enum.GetNames(typeof(LoadBalancingStrategy)));
            errors.Add($"Unknown strategy '{strategyName}'. Valid values: {validValues}.");
        }

        return errors;
    }

    /// <summary>
    /// Determines whether the specified <see cref="ServiceEndpoint"/> is valid.
    /// </summary>
    /// <param name="value">The endpoint to check.</param>
    /// <returns><see langword="true"/> if the endpoint is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ServiceEndpoint value) => Validate(value).Count == 0;

    /// <summary>
    /// Determines whether the specified service ID is valid.
    /// </summary>
    /// <param name="serviceId">The service identifier to check.</param>
    /// <returns><see langword="true"/> if the service ID is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidServiceId(int serviceId) => ValidateServiceId(serviceId).Count == 0;

    /// <summary>
    /// Determines whether the specified endpoint ID is valid.
    /// </summary>
    /// <param name="endpointId">The endpoint identifier to check.</param>
    /// <returns><see langword="true"/> if the endpoint ID is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidEndpointId(int endpointId) => ValidateEndpointId(endpointId).Count == 0;

    /// <summary>
    /// Determines whether the specified strategy name is valid.
    /// </summary>
    /// <param name="strategyName">The strategy name to check.</param>
    /// <returns><see langword="true"/> if the strategy name is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(string strategyName) => Validate(strategyName).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="ServiceEndpoint"/> is valid, throwing an <see cref="ArgumentException"/>
    /// if it is not.
    /// </summary>
    /// <param name="value">The endpoint to validate.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this ServiceEndpoint value)
    {
        var errors = Validate(value);
        if (errors.Count == 0)
            return;

        throw new ArgumentException(
            $"ServiceEndpoint is invalid. Details: {string.Join(" ", errors)}");
    }

    /// <summary>
    /// Ensures that the specified service ID is valid, throwing an <see cref="ArgumentException"/>
    /// if it is not.
    /// </summary>
    /// <param name="serviceId">The service identifier to validate.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="serviceId"/> is not valid.</exception>
    public static void EnsureValidServiceId(int serviceId)
    {
        var errors = ValidateServiceId(serviceId);
        if (errors.Count == 0)
            return;

        throw new ArgumentException(
            $"Service ID {serviceId} is invalid. Details: {string.Join(" ", errors)}");
    }

    /// <summary>
    /// Ensures that the specified endpoint ID is valid, throwing an <see cref="ArgumentException"/>
    /// if it is not.
    /// </summary>
    /// <param name="endpointId">The endpoint identifier to validate.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="endpointId"/> is not valid.</exception>
    public static void EnsureValidEndpointId(int endpointId)
    {
        var errors = ValidateEndpointId(endpointId);
        if (errors.Count == 0)
            return;

        throw new ArgumentException(
            $"Endpoint ID {endpointId} is invalid. Details: {string.Join(" ", errors)}");
    }

    /// <summary>
    /// Ensures that the specified strategy name is valid, throwing an <see cref="ArgumentException"/>
    /// if it is not.
    /// </summary>
    /// <param name="strategyName">The strategy name to validate.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="strategyName"/> is not valid.</exception>
    public static void EnsureValid(string strategyName)
    {
        var errors = Validate(strategyName);
        if (errors.Count == 0)
            return;

        throw new ArgumentException(
            $"Strategy name '{strategyName}' is invalid. Details: {string.Join(" ", errors)}");
    }
}