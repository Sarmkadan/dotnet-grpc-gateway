#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using DotNetGrpcGateway.Domain;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class ServiceHealthReportTests
{
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

        report.HealthCheckSuccessRate.Should().BeApproximately(77.77, 0.01);
    }

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
