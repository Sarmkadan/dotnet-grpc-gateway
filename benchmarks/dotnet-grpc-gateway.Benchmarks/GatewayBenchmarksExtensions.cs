using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using dotnet_grpc_gateway.Benchmarks;

namespace dotnet_grpc_gateway.Benchmarks
{
    public static class GatewayBenchmarksExtensions
    {
        [Benchmark]
        public static void FindMatchingRoute_WithNoParams(this GatewayBenchmarks benchmarks)
        {
            benchmarks.Setup();
            benchmarks.FindMatchingRoute_ExactMatch();
        }

        [Benchmark]
        public static async Task GetFromCache_WithLargeData(this GatewayBenchmarks benchmarks)
        {
            benchmarks.Setup();
            await benchmarks.GetFromCache_Miss();
            await benchmarks.SetInCache();
            await benchmarks.GetFromCache_Hit();
        }

        [Benchmark]
        public static void GetNextEndpoint_WithEmptyList(this GatewayBenchmarks benchmarks)
        {
            benchmarks.Setup();
            benchmarks.GetNextEndpoint();
        }

        [Benchmark]
        public static async Task RegisterService_WithValidData(this GatewayBenchmarks benchmarks)
        {
            benchmarks.Setup();
            await benchmarks.RegisterService();
        }
    }
}
