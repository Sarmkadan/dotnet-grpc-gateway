using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using DotNetGrpcGateway.Caching;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Infrastructure;
using DotNetGrpcGateway.Services;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace dotnet_grpc_gateway.Benchmarks
{
    [MemoryDiagnoser]
    [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]

    /// <summary>
    /// Benchmark suite for testing the performance and throughput of the gRPC gateway services.
    /// Measures various operations including route resolution, load balancing, caching, service management,
    /// and performance monitoring to identify performance bottlenecks and optimize the gateway implementation.
    /// </summary>
    public class GatewayBenchmarks
    {
        /// <summary>
        /// Service provider for resolving gateway dependencies during benchmark execution.
        /// </summary>
        private IServiceProvider _serviceProvider;
        /// <summary>
        /// Gateway service for managing and routing gRPC requests to appropriate services.
        /// </summary>
        private IGatewayService _gatewayService;
        /// <summary>
        /// Service for managing routes including registration, retrieval, and health status updates.
        /// </summary>
        private IRouteManagementService _routeManagementService;
        /// <summary>
        /// Service for monitoring and recording performance metrics including request durations and throughput.
        /// </summary>
        private IPerformanceMonitor _performanceMonitor;
        /// <summary>
        /// Service for resolving routes by matching service names and method paths to registered routes.
        /// </summary>
        private IRouteResolutionService _routeResolutionService;
        /// <summary>
        /// Service for distributing requests across multiple endpoints using round-robin and weighted algorithms.
        /// </summary>
        private ILoadBalancerService _loadBalancerService;
        /// <summary>
        /// Service for caching frequently accessed data and reducing backend service load.
        /// </summary>
        private ICacheService _cacheService;
        /// <summary>
        /// Factory for creating gRPC HTTP clients to communicate with backend services.
        /// </summary>
        private IGrpcClientFactory _grpcClientFactory;
        /// <summary>
        /// Service for discovering and monitoring the health status of registered backend services.
        /// </summary>
        private IServiceDiscoveryService _serviceDiscoveryService;
        /// <summary>
        /// Unit of work for managing database transactions and coordinating multiple operations.
        /// </summary>
        private IUnitOfWork _unitOfWork;

        /// <summary>
        /// Performs global setup for benchmarks, configuring dependency injection and initializing test data.
        /// </summary>
        [GlobalSetup]
        public void Setup()
        {
            // Create service collection
            var services = new ServiceCollection();

            // Add logging
            services.AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Warning));

            // Add gateway services
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, MemoryCacheService>();
            services.AddSingleton<IPerformanceMonitor, PerformanceMonitor>();
            services.AddScoped<IRouteManagementService, RouteManagementService>();
            services.AddScoped<IRouteResolutionService, RouteResolutionService>();
            services.AddSingleton<ILoadBalancerService, LoadBalancerService>();
            services.AddSingleton<IServiceDiscoveryService, ServiceDiscoveryService>();
            services.AddHttpClient<IGrpcClientFactory, GrpcClientFactory>();
            services.AddScoped<IGatewayService, GatewayService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            _serviceProvider = services.BuildServiceProvider();
            _gatewayService = _serviceProvider.GetRequiredService<IGatewayService>();
            _routeManagementService = _serviceProvider.GetRequiredService<IRouteManagementService>();
            _performanceMonitor = _serviceProvider.GetRequiredService<IPerformanceMonitor>();
            _routeResolutionService = _serviceProvider.GetRequiredService<IRouteResolutionService>();
            _loadBalancerService = _serviceProvider.GetRequiredService<ILoadBalancerService>();
            _cacheService = _serviceProvider.GetRequiredService<ICacheService>();
            _grpcClientFactory = _serviceProvider.GetRequiredService<IGrpcClientFactory>();
            _serviceDiscoveryService = _serviceProvider.GetRequiredService<IServiceDiscoveryService>();
            _unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();

            // Setup test data
            SetupTestRoutes();
            SetupTestServices();
            SetupTestEndpoints();
        }

        /// <summary>
        /// Initializes test routes in the gateway for benchmarking purposes.
        /// Ensures routes are available for resolution tests.
        /// </summary>
        private void SetupTestRoutes()
        {
            // Routes are already set up via the unit of work in the actual implementation
            // We just need to ensure there are routes available for benchmarking
        }

        /// <summary>
        /// Creates test services for benchmarking load balancing and service discovery.
        /// Registers 20 test services with varying ports and health statuses.
        /// </summary>
        private void SetupTestServices()
        {
            // Add test services
            for (int i = 0; i < 20; i++)
            {
                var service = new GrpcService
                {
                    Id = i + 1,
                    Name = $"TestService{i + 1}",
                    ServiceFullName = $"test.TestService{i + 1}",
                    Host = "localhost",
                    Port = 5001 + i,
                    UseTls = false,
                    HealthCheckIntervalSeconds = 30,
                    IsHealthy = true,
                    IsActive = true
                };

                _gatewayService.RegisterServiceAsync(service).Wait();
            }
        }

        /// <summary>
        /// Registers test endpoints for load balancing benchmarking.
        /// Creates 10 endpoints for service ID 1 to test round-robin and weighted algorithms.
        /// </summary>
        private void SetupTestEndpoints()
        {
            // Register endpoints for load balancing
            for (int i = 0; i < 10; i++)
            {
                var endpoint = new ServiceEndpoint
                {
                    Id = i + 1,
                    ServiceId = 1,
                    Host = "localhost",
                    Port = 5001 + i,
                    UseTls = false,
                    IsHealthy = true,
                    Weight = 1
                };

                _loadBalancerService.RegisterEndpoint(endpoint);
            }
        }

        /// <summary>
        /// Benchmarks exact route matching where service name and method path match registered routes precisely.
        /// Measures the performance of finding routes with exact character-by-character matching.
        /// </summary>
        [Benchmark]
        [BenchmarkCategory("RouteResolution")]
        public void FindMatchingRoute_ExactMatch()
        {
            var result = _routeResolutionService.FindMatchingRouteAsync("test", "Route0.GetData");
            if (result.Result == null)
            {
                throw new InvalidOperationException("Route resolution failed");
            }
        }

        /// <summary>
        /// Benchmarks wildcard route matching where service names are matched using pattern matching.
        /// Tests the ability to resolve routes with partial or pattern-based service name matching.
        /// </summary>
        [Benchmark]
        [BenchmarkCategory("RouteResolution")]
        public void FindMatchingRoute_WildcardMatch()
        {
            var result = _routeResolutionService.FindMatchingRouteAsync("test", "TestService1.GetData");
            if (result.Result == null)
            {
                throw new InvalidOperationException("Route resolution failed");
            }
        }

        /// <summary>
        /// Benchmarks route resolution when no matching routes exist for the given service and method.
        /// Measures the performance of handling non-existent route requests and error cases.
        /// </summary>
        [Benchmark]
        [BenchmarkCategory("RouteResolution")]
        public void FindMatchingRoute_NoMatch()
        {
            var result = _routeResolutionService.FindMatchingRouteAsync("nonexistent", "path");
        }

        /// <summary>
        /// Benchmarks route resolution when multiple routes could match the request.
        /// Ensures the resolver correctly selects the appropriate route among many candidates.
        /// </summary>
        [Benchmark]
        [BenchmarkCategory("RouteResolution")]
        public void FindMatchingRoute_MultipleMatches()
        {
            var result = _routeResolutionService.FindMatchingRouteAsync("test", "Route45.GetData");
            if (result.Result == null)
            {
                throw new InvalidOperationException("Route resolution failed");
            }
        }

        /// <summary>
        /// Benchmarks the load balancer's ability to select the next endpoint for a given service.
        /// </summary>
        [Benchmark]
        [BenchmarkCategory("LoadBalancing")]
        public void GetNextEndpoint()
        {
            var target = _loadBalancerService.GetNextEndpoint(1);
            if (target == null)
            {
                throw new InvalidOperationException("Load balancer failed to select target");
            }
        }

        /// <summary>
        /// Benchmarks the registration of a new endpoint in the load balancer.
        /// </summary>
        /// <param name="endpoint">The endpoint to register.</param>
        [Benchmark]
        [BenchmarkCategory("LoadBalancing")]
        public void RegisterEndpoint()
        {
            var endpoint = new ServiceEndpoint
            {
                Id = 999,
                ServiceId = 1,
                Host = "localhost",
                Port = 9999,
                UseTls = false,
                IsHealthy = true,
                Weight = 1
            };

            _loadBalancerService.RegisterEndpoint(endpoint);
        }

        /// <summary>
        /// Benchmarks updating the health status of an existing endpoint.
        /// </summary>
        /// <param name="serviceId">The identifier of the service containing the endpoint.</param>
        /// <param name="endpointId">The identifier of the endpoint to update.</param>
        /// <param name="isHealthy">The new health status to set.</param>
        [Benchmark]
        [BenchmarkCategory("LoadBalancing")]
        public void UpdateEndpointHealth()
        {
            _loadBalancerService.UpdateEndpointHealth(1, 1, true);
        }

        /// <summary>
        /// Benchmarks a cache miss scenario where the requested key does not exist.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Benchmark]
        [BenchmarkCategory("Caching")]
        public async Task GetFromCache_Miss()
        {
            var result = await _cacheService.GetAsync<string>("nonexistent_key_" + Guid.NewGuid().ToString());
        }

        /// <summary>
        /// Benchmarks a cache hit scenario where the requested key exists.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Benchmark]
        [BenchmarkCategory("Caching")]
        public async Task GetFromCache_Hit()
        {
            var key = "test_key_" + Guid.NewGuid().ToString();
            await _cacheService.SetAsync(key, "test_value", TimeSpan.FromMinutes(5));
            var result = await _cacheService.GetAsync<string>(key);
        }

        /// <summary>
        /// Benchmarks setting a value in the cache.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Benchmark]
        [BenchmarkCategory("Caching")]
        public async Task SetInCache()
        {
            await _cacheService.SetAsync("test_key_" + Guid.NewGuid().ToString(), "test_value", TimeSpan.FromMinutes(5));
        }

        /// <summary>
        /// Benchmarks removing a value from the cache.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Benchmark]
        [BenchmarkCategory("Caching")]
        public async Task RemoveFromCache()
        {
            await _cacheService.RemoveAsync("test_key_" + Guid.NewGuid().ToString());
        }

        /// <summary>
        /// Benchmarks retrieving cache statistics.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Benchmark]
        [BenchmarkCategory("Caching")]
        public async Task CacheStatistics()
        {
            var stats = await _cacheService.GetStatisticsAsync();
        }

        /// <summary>
        /// Benchmarks recording a request duration in the performance monitor.
        /// </summary>
        [Benchmark]
        [BenchmarkCategory("PerformanceMonitor")]
        public void RecordRequestDuration()
        {
            _performanceMonitor.RecordRequestDuration("/test.TestService1.GetData", 15);
        }

        /// <summary>
        /// Benchmarks retrieving all routes for a specific service.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Benchmark]
        [BenchmarkCategory("RouteManagement")]
        public async Task GetAllRoutes()
        {
            var routes = await _routeManagementService.GetRoutesByServiceAsync(1);
        }

        /// <summary>
        /// Benchmarks registering a new gRPC service in the gateway.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Benchmark]
        [BenchmarkCategory("ServiceManagement")]
        public async Task RegisterService()
        {
            var service = new GrpcService
            {
                Id = 999,
                Name = "BenchmarkService_" + Guid.NewGuid().ToString(),
                ServiceFullName = "benchmark.BenchmarkService_" + Guid.NewGuid().ToString(),
                Host = "localhost",
                Port = 9999,
                UseTls = false,
                HealthCheckIntervalSeconds = 30,
                IsHealthy = true,
                IsActive = true
            };

            await _gatewayService.RegisterServiceAsync(service);
        }

        /// <summary>
        /// Benchmarks retrieving all registered services from the gateway.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Benchmark]
        [BenchmarkCategory("ServiceManagement")]
        public async Task GetAllServices()
        {
            var services = await _gatewayService.GetAllServicesAsync();
        }

        /// <summary>
        /// Benchmarks retrieving only the healthy services from the gateway.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Benchmark]
        [BenchmarkCategory("ServiceManagement")]
        public async Task GetHealthyServices()
        {
            var healthy = await _gatewayService.GetHealthyServicesAsync();
        }

        /// <summary>
        /// Benchmarks performing a health check for a specific service.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Benchmark]
        [BenchmarkCategory("ServiceDiscovery")]
        public async Task PerformHealthCheck()
        {
            await _serviceDiscoveryService.PerformHealthCheckAsync(1);
        }

        /// <summary>
        /// Benchmarks retrieving health information for all services.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Benchmark]
        [BenchmarkCategory("ServiceDiscovery")]
        public async Task GetAllServicesHealth()
        {
            var health = await _serviceDiscoveryService.GetAllServicesHealthAsync();
        }

        /// <summary>
        /// Benchmarks creating a gRPC HTTP client for a given service.
        /// </summary>
        [Benchmark]
        [BenchmarkCategory("GrpcClient")]
        public void CreateGrpcClient()
        {
            var client = _grpcClientFactory.CreateHttpClient(new GrpcService
            {
                Id = 1,
                Name = "TestClientService",
                ServiceFullName = "test.TestClientService",
                Host = "localhost",
                Port = 5001,
                UseTls = false
            });
        }

        /// <summary>
        /// Benchmarks bulk route resolution by performing many lookups in a loop.
        /// </summary>
        [Benchmark]
        [BenchmarkCategory("Throughput")]
        [IterationCount(5)]
        [WarmupCount(3)]
        public void BulkRouteResolution()
        {
            for (int i = 0; i < 1000; i++)
            {
                var result = _routeResolutionService.FindMatchingRouteAsync("test", "Route" + (i % 50) + ".GetData");
            }
        }

        /// <summary>
        /// Benchmarks bulk cache operations: setting many keys then retrieving them.
        /// </summary>
        [Benchmark]
        [BenchmarkCategory("Throughput")]
        [IterationCount(5)]
        [WarmupCount(3)]
        public async Task BulkCacheOperations()
        {
            for (int i = 0; i < 1000; i++)
            {
                await _cacheService.SetAsync("bulk_key_" + i, "value_" + i, TimeSpan.FromMinutes(5));
            }

            for (int i = 0; i < 1000; i++)
            {
                await _cacheService.GetAsync<string>("bulk_key_" + i);
            }
        }

        /// <summary>
        /// Benchmarks bulk load balancing by repeatedly selecting the next endpoint.
        /// </summary>
        [Benchmark]
        [BenchmarkCategory("Throughput")]
        [IterationCount(5)]
        [WarmupCount(3)]
        public void BulkLoadBalancing()
        {
            for (int i = 0; i < 10000; i++)
            {
                var endpoint = _loadBalancerService.GetNextEndpoint(1);
            }
        }

        /// <summary>
        /// Performs cleanup after benchmarks, disposing the service provider if necessary.
        /// </summary>
        [GlobalCleanup]
        public void Cleanup()
        {
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    /// <summary>
    /// Entry point for running the benchmark suite.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Executes the benchmark runner for <see cref="GatewayBenchmarks"/>.
        /// </summary>
        /// <param name="args">Command-line arguments (unused).</param>
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<GatewayBenchmarks>();
        }
    }
}
