#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.IO;
using System.Net.Sockets;

namespace DotNetGrpcGateway.Infrastructure;

/// <summary>
/// Marker exception that repository/upstream implementations can throw to explicitly
/// signal a transient failure (deadlock, connection reset, transient timeout) that
/// <see cref="IRetryPolicy"/> implementations should retry.
/// </summary>
public class TransientPersistenceException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TransientPersistenceException"/> class.
    /// </summary>
    public TransientPersistenceException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TransientPersistenceException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="message"/> is null or empty.</exception>
    public TransientPersistenceException(string message) : base(message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TransientPersistenceException"/> class
    /// with a specified error message and a reference to the inner exception that caused it.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="message"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="innerException"/> is null.</exception>
    public TransientPersistenceException(string message, Exception innerException) : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        ArgumentNullException.ThrowIfNull(innerException);
    }
}

/// <summary>
/// Classifies exceptions as transient (safe to retry) or permanent.
/// </summary>
public interface ITransientExceptionClassifier
{
    /// <summary>
    /// Determines whether <paramref name="exception"/> represents a transient failure
    /// that is safe to retry, such as a timeout, deadlock, or connection reset.
    /// Constraint violations, validation errors, and not-found errors are never transient.
    /// </summary>
    /// <param name="exception">The exception to classify.</param>
    /// <returns><see langword="true"/> when the exception should be retried; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    bool IsTransient(Exception exception);
}

/// <summary>
/// Default <see cref="ITransientExceptionClassifier"/> implementation that treats timeouts,
/// I/O and socket failures, and explicit <see cref="TransientPersistenceException"/> markers
/// as transient, while never retrying constraint or argument style failures.
/// </summary>
public class TransientExceptionClassifier : ITransientExceptionClassifier
{
    /// <inheritdoc />
    public bool IsTransient(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            TransientPersistenceException => true,
            System.TimeoutException => true,
            SocketException => true,
            IOException => true,
            OperationCanceledException { InnerException: System.TimeoutException } => true,
            _ => false,
        };
    }
}
