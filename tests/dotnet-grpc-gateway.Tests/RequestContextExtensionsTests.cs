using DotNetGrpcGateway.Infrastructure;
using Xunit;

namespace DotNetGrpcGateway.Tests
{
    public class RequestContextExtensionsTests
    {
        [Fact]
        public void HasUserId_WithValidUserId_ReturnsTrue()
        {
            // Arrange
            var requestContext = new RequestContext { UserId = "test-user" };

            // Act
            var result = requestContext.HasUserId();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasUserId_WithEmptyUserId_ReturnsFalse()
        {
            // Arrange
            var requestContext = new RequestContext { UserId = string.Empty };

            // Act
            var result = requestContext.HasUserId();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasUserId_WithNullUserId_ReturnsFalse()
        {
            // Arrange
            var requestContext = new RequestContext { UserId = null };

            // Act
            var result = requestContext.HasUserId();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasUserId_WithNullRequestContext_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ((RequestContext)null).HasUserId());
        }

        [Fact]
        public void GetClientInfo_WithValidRequestContext_ReturnsClientInfo()
        {
            // Arrange
            var requestContext = new RequestContext { ClientIp = "192.168.1.1", UserId = "test-user" };

            // Act
            var result = requestContext.GetClientInfo();

            // Assert
            Assert.Equal("Client IP: 192.168.1.1, User ID: test-user", result);
        }

        [Fact]
        public void GetClientInfo_WithNullRequestContext_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ((RequestContext)null).GetClientInfo());
        }

        [Fact]
        public void SetStartTime_WithValidRequestContextAndStartTime_SetsStartTime()
        {
            // Arrange
            var requestContext = new RequestContext();
            var startTime = DateTime.Now;

            // Act
            requestContext.SetStartTime(startTime);

            // Assert
            Assert.Equal(startTime, requestContext.GetStartTime());
        }

        [Fact]
        public void SetStartTime_WithNullRequestContext_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ((RequestContext)null).SetStartTime(DateTime.Now));
        }

        [Fact]
        public void GetStartTime_WithValidRequestContextAndStartTime_ReturnsStartTime()
        {
            // Arrange
            var requestContext = new RequestContext();
            var startTime = DateTime.Now;
            requestContext.SetStartTime(startTime);

            // Act
            var result = requestContext.GetStartTime();

            // Assert
            Assert.Equal(startTime, result);
        }

        [Fact]
        public void GetStartTime_WithNullRequestContext_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ((RequestContext)null).GetStartTime());
        }

        [Fact]
        public void GetStartTime_WithNoStartTime_ReturnsNull()
        {
            // Arrange
            var requestContext = new RequestContext();

            // Act
            var result = requestContext.GetStartTime();

            // Assert
            Assert.Null(result);
        }
    }
}
