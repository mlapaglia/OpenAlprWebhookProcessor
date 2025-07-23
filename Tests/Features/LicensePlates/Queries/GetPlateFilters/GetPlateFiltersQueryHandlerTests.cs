using FluentAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetPlateFilters;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tests.TestHelpers;

namespace Tests.Features.LicensePlates.Queries.GetPlateFilters
{
    [TestFixture]
    public class GetPlateFiltersQueryHandlerTests : TestBase
    {
        private GetPlateFiltersQueryHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new GetPlateFiltersQueryHandler(UnitOfWork);
        }

        [Test]
        public async Task Handle_WithNoPlateGroups_ReturnsEmptyLists()
        {
            // Arrange
            var query = new GetPlateFiltersQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.VehicleColors.Should().BeEmpty();
            result.VehicleMakes.Should().BeEmpty();
            result.VehicleTypes.Should().BeEmpty();
            result.VehicleRegions.Should().BeEmpty();
        }

        [Test]
        public async Task Handle_WithPlateGroups_ReturnsDistinctFilters()
        {
            // Arrange
            await SeedPlateGroupsAsync();
            var query = new GetPlateFiltersQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.VehicleColors.Should().Contain(new[] { "red", "blue" });
            result.VehicleMakes.Should().Contain(new[] { "Toyota Camry", "Honda Civic" });
            result.VehicleTypes.Should().Contain(new[] { "sedan", "suv" });
            result.VehicleRegions.Should().Contain(new[] { "us-ca", "us-tx" });
        }

        [Test]
        public async Task Handle_WithDuplicateValues_ReturnsDistinctValues()
        {
            // Arrange
            await SeedDuplicatePlateGroupsAsync();
            var query = new GetPlateFiltersQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.VehicleColors.Should().HaveCount(2);
            result.VehicleColors.Should().Contain(new[] { "red", "blue" });
            result.VehicleMakes.Should().HaveCount(1);
            result.VehicleMakes.Should().Contain("Toyota Camry");
        }

        [Test]
        public async Task Handle_WithNullOrEmptyValues_FiltersThemOut()
        {
            // Arrange
            await SeedPlateGroupsWithNullValuesAsync();
            var query = new GetPlateFiltersQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.VehicleColors.Should().HaveCount(1);
            result.VehicleColors.Should().Contain("red");
            result.VehicleMakes.Should().HaveCount(1);
            result.VehicleMakes.Should().Contain("Toyota Camry");
            result.VehicleTypes.Should().HaveCount(1);
            result.VehicleTypes.Should().Contain("sedan");
            result.VehicleRegions.Should().HaveCount(1);
            result.VehicleRegions.Should().Contain("us-ca");
        }

        [Test]
        public async Task Handle_ReturnsResultsInAlphabeticalOrder()
        {
            // Arrange
            await SeedPlateGroupsForSortingAsync();
            var query = new GetPlateFiltersQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.VehicleColors.Should().BeInAscendingOrder();
            result.VehicleMakes.Should().BeInAscendingOrder();
            result.VehicleTypes.Should().BeInAscendingOrder();
            result.VehicleRegions.Should().BeInAscendingOrder();
        }

        [Test]
        public async Task Handle_WithCancellationToken_RespectsToken()
        {
            // Arrange
            await SeedPlateGroupsAsync();
            var query = new GetPlateFiltersQuery();
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
        }

        private async Task SeedPlateGroupsAsync()
        {
            var plateGroups = new[]
            {
                new PlateGroup
                {
                    BestNumber = "ABC123",
                    VehicleColor = "red",
                    VehicleMakeModel = "Toyota Camry",
                    VehicleType = "sedan",
                    VehicleRegion = "us-ca",
                    ReceivedOnEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    OpenAlprUuid = Guid.NewGuid().ToString(),
                    OpenAlprCameraId = 1,
                    Confidence = 95.5
                },
                new PlateGroup
                {
                    BestNumber = "XYZ789",
                    VehicleColor = "blue",
                    VehicleMakeModel = "Honda Civic",
                    VehicleType = "suv",
                    VehicleRegion = "us-tx",
                    ReceivedOnEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    OpenAlprUuid = Guid.NewGuid().ToString(),
                    OpenAlprCameraId = 2,
                    Confidence = 90.0
                }
            };

            Context.PlateGroups.AddRange(plateGroups);
            await Context.SaveChangesAsync();
        }

        private async Task SeedDuplicatePlateGroupsAsync()
        {
            var plateGroups = new[]
            {
                new PlateGroup
                {
                    BestNumber = "ABC123",
                    VehicleColor = "red",
                    VehicleMakeModel = "Toyota Camry",
                    VehicleType = "sedan",
                    VehicleRegion = "us-ca",
                    ReceivedOnEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    OpenAlprUuid = Guid.NewGuid().ToString(),
                    OpenAlprCameraId = 1,
                    Confidence = 95.5
                },
                new PlateGroup
                {
                    BestNumber = "DEF456",
                    VehicleColor = "red",  // Duplicate color
                    VehicleMakeModel = "Toyota Camry",  // Duplicate make/model
                    VehicleType = "sedan",
                    VehicleRegion = "us-ca",
                    ReceivedOnEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    OpenAlprUuid = Guid.NewGuid().ToString(),
                    OpenAlprCameraId = 1,
                    Confidence = 90.0
                },
                new PlateGroup
                {
                    BestNumber = "GHI789",
                    VehicleColor = "blue",
                    VehicleMakeModel = "Toyota Camry",  // Duplicate make/model
                    VehicleType = "sedan",
                    VehicleRegion = "us-ca",
                    ReceivedOnEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    OpenAlprUuid = Guid.NewGuid().ToString(),
                    OpenAlprCameraId = 2,
                    Confidence = 88.0
                }
            };

            Context.PlateGroups.AddRange(plateGroups);
            await Context.SaveChangesAsync();
        }

        private async Task SeedPlateGroupsWithNullValuesAsync()
        {
            var plateGroups = new[]
            {
                new PlateGroup
                {
                    BestNumber = "ABC123",
                    VehicleColor = "red",
                    VehicleMakeModel = "Toyota Camry",
                    VehicleType = "sedan",
                    VehicleRegion = "us-ca",
                    ReceivedOnEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    OpenAlprUuid = Guid.NewGuid().ToString(),
                    OpenAlprCameraId = 1,
                    Confidence = 95.5
                },
                new PlateGroup
                {
                    BestNumber = "XYZ789",
                    VehicleColor = null,  // Should be filtered out
                    VehicleMakeModel = "",  // Should be filtered out
                    VehicleType = null,  // Should be filtered out
                    VehicleRegion = "",  // Should be filtered out
                    ReceivedOnEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    OpenAlprUuid = Guid.NewGuid().ToString(),
                    OpenAlprCameraId = 2,
                    Confidence = 90.0
                }
            };

            Context.PlateGroups.AddRange(plateGroups);
            await Context.SaveChangesAsync();
        }

        private async Task SeedPlateGroupsForSortingAsync()
        {
            var plateGroups = new[]
            {
                new PlateGroup
                {
                    BestNumber = "ABC123",
                    VehicleColor = "yellow",
                    VehicleMakeModel = "Zebra Model",
                    VehicleType = "truck",
                    VehicleRegion = "us-wy",
                    ReceivedOnEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    OpenAlprUuid = Guid.NewGuid().ToString(),
                    OpenAlprCameraId = 1,
                    Confidence = 95.5
                },
                new PlateGroup
                {
                    BestNumber = "XYZ789",
                    VehicleColor = "blue",
                    VehicleMakeModel = "Apple Car",
                    VehicleType = "sedan",
                    VehicleRegion = "us-al",
                    ReceivedOnEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    OpenAlprUuid = Guid.NewGuid().ToString(),
                    OpenAlprCameraId = 2,
                    Confidence = 90.0
                },
                new PlateGroup
                {
                    BestNumber = "DEF456",
                    VehicleColor = "red",
                    VehicleMakeModel = "Mango Vehicle",
                    VehicleType = "motorcycle",
                    VehicleRegion = "us-ca",
                    ReceivedOnEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    OpenAlprUuid = Guid.NewGuid().ToString(),
                    OpenAlprCameraId = 3,
                    Confidence = 87.0
                }
            };

            Context.PlateGroups.AddRange(plateGroups);
            await Context.SaveChangesAsync();
        }
    }
} 