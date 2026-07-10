#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DotNetGrpcGateway.Examples;

/// <summary>
/// Extension methods for DynamicRouteConfigurationExample to provide additional
/// functionality for route management and analysis.
/// </summary>
public static class DynamicRouteConfigurationExampleExtensions
{
    /// <summary>
    /// Creates multiple routes from a collection of route configurations.
    /// </summary>
    /// <param name="example">The DynamicRouteConfigurationExample instance</param>
    /// <param name="routes">Collection of route configurations</param>
    /// <returns>Task representing the asynchronous operation</returns>
    public static async Task CreateMultipleRoutesAsync(
        this DynamicRouteConfigurationExample example,
        IEnumerable<object> routes)
    {
        if (routes == null)
        {
            throw new ArgumentNullException(nameof(routes));
        }

        foreach (var route in routes)
        {
            await example.CreateRouteAsync(route);
        }
    }

    /// <summary>
    /// Bulk updates existing routes with new configurations.
    /// </summary>
    /// <param name="example">The DynamicRouteConfigurationExample instance</param>
    /// <param name="updates">Collection of route updates</param>
    /// <returns>Task representing the asynchronous operation</returns>
    public static async Task UpdateMultipleRoutesAsync(
        this DynamicRouteConfigurationExample example,
        IEnumerable<object> updates)
    {
        if (updates == null)
        {
            throw new ArgumentNullException(nameof(updates));
        }

        foreach (var update in updates)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(update),
                Encoding.UTF8,
                "application/json"
            );

            var response = await example.UpdateRouteAsync(content);

            if (response.IsSuccessStatusCode)
            {
                var routeObj = (dynamic)update;
                Console.WriteLine($"✓ Route updated: {routeObj.pattern}");
            }
            else
            {
                Console.WriteLine($"✗ Error updating route: {response.StatusCode}");
            }
        }
    }

    /// <summary>
    /// Deletes multiple routes by their IDs.
    /// </summary>
    /// <param name="example">The DynamicRouteConfigurationExample instance</param>
    /// <param name="routeIds">Collection of route IDs to delete</param>
    /// <returns>Task representing the asynchronous operation</returns>
    public static async Task DeleteMultipleRoutesAsync(
        this DynamicRouteConfigurationExample example,
        IEnumerable<int> routeIds)
    {
        if (routeIds == null)
        {
            throw new ArgumentNullException(nameof(routeIds));
        }

        foreach (var routeId in routeIds)
        {
            var response = await example.DeleteRouteAsync(routeId);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"✓ Route deleted: {routeId}");
            }
            else
            {
                Console.WriteLine($"✗ Error deleting route {routeId}: {response.StatusCode}");
            }
        }
    }

    /// <summary>
    /// Analyzes route performance metrics for all configured routes.
    /// </summary>
    /// <param name="example">The DynamicRouteConfigurationExample instance</param>
    /// <param name="durationMinutes">Duration in minutes to analyze</param>
    /// <returns>Dictionary mapping route IDs to performance metrics</returns>
    public static async Task<Dictionary<int, RoutePerformanceMetrics>> AnalyzeRoutePerformanceAsync(
        this DynamicRouteConfigurationExample example,
        int durationMinutes = 5)
    {
        var metrics = new Dictionary<int, RoutePerformanceMetrics>();

        var response = await example._httpClient.GetAsync(
            $"{example.GetGatewayUrl()}/gateway/performance?durationMinutes={durationMinutes}"
        );

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(result);

            foreach (var element in doc.RootElement.EnumerateArray())
            {
                var routeId = element.GetProperty("routeId").GetInt32();
                var metricsObj = new RoutePerformanceMetrics
                {
                    RequestCount = element.GetProperty("requestCount").GetInt32(),
                    AverageLatencyMs = element.GetProperty("averageLatencyMs").GetDouble(),
                    ErrorRate = element.GetProperty("errorRate").GetDouble(),
                    CacheHitRate = element.GetProperty("cacheHitRate").GetDouble(),
                    MaxConcurrentRequests = element.GetProperty("maxConcurrentRequests").GetInt32()
                };

                metrics[routeId] = metricsObj;
            }
        }

        return metrics;
    }

    /// <summary>
    /// Gets the gateway URL used by the example.
    /// </summary>
    /// <param name="example">The DynamicRouteConfigurationExample instance</param>
    /// <returns>Gateway URL string</returns>
    public static string GetGatewayUrl(this DynamicRouteConfigurationExample example)
    {
        return example._gatewayUrl;
    }

    /// <summary>
    /// Creates a route with additional metadata for better tracking.
    /// </summary>
    /// <param name="example">The DynamicRouteConfigurationExample instance</param>
    /// <param name="pattern">Route pattern</param>
    /// <param name="targetServiceId">Target service ID</param>
    /// <param name="priority">Route priority</param>
    /// <param name="rateLimitPerMinute">Rate limit per minute</param>
    /// <param name="enableCaching">Whether caching is enabled</param>
    /// <param name="description">Route description</param>
    /// <param name="tags">Optional tags for categorization</param>
    /// <returns>Task representing the asynchronous operation</returns>
    public static async Task CreateRouteWithMetadataAsync(
        this DynamicRouteConfigurationExample example,
        string pattern,
        int targetServiceId,
        int priority = 100,
        int rateLimitPerMinute = 1000,
        bool enableCaching = false,
        string? description = null,
        string[]? tags = null)
    {
        var route = new
        {
            pattern,
            targetServiceId,
            matchType = 1, // Prefix match
            priority,
            rateLimitPerMinute,
            enableCaching,
            description = description ?? pattern,
            tags = tags ?? Array.Empty<string>(),
            createdAt = DateTime.UtcNow.ToString("o"),
            isActive = true
        };

        await example.CreateRouteAsync(route);
    }
}

/// <summary>
/// Represents performance metrics for a route.
/// </summary>
public class RoutePerformanceMetrics
{
    public int RequestCount { get; set; }
    public double AverageLatencyMs { get; set; }
    public double ErrorRate { get; set; }
    public double CacheHitRate { get; set; }
    public int MaxConcurrentRequests { get; set; }
}