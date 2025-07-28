using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.MachineLearning.Queries.GetTrainingStatus;
using OpenAlprWebhookProcessor.Features.MachineLearning.Services;
using Tests.TestHelpers;

namespace Tests.Features.MachineLearning.Queries.GetTrainingStatus
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class GetTrainingStatusQueryHandlerTests : TestBase
    {
        private GetTrainingStatusQueryHandler _handler;
        private ILicensePlateMlTrainingService _trainingService;
        private ILogger<GetTrainingStatusQueryHandler> _logger;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _trainingService = Substitute.For<ILicensePlateMlTrainingService>();
            _logger = Substitute.For<ILogger<GetTrainingStatusQueryHandler>>();

            _handler = new GetTrainingStatusQueryHandler(
                UnitOfWork,
                _trainingService,
                _logger);
        }

        [Test]
        public void Constructor_WithValidParameters_InitializesCorrectly()
        {
            _handler.Should().NotBeNull();
        }

        [Test]
        public async Task Handle_WithCompleteTrainingStatus_ReturnsFullDto()
        {
            // Arrange
            var query = new GetTrainingStatusQuery();
            var status = new TrainingStatus
            {
                IsTraining = true,
                LastTrainingStarted = DateTime.UtcNow.AddMinutes(-30),
                LastTrainingCompleted = DateTime.UtcNow.AddHours(-2),
                LastTrainingSuccessful = true,
                LastError = null,
                TrainingDataCount = 15000,
                RSquared = 0.85,
                MeanAbsoluteError = 2.5,
                RootMeanSquaredError = 3.2,
                ModelLastSaved = DateTime.UtcNow.AddHours(-1),
                ModelFileSize = 2048000
            };

            _trainingService.GetTrainingStatus().Returns(status);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsTraining.Should().BeTrue();
            result.LastTrainingStarted.Should().Be(status.LastTrainingStarted);
            result.LastTrainingCompleted.Should().Be(status.LastTrainingCompleted);
            result.LastTrainingSuccessful.Should().BeTrue();
            result.LastError.Should().BeNull();
            result.TrainingDataCount.Should().Be(15000);

            result.ModelMetrics.Should().NotBeNull();
            result.ModelMetrics.RSquared.Should().Be(0.85);
            result.ModelMetrics.MeanAbsoluteError.Should().Be(2.5);
            result.ModelMetrics.RootMeanSquaredError.Should().Be(3.2);

            result.ModelFile.Should().NotBeNull();
            result.ModelFile.LastSaved.Should().Be(status.ModelLastSaved.Value);
            result.ModelFile.FileSizeBytes.Should().Be(2048000);

            result.Configuration.Should().NotBeNull();
            result.Configuration.TrainingInterval.Should().Be("Every 6 hours");
            result.Configuration.MinimumTrainingData.Should().Be(100);
            result.Configuration.MinimumModelQuality.Should().Be(0.05);
            result.Configuration.BatchSize.Should().Be(50000);

            _trainingService.Received(1).GetTrainingStatus();
        }

        [Test]
        public async Task Handle_WithMinimalTrainingStatus_HandlesNullValues()
        {
            // Arrange
            var query = new GetTrainingStatusQuery();
            var status = new TrainingStatus
            {
                IsTraining = false,
                LastTrainingStarted = null,
                LastTrainingCompleted = null,
                LastTrainingSuccessful = false,
                LastError = "Training failed due to insufficient data",
                TrainingDataCount = 50,
                RSquared = null,
                MeanAbsoluteError = null,
                RootMeanSquaredError = null,
                ModelLastSaved = null,
                ModelFileSize = null
            };

            _trainingService.GetTrainingStatus().Returns(status);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsTraining.Should().BeFalse();
            result.LastTrainingStarted.Should().BeNull();
            result.LastTrainingCompleted.Should().BeNull();
            result.LastTrainingSuccessful.Should().BeFalse();
            result.LastError.Should().Be("Training failed due to insufficient data");
            result.TrainingDataCount.Should().Be(50);

            result.ModelMetrics.Should().BeNull();
            result.ModelFile.Should().BeNull();

            result.Configuration.Should().NotBeNull();
            result.Configuration.TrainingInterval.Should().Be("Every 6 hours");
            result.Configuration.MinimumTrainingData.Should().Be(100);
            result.Configuration.MinimumModelQuality.Should().Be(0.05);
            result.Configuration.BatchSize.Should().Be(50000);

            _trainingService.Received(1).GetTrainingStatus();
        }

        [Test]
        public async Task Handle_WithPartialMetrics_CreatesPartialModelMetrics()
        {
            // Arrange
            var query = new GetTrainingStatusQuery();
            var status = new TrainingStatus
            {
                IsTraining = false,
                TrainingDataCount = 5000,
                RSquared = 0.75,
                MeanAbsoluteError = 3.0,
                RootMeanSquaredError = 4.1,
                ModelLastSaved = null,
                ModelFileSize = null
            };

            _trainingService.GetTrainingStatus().Returns(status);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.ModelMetrics.Should().NotBeNull();
            result.ModelMetrics.RSquared.Should().Be(0.75);
            result.ModelMetrics.MeanAbsoluteError.Should().Be(3.0);
            result.ModelMetrics.RootMeanSquaredError.Should().Be(4.1);

            result.ModelFile.Should().BeNull();
        }

        [Test]
        public async Task Handle_WithModelFileButNoMetrics_CreatesModelFileOnly()
        {
            // Arrange
            var query = new GetTrainingStatusQuery();
            var status = new TrainingStatus
            {
                IsTraining = false,
                TrainingDataCount = 3000,
                RSquared = null,
                MeanAbsoluteError = null,
                RootMeanSquaredError = null,
                ModelLastSaved = DateTime.UtcNow.AddDays(-1),
                ModelFileSize = 1024000
            };

            _trainingService.GetTrainingStatus().Returns(status);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.ModelMetrics.Should().BeNull();
            
            result.ModelFile.Should().NotBeNull();
            result.ModelFile.LastSaved.Should().Be(status.ModelLastSaved.Value);
            result.ModelFile.FileSizeBytes.Should().Be(1024000);
        }

        [Test]
        public async Task Handle_WhenTrainingServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new GetTrainingStatusQuery();
            var expectedException = new InvalidOperationException("Test exception");

            _trainingService.GetTrainingStatus()
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
            var query = new GetTrainingStatusQuery();
            var cancellationToken = new CancellationToken();
            var status = new TrainingStatus
            {
                IsTraining = true,
                TrainingDataCount = 10000
            };

            _trainingService.GetTrainingStatus().Returns(status);

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.IsTraining.Should().BeTrue();
            result.TrainingDataCount.Should().Be(10000);
            _trainingService.Received(1).GetTrainingStatus();
        }

        [Test]
        public async Task Handle_AlwaysReturnsConfigurationObject()
        {
            // Arrange
            var query = new GetTrainingStatusQuery();
            var status = new TrainingStatus(); // Empty status

            _trainingService.GetTrainingStatus().Returns(status);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Configuration.Should().NotBeNull();
            result.Configuration.TrainingInterval.Should().Be("Every 6 hours");
            result.Configuration.MinimumTrainingData.Should().Be(100);
            result.Configuration.MinimumModelQuality.Should().Be(0.05);
            result.Configuration.BatchSize.Should().Be(50000);
        }

        [Test]
        public async Task Handle_WithTrainingInProgress_ReturnsInProgressStatus()
        {
            // Arrange
            var query = new GetTrainingStatusQuery();
            var status = new TrainingStatus
            {
                IsTraining = true,
                LastTrainingStarted = DateTime.UtcNow.AddMinutes(-15),
                LastTrainingCompleted = DateTime.UtcNow.AddHours(-6),
                TrainingDataCount = 8500,
                LastError = null
            };

            _trainingService.GetTrainingStatus().Returns(status);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsTraining.Should().BeTrue();
            result.LastTrainingStarted.Should().Be(status.LastTrainingStarted);
            result.LastTrainingCompleted.Should().Be(status.LastTrainingCompleted);
            result.TrainingDataCount.Should().Be(8500);
            result.LastError.Should().BeNull();
        }

        [Test]
        public async Task Handle_WithFailedTraining_ReturnsErrorInformation()
        {
            // Arrange
            var query = new GetTrainingStatusQuery();
            var status = new TrainingStatus
            {
                IsTraining = false,
                LastTrainingStarted = DateTime.UtcNow.AddHours(-1),
                LastTrainingCompleted = DateTime.UtcNow.AddMinutes(-30),
                LastTrainingSuccessful = false,
                LastError = "Model quality below minimum threshold",
                TrainingDataCount = 500
            };

            _trainingService.GetTrainingStatus().Returns(status);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsTraining.Should().BeFalse();
            result.LastTrainingSuccessful.Should().BeFalse();
            result.LastError.Should().Be("Model quality below minimum threshold");
            result.TrainingDataCount.Should().Be(500);
        }
    }
} 