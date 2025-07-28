using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;
using OpenAlprWebhookProcessor.Features.MachineLearning.Queries.PredictBatch;
using OpenAlprWebhookProcessor.Features.MachineLearning.Services;
using Tests.TestHelpers;

namespace Tests.Features.MachineLearning.Queries.PredictBatch
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class PredictBatchQueryHandlerTests : TestBase
    {
        private PredictBatchQueryHandler _handler;
        private ILicensePlatePredictionService _predictionService;
        private ILogger<PredictBatchQueryHandler> _logger;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _predictionService = Substitute.For<ILicensePlatePredictionService>();
            _logger = Substitute.For<ILogger<PredictBatchQueryHandler>>();

            _handler = new PredictBatchQueryHandler(
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
        public async Task Handle_WithValidInputs_ReturnsBatchPredictionResults()
        {
            // Arrange
            var inputs = new List<LicensePlateInput>
            {
                new LicensePlateInput
                {
                    LicensePlate = "ABC123",
                    CameraId = 1,
                    LastSeen = DateTime.UtcNow.AddHours(-2),
                    VehicleType = "Car",
                    VehicleColor = "Blue"
                },
                new LicensePlateInput
                {
                    LicensePlate = "XYZ789",
                    CameraId = 2,
                    LastSeen = DateTime.UtcNow.AddHours(-1),
                    VehicleType = "Truck",
                    VehicleColor = "Red"
                }
            };

            var expectedResults = new List<LicensePlatePredictionResult>
            {
                new LicensePlatePredictionResult
                {
                    LicensePlate = "ABC123",
                    PredictedNextSeen = DateTime.UtcNow.AddHours(4),
                    PredictedHours = 4.0f,
                    ConfidenceScore = 0.85
                },
                new LicensePlatePredictionResult
                {
                    LicensePlate = "XYZ789",
                    PredictedNextSeen = DateTime.UtcNow.AddHours(6),
                    PredictedHours = 6.0f,
                    ConfidenceScore = 0.75
                }
            };

            var query = new PredictBatchQuery(inputs);
            
            _predictionService.PredictBatchAsync(inputs)
                .Returns(expectedResults);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(expectedResults);
            await _predictionService.Received(1).PredictBatchAsync(inputs);
        }

        [Test]
        public async Task Handle_WithNullInputs_ThrowsArgumentException()
        {
            // Arrange
            var query = new PredictBatchQuery(null);

            // Act & Assert
            var act = async () => await _handler.Handle(query, CancellationToken.None);
            
            await act.Should().ThrowExactlyAsync<ArgumentException>()
                .WithMessage("At least one license plate input is required");
        }

        [Test]
        public async Task Handle_WithEmptyInputsList_ThrowsArgumentException()
        {
            // Arrange
            var inputs = new List<LicensePlateInput>();
            var query = new PredictBatchQuery(inputs);

            // Act & Assert
            var act = async () => await _handler.Handle(query, CancellationToken.None);
            
            await act.Should().ThrowExactlyAsync<ArgumentException>()
                .WithMessage("At least one license plate input is required");
        }

        [Test]
        public async Task Handle_WithTooManyInputs_ThrowsArgumentException()
        {
            // Arrange
            var inputs = Enumerable.Range(1, 101)
                .Select(i => new LicensePlateInput { LicensePlate = $"PLATE{i:000}" })
                .ToList();
            
            var query = new PredictBatchQuery(inputs);

            // Act & Assert
            var act = async () => await _handler.Handle(query, CancellationToken.None);
            
            await act.Should().ThrowExactlyAsync<ArgumentException>()
                .WithMessage("Maximum 100 predictions per batch");
        }

        [Test]
        public async Task Handle_WithExactlyMaxInputs_ProcessesSuccessfully()
        {
            // Arrange
            var inputs = Enumerable.Range(1, 100)
                .Select(i => new LicensePlateInput { LicensePlate = $"PLATE{i:000}" })
                .ToList();

            var expectedResults = inputs.Select(input => new LicensePlatePredictionResult
            {
                LicensePlate = input.LicensePlate,
                PredictedNextSeen = DateTime.UtcNow.AddHours(4),
                PredictedHours = 4.0f,
                ConfidenceScore = 0.8
            }).ToList();

            var query = new PredictBatchQuery(inputs);
            
            _predictionService.PredictBatchAsync(inputs)
                .Returns(expectedResults);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(100);
            await _predictionService.Received(1).PredictBatchAsync(inputs);
        }

        [Test]
        public async Task Handle_WhenPredictionServiceThrows_PropagatesException()
        {
            // Arrange
            var inputs = new List<LicensePlateInput>
            {
                new LicensePlateInput { LicensePlate = "ABC123" }
            };

            var query = new PredictBatchQuery(inputs);
            var expectedException = new InvalidOperationException("Test exception");

            _predictionService.PredictBatchAsync(inputs)
                .Throws(expectedException);

            // Act & Assert
            var act = async () => await _handler.Handle(query, CancellationToken.None);
            
            await act.Should().ThrowExactlyAsync<InvalidOperationException>()
                .WithMessage("Test exception");
        }

        [Test]
        public async Task Handle_WithSingleInput_ProcessesCorrectly()
        {
            // Arrange
            var inputs = new List<LicensePlateInput>
            {
                new LicensePlateInput
                {
                    LicensePlate = "SINGLE1",
                    CameraId = 1,
                    LastSeen = DateTime.UtcNow,
                    VehicleType = "Car",
                    VehicleColor = "White"
                }
            };

            var expectedResults = new List<LicensePlatePredictionResult>
            {
                new LicensePlatePredictionResult
                {
                    LicensePlate = "SINGLE1",
                    PredictedNextSeen = DateTime.UtcNow.AddHours(8),
                    PredictedHours = 8.0f,
                    ConfidenceScore = 0.9
                }
            };

            var query = new PredictBatchQuery(inputs);
            
            _predictionService.PredictBatchAsync(inputs)
                .Returns(expectedResults);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().LicensePlate.Should().Be("SINGLE1");
            result.First().ConfidenceScore.Should().Be(0.9);
        }

        [Test]
        public async Task Handle_WithCancellationToken_PassesToPredictionService()
        {
            // Arrange
            var inputs = new List<LicensePlateInput>
            {
                new LicensePlateInput { LicensePlate = "TOKEN1" }
            };

            var expectedResults = new List<LicensePlatePredictionResult>
            {
                new LicensePlatePredictionResult
                {
                    LicensePlate = "TOKEN1",
                    PredictedNextSeen = DateTime.UtcNow.AddHours(2),
                    PredictedHours = 2.0f,
                    ConfidenceScore = 0.7
                }
            };

            var query = new PredictBatchQuery(inputs);
            var cancellationToken = new CancellationToken();

            _predictionService.PredictBatchAsync(inputs)
                .Returns(expectedResults);

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            await _predictionService.Received(1).PredictBatchAsync(inputs);
        }

        [Test]
        public async Task Handle_WithMixedResults_ProcessesAllCorrectly()
        {
            // Arrange
            var inputs = new List<LicensePlateInput>
            {
                new LicensePlateInput { LicensePlate = "HIGH001" },
                new LicensePlateInput { LicensePlate = "LOW002" },
                new LicensePlateInput { LicensePlate = "MED003" }
            };

            var expectedResults = new List<LicensePlatePredictionResult>
            {
                new LicensePlatePredictionResult
                {
                    LicensePlate = "HIGH001",
                    ConfidenceScore = 0.95,
                    PredictedHours = 2.0f
                },
                new LicensePlatePredictionResult
                {
                    LicensePlate = "LOW002",
                    ConfidenceScore = 0.3,
                    PredictedHours = 12.0f
                },
                new LicensePlatePredictionResult
                {
                    LicensePlate = "MED003",
                    ConfidenceScore = 0.65,
                    PredictedHours = 6.0f
                }
            };

            var query = new PredictBatchQuery(inputs);
            
            _predictionService.PredictBatchAsync(inputs)
                .Returns(expectedResults);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            
            var highResult = result.First(r => r.LicensePlate == "HIGH001");
            highResult.ConfidenceScore.Should().Be(0.95);
            highResult.PredictedHours.Should().Be(2.0f);
            
            var lowResult = result.First(r => r.LicensePlate == "LOW002");
            lowResult.ConfidenceScore.Should().Be(0.3);
            lowResult.PredictedHours.Should().Be(12.0f);
            
            var medResult = result.First(r => r.LicensePlate == "MED003");
            medResult.ConfidenceScore.Should().Be(0.65);
            medResult.PredictedHours.Should().Be(6.0f);
        }
    }
} 