using System.Net;
using DotNetGrpcGateway.Exceptions;
using DotNetGrpcGateway.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetGrpcGateway.Tests
{
    public class ErrorHandlingMiddlewareTests
    {
        private readonly Mock<ILogger<ErrorHandlingMiddleware>> _mockLogger;
        private readonly Mock<RequestDelegate> _mockNext;

        public ErrorHandlingMiddlewareTests()
        {
            _mockLogger = new Mock<ILogger<ErrorHandlingMiddleware>>();
            _mockNext = new Mock<RequestDelegate>();
        }

        [Fact]
        public async Task InvokeAsync_WithSuccessfulRequest_InvokesNextDelegate()
        {
            // Arrange
            var context = new DefaultHttpContext();
            _mockNext.Setup(n => n.Invoke(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            var middleware = new ErrorHandlingMiddleware(_mockNext.Object, _mockLogger.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            _mockNext.Verify(n => n.Invoke(context), Times.Once);
            context.Response.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task InvokeAsync_WithObjectDisposedException_LogsDebugAndDoesNotThrow()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var exception = new ObjectDisposedException("stream", "Test exception");

            _mockNext.Setup(n => n.Invoke(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            var middleware = new ErrorHandlingMiddleware(_mockNext.Object, _mockLogger.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<ObjectDisposedException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
            context.Response.StatusCode.Should().Be(200); // No error response written
        }

        [Fact]
        public async Task InvokeAsync_WithOperationCanceledException_LogsDebugAndDoesNotThrow()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.RequestAborted = new CancellationToken(true); // Mark as cancelled

            var exception = new OperationCanceledException("Request cancelled");
            _mockNext.Setup(n => n.Invoke(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            var middleware = new ErrorHandlingMiddleware(_mockNext.Object, _mockLogger.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<OperationCanceledException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
            context.Response.StatusCode.Should().Be(200); // No error response written
        }

        [Fact]
        public async Task InvokeAsync_WithGatewayException_SetsBadRequestStatusCode()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var exception = new GatewayException("Test gateway error", "TEST_ERROR", 400);

            _mockNext.Setup(n => n.Invoke(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            var middleware = new ErrorHandlingMiddleware(_mockNext.Object, _mockLogger.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be(400);
            context.Response.ContentType.Should().StartWith("application/json");
        }

        [Fact]
        public async Task InvokeAsync_WithGatewayExceptionWithDetails_SetsInternalServerErrorStatusCode()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var exception = new GatewayException("Test gateway error with details");
            exception.AddDetail("key1", "value1");
            exception.AddDetail("key2", 123);
            exception.AddDetail("key3", true);

            _mockNext.Setup(n => n.Invoke(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            var middleware = new ErrorHandlingMiddleware(_mockNext.Object, _mockLogger.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be(500);
            context.Response.ContentType.Should().StartWith("application/json");
        }

        [Fact]
        public async Task InvokeAsync_WithArgumentException_SetsBadRequestStatusCode()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var exception = new ArgumentException("Invalid argument");

            _mockNext.Setup(n => n.Invoke(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            var middleware = new ErrorHandlingMiddleware(_mockNext.Object, _mockLogger.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            context.Response.ContentType.Should().StartWith("application/json");
        }

        [Fact]
        public async Task InvokeAsync_WithUnauthorizedAccessException_SetsUnauthorizedStatusCode()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var exception = new UnauthorizedAccessException("Not authorized");

            _mockNext.Setup(n => n.Invoke(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            var middleware = new ErrorHandlingMiddleware(_mockNext.Object, _mockLogger.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
            context.Response.ContentType.Should().StartWith("application/json");
        }

        [Fact]
        public async Task InvokeAsync_WithKeyNotFoundException_SetsNotFoundStatusCode()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var exception = new KeyNotFoundException("Item not found");

            _mockNext.Setup(n => n.Invoke(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            var middleware = new ErrorHandlingMiddleware(_mockNext.Object, _mockLogger.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            context.Response.ContentType.Should().StartWith("application/json");
        }

        [Fact]
        public async Task InvokeAsync_WithGenericException_SetsInternalServerErrorStatusCode()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var exception = new Exception("Generic error");

            _mockNext.Setup(n => n.Invoke(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            var middleware = new ErrorHandlingMiddleware(_mockNext.Object, _mockLogger.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            context.Response.ContentType.Should().StartWith("application/json");
        }

        [Fact]
        public async Task InvokeAsync_WithNullNextDelegate_ThrowsArgumentNullException()
        {
            // Act and Assert
            Func<Task> act = () => Task.FromResult(new ErrorHandlingMiddleware(null!, _mockLogger.Object));
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task InvokeAsync_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act and Assert
            Func<Task> act = () => Task.FromResult(new ErrorHandlingMiddleware(_mockNext.Object, null!));
            await act.Should().ThrowAsync<ArgumentNullException>();
        }
    }
}