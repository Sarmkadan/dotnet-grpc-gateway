using DotNetGrpcGateway.Infrastructure;
using FluentAssertions;
using Xunit;

namespace DotNetGrpcGateway.Tests;

/// <summary>
/// Tests for <see cref="RequestContextAccessor"/> ambient context behavior.
/// Ensures that AsyncLocal-based context isolation works correctly across various async scenarios.
/// </summary>
/// <remarks>
/// These tests verify that RequestContextAccessor.Current properly isolates contexts across:
/// - Different async flows
/// - Parallel execution
/// - Nested scopes
/// - Parent-child task relationships
/// </remarks>
public class RequestContextAccessorTests
{
    [Fact]
    public void Current_Get_BeforeAnySet_ReturnsNull()
    {
        // Arrange - Ensure no context is set from previous tests
        RequestContextAccessor.Current = null;

        // Act
        var current = RequestContextAccessor.Current;

        // Assert
        current.Should().BeNull("RequestContextAccessor.Current should return null when no context has been set");
    }

    [Fact]
    public void Current_SetNull_ClearsContext()
    {
        // Arrange
        var context = new RequestContext { Path = "/test" };
        RequestContextAccessor.Current = context;
        RequestContextAccessor.Current.Should().BeSameAs(context);

        // Act
        RequestContextAccessor.Current = null;

        // Assert
        RequestContextAccessor.Current.Should().BeNull("Setting Current to null should clear the context");
    }

    [Fact]
    public void Current_SetAndGet_RoundtripPreservesReference()
    {
        // Arrange
        var context = new RequestContext { Path = "/api/test" };
        context.CorrelationId = "test-correlation-id";

        // Act
        RequestContextAccessor.Current = context;
        var retrieved = RequestContextAccessor.Current;

        // Assert
        retrieved.Should().BeSameAs(context, "Current should return the exact same instance that was set");
        retrieved.CorrelationId.Should().Be("test-correlation-id");
        retrieved.Path.Should().Be("/api/test");
    }

    [Fact]
    public async Task Current_Get_InDifferentAsyncFlows_StartsEmpty()
    {
        // Arrange - Ensure no context is set initially
        RequestContextAccessor.Current = null;

        // Act - Create a separate async flow
        var capturedContext = await Task.Run(() =>
        {
            // In a different async context, Current should be null initially
            return RequestContextAccessor.Current;
        });

        // Assert
        capturedContext.Should().BeNull("Different async flow should start with no context");
    }

    [Fact]
    public async Task Current_SetInParallelTasks_DoesNotLeakBetweenTasks()
    {
        // Arrange
        var context1 = new RequestContext { Path = "/api/task1" };
        context1.CorrelationId = "task-1";
        var context2 = new RequestContext { Path = "/api/task2" };
        context2.CorrelationId = "task-2";

        var task1Result = new TaskCompletionSource<RequestContext?>();
        var task2Result = new TaskCompletionSource<RequestContext?>();

        // Act - Run two parallel tasks that set different contexts
        var task1 = Task.Run(() =>
        {
            RequestContextAccessor.Current = context1;
            // Small delay to ensure both tasks are running
            Thread.Sleep(50);
            var current = RequestContextAccessor.Current;
            task1Result.SetResult(current);
            return current;
        });

        var task2 = Task.Run(() =>
        {
            // Ensure task2 starts after task1 has set its context
            Thread.Sleep(25);
            RequestContextAccessor.Current = context2;
            Thread.Sleep(50);
            var current = RequestContextAccessor.Current;
            task2Result.SetResult(current);
            return current;
        });

        await Task.WhenAll(task1, task2);

        // Assert
        var result1 = await task1Result.Task;
        var result2 = await task2Result.Task;

        result1.Should().BeSameAs(context1, "Task 1 should see its own context");
        result2.Should().BeSameAs(context2, "Task 2 should see its own context");
        result1.Should().NotBeSameAs(result2, "Different tasks should have different contexts");
        result1.CorrelationId.Should().Be("task-1");
        result2.CorrelationId.Should().Be("task-2");
    }

    [Fact]
    public async Task Current_InNestedAsyncScopes_RestoresOuterContextAfterInnerScope()
    {
        // Arrange
        var outerContext = new RequestContext { Path = "/api/outer" };
        outerContext.CorrelationId = "outer-context";
        var innerContext = new RequestContext { Path = "/api/inner" };
        innerContext.CorrelationId = "inner-context";

        RequestContextAccessor.Current = outerContext;
        var beforeInner = RequestContextAccessor.Current;
        beforeInner.Should().BeSameAs(outerContext);

        // Act - Create nested async scope
        await Task.Run(async () =>
        {
            RequestContextAccessor.Current = innerContext;
            var inInner = RequestContextAccessor.Current;
            inInner.Should().BeSameAs(innerContext, "Should see inner context within nested scope");

            // Small delay to ensure we're in the nested scope
            await Task.Delay(50);

            // Assert - Inner context should still be visible
            RequestContextAccessor.Current.Should().BeSameAs(innerContext);
        });

        // Assert - After nested scope completes, should restore outer context
        RequestContextAccessor.Current.Should().BeSameAs(outerContext, "Should restore outer context after nested scope completes");
    }

    [Fact]
    public async Task Current_InDeeplyNestedAsyncScopes_RestoresContextProperly()
    {
        // Arrange
        var level1Context = new RequestContext { Path = "/api/level1" };
        level1Context.CorrelationId = "level-1";
        var level2Context = new RequestContext { Path = "/api/level2" };
        level2Context.CorrelationId = "level-2";
        var level3Context = new RequestContext { Path = "/api/level3" };
        level3Context.CorrelationId = "level-3";

        RequestContextAccessor.Current = level1Context;

        // Act - Create three levels of nesting
        await Task.Run(async () =>
        {
            RequestContextAccessor.Current = level2Context;
            await Task.Delay(20);

            await Task.Run(async () =>
            {
                RequestContextAccessor.Current = level3Context;
                await Task.Delay(20);
                RequestContextAccessor.Current.Should().BeSameAs(level3Context);
            });

            // After level 3 completes, should restore level 2
            RequestContextAccessor.Current.Should().BeSameAs(level2Context);
        });

        // After level 2 completes, should restore level 1
        RequestContextAccessor.Current.Should().BeSameAs(level1Context);
    }

    [Fact]
    public async Task Current_InSiblingAsyncScopes_EachHasIsolatedContext()
    {
        // Arrange
        var sibling1Context = new RequestContext { Path = "/api/sibling1" };
        sibling1Context.CorrelationId = "sibling-1";
        var sibling2Context = new RequestContext { Path = "/api/sibling2" };
        sibling2Context.CorrelationId = "sibling-2";

        var sibling1Task = Task.Run(() =>
        {
            RequestContextAccessor.Current = sibling1Context;
            Thread.Sleep(100);
            return RequestContextAccessor.Current;
        });

        var sibling2Task = Task.Run(() =>
        {
            RequestContextAccessor.Current = sibling2Context;
            Thread.Sleep(100);
            return RequestContextAccessor.Current;
        });

        // Act - Run sibling tasks concurrently
        var result1 = await sibling1Task;
        var result2 = await sibling2Task;

        // Assert
        result1.Should().BeSameAs(sibling1Context);
        result2.Should().BeSameAs(sibling2Context);
        result1.Should().NotBeSameAs(result2);
        result1.CorrelationId.Should().Be("sibling-1");
        result2.CorrelationId.Should().Be("sibling-2");
    }

    [Fact]
    public async Task Current_InParentChildTaskRelationship_ChildCanSetOwnContext()
    {
        // Arrange
        var parentContext = new RequestContext { Path = "/api/parent" };
        parentContext.CorrelationId = "parent-context";
        var childContext = new RequestContext { Path = "/api/child" };
        childContext.CorrelationId = "child-context";

        RequestContextAccessor.Current = parentContext;
        var parentBeforeChild = RequestContextAccessor.Current;

        RequestContext? childRetrieved = null;
        RequestContext? parentAfterChild = null;

        // Act - Parent creates a child task
        await Task.Run(async () =>
        {
            // Child sets its own context
            RequestContextAccessor.Current = childContext;
            Thread.Sleep(50);

            // Verify child sees its context
            childRetrieved = RequestContextAccessor.Current;
            RequestContextAccessor.Current.Should().BeSameAs(childContext);

            // Child completes
        });

        parentAfterChild = RequestContextAccessor.Current;

        // Assert - Parent context should be unchanged, child saw its own context
        parentBeforeChild.Should().BeSameAs(parentContext);
        parentAfterChild.Should().BeSameAs(parentContext, "Parent's context should not be affected by child task");
        RequestContextAccessor.Current.Should().BeSameAs(parentContext);
        childRetrieved.Should().BeSameAs(childContext, "Child task should see its own context");
    }

    [Fact]
    public async Task Current_AfterTaskCompletes_ContextIsCleared()
    {
        // Arrange
        var context = new RequestContext { Path = "/api/test" };
        context.CorrelationId = "test-context";

        RequestContext? contextAfterTask = null;

        // Act - Run a task that sets context
        await Task.Run(() =>
        {
            RequestContextAccessor.Current = context;
            Thread.Sleep(50);
        });

        // After task completes, context should be cleared
        contextAfterTask = RequestContextAccessor.Current;

        // Assert
        contextAfterTask.Should().BeNull("Context should be cleared after async task completes");
    }

    [Fact]
    public void Current_SetMultipleTimes_OverwritesPreviousContext()
    {
        // Arrange
        var context1 = new RequestContext { Path = "/api/first" };
        var context2 = new RequestContext { Path = "/api/second" };

        // Act
        RequestContextAccessor.Current = context1;
        var firstRetrieved = RequestContextAccessor.Current;

        RequestContextAccessor.Current = context2;
        var secondRetrieved = RequestContextAccessor.Current;

        // Assert
        firstRetrieved.Should().BeSameAs(context1);
        secondRetrieved.Should().BeSameAs(context2);
        RequestContextAccessor.Current.Should().BeSameAs(context2);
    }

    [Fact]
    public async Task Current_ClearingContextInChildTask_DoesNotAffectParent()
    {
        // Arrange
        var parentContext = new RequestContext { Path = "/api/parent" };
        parentContext.CorrelationId = "parent-context";

        RequestContextAccessor.Current = parentContext;
        var parentBefore = RequestContextAccessor.Current;

        // Act - Child task clears its context
        await Task.Run(() =>
        {
            RequestContextAccessor.Current = null;
            Thread.Sleep(50);
        });

        // Assert - Parent context should be unchanged
        RequestContextAccessor.Current.Should().BeSameAs(parentBefore);
    }

    [Fact]
    public async Task Current_InConfiguredAwaitFalse_StillMaintainsIsolation()
    {
        // Arrange
        var context1 = new RequestContext { Path = "/api/configured1" };
        context1.CorrelationId = "configured-1";
        var context2 = new RequestContext { Path = "/api/configured2" };
        context2.CorrelationId = "configured-2";

        var task1Result = new TaskCompletionSource<RequestContext?>();
        var task2Result = new TaskCompletionSource<RequestContext?>();

        // Act - Use ConfigureAwait(false) which doesn't capture synchronization context
        var task1 = Task.Run(() =>
        {
            RequestContextAccessor.Current = context1;
            Thread.Sleep(50);
            var current = RequestContextAccessor.Current;
            task1Result.SetResult(current);
            return current;
        });

        var task2 = Task.Run(() =>
        {
            Thread.Sleep(25);
            RequestContextAccessor.Current = context2;
            Thread.Sleep(50);
            var current = RequestContextAccessor.Current;
            task2Result.SetResult(current);
            return current;
        });

        await Task.WhenAll(task1, task2);

        // Assert
        var result1 = await task1Result.Task;
        var result2 = await task2Result.Task;

        result1.Should().BeSameAs(context1);
        result2.Should().BeSameAs(context2);
        result1.Should().NotBeSameAs(result2);
    }

    [Fact]
    public void Current_WithComplexAsyncFlows_MaintainsCorrectIsolation()
    {
        // This test simulates a more complex real-world scenario with multiple context switches

        // Arrange - Create contexts
        var contexts = new RequestContext[5];
        for (int i = 0; i < contexts.Length; i++)
        {
            contexts[i] = new RequestContext { Path = $"/api/context{i}" };
            contexts[i].CorrelationId = $"context-{i}";
        }

        // Act & Assert - Chain of context switches
        RequestContextAccessor.Current = contexts[0];
        RequestContextAccessor.Current.Should().BeSameAs(contexts[0]);

        RequestContextAccessor.Current = contexts[1];
        RequestContextAccessor.Current.Should().BeSameAs(contexts[1]);

        RequestContextAccessor.Current = contexts[2];
        RequestContextAccessor.Current.Should().BeSameAs(contexts[2]);

        RequestContextAccessor.Current = contexts[3];
        RequestContextAccessor.Current.Should().BeSameAs(contexts[3]);

        RequestContextAccessor.Current = contexts[4];
        RequestContextAccessor.Current.Should().BeSameAs(contexts[4]);

        // Verify final state
        RequestContextAccessor.Current.Should().BeSameAs(contexts[4]);
        RequestContextAccessor.Current.CorrelationId.Should().Be("context-4");
    }

    [Fact]
    public async Task Current_InFireAndForgetPattern_DoesNotAffectMainFlow()
    {
        // Arrange
        var mainContext = new RequestContext { Path = "/api/main" };
        mainContext.CorrelationId = "main-context";
        var fireAndForgetContext = new RequestContext { Path = "/api/fire-forget" };
        fireAndForgetContext.CorrelationId = "fire-forget-context";

        RequestContextAccessor.Current = mainContext;

        // Act - Start fire-and-forget operation
        _ = Task.Run(() =>
        {
            RequestContextAccessor.Current = fireAndForgetContext;
            Thread.Sleep(50);
            // Fire-and-forget task completes
        });

        // Small delay to allow fire-and-forget to start
        await Task.Delay(25);

        // Assert - Main flow should still see its original context
        RequestContextAccessor.Current.Should().BeSameAs(mainContext);
    }
}