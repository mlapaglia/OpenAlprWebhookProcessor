using AwesomeAssertions;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Webhooks.Commands.ProcessAlertWebhook;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebhook;
using Tests.TestHelpers;

namespace Tests.Features.Webhooks.Commands
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class ProcessAlertWebhookCommandHandlerTests : TestBase
    {
        private ProcessAlertWebhookCommandHandler _handler;
        private IGroupWebhookHandler _groupWebhookHandler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _groupWebhookHandler = Substitute.For<IGroupWebhookHandler>();
            
            _handler = new ProcessAlertWebhookCommandHandler(
                UnitOfWork,
                _groupWebhookHandler);
        }

        [Test]
        public void Constructor_WithValidParameters_CreatesInstance()
        {
            // Act
            var handler = new ProcessAlertWebhookCommandHandler(
                UnitOfWork,
                _groupWebhookHandler);

            // Assert
            handler.Should().NotBeNull();
        }

        [Test]
        public async Task Handle_WithValidWebhook_CallsGroupWebhookHandler()
        {
            // Arrange
            var webhook = CreateTestWebhook();
            var command = new ProcessAlertWebhookCommand(webhook, false);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            await _groupWebhookHandler.Received(1).HandleWebhookAsync(
                webhook,
                false,
                cancellationToken);
        }

        [Test]
        public async Task Handle_WithBulkImportTrue_PassesBulkImportFlag()
        {
            // Arrange
            var webhook = CreateTestWebhook();
            var command = new ProcessAlertWebhookCommand(webhook, true);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            await _groupWebhookHandler.Received(1).HandleWebhookAsync(
                webhook,
                true,
                cancellationToken);
        }

        [Test]
        public async Task Handle_WithNullWebhook_CallsGroupWebhookHandlerWithNull()
        {
            // Arrange
            var command = new ProcessAlertWebhookCommand(null, false);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            await _groupWebhookHandler.Received(1).HandleWebhookAsync(
                null,
                false,
                cancellationToken);
        }

        [Test]
        public void Handle_WhenGroupWebhookHandlerThrows_PropagatesException()
        {
            // Arrange
            var webhook = CreateTestWebhook();
            var command = new ProcessAlertWebhookCommand(webhook, false);
            var cancellationToken = GetCancellationToken();
            var expectedException = new InvalidOperationException("Test exception");

            _groupWebhookHandler
                .When(x => x.HandleWebhookAsync(Arg.Any<Webhook>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()))
                .Do(x => throw expectedException);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _handler.Handle(command, cancellationToken));
        }

        [Test]
        public async Task Handle_WithDifferentCancellationToken_PassesTokenCorrectly()
        {
            // Arrange
            var webhook = CreateTestWebhook();
            var command = new ProcessAlertWebhookCommand(webhook, false);
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            await _groupWebhookHandler.Received(1).HandleWebhookAsync(
                webhook,
                false,
                cancellationToken);
        }

        #region Test Helper Methods

        private Webhook CreateTestWebhook()
        {
            return new Webhook
            {
                DataType = "alpr_alert",
                Description = "Test alert webhook",
                Group = new Group
                {
                    CameraId = 1,
                    IsParked = false,
                    IsPreview = false,
                    BestPlateNumber = "ALERT123",
                    BestUuid = Guid.NewGuid().ToString(),
                    EpochStart = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    EpochEnd = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 5000,
                    Uuids = new List<string> { Guid.NewGuid().ToString() },
                    Candidates = new List<Candidate>
                    {
                        new Candidate { Plate = "ALERT123", Confidence = 95.5 }
                    },
                    BestPlate = new Plate
                    {
                        Coordinates = new List<Coordinate>
                        {
                            new Coordinate { X = 100, Y = 200 },
                            new Coordinate { X = 300, Y = 200 },
                            new Coordinate { X = 300, Y = 400 },
                            new Coordinate { X = 100, Y = 400 }
                        },
                        Confidence = 95.5,
                        ProcessingTimeMs = 150.0,
                        PlateCropJpeg = Convert.ToBase64String(TestDataFactory.CreateTestJpegBytes()),
                        RegionConfidence = 85.0,
                        Region = "ca"
                    },
                    TravelDirection = 45.0,
                    BestConfidence = 95.5
                }
            };
        }

        #endregion
    }
}
