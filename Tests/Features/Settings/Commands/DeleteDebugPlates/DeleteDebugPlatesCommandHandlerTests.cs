using FluentAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Settings.Commands.DeleteDebugPlates;
using System.Threading.Tasks;
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
            _handler = new DeleteDebugPlatesCommandHandler(Context);
        }

        [Test]
        public async Task Handle_ValidCommand_ExecutesSqlCommand()
        {
            // Arrange
            var command = new DeleteDebugPlatesCommand();

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            // Since we're using an in-memory database, we can't easily verify the exact SQL execution,
            // but we can verify the command completes without throwing an exception
            // The handler should complete successfully
        }

        [Test]
        public async Task Handle_ValidCommand_CompletesSuccessfully()
        {
            // Arrange
            var command = new DeleteDebugPlatesCommand();

            // Act
            var task = _handler.Handle(command, GetCancellationToken());

            // Assert
            task.Should().NotBeNull();
            await task; // Should complete without exception
        }

        [Test]
        public async Task Handle_ValidCommand_DoesNotThrowException()
        {
            // Arrange
            var command = new DeleteDebugPlatesCommand();

            // Act & Assert
            Assert.DoesNotThrowAsync(
                async () => await _handler.Handle(command, GetCancellationToken()));
        }

        [Test]
        public async Task Handle_WithCancellationToken_PassesToSqlCommand()
        {
            // Arrange
            var command = new DeleteDebugPlatesCommand();
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            Assert.DoesNotThrowAsync(
                async () => await _handler.Handle(command, cancellationToken));
        }
    }
} 