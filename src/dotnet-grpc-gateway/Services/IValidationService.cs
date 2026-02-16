// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Exceptions;

namespace DotNetGrpcGateway.Services;

/// <summary>
/// Validates requests, configurations, and authentication tokens
/// </summary>
public interface IValidationService
{
    void ValidateGatewayConfiguration(GatewayConfiguration config);
    void ValidateGrpcService(GrpcService service);
    void ValidateGatewayRoute(GatewayRoute route);
    void ValidateRequestMetric(RequestMetric metric);
    Task<bool> ValidateAuthenticationTokenAsync(string token);
    Task<bool> ValidateAuthorizationAsync(string clientId, int serviceId);
    bool ValidateIpAddress(string ipAddress);
}

public class ValidationService : IValidationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ValidationService> _logger;

    public ValidationService(IUnitOfWork unitOfWork, ILogger<ValidationService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void ValidateGatewayConfiguration(GatewayConfiguration config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(config.Name))
            errors.Add("Gateway name is required");

        if (config.Port < 1 || config.Port > 65535)
            errors.Add("Port must be between 1 and 65535");

        if (config.MaxConcurrentConnections < 1)
            errors.Add("Max concurrent connections must be greater than 0");

        if (config.RequestTimeoutMs < 100)
            errors.Add("Request timeout must be at least 100ms");

        if (config.MaxMessageSize < 1024)
            errors.Add("Max message size must be at least 1KB");

        if (errors.Count > 0)
        {
            var message = string.Join("; ", errors);
            throw new ConfigurationException("GatewayConfiguration", message);
        }

        _logger.LogDebug("Gateway configuration validated successfully");
    }

    public void ValidateGrpcService(GrpcService service)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(service.Name))
            errors.Add("Service name is required");

        if (string.IsNullOrWhiteSpace(service.Host))
            errors.Add("Service host is required");

        if (service.Port < 1 || service.Port > 65535)
            errors.Add("Service port must be between 1 and 65535");

        if (service.MaxRetries < 0)
            errors.Add("Max retries cannot be negative");

        if (errors.Count > 0)
        {
            var message = string.Join("; ", errors);
            throw new ConfigurationException("GrpcService", message);
        }

        _logger.LogDebug("gRPC service '{ServiceName}' validated successfully", service.Name);
    }

    public void ValidateGatewayRoute(GatewayRoute route)
    {
        if (route == null)
            throw new ArgumentNullException(nameof(route));

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(route.Pattern))
            errors.Add("Route pattern is required");

        if (route.TargetServiceId <= 0)
            errors.Add("Target service ID must be valid");

        if (route.Priority < 0 || route.Priority > 1000)
            errors.Add("Priority must be between 0 and 1000");

        if (route.RateLimitPerMinute <= 0)
            errors.Add("Rate limit must be greater than 0");

        if (errors.Count > 0)
        {
            var message = string.Join("; ", errors);
            throw new ConfigurationException("GatewayRoute", message);
        }

        _logger.LogDebug("Gateway route '{Pattern}' validated successfully", route.Pattern);
    }

    public void ValidateRequestMetric(RequestMetric metric)
    {
        if (metric == null)
            throw new ArgumentNullException(nameof(metric));

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(metric.ServiceName))
            errors.Add("Service name is required");

        if (string.IsNullOrWhiteSpace(metric.MethodName))
            errors.Add("Method name is required");

        if (string.IsNullOrWhiteSpace(metric.ClientIpAddress))
            errors.Add("Client IP address is required");

        if (metric.DurationMs < 0)
            errors.Add("Duration cannot be negative");

        if (errors.Count > 0)
        {
            var message = string.Join("; ", errors);
            throw new ConfigurationException("RequestMetric", message);
        }
    }

    public async Task<bool> ValidateAuthenticationTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("Authentication attempted with empty token");
            return false;
        }

        try
        {
            // In real implementation, would validate JWT or API key against database
            // For now, just do basic validation
            return token.Length > 20 && token.Contains(".");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating authentication token");
            return false;
        }
    }

    public async Task<bool> ValidateAuthorizationAsync(string clientId, int serviceId)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            return false;

        try
        {
            // In real implementation, would check token against service access list
            _logger.LogDebug("Validating authorization for client {ClientId} accessing service {ServiceId}", clientId, serviceId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating authorization");
            return false;
        }
    }

    public bool ValidateIpAddress(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return false;

        var parts = ipAddress.Split('.');
        if (parts.Length != 4)
            return false;

        return parts.All(part =>
            int.TryParse(part, out var num) && num >= 0 && num <= 255);
    }
}
