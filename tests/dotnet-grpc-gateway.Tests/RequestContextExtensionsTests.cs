using DotNetGrpcGateway.Infrastructure;
using Xunit;

namespace DotNetGrpcGateway.Tests
{
    /// <summary>
    /// Test class for RequestContextExtensions.
    /// </summary>
    public class RequestContextExtensionsTests
    {
        /// <summary>
        /// Tests that HasUserId returns true when UserId is a non-empty string.
        /// </summary>
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

        /// <summary>
        /// Tests that HasUserId returns false when UserId is an empty string.
        /// </summary>
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

        /// <summary>
        /// Tests that HasUserId returns false when UserId is null.
        /// </summary>
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

        /// <summary>
        /// Tests that HasUserId throws ArgumentNullException when the RequestContext is null.
        /// </summary>
        [Fact]
        public void HasUserId_WithNullRequestContext_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ((RequestContext)null).HasUserId());
        }

        /// <summary>
        /// Tests that GetClientInfo returns the expected string when ClientIp and UserId are set.
        /// </summary>
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

        /// <summary>
        /// Tests that GetClientInfo throws ArgumentNullException when the RequestContext is null.
        /// </summary>
        [Fact]
        public void GetClientInfo_WithNullRequestContext_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ((RequestContext)null).GetClientInfo());
        }

        /// <summary>
        /// Tests that SetStartTime sets the start time correctly.
        /// </summary>
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

        /// <summary>
        /// Tests that SetStartTime throws ArgumentNullException when the RequestContext is null.
        /// </summary>
        [Fact]
        public void SetStartTime_WithNullRequestContext_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ((RequestContext)null).SetStartTime(DateTime.Now));
        }

        /// <summary>
        /// Tests that GetStartTime returns the previously set start time.
        /// </summary>
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

        /// <summary>
        /// Tests that GetStartTime throws ArgumentNullException when the RequestContext is null.
        /// </summary>
        [Fact]
        public void GetStartTime_WithNullRequestContext_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ((RequestContext)null).GetStartTime());
        }

        /// <summary>
        /// Tests that GetStartTime returns null when no start time has been set.
        /// </summary>
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