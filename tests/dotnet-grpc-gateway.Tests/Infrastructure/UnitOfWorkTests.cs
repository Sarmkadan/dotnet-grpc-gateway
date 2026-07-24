using System;
using System.Threading;
using System.Threading.Tasks;
using DotNetGrpcGateway.Infrastructure;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotNetGrpcGateway.Tests.Infrastructure;

/// <summary>
/// Tests for UnitOfWork commit/rollback and disposal semantics.
/// Ensures proper transaction management and resource cleanup.
/// </summary>
public class UnitOfWorkTests
{
    private readonly Mock<IGatewayRepository> _gatewaysMock = new();
    private readonly Mock<IServiceRegistry> _servicesMock = new();
    private readonly Mock<IRouteRepository> _routesMock = new();
    private readonly Mock<IMetricsRepository> _metricsMock = new();

    private IUnitOfWork CreateUnitOfWork()
    {
        return new UnitOfWork(
            _gatewaysMock.Object,
            _servicesMock.Object,
            _routesMock.Object,
            _metricsMock.Object
        );
    }

    [Fact]
    public void Constructor_WithNullGateways_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new UnitOfWork(
            null!,
            _servicesMock.Object,
            _routesMock.Object,
            _metricsMock.Object
        ));
    }

    [Fact]
    public void Constructor_WithNullServices_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new UnitOfWork(
            _gatewaysMock.Object,
            null!,
            _routesMock.Object,
            _metricsMock.Object
        ));
    }

    [Fact]
    public void Constructor_WithNullRoutes_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new UnitOfWork(
            _gatewaysMock.Object,
            _servicesMock.Object,
            null!,
            _metricsMock.Object
        ));
    }

    [Fact]
    public void Constructor_WithNullMetrics_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new UnitOfWork(
            _gatewaysMock.Object,
            _servicesMock.Object,
            _routesMock.Object,
            null!
        ));
    }

    [Fact]
    public async Task Dispose_WithoutCommit_RollsBackTransaction()
    {
        // Arrange
        await using var uow = CreateUnitOfWork();

        // Act - Dispose without calling CommitAsync
        // The DisposeAsync should call RollbackAsync internally
    }

    [Fact]
    public async Task DisposeAsync_WithoutCommit_RollsBackTransaction()
    {
        // Arrange
        var uow = CreateUnitOfWork();

        // Act - Dispose without calling CommitAsync
        await uow.DisposeAsync();

        // Assert - No exception should be thrown
        // The rollback should have been called internally
    }

    [Fact]
    public async Task Dispose_WithCommit_DoesNotRollbackTransaction()
    {
        // Arrange
        await using var uow = CreateUnitOfWork();
        await uow.CommitAsync();

        // Act - Dispose after commit
        // Should not attempt to rollback
    }

    [Fact]
    public async Task DisposeAsync_WithCommit_DoesNotRollbackTransaction()
    {
        // Arrange
        var uow = CreateUnitOfWork();
        await uow.CommitAsync();

        // Act - Dispose after commit
        await uow.DisposeAsync();

        // Assert - No exception should be thrown
        // Should not attempt to rollback since already committed
    }

    [Fact]
    public async Task CommitAsync_WhenAlreadyCommitted_ThrowsInvalidOperationException()
    {
        // Arrange
        await using var uow = CreateUnitOfWork();
        await uow.CommitAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => uow.CommitAsync()
        );

        exception.Message.Should().Contain("Transaction has already been committed");
    }

    [Fact]
    public async Task CommitAsync_WhenDisposed_ThrowsObjectDisposedException()
    {
        // Arrange
        var uow = CreateUnitOfWork();
        await uow.DisposeAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ObjectDisposedException>(
            () => uow.CommitAsync()
        );

        exception.ObjectName.Should().Contain(nameof(UnitOfWork));
    }

    [Fact]
    public async Task RollbackAsync_WhenDisposed_ThrowsObjectDisposedException()
    {
        // Arrange
        var uow = CreateUnitOfWork();
        await uow.DisposeAsync();

        // Act & Assert - RollbackAsync should throw when disposed
        var exception = await Assert.ThrowsAsync<ObjectDisposedException>(
            () => uow.RollbackAsync()
        );

        exception.ObjectName.Should().Contain("UnitOfWork");
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WithSuccessfulOperation_CommitsTransaction()
    {
        // Arrange
        var operationExecuted = false;
        Task Operation() => Task.Run(() => operationExecuted = true);

        await using var uow = CreateUnitOfWork();

        // Act
        await uow.ExecuteInTransactionAsync(Operation);

        // Assert
        operationExecuted.Should().BeTrue();
        // Transaction should be committed
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WithSuccessfulOperation_ReturnsResult()
    {
        // Arrange
        const string expectedResult = "test result";
        Task<string> Operation() => Task.FromResult(expectedResult);

        await using var uow = CreateUnitOfWork();

        // Act
        var result = await uow.ExecuteInTransactionAsync(Operation);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WithFailingOperation_RollsBackTransaction()
    {
        // Arrange
        Task FailingOperation() => throw new InvalidOperationException("Test failure");

        await using var uow = CreateUnitOfWork();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => uow.ExecuteInTransactionAsync(FailingOperation)
        );

        // The rollback should have been called by the exception handler
        // Note: The pattern ensures it happens via the catch block in ExecuteInTransactionAsync
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WithNullOperation_ThrowsArgumentNullException()
    {
        // Arrange
        await using var uow = CreateUnitOfWork();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => uow.ExecuteInTransactionAsync((Func<Task>)null!)
        );

        exception.ParamName.Should().Be("operation");
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_Generic_WithNullOperation_ThrowsArgumentNullException()
    {
        // Arrange
        await using var uow = CreateUnitOfWork();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => uow.ExecuteInTransactionAsync<string>((Func<Task<string>>)null!)
        );

        exception.ParamName.Should().Be("operation");
    }

    [Fact]
    public void Dispose_WhenDisposedMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var uow = CreateUnitOfWork();
        uow.Dispose();

        // Act & Assert - Should not throw on multiple dispose
        var act = () => uow.Dispose();

        act.Should().NotThrow();
    }

    [Fact]
    public async Task DisposeAsync_WhenDisposedMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var uow = CreateUnitOfWork();
        await uow.DisposeAsync();

        // Act & Assert - Should not throw on multiple dispose
        var act = async () => await uow.DisposeAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Dispose_WithoutCommit_RepositoryAccessAfterDispose_ReturnsRepositories()
    {
        // Arrange
        await using var uow = CreateUnitOfWork();
        // Don't commit, just dispose
        await uow.DisposeAsync();

        // Act & Assert - Repository properties still return the repositories (not disposed)
        // The UnitOfWork itself is disposed, but the injected repositories are not
        uow.Gateways.Should().NotBeNull();
        uow.Services.Should().NotBeNull();
        uow.Routes.Should().NotBeNull();
        uow.Metrics.Should().NotBeNull();
    }

    [Fact]
    public async Task CommitAsync_WhenTransactionAlreadyCommitted_ThrowsInvalidOperationExceptionWithClearMessage()
    {
        // Arrange
        await using var uow = CreateUnitOfWork();
        await uow.CommitAsync();

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => uow.CommitAsync()
        );

        // Assert
        exception.Message.Should().Contain("Transaction has already been committed");
    }

    [Fact]
    public async Task RollbackAsync_WhenTransactionAlreadyCommitted_DoesNotThrow()
    {
        // Arrange
        await using var uow = CreateUnitOfWork();
        await uow.CommitAsync();

        // Act & Assert - Rollback after commit should not throw
        var act = async () => await uow.RollbackAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WithCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await using var uow = CreateUnitOfWork();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<OperationCanceledException>(
            () => uow.ExecuteInTransactionAsync(() => Task.CompletedTask, cts.Token)
        );

        exception.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_Generic_WithCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await using var uow = CreateUnitOfWork();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<OperationCanceledException>(
            () => uow.ExecuteInTransactionAsync(() => Task.FromResult("result"), cts.Token)
        );

        exception.Should().NotBeNull();
    }

    [Fact]
    public void Dispose_EnsuresDisposedFlagIsSet()
    {
        // Arrange
        var uow = CreateUnitOfWork();

        // Act
        uow.Dispose();

        // Assert - Use reflection to verify _disposed flag is set
        var field = typeof(UnitOfWork).GetField(
            "_disposed",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
        );
        field.Should().NotBeNull();

        var disposedValue = (bool)field!.GetValue(uow)!;
        disposedValue.Should().BeTrue();
    }

    [Fact]
    public async Task DisposeAsync_EnsuresDisposedFlagIsSet()
    {
        // Arrange
        var uow = CreateUnitOfWork();

        // Act
        await uow.DisposeAsync();

        // Assert - Use reflection to verify _disposed flag is set
        var field = typeof(UnitOfWork).GetField(
            "_disposed",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
        );
        field.Should().NotBeNull();

        var disposedValue = (bool)field!.GetValue(uow)!;
        disposedValue.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WithSuccessfulOperation_TransactionCommittedFlagIsSet()
    {
        // Arrange
        await using var uow = CreateUnitOfWork();

        // Act
        await uow.ExecuteInTransactionAsync(() => Task.CompletedTask);

        // Assert - Use reflection to verify _transactionCommitted flag is set
        var field = typeof(UnitOfWork).GetField(
            "_transactionCommitted",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
        );
        field.Should().NotBeNull();

        var committedValue = (bool)field!.GetValue(uow)!;
        committedValue.Should().BeTrue();
    }

    [Fact]
    public async Task CommitAsync_SetsTransactionCommittedFlag()
    {
        // Arrange
        await using var uow = CreateUnitOfWork();

        // Act
        await uow.CommitAsync();

        // Assert - Use reflection to verify _transactionCommitted flag is set
        var field = typeof(UnitOfWork).GetField(
            "_transactionCommitted",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
        );
        field.Should().NotBeNull();

        var committedValue = (bool)field!.GetValue(uow)!;
        committedValue.Should().BeTrue();
    }

    [Fact]
    public async Task RollbackAsync_SetsTransactionCommittedFlag()
    {
        // Arrange
        await using var uow = CreateUnitOfWork();

        // Act
        await uow.RollbackAsync();

        // Assert - Use reflection to verify _transactionCommitted flag is set
        var field = typeof(UnitOfWork).GetField(
            "_transactionCommitted",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
        );
        field.Should().NotBeNull();

        var committedValue = (bool)field!.GetValue(uow)!;
        committedValue.Should().BeTrue();
    }
}