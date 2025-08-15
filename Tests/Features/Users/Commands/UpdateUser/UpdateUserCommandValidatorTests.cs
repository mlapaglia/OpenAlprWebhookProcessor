using FluentValidation.TestHelper;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Commands.UpdateUser;
using Tests.TestHelpers;

namespace Tests.Features.Users.Commands.UpdateUser
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class UpdateUserCommandValidatorTests : TestBase
    {
        private UpdateUserCommandValidator _validator;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _validator = new UpdateUserCommandValidator();
        }

        #region Id Validation Tests

        [Test]
        public void Id_WhenZero_ShouldHaveValidationError()
        {
            // Arrange
            var command = new UpdateUserCommand(0, "John", "Doe", "johndoe", "password123");

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Id)
                .WithErrorMessage("User ID must be greater than 0");
        }

        [Test]
        public void Id_WhenNegative_ShouldHaveValidationError()
        {
            // Arrange
            var command = new UpdateUserCommand(-1, "John", "Doe", "johndoe", "password123");

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Id)
                .WithErrorMessage("User ID must be greater than 0");
        }

        [Test]
        public void Id_WhenPositive_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdateUserCommand(1, "John", "Doe", "johndoe", "password123");

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }

        #endregion

        #region FirstName Validation Tests

        [Test]
        public void FirstName_WhenNull_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdateUserCommand(1, null, "Doe", "johndoe", "password123");

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
        }

        [Test]
        public void FirstName_WhenEmpty_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdateUserCommand(1, "", "Doe", "johndoe", "password123");

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
        }

        [Test]
        public void FirstName_WhenWhitespace_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdateUserCommand(1, "   ", "Doe", "johndoe", "password123");

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
        }

        [Test]
        public void FirstName_WhenValid_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdateUserCommand(1, "John", "Doe", "johndoe", "password123");

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
        }

        [Test]
        public void FirstName_WhenExactly100Characters_ShouldNotHaveValidationError()
        {
            // Arrange
            var firstName = new string('A', 100); // Exactly 100 characters
            var command = new UpdateUserCommand(1, firstName, "Doe", "johndoe", "password123");

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
        }

        [Test]
        public void FirstName_WhenExceedsMaxLength_ShouldHaveValidationError()
        {
            // Arrange
            var firstName = new string('A', 101); // 101 characters
            var command = new UpdateUserCommand(1, firstName, "Doe", "johndoe", "password123");

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.FirstName)
                .WithErrorMessage("First name must be less than 100 characters");
        }

        #endregion

        #region LastName Validation Tests

        [Test]
        public void LastName_WhenNull_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdateUserCommand(1, "John", null, "johndoe", "password123");

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.LastName);
        }

        [Test]
        public void LastName_WhenEmpty_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdateUserCommand(1, "John", "", "johndoe", "password123");

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.LastName);
        }

        [Test]
        public void LastName_WhenWhitespace_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdateUserCommand(1, "John", "   ", "johndoe", "password123");

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.LastName);
        }

        [Test]
        public void LastName_WhenValid_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdateUserCommand(1, "John", "Doe", "johndoe", "password123");

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.LastName);
        }

        [Test]
        public void LastName_WhenExactly100Characters_ShouldNotHaveValidationError()
        {
            // Arrange
            var lastName = new string('B', 100); // Exactly 100 characters
            var command = new UpdateUserCommand(1, "John", lastName, "johndoe", "password123");

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.LastName);
        }

        [Test]
        public void LastName_WhenExceedsMaxLength_ShouldHaveValidationError()
        {
            // Arrange
            var lastName = new string('B', 101); // 101 characters
            var command = new UpdateUserCommand(1, "John", lastName, "johndoe", "password123");

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.LastName)
                .WithErrorMessage("Last name must be less than 100 characters");
        }

        #endregion

        #region Username Validation Tests

        [Test]
        public void Username_WhenNull_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdateUserCommand(1, "John", "Doe", null, "password123");

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.Username);
        }

        [Test]
        public void Username_WhenEmpty_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdateUserCommand(1, "John", "Doe", "", "password123");

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.Username);
        }

        [Test]
        public void Username_WhenWhitespace_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdateUserCommand(1, "John", "Doe", "   ", "password123");

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.Username);
        }

        [Test]
        public void Username_WhenValid_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdateUserCommand(1, "John", "Doe", "johndoe", "password123");

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.Username);
        }

        [Test]
        public void Username_WhenExactly50Characters_ShouldNotHaveValidationError()
        {
            // Arrange
            var username = new string('u', 50); // Exactly 50 characters
            var command = new UpdateUserCommand(1, "John", "Doe", username, "password123");

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.Username);
        }

        [Test]
        public void Username_WhenExceedsMaxLength_ShouldHaveValidationError()
        {
            // Arrange
            var username = new string('u', 51); // 51 characters
            var command = new UpdateUserCommand(1, "John", "Doe", username, "password123");

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Username)
                .WithErrorMessage("Username must be less than 50 characters");
        }

        #endregion

        #region Password Validation Tests

        [Test]
        public void Password_WhenNull_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdateUserCommand(1, "John", "Doe", "johndoe", null);

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.Password);
        }

        [Test]
        public void Password_WhenEmpty_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdateUserCommand(1, "John", "Doe", "johndoe", "");

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.Password);
        }

        [Test]
        public void Password_WhenWhitespace_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdateUserCommand(1, "John", "Doe", "johndoe", "   ");

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.Password);
        }

        [Test]
        public void Password_WhenTooShort_ShouldHaveValidationError()
        {
            // Arrange
            var command = new UpdateUserCommand(1, "John", "Doe", "johndoe", "12345"); // 5 characters

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Password)
                .WithErrorMessage("Password must be at least 6 characters");
        }

        [Test]
        public void Password_WhenExactly6Characters_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdateUserCommand(1, "John", "Doe", "johndoe", "123456"); // Exactly 6 characters

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.Password);
        }

        [Test]
        public void Password_WhenValid_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdateUserCommand(1, "John", "Doe", "johndoe", "password123");

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.Password);
        }

        #endregion

        #region Integration Tests

        [Test]
        public void ValidCommand_ShouldNotHaveAnyValidationErrors()
        {
            // Arrange
            var command = new UpdateUserCommand(1, "John", "Doe", "johndoe", "password123");

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Test]
        public void CommandWithMultipleErrors_ShouldHaveAllValidationErrors()
        {
            // Arrange
            var command = new UpdateUserCommand(
                0, // Invalid ID
                new string('A', 101), // FirstName too long
                new string('B', 101), // LastName too long
                new string('u', 51), // Username too long
                "12345" // Password too short
            );

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Id);
            result.ShouldHaveValidationErrorFor(x => x.FirstName);
            result.ShouldHaveValidationErrorFor(x => x.LastName);
            result.ShouldHaveValidationErrorFor(x => x.Username);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Test]
        public void CommandWithNullOptionalFields_ShouldOnlyValidateId()
        {
            // Arrange
            var command = new UpdateUserCommand(1, null, null, null, null);

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
            result.ShouldNotHaveValidationErrorFor(x => x.LastName);
            result.ShouldNotHaveValidationErrorFor(x => x.Username);
            result.ShouldNotHaveValidationErrorFor(x => x.Password);
            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }

        #endregion
    }
}
