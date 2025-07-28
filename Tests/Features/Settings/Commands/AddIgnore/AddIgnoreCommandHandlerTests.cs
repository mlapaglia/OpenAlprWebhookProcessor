using AwesomeAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Settings.Commands.AddIgnore;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetIgnores;
using Tests.TestHelpers;

namespace Tests.Features.Settings.Commands.AddIgnore
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class AddIgnoreCommandHandlerTests : TestBase
    {
        private AddIgnoreCommandHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new AddIgnoreCommandHandler(UnitOfWork);
        }

        [Test]
        public async Task Handle_ValidIgnore_AddsIgnoreToDatabase()
        {
            // Arrange
            var ignoreDto = new IgnoreDto
            {
                PlateNumber = "TEST123",
                Description = "Test ignore",
                StrictMatch = true
            };
            var command = new AddIgnoreCommand(ignoreDto);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbIgnores = await UnitOfWork.Ignores.GetAllAsync();
            dbIgnores.Should().HaveCount(1);

            var dbIgnore = dbIgnores.First();
            dbIgnore.PlateNumber.Should().Be("TEST123");
            dbIgnore.Description.Should().Be("Test ignore");
            dbIgnore.IsStrictMatch.Should().BeTrue();
        }

        [Test]
        public async Task Handle_DuplicatePlateNumber_ThrowsArgumentException()
        {
            // Arrange
            var existingIgnore = new OpenAlprWebhookProcessor.Data.Ignore
            {
                PlateNumber = "TEST123",
                Description = "Existing ignore",
                IsStrictMatch = false
            };
            await UnitOfWork.Ignores.AddAsync(existingIgnore);
            await UnitOfWork.SaveChangesAsync();

            var ignoreDto = new IgnoreDto
            {
                PlateNumber = "TEST123",
                Description = "New ignore",
                StrictMatch = true
            };
            var command = new AddIgnoreCommand(ignoreDto);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _handler.Handle(command, GetCancellationToken()));
            exception.Message.Should().Be("ignore already exists");
        }

        [Test]
        public async Task Handle_NullDescription_AddsIgnoreWithNullDescription()
        {
            // Arrange
            var ignoreDto = new IgnoreDto
            {
                PlateNumber = "TEST123",
                Description = null,
                StrictMatch = false
            };
            var command = new AddIgnoreCommand(ignoreDto);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbIgnores = await UnitOfWork.Ignores.GetAllAsync();
            var dbIgnore = dbIgnores.First();
            dbIgnore.Description.Should().BeNull();
        }

        [Test]
        public async Task Handle_ValidIgnore_SavesChanges()
        {
            // Arrange
            var ignoreDto = new IgnoreDto
            {
                PlateNumber = "TEST123",
                Description = "Test ignore",
                StrictMatch = true
            };
            var command = new AddIgnoreCommand(ignoreDto);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbIgnores = await UnitOfWork.Ignores.GetAllAsync();
            dbIgnores.Should().HaveCount(1);
        }
    }
} 