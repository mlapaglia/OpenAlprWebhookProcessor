using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.SearchLicensePlates;
using Tests.TestHelpers;

namespace Tests.Features.LicensePlates.Queries.SearchLicensePlates
{
    [TestFixture]
    public class SearchLicensePlatesQueryHandlerTests : TestBase
    {
        private SearchLicensePlatesQueryHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new SearchLicensePlatesQueryHandler(UnitOfWork);
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public async Task Handle_BasicSearch_ReturnsSearchResults()
        {
            // Arrange
            var plateGroup = TestDataFactory.CreateTestPlateGroupDetailed(plateNumber: "ABC123", vehicleColor: "Red");
            
            // Add required data
            var ignore = new Ignore { PlateNumber = "IGNORE1" };
            var alert = new Alert { PlateNumber = "ALERT1" };
            var enricher = TestDataFactory.CreateTestEnricher(isEnabled: true);

            Context.PlateGroups.Add(plateGroup);
            Context.Ignores.Add(ignore);
            Context.Alerts.Add(alert);
            Context.Enrichers.Add(enricher);
            await Context.SaveChangesAsync();

            // Verify data was saved
            var savedPlates = await Context.PlateGroups.ToListAsync();
            savedPlates.Should().HaveCount(1);
            savedPlates[0].BestNumber.Should().Be("ABC123");

            var query = new SearchLicensePlatesQuery
            {
                PlateNumber = "ABC123",
                StrictMatch = true,
                PageNumber = 0, // 0-based paging: Skip(0 * 10) = 0, Take(10)
                PageSize = 10
            };

            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Plates.Should().HaveCount(1);
            result.Plates[0].PlateNumber.Should().Be("ABC123");
            result.TotalCount.Should().Be(1);
        }

        [Test]
        public async Task Handle_WithDateRange_PassesDateRangeToRepository()
        {
            // Arrange
            var startDate = DateTimeOffset.UtcNow.AddDays(-7);
            var endDate = DateTimeOffset.UtcNow;
            
            var oldPlateGroup = TestDataFactory.CreateTestPlateGroupDetailed(
                plateNumber: "OLD123", 
                epochTimeMs: startDate.AddDays(-2).ToUnixTimeMilliseconds());
            
            var newPlateGroup = TestDataFactory.CreateTestPlateGroupDetailed(
                plateNumber: "NEW123", 
                epochTimeMs: startDate.AddDays(1).ToUnixTimeMilliseconds());

            var enricher = TestDataFactory.CreateTestEnricher(isEnabled: true);

            Context.PlateGroups.AddRange(oldPlateGroup, newPlateGroup);
            Context.Enrichers.Add(enricher);
            await Context.SaveChangesAsync();

            var query = new SearchLicensePlatesQuery
            {
                PlateNumber = "", // Empty to get all plates
                StartSearchOn = startDate,
                EndSearchOn = endDate,
                PageNumber = 0,
                PageSize = 10
            };

            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Plates.Should().HaveCount(1);
            result.Plates[0].PlateNumber.Should().Be("NEW123");
        }

        [Test]
        public async Task Handle_WithVehicleFilters_PassesFiltersToRepository()
        {
            // Arrange
            var matchingPlate = TestDataFactory.CreateTestPlateGroupDetailed(
                plateNumber: "MATCH123", 
                vehicleColor: "red", 
                vehicleMakeModel: "toyota camry",
                vehicleType: "sedan",
                vehicleRegion: "us-ca");

            var nonMatchingPlate = TestDataFactory.CreateTestPlateGroupDetailed(
                plateNumber: "NOMATCH123", 
                vehicleColor: "blue", 
                vehicleMakeModel: "honda civic",
                vehicleType: "coupe",
                vehicleRegion: "us-ny");

            var enricher = TestDataFactory.CreateTestEnricher(isEnabled: true);

            Context.PlateGroups.AddRange(matchingPlate, nonMatchingPlate);
            Context.Enrichers.Add(enricher);
            await Context.SaveChangesAsync();

            var query = new SearchLicensePlatesQuery
            {
                PlateNumber = "",
                VehicleColor = "red",
                VehicleMake = "toyota",
                VehicleModel = "camry",
                VehicleType = "sedan",
                VehicleRegion = "us-ca",
                FilterPlatesSeenLessThan = 0,
                PageNumber = 0,
                PageSize = 10
            };

            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Plates.Should().HaveCount(1);
            result.Plates[0].PlateNumber.Should().Be("MATCH123");
        }

        [Test]
        public async Task Handle_RegexSearchEnabled_FindsMatchingPatterns()
        {
            // Arrange
            var plateGroups = new List<PlateGroup>
            {
                TestDataFactory.CreateTestPlateGroup(plateNumber: "ABC123"),
                TestDataFactory.CreateTestPlateGroup(plateNumber: "ABC456"),
                TestDataFactory.CreateTestPlateGroup(plateNumber: "XYZ789")
            };

            var enricher = TestDataFactory.CreateTestEnricher(isEnabled: true);

            Context.PlateGroups.AddRange(plateGroups);
            Context.Enrichers.Add(enricher);
            await Context.SaveChangesAsync();

            var query = new SearchLicensePlatesQuery
            {
                PlateNumber = "ABC.*",
                RegexSearchEnabled = true,
                PageNumber = 0,
                PageSize = 10
            };

            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Plates.Should().HaveCount(2);
            result.Plates.Should().OnlyContain(p => p.PlateNumber.StartsWith("ABC"));
        }

        [Test]
        public async Task Handle_FilterIgnoredPlates_ExcludesIgnoredPlates()
        {
            // Arrange
            var normalPlate = TestDataFactory.CreateTestPlateGroup(plateNumber: "NORMAL123");
            var ignoredPlate = TestDataFactory.CreateTestPlateGroup(plateNumber: "IGNORED123");

            var ignore = new Ignore { PlateNumber = "IGNORED123" };
            var enricher = TestDataFactory.CreateTestEnricher(isEnabled: true);

            Context.PlateGroups.AddRange(normalPlate, ignoredPlate);
            Context.Ignores.Add(ignore);
            Context.Enrichers.Add(enricher);
            await Context.SaveChangesAsync();

            var query = new SearchLicensePlatesQuery
            {
                PlateNumber = "",
                FilterIgnoredPlates = false,
                PageNumber = 0,
                PageSize = 10
            };

            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Plates.Should().HaveCount(1);
            result.Plates[0].PlateNumber.Should().Be("NORMAL123");
        }

        [Test]
        public async Task Handle_WithAlertsAndIgnores_MarksPlatesToAlert()
        {
            // Arrange
            var normalPlate = TestDataFactory.CreateTestPlateGroup(plateNumber: "NORMAL123");
            var alertPlate = TestDataFactory.CreateTestPlateGroup(plateNumber: "ALERT123");
            var ignoredPlate = TestDataFactory.CreateTestPlateGroup(plateNumber: "IGNORED123");

            var ignore = new Ignore { PlateNumber = "IGNORED123" };
            var alert = new Alert { PlateNumber = "ALERT123" };
            var enricher = TestDataFactory.CreateTestEnricher(isEnabled: true);

            Context.PlateGroups.AddRange(normalPlate, alertPlate, ignoredPlate);
            Context.Ignores.Add(ignore);
            Context.Alerts.Add(alert);
            Context.Enrichers.Add(enricher);
            await Context.SaveChangesAsync();

            var query = new SearchLicensePlatesQuery
            {
                PlateNumber = "",
                FilterIgnoredPlates = true, // Include ignored plates
                PageNumber = 0,
                PageSize = 10
            };

            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Plates.Should().HaveCount(3);

            var alertedPlate = result.Plates.Single(p => p.PlateNumber == "ALERT123");
            alertedPlate.IsAlert.Should().BeTrue();

            var ignoredPlateResult = result.Plates.Single(p => p.PlateNumber == "IGNORED123");
            ignoredPlateResult.IsIgnore.Should().BeTrue();

            var normalPlateResult = result.Plates.Single(p => p.PlateNumber == "NORMAL123");
            normalPlateResult.IsAlert.Should().BeFalse();
            normalPlateResult.IsIgnore.Should().BeFalse();
        }

        [Test]
        public async Task Handle_NoEnricherEnabled_SetsCanBeEnrichedToFalse()
        {
            // Arrange
            var plateGroup = TestDataFactory.CreateTestPlateGroup(plateNumber: "TEST123");

            // No enricher added or disabled enricher
            var disabledEnricher = TestDataFactory.CreateTestEnricher(isEnabled: false);

            Context.PlateGroups.Add(plateGroup);
            Context.Enrichers.Add(disabledEnricher);
            await Context.SaveChangesAsync();

            var query = new SearchLicensePlatesQuery
            {
                PlateNumber = "TEST123",
                StrictMatch = true,
                PageNumber = 0,
                PageSize = 10
            };

            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Plates.Should().HaveCount(1);
            result.Plates[0].CanBeEnriched.Should().BeFalse();
        }

        [Test]
        public async Task Handle_EnabledEnricher_SetsCanBeEnrichedToTrue()
        {
            // Arrange
            var plateGroup = TestDataFactory.CreateTestPlateGroup(plateNumber: "TEST123");
            var enabledEnricher = TestDataFactory.CreateTestEnricher(isEnabled: true);

            Context.PlateGroups.Add(plateGroup);
            Context.Enrichers.Add(enabledEnricher);
            await Context.SaveChangesAsync();

            var query = new SearchLicensePlatesQuery
            {
                PlateNumber = "TEST123",
                StrictMatch = true,
                PageNumber = 0,
                PageSize = 10
            };

            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Plates.Should().HaveCount(1);
            result.Plates[0].CanBeEnriched.Should().BeTrue();
        }

        [Test]
        public async Task Handle_EmptyResults_ReturnsEmptySearchResponse()
        {
            // Arrange
            var enricher = TestDataFactory.CreateTestEnricher(isEnabled: true);
            Context.Enrichers.Add(enricher);
            await Context.SaveChangesAsync();

            var query = new SearchLicensePlatesQuery
            {
                PlateNumber = "NONEXISTENT",
                StrictMatch = true,
                PageNumber = 0,
                PageSize = 10
            };

            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Plates.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Test]
        public async Task Handle_PaginationTest_ReturnsCorrectPage()
        {
            // Arrange
            var plateGroups = Enumerable.Range(1, 15)
                .Select(i => TestDataFactory.CreateTestPlateGroup(plateNumber: $"PLATE{i:D3}"))
                .ToList();

            var enricher = TestDataFactory.CreateTestEnricher(isEnabled: true);

            Context.PlateGroups.AddRange(plateGroups);
            Context.Enrichers.Add(enricher);
            await Context.SaveChangesAsync();

            var query = new SearchLicensePlatesQuery
            {
                PlateNumber = "",
                PageNumber = 1, // Second page (Skip 5, Take 5)
                PageSize = 5
            };

            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Plates.Should().HaveCount(5);
            result.TotalCount.Should().Be(15);
        }

        [Test]
        public async Task Handle_FilterPlatesSeenLessThan_FiltersCorrectly()
        {
            // Arrange
            var frequentPlates = new List<PlateGroup>()
            {
                TestDataFactory.CreateTestPlateGroup(plateNumber: "FREQUENT"),
                TestDataFactory.CreateTestPlateGroup(plateNumber: "FREQUENT"),
                TestDataFactory.CreateTestPlateGroup(plateNumber: "FREQUENT"),
                TestDataFactory.CreateTestPlateGroup(plateNumber: "FREQUENT"),
                TestDataFactory.CreateTestPlateGroup(plateNumber: "FREQUENT"),
                TestDataFactory.CreateTestPlateGroup(plateNumber: "FREQUENT"),
            };
            
            var rarePlate = TestDataFactory.CreateTestPlateGroup(
                plateNumber: "RARE");

            var enricher = TestDataFactory.CreateTestEnricher(isEnabled: true);

            
            Context.PlateGroups.AddRange(frequentPlates);
            await Context.PlateGroups.AddAsync(rarePlate);
            await Context.Enrichers.AddAsync(enricher);
            await Context.SaveChangesAsync();

            var query = new SearchLicensePlatesQuery
            {
                PlateNumber = "",
                FilterPlatesSeenLessThan = 5,
                PageNumber = 0,
                PageSize = 10
            };

            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Plates.Should().HaveCount(1);
            result.Plates[0].PlateNumber.Should().Be("RARE");
        }
    }
} 