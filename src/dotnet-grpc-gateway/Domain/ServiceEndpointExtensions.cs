using System;

namespace DotNetGrpcGateway.Domain
{
    /// <summary>
    /// Provides extension methods for <see cref="ServiceEndpoint"/> instances to facilitate common endpoint operations.
    /// </summary>
    public static class ServiceEndpointExtensions
    {
        /// <summary>
        /// Determines whether the endpoint is available for handling requests.
        /// </summary>
        /// <param name="endpoint">The endpoint to check.</param>
        /// <returns><see langword="true"/> if the endpoint is healthy and has a positive weight; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="endpoint"/> is <see langword="null"/>.</exception>
        public static bool IsAvailable(this ServiceEndpoint endpoint)
        {
            ArgumentNullException.ThrowIfNull(endpoint);
            return endpoint.IsHealthy && endpoint.Weight > 0;
        }

        /// <summary>
        /// Calculates the success rate of the endpoint based on handled requests.
        /// </summary>
        /// <param name="endpoint">The endpoint to calculate success rate for.</param>
        /// <returns>The success rate as a value between 0 and 1, where 1 represents 100% success.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="endpoint"/> is <see langword="null"/>.</exception>
        public static double GetSuccessRate(this ServiceEndpoint endpoint)
        {
            ArgumentNullException.ThrowIfNull(endpoint);

            if (endpoint.TotalRequestsHandled == 0)
            {
                return 0;
            }

            var successful = endpoint.TotalRequestsHandled - endpoint.FailedRequestsCount;
            if (successful < 0)
            {
                successful = 0;
            }

            return (double)successful / endpoint.TotalRequestsHandled;
        }

        /// <summary>
        /// Determines whether the endpoint was recently used within the specified time threshold.
        /// </summary>
        /// <param name="endpoint">The endpoint to check.</param>
        /// <param name="threshold">The time threshold within which the endpoint is considered recently used.</param>
        /// <returns><see langword="true"/> if the endpoint was used within the threshold; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="endpoint"/> is <see langword="null"/>.</exception>
        public static bool IsRecentlyUsed(this ServiceEndpoint endpoint, TimeSpan threshold)
        {
            ArgumentNullException.ThrowIfNull(endpoint);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(threshold, TimeSpan.Zero);
            return DateTime.UtcNow - endpoint.LastUsedAt <= threshold;
        }
    }
}
