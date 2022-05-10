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
/// Extension methods for <see cref="DynamicRouteConfigurationExample"/> to provide additional
/// functionality for route management and analysis.
/// </summary>
public static class DynamicRouteConfigurationExampleExtensions
{
    /// <summary>
    /// Creates multiple routes from a collection of route configurations.
    /// </summary>
    /// <param name="example">The <see cref="DynamicRouteConfigurationExample"/> instance</param>
    /// <param name="routes">Collection of route configurations</param>
    /// <exception cref="ArgumentNullException"><paramref name="routes"/> is <see langword="null"/></exception>
    public static async Task CreateMultipleRoutesAsync(
        this DynamicRouteConfigurationExample example,
        IEnumerable<object> routes)
    {
        ArgumentNullException.ThrowIfNull(example);
        ArgumentNullException.ThrowIfNull(routes);

        foreach (var route in routes)
        {
            ArgumentNullException.ThrowIfNull(route);
            await example.CreateRouteAsync(route);
        }
    }

    /// <summary>
    /// Bulk updates existing routes with new configurations.
    /// </summary>
    /// <param name="example">The <see cref="DynamicRouteConfigurationExample"/> instance</param>
    /// <param name="updates">Collection of route updates</param>
    /// <exception cref="ArgumentNullException"><paramref name="updates"/> is <see langword="null"/></exception>
    public static async Task UpdateMultipleRoutesAsync(
        this DynamicRouteConfigurationExample example,
        IEnumerable<object> updates)
    {
        ArgumentNullException.ThrowIfNull(example);
        ArgumentNullException.ThrowIfNull(updates);

        foreach (var update in updates)
        {
            ArgumentNullException.ThrowIfNull(update);

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
    /// <param name="example">The <see cref="DynamicRouteConfigurationExample"/> instance</param>
    /// <param name="routeIds">Collection of route IDs to delete</param>
    /// <exception cref="ArgumentNullException"><paramref name="routeIds"/> is <see langword="null"/></exception>
    public static async Task DeleteMultipleRoutesAsync(
        this DynamicRouteConfigurationExample example,
        IEnumerable<int> routeIds)
    {
        ArgumentNullException.ThrowIfNull(example);
        ArgumentNullException.ThrowIfNull(routeIds);

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
    /// <param name="example">The <see cref="DynamicRouteConfigurationExample"/> instance</param>
    /// <param name="durationMinutes">Duration in minutes to analyze</param>
    /// <returns>Dictionary mapping route IDs to performance metrics</returns>
    /// <exception cref="ArgumentNullException"><paramref name="example"/> is <see langword="null"/></exception>
    public static async Task<Dictionary<int, RoutePerformanceMetrics>> AnalyzeRoutePerformanceAsync(
        this DynamicRouteConfigurationExample example,
        int durationMinutes = 5)
    {
        ArgumentNullException.ThrowIfNull(example);

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
    /// <param name="example">The <see cref="DynamicRouteConfigurationExample"/> instance</param>
    /// <returns>Gateway URL string</returns>
    /// <exception cref="ArgumentNullException"><paramref name="example"/> is <see langword="null"/></exception>
    public static string GetGatewayUrl(this DynamicRouteConfigurationExample example)
    {
        ArgumentNullException.ThrowIfNull(example);
        return example._gatewayUrl;
    }

    /// <summary>
    /// Creates a route with additional metadata for better tracking.
    /// </summary>
    /// <param name="example">The <see cref="DynamicRouteConfigurationExample"/> instance</param>
    /// <param name="pattern">Route pattern</param>
    /// <param name="targetServiceId">Target service ID</param>
    /// <param name="priority">Route priority</param>
    /// <param name="rateLimitPerMinute">Rate limit per minute</param>
    /// <param name="enableCaching">Whether caching is enabled</param>
    /// <param name="description">Route description</param>
    /// <param name="tags">Optional tags for categorization</param>
    /// <returns>Task representing the asynchronous operation</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="pattern"/> is <see langword="null"/> or
    /// <paramref name="example"/> is <see langword="null"/>
    /// </exception>
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
        ArgumentNullException.ThrowIfNull(example);
        ArgumentException.ThrowIfNullOrEmpty(pattern, nameof(pattern));

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
public sealed class RoutePerformanceMetrics
{
    /// <summary>
    /// Gets or sets the total number of requests processed by the route.
    /// </summary>
    public int RequestCount { get; set; }

    /// <summary>
    /// Gets or sets the average latency in milliseconds.
    /// </summary>
    public double AverageLatencyMs { get; set; }

    /// <summary>
    /// Gets or sets the error rate as a percentage (0.0 to 1.0).
    /// </summary>
    public double ErrorRate { get; set; }

    /// <summary>
    /// Gets or sets the cache hit rate as a percentage (0.0 to 1.0).
    /// </summary>
    public double CacheHitRate { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of concurrent requests observed.
    /// </summary>
    public int MaxConcurrentRequests { get; set; }
}