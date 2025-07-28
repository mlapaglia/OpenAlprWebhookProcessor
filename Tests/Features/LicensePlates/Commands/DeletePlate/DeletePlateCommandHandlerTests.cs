using AwesomeAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Features.LicensePlates.Commands.DeletePlate;
using Tests.TestHelpers;

namespace Tests.Features.LicensePlates.Commands.DeletePlate
{
    [TestFixture]
    public class DeletePlateCommandHandlerTests : TestBase
    {
        private DeletePlateCommandHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new DeletePlateCommandHandler(UnitOfWork);
        }

        [Test]
        public async Task Handle_ValidPlateId_DeletesPlateSuccessfully()
        {
            // Arrange
            var plateGroup = TestDataFactory.CreateTestPlateGroup();
            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.SaveChangesAsync();

            var command = new DeletePlateCommand(plateGroup.Id);
            var cancellationToken = GetCancellationToken();

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert - verify plate was deleted from database
            var plateAfterDeletion = await UnitOfWork.PlateGroups.GetByIdAsync(plateGroup.Id, cancellationToken);
            plateAfterDeletion.Should().BeNull();
        }

        [Test]
        public void Handle_PlateNotFound_ThrowsArgumentException()
        {
            // Arrange
            var nonExistentPlateId = Guid.NewGuid();
            var command = new DeletePlateCommand(nonExistentPlateId);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => 
                _handler.Handle(command, cancellationToken));

            exception.Message.Should().Be($"Plate with ID {nonExistentPlateId} not found");
        }

        [Test]
        public void Handle_EmptyGuid_ThrowsArgumentException()
        {
            // Arrange
            var command = new DeletePlateCommand(Guid.Empty);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => 
                _handler.Handle(command, cancellationToken));

            exception.Message.Should().Be($"Plate with ID {Guid.Empty} not found");
        }

        [Test]
        public async Task Handle_PlateWithRelatedData_DeletesPlateAndRelatedData()
        {
            // Arrange
            var plateGroup = TestDataFactory.CreateTestPlateGroup();
            
            // Add some possible numbers to test cascade deletion
            plateGroup.PossibleNumbers.Add(new PlateGroupPossibleNumbers 
            { 
                Id = Guid.NewGuid(),
                Number = "ABC123",
                PlateGroupId = plateGroup.Id
            });
            plateGroup.PossibleNumbers.Add(new PlateGroupPossibleNumbers 
            { 
                Id = Guid.NewGuid(),
                Number = "ABD123",
                PlateGroupId = plateGroup.Id
            });

            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.SaveChangesAsync();

            var command = new DeletePlateCommand(plateGroup.Id);
            var cancellationToken = GetCancellationToken();

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            var plateAfterDeletion = await UnitOfWork.PlateGroups.GetByIdWithDetailsAsync(plateGroup.Id, cancellationToken);
            plateAfterDeletion.Should().BeNull();
        }

        [Test]
        public async Task Handle_MultiplePlatesExist_DeletesOnlySpecifiedPlate()
        {
            // Arrange
            var plateGroup1 = TestDataFactory.CreateTestPlateGroup();
            var plateGroup2 = TestDataFactory.CreateTestPlateGroup();
            
            await UnitOfWork.PlateGroups.AddAsync(plateGroup1);
            await UnitOfWork.PlateGroups.AddAsync(plateGroup2);
            await UnitOfWork.SaveChangesAsync();

            var command = new DeletePlateCommand(plateGroup1.Id);
            var cancellationToken = GetCancellationToken();

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            var remainingPlate = await UnitOfWork.PlateGroups.GetByIdAsync(plateGroup2.Id, cancellationToken);
            remainingPlate.Should().NotBeNull();
            remainingPlate.Id.Should().Be(plateGroup2.Id);
        }
    }
} 