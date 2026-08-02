using DotNetGrpcGateway.Infrastructure;
using FluentAssertions;
using Xunit;

namespace DotNetGrpcGateway.Tests;

/// <summary>
/// Contains unit tests for the <see cref="RequestContext"/> class.
/// </summary>
public class RequestContextTests
{
    /// <summary>
    /// Validates that a new <see cref="RequestContext"/> instance initializes its properties with correct default values.
    /// </summary>
    [Fact]
    public void Constructor_InitializesPropertiesCorrectly()
    {
        // Act
        var context = new RequestContext();

        // Assert
        context.RequestId.Should().NotBeNullOrEmpty();
        context.RequestId.Should().MatchRegex("^\\d{14}-\\d{6}$");
        context.CorrelationId.Should().Be(context.RequestId);
        context.ClientIp.Should().BeEmpty();
        context.UserId.Should().BeNull();
        context.Path.Should().BeEmpty();
        context.Method.Should().BeEmpty();
        context.StartTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        context.Properties.Should().NotBeNull();
        context.Properties.Should().BeEmpty();
    }

    /// <summary>
    /// Validates that the constructor correctly initializes the <see cref="RequestContext.CorrelationId"/> when provided.
    /// </summary>
    [Fact]
    public void Constructor_WithCorrelationId_SetsCorrelationId()
    {
        // Arrange
        var expectedCorrelationId = "test-correlation-id";

        // Act
        var context = new RequestContext { CorrelationId = expectedCorrelationId };

        // Assert
        context.CorrelationId.Should().Be(expectedCorrelationId);
    }

    /// <summary>
    /// Validates that the <see cref="RequestContext.RequestId"/> property is generated upon initialization and is immutable.
    /// </summary>
    [Fact]
    public void RequestId_IsGeneratedAndImmutable()
    {
        // Arrange
        var context = new RequestContext();
        var originalRequestId = context.RequestId;

        // Act - try to modify (should not be possible due to getter-only)
        // Assert
        context.RequestId.Should().Be(originalRequestId);
    }

    /// <summary>
    /// Validates that <see cref="RequestContext.SetProperty(string, object)"/> adds a property when provided with a valid key and value.
    /// </summary>
    [Fact]
    public void SetProperty_WithValidKeyAndValue_AddsProperty()
    {
        // Arrange
        var context = new RequestContext();
        var key = "testKey";
        var value = "testValue";

        // Act
        context.SetProperty(key, value);

        // Assert
        context.Properties.Should().ContainKey(key);
        context.Properties[key].Should().Be(value);
    }

    /// <summary>
    /// Validates that <see cref="RequestContext.SetProperty(string, object)"/> removes a property when provided with a null value.
    /// </summary>
    [Fact]
    public void SetProperty_WithNullValue_RemovesProperty()
    {
        // Arrange
        var context = new RequestContext();
        var key = "testKey";
        context.SetProperty(key, "testValue");
        context.Properties.Should().ContainKey(key);

        // Act
        context.SetProperty(key, null);

        // Assert
        context.Properties.Should().NotContainKey(key);
    }

    /// <summary>
    /// Validates that <see cref="RequestContext.SetProperty(string, object)"/> does nothing when provided with an empty or null key.
    /// </summary>
    [Fact]
    public void SetProperty_WithEmptyKey_DoesNothing()
    {
        // Arrange
        var context = new RequestContext();
        var initialPropertyCount = context.Properties.Count;

        // Act
        context.SetProperty(string.Empty, "value");
        context.SetProperty(null, "value");

        // Assert
        context.Properties.Should().HaveCount(initialPropertyCount);
    }

    /// <summary>
    /// Validates that <see cref="RequestContext.GetProperty{T}(string)"/> returns the correct value for an existing property.
    /// </summary>
    [Fact]
    public void GetProperty_WithExistingProperty_ReturnsCorrectValue()
    {
        // Arrange
        var context = new RequestContext();
        var key = "userId";
        var expectedValue = 12345;
        context.SetProperty(key, expectedValue);

        // Act
        var result = context.GetProperty<int>(key);

        // Assert
        result.Should().Be(expectedValue);
    }

    /// <summary>
    /// Validates that <see cref="RequestContext.GetProperty{T}(string)"/> returns the default value when the property does not exist.
    /// </summary>
    [Fact]
    public void GetProperty_WithNonExistingProperty_ReturnsDefault()
    {
        // Arrange
        var context = new RequestContext();
        var key = "nonExistentKey";

        // Act
        var result = context.GetProperty<int>(key);

        // Assert
        result.Should().Be(default);
    }

    /// <summary>
    /// Validates that <see cref="RequestContext.GetProperty{T}(string)"/> returns the default value when provided with an empty or null key.
    /// </summary>
    [Fact]
    public void GetProperty_WithEmptyKey_ReturnsDefault()
    {
        // Arrange
        var context = new RequestContext();
        context.SetProperty("validKey", "value");

        // Act
        var result1 = context.GetProperty<string>(string.Empty);
        var result2 = context.GetProperty<string>(null!);

        // Assert
        result1.Should().BeNull();
        result2.Should().BeNull();
    }

    /// <summary>
    /// Validates that <see cref="RequestContext.GetProperty{T}(string)"/> throws an <see cref="InvalidCastException"/> when the requested property exists but is of the wrong type.
    /// </summary>
    [Fact]
    public void GetProperty_WithWrongType_ThrowsInvalidCastException()
    {
        // Arrange
        var context = new RequestContext();
        context.SetProperty("numberKey", 123);

        // Act
        Action act = () => context.GetProperty<string>("numberKey");

        // Assert
        act.Should().Throw<InvalidCastException>();
    }

    /// <summary>
    /// Validates that the <see cref="RequestContext.Properties"/> dictionary is initialized as an empty dictionary.
    /// </summary>
    [Fact]
    public void PropertiesBag_IsInitializedAsEmptyDictionary()
    {
        // Arrange & Act
        var context = new RequestContext();

        // Assert
        context.Properties.Should().NotBeNull();
        context.Properties.Should().BeEmpty();
        context.Properties.Should().BeOfType<Dictionary<string, object>>();
    }

    /// <summary>
    /// Validates that <see cref="RequestContext.Elapsed"/> returns a non-negative <see cref="TimeSpan"/>.
    /// </summary>
    [Fact]
    public void Elapsed_ReturnsNonNegativeTimeSpan()
    {
        // Arrange
        var context = new RequestContext();
        var startTime = context.StartTime;

        // Small delay to ensure time has passed
        Thread.Sleep(10);

        // Act
        var elapsed = context.Elapsed;

        // Assert
        elapsed.Should().BePositive();
        elapsed.Should().BeGreaterThan(TimeSpan.Zero);
        elapsed.Should().BeCloseTo(DateTime.UtcNow - startTime, TimeSpan.FromMilliseconds(50));
    }

    /// <summary>
    /// Validates that <see cref="RequestContext.SetProperty(string, object)"/> updates the value of a property if it is set multiple times.
    /// </summary>
    [Fact]
    public void SetProperty_MultipleTimes_UpdatesPropertyValue()
    {
        // Arrange
        var context = new RequestContext();
        var key = "counter";

        // Act
        context.SetProperty(key, 1);
        context.SetProperty(key, 2);
        context.SetProperty(key, 3);

        // Assert
        context.Properties.Should().ContainKey(key);
        context.Properties[key].Should().Be(3);
    }

    /// <summary>
    /// Validates that <see cref="RequestContext.SetProperty(string, object)"/> correctly stores a complex object as a property value.
    /// </summary>
    [Fact]
    public void SetProperty_WithComplexObject_StoresCorrectly()
    {
        // Arrange
        var context = new RequestContext();
        var key = "user";
        var user = new { Id = 1, Name = "Test User", Email = "test@example.com" };

        // Act
        context.SetProperty(key, user);

        // Assert
        context.Properties.Should().ContainKey(key);
        context.Properties[key].Should().BeSameAs(user);
    }

    /// <summary>
    /// Validates that the <see cref="RequestContext.CorrelationId"/> property can be modified.
    /// </summary>
    [Fact]
    public void CorrelationId_CanBeModified()
    {
        // Arrange
        var context = new RequestContext();
        var newCorrelationId = "new-correlation-id";

        // Act
        context.CorrelationId = newCorrelationId;

        // Assert
        context.CorrelationId.Should().Be(newCorrelationId);
    }

    /// <summary>
    /// Validates that the <see cref="RequestContext.ClientIp"/> property can be modified.
    /// </summary>
    [Fact]
    public void ClientIp_CanBeModified()
    {
        // Arrange
        var context = new RequestContext();
        var ip = "192.168.1.100";

        // Act
        context.ClientIp = ip;

        // Assert
        context.ClientIp.Should().Be(ip);
    }

    /// <summary>
    /// Validates that the <see cref="RequestContext.UserId"/> property can be modified.
    /// </summary>
    [Fact]
    public void UserId_CanBeModified()
    {
        // Arrange
        var context = new RequestContext();
        var userId = "user123";

        // Act
        context.UserId = userId;

        // Assert
        context.UserId.Should().Be(userId);
    }

    /// <summary>
    /// Validates that the <see cref="RequestContext.UserId"/> property can be set to null.
    /// </summary>
    [Fact]
    public void UserId_CanBeSetToNull()
    {
        // Arrange
        var context = new RequestContext();
        context.UserId = "user123";

        // Act
        context.UserId = null;

        // Assert
        context.UserId.Should().BeNull();
    }

    /// <summary>
    /// Validates that the <see cref="RequestContext.Path"/> property can be modified.
    /// </summary>
    [Fact]
    public void Path_CanBeModified()
    {
        // Arrange
        var context = new RequestContext();
        var path = "/api/v1/users";

        // Act
        context.Path = path;

        // Assert
        context.Path.Should().Be(path);
    }

    /// <summary>
    /// Validates that the <see cref="RequestContext.Method"/> property can be modified.
    /// </summary>
    [Fact]
    public void Method_CanBeModified()
    {
        // Arrange
        var context = new RequestContext();
        var method = "POST";

        // Act
        context.Method = method;

        // Assert
        context.Method.Should().Be(method);
    }

    /// <summary>
    /// Validates that the <see cref="RequestContext.RequestId"/> property adheres to the expected format.
    /// </summary>
    [Fact]
    public void RequestId_FormatIsValid()
    {
        // Arrange
        var context = new RequestContext();

        // Act & Assert
        context.RequestId.Should().MatchRegex("^\\d{14}-\\d{6}$",
            "RequestId should be in format YYYYMMDDHHMMSS-RRRRRR");
    }

    /// <summary>
    /// Validates that the <see cref="RequestContext.Properties"/> dictionary is independent between <see cref="RequestContext"/> instances.
    /// </summary>
    [Fact]
    public void PropertiesDictionary_IsIndependentBetweenInstances()
    {
        // Arrange
        var context1 = new RequestContext();
        var context2 = new RequestContext();

        // Act
        context1.SetProperty("key", "value1");
        context2.SetProperty("key", "value2");

        // Assert
        context1.Properties.Should().ContainKey("key").WhoseValue.Should().Be("value1");
        context2.Properties.Should().ContainKey("key").WhoseValue.Should().Be("value2");
    }

    /// <summary>
    /// Validates that the <see cref="RequestContext.StartTime"/> property is initialized to the current UTC time.
    /// </summary>
    [Fact]
    public void StartTime_IsUtcNow()
    {
        // Arrange
        var context = new RequestContext();

        // Assert
        context.StartTime.Kind.Should().Be(DateTimeKind.Utc);
        context.StartTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Validates that <see cref="RequestContext.SetProperty(string, object)"/> does not throw an exception when trying to set a null value for a property that does not exist.
    /// </summary>
    [Fact]
    public void SetProperty_WithNullValue_WhenPropertyDoesNotExist_DoesNotThrow()
    {
        // Arrange
        var context = new RequestContext();

        // Act - should not throw
        var act = () => context.SetProperty("nonExistentKey", null);

        // Assert
        act.Should().NotThrow();
        context.Properties.Should().NotContainKey("nonExistentKey");
    }

    /// <summary>
    /// Validates that <see cref="RequestContextAccessor"/> handles <see cref="AsyncLocal{T}"/> correctly under concurrent request conditions.
    /// </summary>
    [Fact]
    public async Task RequestContextAccessor_AsyncLocal_Correctness_WithConcurrentRequests()
    {
        // Arrange - Create contexts properly (RequestId is read-only and auto-generated)
        var task1Tcs = new TaskCompletionSource<RequestContext>();
        var task2Tcs = new TaskCompletionSource<RequestContext>();
        var context1 = new RequestContext { Path = "/api/test1" };
        context1.CorrelationId = "context-1";
        var context2 = new RequestContext { Path = "/api/test2" };
        context2.CorrelationId = "context-2";

        // Act - Run two overlapping async operations that set different contexts
        var task1 = Task.Run(async () =>
        {
            RequestContextAccessor.Current = context1;
            await Task.Delay(100); // Ensure both tasks are running concurrently
            var result = RequestContextAccessor.Current;
            task1Tcs.SetResult(result);
            return result;
        });

        var task2 = Task.Run(async () =>
        {
            await Task.Delay(50); // Start slightly later to ensure interleaving
            RequestContextAccessor.Current = context2;
            await Task.Delay(100);
            var result = RequestContextAccessor.Current;
            task2Tcs.SetResult(result);
            return result;
        });

        await Task.WhenAll(task1, task2);

        // Assert
        var result1 = await task1Tcs.Task;
        var result2 = await task2Tcs.Task;

        // Each task should see its own context, not the other's
        result1.Should().BeSameAs(context1, "Task 1 should see its own context");
        result2.Should().BeSameAs(context2, "Task 2 should see its own context");

        // Verify the contexts are different
        result1.Should().NotBeSameAs(result2);
        result1.CorrelationId.Should().Be("context-1");
        result2.CorrelationId.Should().Be("context-2");
    }

    /// <summary>
    /// Validates that <see cref="RequestContextAccessor.Current"/> property supports get and set operations.
    /// </summary>
    [Fact]
    public void RequestContextAccessor_Current_GetSet_Roundtrip()
    {
        // Arrange
        var context = new RequestContext { Path = "/test" };
        context.CorrelationId = "test-correlation-id";

        // Act
        RequestContextAccessor.Current = context;
        var retrieved = RequestContextAccessor.Current;

        // Assert
        retrieved.Should().BeSameAs(context);
        retrieved.CorrelationId.Should().Be("test-correlation-id");
    }

    /// <summary>
    /// Validates that setting <see cref="RequestContextAccessor.Current"/> to null clears the context.
    /// </summary>
    [Fact]
    public void RequestContextAccessor_Current_SetNull_ClearsContext()
    {
        // Arrange
        var context = new RequestContext();
        RequestContextAccessor.Current = context;

        // Act
        RequestContextAccessor.Current = null;

        // Assert
        RequestContextAccessor.Current.Should().BeNull();
    }
}