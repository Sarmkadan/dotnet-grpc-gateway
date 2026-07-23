#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using DotNetGrpcGateway.Infrastructure;

namespace DotNetGrpcGateway.Middleware
{
    /// <summary>
    /// Middleware that initializes and manages the <see cref="RequestContext"/> for each HTTP request.
    /// This ensures that request-specific context flows through the entire async pipeline correctly.
    /// </summary>
    /// <remarks>
    /// This middleware:
    /// <list type="bullet">
    /// <item>Creates a new <see cref="RequestContext"/> for each request</item>
    /// <item>Populates it with request metadata (path, method, client IP)</item>
    /// <item>Stores it in <see cref="RequestContextAccessor.Current"/> for the duration of the request</item>
    /// <item>Ensures proper cleanup when the request completes</item>
    /// </list>
    ///
    /// The use of <see cref="RequestContextAccessor"/> with <see cref="AsyncLocal{T}"/> ensures that
    /// the request context is properly flowed through async/await boundaries, preventing context
    /// leakage between concurrent requests.
    /// </remarks>
    public class RequestContextMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestContextMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestContextMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline</param>
        /// <param name="logger">The logger</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="next"/> or <paramref name="logger"/> is null</exception>
        public RequestContextMiddleware(
            RequestDelegate next,
            ILogger<RequestContextMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Invokes the middleware to set up the request context.
        /// </summary>
        /// <param name="context">The HTTP context</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null</exception>
        public async Task InvokeAsync(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            // Create a new request context for this request
            var requestContext = new RequestContext
            {
                Path = context.Request.Path.ToString(),
                Method = context.Request.Method,
                ClientIp = context.Connection.RemoteIpAddress?.ToString() ?? string.Empty
            };

            // Set the correlation ID from the request headers if available
            if (context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
            {
                requestContext.CorrelationId = correlationId.ToString();
            }

            _logger.LogDebug("Initializing RequestContext for path: {Path}, method: {Method}", requestContext.Path, requestContext.Method);

            // Store the context in the accessor - this will flow through async calls via AsyncLocal
            RequestContextAccessor.Current = requestContext;

            try
            {
                await _next(context);
            }
            finally
            {
                // Clean up the context to prevent memory leaks
                // Note: We don't clear RequestContextAccessor.Current here because the static
                // AsyncLocal will automatically clean up when the async flow completes.
                _logger.LogDebug("Completed RequestContext for path: {Path}", requestContext.Path);
            }
        }
    }
}