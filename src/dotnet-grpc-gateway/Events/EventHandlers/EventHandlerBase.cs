#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace DotNetGrpcGateway.Events.EventHandlers;

/// <summary>
/// Base class for event handlers providing common validation and logging patterns.
/// Ensures consistent exception handling and validation across all event handlers.
/// </summary>
/// <typeparam name="TEvent">The type of event this handler processes.</typeparam>
public abstract class EventHandlerBase<TEvent> where TEvent : GatewayEvent
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventHandlerBase{TEvent}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    protected EventHandlerBase(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the logger instance for derived classes to use.
    /// </summary>
    protected ILogger Logger => _logger;

    /// <summary>
    /// Validates the event parameter and throws appropriate exceptions.
    /// </summary>
    /// <param name="event">The event to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when the event is null.</exception>
    protected void ValidateEvent(TEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
    }

    /// <summary>
    /// Safely logs a message with the specified log level.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="args">Optional format arguments.</param>
    protected void SafeLog(LogLevel logLevel, string message, params object?[] args)
    {
        try
        {
            _logger.Log(logLevel, message, args);
        }
        catch (Exception ex)
        {
            // Fallback to console if logger fails
            Console.Error.WriteLine($"[{logLevel}] {message}", args);
            Console.Error.WriteLine($"Logger error: {ex.Message}");
        }
    }

    /// <summary>
    /// Safely logs a message with the specified log level and exception.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="args">Optional format arguments.</param>
    protected void SafeLog(LogLevel logLevel, Exception exception, string message, params object?[] args)
    {
        try
        {
            _logger.Log(logLevel, exception, message, args);
        }
        catch (Exception ex)
        {
            // Fallback to console if logger fails
            Console.Error.WriteLine($"[{logLevel}] {message}", args);
            Console.Error.WriteLine($"Exception: {exception}");
            Console.Error.WriteLine($"Logger error: {ex.Message}");
        }
    }
}