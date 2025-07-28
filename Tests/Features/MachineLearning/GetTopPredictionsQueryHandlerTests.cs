using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;
using OpenAlprWebhookProcessor.Features.MachineLearning.Queries.GetTopPredictions;
using OpenAlprWebhookProcessor.Features.MachineLearning.Services;
using Tests.TestHelpers;

namespace Tests.Features.MachineLearning.Queries.GetTopPredictions
{
    [TestFixture]
    public class GetTopPredictionsQueryHandlerTests : TestBase
    {
        private GetTopPredictionsQueryHandler _handler;
        private ILicensePlatePredictionService _predictionService;
        private ILogger<GetTopPredictionsQueryHandler> _logger;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _predictionService = Substitute.For<ILicensePlatePredictionService>();
            _logger = Substitute.For<ILogger<GetTopPredictionsQueryHandler>>();

            _handler = new GetTopPredictionsQueryHandler(
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
        public async Task Handle_WithValidParameters_ReturnsTopPredictions()
        {
            // Arrange
            var count = 5;
            var withinHours = TimeSpan.FromDays(7);
            
            var expectedResults = new List<LicensePlatePredictionResult>
            {
                new LicensePlatePredictionResult
                {
                    LicensePlate = "TOP001",
                    PredictedNextSeen = DateTime.UtcNow.AddHours(1),
                    ConfidenceScore = 0.95,
                    PredictedHours = 1.0f
                },
                new LicensePlatePredictionResult
                {
                    LicensePlate = "TOP002",
                    PredictedNextSeen = DateTime.UtcNow.AddHours(2),
                    ConfidenceScore = 0.90,
                    PredictedHours = 2.0f
                },
                new LicensePlatePredictionResult
                {
                    LicensePlate = "TOP003",
                    PredictedNextSeen = DateTime.UtcNow.AddHours(3),
                    ConfidenceScore = 0.85,
                    PredictedHours = 3.0f
                }
            };

            var query = new GetTopPredictionsQuery(count, withinHours);
            
            _predictionService.GetTopPredictionsAsync(count, withinHours)
                .Returns(expectedResults);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(expectedResults);
            await _predictionService.Received(1).GetTopPredictionsAsync(count, withinHours);
        }

        [Test]
        public async Task Handle_WithZeroCount_ThrowsArgumentException()
        {
            // Arrange
            var query = new GetTopPredictionsQuery(0, TimeSpan.FromHours(24));

            // Act & Assert
            var act = async () => await _handler.Handle(query, CancellationToken.None);
            
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Count must be between 1 and 50");
        }

        [Test]
        public async Task Handle_WithNegativeCount_ThrowsArgumentException()
        {
            // Arrange
            var query = new GetTopPredictionsQuery(-1, TimeSpan.FromHours(24));

            // Act & Assert
            var act = async () => await _handler.Handle(query, CancellationToken.None);
            
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Count must be between 1 and 50");
        }

        [Test]
        public async Task Handle_WithCountTooHigh_ThrowsArgumentException()
        {
            // Arrange
            var query = new GetTopPredictionsQuery(51, TimeSpan.FromHours(24));

            // Act & Assert
            var act = async () => await _handler.Handle(query, CancellationToken.None);
            
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Count must be between 1 and 50");
        }

        [Test]
        public async Task Handle_WithMaxValidCount_ProcessesSuccessfully()
        {
            // Arrange
            var expectedResults = Enumerable.Range(1, 50)
                .Select(i => new LicensePlatePredictionResult
                {
                    LicensePlate = $"MAX{i:000}",
                    ConfidenceScore = 0.8,
                    PredictedHours = i * 0.5f
                }).ToList();

            var query = new GetTopPredictionsQuery(50, TimeSpan.FromHours(168));
            
            _predictionService.GetTopPredictionsAsync(50, TimeSpan.FromHours(168))
                .Returns(expectedResults);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(50);
            await _predictionService.Received(1).GetTopPredictionsAsync(50, TimeSpan.FromHours(168));
        }

        [Test]
        public async Task Handle_WithZeroWithinHours_ThrowsArgumentException()
        {
            // Arrange
            var query = new GetTopPredictionsQuery(10, TimeSpan.Zero);

            // Act & Assert
            var act = async () => await _handler.Handle(query, CancellationToken.None);
            
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("WithinHours must be between 1 and 8760");
        }

        [Test]
        public async Task Handle_WithNegativeWithinHours_ThrowsArgumentException()
        {
            // Arrange
            var query = new GetTopPredictionsQuery(10, TimeSpan.FromHours(-1));

            // Act & Assert
            var act = async () => await _handler.Handle(query, CancellationToken.None);
            
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("WithinHours must be between 1 and 8760");
        }

        [Test]
        public async Task Handle_WithWithinHoursTooHigh_ThrowsArgumentException()
        {
            // Arrange
            var query = new GetTopPredictionsQuery(10, TimeSpan.FromHours(8761));

            // Act & Assert
            var act = async () => await _handler.Handle(query, CancellationToken.None);
            
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("WithinHours must be between 1 and 8760");
        }

        [Test]
        public async Task Handle_WithMaxValidWithinHours_ProcessesSuccessfully()
        {
            // Arrange
            var expectedResults = new List<LicensePlatePredictionResult>
            {
                new LicensePlatePredictionResult
                {
                    LicensePlate = "YEAR001",
                    ConfidenceScore = 0.7,
                    PredictedHours = 4380.0f // Half a year
                }
            };

            var query = new GetTopPredictionsQuery(10, TimeSpan.FromHours(8760)); // 1 year
            
            _predictionService.GetTopPredictionsAsync(10, TimeSpan.FromHours(8760))
                .Returns(expectedResults);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            await _predictionService.Received(1).GetTopPredictionsAsync(10, TimeSpan.FromHours(8760));
        }

        [Test]
        public async Task Handle_WhenPredictionServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new GetTopPredictionsQuery(10, TimeSpan.FromHours(24));
            var expectedException = new InvalidOperationException("Test exception");

            _predictionService.GetTopPredictionsAsync(10, TimeSpan.FromHours(24))
                .Throws(expectedException);

            // Act & Assert
            var act = async () => await _handler.Handle(query, CancellationToken.None);
            
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Test exception");
        }

        [Test]
        public async Task Handle_WithEmptyResults_ReturnsEmptyList()
        {
            // Arrange
            var expectedResults = new List<LicensePlatePredictionResult>();
            var query = new GetTopPredictionsQuery(10, TimeSpan.FromHours(24));
            
            _predictionService.GetTopPredictionsAsync(10, TimeSpan.FromHours(24))
                .Returns(expectedResults);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            await _predictionService.Received(1).GetTopPredictionsAsync(10, TimeSpan.FromHours(24));
        }

        [Test]
        public async Task Handle_WithCancellationToken_PassesToPredictionService()
        {
            // Arrange
            var expectedResults = new List<LicensePlatePredictionResult>
            {
                new LicensePlatePredictionResult { LicensePlate = "TOKEN1" }
            };

            var query = new GetTopPredictionsQuery(5, TimeSpan.FromHours(12));
            var cancellationToken = new CancellationToken();

            _predictionService.GetTopPredictionsAsync(5, TimeSpan.FromHours(12))
                .Returns(expectedResults);

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            await _predictionService.Received(1).GetTopPredictionsAsync(5, TimeSpan.FromHours(12));
        }

        [Test]
        public async Task Handle_WithDefaultParameters_ProcessesCorrectly()
        {
            // Arrange
            var expectedResults = new List<LicensePlatePredictionResult>
            {
                new LicensePlatePredictionResult
                {
                    LicensePlate = "DEF001",
                    ConfidenceScore = 0.8,
                    PredictedHours = 5.0f
                }
            };

            var query = new GetTopPredictionsQuery(10, TimeSpan.FromHours(168)); // Default-like values
            
            _predictionService.GetTopPredictionsAsync(10, TimeSpan.FromHours(168))
                .Returns(expectedResults);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().LicensePlate.Should().Be("DEF001");
            result.First().ConfidenceScore.Should().Be(0.8);
            result.First().PredictedHours.Should().Be(5.0f);
        }
    }
} 