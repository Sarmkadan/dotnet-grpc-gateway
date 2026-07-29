// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Domain;

namespace DotNetGrpcGateway.Domain;

/// <summary>
/// Provides extension methods for <see cref="GatewayRoute"/>.
/// </summary>
public static class GatewayRouteExtensions
{
    /// <summary>
    /// Checks if the route matches the given request path.
    /// The path is expected to be in the format 'ServiceName.MethodName'.
    /// </summary>
    public static bool MatchesPath(this GatewayRoute route, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        var lastDotIndex = path.LastIndexOf('.');
        if (lastDotIndex == -1)
            return false;

        var serviceName = path.Substring(0, lastDotIndex);
        var methodName = path.Substring(lastDotIndex + 1);

        return route.MatchesRequest(serviceName, methodName);
    }

    /// <summary>
    /// Returns a string representation of the route for display purposes.
    /// </summary>
    public static string ToDisplayString(this GatewayRoute route)
    {
        return $"[{route.Id}] {route.Pattern} ({route.MatchType}) -> Service ID: {route.TargetServiceId}";
    }
}
