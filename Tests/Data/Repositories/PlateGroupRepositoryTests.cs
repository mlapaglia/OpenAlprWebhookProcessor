using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;

namespace Tests.Data.Repositories
{
    [TestFixture]
    public class PlateGroupRepositoryTests
    {
        private ProcessorContext _context;
        private PlateGroupRepository _repository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ProcessorContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ProcessorContext(options);
            _repository = new PlateGroupRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task SearchPlatesAsync_WithPlateNumber_ReturnsMatchingPlates()
        {
            // Arrange
            var testPlates = new List<PlateGroup>
            {
                new PlateGroup
                {
                    Id = Guid.NewGuid(),
                    BestNumber = "ABC123",
                    VehicleColor = "red",
                    VehicleMakeModel = "toyota camry",
                    ReceivedOnEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    PossibleNumbers = new List<PlateGroupPossibleNumbers>
                    {
                        new PlateGroupPossibleNumbers { Number = "ABC123" }
                    }
                },
                new PlateGroup
                {
                    Id = Guid.NewGuid(),
                    BestNumber = "XYZ789",
                    VehicleColor = "blue",
                    VehicleMakeModel = "honda civic",
                    ReceivedOnEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    PossibleNumbers = new List<PlateGroupPossibleNumbers>
                    {
                        new PlateGroupPossibleNumbers { Number = "XYZ789" }
                    }
                }
            };

            await _context.PlateGroups.AddRangeAsync(testPlates);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.SearchPlatesAsync(
                "ABC123",
                strictMatch: true,
                regexSearchEnabled: false,
                startDate: null,
                endDate: null,
                platesToIgnore: new List<string>(),
                vehicleColor: null,
                vehicleMake: null,
                vehicleModel: null,
                vehicleType: null,
                vehicleRegion: null,
                filterPlatesSeenLessThan: 0,
                pageNumber: 0,
                pageSize: 10,
                CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().BestNumber.Should().Be("ABC123");
        }

        [Test]
        public async Task SearchPlatesAsync_WithVehicleColor_ReturnsFilteredResults()
        {
            // Arrange
            var testPlates = new List<PlateGroup>
            {
                new PlateGroup
                {
                    Id = Guid.NewGuid(),
                    BestNumber = "RED001",
                    VehicleColor = "red",
                    VehicleMakeModel = "toyota camry",
                    ReceivedOnEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    PossibleNumbers = new List<PlateGroupPossibleNumbers>()
                },
                new PlateGroup
                {
                    Id = Guid.NewGuid(),
                    BestNumber = "BLUE001",
                    VehicleColor = "blue",
                    VehicleMakeModel = "honda civic",
                    ReceivedOnEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    PossibleNumbers = new List<PlateGroupPossibleNumbers>()
                }
            };

            await _context.PlateGroups.AddRangeAsync(testPlates);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.SearchPlatesAsync(
                plateNumber: null,
                strictMatch: false,
                regexSearchEnabled: false,
                startDate: null,
                endDate: null,
                platesToIgnore: new List<string>(),
                vehicleColor: "red",
                vehicleMake: null,
                vehicleModel: null,
                vehicleType: null,
                vehicleRegion: null,
                filterPlatesSeenLessThan: 0,
                pageNumber: 0,
                pageSize: 10,
                CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().VehicleColor.Should().Be("red");
        }

        [Test]
        public async Task SearchPlatesAsync_WithDateRange_ReturnsFilteredResults()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var yesterday = now.AddDays(-1);
            var tomorrow = now.AddDays(1);

            var testPlates = new List<PlateGroup>
            {
                new PlateGroup
                {
                    Id = Guid.NewGuid(),
                    BestNumber = "OLD001",
                    VehicleColor = "red",
                    VehicleMakeModel = "toyota camry",
                    ReceivedOnEpoch = yesterday.ToUnixTimeMilliseconds(),
                    PossibleNumbers = new List<PlateGroupPossibleNumbers>()
                },
                new PlateGroup
                {
                    Id = Guid.NewGuid(),
                    BestNumber = "NEW001",
                    VehicleColor = "blue",
                    VehicleMakeModel = "honda civic",
                    ReceivedOnEpoch = now.ToUnixTimeMilliseconds(),
                    PossibleNumbers = new List<PlateGroupPossibleNumbers>()
                }
            };

            await _context.PlateGroups.AddRangeAsync(testPlates);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.SearchPlatesAsync(
                plateNumber: null,
                strictMatch: false,
                regexSearchEnabled: false,
                startDate: now.AddHours(-1),
                endDate: tomorrow,
                platesToIgnore: new List<string>(),
                vehicleColor: null,
                vehicleMake: null,
                vehicleModel: null,
                vehicleType: null,
                vehicleRegion: null,
                filterPlatesSeenLessThan: 0,
                pageNumber: 0,
                pageSize: 10,
                CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().BestNumber.Should().Be("NEW001");
        }

        [Test]
        public async Task GetSearchResultsCountAsync_ReturnsCorrectCount()
        {
            // Arrange
            var testPlates = new List<PlateGroup>
            {
                new PlateGroup
                {
                    Id = Guid.NewGuid(),
                    BestNumber = "ABC123",
                    VehicleColor = "red",
                    VehicleMakeModel = "toyota camry",
                    ReceivedOnEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    PossibleNumbers = new List<PlateGroupPossibleNumbers>()
                },
                new PlateGroup
                {
                    Id = Guid.NewGuid(),
                    BestNumber = "ABC456",
                    VehicleColor = "red",
                    VehicleMakeModel = "honda civic",
                    ReceivedOnEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    PossibleNumbers = new List<PlateGroupPossibleNumbers>()
                }
            };

            await _context.PlateGroups.AddRangeAsync(testPlates);
            await _context.SaveChangesAsync();

            // Act
            var count = await _repository.GetSearchResultsCountAsync(
                plateNumber: null,
                strictMatch: false,
                regexSearchEnabled: false,
                startDate: null,
                endDate: null,
                platesToIgnore: new List<string>(),
                vehicleColor: "red",
                vehicleMake: null,
                vehicleModel: null,
                vehicleType: null,
                vehicleRegion: null,
                filterPlatesSeenLessThan: 0,
                CancellationToken.None);

            // Assert
            count.Should().Be(2);
        }

        [Test]
        public async Task GetByIdWithDetailsAsync_ReturnsPlateWithPossibleNumbers()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var testPlate = new PlateGroup
            {
                Id = plateId,
                BestNumber = "ABC123",
                VehicleColor = "red",
                VehicleMakeModel = "toyota camry",
                ReceivedOnEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                PossibleNumbers = new List<PlateGroupPossibleNumbers>
                {
                    new PlateGroupPossibleNumbers { Number = "ABC123" },
                    new PlateGroupPossibleNumbers { Number = "A8C123" }
                }
            };

            await _context.PlateGroups.AddAsync(testPlate);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdWithDetailsAsync(plateId, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(plateId);
            result.PossibleNumbers.Should().HaveCount(2);
        }
    }
} 