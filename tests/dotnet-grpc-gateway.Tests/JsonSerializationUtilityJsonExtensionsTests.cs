using Xunit;
using System.Text.Json;
using DotNetGrpcGateway.Utilities;

namespace DotNetGrpcGateway.Tests
{
    public class JsonSerializationUtilityJsonExtensionsTests
    {
        [Fact]
        public void ToJson_HappyPath_ReturnsJsonString()
        {
            // Arrange
            var jsonSerializationUtility = new JsonSerializationUtility();
            var expectedJson = "{}";

            // Act
            var actualJson = jsonSerializationUtility.ToJson();

            // Assert
            Assert.Equal(expectedJson, actualJson);
        }

        [Fact]
        public void FromJson_HappyPath_ReturnsJsonSerializationUtilityInstance()
        {
            // Arrange
            var json = "{}";

            // Act
            var actualJsonSerializationUtility = JsonSerializationUtilityJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(actualJsonSerializationUtility);
        }

        [Fact]
        public void TryFromJson_HappyPath_ReturnsTrue()
        {
            // Arrange
            var json = "{}";

            // Act
            var actualResult = JsonSerializationUtilityJsonExtensions.TryFromJson(json, out var actualJsonSerializationUtility);

            // Assert
            Assert.True(actualResult);
            Assert.NotNull(actualJsonSerializationUtility);
        }

        [Fact]
        public void ToJson_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => JsonSerializationUtilityJsonExtensions.ToJson(null));
        }

        [Fact]
        public void FromJson_NullInput_ReturnsNull()
        {
            // Act
            var actualJsonSerializationUtility = JsonSerializationUtilityJsonExtensions.FromJson(null);

            // Assert
            Assert.Null(actualJsonSerializationUtility);
        }

        [Fact]
        public void TryFromJson_NullInput_ReturnsFalse()
        {
            // Act
            var actualResult = JsonSerializationUtilityJsonExtensions.TryFromJson(null, out var actualJsonSerializationUtility);

            // Assert
            Assert.False(actualResult);
            Assert.Null(actualJsonSerializationUtility);
        }
    }
}
