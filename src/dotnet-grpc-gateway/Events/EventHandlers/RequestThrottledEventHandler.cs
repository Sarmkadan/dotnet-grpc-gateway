#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Events.EventHandlers;

/// <summary>
/// Event handler for request throttling events.
/// Tracks throttled requests and logs excessive throttling patterns.
/// </summary>
public class RequestThrottledEventHandler : IEventHandler<RequestThrottledEvent>
{
	private readonly ILogger<RequestThrottledEventHandler> _logger;
	private readonly Dictionary<string, int> _throttleCounter = new();
	private readonly object _lock = new();

	public RequestThrottledEventHandler(ILogger<RequestThrottledEventHandler> logger)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// Handles request throttling events by logging the throttling and tracking per-IP counters.
	/// </summary>
	/// <param name="@event">The request throttling event.</param>
	/// <exception cref="ArgumentNullException">Thrown when the event is null.</exception>
	public async Task HandleAsync(RequestThrottledEvent @event)
	{
		ArgumentNullException.ThrowIfNull(@event);

		_logger.LogWarning(
			"Request throttled - ClientIp: {ClientIp}, Path: {Path}, RateLimit: {RateLimit}, EventId: {@EventId}",
		@event.ClientIp ?? "unknown", @event.RequestPath ?? "/unknown", @event.RateLimitPerWindow, @event.EventId);

		// Track throttling per IP to detect potential abuse
		lock (_lock)
		{
			var key = @event.ClientIp ?? "unknown";
			if (_throttleCounter.ContainsKey(key))
				_throttleCounter[key]++;
			else
				_throttleCounter[key] = 1;

			// Alert if same IP is throttled too frequently
			if (_throttleCounter[key] > 5)
			{
				_logger.LogError(
					"Excessive throttling from ClientIp {ClientIp} - {Count} throttles in current window",
				@event.ClientIp ?? "unknown", _throttleCounter[key]);
			}
		}

		await Task.CompletedTask;
	}
}
