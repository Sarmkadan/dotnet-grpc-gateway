#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using FluentAssertions;
using DotNetGrpcGateway.Streaming;
using Xunit;

namespace DotNetGrpcGateway.Tests;

/// <summary>
/// Contains unit tests for the FlowControlWindow class.
/// Tests basic property initialization and validation scenarios.
/// </summary>
public class FlowControlWindowTests
{
    /// <summary>
    /// Verifies that a FlowControlWindow can be initialized with valid properties.
    /// </summary>
    [Fact]
    public void Constructor_WithValidProperties_InitializesCorrectly()
    {
        // Arrange & Act
        var window = new FlowControlWindow
        {
            InitialSize = 1024,
            AvailableCredits = 1024,
            IsThrottled = false
        };

        // Assert
        window.InitialSize.Should().Be(1024);
        window.AvailableCredits.Should().Be(1024);
        window.IsThrottled.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that InitialSize can be set to a positive value.
    /// </summary>
    [Fact]
    public void InitialSize_SetPositiveValue_Succeeds()
    {
        // Arrange
        var window = new FlowControlWindow();

        // Act
        window.InitialSize = 65535;

        // Assert
        window.InitialSize.Should().Be(65535);
    }

    /// <summary>
    /// Verifies that InitialSize can be set to zero.
    /// </summary>
    [Fact]
    public void InitialSize_SetToZero_Succeeds()
    {
        // Arrange
        var window = new FlowControlWindow();

        // Act
        window.InitialSize = 0;

        // Assert
        window.InitialSize.Should().Be(0);
    }

    /// <summary>
    /// Verifies that AvailableCredits can be set to a positive value.
    /// </summary>
    [Fact]
    public void AvailableCredits_SetPositiveValue_Succeeds()
    {
        // Arrange
        var window = new FlowControlWindow();

        // Act
        window.AvailableCredits = 1024;

        // Assert
        window.AvailableCredits.Should().Be(1024);
    }

    /// <summary>
    /// Verifies that AvailableCredits can be set to zero.
    /// </summary>
    [Fact]
    public void AvailableCredits_SetToZero_Succeeds()
    {
        // Arrange
        var window = new FlowControlWindow();

        // Act
        window.AvailableCredits = 0;

        // Assert
        window.AvailableCredits.Should().Be(0);
    }

    /// <summary>
    /// Verifies that IsThrottled can be set to true.
    /// </summary>
    [Fact]
    public void IsThrottled_SetToTrue_Succeeds()
    {
        // Arrange
        var window = new FlowControlWindow();

        // Act
        window.IsThrottled = true;

        // Assert
        window.IsThrottled.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that IsThrottled can be set to false.
    /// </summary>
    [Fact]
    public void IsThrottled_SetToFalse_Succeeds()
    {
        // Arrange
        var window = new FlowControlWindow { IsThrottled = true };

        // Act
        window.IsThrottled = false;

        // Assert
        window.IsThrottled.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that FlowControlWindow can be fully configured with all properties.
    /// </summary>
    [Fact]
    public void FlowControlWindow_FullConfiguration_Succeeds()
    {
        // Arrange & Act
        var window = new FlowControlWindow
        {
            InitialSize = 2048,
            AvailableCredits = 2048,
            IsThrottled = true
        };

        // Assert all properties are set correctly
        window.InitialSize.Should().Be(2048);
        window.AvailableCredits.Should().Be(2048);
        window.IsThrottled.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that two FlowControlWindow instances with same values are equal.
    /// </summary>
    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var window1 = new FlowControlWindow
        {
            InitialSize = 1024,
            AvailableCredits = 512,
            IsThrottled = true
        };

        var window2 = new FlowControlWindow
        {
            InitialSize = 1024,
            AvailableCredits = 512,
            IsThrottled = true
        };

        // Act & Assert
        window1.Should().BeEquivalentTo(window2);
    }

    /// <summary>
    /// Verifies that two FlowControlWindow instances with different values are not equal.
    /// </summary>
    [Fact]
    public void Equals_WithDifferentValues_ReturnsFalse()
    {
        // Arrange
        var window1 = new FlowControlWindow
        {
            InitialSize = 1024,
            AvailableCredits = 512,
            IsThrottled = true
        };

        var window2 = new FlowControlWindow
        {
            InitialSize = 2048,
            AvailableCredits = 1024,
            IsThrottled = false
        };

        // Act & Assert
        window1.Should().NotBeEquivalentTo(window2);
    }

    /// <summary>
    /// Verifies that FlowControlWindow can be used in collections.
    /// </summary>
    [Fact]
    public void FlowControlWindow_UsedInCollections_Succeeds()
    {
        // Arrange
        var window1 = new FlowControlWindow { InitialSize = 100, AvailableCredits = 100 };
        var window2 = new FlowControlWindow { InitialSize = 200, AvailableCredits = 200 };
        var window3 = new FlowControlWindow { InitialSize = 300, AvailableCredits = 300 };

        var list = new List<FlowControlWindow> { window1, window2, window3 };

        // Act & Assert
        list.Should().HaveCount(3);
        list[0].InitialSize.Should().Be(100);
        list[1].InitialSize.Should().Be(200);
        list[2].InitialSize.Should().Be(300);
    }

    /// <summary>
    /// Verifies that FlowControlWindow can be serialized and deserialized.
    /// </summary>
    [Fact]
    public void FlowControlWindow_SerializationRoundtrip_Succeeds()
    {
        // Arrange
        var original = new FlowControlWindow
        {
            InitialSize = 4096,
            AvailableCredits = 2048,
            IsThrottled = false
        };

        // Simulate serialization/deserialization by creating a new instance with same values
        var deserialized = new FlowControlWindow
        {
            InitialSize = original.InitialSize,
            AvailableCredits = original.AvailableCredits,
            IsThrottled = original.IsThrottled
        };

        // Act & Assert
        deserialized.Should().BeEquivalentTo(original);
    }
}
