#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Infrastructure;

/// <summary>
/// Executes operations with automatic retry using exponential backoff and jitter,
/// limited to exceptions classified as transient.
/// </summary>
public interface IRetryPolicy
{
    /// <summary>
    /// Executes <paramref name="operation"/>, retrying on transient failures according
    /// to the configured <see cref="RetryPolicyOptions"/>.
    /// </summary>
    /// <typeparam name="T">The result type produced by the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="operationName">A short name used for logging/diagnostics.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The result produced by <paramref name="operation"/> once it succeeds.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="operation"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="operationName"/> is null or empty.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the total retry timeout elapses or the token is cancelled.</exception>
    Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, string operationName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes <paramref name="operation"/>, retrying on transient failures according
    /// to the configured <see cref="RetryPolicyOptions"/>.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="operationName">A short name used for logging/diagnostics.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="operation"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="operationName"/> is null or empty.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the total retry timeout elapses or the token is cancelled.</exception>
    Task ExecuteAsync(Func<CancellationToken, Task> operation, string operationName, CancellationToken cancellationToken = default);
}
