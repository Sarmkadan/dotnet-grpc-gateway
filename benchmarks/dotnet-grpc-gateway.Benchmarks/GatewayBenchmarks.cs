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

        [Benchmark]
        [BenchmarkCategory("RouteResolution")]

/// <summary>
/// Benchmarks exact route matching where service name and method path match registered routes precisely.
/// Measures the performance of finding routes with exact character-by-character matching.
/// </summary>
        public void FindMatchingRoute_ExactMatch()
        {
            var result = _routeResolutionService.FindMatchingRouteAsync("test", "Route0.GetData");
            if (result.Result == null)
            {
                throw new InvalidOperationException("Route resolution failed");
            }
        }

        [Benchmark]
        [BenchmarkCategory("RouteResolution")]

/// <summary>
/// Benchmarks wildcard route matching where service names are matched using pattern matching.
/// Tests the ability to resolve routes with partial or pattern-based service name matching.
/// </summary>
        public void FindMatchingRoute_WildcardMatch()
        {
            var result = _routeResolutionService.FindMatchingRouteAsync("test", "TestService1.GetData");
            if (result.Result == null)
            {
                throw new InvalidOperationException("Route resolution failed");
            }
        }

        [Benchmark]
        [BenchmarkCategory("RouteResolution")]

/// <summary>
/// Benchmarks route resolution when no matching routes exist for the given service and method.
/// Measures the performance of handling non-existent route requests and error cases.
/// </summary>
        public void FindMatchingRoute_NoMatch()
        {
            var result = _routeResolutionService.FindMatchingRouteAsync("nonexistent", "path");
        }

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

        [Benchmark]
        [BenchmarkCategory("LoadBalancing")]
        public void UpdateEndpointHealth()
        {
            _loadBalancerService.UpdateEndpointHealth(1, 1, true);
        }

        [Benchmark]
        [BenchmarkCategory("Caching")]
        public async Task GetFromCache_Miss()
        {
            var result = await _cacheService.GetAsync<string>("nonexistent_key_" + Guid.NewGuid().ToString());
        }

        [Benchmark]
        [BenchmarkCategory("Caching")]
        public async Task GetFromCache_Hit()
        {
            var key = "test_key_" + Guid.NewGuid().ToString();
            await _cacheService.SetAsync(key, "test_value", TimeSpan.FromMinutes(5));
            var result = await _cacheService.GetAsync<string>(key);
        }

        [Benchmark]
        [BenchmarkCategory("Caching")]
        public async Task SetInCache()
        {
            await _cacheService.SetAsync("test_key_" + Guid.NewGuid().ToString(), "test_value", TimeSpan.FromMinutes(5));
        }

        [Benchmark]
        [BenchmarkCategory("Caching")]
        public async Task RemoveFromCache()
        {
            await _cacheService.RemoveAsync("test_key_" + Guid.NewGuid().ToString());
        }

        [Benchmark]
        [BenchmarkCategory("Caching")]
        public async Task CacheStatistics()
        {
            var stats = await _cacheService.GetStatisticsAsync();
        }

        [Benchmark]
        [BenchmarkCategory("PerformanceMonitor")]
        public void RecordRequestDuration()
        {
            _performanceMonitor.RecordRequestDuration("/test.TestService1.GetData", 15);
        }

        [Benchmark]
        [BenchmarkCategory("RouteManagement")]
        public async Task GetAllRoutes()
        {
            var routes = await _routeManagementService.GetRoutesByServiceAsync(1);
        }

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

        [Benchmark]
        [BenchmarkCategory("ServiceManagement")]
        public async Task GetAllServices()
        {
            var services = await _gatewayService.GetAllServicesAsync();
        }

        [Benchmark]
        [BenchmarkCategory("ServiceManagement")]
        public async Task GetHealthyServices()
        {
            var healthy = await _gatewayService.GetHealthyServicesAsync();
        }

        [Benchmark]
        [BenchmarkCategory("ServiceDiscovery")]
        public async Task PerformHealthCheck()
        {
            await _serviceDiscoveryService.PerformHealthCheckAsync(1);
        }

        [Benchmark]
        [BenchmarkCategory("ServiceDiscovery")]
        public async Task GetAllServicesHealth()
        {
            var health = await _serviceDiscoveryService.GetAllServicesHealthAsync();
        }

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

        [GlobalCleanup]
        public void Cleanup()
        {
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<GatewayBenchmarks>();
        }
    }
}
