using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.MachineLearning.Queries.GetModelStatus;
using OpenAlprWebhookProcessor.Features.MachineLearning.Services;
using Tests.TestHelpers;

namespace Tests.Features.MachineLearning.Queries.GetModelStatus
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class GetModelStatusQueryHandlerTests : TestBase
    {
        private GetModelStatusQueryHandler _handler;
        private ILicensePlatePredictionService _predictionService;
        private ILogger<GetModelStatusQueryHandler> _logger;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _predictionService = Substitute.For<ILicensePlatePredictionService>();
            _logger = Substitute.For<ILogger<GetModelStatusQueryHandler>>();

            _handler = new GetModelStatusQueryHandler(
                _predictionService,
                _logger);
        }

        [Test]
        public void Constructor_WithValidParameters_InitializesCorrectly()
        {
            _handler.Should().NotBeNull();
        }

        [Test]
        public async Task Handle_WhenModelIsAvailable_ReturnsReadyStatus()
        {
            // Arrange
            var query = new GetModelStatusQuery();
            _predictionService.IsModelAvailable().Returns(true);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.ModelAvailable.Should().BeTrue();
            result.Status.Should().Be("Ready");
            result.LastChecked.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            _predictionService.Received(1).IsModelAvailable();
        }

        [Test]
        public async Task Handle_WhenModelIsNotAvailable_ReturnsNotAvailableStatus()
        {
            // Arrange
            var query = new GetModelStatusQuery();
            _predictionService.IsModelAvailable().Returns(false);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.ModelAvailable.Should().BeFalse();
            result.Status.Should().Be("Training or Not Available");
            result.LastChecked.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            _predictionService.Received(1).IsModelAvailable();
        }

        [Test]
        public async Task Handle_WhenPredictionServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new GetModelStatusQuery();
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
            var query = new GetModelStatusQuery();
            var cancellationToken = new CancellationToken();
            _predictionService.IsModelAvailable().Returns(true);

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.ModelAvailable.Should().BeTrue();
            result.Status.Should().Be("Ready");
            _predictionService.Received(1).IsModelAvailable();
        }

        [Test]
        public async Task Handle_MultipleCallsInSequence_ReturnsConsistentResults()
        {
            // Arrange
            var query = new GetModelStatusQuery();
            _predictionService.IsModelAvailable().Returns(true);

            // Act
            var result1 = await _handler.Handle(query, CancellationToken.None);
            var result2 = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result1.Should().NotBeNull();
            result2.Should().NotBeNull();
            result1.ModelAvailable.Should().Be(result2.ModelAvailable);
            result1.Status.Should().Be(result2.Status);
            _predictionService.Received(2).IsModelAvailable();
        }

        [Test]
        public async Task Handle_WhenModelStatusChanges_ReflectsChange()
        {
            // Arrange
            var query = new GetModelStatusQuery();
            
            // First call - model not available
            _predictionService.IsModelAvailable().Returns(false);
            var result1 = await _handler.Handle(query, CancellationToken.None);

            // Second call - model becomes available
            _predictionService.IsModelAvailable().Returns(true);
            var result2 = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result1.Should().NotBeNull();
            result1.ModelAvailable.Should().BeFalse();
            result1.Status.Should().Be("Training or Not Available");

            result2.Should().NotBeNull();
            result2.ModelAvailable.Should().BeTrue();
            result2.Status.Should().Be("Ready");

            _predictionService.Received(2).IsModelAvailable();
        }
    }
} 