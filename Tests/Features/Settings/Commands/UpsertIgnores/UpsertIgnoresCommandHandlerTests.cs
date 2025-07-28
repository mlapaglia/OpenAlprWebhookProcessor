using AwesomeAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Settings.Commands.UpsertIgnores;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetIgnores;
using Tests.TestHelpers;

namespace Tests.Features.Settings.Commands.UpsertIgnores
{
    [TestFixture]
    public class UpsertIgnoresCommandHandlerTests : TestBase
    {
        private UpsertIgnoresCommandHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new UpsertIgnoresCommandHandler(UnitOfWork);
        }

        [Test]
        public async Task Handle_NewIgnores_AddsIgnoresToDatabase()
        {
            // Arrange
            var ignores = new List<IgnoreDto>
            {
                new IgnoreDto
                {
                    Id = Guid.NewGuid(),
                    PlateNumber = "TEST123",
                    Description = "Test ignore 1",
                    StrictMatch = true
                },
                new IgnoreDto
                {
                    Id = Guid.NewGuid(),
                    PlateNumber = "TEST456",
                    Description = "Test ignore 2",
                    StrictMatch = false
                }
            };
            var command = new UpsertIgnoresCommand(ignores);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbIgnores = await UnitOfWork.Ignores.GetAllAsync();
            dbIgnores.Should().HaveCount(2);

            var plateNumbers = dbIgnores.Select(i => i.PlateNumber).ToList();
            plateNumbers.Should().Contain("TEST123");
            plateNumbers.Should().Contain("TEST456");
        }

        [Test]
        public async Task Handle_UpdateExistingIgnores_UpdatesIgnoresInDatabase()
        {
            // Arrange
            var ignoreId = Guid.NewGuid();
            var existingIgnore = new OpenAlprWebhookProcessor.Data.Ignore
            {
                Id = ignoreId,
                PlateNumber = "OLD123",
                Description = "Old description",
                IsStrictMatch = false
            };
            await UnitOfWork.Ignores.AddAsync(existingIgnore);
            await UnitOfWork.SaveChangesAsync();

            var ignores = new List<IgnoreDto>
            {
                new IgnoreDto
                {
                    Id = ignoreId,
                    PlateNumber = "NEW123",
                    Description = "New description",
                    StrictMatch = true
                }
            };
            var command = new UpsertIgnoresCommand(ignores);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbIgnores = await UnitOfWork.Ignores.GetAllAsync();
            dbIgnores.Should().HaveCount(1);

            var dbIgnore = dbIgnores.First();
            dbIgnore.Id.Should().Be(ignoreId);
            dbIgnore.PlateNumber.Should().Be("NEW123");
            dbIgnore.Description.Should().Be("New description");
            dbIgnore.IsStrictMatch.Should().BeTrue();
        }

        [Test]
        public async Task Handle_RemoveIgnores_DeletesIgnoresFromDatabase()
        {
            // Arrange
            var ignoreToKeep = new OpenAlprWebhookProcessor.Data.Ignore
            {
                Id = Guid.NewGuid(),
                PlateNumber = "KEEP123",
                Description = "Keep this",
                IsStrictMatch = true
            };
            var ignoreToRemove = new OpenAlprWebhookProcessor.Data.Ignore
            {
                Id = Guid.NewGuid(),
                PlateNumber = "REMOVE123",
                Description = "Remove this",
                IsStrictMatch = false
            };
            await UnitOfWork.Ignores.AddAsync(ignoreToKeep);
            await UnitOfWork.Ignores.AddAsync(ignoreToRemove);
            await UnitOfWork.SaveChangesAsync();

            var ignores = new List<IgnoreDto>
            {
                new IgnoreDto
                {
                    Id = ignoreToKeep.Id,
                    PlateNumber = "KEEP123",
                    Description = "Keep this",
                    StrictMatch = true
                }
            };
            var command = new UpsertIgnoresCommand(ignores);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbIgnores = await UnitOfWork.Ignores.GetAllAsync();
            dbIgnores.Should().HaveCount(1);
            dbIgnores.First().PlateNumber.Should().Be("KEEP123");
        }

        [Test]
        public async Task Handle_EmptyOrNullPlateNumbers_FiltersOutInvalidIgnores()
        {
            // Arrange
            var ignores = new List<IgnoreDto>
            {
                new IgnoreDto
                {
                    Id = Guid.NewGuid(),
                    PlateNumber = "VALID123",
                    Description = "Valid ignore",
                    StrictMatch = true
                },
                new IgnoreDto
                {
                    Id = Guid.NewGuid(),
                    PlateNumber = "",
                    Description = "Empty plate",
                    StrictMatch = false
                },
                new IgnoreDto
                {
                    Id = Guid.NewGuid(),
                    PlateNumber = null,
                    Description = "Null plate",
                    StrictMatch = false
                },
                new IgnoreDto
                {
                    Id = Guid.NewGuid(),
                    PlateNumber = "   ",
                    Description = "Whitespace plate",
                    StrictMatch = false
                }
            };
            var command = new UpsertIgnoresCommand(ignores);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbIgnores = await UnitOfWork.Ignores.GetAllAsync();
            dbIgnores.Should().HaveCount(1);
            dbIgnores.First().PlateNumber.Should().Be("VALID123");
        }

        [Test]
        public async Task Handle_MixedAddUpdateRemove_HandlesAllOperations()
        {
            // Arrange
            var existingIgnoreId = Guid.NewGuid();
            var ignoreToRemoveId = Guid.NewGuid();
            
            var existingIgnore = new OpenAlprWebhookProcessor.Data.Ignore
            {
                Id = existingIgnoreId,
                PlateNumber = "UPDATE123",
                Description = "To be updated",
                IsStrictMatch = false
            };
            var ignoreToRemove = new OpenAlprWebhookProcessor.Data.Ignore
            {
                Id = ignoreToRemoveId,
                PlateNumber = "REMOVE123",
                Description = "To be removed",
                IsStrictMatch = true
            };
            await UnitOfWork.Ignores.AddAsync(existingIgnore);
            await UnitOfWork.Ignores.AddAsync(ignoreToRemove);
            await UnitOfWork.SaveChangesAsync();

            var ignores = new List<IgnoreDto>
            {
                new IgnoreDto
                {
                    Id = existingIgnoreId,
                    PlateNumber = "UPDATED123",
                    Description = "Updated description",
                    StrictMatch = true
                },
                new IgnoreDto
                {
                    Id = Guid.NewGuid(),
                    PlateNumber = "NEW123",
                    Description = "New ignore",
                    StrictMatch = false
                }
            };
            var command = new UpsertIgnoresCommand(ignores);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbIgnores = await UnitOfWork.Ignores.GetAllAsync();
            dbIgnores.Should().HaveCount(2);

            var plateNumbers = dbIgnores.Select(i => i.PlateNumber).ToList();
            plateNumbers.Should().Contain("UPDATED123");
            plateNumbers.Should().Contain("NEW123");
            plateNumbers.Should().NotContain("REMOVE123");
        }
    }
} 