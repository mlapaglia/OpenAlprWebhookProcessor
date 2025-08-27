using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebhook;
using System.Linq.Expressions;
using Tests.TestHelpers;

namespace Tests.Features.Webhooks.WebhookProcessor
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class SinglePlateWebhookHandlerTests : TestBase
    {
        private SinglePlateWebhookHandler _handler;
        private ILogger<SinglePlateWebhookHandler> _logger;
        private ISimpleCameraScheduler _cameraUpdateService;
        private IUnitOfWork _unitOfWork;
        private IWebhookForwarder _webhookForwarder;
        private IRepository<WebhookForward> _webhookForwardRepository;
        private IRepository<OpenAlprWebhookProcessor.Data.Camera> _cameraRepository;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _logger = Substitute.For<ILogger<SinglePlateWebhookHandler>>();
            _cameraUpdateService = Substitute.For<ISimpleCameraScheduler>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _webhookForwarder = Substitute.For<IWebhookForwarder>();
            _webhookForwardRepository = Substitute.For<IRepository<WebhookForward>>();
            _cameraRepository = Substitute.For<IRepository<OpenAlprWebhookProcessor.Data.Camera>>();
            
            _unitOfWork.WebhookForwards.Returns(_webhookForwardRepository);
            _unitOfWork.Cameras.Returns(_cameraRepository);
            
            _handler = new SinglePlateWebhookHandler(_logger, _cameraUpdateService, _unitOfWork, _webhookForwarder);
        }

        [TearDown]
        public new void TearDown()
        {
            _unitOfWork?.Dispose();
            base.TearDown();
        }

        [Test]
        public async Task HandleWebhookAsync_WithUnknownCamera_ThrowsArgumentException()
        {
            // Arrange
            var webhook = CreateSampleWebhook();
            
            _cameraRepository.FirstOrDefaultAsync(
                Arg.Any<Expression<Func<OpenAlprWebhookProcessor.Data.Camera, bool>>>(),
                Arg.Any<CancellationToken>())
                .Returns((OpenAlprWebhookProcessor.Data.Camera)null);

            // Act & Assert
            await FluentActions.Invoking(async () => 
                await _handler.HandleWebhookAsync(webhook, CancellationToken.None))
                .Should().ThrowExactlyAsync<ArgumentException>()
                .WithMessage("unknown camera, skipping");
        }

        [Test]
        public async Task HandleWebhookAsync_WithOpenAlprDisabled_ThrowsArgumentException()
        {
            // Arrange
            var webhook = CreateSampleWebhook();
            var camera = new OpenAlprWebhookProcessor.Data.Camera
            {
                Id = Guid.NewGuid(),
                OpenAlprCameraId = webhook.CameraId,
                OpenAlprEnabled = false,
                UpdateOverlayEnabled = false
            };
            
            _cameraRepository.FirstOrDefaultAsync(
                Arg.Any<Expression<Func<OpenAlprWebhookProcessor.Data.Camera, bool>>>(),
                Arg.Any<CancellationToken>())
                .Returns(camera);

            // Act & Assert
            await FluentActions.Invoking(async () => 
                await _handler.HandleWebhookAsync(webhook, CancellationToken.None))
                .Should().ThrowExactlyAsync<ArgumentException>()
                .WithMessage("camera has OpenALPR integration disabled, skipping");
        }

        [Test]
        public async Task HandleWebhookAsync_WithOverlayEnabled_SchedulesOverlayRequest()
        {
            // Arrange
            var webhook = CreateSampleWebhook();
            var camera = new OpenAlprWebhookProcessor.Data.Camera
            {
                Id = Guid.NewGuid(),
                OpenAlprCameraId = webhook.CameraId,
                OpenAlprEnabled = true,
                UpdateOverlayEnabled = true
            };
            
            _cameraRepository.FirstOrDefaultAsync(
                Arg.Any<Expression<Func<OpenAlprWebhookProcessor.Data.Camera, bool>>>(),
                Arg.Any<CancellationToken>())
                .Returns(camera);
            
            _webhookForwardRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(new List<WebhookForward>());

            // Act
            await _handler.HandleWebhookAsync(webhook, CancellationToken.None);

            // Assert
            await _cameraUpdateService.Received(1).ScheduleOverlayAsync(
                Arg.Is<CameraUpdateRequest>(req => 
                    req.LicensePlateImageUuid == webhook.Uuid &&
                    req.LicensePlate == webhook.Results[0].Plate &&
                    req.Id == camera.Id &&
                    req.OpenAlprProcessingTimeMs == Math.Round(webhook.ProcessingTimeMs, 2) &&
                    req.ProcessedPlateConfidence == Math.Round(webhook.Results[0].Confidence, 2) &&
                    req.IsAlert == (webhook.DataType == "alpr_alert") &&
                    req.IsSinglePlate == true),
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task HandleWebhookAsync_WithOverlayDisabled_DoesNotScheduleOverlayRequest()
        {
            // Arrange
            var webhook = CreateSampleWebhook();
            var camera = new OpenAlprWebhookProcessor.Data.Camera
            {
                Id = Guid.NewGuid(),
                OpenAlprCameraId = webhook.CameraId,
                OpenAlprEnabled = true,
                UpdateOverlayEnabled = false
            };
            
            _cameraRepository.FirstOrDefaultAsync(
                Arg.Any<Expression<Func<OpenAlprWebhookProcessor.Data.Camera, bool>>>(),
                Arg.Any<CancellationToken>())
                .Returns(camera);
            
            _webhookForwardRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(new List<WebhookForward>());

            // Act
            await _handler.HandleWebhookAsync(webhook, CancellationToken.None);

            // Assert
            await _cameraUpdateService.DidNotReceive().ScheduleOverlayAsync(
                Arg.Any<CameraUpdateRequest>(),
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task HandleWebhookAsync_WithForwardingSinglePlates_ForwardsWebhook()
        {
            // Arrange
            var webhook = CreateSampleWebhook();
            var camera = new OpenAlprWebhookProcessor.Data.Camera
            {
                Id = Guid.NewGuid(),
                OpenAlprCameraId = webhook.CameraId,
                OpenAlprEnabled = true,
                UpdateOverlayEnabled = false
            };
            
            var webhookForward = new WebhookForward
            {
                Id = Guid.NewGuid(),
                FowardingDestination = new Uri("https://example.com/webhook"),
                ForwardSinglePlates = true,
                IgnoreSslErrors = false
            };
            
            _cameraRepository.FirstOrDefaultAsync(
                Arg.Any<Expression<Func<OpenAlprWebhookProcessor.Data.Camera, bool>>>(),
                Arg.Any<CancellationToken>())
                .Returns(camera);
            
            _webhookForwardRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(new List<WebhookForward> { webhookForward });

            // Act
            await _handler.HandleWebhookAsync(webhook, CancellationToken.None);

            // Assert
            await _webhookForwarder.Received(1).ForwardWebhookAsync(
                webhook,
                webhookForward.FowardingDestination,
                webhookForward.IgnoreSslErrors,
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task HandleWebhookAsync_WithForwardingDisabled_DoesNotForwardWebhook()
        {
            // Arrange
            var webhook = CreateSampleWebhook();
            var camera = new OpenAlprWebhookProcessor.Data.Camera
            {
                Id = Guid.NewGuid(),
                OpenAlprCameraId = webhook.CameraId,
                OpenAlprEnabled = true,
                UpdateOverlayEnabled = false
            };
            
            var webhookForward = new WebhookForward
            {
                Id = Guid.NewGuid(),
                FowardingDestination = new Uri("https://example.com/webhook"),
                ForwardSinglePlates = false, // Forwarding disabled
                IgnoreSslErrors = false
            };
            
            _cameraRepository.FirstOrDefaultAsync(
                Arg.Any<Expression<Func<OpenAlprWebhookProcessor.Data.Camera, bool>>>(),
                Arg.Any<CancellationToken>())
                .Returns(camera);
            
            _webhookForwardRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(new List<WebhookForward> { webhookForward });

            // Act
            await _handler.HandleWebhookAsync(webhook, CancellationToken.None);

            // Assert
            await _webhookForwarder.DidNotReceive().ForwardWebhookAsync(
                Arg.Any<object>(),
                Arg.Any<Uri>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task HandleWebhookAsync_WithMultipleForwards_ProcessesAll()
        {
            // Arrange
            var webhook = CreateSampleWebhook();
            var camera = new OpenAlprWebhookProcessor.Data.Camera
            {
                Id = Guid.NewGuid(),
                OpenAlprCameraId = webhook.CameraId,
                OpenAlprEnabled = true,
                UpdateOverlayEnabled = false
            };
            
            var webhookForwards = new List<WebhookForward>
            {
                new WebhookForward
                {
                    Id = Guid.NewGuid(),
                    FowardingDestination = new Uri("https://example1.com/webhook"),
                    ForwardSinglePlates = true,
                    IgnoreSslErrors = false
                },
                new WebhookForward
                {
                    Id = Guid.NewGuid(),
                    FowardingDestination = new Uri("https://example2.com/webhook"),
                    ForwardSinglePlates = true,
                    IgnoreSslErrors = true
                }
            };
            
            _cameraRepository.FirstOrDefaultAsync(
                Arg.Any<Expression<Func<OpenAlprWebhookProcessor.Data.Camera, bool>>>(),
                Arg.Any<CancellationToken>())
                .Returns(camera);
            
            _webhookForwardRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(webhookForwards);

            // Act
            await _handler.HandleWebhookAsync(webhook, CancellationToken.None);

            // Assert
            await _webhookForwarder.Received(2).ForwardWebhookAsync(
                webhook,
                Arg.Any<Uri>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task HandleWebhookAsync_WithAlertDataType_SetsIsAlertToTrue()
        {
            // Arrange
            var webhook = CreateSampleWebhook();
            webhook.DataType = "alpr_alert";
            
            var camera = new OpenAlprWebhookProcessor.Data.Camera
            {
                Id = Guid.NewGuid(),
                OpenAlprCameraId = webhook.CameraId,
                OpenAlprEnabled = true,
                UpdateOverlayEnabled = true
            };
            
            _cameraRepository.FirstOrDefaultAsync(
                Arg.Any<Expression<Func<OpenAlprWebhookProcessor.Data.Camera, bool>>>(),
                Arg.Any<CancellationToken>())
                .Returns(camera);
            
            _webhookForwardRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(new List<WebhookForward>());

            // Act
            await _handler.HandleWebhookAsync(webhook, CancellationToken.None);

            // Assert
            await _cameraUpdateService.Received(1).ScheduleOverlayAsync(
                Arg.Is<CameraUpdateRequest>(req => req.IsAlert == true),
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task HandleWebhookAsync_WithNonAlertDataType_SetsIsAlertToFalse()
        {
            // Arrange
            var webhook = CreateSampleWebhook();
            webhook.DataType = "alpr_results";
            
            var camera = new OpenAlprWebhookProcessor.Data.Camera
            {
                Id = Guid.NewGuid(),
                OpenAlprCameraId = webhook.CameraId,
                OpenAlprEnabled = true,
                UpdateOverlayEnabled = true
            };
            
            _cameraRepository.FirstOrDefaultAsync(
                Arg.Any<Expression<Func<OpenAlprWebhookProcessor.Data.Camera, bool>>>(),
                Arg.Any<CancellationToken>())
                .Returns(camera);
            
            _webhookForwardRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(new List<WebhookForward>());

            // Act
            await _handler.HandleWebhookAsync(webhook, CancellationToken.None);

            // Assert
            await _cameraUpdateService.Received(1).ScheduleOverlayAsync(
                Arg.Is<CameraUpdateRequest>(req => req.IsAlert == false),
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task HandleWebhookAsync_WithCancellation_PassesCancellationToken()
        {
            // Arrange
            var webhook = CreateSampleWebhook();
            var camera = new OpenAlprWebhookProcessor.Data.Camera
            {
                Id = Guid.NewGuid(),
                OpenAlprCameraId = webhook.CameraId,
                OpenAlprEnabled = true,
                UpdateOverlayEnabled = false
            };
            
            using var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            
            _cameraRepository.FirstOrDefaultAsync(
                Arg.Any<Expression<Func<OpenAlprWebhookProcessor.Data.Camera, bool>>>(),
                Arg.Any<CancellationToken>())
                .Returns(camera);
            
            _webhookForwardRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(new List<WebhookForward>());

            // Act
            await _handler.HandleWebhookAsync(webhook, cancellationToken);

            // Assert
            await _cameraRepository.Received(1).FirstOrDefaultAsync(
                Arg.Any<Expression<Func<OpenAlprWebhookProcessor.Data.Camera, bool>>>(),
                Arg.Is<CancellationToken>(ct => ct == cancellationToken));
            
            await _webhookForwardRepository.Received(1).GetAllAsync(
                Arg.Is<CancellationToken>(ct => ct == cancellationToken));
        }

        private static SinglePlate CreateSampleWebhook()
        {
            return new SinglePlate
            {
                Version = 1,
                DataType = "alpr_results",
                EpochTime = 1625097600,
                ImgWidth = 1920,
                ImgHeight = 1080,
                ProcessingTimeMs = 123.45,
                Uuid = "test-uuid-123",
                Error = false,
                CameraId = 1,
                AgentUid = "test-agent",
                AgentVersion = "1.0.0",
                AgentType = "camera",
                CompanyId = "test-company",
                Results = new List<Result>
                {
                    new Result
                    {
                        Plate = "ABC123",
                        Confidence = 95.5,
                        PlateCropJpeg = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 }),
                        Region = "us",
                        RegionConfidence = 90
                    }
                },
                RegionsOfInterest = new List<RegionsOfInterest>(),
                Vehicles = new List<object>()
            };
        }
    }
}