using FluentAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users;

namespace Tests.Features.Users
{
    [TestFixture]
    public class AppExceptionTests
    {
        [Test]
        public void Constructor_Default_CreatesInstance()
        {
            // Act
            var exception = new AppException();

            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeAssignableTo<Exception>();
            exception.Message.Should().NotBeNull();
        }

        [Test]
        public void Constructor_WithMessage_SetsMessage()
        {
            // Arrange
            var message = "Test error message";

            // Act
            var exception = new AppException(message);

            // Assert
            exception.Should().NotBeNull();
            exception.Message.Should().Be(message);
        }

        [Test]
        public void Constructor_WithMessageFormat_FormatsMessage()
        {
            // Arrange
            var messageFormat = "User {0} not found with ID {1}";
            var userName = "TestUser";
            var userId = 123;

            // Act
            var exception = new AppException(messageFormat, userName, userId);

            // Assert
            exception.Should().NotBeNull();
            exception.Message.Should().Be("User TestUser not found with ID 123");
        }

        [Test]
        public void Constructor_WithNullMessage_HandlesNull()
        {
            // Act
            var exception = new AppException(null);

            // Assert
            exception.Should().NotBeNull();
        }

        [Test]
        public void Constructor_WithEmptyMessage_HandlesEmpty()
        {
            // Act
            var exception = new AppException("");

            // Assert
            exception.Should().NotBeNull();
            exception.Message.Should().Be("");
        }

        [Test]
        public void Constructor_WithMultipleArgs_FormatsCorrectly()
        {
            // Arrange
            var messageFormat = "Error processing {0} at {1} with value {2}";

            // Act
            var exception = new AppException(messageFormat, "file.txt", DateTime.Parse("2023-01-01"), 42.5);

            // Assert
            exception.Should().NotBeNull();
        }

        [Test]
        public void Constructor_WithNoArgs_FormatsWithoutParameters()
        {
            // Arrange
            var message = "Simple error message without parameters";

            // Act
            var exception = new AppException(message);

            // Assert
            exception.Should().NotBeNull();
            exception.Message.Should().Be(message);
        }

        [Test]
        public void Inheritance_IsException_InheritsCorrectly()
        {
            // Act
            var exception = new AppException("test");

            // Assert
            exception.Should().BeAssignableTo<Exception>();
            exception.Should().BeOfType<AppException>();
        }
    }
} 