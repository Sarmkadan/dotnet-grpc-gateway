using System;
using System.Net;
using Microsoft.Extensions.Options;

namespace DotNetGrpcGateway.Options;

/// <summary>
/// Extension methods for configuring <see cref="DotnetGrpcGatewayOptions"/> instances.
/// </summary>
public static class DotnetGrpcGatewayOptionsExtensions
{
    /// <summary>
    /// Configures the gateway to listen on localhost only.
    /// </summary>
    /// <param name="options">The gateway options to configure.</param>
    /// <returns>The configured <see cref="DotnetGrpcGatewayOptions"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static DotnetGrpcGatewayOptions UseLocalhost(this DotnetGrpcGatewayOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.ListenAddress = IPAddress.Loopback.ToString();
        return options;
    }

    /// <summary>
    /// Configures the gateway to listen on all network interfaces.
    /// </summary>
    /// <param name="options">The gateway options to configure.</param>
    /// <returns>The configured <see cref="DotnetGrpcGatewayOptions"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static DotnetGrpcGatewayOptions UseAllInterfaces(this DotnetGrpcGatewayOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.ListenAddress = "0.0.0.0";
        return options;
    }

    /// <summary>
    /// Configures the gateway to listen on a specific network interface.
    /// </summary>
    /// <param name="options">The gateway options to configure.</param>
    /// <param name="address">The IP address to listen on.</param>
    /// <returns>The configured <see cref="DotnetGrpcGatewayOptions"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="address"/> is <see langword="null"/>, empty, or whitespace.</exception>
    public static DotnetGrpcGatewayOptions UseAddress(this DotnetGrpcGatewayOptions options, string address)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(address);

        options.ListenAddress = address;
        return options;
    }

    /// <summary>
    /// Configures the gateway to use a specific port.
    /// </summary>
    /// <param name="options">The gateway options to configure.</param>
    /// <param name="port">The port number.</param>
    /// <returns>The configured <see cref="DotnetGrpcGatewayOptions"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is outside the valid range of 1-65535.</exception>
    public static DotnetGrpcGatewayOptions UsePort(this DotnetGrpcGatewayOptions options, int port)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (port is < 1 or > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(port), port, "Port must be between 1 and 65535");
        }

        options.Port = port;
        return options;
    }

    /// <summary>
    /// Disables gRPC reflection endpoint.
    /// </summary>
    /// <param name="options">The gateway options to configure.</param>
    /// <returns>The configured <see cref="DotnetGrpcGatewayOptions"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static DotnetGrpcGatewayOptions DisableReflection(this DotnetGrpcGatewayOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.EnableReflection = false;
        return options;
    }

    /// <summary>
    /// Disables metrics collection and reporting.
    /// </summary>
    /// <param name="options">The gateway options to configure.</param>
    /// <returns>The configured <see cref="DotnetGrpcGatewayOptions"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static DotnetGrpcGatewayOptions DisableMetrics(this DotnetGrpcGatewayOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.EnableMetrics = false;
        options.Metrics.EnableMetrics = false;
        return options;
    }

    /// <summary>
    /// Configures health check settings.
    /// </summary>
    /// <param name="options">The gateway options to configure.</param>
    /// <param name="configure">Action to configure health check options.</param>
    /// <returns>The configured <see cref="DotnetGrpcGatewayOptions"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="options"/> is <see langword="null"/>.
    /// <paramref name="configure"/> is <see langword="null"/>.
    /// </exception>
    public static DotnetGrpcGatewayOptions ConfigureHealthCheck(this DotnetGrpcGatewayOptions options, Action<HealthCheckOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(configure);

        configure(options.HealthCheck);
        return options;
    }

    /// <summary>
    /// Configures metrics settings.
    /// </summary>
    /// <param name="options">The gateway options to configure.</param>
    /// <param name="configure">Action to configure metrics options.</param>
    /// <returns>The configured <see cref="DotnetGrpcGatewayOptions"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="options"/> is <see langword="null"/>.
    /// <paramref name="configure"/> is <see langword="null"/>.
    /// </exception>
    public static DotnetGrpcGatewayOptions ConfigureMetrics(this DotnetGrpcGatewayOptions options, Action<MetricsOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(configure);

        configure(options.Metrics);
        return options;
    }

    /// <summary>
    /// Configures request logging settings.
    /// </summary>
    /// <param name="options">The gateway options to configure.</param>
    /// <param name="configure">Action to configure request logging options.</param>
    /// <returns>The configured <see cref="DotnetGrpcGatewayOptions"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="options"/> is <see langword="null"/>.
    /// <paramref name="configure"/> is <see langword="null"/>.
    /// </exception>
    public static DotnetGrpcGatewayOptions ConfigureRequestLogging(this DotnetGrpcGatewayOptions options, Action<RequestLoggingOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(configure);

        configure(options.RequestLogging);
        return options;
    }

    /// <summary>
    /// Sets the log level for the gateway.
    /// </summary>
    /// <param name="options">The gateway options to configure.</param>
    /// <param name="logLevel">The log level (e.g., "Debug", "Information", "Warning", "Error").</param>
    /// <returns>The configured <see cref="DotnetGrpcGatewayOptions"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="logLevel"/> is <see langword="null"/>, empty, or whitespace.</exception>
    public static DotnetGrpcGatewayOptions SetLogLevel(this DotnetGrpcGatewayOptions options, string logLevel)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(logLevel);

        options.LogLevel = logLevel;
        return options;
    }

    /// <summary>
    /// Disables request compression.
    /// </summary>
    /// <param name="options">The gateway options to configure.</param>
    /// <returns>The configured <see cref="DotnetGrpcGatewayOptions"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static DotnetGrpcGatewayOptions DisableCompression(this DotnetGrpcGatewayOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.EnableCompression = false;
        return options;
    }

    /// <summary>
    /// Sets the maximum number of concurrent connections.
    /// </summary>
    /// <param name="options">The gateway options to configure.</param>
    /// <param name="maxConcurrentConnections">Maximum concurrent connections.</param>
    /// <returns>The configured <see cref="DotnetGrpcGatewayOptions"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxConcurrentConnections"/> is outside the valid range of 1-10000.</exception>
    public static DotnetGrpcGatewayOptions SetMaxConcurrentConnections(this DotnetGrpcGatewayOptions options, int maxConcurrentConnections)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (maxConcurrentConnections is < 1 or > 10000)
        {
            throw new ArgumentOutOfRangeException(nameof(maxConcurrentConnections), maxConcurrentConnections, "Max concurrent connections must be between 1 and 10000");
        }

        options.MaxConcurrentConnections = maxConcurrentConnections;
        return options;
    }

    /// <summary>
    /// Sets the request timeout in milliseconds.
    /// </summary>
    /// <param name="options">The gateway options to configure.</param>
    /// <param name="timeoutMs">Request timeout in milliseconds.</param>
    /// <returns>The configured <see cref="DotnetGrpcGatewayOptions"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeoutMs"/> is outside the valid range of 1-300000.</exception>
    public static DotnetGrpcGatewayOptions SetRequestTimeout(this DotnetGrpcGatewayOptions options, int timeoutMs)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (timeoutMs is < 1 or > 300000)
        {
            throw new ArgumentOutOfRangeException(nameof(timeoutMs), timeoutMs, "Request timeout must be between 1 and 300000 milliseconds");
        }

        options.RequestTimeoutMs = timeoutMs;
        return options;
    }

    /// <summary>
    /// Sets the health check failure threshold.
    /// </summary>
    /// <param name="options">The gateway options to configure.</param>
    /// <param name="failureThreshold">Number of consecutive failures before marking unhealthy.</param>
    /// <returns>The configured <see cref="DotnetGrpcGatewayOptions"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="failureThreshold"/> is outside the valid range of 0-10.</exception>
    public static DotnetGrpcGatewayOptions SetHealthCheckFailureThreshold(this DotnetGrpcGatewayOptions options, int failureThreshold)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (failureThreshold is < 0 or > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(failureThreshold), failureThreshold, "Failure threshold must be between 0 and 10");
        }

        options.HealthCheck.FailureThreshold = failureThreshold;
        return options;
    }

    /// <summary>
    /// Sets the health check timeout in milliseconds.
    /// </summary>
    /// <param name="options">The gateway options to configure.</param>
    /// <param name="timeoutMs">Health check timeout in milliseconds.</param>
    /// <returns>The configured <see cref="DotnetGrpcGatewayOptions"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeoutMs"/> is outside the valid range of 1-60000.</exception>
    public static DotnetGrpcGatewayOptions SetHealthCheckTimeout(this DotnetGrpcGatewayOptions options, int timeoutMs)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (timeoutMs is < 1 or > 60000)
        {
            throw new ArgumentOutOfRangeException(nameof(timeoutMs), timeoutMs, "Health check timeout must be between 1 and 60000 milliseconds");
        }

        options.HealthCheck.TimeoutMs = timeoutMs;
        return options;
    }

    /// <summary>
    /// Sets the health check interval in seconds.
    /// </summary>
    /// <param name="options">The gateway options to configure.</param>
    /// <param name="intervalSeconds">Health check interval in seconds.</param>
    /// <returns>The configured <see cref="DotnetGrpcGatewayOptions"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="intervalSeconds"/> is outside the valid range of 1-3600.</exception>
    public static DotnetGrpcGatewayOptions SetHealthCheckInterval(this DotnetGrpcGatewayOptions options, int intervalSeconds)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (intervalSeconds is < 1 or > 3600)
        {
            throw new ArgumentOutOfRangeException(nameof(intervalSeconds), intervalSeconds, "Health check interval must be between 1 and 3600 seconds");
        }

        options.HealthCheck.IntervalSeconds = intervalSeconds;
        return options;
    }

    /// <summary>
    /// Sets the metrics collection interval in seconds.
    /// </summary>
    /// <param name="options">The gateway options to configure.</param>
    /// <param name="intervalSeconds">Metrics collection interval in seconds.</param>
    /// <returns>The configured <see cref="DotnetGrpcGatewayOptions"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="intervalSeconds"/> is outside the valid range of 1-86400.</exception>
    public static DotnetGrpcGatewayOptions SetMetricsCollectionInterval(this DotnetGrpcGatewayOptions options, int intervalSeconds)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (intervalSeconds is < 1 or > 86400)
        {
            throw new ArgumentOutOfRangeException(nameof(intervalSeconds), intervalSeconds, "Metrics collection interval must be between 1 and 86400 seconds");
        }

        options.Metrics.CollectionIntervalSeconds = intervalSeconds;
        return options;
    }

    /// <summary>
    /// Sets the metrics retention period in days.
    /// </summary>
    /// <param name="options">The gateway options to configure.</param>
    /// <param name="retentionDays">Metrics retention period in days.</param>
    /// <returns>The configured <see cref="DotnetGrpcGatewayOptions"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="retentionDays"/> is outside the valid range of 1-365.</exception>
    public static DotnetGrpcGatewayOptions SetMetricsRetentionDays(this DotnetGrpcGatewayOptions options, int retentionDays)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (retentionDays is < 1 or > 365)
        {
            throw new ArgumentOutOfRangeException(nameof(retentionDays), retentionDays, "Metrics retention must be between 1 and 365 days");
        }

        options.Metrics.RetentionDays = retentionDays;
        return options;
    }

    /// <summary>
    /// Sets the request logging verbosity level.
    /// </summary>
    /// <param name="options">The gateway options to configure.</param>
    /// <param name="verbosity">The verbosity level.</param>
    /// <returns>The configured <see cref="DotnetGrpcGatewayOptions"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static DotnetGrpcGatewayOptions SetRequestLoggingVerbosity(this DotnetGrpcGatewayOptions options, RequestLoggingVerbosity verbosity)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.RequestLogging.Verbosity = verbosity;
        return options;
    }

    /// <summary>
    /// Enables or disables request logging.
    /// </summary>
    /// <param name="options">The gateway options to configure.</param>
    /// <param name="enabled">Whether request logging is enabled.</param>
    /// <returns>The configured <see cref="DotnetGrpcGatewayOptions"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static DotnetGrpcGatewayOptions SetRequestLoggingEnabled(this DotnetGrpcGatewayOptions options, bool enabled)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.RequestLogging.Enabled = enabled;
        return options;
    }
}