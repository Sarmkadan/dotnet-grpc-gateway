// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Infrastructure;

/// <summary>
/// Unit of Work pattern implementation for transaction management
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IGatewayRepository Gateways { get; }
    IServiceRegistry Services { get; }
    IRouteRepository Routes { get; }
    IMetricsRepository Metrics { get; }

    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation);
    Task ExecuteInTransactionAsync(Func<Task> operation);
    Task CommitAsync();
    Task RollbackAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly IGatewayRepository _gateways;
    private readonly IServiceRegistry _services;
    private readonly IRouteRepository _routes;
    private readonly IMetricsRepository _metrics;

    public IGatewayRepository Gateways => _gateways;
    public IServiceRegistry Services => _services;
    public IRouteRepository Routes => _routes;
    public IMetricsRepository Metrics => _metrics;

    public UnitOfWork(
        IGatewayRepository gateways,
        IServiceRegistry services,
        IRouteRepository routes,
        IMetricsRepository metrics)
    {
        _gateways = gateways ?? throw new ArgumentNullException(nameof(gateways));
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _routes = routes ?? throw new ArgumentNullException(nameof(routes));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation)
    {
        try
        {
            var result = await operation();
            await CommitAsync();
            return result;
        }
        catch
        {
            await RollbackAsync();
            throw;
        }
    }

    public async Task ExecuteInTransactionAsync(Func<Task> operation)
    {
        try
        {
            await operation();
            await CommitAsync();
        }
        catch
        {
            await RollbackAsync();
            throw;
        }
    }

    public async Task CommitAsync()
    {
        // Implementation would persist changes to database
        await Task.CompletedTask;
    }

    public async Task RollbackAsync()
    {
        // Implementation would rollback changes
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
