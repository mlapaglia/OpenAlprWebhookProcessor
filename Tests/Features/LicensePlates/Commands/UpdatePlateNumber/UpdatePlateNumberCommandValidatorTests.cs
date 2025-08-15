using FluentValidation.TestHelper;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.LicensePlates.Commands.UpdatePlateNumber;
using System;
using Tests.TestHelpers;

namespace Tests.Features.LicensePlates.Commands.UpdatePlateNumber
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class UpdatePlateNumberCommandValidatorTests : TestBase
    {
        private UpdatePlateNumberCommandValidator _validator;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _validator = new UpdatePlateNumberCommandValidator();
        }

        [Test]
        public void PlateId_WhenEmpty_ShouldHaveValidationError()
        {
            // Arrange
            var command = new UpdatePlateNumberCommand
            {
                PlateId = Guid.Empty,
                PlateNumber = "ABC123"
            };

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.PlateId)
                .WithErrorMessage("Plate ID is required");
        }

        [Test]
        public void PlateId_WhenValid_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdatePlateNumberCommand
            {
                PlateId = Guid.NewGuid(),
                PlateNumber = "ABC123"
            };

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.PlateId);
        }

        [Test]
        public void PlateNumber_WhenEmpty_ShouldHaveValidationError()
        {
            // Arrange
            var command = new UpdatePlateNumberCommand
            {
                PlateId = Guid.NewGuid(),
                PlateNumber = ""
            };

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.PlateNumber)
                .WithErrorMessage("Plate number is required");
        }

        [Test]
        public void PlateNumber_WhenNull_ShouldHaveValidationError()
        {
            // Arrange
            var command = new UpdatePlateNumberCommand
            {
                PlateId = Guid.NewGuid(),
                PlateNumber = null
            };

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.PlateNumber)
                .WithErrorMessage("Plate number is required");
        }

        [Test]
        public void PlateNumber_WhenWhitespace_ShouldHaveValidationError()
        {
            // Arrange
            var command = new UpdatePlateNumberCommand
            {
                PlateId = Guid.NewGuid(),
                PlateNumber = "   "
            };

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.PlateNumber)
                .WithErrorMessage("Plate number is required");
        }

        [Test]
        public void PlateNumber_WhenValid_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdatePlateNumberCommand
            {
                PlateId = Guid.NewGuid(),
                PlateNumber = "ABC123"
            };

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.PlateNumber);
        }

        [Test]
        public void PlateNumber_WhenExactly20Characters_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdatePlateNumberCommand
            {
                PlateId = Guid.NewGuid(),
                PlateNumber = "12345678901234567890" // Exactly 20 characters
            };

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.PlateNumber);
        }

        [Test]
        public void PlateNumber_WhenExceedsMaxLength_ShouldHaveValidationError()
        {
            // Arrange
            var command = new UpdatePlateNumberCommand
            {
                PlateId = Guid.NewGuid(),
                PlateNumber = "123456789012345678901" // 21 characters
            };

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.PlateNumber)
                .WithErrorMessage("Plate number must be less than 20 characters");
        }

        [Test]
        public void PlateNumber_WhenVeryLong_ShouldHaveValidationError()
        {
            // Arrange
            var command = new UpdatePlateNumberCommand
            {
                PlateId = Guid.NewGuid(),
                PlateNumber = new string('A', 50) // 50 characters
            };

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.PlateNumber)
                .WithErrorMessage("Plate number must be less than 20 characters");
        }

        [Test]
        public void ValidCommand_ShouldNotHaveAnyValidationErrors()
        {
            // Arrange
            var command = new UpdatePlateNumberCommand
            {
                PlateId = Guid.NewGuid(),
                PlateNumber = "ABC123"
            };

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Test]
        public void PlateNumber_WithSpecialCharacters_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdatePlateNumberCommand
            {
                PlateId = Guid.NewGuid(),
                PlateNumber = "ABC-123*"
            };

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.PlateNumber);
        }

        [Test]
        public void PlateNumber_WithNumbers_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdatePlateNumberCommand
            {
                PlateId = Guid.NewGuid(),
                PlateNumber = "123456"
            };

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.PlateNumber);
        }

        [Test]
        public void PlateNumber_WithMixedCase_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdatePlateNumberCommand
            {
                PlateId = Guid.NewGuid(),
                PlateNumber = "AbC123"
            };

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.PlateNumber);
        }
    }
}
