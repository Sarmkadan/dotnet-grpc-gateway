using System;
using System.Collections.Generic;

namespace DotNetGrpcGateway.Exceptions
{
    /// <summary>
    /// Provides extension methods for <see cref="GatewayException"/> and its derived types to facilitate
    /// inspection and handling of exception details.
    /// </summary>
    public static class GatewayExceptionExtensions
    {
        /// <summary>
        /// Determines whether the exception is a rate limit exceeded exception.
        /// </summary>
        /// <param name="exception">The exception to check. Cannot be null.</param>
        /// <returns><see langword="true"/> if the exception is a <see cref="RateLimitException"/>; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/></exception>
        public static bool IsRateLimitExceeded(this GatewayException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return exception is RateLimitException;
        }

        /// <summary>
        /// Determines whether the exception contains any details.
        /// </summary>
        /// <param name="exception">The exception to check. Cannot be null.</param>
        /// <returns><see langword="true"/> if the exception has details and the details collection is not empty; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/></exception>
        public static bool HasDetails(this GatewayException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return exception.Details is { Count: > 0 };
        }

        /// <summary>
        /// Gets the error code from the exception, or returns a default value if the error code is null or empty.
        /// </summary>
        /// <param name="exception">The exception containing the error code. Cannot be null.</param>
        /// <param name="defaultValue">The default value to return if the error code is null or empty.</param>
        /// <returns>The error code if it is not null or empty; otherwise, the default value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/></exception>
        public static string GetErrorCodeOrDefault(this GatewayException exception, string defaultValue)
        {
            ArgumentNullException.ThrowIfNull(exception);
            ArgumentNullException.ThrowIfNull(defaultValue);
            return string.IsNullOrEmpty(exception.ErrorCode) ? defaultValue : exception.ErrorCode;
        }

        /// <summary>
        /// Gets the HTTP status code from the exception, or returns a default value if the status code is null.
        /// </summary>
        /// <param name="exception">The exception containing the HTTP status code. Cannot be null.</param>
        /// <param name="defaultValue">The default value to return if the HTTP status code is null.</param>
        /// <returns>The HTTP status code if it is not null; otherwise, the default value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/></exception>
        public static int GetHttpStatusCodeOrDefault(this GatewayException exception, int defaultValue)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return exception.HttpStatusCode ?? defaultValue;
        }
    }
}
