using System;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotNetGrpcGateway.Options;

/// <summary>
/// Extension methods for configuring DotnetGrpcGatewayOptions
/// </summary>
public static class DotnetGrpcGatewayOptionsExtensions
{
    /// <summary>
    /// Configures the gateway to listen on localhost only
    /// </summary>
    /// <param name="options">The gateway options</param>
    /// <returns>The configured options for chaining</returns>
    public static DotnetGrpcGatewayOptions UseLocalhost(this DotnetGrpcGatewayOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.ListenAddress = IPAddress.Loopback.ToString();
        return options;
    }

    /// <summary>
    /// Configures the gateway to listen on all network interfaces
    /// </summary>
    /// <param name="options">The gateway options</param>
    /// <returns>The configured options for chaining</returns>
    public static DotnetGrpcGatewayOptions UseAllInterfaces(this DotnetGrpcGatewayOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.ListenAddress = "0.0.0.0";
        return options;
    }

    /// <summary>
    /// Configures the gateway to listen on a specific network interface
    /// </summary>
    /// <param name="options">The gateway options</param>
    /// <param name="address">The IP address to listen on</param>
    /// <returns>The configured options for chaining</returns>
    public static DotnetGrpcGatewayOptions UseAddress(this DotnetGrpcGatewayOptions options, string address)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (string.IsNullOrWhiteSpace(address))
        {
            throw new ArgumentException("Address cannot be null or whitespace", nameof(address));
        }

        options.ListenAddress = address;
        return options;
    }

    /// <summary>
    /// Configures the gateway to use a specific port
    /// </summary>
    /// <param name="options">The gateway options</param>
    /// <param name="port">The port number</param>
    /// <returns>The configured options for chaining</returns>
    public static DotnetGrpcGatewayOptions UsePort(this DotnetGrpcGatewayOptions options, int port)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (port < 1 || port > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 1 and 65535");
        }

        options.Port = port;
        return options;
    }

    /// <summary>
    /// Disables gRPC reflection endpoint
    /// </summary>
    /// <param name="options">The gateway options</param>
    /// <returns>The configured options for chaining</returns>
    public static DotnetGrpcGatewayOptions DisableReflection(this DotnetGrpcGatewayOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.EnableReflection = false;
        return options;
    }

    /// <summary>
    /// Disables metrics collection and reporting
    /// </summary>
    /// <param name="options">The gateway options</param>
    /// <returns>The configured options for chaining</returns>
    public static DotnetGrpcGatewayOptions DisableMetrics(this DotnetGrpcGatewayOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.EnableMetrics = false;
        options.Metrics.EnableMetrics = false;
        return options;
    }

    /// <summary>
    /// Configures health check settings
    /// </summary>
    /// <param name="options">The gateway options</param>
    /// <param name="configure">Action to configure health check options</param>
    /// <returns>The configured options for chaining</returns>
    public static DotnetGrpcGatewayOptions ConfigureHealthCheck(this DotnetGrpcGatewayOptions options, Action<HealthCheckOptions> configure)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        configure(options.HealthCheck);
        return options;
    }

    /// <summary>
    /// Configures metrics settings
    /// </summary>
    /// <param name="options">The gateway options</param>
    /// <param name="configure">Action to configure metrics options</param>
    /// <returns>The configured options for chaining</returns>
    public static DotnetGrpcGatewayOptions ConfigureMetrics(this DotnetGrpcGatewayOptions options, Action<MetricsOptions> configure)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        configure(options.Metrics);
        return options;
    }

    /// <summary>
    /// Configures request logging settings
    /// </summary>
    /// <param name="options">The gateway options</param>
    /// <param name="configure">Action to configure request logging options</param>
    /// <returns>The configured options for chaining</returns>
    public static DotnetGrpcGatewayOptions ConfigureRequestLogging(this DotnetGrpcGatewayOptions options, Action<RequestLoggingOptions> configure)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        configure(options.RequestLogging);
        return options;
    }

    /// <summary>
    /// Sets the log level for the gateway
    /// </summary>
    /// <param name="options">The gateway options</param>
    /// <param name="logLevel">The log level (e.g., "Debug", "Information", "Warning", "Error")</param>
    /// <returns>The configured options for chaining</returns>
    public static DotnetGrpcGatewayOptions SetLogLevel(this DotnetGrpcGatewayOptions options, string logLevel)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (string.IsNullOrWhiteSpace(logLevel))
        {
            throw new ArgumentException("Log level cannot be null or whitespace", nameof(logLevel));
        }

        options.LogLevel = logLevel;
        return options;
    }

    /// <summary>
    /// Disables request compression
    /// </summary>
    /// <param name="options">The gateway options</param>
    /// <returns>The configured options for chaining</returns>
    public static DotnetGrpcGatewayOptions DisableCompression(this DotnetGrpcGatewayOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.EnableCompression = false;
        return options;
    }

    /// <summary>
    /// Sets the maximum number of concurrent connections
    /// </summary>
    /// <param name="options">The gateway options</param>
    /// <param name="maxConcurrentConnections">Maximum concurrent connections</param>
    /// <returns>The configured options for chaining</returns>
    public static DotnetGrpcGatewayOptions SetMaxConcurrentConnections(this DotnetGrpcGatewayOptions options, int maxConcurrentConnections)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (maxConcurrentConnections < 1 || maxConcurrentConnections > 10000)
        {
            throw new ArgumentOutOfRangeException(nameof(maxConcurrentConnections), "Max concurrent connections must be between 1 and 10000");
        }

        options.MaxConcurrentConnections = maxConcurrentConnections;
        return options;
    }

    /// <summary>
    /// Sets the request timeout in milliseconds
    /// </summary>
    /// <param name="options">The gateway options</param>
    /// <param name="timeoutMs">Request timeout in milliseconds</param>
    /// <returns>The configured options for chaining</returns>
    public static DotnetGrpcGatewayOptions SetRequestTimeout(this DotnetGrpcGatewayOptions options, int timeoutMs)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (timeoutMs < 1 || timeoutMs > 300000)
        {
            throw new ArgumentOutOfRangeException(nameof(timeoutMs), "Request timeout must be between 1 and 300000 milliseconds");
        }

        options.RequestTimeoutMs = timeoutMs;
        return options;
    }

    /// <summary>
    /// Sets the health check failure threshold
    /// </summary>
    /// <param name="options">The gateway options</param>
    /// <param name="failureThreshold">Number of consecutive failures before marking unhealthy</param>
    /// <returns>The configured options for chaining</returns>
    public static DotnetGrpcGatewayOptions SetHealthCheckFailureThreshold(this DotnetGrpcGatewayOptions options, int failureThreshold)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (failureThreshold < 0 || failureThreshold > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(failureThreshold), "Failure threshold must be between 0 and 10");
        }

        options.HealthCheck.FailureThreshold = failureThreshold;
        return options;
    }

    /// <summary>
    /// Sets the health check timeout in milliseconds
    /// </summary>
    /// <param name="options">The gateway options</param>
    /// <param name="timeoutMs">Health check timeout in milliseconds</param>
    /// <returns>The configured options for chaining</returns>
    public static DotnetGrpcGatewayOptions SetHealthCheckTimeout(this DotnetGrpcGatewayOptions options, int timeoutMs)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (timeoutMs < 1 || timeoutMs > 60000)
        {
            throw new ArgumentOutOfRangeException(nameof(timeoutMs), "Health check timeout must be between 1 and 60000 milliseconds");
        }

        options.HealthCheck.TimeoutMs = timeoutMs;
        return options;
    }

    /// <summary>
    /// Sets the health check interval in seconds
    /// </summary>
    /// <param name="options">The gateway options</param>
    /// <param name="intervalSeconds">Health check interval in seconds</param>
    /// <returns>The configured options for chaining</returns>
    public static DotnetGrpcGatewayOptions SetHealthCheckInterval(this DotnetGrpcGatewayOptions options, int intervalSeconds)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (intervalSeconds < 1 || intervalSeconds > 3600)
        {
            throw new ArgumentOutOfRangeException(nameof(intervalSeconds), "Health check interval must be between 1 and 3600 seconds");
        }

        options.HealthCheck.IntervalSeconds = intervalSeconds;
        return options;
    }

    /// <summary>
    /// Sets the metrics collection interval in seconds
    /// </summary>
    /// <param name="options">The gateway options</param>
    /// <param name="intervalSeconds">Metrics collection interval in seconds</param>
    /// <returns>The configured options for chaining</returns>
    public static DotnetGrpcGatewayOptions SetMetricsCollectionInterval(this DotnetGrpcGatewayOptions options, int intervalSeconds)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (intervalSeconds < 1 || intervalSeconds > 86400)
        {
            throw new ArgumentOutOfRangeException(nameof(intervalSeconds), "Metrics collection interval must be between 1 and 86400 seconds");
        }

        options.Metrics.CollectionIntervalSeconds = intervalSeconds;
        return options;
    }

    /// <summary>
    /// Sets the metrics retention period in days
    /// </summary>
    /// <param name="options">The gateway options</param>
    /// <param name="retentionDays">Metrics retention period in days</param>
    /// <returns>The configured options for chaining</returns>
    public static DotnetGrpcGatewayOptions SetMetricsRetentionDays(this DotnetGrpcGatewayOptions options, int retentionDays)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (retentionDays < 1 || retentionDays > 365)
        {
            throw new ArgumentOutOfRangeException(nameof(retentionDays), "Metrics retention must be between 1 and 365 days");
        }

        options.Metrics.RetentionDays = retentionDays;
        return options;
    }

    /// <summary>
    /// Sets the request logging verbosity level
    /// </summary>
    /// <param name="options">The gateway options</param>
    /// <param name="verbosity">The verbosity level</param>
    /// <returns>The configured options for chaining</returns>
    public static DotnetGrpcGatewayOptions SetRequestLoggingVerbosity(this DotnetGrpcGatewayOptions options, RequestLoggingVerbosity verbosity)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.RequestLogging.Verbosity = verbosity;
        return options;
    }

    /// <summary>
    /// Enables or disables request logging
    /// </summary>
    /// <param name="options">The gateway options</param>
    /// <param name="enabled">Whether request logging is enabled</param>
    /// <returns>The configured options for chaining</returns>
    public static DotnetGrpcGatewayOptions SetRequestLoggingEnabled(this DotnetGrpcGatewayOptions options, bool enabled)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.RequestLogging.Enabled = enabled;
        return options;
    }
}
