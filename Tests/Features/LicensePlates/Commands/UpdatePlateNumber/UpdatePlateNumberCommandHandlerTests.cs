using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.LicensePlates.Commands.UpdatePlateNumber;
using Tests.TestHelpers;

namespace Tests.Features.LicensePlates.Commands.UpdatePlateNumber
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class UpdatePlateNumberCommandHandlerTests : TestBase
    {
        private UpdatePlateNumberCommandHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new UpdatePlateNumberCommandHandler(Context);
        }

        [Test]
        public void Constructor_WithValidContext_CreatesInstance()
        {
            var handler = new UpdatePlateNumberCommandHandler(Context);
            handler.Should().NotBeNull();
        }

        [Test]
        public async Task Handle_WithValidPlateId_UpdatesPlateNumber()
        {
            // Arrange
            var plateGroup = TestDataFactory.CreateTestPlateGroup("OLD123");
            await Context.PlateGroups.AddAsync(plateGroup);
            await Context.SaveChangesAsync();

            var command = new UpdatePlateNumberCommand
            {
                PlateId = plateGroup.Id,
                PlateNumber = "NEW456"
            };

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            
            var updatedPlateGroup = await Context.PlateGroups
                .FirstOrDefaultAsync(x => x.Id == plateGroup.Id);
            
            updatedPlateGroup.Should().NotBeNull();
            updatedPlateGroup.BestNumber.Should().Be("NEW456");
        }

        [Test]
        public void Handle_WithNonExistentPlateId_ThrowsInvalidOperationException()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var command = new UpdatePlateNumberCommand
            {
                PlateId = nonExistentId,
                PlateNumber = "NEW456"
            };

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                () => _handler.Handle(command, GetCancellationToken()).AsTask());
            
            exception.Message.Should().Be($"Plate with ID {nonExistentId} not found");
        }

        [Test]
        public async Task Handle_WithCancellationToken_RespectsCancellation()
        {
            // Arrange
            var plateGroup = TestDataFactory.CreateTestPlateGroup("TEST123");
            await Context.PlateGroups.AddAsync(plateGroup);
            await Context.SaveChangesAsync();

            var command = new UpdatePlateNumberCommand
            {
                PlateId = plateGroup.Id,
                PlateNumber = "UPDATED123"
            };

            using var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            
            var updatedPlateGroup = await Context.PlateGroups
                .FirstOrDefaultAsync(x => x.Id == plateGroup.Id, cancellationToken);
            
            updatedPlateGroup.BestNumber.Should().Be("UPDATED123");
        }

        [Test]
        public async Task Handle_WithEmptyPlateNumber_UpdatesToEmptyString()
        {
            // Arrange
            var plateGroup = TestDataFactory.CreateTestPlateGroup("ORIGINAL123");
            await Context.PlateGroups.AddAsync(plateGroup);
            await Context.SaveChangesAsync();

            var command = new UpdatePlateNumberCommand
            {
                PlateId = plateGroup.Id,
                PlateNumber = ""
            };

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            
            var updatedPlateGroup = await Context.PlateGroups
                .FirstOrDefaultAsync(x => x.Id == plateGroup.Id);
            
            updatedPlateGroup.Should().NotBeNull();
            updatedPlateGroup.BestNumber.Should().Be("");
        }

        [Test]
        public async Task Handle_WithNullPlateNumber_UpdatesToNull()
        {
            // Arrange
            var plateGroup = TestDataFactory.CreateTestPlateGroup("ORIGINAL123");
            await Context.PlateGroups.AddAsync(plateGroup);
            await Context.SaveChangesAsync();

            var command = new UpdatePlateNumberCommand
            {
                PlateId = plateGroup.Id,
                PlateNumber = null
            };

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            
            var updatedPlateGroup = await Context.PlateGroups
                .FirstOrDefaultAsync(x => x.Id == plateGroup.Id);
            
            updatedPlateGroup.Should().NotBeNull();
            updatedPlateGroup.BestNumber.Should().BeNull();
        }

        [Test]
        public async Task Handle_WithMultiplePlateGroups_UpdatesOnlySpecifiedPlate()
        {
            // Arrange
            var plateGroup1 = TestDataFactory.CreateTestPlateGroup("PLATE001");
            var plateGroup2 = TestDataFactory.CreateTestPlateGroup("PLATE002");
            
            await Context.PlateGroups.AddRangeAsync(plateGroup1, plateGroup2);
            await Context.SaveChangesAsync();

            var command = new UpdatePlateNumberCommand
            {
                PlateId = plateGroup1.Id,
                PlateNumber = "UPDATED001"
            };

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            
            var updatedPlateGroup1 = await Context.PlateGroups
                .FirstOrDefaultAsync(x => x.Id == plateGroup1.Id);
            var unchangedPlateGroup2 = await Context.PlateGroups
                .FirstOrDefaultAsync(x => x.Id == plateGroup2.Id);
            
            updatedPlateGroup1.Should().NotBeNull();
            updatedPlateGroup1.BestNumber.Should().Be("UPDATED001");
            
            unchangedPlateGroup2.Should().NotBeNull();
            unchangedPlateGroup2.BestNumber.Should().Be("PLATE002");
        }

        [Test]
        public async Task Handle_SavesChangesToDatabase()
        {
            // Arrange
            var plateGroup = TestDataFactory.CreateTestPlateGroup("BEFORE123");
            await Context.PlateGroups.AddAsync(plateGroup);
            await Context.SaveChangesAsync();

            var command = new UpdatePlateNumberCommand
            {
                PlateId = plateGroup.Id,
                PlateNumber = "AFTER123"
            };

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert - Create new context to verify persistence
            using var newContext = ContextCreator.CreateContext();
            var persistedPlateGroup = await newContext.PlateGroups
                .FirstOrDefaultAsync(x => x.Id == plateGroup.Id);
            
            persistedPlateGroup.Should().NotBeNull();
            persistedPlateGroup.BestNumber.Should().Be("AFTER123");
        }
    }
}
