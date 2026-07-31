#nullable enable
using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using DotNetGrpcGateway.Exceptions;

namespace DotNetGrpcGateway.Benchmarks;

/// <summary>
/// Benchmarks for the GatewayException hierarchy.
/// </summary>
[MemoryDiagnoser]
public class GatewayExceptionBenchmarks
{
    // Test data used across benchmarks
    private string _message = "Test exception message";
    private string _errorCode = "TEST_ERROR";
    private int _httpStatusCode = 400;
    private string _serviceName = "TestService";
    private string _reason = "Test reason";
    private string _pattern = "/test/pattern";

    // Parameter for number of details to add
    [Params(10, 100, 1000)]
    public int DetailCount;

    // Holds a pre‑populated dictionary for the AddDetailWithExistingDictionary benchmark
    private Dictionary<string, object> _prePopulatedDetails;

    [GlobalSetup]
    public void GlobalSetup()
    {
        // Prepare a dictionary with DetailCount entries
        _prePopulatedDetails = new Dictionary<string, object>(DetailCount);
        for (int i = 0; i < DetailCount; i++)
        {
            _prePopulatedDetails[$"key{i}"] = $"value{i}";
        }
    }

    /// <summary>
    /// Benchmark constructing a base GatewayException.
    /// </summary>
    [Benchmark]
    public GatewayException ConstructGatewayException()
    {
        return new GatewayException(_message, _errorCode, _httpStatusCode);
    }

    /// <summary>
    /// Benchmark constructing a ServiceNotFoundException.
    /// </summary>
    [Benchmark]
    public ServiceNotFoundException ConstructServiceNotFoundException()
    {
        return new ServiceNotFoundException(_serviceName);
    }

    /// <summary>
    /// Benchmark adding a single detail to a new exception.
    /// </summary>
    [Benchmark]
    public void AddSingleDetail()
    {
        var ex = new GatewayException(_message);
        ex.AddDetail("single_key", "single_value");
    }

    /// <summary>
    /// Benchmark adding multiple details to a new exception.
    /// </summary>
    [Benchmark]
    public void AddMultipleDetails()
    {
        var ex = new GatewayException(_message);
        for (int i = 0; i < DetailCount; i++)
        {
            ex.AddDetail($"key{i}", $"value{i}");
        }
    }

    /// <summary>
    /// Benchmark adding details to an exception that already has a pre‑populated dictionary.
    /// </summary>
    [Benchmark]
    public void AddDetailWithExistingDictionary()
    {
        var ex = new GatewayException(_message);
        // Pre‑populate the dictionary
        foreach (var kvp in _prePopulatedDetails)
        {
            ex.AddDetail(kvp.Key, kvp.Value);
        }

        // Add one more detail to test the growth path
        ex.AddDetail("extra_key", "extra_value");
    }
}
