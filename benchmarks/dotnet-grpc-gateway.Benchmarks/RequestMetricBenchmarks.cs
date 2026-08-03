using BenchmarkDotNet.Attributes;
using DotNetGrpcGateway.Domain;

namespace DotNetGrpcGateway.Benchmarks;

[MemoryDiagnoser]
public class RequestMetricBenchmarks
{
    private RequestMetric _metric = null!;

    [GlobalSetup]
    public void Setup()
    {
        _metric = new RequestMetric
        {
            ServiceName = "TestService",
            MethodName = "TestMethod",
            ClientIpAddress = "127.0.0.1",
            DurationMs = 100.0
        };
    }

    [Benchmark]
    public void Validate_Success()
    {
        _metric.Validate();
    }

    [Benchmark]
    public void Validate_Failure_ServiceName()
    {
        _metric.ServiceName = string.Empty;
        _metric.Validate(); // Expected to throw InvalidOperationException
    }

    [Benchmark]
    public bool IsSlowRequest_True()
    {
        return _metric.IsSlowRequest(50.0); // DurationMs=100 > 50 -> true
    }

    [Benchmark]
    public bool IsSlowRequest_False()
    {
        return _metric.IsSlowRequest(200.0); // DurationMs=100 < 200 -> false
    }

    [Benchmark]
    public void RecordError()
    {
        _metric.RecordError("Test error", "Stack trace");
    }
}