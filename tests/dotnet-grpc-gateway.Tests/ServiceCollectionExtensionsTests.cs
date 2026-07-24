using DotNetGrpcGateway.Configuration;
using DotNetGrpcGateway.Infrastructure;
using DotNetGrpcGateway.Options;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddGatewayServices_RegistersAllExpectedServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddGatewayServices();

        // Assert
        // Check that the expected services are registered as singletons
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(RetryPolicyOptions) &&
            descriptor.Lifetime == ServiceLifetime.Singleton);

        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(ITransientExceptionClassifier) &&
            descriptor.Lifetime == ServiceLifetime.Singleton);

        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(IRetryPolicy) &&
            descriptor.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddGatewayServices_ThrowsArgumentNullException_WhenServicesIsNull()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddGatewayServices());
    }
}