using FluentAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EditPlate;
using Tests.TestHelpers;

namespace Tests.Features.LicensePlates.Commands.EditPlate
{
    [TestFixture]
    public class EditPlateCommandHandlerTests : TestBase
    {
        private EditPlateCommandHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new EditPlateCommandHandler(UnitOfWork);
        }

        [Test]
        public async Task Handle_ValidPlateId_UpdatesPlateSuccessfully()
        {
            // Arrange
            var plateGroup = TestDataFactory.CreateTestPlateGroup();
            plateGroup.BestNumber = "ABC123";
            
            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.SaveChangesAsync();

            var command = new EditPlateCommand
            {
                Id = plateGroup.Id,
                PlateNumber = "XYZ789",
                Notes = "Updated notes"
            };

            var cancellationToken = GetCancellationToken();

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            var updatedPlate = await UnitOfWork.PlateGroups.GetByIdAsync(plateGroup.Id, cancellationToken);
            updatedPlate.Should().NotBeNull();
            updatedPlate.BestNumber.Should().Be("XYZ789");
        }

        [Test]
        public void Handle_PlateNotFound_ThrowsArgumentException()
        {
            // Arrange
            var nonExistentPlateId = Guid.NewGuid();
            var command = new EditPlateCommand
            {
                Id = nonExistentPlateId,
                PlateNumber = "XYZ789",
                Notes = "Updated notes"
            };

            var cancellationToken = GetCancellationToken();

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => 
                _handler.Handle(command, cancellationToken));

            exception.Message.Should().Be($"Plate with ID {nonExistentPlateId} not found");
        }

        [Test]
        public async Task Handle_EmptyPlateNumber_UpdatesPlateWithEmptyString()
        {
            // Arrange
            var plateGroup = TestDataFactory.CreateTestPlateGroup();
            plateGroup.BestNumber = "ABC123";
            
            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.SaveChangesAsync();

            var command = new EditPlateCommand
            {
                Id = plateGroup.Id,
                PlateNumber = "",
                Notes = "Some notes"
            };

            var cancellationToken = GetCancellationToken();

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            var updatedPlate = await UnitOfWork.PlateGroups.GetByIdAsync(plateGroup.Id, cancellationToken);
            updatedPlate.Should().NotBeNull();
            updatedPlate.BestNumber.Should().Be("");
        }

        [Test]
        public async Task Handle_NullPlateNumber_UpdatesPlateWithNullValue()
        {
            // Arrange
            var plateGroup = TestDataFactory.CreateTestPlateGroup();
            plateGroup.BestNumber = "ABC123";
            
            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.SaveChangesAsync();

            var command = new EditPlateCommand
            {
                Id = plateGroup.Id,
                PlateNumber = null,
                Notes = "Some notes"
            };

            var cancellationToken = GetCancellationToken();

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            var updatedPlate = await UnitOfWork.PlateGroups.GetByIdAsync(plateGroup.Id, cancellationToken);
            updatedPlate.Should().NotBeNull();
            updatedPlate.BestNumber.Should().BeNull();
        }

        [Test]
        public void Handle_EmptyGuid_ThrowsArgumentException()
        {
            // Arrange
            var command = new EditPlateCommand
            {
                Id = Guid.Empty,
                PlateNumber = "XYZ789",
                Notes = "Updated notes"
            };

            var cancellationToken = GetCancellationToken();

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => 
                _handler.Handle(command, cancellationToken));

            exception.Message.Should().Be($"Plate with ID {Guid.Empty} not found");
        }

        [Test]
        public async Task Handle_MultiplePlatesExist_UpdatesOnlySpecifiedPlate()
        {
            // Arrange
            var plateGroup1 = TestDataFactory.CreateTestPlateGroup();
            plateGroup1.BestNumber = "ABC123";
            
            var plateGroup2 = TestDataFactory.CreateTestPlateGroup();
            plateGroup2.BestNumber = "DEF456";
            
            await UnitOfWork.PlateGroups.AddAsync(plateGroup1);
            await UnitOfWork.PlateGroups.AddAsync(plateGroup2);
            await UnitOfWork.SaveChangesAsync();

            var command = new EditPlateCommand
            {
                Id = plateGroup1.Id,
                PlateNumber = "UPDATED",
                Notes = "Updated notes"
            };

            var cancellationToken = GetCancellationToken();

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            var updatedPlate1 = await UnitOfWork.PlateGroups.GetByIdAsync(plateGroup1.Id, cancellationToken);
            updatedPlate1.Should().NotBeNull();
            updatedPlate1.BestNumber.Should().Be("UPDATED");

            var unchangedPlate2 = await UnitOfWork.PlateGroups.GetByIdAsync(plateGroup2.Id, cancellationToken);
            unchangedPlate2.Should().NotBeNull();
            unchangedPlate2.BestNumber.Should().Be("DEF456");
        }
    }
} 