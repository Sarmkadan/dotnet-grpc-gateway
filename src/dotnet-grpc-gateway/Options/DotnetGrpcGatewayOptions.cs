using System.ComponentModel.DataAnnotations;

namespace DotNetGrpcGateway.Options;

/// <summary>
/// Configuration options for the gateway
/// </summary>
public class DotnetGrpcGatewayOptions
{
    public const string SectionName = "Gateway";

    [Required]
    public string ListenAddress { get; set; } = "0.0.0.0";

    [Range(1, 65535)]
    public int Port { get; set; } = 5000;

    public bool EnableReflection { get; set; } = true;

    public bool EnableMetrics { get; set; } = true;

    [Range(1, 10000)]
    public int MaxConcurrentConnections { get; set; } = 1000;

    [Range(1, 300000)]
    public int RequestTimeoutMs { get; set; } = 30000;

    [Required]
    public string LogLevel { get; set; } = "Information";

    public bool EnableCompression { get; set; } = true;

    [Required]
    public HealthCheckOptions HealthCheck { get; set; } = new();

    [Required]
    public MetricsOptions Metrics { get; set; } = new();

    [Required]
    public RequestLoggingOptions RequestLogging { get; set; } = new();
}

public class HealthCheckOptions
{
    [Range(1, 3600)]
    public int IntervalSeconds { get; set; } = 30;

    [Range(1, 60000)]
    public int TimeoutMs { get; set; } = 5000;

    [Range(0, 10)]
    public int FailureThreshold { get; set; } = 3;
}

public class MetricsOptions
{
    public bool EnableMetrics { get; set; } = true;

    [Range(1, 86400)]
    public int CollectionIntervalSeconds { get; set; } = 60;

    [Range(1, 365)]
    public int RetentionDays { get; set; } = 30;
}

public enum RequestLoggingVerbosity
{
    Minimal,
    Normal,
    Verbose
}

public class RequestLoggingOptions
{
    public bool Enabled { get; set; } = true;

    public RequestLoggingVerbosity Verbosity { get; set; } = RequestLoggingVerbosity.Normal;
}
