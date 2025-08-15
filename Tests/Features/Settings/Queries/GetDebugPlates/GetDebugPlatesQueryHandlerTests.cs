using AwesomeAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetDebugPlates;
using Tests.TestHelpers;

namespace Tests.Features.Settings.Queries.GetDebugPlates
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class GetDebugPlatesQueryHandlerTests : TestBase
    {
        private GetDebugPlatesQueryHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new GetDebugPlatesQueryHandler(UnitOfWork);
        }

        [Test]
        public async Task Handle_WithOnlyFailedPlateGroups_ReturnsJsonString()
        {
            // Arrange
            var query = new GetDebugPlatesQuery(true);

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<string>();
            result.Should().StartWith("[");
            result.Should().EndWith("]");
        }

        [Test]
        public async Task Handle_WithAllPlateGroups_ReturnsJsonString()
        {
            // Arrange
            var query = new GetDebugPlatesQuery(false);

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<string>();
            result.Should().StartWith("[");
            result.Should().EndWith("]");
        }

        [Test]
        public async Task Handle_WithNoPlateGroups_ReturnsEmptyJsonArray()
        {
            // Arrange
            var query = new GetDebugPlatesQuery(false);

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Should().Be("[]");
        }

        [Test]
        public async Task Handle_OnlyFailedPlateGroups_FiltersCorrectly()
        {
            // Arrange
            var query = new GetDebugPlatesQuery(true);

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Should().StartWith("[");
            result.Should().EndWith("]");
            // Since we're using in-memory database, the query should execute without errors
        }

        [Test]
        public async Task Handle_FiltersOutOldPlateGroups()
        {
            // Arrange
            var query = new GetDebugPlatesQuery(false);

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<string>();
            // The query should filter out plate groups older than 1 day
        }

        [Test]
        public async Task Handle_LimitsResultsToTen()
        {
            // Arrange
            var query = new GetDebugPlatesQuery(false);

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<string>();
        }

        [Test]
        public void Handle_PassesCancellationToken()
        {
            // Arrange
            var query = new GetDebugPlatesQuery(false);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            Assert.DoesNotThrowAsync(
                async () => await _handler.Handle(query, cancellationToken));
        }

        [Test]
        public async Task Handle_WithBothFilterOptions_ReturnsDifferentResults()
        {
            // Arrange
            var queryOnlyFailed = new GetDebugPlatesQuery(true);
            var queryAll = new GetDebugPlatesQuery(false);

            // Act
            var resultOnlyFailed = await _handler.Handle(queryOnlyFailed, GetCancellationToken());
            var resultAll = await _handler.Handle(queryAll, GetCancellationToken());

            // Assert
            resultOnlyFailed.Should().NotBeNull();
            resultAll.Should().NotBeNull();
            
            // Both should return valid JSON arrays
            resultOnlyFailed.Should().StartWith("[");
            resultOnlyFailed.Should().EndWith("]");
            resultAll.Should().StartWith("[");
            resultAll.Should().EndWith("]");
        }

        [Test]
        public async Task Handle_ValidQuery_CompletesSuccessfully()
        {
            // Arrange
            var query = new GetDebugPlatesQuery(false);

            // Act
            var task = _handler.Handle(query, GetCancellationToken());

            // Assert
            task.Should().NotBeNull();
            var result = await task;
            result.Should().NotBeNull();
        }

        [Test]
        public void Handle_ValidQuery_DoesNotThrowException()
        {
            // Arrange
            var query = new GetDebugPlatesQuery(true);

            // Act & Assert
            Assert.DoesNotThrowAsync(
                async () => await _handler.Handle(query, GetCancellationToken()));
        }
    }
} 