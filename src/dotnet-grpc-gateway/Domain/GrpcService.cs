#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Domain;

/// <summary>
/// Represents a gRPC service that can be routed through the gateway
/// </summary>
public class GrpcService
{
/// <summary>
    /// The unique identifier for the gRPC service.
    /// </summary>
    public int Id { get; set; }

/// <summary>
    /// The name of the gRPC service.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// The fully qualified name of the gRPC service.
    /// </summary>
    public string ServiceFullName { get; set; } = null!;

    /// <summary>
    /// The host address of the gRPC service.
    /// </summary>
    public string Host { get; set; } = null!;

    /// <summary>
    /// The port on which the gRPC service listens.
    /// </summary>
    public int Port { get; set; } = 5000;

    /// <summary>
    /// Indicates whether the gRPC service uses TLS.
    /// </summary>
    public bool UseTls { get; set; } = false;

    /// <summary>
    /// An optional description of the gRPC service.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The optional protobuf package name of the gRPC service.
    /// </summary>
    public string? ProtoPackage { get; set; }

    /// <summary>
    /// The interval in seconds between health checks.
    /// </summary>
    public int HealthCheckIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// The maximum number of retries for requests to the service.
    /// </summary>
    public int MaxRetries { get; set; } = 5;

    /// <summary>
    /// Indicates whether the service is currently healthy.
    /// </summary>
    public bool IsHealthy { get; set; } = true;

    /// <summary>
    /// The timestamp of the last health check.
    /// </summary>
    public DateTime LastHealthCheckAt { get; set; }

    /// <summary>
    /// The error message from the last health check, if any.
    /// </summary>
    public string? LastHealthCheckError { get; set; }

    /// <summary>
    /// The average response time of the service in milliseconds.
    /// </summary>
    public double AverageResponseTimeMs { get; set; } = 0;

    /// <summary>
    /// The total number of requests processed by the service.
    /// </summary>
    public long TotalRequestsProcessed { get; set; } = 0;

    /// <summary>
    /// The number of failed requests to the service.
    /// </summary>
    public long FailedRequestsCount { get; set; } = 0;

    /// <summary>
    /// The timestamp when the service was registered.
    /// </summary>
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The timestamp when the service was last modified.
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates whether the service is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

/// <summary>
/// Gets or sets the current service status.
/// </summary>
public ServiceStatus Status { get; set; } = ServiceStatus.Active;

/// <summary>
/// Gets or sets the timestamp when the service was marked for draining.
/// Null when not draining.
/// </summary>
public DateTime? DrainStartedAt { get; set; }

/// <summary>
/// Gets or sets the drain timeout in seconds.
/// </summary>
public int DrainTimeoutSeconds { get; set; } = 30;

public string GetEndpointUri() => $"{(UseTls ? "https" : "http")}://{Host}:{Port}";

public override string ToString() => $"GrpcService {{ Id = {Id}, Name = {Name}, ServiceFullName = {ServiceFullName}, Host = {Host}, Port = {Port}, UseTls = {UseTls}, Status = {Status}, IsHealthy = {IsHealthy} }}";

public void Validate()
{
if (string.IsNullOrWhiteSpace(Name))
throw new InvalidOperationException("Service name is required");

if (string.IsNullOrWhiteSpace(ServiceFullName))
throw new InvalidOperationException("Service full name is required");

if (string.IsNullOrWhiteSpace(Host))
throw new InvalidOperationException("Service host is required");

if (Port < 1 || Port > 65535)
throw new InvalidOperationException("Service port must be between 1 and 65535");

if (HealthCheckIntervalSeconds < 1)
throw new InvalidOperationException("Health check interval must be at least 1 second");

if (MaxRetries < 0)
throw new InvalidOperationException("Max retries cannot be negative");
}

public void UpdateHealthStatus(bool isHealthy, string? errorMessage = null)
{
IsHealthy = isHealthy;
LastHealthCheckAt = DateTime.UtcNow;
LastHealthCheckError = errorMessage;
}

public void RecordRequestMetric(double responseTimeMs, bool success)
{
TotalRequestsProcessed++;
if (!success) FailedRequestsCount++;

// Update running average response time
AverageResponseTimeMs = (AverageResponseTimeMs * (TotalRequestsProcessed - 1) + responseTimeMs) / TotalRequestsProcessed;
ModifiedAt = DateTime.UtcNow;
}

/// <summary>
/// Marks the service as draining and sets the drain start time.
/// </summary>
/// <param name="drainTimeoutSeconds">The drain timeout in seconds. Defaults to 30 seconds.</param>
public void MarkForDrain(int drainTimeoutSeconds = 30)
{
Status = ServiceStatus.Draining;
DrainStartedAt = DateTime.UtcNow;
DrainTimeoutSeconds = drainTimeoutSeconds;
ModifiedAt = DateTime.UtcNow;
}

/// <summary>
/// Completes the drain process by marking the service as unregistered.
/// </summary>
public void CompleteDrain()
{
Status = ServiceStatus.Unregistered;
IsActive = false;
DrainStartedAt = null;
ModifiedAt = DateTime.UtcNow;
}

/// <summary>
/// Checks if the drain period has elapsed.
/// </summary>
/// <returns>True if drain period has elapsed or no drain in progress; otherwise false.</returns>
public bool IsDrainComplete()
{
if (Status != ServiceStatus.Draining || !DrainStartedAt.HasValue)
return true;

var elapsed = DateTime.UtcNow - DrainStartedAt.Value;
return elapsed.TotalSeconds >= DrainTimeoutSeconds;
}
}

/// <summary>
/// Service status enumeration for tracking service lifecycle
/// </summary>
public enum ServiceStatus
{
/// <summary>
/// Service is active and accepting new requests
/// </summary>
Active = 0,

/// <summary>
/// Service is being drained - no new requests routed, existing requests allowed to complete
/// </summary>
Draining = 1,

/// <summary>
/// Service has been unregistered and removed from active routing
/// </summary>
Unregistered = 2
}