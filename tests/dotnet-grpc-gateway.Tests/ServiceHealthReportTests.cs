#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using DotNetGrpcGateway.Domain;
using Xunit;

namespace DotNetGrpcGateway.Tests;

/// <summary>
/// Unit tests for the <see cref="ServiceHealthReport"/> class.
/// Contains tests for validation, health check recording, diagnostic messages,
/// and various health status calculations.
/// </summary>
public class ServiceHealthReportTests
{
    /// <summary>
    /// Tests that a valid <see cref="ServiceHealthReport"/> with all required properties set
    /// does not throw validation exceptions.
    /// </summary>
    [Fact]
    public void Validate_ValidReport_DoesNotThrow()
    {
        var report = new ServiceHealthReport
        {
            ServiceId = 1,
            IsHealthy = true,
            HealthStatus = "Healthy",
            ResponseTimeMs = 100,
            HttpStatusCode = 200,
            TotalHealthChecks = 10,
            SuccessfulHealthChecks = 10,
            HealthCheckSuccessRate = 100.0
        };

        var act = () => report.Validate();

        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that validation throws <see cref="InvalidOperationException"/> when ServiceId is 0 or negative.
    /// </summary>
    [Fact]
    public void Validate_InvalidServiceId_ThrowsInvalidOperationException()
    {
        var report = new ServiceHealthReport
        {
            ServiceId = 0,
            TotalHealthChecks = 10,
            SuccessfulHealthChecks = 10
        };

        var act = () => report.Validate();

        act.Should().Throw<InvalidOperationException>()
        .WithMessage("*Service ID must be valid*");
    }

    /// <summary>
    /// Tests that validation throws <see cref="InvalidOperationException"/> when success rate is negative.
    /// </summary>
    [Fact]
    public void Validate_NegativeSuccessRate_ThrowsInvalidOperationException()
    {
        var report = new ServiceHealthReport
        {
            ServiceId = 1,
            TotalHealthChecks = 10,
            SuccessfulHealthChecks = 5,
            HealthCheckSuccessRate = -5.0
        };

        var act = () => report.Validate();

        act.Should().Throw<InvalidOperationException>()
        .WithMessage("*Success rate must be between 0 and 100*");
    }

    /// <summary>
    /// Tests that validation throws <see cref="InvalidOperationException"/> when success rate exceeds 100.
    /// </summary>
    [Fact]
    public void Validate_SuccessRateAbove100_ThrowsInvalidOperationException()
    {
        var report = new ServiceHealthReport
        {
            ServiceId = 1,
            TotalHealthChecks = 10,
            SuccessfulHealthChecks = 15,
            HealthCheckSuccessRate = 150.0
        };

        var act = () => report.Validate();

        act.Should().Throw<InvalidOperationException>()
        .WithMessage("*Success rate must be between 0 and 100*");
    }

    /// <summary>
    /// Tests that validation throws <see cref="InvalidOperationException"/> when HealthStatus is null.
    /// </summary>
    [Fact]
    public void Validate_NullHealthStatus_ThrowsInvalidOperationException()
    {
        var report = new ServiceHealthReport
        {
            ServiceId = 1,
            HealthStatus = null!
        };

        var act = () => report.Validate();

        act.Should().Throw<InvalidOperationException>()
        .WithMessage("*Health status is required*");
    }

    /// <summary>
    /// Tests that validation throws <see cref="InvalidOperationException"/> when ResponseTimeMs is negative.
    /// </summary>
    [Fact]
    public void Validate_NegativeResponseTime_ThrowsInvalidOperationException()
    {
        var report = new ServiceHealthReport
        {
            ServiceId = 1,
            ResponseTimeMs = -100
        };

        var act = () => report.Validate();

        act.Should().Throw<InvalidOperationException>()
        .WithMessage("*Response time cannot be negative*");
    }

    /// <summary>
    /// Tests that <see cref="ServiceHealthReport.RecordCheckResult"/> increments success counters correctly
    /// when a health check succeeds, updates health status to "Healthy", and resets failure counters.
    /// </summary>
    [Fact]
    public void RecordCheckResult_SuccessfulCheck_IncrementsSuccessCounters()
    {
        var report = new ServiceHealthReport
        {
            ServiceId = 1,
            TotalHealthChecks = 5,
            SuccessfulHealthChecks = 3,
            FailedChecksInARow = 2,
            IsHealthy = false,
            HealthStatus = "Unhealthy"
        };

        report.RecordCheckResult(success: true, responseTimeMs: 150);

        report.TotalHealthChecks.Should().Be(6);
        report.SuccessfulHealthChecks.Should().Be(4);
        report.SuccessfulChecksInARow.Should().Be(1);
        report.FailedChecksInARow.Should().Be(0);
        report.IsHealthy.Should().BeTrue();
        report.HealthStatus.Should().Be("Healthy");
        report.ErrorMessage.Should().BeNull();
        report.StackTrace.Should().BeNull();
    }

    /// <summary>
    /// Tests that <see cref="ServiceHealthReport.RecordCheckResult"/> increments failure counters correctly
    /// when a health check fails, updates health status to "Degraded", and resets success counters.
    /// </summary>
    [Fact]
    public void RecordCheckResult_FailedCheck_IncrementsFailureCounters()
    {
        var report = new ServiceHealthReport
        {
            ServiceId = 1,
            TotalHealthChecks = 5,
            SuccessfulHealthChecks = 5,
            SuccessfulChecksInARow = 5,
            FailedChecksInARow = 0,
            IsHealthy = true,
            HealthStatus = "Healthy"
        };

        report.RecordCheckResult(success: false, responseTimeMs: 200, errorMessage: "Connection refused");

        report.TotalHealthChecks.Should().Be(6);
        report.SuccessfulHealthChecks.Should().Be(5);
        report.SuccessfulChecksInARow.Should().Be(0);
        report.FailedChecksInARow.Should().Be(1);
        report.IsHealthy.Should().BeTrue();
        report.HealthStatus.Should().Be("Degraded");
        report.ErrorMessage.Should().Be("Connection refused");
    }

    /// <summary>
    /// Tests that <see cref="ServiceHealthReport.RecordCheckResult"/> eventually marks a service as unhealthy
    /// after multiple consecutive failures.
    /// </summary>
    [Fact]
    public void RecordCheckResult_MultipleFailures_EventuallyMarksUnhealthy()
    {
        var report = new ServiceHealthReport
        {
            ServiceId = 1,
            TotalHealthChecks = 2,
            FailedChecksInARow = 2
        };

        report.RecordCheckResult(success: false, responseTimeMs: 200);
        report.IsHealthy.Should().BeFalse();
        report.HealthStatus.Should().Be("Unhealthy");
    }

    /// <summary>
    /// Tests that <see cref="ServiceHealthReport.RecordCheckResult"/> correctly calculates success rate
    /// after recording a successful health check.
    /// </summary>
    [Fact]
    public void RecordCheckResult_CalculatesSuccessRateCorrectly()
    {
        var report = new ServiceHealthReport
        {
            ServiceId = 1,
            TotalHealthChecks = 10,
            SuccessfulHealthChecks = 7
        };

        report.RecordCheckResult(success: true, responseTimeMs: 150);

        report.HealthCheckSuccessRate.Should().BeApproximately(72.73, 0.01);
    }

    /// <summary>
    /// Tests that <see cref="ServiceHealthReport.AddDiagnosticMessage"/> adds messages to the diagnostic messages list.
    /// </summary>
    [Fact]
    public void AddDiagnosticMessage_AddsMessageToList()
    {
        var report = new ServiceHealthReport
        {
            ServiceId = 1
        };

        report.AddDiagnosticMessage("Test message 1");
        report.AddDiagnosticMessage("Test message 2");

        report.DiagnosticMessages.Should().HaveCount(2);
        report.DiagnosticMessages[0].Should().Contain("Test message 1");
        report.DiagnosticMessages[1].Should().Contain("Test message 2");
    }

    /// <summary>
    /// Tests that <see cref="ServiceHealthReport.AddDiagnosticMessage"/> maintains a maximum of 10 messages
    /// by removing the oldest message when more than 10 are added.
    /// </summary>
    [Fact]
    public void AddDiagnosticMessage_MoreThan10Messages_RemovesOldest()
    {
        var report = new ServiceHealthReport
        {
            ServiceId = 1
        };

        for (int i = 0; i < 15; i++)
        {
            report.AddDiagnosticMessage($"Message {i}");
        }

        report.DiagnosticMessages.Should().HaveCount(10);
        report.DiagnosticMessages.Should().NotContain("Message 0");
        report.DiagnosticMessages.Should().Contain("Message 14");
    }

    /// <summary>
    /// Tests that <see cref="ServiceHealthReport.ShouldBeMarkedUnhealthy"/> returns true
    /// when FailedChecksInARow is 3 or more.
    /// </summary>
    [Fact]
    public void ShouldBeMarkedUnhealthy_AfterThreeFailures_ReturnsTrue()
    {
        var report = new ServiceHealthReport
        {
            ServiceId = 1,
            FailedChecksInARow = 3
        };

        report.ShouldBeMarkedUnhealthy.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="ServiceHealthReport.ShouldBeMarkedUnhealthy"/> returns false
    /// when FailedChecksInARow is less than 3.
    /// </summary>
    [Fact]
    public void ShouldBeMarkedUnhealthy_BeforeThreeFailures_ReturnsFalse()
    {
        var report = new ServiceHealthReport
        {
            ServiceId = 1,
            FailedChecksInARow = 2
        };

        report.ShouldBeMarkedUnhealthy.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="ServiceHealthReport.GetAvailabilityPercentage"/> returns the correct availability percentage
    /// based on TotalHealthChecks and SuccessfulHealthChecks.
    /// </summary>
    [Fact]
    public void GetAvailabilityPercentage_ReturnsCorrectPercentage()
    {
        var report = new ServiceHealthReport
        {
            ServiceId = 1,
            TotalHealthChecks = 20,
            SuccessfulHealthChecks = 15
        };

        report.GetAvailabilityPercentage.Should().BeApproximately(75.0, 0.01);
    }

    /// <summary>
    /// Tests that the default constructor sets appropriate default values for all properties.
    /// </summary>
    [Fact]
    public void DefaultConstructor_SetsDefaultValues()
    {
        var report = new ServiceHealthReport();

        report.Id.Should().Be(0);
        report.ServiceId.Should().Be(0);
        report.IsHealthy.Should().BeFalse();
        report.HealthStatus.Should().Be("Unknown");
        report.ResponseTimeMs.Should().Be(0);
        report.HttpStatusCode.Should().Be(0);
        report.ErrorMessage.Should().BeNull();
        report.StackTrace.Should().BeNull();
        report.SuccessfulChecksInARow.Should().Be(0);
        report.FailedChecksInARow.Should().Be(0);
        report.TotalHealthChecks.Should().Be(0);
        report.SuccessfulHealthChecks.Should().Be(0);
        report.HealthCheckSuccessRate.Should().Be(0.0);
        report.LastCheckAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        report.NextCheckScheduledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        report.ReportedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        report.DiagnosticMessages.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that a valid <see cref="ServiceHealthReport"/> with all properties set correctly
    /// does not throw validation exceptions.
    /// </summary>
    [Fact]
    public void Validate_ValidReportWithAllProperties_SetCorrectly()
    {
        var report = new ServiceHealthReport
        {
            ServiceId = 1,
            IsHealthy = true,
            HealthStatus = "Healthy",
            ResponseTimeMs = 100,
            HttpStatusCode = 200,
            TotalHealthChecks = 100,
            SuccessfulHealthChecks = 95,
            ErrorMessage = null,
            StackTrace = null
        };

        var act = () => report.Validate();

        act.Should().NotThrow();
    }
}