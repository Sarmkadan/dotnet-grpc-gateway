using System;

namespace DotnetGrpcGateway.Domain
{
    /// <summary>
    /// Extension methods that add useful behaviour to <see cref="GrpcService"/>.
    /// </summary>
    public static class GrpcServiceExtensions
    {
        /// <summary>
        /// Determines whether a health‑check is due based on the configured interval
        /// and the timestamp of the last health check.
        /// </summary>
        public static bool IsHealthCheckDue(this GrpcService service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            // If the service has never been health‑checked, we consider it due.
            if (service.LastHealthCheckAt == default)
                return true;

            var elapsedSeconds = (DateTime.UtcNow - service.LastHealthCheckAt).TotalSeconds;
            return elapsedSeconds >= service.HealthCheckIntervalSeconds;
        }

        /// <summary>
        /// Returns the full URI that can be used to perform a health‑check on the service.
        /// </summary>
        public static string GetHealthCheckUri(this GrpcService service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            // Convention: health‑check endpoint is "/health" on the service base URI.
            return $"{service.GetEndpointUri()}/health";
        }

        /// <summary>
        /// Records a successful request, updating request counters and the average response time.
        /// </summary>
        /// <param name="responseTimeMs">The response time of the request in milliseconds.</param>
        public static void RecordSuccessfulRequest(this GrpcService service, double responseTimeMs)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (responseTimeMs < 0) throw new ArgumentOutOfRangeException(nameof(responseTimeMs));

            // Increment total processed requests.
            service.TotalRequestsProcessed++;

            // Update the running average response time.
            // NewAverage = ((oldAverage * (n‑1)) + newValue) / n
            var previousCount = service.TotalRequestsProcessed - 1;
            service.AverageResponseTimeMs =
                ((service.AverageResponseTimeMs * previousCount) + responseTimeMs) / service.TotalRequestsProcessed;

            // A successful request implies the service is healthy.
            service.IsHealthy = true;
            service.LastHealthCheckAt = DateTime.UtcNow;
            service.LastHealthCheckError = null;
        }

        /// <summary>
        /// Records a failed request, incrementing the failure counter and storing the error message.
        /// </summary>
        /// <param name="errorMessage">A description of the failure.</param>
        public static void RecordFailedRequest(this GrpcService service, string errorMessage)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (errorMessage == null) throw new ArgumentNullException(nameof(errorMessage));

            service.FailedRequestsCount++;
            service.LastHealthCheckError = errorMessage;
            service.IsHealthy = false;
            service.LastHealthCheckAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Returns a concise, human‑readable summary of the service.
        /// </summary>
        public static string GetSummary(this GrpcService service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            return $"[{service.Id}] {service.Name} ({service.ServiceFullName}) - " +
                   $"{service.GetEndpointUri()} - " +
                   $"Active: {service.IsActive}, Healthy: {service.IsHealthy}, " +
                   $"Requests: {service.TotalRequestsProcessed}, Failures: {service.FailedRequestsCount}";
        }
    }
}
