using System;
using System.Collections.Generic;
using System.Linq;

namespace dotnet_grpc_gateway.Domain
{
    public static class AuthenticationTokenExtensions
    {
        public static bool HasScope(this AuthenticationToken token, string scope)
        {
            return token.Scopes.Contains(scope, StringComparer.OrdinalIgnoreCase);
        }

        public static bool IsExpired(this AuthenticationToken token)
        {
            return token.ExpiresAt.HasValue && token.ExpiresAt.Value < DateTime.UtcNow;
        }

        public static bool IsNearExpiry(this AuthenticationToken token, TimeSpan threshold)
        {
            return token.ExpiresAt.HasValue && token.ExpiresAt.Value - DateTime.UtcNow <= threshold;
        }
    }
}
