using AwesomeAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Features.Settings.Commands.UpdateIgnore;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetIgnores;
using System;
using System.Threading.Tasks;
using Tests.TestHelpers;

namespace Tests.Features.Settings.Commands.UpdateIgnore
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class UpdateIgnoreCommandHandlerTests : TestBase
    {
        private UpdateIgnoreCommandHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new UpdateIgnoreCommandHandler(UnitOfWork);
        }

        [Test]
        public async Task Handle_ValidIgnore_UpdatesIgnoreInDatabase()
        {
            // Arrange
            var existingDbIgnore = CreateTestDbIgnore("ORIGINAL123", "Original Description", false);
            await UnitOfWork.Ignores.AddAsync(existingDbIgnore);
            await UnitOfWork.SaveChangesAsync();

            var updatedIgnoreDto = TestDataFactory.CreateTestIgnoreDto("UPDATED456");
            updatedIgnoreDto.Id = existingDbIgnore.Id;
            updatedIgnoreDto.Description = "Updated Description";
            updatedIgnoreDto.StrictMatch = true;
            
            var command = new UpdateIgnoreCommand(updatedIgnoreDto);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbIgnore = await UnitOfWork.Ignores.GetByIdAsync(existingDbIgnore.Id);
            dbIgnore.Should().NotBeNull();
            dbIgnore.PlateNumber.Should().Be("UPDATED456");
            dbIgnore.Description.Should().Be("Updated Description");
            dbIgnore.IsStrictMatch.Should().BeTrue();
        }

        [Test]
        public async Task Handle_PlateNumberToUpperCase_SavesUpperCaseNumber()
        {
            // Arrange
            var existingDbIgnore = CreateTestDbIgnore("ORIGINAL123", "Original Description");
            await UnitOfWork.Ignores.AddAsync(existingDbIgnore);
            await UnitOfWork.SaveChangesAsync();

            var updatedIgnoreDto = TestDataFactory.CreateTestIgnoreDto("updated456");
            updatedIgnoreDto.Id = existingDbIgnore.Id;
            updatedIgnoreDto.Description = "Updated Description";
            
            var command = new UpdateIgnoreCommand(updatedIgnoreDto);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbIgnore = await UnitOfWork.Ignores.GetByIdAsync(existingDbIgnore.Id);
            dbIgnore.PlateNumber.Should().Be("UPDATED456");
        }

        [Test]
        public async Task Handle_StrictMatchUpdate_UpdatesStrictMatchFlag()
        {
            // Arrange
            var existingDbIgnore = CreateTestDbIgnore("TEST123", "Description", false);
            await UnitOfWork.Ignores.AddAsync(existingDbIgnore);
            await UnitOfWork.SaveChangesAsync();

            var updatedIgnoreDto = TestDataFactory.CreateTestIgnoreDto("TEST123");
            updatedIgnoreDto.Id = existingDbIgnore.Id;
            updatedIgnoreDto.Description = "Description";
            updatedIgnoreDto.StrictMatch = true;
            
            var command = new UpdateIgnoreCommand(updatedIgnoreDto);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbIgnore = await UnitOfWork.Ignores.GetByIdAsync(existingDbIgnore.Id);
            dbIgnore.IsStrictMatch.Should().BeTrue();
        }

        [Test]
        public void Handle_NonExistentIgnore_ThrowsInvalidOperationException()
        {
            // Arrange
            var nonExistentIgnoreDto = TestDataFactory.CreateTestIgnoreDto("TEST123");
            var command = new UpdateIgnoreCommand(nonExistentIgnoreDto);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _handler.Handle(command, GetCancellationToken()));

            exception.Message.Should().Be($"Ignore with ID {nonExistentIgnoreDto.Id} not found");
        }

        [Test]
        public async Task Handle_UpdatesOnlySpecifiedIgnore_LeavesOtherIgnoresUnchanged()
        {
            // Arrange
            var ignore1 = CreateTestDbIgnore("IGNORE1", "Description 1", false);
            var ignore2 = CreateTestDbIgnore("IGNORE2", "Description 2", false);
            await UnitOfWork.Ignores.AddAsync(ignore1);
            await UnitOfWork.Ignores.AddAsync(ignore2);
            await UnitOfWork.SaveChangesAsync();

            var updatedIgnoreDto = TestDataFactory.CreateTestIgnoreDto("UPDATED");
            updatedIgnoreDto.Id = ignore1.Id;
            updatedIgnoreDto.Description = "Updated Description";
            updatedIgnoreDto.StrictMatch = true;
            
            var command = new UpdateIgnoreCommand(updatedIgnoreDto);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbIgnore1 = await UnitOfWork.Ignores.GetByIdAsync(ignore1.Id);
            var dbIgnore2 = await UnitOfWork.Ignores.GetByIdAsync(ignore2.Id);
            
            dbIgnore1.PlateNumber.Should().Be("UPDATED");
            dbIgnore1.Description.Should().Be("Updated Description");
            dbIgnore1.IsStrictMatch.Should().BeTrue();
            
            dbIgnore2.PlateNumber.Should().Be("IGNORE2");
            dbIgnore2.Description.Should().Be("Description 2");
            dbIgnore2.IsStrictMatch.Should().BeFalse();
        }

        [Test]
        public async Task Handle_AllFieldsUpdate_UpdatesAllFields()
        {
            // Arrange
            var existingDbIgnore = CreateTestDbIgnore("OLD123", "Old Description", false);
            await UnitOfWork.Ignores.AddAsync(existingDbIgnore);
            await UnitOfWork.SaveChangesAsync();

            var updatedIgnoreDto = TestDataFactory.CreateTestIgnoreDto("new456");
            updatedIgnoreDto.Id = existingDbIgnore.Id;
            updatedIgnoreDto.Description = "New Description";
            updatedIgnoreDto.StrictMatch = true;
            
            var command = new UpdateIgnoreCommand(updatedIgnoreDto);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbIgnore = await UnitOfWork.Ignores.GetByIdAsync(existingDbIgnore.Id);
            dbIgnore.Should().NotBeNull();
            dbIgnore.PlateNumber.Should().Be("NEW456");
            dbIgnore.Description.Should().Be("New Description");
            dbIgnore.IsStrictMatch.Should().BeTrue();
        }

        private static Ignore CreateTestDbIgnore(string plateNumber, string description, bool strictMatch = false)
        {
            return new Ignore
            {
                Id = Guid.NewGuid(),
                PlateNumber = plateNumber,
                Description = description,
                IsStrictMatch = strictMatch
            };
        }
    }
}
