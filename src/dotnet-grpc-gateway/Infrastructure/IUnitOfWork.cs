#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetGrpcGateway.Infrastructure;

/// <summary>
/// Unit of Work pattern implementation for transaction management
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IGatewayRepository Gateways { get; }
    IServiceRegistry Services { get; }
    IRouteRepository Routes { get; }
    IMetricsRepository Metrics { get; }

    /// <summary>
    /// Executes an operation within a transaction and commits on success.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an operation within a transaction and commits on success.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when there is no active transaction.</exception>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}

public class UnitOfWork : IUnitOfWork
{
    private readonly IGatewayRepository _gateways;
    private readonly IServiceRegistry _services;
    private readonly IRouteRepository _routes;
    private readonly IMetricsRepository _metrics;
    private bool _disposed;
    private bool _transactionCommitted;

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

    /// <summary>
    /// Executes an operation within a transaction and commits on success.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var result = await operation();
            await CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Executes an operation within a transaction and commits on success.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            await operation();
            await CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when there is no active transaction.</exception>
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_transactionCommitted)
            throw new InvalidOperationException("Transaction has already been committed.");

        // Implementation would persist changes to database
        await Task.CompletedTask;
        _transactionCommitted = true;
    }

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Implementation would rollback changes
        await Task.CompletedTask;
        _transactionCommitted = true; // Prevent double rollback
    }

    /// <summary>
    /// Disposes the unit of work and rolls back any uncommitted transaction.
    /// </summary>
    [Obsolete("Use DisposeAsync instead for proper async disposal in async contexts. This synchronous dispose may block threads.")]
    public void Dispose()
    {
        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Asynchronously disposes the unit of work and rolls back any uncommitted transaction.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    private async ValueTask DisposeAsyncCore()
    {
        if (_disposed)
            return;

        _disposed = true;

        // Rollback any uncommitted transaction
        if (!_transactionCommitted)
        {
            try
            {
                await RollbackAsync().ConfigureAwait(false);
            }
            catch
            {
                // Swallow exceptions during disposal to prevent cascading failures
            }
        }
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        _disposed = true;

        if (disposing && !_transactionCommitted)
        {
            // Synchronous rollback on dispose
            // Note: This may block in async contexts - prefer DisposeAsync
            try
            {
                RollbackAsync().GetAwaiter().GetResult();
            }
            catch
            {
                // Swallow exceptions during disposal to prevent cascading failures
            }
        }
    }

    ~UnitOfWork()
    {
        Dispose(disposing: false);
    }
}