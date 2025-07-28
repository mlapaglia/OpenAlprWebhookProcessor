using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.MachineLearning.Commands.TriggerTraining;
using OpenAlprWebhookProcessor.Features.MachineLearning.Services;
using Tests.TestHelpers;

namespace Tests.Features.MachineLearning.Commands.TriggerTraining
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class TriggerTrainingCommandHandlerTests : TestBase
    {
        private TriggerTrainingCommandHandler _handler;
        private ILicensePlateMlTrainingService _trainingService;
        private ILogger<TriggerTrainingCommandHandler> _logger;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _trainingService = Substitute.For<ILicensePlateMlTrainingService>();
            _logger = Substitute.For<ILogger<TriggerTrainingCommandHandler>>();

            _handler = new TriggerTrainingCommandHandler(
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
        public async Task Handle_WhenTrainingSucceeds_ReturnsSuccessResult()
        {
            // Arrange
            var command = new TriggerTrainingCommand("TestUser");
            _trainingService.TrainModelAsync().Returns(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Model training completed successfully");
            result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            
            await _trainingService.Received(1).TrainModelAsync();
        }

        [Test]
        public async Task Handle_WhenTrainingFails_ReturnsFailureResult()
        {
            // Arrange
            var command = new TriggerTrainingCommand("TestUser");
            _trainingService.TrainModelAsync().Returns(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Model training failed or insufficient data");
            result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            
            await _trainingService.Received(1).TrainModelAsync();
        }

        [Test]
        public async Task Handle_WhenTrainingServiceThrows_ReturnsErrorResult()
        {
            // Arrange
            var command = new TriggerTrainingCommand("TestUser");
            var expectedException = new InvalidOperationException("Training service error");

            _trainingService.TrainModelAsync()
                .Throws(expectedException);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Error triggering model training");
            result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            
            await _trainingService.Received(1).TrainModelAsync();
        }

        [Test]
        public async Task Handle_WithCancellationToken_PassesToTrainingService()
        {
            // Arrange
            var command = new TriggerTrainingCommand("TestUser");
            var cancellationToken = new CancellationToken();
            _trainingService.TrainModelAsync().Returns(true);

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            await _trainingService.Received(1).TrainModelAsync();
        }

        [Test]
        public async Task Handle_WithDifferentRequestedByValues_ProcessesCorrectly()
        {
            // Arrange
            var command1 = new TriggerTrainingCommand("AdminUser");
            var command2 = new TriggerTrainingCommand("RegularUser");
            var command3 = new TriggerTrainingCommand(null);

            _trainingService.TrainModelAsync().Returns(true);

            // Act
            var result1 = await _handler.Handle(command1, CancellationToken.None);
            var result2 = await _handler.Handle(command2, CancellationToken.None);
            var result3 = await _handler.Handle(command3, CancellationToken.None);

            // Assert
            result1.Should().NotBeNull();
            result1.Success.Should().BeTrue();
            
            result2.Should().NotBeNull();
            result2.Success.Should().BeTrue();
            
            result3.Should().NotBeNull();
            result3.Success.Should().BeTrue();
            
            await _trainingService.Received(3).TrainModelAsync();
        }

        [Test]
        public async Task Handle_MultipleCallsInSequence_EachCallProcessedIndependently()
        {
            // Arrange
            var command = new TriggerTrainingCommand("TestUser");
            
            // First call succeeds
            _trainingService.TrainModelAsync().Returns(true);
            var result1 = await _handler.Handle(command, CancellationToken.None);

            // Second call fails
            _trainingService.TrainModelAsync().Returns(false);
            var result2 = await _handler.Handle(command, CancellationToken.None);

            // Third call throws exception
            _trainingService.TrainModelAsync().Throws(new Exception("Test error"));
            var result3 = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result1.Should().NotBeNull();
            result1.Success.Should().BeTrue();
            result1.Message.Should().Be("Model training completed successfully");

            result2.Should().NotBeNull();
            result2.Success.Should().BeFalse();
            result2.Message.Should().Be("Model training failed or insufficient data");

            result3.Should().NotBeNull();
            result3.Success.Should().BeFalse();
            result3.Message.Should().Be("Error triggering model training");

            await _trainingService.Received(3).TrainModelAsync();
        }

        [Test]
        public async Task Handle_WhenTaskCancelled_ReturnsErrorResult()
        {
            // Arrange
            var command = new TriggerTrainingCommand("TestUser");
            
            _trainingService.TrainModelAsync()
                .Throws(new OperationCanceledException("Task was cancelled"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Error triggering model training");
            result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Test]
        public async Task Handle_WithEmptyRequestedBy_StillProcessesCorrectly()
        {
            // Arrange
            var command = new TriggerTrainingCommand("");
            _trainingService.TrainModelAsync().Returns(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Model training completed successfully");
            await _trainingService.Received(1).TrainModelAsync();
        }

        [Test]
        public async Task Handle_ResultTimestampIsAccurate()
        {
            // Arrange
            var command = new TriggerTrainingCommand("TestUser");
            var beforeCall = DateTime.UtcNow;
            
            _trainingService.TrainModelAsync().Returns(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);
            var afterCall = DateTime.UtcNow;

            // Assert
            result.Should().NotBeNull();
            result.Timestamp.Should().BeOnOrAfter(beforeCall);
            result.Timestamp.Should().BeOnOrBefore(afterCall);
        }


    }
} 