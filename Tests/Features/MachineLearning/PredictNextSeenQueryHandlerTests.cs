using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;
using OpenAlprWebhookProcessor.Features.MachineLearning.Queries.PredictNextSeen;
using OpenAlprWebhookProcessor.Features.MachineLearning.Services;
using Tests.TestHelpers;

namespace Tests.Features.MachineLearning.Queries.PredictNextSeen
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class PredictNextSeenQueryHandlerTests : TestBase
    {
        private PredictNextSeenQueryHandler _handler;
        private ILicensePlatePredictionService _predictionService;
        private ILogger<PredictNextSeenQueryHandler> _logger;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _predictionService = Substitute.For<ILicensePlatePredictionService>();
            _logger = Substitute.For<ILogger<PredictNextSeenQueryHandler>>();

            _handler = new PredictNextSeenQueryHandler(
                UnitOfWork,
                _predictionService,
                _logger);
        }

        [Test]
        public void Constructor_WithValidParameters_InitializesCorrectly()
        {
            _handler.Should().NotBeNull();
        }

        [Test]
        public async Task Handle_WithValidInput_ReturnsPredictionResult()
        {
            // Arrange
            var input = new LicensePlateInput
            {
                LicensePlate = "ABC123",
                CameraId = 1,
                LastSeen = DateTime.UtcNow.AddHours(-2),
                VehicleType = "Car",
                VehicleColor = "Blue"
            };

            var expectedResult = new LicensePlatePredictionResult
            {
                LicensePlate = "ABC123",
                PredictedNextSeen = DateTime.UtcNow.AddHours(4),
                PredictedHours = 4.0f,
                ConfidenceScore = 0.85,
                TotalHistoricalVisits = 15,
                AverageTimeBetweenVisits = 12.5,
                LastSeen = input.LastSeen,
                ModelVersion = "ML.NET-v1.0-test",
                PredictionMadeAt = DateTime.UtcNow
            };

            var query = new PredictNextSeenQuery(input);
            
            _predictionService.PredictNextSeenAsync(input)
                .Returns(expectedResult);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedResult);
            await _predictionService.Received(1).PredictNextSeenAsync(input);
        }

        [Test]
        public async Task Handle_WithNullInput_ThrowsArgumentException()
        {
            // Arrange
            var query = new PredictNextSeenQuery(null);

            // Act & Assert
            var act = async () => await _handler.Handle(query, CancellationToken.None);
            
            await act.Should().ThrowExactlyAsync<ArgumentException>()
                .WithMessage("License plate is required");
        }

        [Test]
        public async Task Handle_WithEmptyLicensePlate_ThrowsArgumentException()
        {
            // Arrange
            var input = new LicensePlateInput
            {
                LicensePlate = "",
                CameraId = 1,
                LastSeen = DateTime.UtcNow,
                VehicleType = "Car",
                VehicleColor = "Blue"
            };

            var query = new PredictNextSeenQuery(input);

            // Act & Assert
            var act = async () => await _handler.Handle(query, CancellationToken.None);
            
            await act.Should().ThrowExactlyAsync<ArgumentException>()
                .WithMessage("License plate is required");
        }

        [Test]
        public async Task Handle_WithNullLicensePlate_ThrowsArgumentException()
        {
            // Arrange
            var input = new LicensePlateInput
            {
                LicensePlate = null,
                CameraId = 1,
                LastSeen = DateTime.UtcNow,
                VehicleType = "Car",
                VehicleColor = "Blue"
            };

            var query = new PredictNextSeenQuery(input);

            // Act & Assert
            var act = async () => await _handler.Handle(query, CancellationToken.None);
            
            await act.Should().ThrowExactlyAsync<ArgumentException>()
                .WithMessage("License plate is required");
        }

        [Test]
        public async Task Handle_WhenPredictionServiceThrows_PropagatesException()
        {
            // Arrange
            var input = new LicensePlateInput
            {
                LicensePlate = "ABC123",
                CameraId = 1,
                LastSeen = DateTime.UtcNow,
                VehicleType = "Car",
                VehicleColor = "Blue"
            };

            var query = new PredictNextSeenQuery(input);
            var expectedException = new InvalidOperationException("Test exception");

            _predictionService.PredictNextSeenAsync(input)
                .Throws(expectedException);

            // Act & Assert
            var act = async () => await _handler.Handle(query, CancellationToken.None);
            
            await act.Should().ThrowExactlyAsync<InvalidOperationException>()
                .WithMessage("Test exception");
        }

        [Test]
        public async Task Handle_WithCancellationToken_PassesToPredictionService()
        {
            // Arrange
            var input = new LicensePlateInput
            {
                LicensePlate = "ABC123",
                CameraId = 1,
                LastSeen = DateTime.UtcNow,
                VehicleType = "Car",
                VehicleColor = "Blue"
            };

            var expectedResult = new LicensePlatePredictionResult
            {
                LicensePlate = "ABC123",
                PredictedNextSeen = DateTime.UtcNow.AddHours(4),
                PredictedHours = 4.0f,
                ConfidenceScore = 0.85
            };

            var query = new PredictNextSeenQuery(input);
            var cancellationToken = new CancellationToken();

            _predictionService.PredictNextSeenAsync(input)
                .Returns(expectedResult);

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            await _predictionService.Received(1).PredictNextSeenAsync(input);
        }

        [Test]
        public async Task Handle_WithMinimalInput_StillProcessesCorrectly()
        {
            // Arrange
            var input = new LicensePlateInput
            {
                LicensePlate = "MIN001"
            };

            var expectedResult = new LicensePlatePredictionResult
            {
                LicensePlate = "MIN001",
                PredictedNextSeen = DateTime.UtcNow.AddHours(6),
                PredictedHours = 6.0f,
                ConfidenceScore = 0.5
            };

            var query = new PredictNextSeenQuery(input);
            
            _predictionService.PredictNextSeenAsync(input)
                .Returns(expectedResult);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.LicensePlate.Should().Be("MIN001");
            result.PredictedHours.Should().Be(6.0f);
            result.ConfidenceScore.Should().Be(0.5);
        }
    }
} 