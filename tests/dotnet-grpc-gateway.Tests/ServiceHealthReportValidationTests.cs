// Copyright (c) 2024
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using DotNetGrpcGateway.Domain;
using Xunit;

namespace DotNetGrpcGateway.Tests;

public class ServiceHealthReportValidationTests
{
    private static ServiceHealthReport CreateValidReport()
    {
        var now = DateTime.UtcNow;

        return new ServiceHealthReport
        {
            ServiceId = 1,
            HealthStatus = "Healthy",
            ResponseTimeMs = 123,
            HttpStatusCode = 200,
            SuccessfulChecksInARow = 5,
            FailedChecksInARow = 0,
            TotalHealthChecks = 10,
            SuccessfulHealthChecks = 10,
            HealthCheckSuccessRate = 100,
            LastCheckAt = now,
            NextCheckScheduledAt = now.AddMinutes(10),
            ReportedAt = now,
            HealthCheckEndpoint = "https://example.com/health",
            IsHealthy = true,
            ErrorMessage = null,
            StackTrace = null,
            DiagnosticMessages = new List<string>()
        };
    }

    [Fact]
    public void ValidateReport_HappyPath_ReturnsEmpty()
    {
        // Arrange
        var report = CreateValidReport();

        // Act
        var problems = report.ValidateReport();

        // Assert
        Assert.Empty(problems);
        Assert.True(report.IsValid());
    }

    [Fact]
    public void EnsureValid_HappyPath_DoesNotThrow()
    {
        // Arrange
        var report = CreateValidReport();

        // Act & Assert
        var exception = Record.Exception(() => report.EnsureValid());
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateReport_Null_ThrowsArgumentNullException()
    {
        // Arrange
        ServiceHealthReport? report = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => report!.ValidateReport());
        Assert.Throws<ArgumentNullException>(() => report!.IsValid());
        Assert.Throws<ArgumentNullException>(() => report!.EnsureValid());
    }

    [Fact]
    public void IsValid_InvalidReport_ReturnsFalse()
    {
        // Arrange
        var report = CreateValidReport();
        report.ServiceId = 0; // invalid

        // Act
        var isValid = report.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void EnsureValid_InvalidReport_ThrowsArgumentException_WithProblemDetails()
    {
        // Arrange
        var report = CreateValidReport();
        report.ServiceId = -5; // invalid
        report.HealthStatus = ""; // invalid
        report.DiagnosticMessages = Enumerable.Range(0, 11).Select(i => $"msg{i}").ToList();

        // Act
        var ex = Assert.Throws<ArgumentException>(() => report.EnsureValid());

        // Assert
        var message = ex.Message;
        Assert.Contains("Service ID must be a positive integer", message);
        Assert.Contains("Health status must be specified", message);
        Assert.Contains("Diagnostic messages cannot exceed 10 entries", message);
    }

    [Fact]
    public void ValidateReport_BoundaryValues_Accepted()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var report = new ServiceHealthReport
        {
            ServiceId = 1,
            HealthStatus = new string('a', 50), // max length
            ResponseTimeMs = 0,
            HttpStatusCode = 0,
            SuccessfulChecksInARow = 0,
            FailedChecksInARow = 0,
            TotalHealthChecks = 0,
            SuccessfulHealthChecks = 0,
            HealthCheckSuccessRate = 0,
            LastCheckAt = now.AddYears(-1), // exactly one year ago
            NextCheckScheduledAt = now.AddYears(1), // exactly one year ahead
            ReportedAt = now.AddMinutes(5), // exactly 5 minutes in future (allowed)
            HealthCheckEndpoint = "relative/path", // relative URI allowed
            IsHealthy = false,
            ErrorMessage = "Something went wrong",
            StackTrace = "stack trace",
            DiagnosticMessages = new List<string>()
        };

        // Act
        var problems = report.ValidateReport();

        // Assert
        Assert.Empty(problems);
    }
}
