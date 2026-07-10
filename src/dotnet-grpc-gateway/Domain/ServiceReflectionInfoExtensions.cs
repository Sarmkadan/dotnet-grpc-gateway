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
        /// Returns a short, human‑readable summary of the service.
        /// </summary>
        public static string ToSummaryString(this ServiceReflectionInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));

            return $"{info.ServiceName} ({info.ServiceFullName}) - " +
                   $"Methods: {info.Methods?.Count ?? 0}, " +
                   $"Available: {info.IsAvailable}, " +
                   $"ReflectedAt: {info.ReflectedAt:u}";
        }

        /// <summary>
        /// Gets the names of all methods defined on the service.
        /// </summary>
        public static IEnumerable<string> GetMethodNames(this ServiceReflectionInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (info.Methods == null) return Enumerable.Empty<string>();

            return info.Methods
                       .Where(m => !string.IsNullOrEmpty(m.Name))
                       .Select(m => m.Name);
        }

        /// <summary>
        /// Determines whether the service is considered healthy.
        /// A service is healthy when it is marked as available and has no error message.
        /// </summary>
        public static bool IsHealthy(this ServiceReflectionInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));

            return info.IsAvailable && string.IsNullOrWhiteSpace(info.ErrorMessage);
        }

        /// <summary>
        /// Retrieves a method descriptor by its name, or <c>null</c> if not found.
        /// </summary>
        public static ServiceMethodDescriptor? GetMethodByName(this ServiceReflectionInfo info, string methodName)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (string.IsNullOrEmpty(methodName)) throw new ArgumentException("Method name must be provided.", nameof(methodName));
            if (info.Methods == null) return null;

            return info.Methods.FirstOrDefault(m => string.Equals(m.Name, methodName, StringComparison.Ordinal));
        }
    }
}
