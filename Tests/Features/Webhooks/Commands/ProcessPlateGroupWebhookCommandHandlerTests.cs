using AwesomeAssertions;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Webhooks.Commands.ProcessPlateGroupWebhook;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebhook;
using Tests.TestHelpers;

namespace Tests.Features.Webhooks.Commands
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class ProcessPlateGroupWebhookCommandHandlerTests : TestBase
    {
        private ProcessPlateGroupWebhookCommandHandler _handler;
        private IGroupWebhookHandler _groupWebhookHandler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _groupWebhookHandler = Substitute.For<IGroupWebhookHandler>();
            
            _handler = new ProcessPlateGroupWebhookCommandHandler(_groupWebhookHandler);
        }

        [Test]
        public void Constructor_WithValidParameters_CreatesInstance()
        {
            // Act
            var handler = new ProcessPlateGroupWebhookCommandHandler(_groupWebhookHandler);

            // Assert
            handler.Should().NotBeNull();
        }

        [Test]
        public async Task Handle_WithValidWebhook_CallsGroupWebhookHandler()
        {
            // Arrange
            var webhook = CreateTestPlateGroupWebhook();
            var command = new ProcessPlateGroupWebhookCommand(webhook, false);
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
            var webhook = CreateTestPlateGroupWebhook();
            var command = new ProcessPlateGroupWebhookCommand(webhook, true);
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
            var command = new ProcessPlateGroupWebhookCommand(null, false);
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
            var webhook = CreateTestPlateGroupWebhook();
            var command = new ProcessPlateGroupWebhookCommand(webhook, false);
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
            var webhook = CreateTestPlateGroupWebhook();
            var command = new ProcessPlateGroupWebhookCommand(webhook, false);
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

        [Test]
        public async Task Handle_WithPreviewGroup_PassesCorrectly()
        {
            // Arrange
            var webhook = CreateTestPlateGroupWebhook(isPreview: true);
            var command = new ProcessPlateGroupWebhookCommand(webhook, false);
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
        public async Task Handle_WithNonPreviewGroup_PassesCorrectly()
        {
            // Arrange
            var webhook = CreateTestPlateGroupWebhook(isPreview: false);
            var command = new ProcessPlateGroupWebhookCommand(webhook, false);
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
        public async Task Handle_WithParkedVehicle_PassesCorrectly()
        {
            // Arrange
            var webhook = CreateTestPlateGroupWebhook(isParked: true);
            var command = new ProcessPlateGroupWebhookCommand(webhook, false);
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
        public async Task Handle_WithVehicleData_PassesCorrectly()
        {
            // Arrange
            var webhook = CreateTestPlateGroupWebhookWithVehicle();
            var command = new ProcessPlateGroupWebhookCommand(webhook, false);
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

        #region Test Helper Methods

        private Webhook CreateTestPlateGroupWebhook(bool isParked = false, bool isPreview = true, string plateNumber = "GROUP123")
        {
            return new Webhook
            {
                DataType = "alpr_group",
                Description = "Test plate group webhook",
                Group = new Group
                {
                    CameraId = 1,
                    IsParked = isParked,
                    IsPreview = isPreview,
                    BestPlateNumber = plateNumber,
                    BestUuid = Guid.NewGuid().ToString(),
                    EpochStart = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    EpochEnd = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 5000,
                    Uuids = new List<string> { Guid.NewGuid().ToString() },
                    Candidates = new List<Candidate>
                    {
                        new Candidate { Plate = plateNumber, Confidence = 95.5 }
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

        private Webhook CreateTestPlateGroupWebhookWithVehicle(string plateNumber = "VEHICLE123")
        {
            var webhook = CreateTestPlateGroupWebhook(plateNumber: plateNumber);
            
            webhook.Group.Vehicle = new Vehicle
            {
                MakeModels = new List<VehicleDetail>
                {
                    new VehicleDetail { Name = "Honda Civic", Confidence = 85.0 }
                },
                Colors = new List<VehicleDetail>
                {
                    new VehicleDetail { Name = "Red", Confidence = 80.0 }
                },
                Makes = new List<VehicleDetail>
                {
                    new VehicleDetail { Name = "Honda", Confidence = 90.0 }
                },
                BodyTypes = new List<VehicleDetail>
                {
                    new VehicleDetail { Name = "Car", Confidence = 95.0 }
                },
                Years = new List<VehicleDetail>
                {
                    new VehicleDetail { Name = "2020", Confidence = 75.0 }
                }
            };
            
            return webhook;
        }

        #endregion
    }
}
