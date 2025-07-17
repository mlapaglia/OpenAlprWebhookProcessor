using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EnrichPlate;
using OpenAlprWebhookProcessor.Features.Settings.Commands.TestEnrichers;
using Tests.TestHelpers;

namespace Tests.Features.Settings.Commands.TestEnrichers
{
    [TestFixture]
    public class TestEnrichersCommandHandlerTests : TestBase
    {
        private TestEnrichersCommandHandler _handler;
        private ILicensePlateEnricherClient _licensePlateEnricherClient;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _licensePlateEnricherClient = Substitute.For<ILicensePlateEnricherClient>();
            _handler = new TestEnrichersCommandHandler(_licensePlateEnricherClient);
        }

        [Test]
        public async Task Handle_ValidCommand_CallsLicensePlateEnricherClient()
        {
            // Arrange
            var command = new TestEnrichersCommand();
            _licensePlateEnricherClient.TestAsync(Arg.Any<CancellationToken>())
                .Returns(true);

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.Should().BeTrue();
            await _licensePlateEnricherClient.Received(1).TestAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_EnricherClientReturnsTrue_ReturnsTrue()
        {
            // Arrange
            var command = new TestEnrichersCommand();
            _licensePlateEnricherClient.TestAsync(Arg.Any<CancellationToken>())
                .Returns(true);

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task Handle_EnricherClientReturnsFalse_ReturnsFalse()
        {
            // Arrange
            var command = new TestEnrichersCommand();
            _licensePlateEnricherClient.TestAsync(Arg.Any<CancellationToken>())
                .Returns(false);

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task Handle_EnricherClientThrowsException_ExceptionBubbles()
        {
            // Arrange
            var command = new TestEnrichersCommand();
            _licensePlateEnricherClient.TestAsync(Arg.Any<CancellationToken>())
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _handler.Handle(command, GetCancellationToken()));
        }

        [Test]
        public async Task Handle_ValidCommand_PassesCancellationToken()
        {
            // Arrange
            var command = new TestEnrichersCommand();
            var cancellationToken = GetCancellationToken();

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            await _licensePlateEnricherClient.Received(1).TestAsync(cancellationToken);
        }
    }
} 