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
        /// <param name="token">The authentication token to check. Cannot be <c>null</c>.</param>
        /// <param name="scope">The scope to search for. Cannot be <c>null</c>, empty, or whitespace.</param>
        /// <returns><see langword="true"/> if the token's <c>Scopes</c> collection contains <paramref name="scope"/> (case‑insensitive); otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="token"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="scope"/> is <c>null</c>, empty, or consists only of whitespace.</exception>
        public static bool HasScope(this AuthenticationToken token, string scope)
        {
            ArgumentNullException.ThrowIfNull(token);
            ArgumentException.ThrowIfNullOrWhiteSpace(scope);

            return token.Scopes.Contains(scope, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines whether the token has expired.
        /// </summary>
        /// <param name="token">The authentication token to check. Cannot be <c>null</c>.</param>
        /// <returns>
        /// <see langword="true"/> if the token defines an <c>ExpiresAt</c> value and that value is earlier than the current UTC time;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="token"/> is <c>null</c>.</exception>
        public static bool IsExpired(this AuthenticationToken token)
        {
            ArgumentNullException.ThrowIfNull(token);

            return token.ExpiresAt.HasValue && token.ExpiresAt.Value < DateTime.UtcNow;
        }

        /// <summary>
        /// Determines whether the token is near expiry based on the provided threshold.
        /// </summary>
        /// <param name="token">The authentication token to check. Cannot be <c>null</c>.</param>
        /// <param name="threshold">
        /// The time span before expiry that qualifies the token as “near expiry”. Must be zero or positive.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the token defines an <c>ExpiresAt</c> value and the remaining time until that expiry
        /// is less than or equal to <paramref name="threshold"/>; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="token"/> is <c>null</c>.</exception>
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
