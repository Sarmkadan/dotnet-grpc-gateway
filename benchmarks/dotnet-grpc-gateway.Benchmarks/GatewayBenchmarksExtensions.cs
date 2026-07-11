using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using System;
using System.Threading.Tasks;

namespace dotnet_grpc_gateway.Benchmarks
{
    /// <summary>
    /// Provides extension methods for <see cref="GatewayBenchmarks"/> that encapsulate common benchmark scenarios.
    /// These methods simplify benchmark execution by combining setup and execution steps.
    /// </summary>
    public static class GatewayBenchmarksExtensions
    {
        /// <summary>
        /// Executes a benchmark scenario that tests exact route matching with no parameters.
        /// </summary>
        /// <param name="benchmarks">The benchmark instance to execute against. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="benchmarks"/> is null.</exception>
        [Benchmark]
        public static void FindMatchingRoute_WithNoParams(this GatewayBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            benchmarks.Setup();
            benchmarks.FindMatchingRoute_ExactMatch();
        }

        /// <summary>
        /// Executes a benchmark scenario that tests cache operations with large data sets.
        /// This includes a cache miss, followed by setting data in cache, then a cache hit.
        /// </summary>
        /// <param name="benchmarks">The benchmark instance to execute against. Cannot be null.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="benchmarks"/> is null.</exception>
        [Benchmark]
        public static async Task GetFromCache_WithLargeData(this GatewayBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            benchmarks.Setup();
            await benchmarks.GetFromCache_Miss();
            await benchmarks.SetInCache();
            await benchmarks.GetFromCache_Hit();
        }

        /// <summary>
        /// Executes a benchmark scenario that tests load balancer behavior with an empty endpoint list.
        /// </summary>
        /// <param name="benchmarks">The benchmark instance to execute against. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="benchmarks"/> is null.</exception>
        [Benchmark]
        public static void GetNextEndpoint_WithEmptyList(this GatewayBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            benchmarks.Setup();
            benchmarks.GetNextEndpoint();
        }

        /// <summary>
        /// Executes a benchmark scenario that tests service registration with valid data.
        /// </summary>
        /// <param name="benchmarks">The benchmark instance to execute against. Cannot be null.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="benchmarks"/> is null.</exception>
        [Benchmark]
        public static async Task RegisterService_WithValidData(this GatewayBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            benchmarks.Setup();
            await benchmarks.RegisterService();
        }
    }
}