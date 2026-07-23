#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Infrastructure;

namespace DotNetGrpcGateway.Services;

/// <summary>
/// Service that wraps gRPC calls with circuit breaker protection.
/// Provides circuit breaker aware invocation methods for downstream services.
/// </summary>
public interface ICircuitBreakerService
{
    /// <summary>
    /// Invokes a unary gRPC method with circuit breaker protection.
    /// </summary>
    Task<T> InvokeWithCircuitBreakerAsync<T>(
        GrpcService service,
        string methodName,
        object request,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Invokes a server-streaming gRPC method with circuit breaker protection.
    /// </summary>
    Task<Stream> InvokeStreamingWithCircuitBreakerAsync(
        GrpcService service,
        string methodName,
        object request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of ICircuitBreakerService that integrates circuit breakers
/// with gRPC client invocations.
/// </summary>
public class CircuitBreakerService : ICircuitBreakerService
{
    private readonly IGrpcClientFactory _grpcClientFactory;
    private readonly ICircuitBreakerRegistry _circuitBreakerRegistry;
    private readonly ILogger<CircuitBreakerService> _logger;

    public CircuitBreakerService(
        IGrpcClientFactory grpcClientFactory,
        ICircuitBreakerRegistry circuitBreakerRegistry,
        ILogger<CircuitBreakerService> logger)
    {
        _grpcClientFactory = grpcClientFactory ?? throw new ArgumentNullException(nameof(grpcClientFactory));
        _circuitBreakerRegistry = circuitBreakerRegistry ?? throw new ArgumentNullException(nameof(circuitBreakerRegistry));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<T> InvokeWithCircuitBreakerAsync<T>(
        GrpcService service,
        string methodName,
        object request,
        CancellationToken cancellationToken = default) where T : class
    {
        if (service is null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        if (string.IsNullOrWhiteSpace(methodName))
        {
            throw new ArgumentException("Method name is required", nameof(methodName));
        }

        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var circuitBreaker = _circuitBreakerRegistry.GetOrCreate(service.Id);

        try
        {
            // Check if circuit allows the request
            if (!circuitBreaker.AllowRequest())
            {
                _logger.LogWarning(
                    "Circuit breaker OPEN for service {ServiceName} (ID: {ServiceId}) - request rejected",
                    service.Name,
                    service.Id);

                throw new HttpRequestException(
                    $"Service {service.Name} is currently unavailable due to circuit breaker protection");
            }

            // Circuit allows the request, invoke the service
            var result = await _grpcClientFactory.InvokeAsync<T>(service, methodName, request, cancellationToken);

            // Record success
            circuitBreaker.RecordSuccess();

            return result;
        }
        catch (Exception ex)
        {
            // Record failure
            circuitBreaker.RecordFailure();

            _logger.LogError(
                ex,
                "Circuit breaker invocation failed for service {ServiceName} (ID: {ServiceId}) - {ErrorMessage}",
                service.Name,
                service.Id,
                ex.Message);

            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Stream> InvokeStreamingWithCircuitBreakerAsync(
        GrpcService service,
        string methodName,
        object request,
        CancellationToken cancellationToken = default)
    {
        if (service is null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        if (string.IsNullOrWhiteSpace(methodName))
        {
            throw new ArgumentException("Method name is required", nameof(methodName));
        }

        var circuitBreaker = _circuitBreakerRegistry.GetOrCreate(service.Id);

        try
        {
            // Check if circuit allows the request
            if (!circuitBreaker.AllowRequest())
            {
                _logger.LogWarning(
                    "Circuit breaker OPEN for service {ServiceName} (ID: {ServiceId}) - request rejected",
                    service.Name,
                    service.Id);

                throw new HttpRequestException(
                    $"Service {service.Name} is currently unavailable due to circuit breaker protection");
            }

            // Circuit allows the request, invoke the service
            var stream = await _grpcClientFactory.InvokeStreamingAsync(service, methodName, request, cancellationToken);

            // Record success
            circuitBreaker.RecordSuccess();

            return stream;
        }
        catch (Exception ex)
        {
            // Record failure
            circuitBreaker.RecordFailure();

            _logger.LogError(
                ex,
                "Circuit breaker streaming invocation failed for service {ServiceName} (ID: {ServiceId}) - {ErrorMessage}",
                service.Name,
                service.Id,
                ex.Message);

            throw;
        }
    }
}
