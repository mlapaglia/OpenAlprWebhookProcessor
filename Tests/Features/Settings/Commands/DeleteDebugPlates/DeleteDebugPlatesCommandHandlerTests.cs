using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Features.Settings.Commands.DeleteDebugPlates;
using Tests.TestHelpers;

namespace Tests.Features.Settings.Commands.DeleteDebugPlates
{
    [TestFixture]
    public class DeleteDebugPlatesCommandHandlerTests : TestBase
    {
        private DeleteDebugPlatesCommandHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new DeleteDebugPlatesCommandHandler(UnitOfWork);
        }

        [Test]
        public async Task Handle_WithExistingRawPlateGroups_DeletesAllRawPlateGroups()
        {
            // Arrange
            var rawPlate1 = CreateTestRawPlateGroup("plate-1");
            var rawPlate2 = CreateTestRawPlateGroup("plate-2");
            var rawPlate3 = CreateTestRawPlateGroup("plate-3");

            await UnitOfWork.RawPlateGroups.AddAsync(rawPlate1);
            await UnitOfWork.RawPlateGroups.AddAsync(rawPlate2);
            await UnitOfWork.RawPlateGroups.AddAsync(rawPlate3);
            await UnitOfWork.SaveChangesAsync();

            var command = new DeleteDebugPlatesCommand();
            var cancellationToken = GetCancellationToken();

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert - verify all raw plate groups are deleted
            var remainingRawPlates = await UnitOfWork.RawPlateGroups.GetAllAsync(cancellationToken);
            remainingRawPlates.Should().BeEmpty();
        }

        [Test]
        public async Task Handle_WithNoRawPlateGroups_CompletesSuccessfully()
        {
            // Arrange
            var command = new DeleteDebugPlatesCommand();
            var cancellationToken = GetCancellationToken();

            // Verify no raw plates exist initially
            var initialRawPlates = await UnitOfWork.RawPlateGroups.GetAllAsync(cancellationToken);
            initialRawPlates.Should().BeEmpty();

            // Act & Assert - should complete without throwing
            await _handler.Handle(command, cancellationToken);

            // Verify still no raw plates exist
            var finalRawPlates = await UnitOfWork.RawPlateGroups.GetAllAsync(cancellationToken);
            finalRawPlates.Should().BeEmpty();
        }

        [Test]
        public async Task Handle_WithSingleRawPlateGroup_DeletesPlateGroup()
        {
            // Arrange
            var rawPlate = CreateTestRawPlateGroup("single-plate");
            await UnitOfWork.RawPlateGroups.AddAsync(rawPlate);
            await UnitOfWork.SaveChangesAsync();

            var command = new DeleteDebugPlatesCommand();
            var cancellationToken = GetCancellationToken();

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            var remainingRawPlates = await UnitOfWork.RawPlateGroups.GetAllAsync(cancellationToken);
            remainingRawPlates.Should().BeEmpty();
        }

        [Test]
        public async Task Handle_WithManyRawPlateGroups_DeletesAllRawPlateGroups()
        {
            // Arrange
            var rawPlates = new List<PlateGroupRaw>();
            for (int i = 0; i < 50; i++)
            {
                rawPlates.Add(CreateTestRawPlateGroup($"plate-{i}"));
            }

            foreach (var rawPlate in rawPlates)
            {
                await UnitOfWork.RawPlateGroups.AddAsync(rawPlate);
            }
            await UnitOfWork.SaveChangesAsync();

            var command = new DeleteDebugPlatesCommand();
            var cancellationToken = GetCancellationToken();

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            var remainingRawPlates = await UnitOfWork.RawPlateGroups.GetAllAsync(cancellationToken);
            remainingRawPlates.Should().BeEmpty();
        }

        [Test]
        public async Task Handle_WithMixedProcessingStates_DeletesAllRegardlessOfProcessingState()
        {
            // Arrange
            var processedPlate = CreateTestRawPlateGroup("processed-plate");
            processedPlate.WasProcessedCorrectly = true;

            var unprocessedPlate = CreateTestRawPlateGroup("unprocessed-plate");
            unprocessedPlate.WasProcessedCorrectly = false;

            await UnitOfWork.RawPlateGroups.AddAsync(processedPlate);
            await UnitOfWork.RawPlateGroups.AddAsync(unprocessedPlate);
            await UnitOfWork.SaveChangesAsync();

            var command = new DeleteDebugPlatesCommand();
            var cancellationToken = GetCancellationToken();

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert - both should be deleted regardless of processing state
            var remainingRawPlates = await UnitOfWork.RawPlateGroups.GetAllAsync(cancellationToken);
            remainingRawPlates.Should().BeEmpty();
        }

        [Test]
        public async Task Handle_WithRawPlateGroupsAndRegularPlateGroups_OnlyDeletesRawPlateGroups()
        {
            // Arrange
            var rawPlate = CreateTestRawPlateGroup("raw-plate");
            var regularPlate = TestDataFactory.CreateTestPlateGroup();

            await UnitOfWork.RawPlateGroups.AddAsync(rawPlate);
            await UnitOfWork.PlateGroups.AddAsync(regularPlate);
            await UnitOfWork.SaveChangesAsync();

            var command = new DeleteDebugPlatesCommand();
            var cancellationToken = GetCancellationToken();

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            var remainingRawPlates = await UnitOfWork.RawPlateGroups.GetAllAsync(cancellationToken);
            remainingRawPlates.Should().BeEmpty();

            var remainingRegularPlates = await UnitOfWork.PlateGroups.GetAllAsync(cancellationToken);
            remainingRegularPlates.Should().ContainSingle()
                .Which.Id.Should().Be(regularPlate.Id);
        }

        [Test]
        public async Task Handle_SavesChangesToDatabase()
        {
            // Arrange
            var rawPlate = CreateTestRawPlateGroup("test-plate");
            await UnitOfWork.RawPlateGroups.AddAsync(rawPlate);
            await UnitOfWork.SaveChangesAsync();

            var command = new DeleteDebugPlatesCommand();
            var cancellationToken = GetCancellationToken();

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert - verify changes were persisted by checking with a new context/unit of work
            using var freshContext = ContextCreator.CreateContext();
            var remainingRawPlates = await freshContext.RawPlateGroups.ToListAsync();
            remainingRawPlates.Should().BeEmpty();
        }

        [Test]
        public async Task Handle_WithCancellationToken_UsesTokenCorrectly()
        {
            // Arrange
            var rawPlate = CreateTestRawPlateGroup("test-plate");
            await UnitOfWork.RawPlateGroups.AddAsync(rawPlate);
            await UnitOfWork.SaveChangesAsync();

            var command = new DeleteDebugPlatesCommand();
            var cancellationToken = GetCancellationToken();

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert - verify deletion worked with the cancellation token
            var remainingRawPlates = await UnitOfWork.RawPlateGroups.GetAllAsync(cancellationToken);
            remainingRawPlates.Should().BeEmpty();
        }

        #region Helper Methods

        private static PlateGroupRaw CreateTestRawPlateGroup(string plateGroupId)
        {
            return new PlateGroupRaw
            {
                Id = Guid.NewGuid(),
                PlateGroupId = plateGroupId,
                ReceivedOnEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                RawPlateGroup = $"{{\"test\":\"data\",\"plateId\":\"{plateGroupId}\"}}",
                WasProcessedCorrectly = false
            };
        }

        #endregion
    }
} 