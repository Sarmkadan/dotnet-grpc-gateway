#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using DotNetGrpcGateway.Controllers;
using DotNetGrpcGateway.Services;
using DotNetGrpcGateway.Domain;

namespace DotNetGrpcGateway.Tests;

public class RequestLogsControllerTests
{
    private readonly Mock<IRequestLogService> _logServiceMock;
    private readonly Mock<ILogger<RequestLogsController>> _loggerMock;
    private readonly RequestLogsController _controller;

    public RequestLogsControllerTests()
    {
        _logServiceMock = new Mock<IRequestLogService>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<RequestLogsController>>(MockBehavior.Strict);
        _controller = new RequestLogsController(_logServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void GetRecent_WithValidLimit_ReturnsOkResultContainingEntries()
    {
        // Arrange
        var expected = new List<RequestLogEntry>
        {
            Mock.Of<RequestLogEntry>(),
            Mock.Of<RequestLogEntry>()
        };
        _logServiceMock.Setup(s => s.GetRecent(10)).Returns(expected);

        // Act
        var result = _controller.GetRecent(10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsAssignableFrom<IReadOnlyList<RequestLogEntry>>(okResult.Value);
        Assert.Equal(expected, actual);
        _logServiceMock.VerifyAll();
    }

    [Fact]
    public void GetRecent_WithInvalidLimit_UsesDefaultLimit()
    {
        // Arrange
        int capturedLimit = -1;
        var expected = new List<RequestLogEntry>();
        _logServiceMock
            .Setup(s => s.GetRecent(It.IsAny<int>()))
            .Callback<int>(l => capturedLimit = l)
            .Returns(expected);

        // Act
        var result = _controller.GetRecent(-5); // invalid limit

        // Assert
        Assert.Equal(50, capturedLimit); // controller should replace with default 50
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, okResult.Value);
        _logServiceMock.VerifyAll();
    }

    [Fact]
    public void Search_WhenFromIsAfterTo_ReturnsBadRequest()
    {
        // Arrange
        var from = DateTime.UtcNow;
        var to = from.AddHours(-1); // from > to

        // Act
        var result = _controller.Search(null, null, from, to, 10);

        // Assert
        var badResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("'from' must be before 'to'", badResult.Value);
        // No service call should be made
        _logServiceMock.Verify(s => s.Search(It.IsAny<string?>(),
                                            It.IsAny<int?>(),
                                            It.IsAny<DateTime?>(),
                                            It.IsAny<DateTime?>(),
                                            It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void Search_WithValidParameters_ReturnsOkResult()
    {
        // Arrange
        var expected = new List<RequestLogEntry> { Mock.Of<RequestLogEntry>() };
        _logServiceMock
            .Setup(s => s.Search("GET", 200, null, null, 25))
            .Returns(expected);

        // Act
        var result = _controller.Search("GET", 200, null, null, 25);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsAssignableFrom<IReadOnlyList<RequestLogEntry>>(okResult.Value);
        Assert.Equal(expected, actual);
        _logServiceMock.VerifyAll();
    }

    [Fact]
    public void GetSummary_ReturnsOkResultWithSummary()
    {
        // Arrange
        var summary = Mock.Of<RequestLogSummary>();
        _logServiceMock.Setup(s => s.GetSummary()).Returns(summary);

        // Act
        var result = _controller.GetSummary();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(summary, okResult.Value);
        _logServiceMock.VerifyAll();
    }

    [Fact]
    public void Clear_InvokesServiceAndLogs_ReturnsNoContent()
    {
        // Arrange
        _logServiceMock.Setup(s => s.Clear());
        // Setup logger to accept the LogInformation call (no verification of message content needed)
        _loggerMock.Setup(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                null,
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        // Act
        var result = _controller.Clear();

        // Assert
        Assert.IsType<NoContentResult>(result);
        _logServiceMock.Verify(s => s.Clear(), Times.Once);
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.Once);
    }
}
