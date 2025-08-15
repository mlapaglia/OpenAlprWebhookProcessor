using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.MachineLearning.Queries.GetModelInfo;
using OpenAlprWebhookProcessor.Features.MachineLearning.Services;
using Tests.TestHelpers;

namespace Tests.Features.MachineLearning.Queries.GetModelInfo
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class GetModelInfoQueryHandlerTests : TestBase
    {
        private GetModelInfoQueryHandler _handler;
        private ILicensePlatePredictionService _predictionService;
        private ILogger<GetModelInfoQueryHandler> _logger;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _predictionService = Substitute.For<ILicensePlatePredictionService>();
            _logger = Substitute.For<ILogger<GetModelInfoQueryHandler>>();

            _handler = new GetModelInfoQueryHandler(
                _predictionService,
                _logger);
        }

        [Test]
        public void Constructor_WithValidParameters_InitializesCorrectly()
        {
            _handler.Should().NotBeNull();
        }

        [Test]
        public async Task Handle_WhenModelIsAvailable_ReturnsCompleteModelInfo()
        {
            // Arrange
            var query = new GetModelInfoQuery();
            _predictionService.IsModelAvailable().Returns(true);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.ModelAvailable.Should().BeTrue();
            result.ModelType.Should().Be("FastTree Regression");
            result.Description.Should().Be("Predicts when a license plate will next be seen based on historical patterns");
            result.TrainingSchedule.Should().Be("Every 6 hours");
            result.LastUpdated.Should().NotBeNullOrEmpty();
            
            result.Features.Should().NotBeNull();
            result.Features.Should().HaveCount(14);
            result.Features.Should().Contain("HourOfDay");
            result.Features.Should().Contain("DayOfWeek");
            result.Features.Should().Contain("CameraId");
            result.Features.Should().Contain("VehicleType");
            result.Features.Should().Contain("VehicleColor");
            
            _predictionService.Received(1).IsModelAvailable();
        }

        [Test]
        public async Task Handle_WhenModelIsNotAvailable_ReturnsModelInfoWithUnavailableFlag()
        {
            // Arrange
            var query = new GetModelInfoQuery();
            _predictionService.IsModelAvailable().Returns(false);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.ModelAvailable.Should().BeFalse();
            result.ModelType.Should().Be("FastTree Regression");
            result.Description.Should().Be("Predicts when a license plate will next be seen based on historical patterns");
            result.TrainingSchedule.Should().Be("Every 6 hours");
            result.Features.Should().NotBeNull();
            result.Features.Should().HaveCount(14);
            
            _predictionService.Received(1).IsModelAvailable();
        }

        [Test]
        public async Task Handle_ReturnsCorrectFeatureList()
        {
            // Arrange
            var query = new GetModelInfoQuery();
            _predictionService.IsModelAvailable().Returns(true);

            var expectedFeatures = new[]
            {
                "HourOfDay", "DayOfWeek", "DayOfMonth", "MonthOfYear",
                "CameraId", "TimeSinceLastSeen", "HistoricalFrequency",
                "AverageTimeBetweenVisits", "TotalVisits", "IsWeekend",
                "IsBusinessHour", "SeasonalFactor", "VehicleType", "VehicleColor"
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Features.Should().BeEquivalentTo(expectedFeatures);
        }

        [Test]
        public async Task Handle_WhenPredictionServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new GetModelInfoQuery();
            var expectedException = new InvalidOperationException("Test exception");

            _predictionService.IsModelAvailable()
                .Throws(expectedException);

            // Act & Assert
            var act = async () => await _handler.Handle(query, CancellationToken.None);
            
            await act.Should().ThrowExactlyAsync<InvalidOperationException>()
                .WithMessage("Test exception");
        }

        [Test]
        public async Task Handle_WithCancellationToken_StillProcessesCorrectly()
        {
            // Arrange
            var query = new GetModelInfoQuery();
            var cancellationToken = new CancellationToken();
            _predictionService.IsModelAvailable().Returns(true);

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.ModelAvailable.Should().BeTrue();
            result.ModelType.Should().Be("FastTree Regression");
            _predictionService.Received(1).IsModelAvailable();
        }

        [Test]
        public async Task Handle_LastUpdatedFieldIsFormattedCorrectly()
        {
            // Arrange
            var query = new GetModelInfoQuery();
            _predictionService.IsModelAvailable().Returns(true);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.LastUpdated.Should().NotBeNullOrEmpty();
            result.LastUpdated.Should().EndWith(" UTC");
            
            // Verify it's in the expected format (yyyy-MM-dd HH:mm:ss UTC)
            var dateTimeString = result.LastUpdated.Replace(" UTC", "");
            DateTime.TryParseExact(dateTimeString, "yyyy-MM-dd HH:mm:ss", null, 
                System.Globalization.DateTimeStyles.None, out var parsedDate)
                .Should().BeTrue("LastUpdated should be in format yyyy-MM-dd HH:mm:ss UTC");
            
            parsedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Test]
        public async Task Handle_MultipleCallsReturnConsistentStaticData()
        {
            // Arrange
            var query = new GetModelInfoQuery();
            _predictionService.IsModelAvailable().Returns(true);

            // Act
            var result1 = await _handler.Handle(query, CancellationToken.None);
            var result2 = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result1.Should().NotBeNull();
            result2.Should().NotBeNull();
            
            result1.ModelType.Should().Be(result2.ModelType);
            result1.Description.Should().Be(result2.Description);
            result1.TrainingSchedule.Should().Be(result2.TrainingSchedule);
            result1.Features.Should().BeEquivalentTo(result2.Features);
            
            _predictionService.Received(2).IsModelAvailable();
        }

        [Test]
        public async Task Handle_ContainsAllExpectedStaticValues()
        {
            // Arrange
            var query = new GetModelInfoQuery();
            _predictionService.IsModelAvailable().Returns(false);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.ModelType.Should().Be("FastTree Regression");
            result.Description.Should().Be("Predicts when a license plate will next be seen based on historical patterns");
            result.TrainingSchedule.Should().Be("Every 6 hours");
            
            // Verify all expected features are present
            var expectedFeatureCount = 14;
            result.Features.Should().HaveCount(expectedFeatureCount);
            
            // Verify some key features exist
            result.Features.Should().Contain(f => f == "HourOfDay");
            result.Features.Should().Contain(f => f == "CameraId");
            result.Features.Should().Contain(f => f == "VehicleType");
            result.Features.Should().Contain(f => f == "VehicleColor");
            result.Features.Should().Contain(f => f == "IsWeekend");
            result.Features.Should().Contain(f => f == "IsBusinessHour");
        }
    }
} 