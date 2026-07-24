#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Diagnostics.CodeAnalysis;

namespace DotNetGrpcGateway.Infrastructure;

/// <summary>
/// Provides database connection strings to data access layer with built-in security protections
/// to prevent secrets from leaking in logs, exceptions, or diagnostic output.
/// </summary>
public interface IConnectionStringProvider
{
    /// <summary>
    /// Gets the connection string. The returned value should be treated as sensitive and
    /// never logged or included in exception messages.
    /// </summary>
    /// <returns>The connection string.</returns>
    string GetConnectionString();

    /// <summary>
    /// Gets a redacted version of the connection string suitable for logging or diagnostics.
    /// Password and other sensitive parameters are replaced with [REDACTED].
    /// </summary>
    /// <returns>A redacted connection string.</returns>
    string GetRedactedConnectionString();

    /// <summary>
    /// Gets the connection string provider type identifier for diagnostics.
    /// </summary>
    /// <returns>A string identifying the provider type.</returns>
    string GetProviderType();
}

/// <summary>
/// Base interface for connection string providers that support secret store integration.
/// </summary>
public interface ISecretStoreConnectionStringProvider : IConnectionStringProvider
{
    /// <summary>
    /// Gets the secret store identifier (e.g., Azure Key Vault URI, AWS Secrets Manager ARN).
    /// </summary>
    string SecretStoreIdentifier { get; }
}

/// <summary>
/// Default implementation of <see cref="IConnectionStringProvider"/> that sources
/// connection strings from configuration and provides security protections.
/// </summary>
public sealed class ConnectionStringProvider : IConnectionStringProvider, ISecretStoreConnectionStringProvider
{
    private readonly string _connectionString;
    private readonly string? _secretStoreIdentifier;
    private readonly string? _providerType;
    private readonly Lazy<string> _redactedConnectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionStringProvider"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string (must not be null or empty).</param>
    /// <param name="secretStoreIdentifier">Optional identifier for the secret store (e.g., Azure Key Vault URI).</param>
    /// <param name="providerType">Optional provider type identifier for diagnostics.</param>
    /// <exception cref="ArgumentException"><paramref name="connectionString"/> is null or empty.</exception>
    public ConnectionStringProvider(
        string connectionString,
        string? secretStoreIdentifier = null,
        string? providerType = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(connectionString);

        _connectionString = connectionString;
        _secretStoreIdentifier = secretStoreIdentifier;
        _providerType = providerType ?? nameof(ConnectionStringProvider);

        // Pre-compute the redacted version to avoid repeated allocations
        _redactedConnectionString = new Lazy<string>(CreateRedactedConnectionString);
    }

    /// <summary>
    /// Gets the connection string. This value contains sensitive information and should
    /// never be logged or included in exception messages.
    /// </summary>
    public string GetConnectionString() => _connectionString;

    /// <summary>
    /// Gets a redacted version of the connection string suitable for logging or diagnostics.
    /// Password and other sensitive parameters are replaced with [REDACTED].
    /// </summary>
    public string GetRedactedConnectionString() => _redactedConnectionString.Value;

    /// <summary>
    /// Gets the connection string provider type identifier for diagnostics.
    /// </summary>
    public string GetProviderType() => _providerType ?? nameof(ConnectionStringProvider);

    /// <summary>
    /// Gets the secret store identifier if this provider is backed by a secret store.
    /// </summary>
    public string? SecretStoreIdentifier => _secretStoreIdentifier;

    /// <summary>
    /// Creates a redacted version of the connection string where sensitive parameters
    /// (password, pwd, secret, token, key, accesskey, etc.) are replaced with
    /// [REDACTED] to prevent accidental logging of credentials.
    /// </summary>
    [SuppressMessage("Security", "CA2016:ForwardingProblematicArguments", Justification = "Redaction only")]
    private string CreateRedactedConnectionString()
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            return string.Empty;
        }

        // Split by semicolon to handle individual parameters
        var parts = _connectionString.Split(';');
        var redactedParts = new List<string>();

        foreach (var part in parts)
        {
            if (string.IsNullOrEmpty(part))
            {
                continue;
            }

            var keyValue = part.Split('=', 2);
            if (keyValue.Length == 2)
            {
                var key = keyValue[0].Trim();
                var value = keyValue[1].Trim();

                // Check if this is a sensitive parameter
                if (IsSensitiveParameter(key))
                {
                    redactedParts.Add($"{key}=[REDACTED]");
                }
                else
                {
                    redactedParts.Add(part);
                }
            }
            else
            {
                // Not a key=value pair, keep as-is
                redactedParts.Add(part);
            }
        }

        return string.Join(";", redactedParts);
    }

    /// <summary>
    /// Determines whether a parameter name indicates sensitive data that should be redacted.
    /// </summary>
    /// <param name="parameterName">The parameter name to check.</param>
    /// <returns>True if the parameter should be redacted; otherwise false.</returns>
    private static bool IsSensitiveParameter(string parameterName)
    {
        if (string.IsNullOrEmpty(parameterName))
        {
            return false;
        }

        var normalized = parameterName.Trim().ToLowerInvariant();

        // Common patterns for sensitive parameters
        return normalized switch
        {
            "password" or
            "pwd" or
            "passwd" or
            "user" or
            "username" or
            "uid" or
            "secret" or
            "token" or
            "key" or
            "accesskey" or
            "apikey" or
            "authentication" or
            "auth" or
            "credential" or
            "credentials" or
            "connectionstring" or
            "pwd" => true,
            _ => normalized.Contains("password") ||
                 normalized.Contains("secret") ||
                 normalized.Contains("token") ||
                 normalized.Contains("key") ||
                 normalized.Contains("credential")
        };
    }

    /// <summary>
    /// Returns a string representation of the provider for diagnostics.
    /// Never includes the actual connection string.
    /// </summary>
    public override string ToString() =>
        _secretStoreIdentifier is not null
            ? $"ConnectionStringProvider {{ ProviderType={GetProviderType()}, SecretStore={_secretStoreIdentifier} }}"
            : $"ConnectionStringProvider {{ ProviderType={GetProviderType()} }}";

    /// <summary>
    /// Returns a debug representation that includes the redacted connection string.
    /// </summary>
    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = "Debug only")]
    internal string DebugToString() =>
        _secretStoreIdentifier is not null
            ? $"ConnectionStringProvider {{ ProviderType={GetProviderType()}, SecretStore={_secretStoreIdentifier}, ConnectionString=[REDACTED] }}"
            : $"ConnectionStringProvider {{ ProviderType={GetProviderType()}, ConnectionString=[REDACTED] }}";
}
