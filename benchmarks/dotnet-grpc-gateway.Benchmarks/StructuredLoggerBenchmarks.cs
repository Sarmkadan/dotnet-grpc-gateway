using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using DotNetGrpcGateway.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace DotNetGrpcGateway.Benchmarks;

[MemoryDiagnoser]
public class StructuredLoggerBenchmarks
{
    private ILogger _logger = null!;

    [Params(10, 100, 1000)]
    public int MessageCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        // Create a fake logger that does nothing
        _logger = new FakeLogger();
    }

    [Benchmark]
    public void LogRequestStart()
    {
        for (int i = 0; i < MessageCount; i++)
        {
            StructuredLogger.LogRequestStart(
                _logger,
                $"request-{i}",
                "/api/test",
                "GET",
                "192.168.1.1");
        }
    }

    [Benchmark]
    public void LogRequestComplete()
    {
        for (int i = 0; i < MessageCount; i++)
        {
            StructuredLogger.LogRequestComplete(
                _logger,
                $"request-{i}",
                "/api/test",
                200,
                150L);
        }
    }

    [Benchmark]
    public void LogServiceDiscovery()
    {
        for (int i = 0; i < MessageCount; i++)
        {
            StructuredLogger.LogServiceDiscovery(
                _logger,
                i,
                $"service-{i}",
                i % 2 == 0);
        }
    }

    [Benchmark]
    public void LogCacheOperation()
    {
        for (int i = 0; i < MessageCount; i++)
        {
            StructuredLogger.LogCacheOperation(
                _logger,
                "GET",
                $"key-{i}",
                i % 3 == 0);
        }
    }

    [Benchmark]
    public void LogAuthentication()
    {
        for (int i = 0; i < MessageCount; i++)
        {
            StructuredLogger.LogAuthentication(
                _logger,
                $"user-{i}",
                i % 2 == 0,
                i % 5 == 0 ? "invalid credentials" : null);
        }
    }

    // Simple fake logger implementation
    private class FakeLogger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullDisposable.Instance;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }

        private class NullDisposable : IDisposable
        {
            public static readonly NullDisposable Instance = new NullDisposable();
            public void Dispose() { }
        }
    }
}