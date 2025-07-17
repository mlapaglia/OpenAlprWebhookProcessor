using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.SearchLicensePlates;
using Tests.TestHelpers;

namespace Tests.Features.LicensePlates.Queries.SearchLicensePlates
{
    [TestFixture]
    public class SearchLicensePlatesQueryHandlerTests : TestBase
    {
        private SearchLicensePlatesQueryHandler _handler;
        private IUnitOfWork _unitOfWork;
        private IPlateGroupRepository _plateGroupRepository;
        private IRepository<Ignore> _ignoreRepository;
        private IRepository<Alert> _alertRepository;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _plateGroupRepository = Substitute.For<IPlateGroupRepository>();
            _ignoreRepository = Substitute.For<IRepository<Ignore>>();
            _alertRepository = Substitute.For<IRepository<Alert>>();
            
            _unitOfWork.PlateGroups.Returns(_plateGroupRepository);
            _unitOfWork.Ignores.Returns(_ignoreRepository);
            _unitOfWork.Alerts.Returns(_alertRepository);
            
            _handler = new SearchLicensePlatesQueryHandler(_unitOfWork);
        }

        [TearDown]
        public override void TearDown()
        {
            _unitOfWork.Dispose();
        }

        [Test]
        public async Task Handle_BasicSearch_ReturnsSearchResults()
        {
            // Arrange
            var query = new SearchLicensePlatesQuery
            {
                PlateNumber = "ABC123",
                StrictMatch = true,
                PageNumber = 1,
                PageSize = 10
            };

            var plateGroups = new List<PlateGroup>
            {
                TestDataFactory.CreateTestPlateGroup(),
                TestDataFactory.CreateTestPlateGroup()
            };

            var ignores = new List<Ignore>
            {
                new Ignore { PlateNumber = "IGNORE1" }
            };

            var alerts = new List<Alert>
            {
                new Alert { PlateNumber = "ALERT1" }
            };

            var cancellationToken = GetCancellationToken();

            _ignoreRepository.GetAllAsync(cancellationToken)
                .Returns(ignores);

            _plateGroupRepository.SearchPlatesAsync(
                query.PlateNumber,
                query.StrictMatch,
                query.RegexSearchEnabled,
                query.StartSearchOn,
                query.EndSearchOn,
                Arg.Any<List<string>>(),
                query.VehicleColor,
                query.VehicleMake,
                query.VehicleModel,
                query.VehicleType,
                query.VehicleRegion,
                query.FilterPlatesSeenLessThan,
                query.PageNumber,
                query.PageSize,
                cancellationToken)
                .Returns(plateGroups);

            _plateGroupRepository.GetSearchResultsCountAsync(
                query.PlateNumber,
                query.StrictMatch,
                query.RegexSearchEnabled,
                query.StartSearchOn,
                query.EndSearchOn,
                Arg.Any<List<string>>(),
                query.VehicleColor,
                query.VehicleMake,
                query.VehicleModel,
                query.VehicleType,
                query.VehicleRegion,
                query.FilterPlatesSeenLessThan,
                cancellationToken)
                .Returns(25);

            _alertRepository.GetAllAsync(cancellationToken)
                .Returns(alerts);

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Plates.Should().HaveCount(2);
            result.TotalCount.Should().Be(25);

            await _ignoreRepository.Received(1).GetAllAsync(cancellationToken);
            await _alertRepository.Received(1).GetAllAsync(cancellationToken);
        }

        [Test]
        public async Task Handle_FilterIgnoredPlatesTrue_CallsRepositoryWithIgnoreList()
        {
            // Arrange
            var query = new SearchLicensePlatesQuery
            {
                PlateNumber = "ABC123",
                FilterIgnoredPlates = true,
                PageNumber = 1,
                PageSize = 10
            };

            var plateGroups = new List<PlateGroup>
            {
                TestDataFactory.CreateTestPlateGroup()
            };

            var ignores = new List<Ignore>
            {
                new Ignore { PlateNumber = "IGNORE1" },
                new Ignore { PlateNumber = "IGNORE2" }
            };

            var alerts = new List<Alert>
            {
                new Alert { PlateNumber = "ALERT1" }
            };

            var cancellationToken = GetCancellationToken();

            _ignoreRepository.GetAllAsync(cancellationToken)
                .Returns(ignores);

            _plateGroupRepository.SearchPlatesAsync(
                query.PlateNumber,
                query.StrictMatch,
                query.RegexSearchEnabled,
                query.StartSearchOn,
                query.EndSearchOn,
                Arg.Is<List<string>>(list => list.Contains("IGNORE1") && list.Contains("IGNORE2")),
                query.VehicleColor,
                query.VehicleMake,
                query.VehicleModel,
                query.VehicleType,
                query.VehicleRegion,
                query.FilterPlatesSeenLessThan,
                query.PageNumber,
                query.PageSize,
                cancellationToken)
                .Returns(plateGroups);

            _plateGroupRepository.GetSearchResultsCountAsync(
                query.PlateNumber,
                query.StrictMatch,
                query.RegexSearchEnabled,
                query.StartSearchOn,
                query.EndSearchOn,
                Arg.Is<List<string>>(list => list.Contains("IGNORE1") && list.Contains("IGNORE2")),
                query.VehicleColor,
                query.VehicleMake,
                query.VehicleModel,
                query.VehicleType,
                query.VehicleRegion,
                query.FilterPlatesSeenLessThan,
                cancellationToken)
                .Returns(10);

            _alertRepository.GetAllAsync(cancellationToken)
                .Returns(alerts);

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Plates.Should().HaveCount(1);
            result.TotalCount.Should().Be(10);
        }

        [Test]
        public async Task Handle_FilterIgnoredPlatesFalse_CallsRepositoryWithEmptyIgnoreList()
        {
            // Arrange
            var query = new SearchLicensePlatesQuery
            {
                PlateNumber = "ABC123",
                FilterIgnoredPlates = false,
                PageNumber = 1,
                PageSize = 10
            };

            var plateGroups = new List<PlateGroup>
            {
                TestDataFactory.CreateTestPlateGroup()
            };

            var alerts = new List<Alert>
            {
                new Alert { PlateNumber = "ALERT1" }
            };

            var cancellationToken = GetCancellationToken();

            _plateGroupRepository.SearchPlatesAsync(
                query.PlateNumber,
                query.StrictMatch,
                query.RegexSearchEnabled,
                query.StartSearchOn,
                query.EndSearchOn,
                Arg.Is<List<string>>(list => list.Count == 0),
                query.VehicleColor,
                query.VehicleMake,
                query.VehicleModel,
                query.VehicleType,
                query.VehicleRegion,
                query.FilterPlatesSeenLessThan,
                query.PageNumber,
                query.PageSize,
                cancellationToken)
                .Returns(plateGroups);

            _plateGroupRepository.GetSearchResultsCountAsync(
                query.PlateNumber,
                query.StrictMatch,
                query.RegexSearchEnabled,
                query.StartSearchOn,
                query.EndSearchOn,
                Arg.Is<List<string>>(list => list.Count == 0),
                query.VehicleColor,
                query.VehicleMake,
                query.VehicleModel,
                query.VehicleType,
                query.VehicleRegion,
                query.FilterPlatesSeenLessThan,
                cancellationToken)
                .Returns(10);

            _alertRepository.GetAllAsync(cancellationToken)
                .Returns(alerts);

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Plates.Should().HaveCount(1);
            result.TotalCount.Should().Be(10);
            
            await _ignoreRepository.Received(1).GetAllAsync(cancellationToken);
        }

        [Test]
        public async Task Handle_WithDateRange_PassesDateRangeToRepository()
        {
            // Arrange
            var startDate = DateTimeOffset.UtcNow.AddDays(-7);
            var endDate = DateTimeOffset.UtcNow;
            
            var query = new SearchLicensePlatesQuery
            {
                PlateNumber = "ABC123",
                StartSearchOn = startDate,
                EndSearchOn = endDate,
                PageNumber = 1,
                PageSize = 10
            };

            var plateGroups = new List<PlateGroup>
            {
                TestDataFactory.CreateTestPlateGroup()
            };

            var alerts = new List<Alert>
            {
                new Alert { PlateNumber = "ALERT1" }
            };

            var cancellationToken = GetCancellationToken();

            _plateGroupRepository.SearchPlatesAsync(
                query.PlateNumber,
                query.StrictMatch,
                query.RegexSearchEnabled,
                startDate,
                endDate,
                Arg.Any<List<string>>(),
                query.VehicleColor,
                query.VehicleMake,
                query.VehicleModel,
                query.VehicleType,
                query.VehicleRegion,
                query.FilterPlatesSeenLessThan,
                query.PageNumber,
                query.PageSize,
                cancellationToken)
                .Returns(plateGroups);

            _plateGroupRepository.GetSearchResultsCountAsync(
                query.PlateNumber,
                query.StrictMatch,
                query.RegexSearchEnabled,
                startDate,
                endDate,
                Arg.Any<List<string>>(),
                query.VehicleColor,
                query.VehicleMake,
                query.VehicleModel,
                query.VehicleType,
                query.VehicleRegion,
                query.FilterPlatesSeenLessThan,
                cancellationToken)
                .Returns(5);

            _alertRepository.GetAllAsync(cancellationToken)
                .Returns(alerts);

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Plates.Should().HaveCount(1);
            result.TotalCount.Should().Be(5);
        }

        [Test]
        public async Task Handle_WithVehicleFilters_PassesFiltersToRepository()
        {
            // Arrange
            var query = new SearchLicensePlatesQuery
            {
                PlateNumber = "ABC123",
                VehicleColor = "Red",
                VehicleMake = "Toyota",
                VehicleModel = "Camry",
                VehicleType = "Sedan",
                VehicleRegion = "us-ca",
                FilterPlatesSeenLessThan = 5,
                PageNumber = 1,
                PageSize = 10
            };

            var plateGroups = new List<PlateGroup>
            {
                TestDataFactory.CreateTestPlateGroup()
            };

            var alerts = new List<Alert>
            {
                new Alert { PlateNumber = "ALERT1" }
            };

            var cancellationToken = GetCancellationToken();

            _plateGroupRepository.SearchPlatesAsync(
                query.PlateNumber,
                query.StrictMatch,
                query.RegexSearchEnabled,
                query.StartSearchOn,
                query.EndSearchOn,
                Arg.Any<List<string>>(),
                "Red",
                "Toyota",
                "Camry",
                "Sedan",
                "us-ca",
                5,
                query.PageNumber,
                query.PageSize,
                cancellationToken)
                .Returns(plateGroups);

            _plateGroupRepository.GetSearchResultsCountAsync(
                query.PlateNumber,
                query.StrictMatch,
                query.RegexSearchEnabled,
                query.StartSearchOn,
                query.EndSearchOn,
                Arg.Any<List<string>>(),
                "Red",
                "Toyota",
                "Camry",
                "Sedan",
                "us-ca",
                5,
                cancellationToken)
                .Returns(3);

            _alertRepository.GetAllAsync(cancellationToken)
                .Returns(alerts);

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Plates.Should().HaveCount(1);
            result.TotalCount.Should().Be(3);
        }

        [Test]
        public async Task Handle_RegexSearchEnabled_PassesRegexFlagToRepository()
        {
            // Arrange
            var query = new SearchLicensePlatesQuery
            {
                PlateNumber = "ABC.*",
                RegexSearchEnabled = true,
                PageNumber = 1,
                PageSize = 10
            };

            var plateGroups = new List<PlateGroup>
            {
                TestDataFactory.CreateTestPlateGroup()
            };

            var alerts = new List<Alert>
            {
                new Alert { PlateNumber = "ALERT1" }
            };

            var cancellationToken = GetCancellationToken();

            _plateGroupRepository.SearchPlatesAsync(
                "ABC.*",
                query.StrictMatch,
                true,
                query.StartSearchOn,
                query.EndSearchOn,
                Arg.Any<List<string>>(),
                query.VehicleColor,
                query.VehicleMake,
                query.VehicleModel,
                query.VehicleType,
                query.VehicleRegion,
                query.FilterPlatesSeenLessThan,
                query.PageNumber,
                query.PageSize,
                cancellationToken)
                .Returns(plateGroups);

            _plateGroupRepository.GetSearchResultsCountAsync(
                "ABC.*",
                query.StrictMatch,
                true,
                query.StartSearchOn,
                query.EndSearchOn,
                Arg.Any<List<string>>(),
                query.VehicleColor,
                query.VehicleMake,
                query.VehicleModel,
                query.VehicleType,
                query.VehicleRegion,
                query.FilterPlatesSeenLessThan,
                cancellationToken)
                .Returns(8);

            _alertRepository.GetAllAsync(cancellationToken)
                .Returns(alerts);

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Plates.Should().HaveCount(1);
            result.TotalCount.Should().Be(8);
        }

        [Test]
        public void Handle_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var query = new SearchLicensePlatesQuery
            {
                PlateNumber = "ABC123",
                PageNumber = 1,
                PageSize = 10
            };

            var cancellationToken = GetCancellationToken();
            var repositoryException = new InvalidOperationException("Database connection failed");

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
                cancellationToken)
                .ThrowsAsync(repositoryException);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(() => 
                _handler.Handle(query, cancellationToken));

            exception.Should().Be(repositoryException);
        }

        [Test]
        public void Handle_CountRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var query = new SearchLicensePlatesQuery
            {
                PlateNumber = "ABC123",
                PageNumber = 1,
                PageSize = 10
            };

            var plateGroups = new List<PlateGroup>
            {
                TestDataFactory.CreateTestPlateGroup()
            };

            var alerts = new List<Alert>
            {
                new Alert { PlateNumber = "ALERT1" }
            };

            var cancellationToken = GetCancellationToken();
            var countException = new InvalidOperationException("Count query failed");

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
                cancellationToken)
                .Returns(plateGroups);

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
                cancellationToken)
                .ThrowsAsync(countException);

            _alertRepository.GetAllAsync(cancellationToken)
                .Returns(alerts);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(() => 
                _handler.Handle(query, cancellationToken));

            exception.Should().Be(countException);
        }

        [Test]
        public async Task Handle_EmptyResults_ReturnsEmptySearchResponse()
        {
            // Arrange
            var query = new SearchLicensePlatesQuery
            {
                PlateNumber = "NONEXISTENT",
                PageNumber = 1,
                PageSize = 10
            };

            var emptyPlateGroups = new List<PlateGroup>();
            var alerts = new List<Alert>
            {
                new Alert { PlateNumber = "ALERT1" }
            };

            var cancellationToken = GetCancellationToken();

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
                cancellationToken)
                .Returns(emptyPlateGroups);

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
                cancellationToken)
                .Returns(0);

            _alertRepository.GetAllAsync(cancellationToken)
                .Returns(alerts);

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Plates.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }
    }
} 