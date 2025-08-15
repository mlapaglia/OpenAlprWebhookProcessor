using FluentValidation.TestHelper;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Commands.CreateUser;

namespace Tests.Features.Users.Commands
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class CreateUserCommandValidatorTests
    {
        private CreateUserCommandValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _validator = new CreateUserCommandValidator();
        }

        [Test]
        public void Should_Have_Error_When_FirstName_Is_Empty()
        {
            var command = new CreateUserCommand("", "Doe", "johndoe", "password123");
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.FirstName)
                .WithErrorMessage("First name is required");
        }

        [Test]
        public void Should_Have_Error_When_FirstName_Is_Too_Long()
        {
            var command = new CreateUserCommand(new string('a', 101), "Doe", "johndoe", "password123");
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.FirstName)
                .WithErrorMessage("First name must be less than 100 characters");
        }

        [Test]
        public void Should_Have_Error_When_LastName_Is_Empty()
        {
            var command = new CreateUserCommand("John", "", "johndoe", "password123");
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.LastName)
                .WithErrorMessage("Last name is required");
        }

        [Test]
        public void Should_Have_Error_When_LastName_Is_Too_Long()
        {
            var command = new CreateUserCommand("John", new string('a', 101), "johndoe", "password123");
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.LastName)
                .WithErrorMessage("Last name must be less than 100 characters");
        }

        [Test]
        public void Should_Have_Error_When_Username_Is_Empty()
        {
            var command = new CreateUserCommand("John", "Doe", "", "password123");
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Username)
                .WithErrorMessage("Username is required");
        }

        [Test]
        public void Should_Have_Error_When_Username_Is_Too_Long()
        {
            var command = new CreateUserCommand("John", "Doe", new string('a', 51), "password123");
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Username)
                .WithErrorMessage("Username must be less than 50 characters");
        }

        [Test]
        public void Should_Have_Error_When_Password_Is_Empty()
        {
            var command = new CreateUserCommand("John", "Doe", "johndoe", "");
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Password)
                .WithErrorMessage("Password is required");
        }

        [Test]
        public void Should_Have_Error_When_Password_Is_Null()
        {
            var command = new CreateUserCommand("John", "Doe", "johndoe", null);
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Password)
                .WithErrorMessage("Password is required");
        }

        [Test]
        public void Should_Have_Error_When_Password_Is_Whitespace()
        {
            var command = new CreateUserCommand("John", "Doe", "johndoe", "   ");
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Password)
                .WithErrorMessage("Password is required");
        }

        [Test]
        public void Should_Have_Error_When_Password_Is_Too_Short()
        {
            var command = new CreateUserCommand("John", "Doe", "johndoe", "12345");
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Password)
                .WithErrorMessage("Password must be at least 6 characters");
        }

        [Test]
        public void Should_Not_Have_Error_When_Command_Is_Valid()
        {
            var command = new CreateUserCommand("John", "Doe", "johndoe", "password123");
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
} 