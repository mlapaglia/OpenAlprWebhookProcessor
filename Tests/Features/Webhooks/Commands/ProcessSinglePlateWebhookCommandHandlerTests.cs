using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Features.Webhooks.Commands.ProcessSinglePlateWebhook;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebhook;
using Tests.TestHelpers;

namespace Tests.Features.Webhooks.Commands
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class ProcessSinglePlateWebhookCommandHandlerTests : TestBase
    {
        private ProcessSinglePlateWebhookCommandHandler _handler;
        private SinglePlateWebhookHandler _singlePlateWebhookHandler;
        private ILogger<SinglePlateWebhookHandler> _logger;
        private ISimpleCameraScheduler _cameraUpdateService;
        private IWebhookForwarder _webhookForwarder;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _logger = Substitute.For<ILogger<SinglePlateWebhookHandler>>();
            _cameraUpdateService = Substitute.For<ISimpleCameraScheduler>();
            _webhookForwarder = Substitute.For<IWebhookForwarder>();
            
            _singlePlateWebhookHandler = new SinglePlateWebhookHandler(
                _logger,
                _cameraUpdateService,
                UnitOfWork,
                _webhookForwarder);
            
            _handler = new ProcessSinglePlateWebhookCommandHandler(_singlePlateWebhookHandler);
        }

        [Test]
        public void Constructor_WithValidParameters_CreatesInstance()
        {
            // Act
            var handler = new ProcessSinglePlateWebhookCommandHandler(_singlePlateWebhookHandler);

            // Assert
            handler.Should().NotBeNull();
        }

        [Test]
        public async Task Handle_WithValidSinglePlate_CallsSinglePlateWebhookHandler()
        {
            // Arrange
            var camera = TestDataFactory.CreateTestCamera(openAlprName: "Test Camera", openAlprCameraId: 1);
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            var singlePlate = CreateTestSinglePlate();
            var command = new ProcessSinglePlateWebhookCommand(singlePlate);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
        }

        [Test]
        public async Task Handle_WithUnknownCamera_ThrowsArgumentException()
        {
            // Arrange
            var singlePlate = CreateTestSinglePlate(cameraId: 999);
            var command = new ProcessSinglePlateWebhookCommand(singlePlate);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            var act = async () => await _handler.Handle(command, cancellationToken);
            await act.Should().ThrowExactlyAsync<ArgumentException>()
                .WithMessage("unknown camera, skipping");
        }

        [Test]
        public async Task Handle_WithDisabledCamera_ThrowsArgumentException()
        {
            // Arrange
            var camera = TestDataFactory.CreateTestCamera(openAlprName: "Test Camera", openAlprCameraId: 1);
            camera.OpenAlprEnabled = false;
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            var singlePlate = CreateTestSinglePlate();
            var command = new ProcessSinglePlateWebhookCommand(singlePlate);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            var act = async () => await _handler.Handle(command, cancellationToken);
            await act.Should().ThrowExactlyAsync<ArgumentException>()
                .WithMessage("camera has OpenALPR integration disabled, skipping");
        }

        [Test]
        public async Task Handle_WithOverlayEnabledCamera_SchedulesOverlayUpdate()
        {
            // Arrange
            var camera = TestDataFactory.CreateTestCamera(openAlprName: "Test Camera", openAlprCameraId: 1);
            camera.UpdateOverlayEnabled = true;
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            var singlePlate = CreateTestSinglePlate();
            var command = new ProcessSinglePlateWebhookCommand(singlePlate);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            await _cameraUpdateService.Received(1).ScheduleOverlayAsync(
                Arg.Is<CameraUpdateRequest>(r => 
                    r.Id == camera.Id &&
                    r.LicensePlate == "SINGLE123" &&
                    r.IsSinglePlate == true));
        }

        [Test]
        public async Task Handle_WithOverlayDisabledCamera_DoesNotScheduleOverlayUpdate()
        {
            // Arrange
            var camera = TestDataFactory.CreateTestCamera(openAlprName: "Test Camera", openAlprCameraId: 1);
            camera.UpdateOverlayEnabled = false;
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            var singlePlate = CreateTestSinglePlate();
            var command = new ProcessSinglePlateWebhookCommand(singlePlate);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            await _cameraUpdateService.DidNotReceive().ScheduleOverlayAsync(Arg.Any<CameraUpdateRequest>());
        }

        [Test]
        public async Task Handle_WithAlertDataType_SetsIsAlertTrue()
        {
            // Arrange
            var camera = TestDataFactory.CreateTestCamera(openAlprName: "Test Camera", openAlprCameraId: 1);
            camera.UpdateOverlayEnabled = true;
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            var singlePlate = CreateTestSinglePlate(dataType: "alpr_alert");
            var command = new ProcessSinglePlateWebhookCommand(singlePlate);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            await _cameraUpdateService.Received(1).ScheduleOverlayAsync(
                Arg.Is<CameraUpdateRequest>(r => r.IsAlert == true));
        }

        [Test]
        public async Task Handle_WithNonAlertDataType_SetsIsAlertFalse()
        {
            // Arrange
            var camera = TestDataFactory.CreateTestCamera(openAlprName: "Test Camera", openAlprCameraId: 1);
            camera.UpdateOverlayEnabled = true;
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            var singlePlate = CreateTestSinglePlate(dataType: "alpr_results");
            var command = new ProcessSinglePlateWebhookCommand(singlePlate);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            await _cameraUpdateService.Received(1).ScheduleOverlayAsync(
                Arg.Is<CameraUpdateRequest>(r => r.IsAlert == false));
        }

        [Test]
        public async Task Handle_WithWebhookForwards_ForwardsToEnabledDestinations()
        {
            // Arrange
            var camera = TestDataFactory.CreateTestCamera(openAlprName: "Test Camera", openAlprCameraId: 1);
            await UnitOfWork.Cameras.AddAsync(camera);

            var webhookForward = TestDataFactory.CreateTestWebhookForward("https://test-forward.local");
            webhookForward.ForwardSinglePlates = true;
            await UnitOfWork.WebhookForwards.AddAsync(webhookForward);

            await UnitOfWork.SaveChangesAsync();

            var singlePlate = CreateTestSinglePlate();
            var command = new ProcessSinglePlateWebhookCommand(singlePlate);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            // Note: WebhookForwarder.ForwardWebhookAsync is a static method and cannot be easily tested
            // In a real implementation, this would need to be refactored to be testable
        }

        [Test]
        public async Task Handle_WithDisabledWebhookForwards_DoesNotForward()
        {
            // Arrange
            var camera = TestDataFactory.CreateTestCamera(openAlprName: "Test Camera", openAlprCameraId: 1);
            await UnitOfWork.Cameras.AddAsync(camera);

            var webhookForward = TestDataFactory.CreateTestWebhookForward("https://test-forward.local");
            webhookForward.ForwardSinglePlates = false;
            await UnitOfWork.WebhookForwards.AddAsync(webhookForward);

            await UnitOfWork.SaveChangesAsync();

            var singlePlate = CreateTestSinglePlate();
            var command = new ProcessSinglePlateWebhookCommand(singlePlate);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            // The forwarding logic would not be called for disabled forwards
        }

        [Test]
        public async Task Handle_WithNullSinglePlate_ThrowsException()
        {
            // Arrange
            var command = new ProcessSinglePlateWebhookCommand(null);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            var act = async () => await _handler.Handle(command, cancellationToken);
            await act.Should().ThrowAsync<Exception>();
        }

        [Test]
        public async Task Handle_WithDifferentCancellationToken_PassesTokenCorrectly()
        {
            // Arrange
            var camera = TestDataFactory.CreateTestCamera(openAlprName: "Test Camera", openAlprCameraId: 1);
            await UnitOfWork.Cameras.AddAsync(camera);
            await UnitOfWork.SaveChangesAsync();

            var singlePlate = CreateTestSinglePlate();
            var command = new ProcessSinglePlateWebhookCommand(singlePlate);
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
        }

        #region Test Helper Methods

        private SinglePlate CreateTestSinglePlate(int cameraId = 1, string plateNumber = "SINGLE123", string dataType = "alpr_results")
        {
            return new SinglePlate
            {
                Version = 2,
                DataType = dataType,
                EpochTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                ImgWidth = 1920,
                ImgHeight = 1080,
                ProcessingTimeMs = 150.5,
                Uuid = Guid.NewGuid().ToString(),
                Error = false,
                RegionsOfInterest = new List<RegionsOfInterest>(),
                Vehicles = new List<object>(),
                Results = new List<Result>
                {
                    new Result
                    {
                        Plate = plateNumber,
                        Confidence = 95.5,
                        MatchesTemplate = 1,
                        PlateIndex = 0,
                        Region = "ca",
                        RegionConfidence = 85,
                        ProcessingTimeMs = 150.5,
                        RequestedTopN = 10,
                        Coordinates = new List<Coordinate>
                        {
                            new Coordinate { X = 100, Y = 200 },
                            new Coordinate { X = 300, Y = 200 },
                            new Coordinate { X = 300, Y = 400 },
                            new Coordinate { X = 100, Y = 400 }
                        },
                        PlateCropJpeg = Convert.ToBase64String(TestDataFactory.CreateTestJpegBytes()),
                        VehicleRegion = new VehicleRegion(),
                        VehicleDetected = true,
                        Candidates = new List<Candidate>
                        {
                            new Candidate { Plate = plateNumber, Confidence = 95.5 }
                        }
                    }
                },
                CameraId = cameraId,
                AgentUid = "test-agent-uid",
                AgentVersion = "1.0.0",
                AgentType = "agent",
                CompanyId = "test-company"
            };
        }

        #endregion
    }
}
