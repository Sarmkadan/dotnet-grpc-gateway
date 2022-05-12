using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using System;
using System.Threading.Tasks;

namespace dotnet_grpc_gateway.Benchmarks
{
    /// <summary>
    /// Provides extension methods for <see cref="GatewayBenchmarks"/> that combine benchmark
    /// setup with the execution of specific benchmark scenarios. These helpers allow a
    /// concise way to run a scenario without manually invoking <c>Setup</c> each time.
    /// </summary>
    public static class GatewayBenchmarksExtensions
    {
        /// <summary>
        /// Executes a benchmark that measures the performance of finding an exact route match
        /// when no route parameters are supplied.
        /// </summary>
        /// <param name="benchmarks">
        /// The <see cref="GatewayBenchmarks"/> instance on which to run the scenario. Cannot be <c>null</c>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="benchmarks"/> is <c>null</c>.
        /// </exception>
        [Benchmark]
        public static void FindMatchingRoute_WithNoParams(this GatewayBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            benchmarks.Setup();
            benchmarks.FindMatchingRoute_ExactMatch();
        }

        /// <summary>
        /// Executes a benchmark that measures cache operations with a large data set.
        /// The sequence performed is: a cache miss, then storing the data in the cache,
        /// followed by a cache hit.
        /// </summary>
        /// <param name="benchmarks">
        /// The <see cref="GatewayBenchmarks"/> instance on which to run the scenario. Cannot be <c>null</c>.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that completes when the entire cache benchmark sequence has finished.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="benchmarks"/> is <c>null</c>.
        /// </exception>
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
        /// Executes a benchmark that measures load‑balancer behavior when the endpoint list is empty.
        /// </summary>
        /// <param name="benchmarks">
        /// The <see cref="GatewayBenchmarks"/> instance on which to run the scenario. Cannot be <c>null</c>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="benchmarks"/> is <c>null</c>.
        /// </exception>
        [Benchmark]
        public static void GetNextEndpoint_WithEmptyList(this GatewayBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            benchmarks.Setup();
            benchmarks.GetNextEndpoint();
        }

        /// <summary>
        /// Executes a benchmark that measures the performance of registering a service with valid data.
        /// </summary>
        /// <param name="benchmarks">
        /// The <see cref="GatewayBenchmarks"/> instance on which to run the scenario. Cannot be <c>null</c>.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that completes when the service registration benchmark has finished.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="benchmarks"/> is <c>null</c>.
        /// </exception>
        [Benchmark]
        public static async Task RegisterService_WithValidData(this GatewayBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            benchmarks.Setup();
            await benchmarks.RegisterService();
        }
    }
}
