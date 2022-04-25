using System;
using System.Collections.Generic;
using System.Linq;

namespace DotnetGrpcGateway.Domain
{
    /// <summary>
    /// Extension methods for <see cref="ServiceReflectionInfo"/>.
    /// </summary>
    public static class ServiceReflectionInfoExtensions
    {
        /// <summary>
        /// Returns a short, human-readable summary of the service.
        /// </summary>
        /// <param name="info">The service reflection information to summarize.</param>
        /// <returns>A formatted string containing service metadata.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="info"/> is <c>null</c>.</exception>
        public static string ToSummaryString(this ServiceReflectionInfo info)
        {
            ArgumentNullException.ThrowIfNull(info);

            return $"{info.ServiceName} ({info.ServiceFullName}) - " +
                   $"Methods: {info.Methods?.Count ?? 0}, " +
                   $"Available: {info.IsAvailable}, " +
                   $"ReflectedAt: {info.ReflectedAt:u}";
        }

        /// <summary>
        /// Gets the names of all methods defined on the service.
        /// </summary>
        /// <param name="info">The service reflection information.</param>
        /// <returns>An enumerable of method names, excluding null or empty names.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="info"/> is <c>null</c>.</exception>
        public static IEnumerable<string> GetMethodNames(this ServiceReflectionInfo info)
        {
            ArgumentNullException.ThrowIfNull(info);

            return info.Methods
                .Where(m => !string.IsNullOrEmpty(m.Name))
                .Select(m => m.Name);
        }

        /// <summary>
        /// Determines whether the service is considered healthy.
        /// A service is healthy when it is marked as available and has no error message.
        /// </summary>
        /// <param name="info">The service reflection information to evaluate.</param>
        /// <returns><c>true</c> if the service is healthy; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="info"/> is <c>null</c>.</exception>
        public static bool IsHealthy(this ServiceReflectionInfo info)
        {
            ArgumentNullException.ThrowIfNull(info);

            return info.IsAvailable && string.IsNullOrWhiteSpace(info.ErrorMessage);
        }

        /// <summary>
        /// Retrieves a method descriptor by its name, or <c>null</c> if not found.
        /// </summary>
        /// <param name="info">The service reflection information.</param>
        /// <param name="methodName">The name of the method to retrieve.</param>
        /// <returns>The method descriptor if found; otherwise, <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="info"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="methodName"/> is null or empty.</exception>
        public static ServiceMethodDescriptor? GetMethodByName(this ServiceReflectionInfo info, string methodName)
        {
            ArgumentNullException.ThrowIfNull(info);
            ArgumentException.ThrowIfNullOrEmpty(methodName, nameof(methodName));

            return info.Methods?.FirstOrDefault(m => string.Equals(m.Name, methodName, StringComparison.Ordinal));
        }
    }
}