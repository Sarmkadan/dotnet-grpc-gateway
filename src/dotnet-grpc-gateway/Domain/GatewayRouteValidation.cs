#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace DotNetGrpcGateway.Domain;

/// <summary>
/// Provides validation helpers for <see cref="GatewayRoute"/> instances.
/// </summary>
public static class GatewayRouteValidation
{
    /// <summary>
    /// Validates a <see cref="GatewayRoute"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The route to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this GatewayRoute? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (value.Id <= 0)
        {
            errors.Add($"Id must be a positive integer, but was {value.Id}.");
        }

        // Validate Pattern
        if (string.IsNullOrWhiteSpace(value.Pattern))
        {
            errors.Add("Pattern is required and cannot be empty or whitespace.");
        }
        else if (value.Pattern.Length > 1024)
        {
            errors.Add("Pattern cannot exceed 1024 characters.");
        }

        // Validate TargetServiceId
        if (value.TargetServiceId <= 0)
        {
            errors.Add("TargetServiceId must be a positive integer.");
        }

        // Validate Priority
        if (value.Priority < 0 || value.Priority > 1000)
        {
            errors.Add("Priority must be between 0 and 1000 inclusive.");
        }

        // Validate MatchType
        // No validation needed - enum has valid default

        // Validate Description (optional, but if present, must meet criteria)
        if (!string.IsNullOrEmpty(value.Description) && value.Description.Length > 2048)
        {
            errors.Add("Description cannot exceed 2048 characters.");
        }

        // Validate Headers
        if (value.Headers is null)
        {
            errors.Add("Headers dictionary cannot be null.");
        }
        else
        {
            foreach (var header in value.Headers)
            {
                if (string.IsNullOrWhiteSpace(header.Key))
                {
                    errors.Add("Header key cannot be null or whitespace.");
                    break;
                }

                if (header.Key.Length > 256)
                {
                    errors.Add("Header key cannot exceed 256 characters.");
                    break;
                }

                if (header.Value is not null && header.Value.Length > 4096)
                {
                    errors.Add("Header value cannot exceed 4096 characters.");
                    break;
                }
            }
        }

        // Validate Metadata
        if (value.Metadata is null)
        {
            errors.Add("Metadata dictionary cannot be null.");
        }
        else
        {
            foreach (var metadata in value.Metadata)
            {
                if (string.IsNullOrWhiteSpace(metadata.Key))
                {
                    errors.Add("Metadata key cannot be null or whitespace.");
                    break;
                }

                if (metadata.Key.Length > 256)
                {
                    errors.Add("Metadata key cannot exceed 256 characters.");
                    break;
                }

                if (metadata.Value is not null && metadata.Value.Length > 4096)
                {
                    errors.Add("Metadata value cannot exceed 4096 characters.");
                    break;
                }
            }
        }

        // Validate AuthorizationPolicy (if present, must meet criteria)
        if (!string.IsNullOrEmpty(value.AuthorizationPolicy) && value.AuthorizationPolicy.Length > 256)
        {
            errors.Add("AuthorizationPolicy cannot exceed 256 characters.");
        }

        // Validate RateLimitPerMinute
        if (value.RateLimitPerMinute <= 0)
        {
            errors.Add("RateLimitPerMinute must be greater than 0.");
        }
        else if (value.RateLimitPerMinute > 1000000)
        {
            errors.Add("RateLimitPerMinute cannot exceed 1,000,000 requests per minute.");
        }

        // Validate CacheDurationSeconds
        if (value.CacheDurationSeconds < 0)
        {
            errors.Add("CacheDurationSeconds cannot be negative.");
        }
        else if (value.CacheDurationSeconds > 86400)
        {
            errors.Add("CacheDurationSeconds cannot exceed 86,400 seconds (24 hours).");
        }

        // Validate RequestTransformationScript (if present, must meet criteria)
        if (!string.IsNullOrEmpty(value.RequestTransformationScript) && value.RequestTransformationScript.Length > 8192)
        {
            errors.Add("RequestTransformationScript cannot exceed 8,192 characters.");
        }

        // Validate ResponseTransformationScript (if present, must meet criteria)
        if (!string.IsNullOrEmpty(value.ResponseTransformationScript) && value.ResponseTransformationScript.Length > 8192)
        {
            errors.Add("ResponseTransformationScript cannot exceed 8,192 characters.");
        }

        // Validate ChannelOptions if present
        if (value.ChannelOptions is not null)
        {
            if (value.ChannelOptions.CallTimeout.HasValue)
            {
                if (value.ChannelOptions.CallTimeout.Value <= TimeSpan.Zero)
                {
                    errors.Add("ChannelOptions.CallTimeout must be positive if specified.");
                }
                else if (value.ChannelOptions.CallTimeout.Value.TotalMilliseconds > 300000) // 5 minutes max
                {
                    errors.Add("ChannelOptions.CallTimeout cannot exceed 5 minutes (300,000 milliseconds).");
                }
            }

            if (value.ChannelOptions.MaxReceiveMessageSize.HasValue)
            {
                if (value.ChannelOptions.MaxReceiveMessageSize.Value <= 0)
                {
                    errors.Add("ChannelOptions.MaxReceiveMessageSize must be positive if specified.");
                }
                else if (value.ChannelOptions.MaxReceiveMessageSize.Value > 100 * 1024 * 1024) // 100MB max
                {
                    errors.Add("ChannelOptions.MaxReceiveMessageSize cannot exceed 100 MB (104,857,600 bytes).");
                }
            }

            if (value.ChannelOptions.MaxSendMessageSize.HasValue)
            {
                if (value.ChannelOptions.MaxSendMessageSize.Value <= 0)
                {
                    errors.Add("ChannelOptions.MaxSendMessageSize must be positive if specified.");
                }
                else if (value.ChannelOptions.MaxSendMessageSize.Value > 100 * 1024 * 1024) // 100MB max
                {
                    errors.Add("ChannelOptions.MaxSendMessageSize cannot exceed 100 MB (104,857,600 bytes).");
                }
            }

            // Validate AdditionalHeaders in ChannelOptions
            if (value.ChannelOptions.AdditionalHeaders is null)
            {
                errors.Add("ChannelOptions.AdditionalHeaders dictionary cannot be null.");
            }
            else
            {
                foreach (var header in value.ChannelOptions.AdditionalHeaders)
                {
                    if (string.IsNullOrWhiteSpace(header.Key))
                    {
                        errors.Add("ChannelOptions.AdditionalHeaders key cannot be null or whitespace.");
                        break;
                    }

                    if (header.Key.Length > 256)
                    {
                        errors.Add("ChannelOptions.AdditionalHeaders key cannot exceed 256 characters.");
                        break;
                    }

                    if (header.Value is not null && header.Value.Length > 4096)
                    {
                        errors.Add("ChannelOptions.AdditionalHeaders value cannot exceed 4096 characters.");
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(value.ChannelOptions.TlsTargetName) && value.ChannelOptions.TlsTargetName.Length > 256)
            {
                errors.Add("ChannelOptions.TlsTargetName cannot exceed 256 characters.");
            }
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt cannot be the default DateTime value.");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("CreatedAt cannot be in the future.");
        }

        // Validate ModifiedAt
        if (value.ModifiedAt == default)
        {
            errors.Add("ModifiedAt cannot be the default DateTime value.");
        }
        else if (value.ModifiedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("ModifiedAt cannot be in the future.");
        }
        else if (value.ModifiedAt < value.CreatedAt)
        {
            errors.Add("ModifiedAt cannot be earlier than CreatedAt.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="GatewayRoute"/> instance is valid.
    /// </summary>
    /// <param name="value">The route to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this GatewayRoute? value)
    {
        return value is not null && Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="GatewayRoute"/> instance is valid, throwing an <see cref="ArgumentException"/>
    /// with a detailed message listing all validation problems if it is not.
    /// </summary>
    /// <param name="value">The route to validate.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is invalid.
    /// The exception message contains a formatted list of all validation errors.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static void EnsureValid(this GatewayRoute? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"GatewayRoute validation failed with {errors.Count} error(s):{Environment.NewLine}-" +
            $"{string.Join($"{Environment.NewLine}-", errors)}");
    }
}