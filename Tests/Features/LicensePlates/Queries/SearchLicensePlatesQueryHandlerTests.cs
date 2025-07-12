using FluentAssertions;
using NSubstitute;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.SearchLicensePlates;

namespace Tests.Features.LicensePlates.Queries
{
    [TestFixture]
    public class SearchLicensePlatesQueryHandlerTests
    {
        private IUnitOfWork _unitOfWork;
        private SearchLicensePlatesQueryHandler _handler;
        private IPlateGroupRepository _plateGroupRepository;
        private IRepository<Alert> _alertRepository;
        private IRepository<Ignore> _ignoreRepository;
        private IRepository<Enricher> _enricherRepository;

        [SetUp]
        public void Setup()
        {
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _plateGroupRepository = Substitute.For<IPlateGroupRepository>();
            _alertRepository = Substitute.For<IRepository<Alert>>();
            _ignoreRepository = Substitute.For<IRepository<Ignore>>();
            _enricherRepository = Substitute.For<IRepository<Enricher>>();

            _unitOfWork.PlateGroups.Returns(_plateGroupRepository);
            _unitOfWork.Alerts.Returns(_alertRepository);
            _unitOfWork.Ignores.Returns(_ignoreRepository);
            _unitOfWork.Enrichers.Returns(_enricherRepository);

            _handler = new SearchLicensePlatesQueryHandler(_unitOfWork);
        }

        [TearDown]
        public void Teardown()
        {
            _unitOfWork.Dispose();
        }

        [Test]
        public async Task Handle_ValidQuery_ReturnsSearchResults()
        {
            // Arrange
            var query = new SearchLicensePlatesQuery
            {
                PlateNumber = "ABC123",
                PageNumber = 0,
                PageSize = 10
            };

            var plateGroups = new List<PlateGroup>
            {
                new PlateGroup
                {
                    Id = Guid.NewGuid(),
                    BestNumber = "ABC123",
                    VehicleColor = "Red",
                    VehicleMakeModel = "Toyota Camry",
                    ReceivedOnEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    PossibleNumbers = new List<PlateGroupPossibleNumbers>()
                }
            };

            var alerts = new List<Alert>();
            var ignores = new List<Ignore>();
            var enrichers = new List<Enricher>();

            _plateGroupRepository.SearchPlatesAsync(
                Arg.Any<string>(),
                Arg.Any<bool>(),
                Arg.Any<bool>(),
                Arg.Any<DateTimeOffset?>(),
                Arg.Any<DateTimeOffset?>(),
                Arg.Any<List<string>>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
                .Returns(Task.FromResult((IEnumerable<PlateGroup>)plateGroups));

            _plateGroupRepository.GetSearchResultsCountAsync(
                Arg.Any<string>(),
                Arg.Any<bool>(),
                Arg.Any<bool>(),
                Arg.Any<DateTimeOffset?>(),
                Arg.Any<DateTimeOffset?>(),
                Arg.Any<List<string>>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(1));

            _alertRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult((IEnumerable<Alert>)alerts));

            _ignoreRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult((IEnumerable<Ignore>)ignores));

            _enricherRepository.FirstOrDefaultAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<Enricher, bool>>>(),
                Arg.Any<CancellationToken>())
                .Returns(Task.FromResult((Enricher?)null));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Plates.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
            result.Plates.First().PlateNumber.Should().Be("ABC123");
        }

        [Test]
        public async Task Handle_WithFilterIgnoredPlates_FiltersCorrectly()
        {
            // Arrange
            var query = new SearchLicensePlatesQuery
            {
                PlateNumber = "ABC123",
                FilterIgnoredPlates = true,
                PageNumber = 0,
                PageSize = 10
            };

            var plateGroups = new List<PlateGroup>();
            var alerts = new List<Alert>();
            var ignores = new List<Ignore>();

            _plateGroupRepository.SearchPlatesAsync(
                Arg.Any<string>(),
                Arg.Any<bool>(),
                Arg.Any<bool>(),
                Arg.Any<DateTimeOffset?>(),
                Arg.Any<DateTimeOffset?>(),
                Arg.Is<List<string>>(list => list.Count == 0),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
                .Returns(Task.FromResult((IEnumerable<PlateGroup>)plateGroups));

            _plateGroupRepository.GetSearchResultsCountAsync(
                Arg.Any<string>(),
                Arg.Any<bool>(),
                Arg.Any<bool>(),
                Arg.Any<DateTimeOffset?>(),
                Arg.Any<DateTimeOffset?>(),
                Arg.Is<List<string>>(list => list.Count == 0),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(0));

            _alertRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult((IEnumerable<Alert>)alerts));

            _ignoreRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult((IEnumerable<Ignore>)ignores));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Plates.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Test]
        public async Task Handle_WithEnricherEnabled_SetsCanBeEnrichedCorrectly()
        {
            // Arrange
            var query = new SearchLicensePlatesQuery
            {
                PageNumber = 0,
                PageSize = 10
            };

            var plateGroups = new List<PlateGroup>
            {
                new PlateGroup
                {
                    Id = Guid.NewGuid(),
                    BestNumber = "ABC123",
                    VehicleColor = "Red",
                    VehicleMakeModel = "Toyota Camry",
                    ReceivedOnEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    PossibleNumbers = new List<PlateGroupPossibleNumbers>()
                }
            };

            var enricher = new Enricher { IsEnabled = true };

            _plateGroupRepository.SearchPlatesAsync(
                Arg.Any<string>(),
                Arg.Any<bool>(),
                Arg.Any<bool>(),
                Arg.Any<DateTimeOffset?>(),
                Arg.Any<DateTimeOffset?>(),
                Arg.Any<List<string>>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
                .Returns(Task.FromResult((IEnumerable<PlateGroup>)plateGroups));

            _plateGroupRepository.GetSearchResultsCountAsync(
                Arg.Any<string>(),
                Arg.Any<bool>(),
                Arg.Any<bool>(),
                Arg.Any<DateTimeOffset?>(),
                Arg.Any<DateTimeOffset?>(),
                Arg.Any<List<string>>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(1));

            _alertRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult((IEnumerable<Alert>)new List<Alert>()));

            _ignoreRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult((IEnumerable<Ignore>)new List<Ignore>()));

            _enricherRepository.FirstOrDefaultAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<Enricher, bool>>>(),
                Arg.Any<CancellationToken>())
                .Returns(Task.FromResult((Enricher?)enricher));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Plates.Should().HaveCount(1);
            result.Plates.First().CanBeEnriched.Should().BeTrue();
        }
    }
} 