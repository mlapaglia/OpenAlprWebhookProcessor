using AwesomeAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Features.Ignores.Commands.DeleteIgnore;
using System;
using System.Threading.Tasks;
using Tests.TestHelpers;

namespace Tests.Features.Ignores.Commands.DeleteIgnore
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class DeleteIgnoreCommandHandlerTests : TestBase
    {
        private DeleteIgnoreCommandHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new DeleteIgnoreCommandHandler(UnitOfWork);
        }

        [Test]
        public async Task Handle_ValidIgnoreId_DeletesIgnoreFromDatabase()
        {
            // Arrange
            var ignore = CreateTestDbIgnore("TEST123", "Test ignore");
            await UnitOfWork.Ignores.AddAsync(ignore);
            await UnitOfWork.SaveChangesAsync();

            var command = new DeleteIgnoreCommand(ignore.Id);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbIgnores = await UnitOfWork.Ignores.GetAllAsync();
            dbIgnores.Should().BeEmpty();
        }

        [Test]
        public void Handle_NonExistentIgnoreId_ThrowsInvalidOperationException()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var command = new DeleteIgnoreCommand(nonExistentId);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _handler.Handle(command, GetCancellationToken()));

            exception.Message.Should().Be($"Ignore with ID {nonExistentId} not found");
        }

        [Test]
        public async Task Handle_DeletesOnlySpecifiedIgnore_LeavesOtherIgnoresUntouched()
        {
            // Arrange
            var ignore1 = CreateTestDbIgnore("IGNORE1", "Description 1");
            var ignore2 = CreateTestDbIgnore("IGNORE2", "Description 2");
            await UnitOfWork.Ignores.AddAsync(ignore1);
            await UnitOfWork.Ignores.AddAsync(ignore2);
            await UnitOfWork.SaveChangesAsync();

            var command = new DeleteIgnoreCommand(ignore1.Id);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbIgnores = await UnitOfWork.Ignores.GetAllAsync();
            dbIgnores.Should().HaveCount(1);
            dbIgnores.First().Id.Should().Be(ignore2.Id);
            dbIgnores.First().PlateNumber.Should().Be("IGNORE2");
        }

        [Test]
        public async Task Handle_ValidIgnoreId_SavesChanges()
        {
            // Arrange
            var ignore = CreateTestDbIgnore("TEST123", "Test ignore");
            await UnitOfWork.Ignores.AddAsync(ignore);
            await UnitOfWork.SaveChangesAsync();

            var command = new DeleteIgnoreCommand(ignore.Id);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbIgnore = await UnitOfWork.Ignores.GetByIdAsync(ignore.Id);
            dbIgnore.Should().BeNull();
        }

        [Test]
        public async Task Handle_MultipleIgnores_DeletesCorrectOne()
        {
            // Arrange
            var ignore1 = CreateTestDbIgnore("KEEP123", "Keep this one");
            var ignore2 = CreateTestDbIgnore("DELETE456", "Delete this one");
            var ignore3 = CreateTestDbIgnore("ALSO_KEEP789", "Also keep this one");
            
            await UnitOfWork.Ignores.AddAsync(ignore1);
            await UnitOfWork.Ignores.AddAsync(ignore2);
            await UnitOfWork.Ignores.AddAsync(ignore3);
            await UnitOfWork.SaveChangesAsync();

            var command = new DeleteIgnoreCommand(ignore2.Id);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbIgnores = await UnitOfWork.Ignores.GetAllAsync();
            dbIgnores.Should().HaveCount(2);
            
            var remainingPlateNumbers = dbIgnores.Select(i => i.PlateNumber).ToList();
            remainingPlateNumbers.Should().Contain("KEEP123");
            remainingPlateNumbers.Should().Contain("ALSO_KEEP789");
            remainingPlateNumbers.Should().NotContain("DELETE456");
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
