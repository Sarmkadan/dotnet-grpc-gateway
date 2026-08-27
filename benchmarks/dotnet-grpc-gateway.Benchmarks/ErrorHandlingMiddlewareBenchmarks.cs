using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using DotNetGrpcGateway.Exceptions;
using DotNetGrpcGateway.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DotNetGrpcGateway.Benchmarks;

[MemoryDiagnoser]
public class ErrorHandlingMiddlewareBenchmarks
{
    private ErrorHandlingMiddleware _middleware = null!;
    private DefaultHttpContext _httpContext = null!;
    private Func<Task> _nextDelegate = null!;
    private ILogger<ErrorHandlingMiddleware> _logger = null!;

    [Params(10, 100, 1000)]
    public int ExceptionMessageSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        // Create a fake logger that does nothing
        _logger = new FakeLogger<ErrorHandlingMiddleware>();

        // Create the middleware
        _middleware = new ErrorHandlingMiddleware(NextDelegate, _logger);

        // Create HTTP context
        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();

        // Create a simple next delegate that does nothing
        _nextDelegate = () => Task.CompletedTask;
    }

    [Benchmark]
    public async Task InvokeAsync_NoException()
    {
        // Reset response body
        _httpContext.Response.Body = new MemoryStream();
        await _middleware.InvokeAsync(_httpContext);
    }

    [Benchmark]
    public async Task InvokeAsync_ArgumentException()
    {
        // Reset response body
        _httpContext.Response.Body = new MemoryStream();

        // Set up next delegate to throw ArgumentException
        _nextDelegate = () =>
        {
            throw new ArgumentException(new string('x', ExceptionMessageSize));
        };

        // Recreate middleware with new delegate
        _middleware = new ErrorHandlingMiddleware(NextDelegate, _logger);

        await _middleware.InvokeAsync(_httpContext);
    }

    [Benchmark]
    public async Task InvokeAsync_GatewayException()
    {
        // Reset response body
        _httpContext.Response.Body = new MemoryStream();

        // Set up next delegate to throw GatewayException
        _nextDelegate = () =>
        {
            var ex = new GatewayException(
                new string('x', ExceptionMessageSize),
                "TEST_ERROR",
                400);
            ex.AddDetail("test_key", "test_value");
            throw ex;
        };

        // Recreate middleware with new delegate
        _middleware = new ErrorHandlingMiddleware(NextDelegate, _logger);

        await _middleware.InvokeAsync(_httpContext);
    }

    private Task NextDelegate(HttpContext context)
    {
        return _nextDelegate();
    }

    // Simple fake logger implementation
    private class FakeLogger<T> : ILogger<T>
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