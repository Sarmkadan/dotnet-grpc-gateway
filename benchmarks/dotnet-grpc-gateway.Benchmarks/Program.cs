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
    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<GatewayBenchmarks>();
        }
    }

    [MemoryDiagnoser]
    [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class GatewayBenchmarks
    {
        private IServiceProvider _serviceProvider;
        private IRouteManagementService _routeManagementService;
        private IPerformanceMonitor _performanceMonitor;
        private IRouteResolutionService _routeResolutionService;
        private ILoadBalancerService _loadBalancerService;
        private ICacheService _cacheService;

        [GlobalSetup]
        public void Setup()
        {
            // Create service collection
            var services = new ServiceCollection();

            // Add logging
            services.AddLogging(configure => configure.AddConsole());

            // Add gateway services
            services.AddGatewayServices();
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, MemoryCacheService>();
            services.AddSingleton<IPerformanceMonitor, PerformanceMonitor>();
            services.AddScoped<IRouteManagementService, RouteManagementService>();
            services.AddScoped<IRouteResolutionService, RouteResolutionService>();
            services.AddSingleton<ILoadBalancerService, LoadBalancerService>();
            services.AddSingleton<IServiceDiscoveryService, ServiceDiscoveryService>();
            services.AddSingleton<IGrpcClientFactory, GrpcClientFactory>();

            _serviceProvider = services.BuildServiceProvider();
            _routeManagementService = _serviceProvider.GetRequiredService<IRouteManagementService>();
            _performanceMonitor = _serviceProvider.GetRequiredService<IPerformanceMonitor>();
            _routeResolutionService = _serviceProvider.GetRequiredService<IRouteResolutionService>();
            _loadBalancerService = _serviceProvider.GetRequiredService<ILoadBalancerService>();
            _cacheService = _serviceProvider.GetRequiredService<ICacheService>();
            _grpcClientFactory = _serviceProvider.GetRequiredService<IGrpcClientFactory>();
            _serviceDiscoveryService = _serviceProvider.GetRequiredService<IServiceDiscoveryService>();

            // Setup test data
            SetupTestRoutes();
            SetupTestServices();
            SetupTestEndpoints();
        }

        private void SetupTestRoutes()
        {
            // Add some test routes for benchmarking
            for (int i = 0; i < 10; i++)
            {
                var route = new GatewayRoute
                {
                    Id = i + 1,
                    Pattern = $"test.Route{i}.GetData",
                    TargetServiceId = 1,
                    Priority = 100,
                    MatchType = RouteMatchType.ExactMatch,
                    RateLimitPerMinute = 1000,
                    EnableCaching = false,
                    CacheDurationSeconds = 300,
                    IsActive = true
                };

                _routeManagementService.AddRoute(route);
            }
        }

        private void SetupTestServices()
        {
            // Add test services
            for (int i = 0; i < 5; i++)
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

                _routeManagementService.AddService(service);
            }
        }

        private void SetupTestEndpoints()
        {
            // Register endpoints for load balancing
            for (int i = 0; i < 3; i++)
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
        public void FindMatchingRoute_ExactMatch()
        {
            var result = _routeManagementService.FindMatchingRouteAsync("test.Route0.GetData");
            if (result.Result == null)
            {
                throw new InvalidOperationException("Route resolution failed");
            }
        }

        [Benchmark]
        [BenchmarkCategory("RouteResolution")]
        public void FindMatchingRoute_WildcardMatch()
        {
            var result = _routeManagementService.FindMatchingRouteAsync("test.TestService1.GetData");
            if (result.Result == null)
            {
                throw new InvalidOperationException("Route resolution failed");
            }
        }

        [Benchmark]
        [BenchmarkCategory("RouteResolution")]
        public void FindMatchingRoute_NoMatch()
        {
            var result = _routeManagementService.FindMatchingRouteAsync("nonexistent.path");
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
        [BenchmarkCategory("Caching")]
        public async Task GetFromCache_Miss()
        {
            var result = await _cacheService.GetAsync<string>("nonexistent_key");
        }

        [Benchmark]
        [BenchmarkCategory("Caching")]
        public async Task SetInCache()
        {
            await _cacheService.SetAsync("test_key", "test_value", TimeSpan.FromMinutes(5));
        }

        [Benchmark]
        [BenchmarkCategory("Caching")]
        public async Task RemoveFromCache()
        {
            await _cacheService.RemoveAsync("test_key");
        }

        [Benchmark]
        [BenchmarkCategory("PerformanceMonitor")]
        public void RecordRequestDuration()
        {
            _performanceMonitor.RecordRequestDuration("/test.TestService1.GetData", 15);
        }

        [Benchmark]
        [BenchmarkCategory("RouteManagement")]
        public void AddRoute()
        {
            var route = new GatewayRoute
            {
                Id = 999,
                Pattern = "benchmark.Route.GetData",
                TargetServiceId = 1,
                Priority = 100,
                MatchType = RouteMatchType.ExactMatch,
                RateLimitPerMinute = 1000,
                EnableCaching = false,
                CacheDurationSeconds = 300,
                IsActive = true
            };

            _routeManagementService.AddRoute(route);
        }

        [Benchmark]
        [BenchmarkCategory("RouteManagement")]
        public void RemoveRoute()
        {
            _routeManagementService.RemoveRoute(999);
        }

        [Benchmark]
        [BenchmarkCategory("RouteManagement")]
        public async Task GetAllRoutes()
        {
            var routes = await _routeManagementService.GetAllRoutesAsync();
        }

        [Benchmark]
        [BenchmarkCategory("RouteManagement")]
        public void AddService()
        {
            var service = new GrpcService
            {
                Id = 999,
                Name = "BenchmarkService",
                ServiceFullName = "benchmark.BenchmarkService",
                Host = "localhost",
                Port = 9999,
                UseTls = false,
                HealthCheckIntervalSeconds = 30,
                IsHealthy = true,
                IsActive = true
            };

            _routeManagementService.AddService(service);
        }

        [Benchmark]
        [BenchmarkCategory("RouteManagement")]
        public async Task GetRoutesByService()
        {
            var routes = await _routeManagementService.GetRoutesByServiceAsync(1);
        }

        [Benchmark]
        [BenchmarkCategory("ServiceDiscovery")]
        public void DiscoverServices()
        {
            _serviceDiscoveryService.DiscoverServices();
        }

        [Benchmark]
        [BenchmarkCategory("ServiceDiscovery")]
        public void GetHealthyServices()
        {
            var healthy = _serviceDiscoveryService.GetHealthyServices();
        }

        [Benchmark]
        [BenchmarkCategory("GrpcClient")]
        public void CreateGrpcClient()
        {
            var client = _grpcClientFactory.CreateClient("test.TestService1");
        }

        [Benchmark]
        [BenchmarkCategory("GrpcClient")]
        public async Task GetGrpcClient()
        {
            var client = await _grpcClientFactory.GetClientAsync("test.TestService1");
        }

    }
}
