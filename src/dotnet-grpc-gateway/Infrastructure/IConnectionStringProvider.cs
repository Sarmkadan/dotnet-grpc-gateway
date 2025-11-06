// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Infrastructure;

/// <summary>
/// Provides database connection strings to data access layer
/// </summary>
public interface IConnectionStringProvider
{
    string GetConnectionString();
    void SetConnectionString(string connectionString);
}

public class ConnectionStringProvider : IConnectionStringProvider
{
    private string _connectionString;

    public ConnectionStringProvider(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public string GetConnectionString() => _connectionString;

    public void SetConnectionString(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }
}
