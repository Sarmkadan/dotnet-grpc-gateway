using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using DotNetGrpcGateway.Domain;
using System.Linq;

namespace DotNetGrpcGateway.Benchmarks;

[MemoryDiagnoser]
public class AuthenticationTokenBenchmarks
{
    private AuthenticationToken _token = null!;
    private int _serviceIdToCheck;
    private string _ipToCheck = null!;

    [Params(10, 100, 1000)]
    public int ListSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _token = new AuthenticationToken
        {
            TokenHash = "test_hash",
            ClientName = "Test Client",
            ClientId = "test_client_id",
            AllowAllServices = false,
            AllowedServiceIds = Enumerable.Range(0, ListSize).ToList(),
            IpWhitelistCsv = string.Join(",", Enumerable.Range(0, ListSize).Select(i => $"192.168.1.{i}")),
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        // Pick an ID that exists in the list (middle)
        _serviceIdToCheck = ListSize / 2;
        // Pick an IP that exists in the list (middle)
        _ipToCheck = $"192.168.1.{ListSize / 2}";
    }

    [Benchmark]
    public void Validate()
    {
        _token.Validate();
    }

    [Benchmark]
    public bool CanAccessService()
    {
        return _token.CanAccessService(_serviceIdToCheck);
    }

    [Benchmark]
    public bool IsIpAllowed()
    {
        return _token.IsIpAllowed(_ipToCheck);
    }
}
