#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetGrpcGateway.Infrastructure;
using FluentAssertions;
using Xunit;

/// <summary>
/// Tests for the CircuitBreakerOptions class.
/// </summary>
public class CircuitBreakerOptionsTests
{
    /// <summary>
    /// Tests that default values are correctly initialized.
    /// </summary>
    [Fact]
    public void Constructor_DefaultValues_InitializedCorrectly()
    {
        // Act
        var options = new CircuitBreakerOptions();

        // Assert
        options.FailureThreshold.Should().Be(5);
        options.OpenDuration.Should().Be(TimeSpan.FromSeconds(30));
        options.HalfOpenSuccessThreshold.Should().Be(2);
    }

    /// <summary>
    /// Tests setting and getting FailureThreshold property.
    /// </summary>
    [Fact]
    public void FailureThreshold_SetAndGet_ReturnsExpectedValue()
    {
        // Arrange
        var options = new CircuitBreakerOptions();
        const int expectedValue = 10;

        // Act
        options.FailureThreshold = expectedValue;

        // Assert
        options.FailureThreshold.Should().Be(expectedValue);
    }

    /// <summary>
    /// Tests setting FailureThreshold to minimum boundary value (0).
    /// </summary>
    [Fact]
    public void FailureThreshold_SetToZero_StoresZero()
    {
        // Arrange
        var options = new CircuitBreakerOptions();

        // Act
        options.FailureThreshold = 0;

        // Assert
        options.FailureThreshold.Should().Be(0);
    }

    /// <summary>
    /// Tests setting FailureThreshold to negative value.
    /// </summary>
    [Fact]
    public void FailureThreshold_SetToNegative_StoresNegativeValue()
    {
        // Arrange
        var options = new CircuitBreakerOptions();
        const int negativeValue = -5;

        // Act
        options.FailureThreshold = negativeValue;

        // Assert
        options.FailureThreshold.Should().Be(negativeValue);
    }

    /// <summary>
    /// Tests setting and getting OpenDuration property.
    /// </summary>
    [Fact]
    public void OpenDuration_SetAndGet_ReturnsExpectedValue()
    {
        // Arrange
        var options = new CircuitBreakerOptions();
        var expectedValue = TimeSpan.FromSeconds(60);

        // Act
        options.OpenDuration = expectedValue;

        // Assert
        options.OpenDuration.Should().Be(expectedValue);
    }

    /// <summary>
    /// Tests setting OpenDuration to zero.
    /// </summary>
    [Fact]
    public void OpenDuration_SetToZero_StoresZero()
    {
        // Arrange
        var options = new CircuitBreakerOptions();

        // Act
        options.OpenDuration = TimeSpan.Zero;

        // Assert
        options.OpenDuration.Should().Be(TimeSpan.Zero);
    }

    /// <summary>
    /// Tests setting OpenDuration to very large value.
    /// </summary>
    [Fact]
    public void OpenDuration_SetToLargeValue_StoresLargeValue()
    {
        // Arrange
        var options = new CircuitBreakerOptions();
        var largeValue = TimeSpan.FromDays(365);

        // Act
        options.OpenDuration = largeValue;

        // Assert
        options.OpenDuration.Should().Be(largeValue);
    }

    /// <summary>
    /// Tests setting and getting HalfOpenSuccessThreshold property.
    /// </summary>
    [Fact]
    public void HalfOpenSuccessThreshold_SetAndGet_ReturnsExpectedValue()
    {
        // Arrange
        var options = new CircuitBreakerOptions();
        const int expectedValue = 5;

        // Act
        options.HalfOpenSuccessThreshold = expectedValue;

        // Assert
        options.HalfOpenSuccessThreshold.Should().Be(expectedValue);
    }

    /// <summary>
    /// Tests setting HalfOpenSuccessThreshold to minimum boundary value (1).
    /// </summary>
    [Fact]
    public void HalfOpenSuccessThreshold_SetToOne_StoresOne()
    {
        // Arrange
        var options = new CircuitBreakerOptions();

        // Act
        options.HalfOpenSuccessThreshold = 1;

        // Assert
        options.HalfOpenSuccessThreshold.Should().Be(1);
    }

    /// <summary>
    /// Tests setting HalfOpenSuccessThreshold to large value.
    /// </summary>
    [Fact]
    public void HalfOpenSuccessThreshold_SetToLargeValue_StoresLargeValue()
    {
        // Arrange
        var options = new CircuitBreakerOptions();
        const int largeValue = 100;

        // Act
        options.HalfOpenSuccessThreshold = largeValue;

        // Assert
        options.HalfOpenSuccessThreshold.Should().Be(largeValue);
    }

    /// <summary>
    /// Tests that all properties can be set independently.
    /// </summary>
    [Fact]
    public void Properties_SetIndependently_AllValuesStoredCorrectly()
    {
        // Arrange
        var options = new CircuitBreakerOptions();

        // Act
        options.FailureThreshold = 15;
        options.OpenDuration = TimeSpan.FromMinutes(2);
        options.HalfOpenSuccessThreshold = 8;

        // Assert
        options.FailureThreshold.Should().Be(15);
        options.OpenDuration.Should().Be(TimeSpan.FromMinutes(2));
        options.HalfOpenSuccessThreshold.Should().Be(8);
    }

    /// <summary>
    /// Tests creating options with object initializer syntax.
    /// </summary>
    [Fact]
    public void ObjectInitializer_Syntax_WorksCorrectly()
    {
        // Act
        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 20,
            OpenDuration = TimeSpan.FromSeconds(45),
            HalfOpenSuccessThreshold = 3
        };

        // Assert
        options.FailureThreshold.Should().Be(20);
        options.OpenDuration.Should().Be(TimeSpan.FromSeconds(45));
        options.HalfOpenSuccessThreshold.Should().Be(3);
    }

    /// <summary>
    /// Tests that default values match expected circuit breaker thresholds.
    /// </summary>
    [Fact]
    public void DefaultValues_MatchExpectedCircuitBreakerBehavior()
    {
        // Arrange
        var options = new CircuitBreakerOptions();

        // Assert - these are typical circuit breaker defaults
        options.FailureThreshold.Should().BeGreaterThan(0, "FailureThreshold should be positive");
        options.OpenDuration.Should().BePositive("OpenDuration should be positive");
        options.HalfOpenSuccessThreshold.Should().BeGreaterThan(0, "HalfOpenSuccessThreshold should be positive");
    }
}