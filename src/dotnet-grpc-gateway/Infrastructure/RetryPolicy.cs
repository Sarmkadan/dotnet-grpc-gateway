#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace DotNetGrpcGateway.Infrastructure;

/// <summary>
/// Default <see cref="IRetryPolicy"/> implementation that retries transient failures with
/// exponential backoff and jitter, bounded by a maximum attempt count and total timeout.
/// </summary>
public class RetryPolicy : IRetryPolicy
{
    private readonly RetryPolicyOptions _options;
    private readonly ITransientExceptionClassifier _classifier;
    private readonly ILogger<RetryPolicy> _logger;
    private readonly Random _random = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RetryPolicy"/> class.
    /// </summary>
    /// <param name="options">Retry configuration; when null, defaults are used.</param>
    /// <param name="classifier">Classifier used to decide whether an exception is retryable.</param>
    /// <param name="logger">Logger used to record retry attempts.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="classifier"/> or <paramref name="logger"/> is null.</exception>
    public RetryPolicy(RetryPolicyOptions? options, ITransientExceptionClassifier classifier, ILogger<RetryPolicy> logger)
    {
        ArgumentNullException.ThrowIfNull(classifier);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options ?? new RetryPolicyOptions();
        _classifier = classifier;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, string operationName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentException.ThrowIfNullOrEmpty(operationName);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_options.TotalTimeout);

        var attempt = 0;

        while (true)
        {
            attempt++;

            try
            {
                return await operation(timeoutCts.Token).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException && ShouldRetry(ex, attempt))
            {
                await DelayBeforeRetryAsync(operationName, attempt, ex, timeoutCts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested is false && timeoutCts.IsCancellationRequested)
            {
                throw new OperationCanceledException(
                    $"Operation '{operationName}' exceeded the total retry timeout of {_options.TotalTimeout} after {attempt} attempt(s).",
                    timeoutCts.Token);
            }
        }
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(Func<CancellationToken, Task> operation, string operationName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentException.ThrowIfNullOrEmpty(operationName);

        await ExecuteAsync<object?>(async token =>
        {
            await operation(token).ConfigureAwait(false);
            return null;
        }, operationName, cancellationToken).ConfigureAwait(false);
    }

    private bool ShouldRetry(Exception exception, int attempt) =>
        attempt < _options.MaxAttempts && _classifier.IsTransient(exception);

    private async Task DelayBeforeRetryAsync(string operationName, int attempt, Exception exception, CancellationToken cancellationToken)
    {
        var delay = ComputeDelayWithJitter(attempt);

        _logger.LogWarning(
            exception,
            "Transient failure on attempt {Attempt}/{MaxAttempts} for '{OperationName}'. Retrying in {DelayMs}ms.",
            attempt,
            _options.MaxAttempts,
            operationName,
            delay.TotalMilliseconds);

        try
        {
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw new OperationCanceledException(
                $"Operation '{operationName}' exceeded the total retry timeout of {_options.TotalTimeout} while waiting to retry.",
                cancellationToken);
        }
    }

    private TimeSpan ComputeDelayWithJitter(int attempt)
    {
        var exponentialMs = _options.BaseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1);
        var cappedMs = Math.Min(exponentialMs, _options.MaxDelay.TotalMilliseconds);

        var jitterRange = cappedMs * _options.JitterFactor;
        var jitterMs = (_random.NextDouble() * 2.0 - 1.0) * jitterRange;

        var finalMs = Math.Max(0, cappedMs + jitterMs);
        return TimeSpan.FromMilliseconds(finalMs);
    }
}
