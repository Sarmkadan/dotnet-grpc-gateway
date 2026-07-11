using System;

namespace DotNetGrpcGateway.Domain
{
    /// <summary>
    /// Provides extension methods for <see cref="AuthenticationToken"/> to simplify common authentication token operations.
    /// </summary>
    public static class AuthenticationTokenExtensions
    {
        /// <summary>
        /// Determines whether the token contains the specified scope.
        /// </summary>
        /// <param name="token">The authentication token to check. Cannot be null.</param>
        /// <param name="scope">The scope to search for. Cannot be null or empty.</param>
        /// <returns><see langword="true"/> if the token contains the scope; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="token"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="scope"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="scope"/> is empty or whitespace.</exception>
        public static bool HasScope(this AuthenticationToken token, string scope)
        {
            ArgumentNullException.ThrowIfNull(token);
            ArgumentException.ThrowIfNullOrWhiteSpace(scope);

            return token.Scopes.Contains(scope, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines whether the token has expired.
        /// </summary>
        /// <param name="token">The authentication token to check. Cannot be null.</param>
        /// <returns><see langword="true"/> if the token has expired; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="token"/> is <see langword="null"/>.</exception>
        public static bool IsExpired(this AuthenticationToken token)
        {
            ArgumentNullException.ThrowIfNull(token);

            return token.ExpiresAt.HasValue && token.ExpiresAt.Value < DateTime.UtcNow;
        }

        /// <summary>
        /// Determines whether the token is near expiry based on the provided threshold.
        /// </summary>
        /// <param name="token">The authentication token to check. Cannot be null.</param>
        /// <param name="threshold">The time span before expiry to consider the token as near expiry. Must be non-negative.</param>
        /// <returns><see langword="true"/> if the token is near expiry; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="token"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="threshold"/> is negative.</exception>
        public static bool IsNearExpiry(this AuthenticationToken token, TimeSpan threshold)
        {
            ArgumentNullException.ThrowIfNull(token);

            if (threshold < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold must be non-negative.");
            }

            return token.ExpiresAt.HasValue && token.ExpiresAt.Value - DateTime.UtcNow <= threshold;
        }
    }
}