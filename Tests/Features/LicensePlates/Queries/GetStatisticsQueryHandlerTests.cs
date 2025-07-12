using FluentAssertions;
using NSubstitute;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetStatistics;

namespace Tests.Features.LicensePlates.Queries
{
    [TestFixture]
    public class GetStatisticsQueryHandlerTests
    {
        private IUnitOfWork _unitOfWork;
        private IPlateGroupRepository _plateGroupRepository;
        private GetStatisticsQueryHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _plateGroupRepository = Substitute.For<IPlateGroupRepository>();
            _unitOfWork.PlateGroups.Returns(_plateGroupRepository);
            _handler = new GetStatisticsQueryHandler(_unitOfWork);
        }

        [TearDown]
        public void TearDown()
        {
            _unitOfWork.Dispose();
        }

        [Test]
        public async Task Handle_WhenPlateExists_ReturnsCorrectStatistics()
        {
            // Arrange
            var plateNumber = "ABC123";
            var now = DateTimeOffset.UtcNow;
            var epoch90DaysAgo = now.AddDays(-90).ToUnixTimeMilliseconds();
            var epoch30DaysAgo = now.AddDays(-30).ToUnixTimeMilliseconds();
            var epoch60DaysAgo = now.AddDays(-60).ToUnixTimeMilliseconds();
            var epoch120DaysAgo = now.AddDays(-120).ToUnixTimeMilliseconds();

            var seenPlates = new List<long>
            {
                epoch30DaysAgo,   // Within 90 days
                epoch60DaysAgo,   // Within 90 days
                epoch120DaysAgo   // Outside 90 days
            };

            var seenPossiblePlates = new List<long>
            {
                epoch30DaysAgo + 1000,   // Within 90 days
                epoch120DaysAgo + 1000   // Outside 90 days
            };

            _plateGroupRepository.GetPlateStatisticsEpochsAsync(plateNumber, Arg.Any<CancellationToken>())
                .Returns((seenPlates, seenPossiblePlates));

            var query = new GetStatisticsQuery(plateNumber);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.TotalSeen.Should().Be(5); // 3 from seenPlates + 2 from seenPossiblePlates
            result.Last90Days.Should().Be(3); // 2 from seenPlates + 1 from seenPossiblePlates within 90 days
            result.FirstSeen.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(epoch120DaysAgo));
            result.LastSeen.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(epoch120DaysAgo + 1000));
        }

        [Test]
        public async Task Handle_WhenPlateDoesNotExist_ReturnsEmptyStatistics()
        {
            // Arrange
            var plateNumber = "NONEXISTENT";
            var emptySeenPlates = new List<long>();
            var emptyPossiblePlates = new List<long>();

            _plateGroupRepository.GetPlateStatisticsEpochsAsync(plateNumber, Arg.Any<CancellationToken>())
                .Returns((emptySeenPlates, emptyPossiblePlates));

            var query = new GetStatisticsQuery(plateNumber);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.TotalSeen.Should().Be(0);
            result.Last90Days.Should().Be(0);
            result.FirstSeen.Should().Be(default(DateTimeOffset));
            result.LastSeen.Should().Be(default(DateTimeOffset));
        }

        [Test]
        public async Task Handle_WhenOnlySeenPlatesExist_ReturnsCorrectStatistics()
        {
            // Arrange
            var plateNumber = "ABC123";
            var now = DateTimeOffset.UtcNow;
            var epoch30DaysAgo = now.AddDays(-30).ToUnixTimeMilliseconds();
            var epoch60DaysAgo = now.AddDays(-60).ToUnixTimeMilliseconds();
            var epoch120DaysAgo = now.AddDays(-120).ToUnixTimeMilliseconds();

            var seenPlates = new List<long>
            {
                epoch30DaysAgo,
                epoch60DaysAgo,
                epoch120DaysAgo
            };

            var emptyPossiblePlates = new List<long>();

            _plateGroupRepository.GetPlateStatisticsEpochsAsync(plateNumber, Arg.Any<CancellationToken>())
                .Returns((seenPlates, emptyPossiblePlates));

            var query = new GetStatisticsQuery(plateNumber);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.TotalSeen.Should().Be(3);
            result.Last90Days.Should().Be(2); // Within 90 days
            result.FirstSeen.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(epoch120DaysAgo));
            result.LastSeen.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(epoch30DaysAgo));
        }

        [Test]
        public async Task Handle_WhenOnlyPossiblePlatesExist_ReturnsCorrectStatistics()
        {
            // Arrange
            var plateNumber = "ABC123";
            var now = DateTimeOffset.UtcNow;
            var epoch30DaysAgo = now.AddDays(-30).ToUnixTimeMilliseconds();
            var epoch120DaysAgo = now.AddDays(-120).ToUnixTimeMilliseconds();

            var emptySeenPlates = new List<long>();
            var seenPossiblePlates = new List<long>
            {
                epoch30DaysAgo,
                epoch120DaysAgo
            };

            _plateGroupRepository.GetPlateStatisticsEpochsAsync(plateNumber, Arg.Any<CancellationToken>())
                .Returns((emptySeenPlates, seenPossiblePlates));

            var query = new GetStatisticsQuery(plateNumber);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.TotalSeen.Should().Be(2);
            result.Last90Days.Should().Be(1); // Within 90 days
            result.FirstSeen.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(epoch120DaysAgo));
            result.LastSeen.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(epoch30DaysAgo));
        }

        [Test]
        public async Task Handle_CallsRepositoryWithCorrectParameters()
        {
            // Arrange
            var plateNumber = "ABC123";
            var query = new GetStatisticsQuery(plateNumber);

            _plateGroupRepository.GetPlateStatisticsEpochsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns((new List<long>(), new List<long>()));

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            await _plateGroupRepository.Received(1).GetPlateStatisticsEpochsAsync(plateNumber, Arg.Any<CancellationToken>());
        }
    }
} 