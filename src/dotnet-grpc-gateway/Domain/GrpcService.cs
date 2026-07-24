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
public int Id { get; set; }

public string Name { get; set; } = null!;

public string ServiceFullName { get; set; } = null!;

public string Host { get; set; } = null!;

public int Port { get; set; } = 5000;

public bool UseTls { get; set; } = false;

public string? Description { get; set; }

public string? ProtoPackage { get; set; }

public int HealthCheckIntervalSeconds { get; set; } = 30;

public int MaxRetries { get; set; } = 5;

public bool IsHealthy { get; set; } = true;

public DateTime LastHealthCheckAt { get; set; }

public string? LastHealthCheckError { get; set; }

public double AverageResponseTimeMs { get; set; } = 0;

public long TotalRequestsProcessed { get; set; } = 0;

public long FailedRequestsCount { get; set; } = 0;

public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

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