using System;
using DotNetGrpcGateway.Domain;
using Xunit;

namespace DotNetGrpcGateway.Tests
{
    public class ServiceEndpointExtensionsTests
    {
        #region IsAvailable

        [Fact]
        public void IsAvailable_ReturnsTrue_WhenHealthyAndWeightPositive()
        {
            var endpoint = new ServiceEndpoint
            {
                IsHealthy = true,
                Weight = 5
            };

            var result = endpoint.IsAvailable();

            Assert.True(result);
        }

        [Fact]
        public void IsAvailable_ReturnsFalse_WhenUnhealthy()
        {
            var endpoint = new ServiceEndpoint
            {
                IsHealthy = false,
                Weight = 5
            };

            var result = endpoint.IsAvailable();

            Assert.False(result);
        }

        [Fact]
        public void IsAvailable_ReturnsFalse_WhenWeightIsZero()
        {
            var endpoint = new ServiceEndpoint
            {
                IsHealthy = true,
                Weight = 0
            };

            var result = endpoint.IsAvailable();

            Assert.False(result);
        }

        [Fact]
        public void IsAvailable_ThrowsArgumentNullException_WhenEndpointIsNull()
        {
            ServiceEndpoint? endpoint = null;

            Assert.Throws<ArgumentNullException>(() => endpoint!.IsAvailable());
        }

        #endregion

        #region GetSuccessRate

        [Fact]
        public void GetSuccessRate_ReturnsZero_WhenNoRequestsHandled()
        {
            var endpoint = new ServiceEndpoint
            {
                TotalRequestsHandled = 0,
                FailedRequestsCount = 0
            };

            var rate = endpoint.GetSuccessRate();

            Assert.Equal(0.0, rate);
        }

        [Fact]
        public void GetSuccessRate_ReturnsOne_WhenAllRequestsSuccessful()
        {
            var endpoint = new ServiceEndpoint
            {
                TotalRequestsHandled = 10,
                FailedRequestsCount = 0
            };

            var rate = endpoint.GetSuccessRate();

            Assert.Equal(1.0, rate);
        }

        [Fact]
        public void GetSuccessRate_CalculatesCorrectRate_WithFailures()
        {
            var endpoint = new ServiceEndpoint
            {
                TotalRequestsHandled = 20,
                FailedRequestsCount = 5
            };

            var rate = endpoint.GetSuccessRate();

            Assert.Equal(0.75, rate, 5); // 15/20 = 0.75
        }

        [Fact]
        public void GetSuccessRate_HandlesNegativeSuccessfulCountGracefully()
        {
            // This situation shouldn't happen in production but the method guards against it.
            var endpoint = new ServiceEndpoint
            {
                TotalRequestsHandled = 5,
                FailedRequestsCount = 10 // failed > total
            };

            var rate = endpoint.GetSuccessRate();

            Assert.Equal(0.0, rate);
        }

        [Fact]
        public void GetSuccessRate_ThrowsArgumentNullException_WhenEndpointIsNull()
        {
            ServiceEndpoint? endpoint = null;

            Assert.Throws<ArgumentNullException>(() => endpoint!.GetSuccessRate());
        }

        #endregion

        #region IsRecentlyUsed

        [Fact]
        public void IsRecentlyUsed_ReturnsTrue_WhenLastUsedWithinThreshold()
        {
            var now = DateTime.UtcNow;
            var endpoint = new ServiceEndpoint
            {
                LastUsedAt = now.AddSeconds(-30)
            };

            var result = endpoint.IsRecentlyUsed(TimeSpan.FromMinutes(1));

            Assert.True(result);
        }

        [Fact]
        public void IsRecentlyUsed_ReturnsFalse_WhenLastUsedOutsideThreshold()
        {
            var now = DateTime.UtcNow;
            var endpoint = new ServiceEndpoint
            {
                LastUsedAt = now.AddHours(-2)
            };

            var result = endpoint.IsRecentlyUsed(TimeSpan.FromMinutes(30));

            Assert.False(result);
        }

        [Fact]
        public void IsRecentlyUsed_ThrowsArgumentNullException_WhenEndpointIsNull()
        {
            ServiceEndpoint? endpoint = null;

            Assert.Throws<ArgumentNullException>(() => endpoint!.IsRecentlyUsed(TimeSpan.FromSeconds(10)));
        }

        [Fact]
        public void IsRecentlyUsed_ThrowsArgumentOutOfRangeException_WhenThresholdIsZeroOrNegative()
        {
            var endpoint = new ServiceEndpoint
            {
                LastUsedAt = DateTime.UtcNow
            };

            Assert.Throws<ArgumentOutOfRangeException>(() => endpoint.IsRecentlyUsed(TimeSpan.Zero));
            Assert.Throws<ArgumentOutOfRangeException>(() => endpoint.IsRecentlyUsed(TimeSpan.FromSeconds(-5)));
        }

        #endregion
    }
}
