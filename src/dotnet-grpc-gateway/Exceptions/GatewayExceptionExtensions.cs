using System;
using System.Collections.Generic;

namespace dotnet_grpc_gateway.Exceptions
{
    public static class GatewayExceptionExtensions
    {
        public static bool IsRateLimitExceeded(this GatewayException exception)
        {
            return exception is RateLimitException;
        }

        public static bool HasDetails(this GatewayException exception)
        {
            return exception.Details != null && exception.Details.Count > 0;
        }

        public static string GetErrorCodeOrDefault(this GatewayException exception, string defaultValue)
        {
            return string.IsNullOrEmpty(exception.ErrorCode) ? defaultValue : exception.ErrorCode;
        }

        public static int GetHttpStatusCodeOrDefault(this GatewayException exception, int defaultValue)
        {
            return exception.HttpStatusCode ?? defaultValue;
        }
    }
}
