using AwesomeAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;
using OpenAlprWebhookProcessor.Features.MachineLearning.Services;
using Tests.TestHelpers;

namespace Tests.Features.MachineLearning.Services
{
    [TestFixture]
    public class LicensePlateFeatureExtractorTests : TestBase
    {
        private LicensePlateFeatureExtractor _extractor;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _extractor = new LicensePlateFeatureExtractor(UnitOfWork);
        }

        [Test]
        public async Task ExtractTrainingDataAsync_WithValidPlateGroups_ReturnsTrainingData()
        {
            // Arrange
            var plateGroups = await CreateTestPlateGroupsInDatabase();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _extractor.ExtractTrainingDataAsync(10000, cancellationToken);

            // Assert
            result.Should().NotBeEmpty();
            result.Should().AllSatisfy(td =>
            {
                td.LicensePlate.Should().NotBeNullOrEmpty();
                td.HourOfDay.Should().BeInRange(0, 23);
                td.DayOfWeek.Should().BeInRange(0, 6);
                td.HoursUntilNextSeen.Should().BeGreaterThan(0);
                td.HoursUntilNextSeen.Should().BeLessThan(96);
            });
        }

        [Test]
        public async Task ExtractTrainingDataAsync_FiltersOutPlatesWithLessThanThreeSightings()
        {
            // Arrange
            var baseTime = DateTimeOffset.UtcNow;

            // Create plate with only 2 sightings (should be filtered out)
            await UnitOfWork.PlateGroups.AddAsync(CreatePlateGroup("ABC123", baseTime.AddHours(-5).ToUnixTimeMilliseconds()));
            await UnitOfWork.PlateGroups.AddAsync(CreatePlateGroup("ABC123", baseTime.AddHours(-3).ToUnixTimeMilliseconds()));

            // Create plate with only 1 sighting (should be filtered out)
            await UnitOfWork.PlateGroups.AddAsync(CreatePlateGroup("XYZ789", baseTime.AddHours(-2).ToUnixTimeMilliseconds()));

            await UnitOfWork.SaveChangesAsync();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _extractor.ExtractTrainingDataAsync(10000, cancellationToken);

            // Assert
            result.Should().BeEmpty("plates with less than 3 sightings should be filtered out");
        }

        [Test]
        public async Task ExtractTrainingDataAsync_FiltersOutIrregularVisitors()
        {
            // Arrange
            var baseTime = DateTimeOffset.UtcNow;

            // Create plate with one gap > 4 days and one gap < 4 days
            await UnitOfWork.PlateGroups.AddAsync(CreatePlateGroup("ABC123", baseTime.AddDays(-10).ToUnixTimeMilliseconds()));
            await UnitOfWork.PlateGroups.AddAsync(CreatePlateGroup("ABC123", baseTime.AddDays(-5).ToUnixTimeMilliseconds())); // 5 days gap (120 hours) - too long
            await UnitOfWork.PlateGroups.AddAsync(CreatePlateGroup("ABC123", baseTime.AddDays(-2).ToUnixTimeMilliseconds())); // 3 days gap (72 hours) - valid
            await UnitOfWork.PlateGroups.AddAsync(CreatePlateGroup("ABC123", baseTime.ToUnixTimeMilliseconds())); // 2 days gap (48 hours) - valid

            await UnitOfWork.SaveChangesAsync();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _extractor.ExtractTrainingDataAsync(10000, cancellationToken);

            // Assert
            result.Should().HaveCount(2, "only gaps less than 4 days should be included");
            result.Should().AllSatisfy(td => td.HoursUntilNextSeen.Should().BeLessThan(96f));
        }

        [Test]
        public async Task ExtractTrainingDataAsync_IncludesMultipleValidGaps()
        {
            // Arrange
            var baseTime = DateTimeOffset.UtcNow;

            // Create plate with multiple valid gaps (all < 4 days)
            await UnitOfWork.PlateGroups.AddAsync(CreatePlateGroup("DEF456", baseTime.AddHours(-72).ToUnixTimeMilliseconds())); // 3 days ago
            await UnitOfWork.PlateGroups.AddAsync(CreatePlateGroup("DEF456", baseTime.AddHours(-48).ToUnixTimeMilliseconds())); // 2 days ago
            await UnitOfWork.PlateGroups.AddAsync(CreatePlateGroup("DEF456", baseTime.AddHours(-24).ToUnixTimeMilliseconds())); // 1 day ago
            await UnitOfWork.PlateGroups.AddAsync(CreatePlateGroup("DEF456", baseTime.ToUnixTimeMilliseconds())); // now

            await UnitOfWork.SaveChangesAsync();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _extractor.ExtractTrainingDataAsync(10000, cancellationToken);

            // Assert
            result.Should().HaveCount(3, "all three gaps should be valid");
            result.Should().AllSatisfy(td => td.LicensePlate.Should().Be("DEF456"));
        }

        [Test]
        public async Task ExtractFeaturesForPredictionAsync_WithExistingPlate_ReturnsFeatures()
        {
            // Arrange
            var plateNumber = "ABC123";
            await CreateTestPlateGroupsInDatabase(plateNumber);

            var input = new LicensePlateInput
            {
                LicensePlate = plateNumber,
                CameraId = 1,
                LastSeen = DateTime.UtcNow.AddHours(-2),
                VehicleType = "car",
                VehicleColor = "white"
            };
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _extractor.ExtractFeaturesForPredictionAsync(input, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.LicensePlate.Should().Be(plateNumber);
            result.TotalVisits.Should().Be(5); // Created 5 plate groups
            result.VehicleTypeCode.Should().Be(1); // car
            result.VehicleColorCode.Should().Be(1); // white
            result.HistoricalFrequency.Should().BeGreaterThan(0);
            result.AverageTimeBetweenVisits.Should().BeGreaterThan(0);
        }

        [Test]
        public async Task ExtractFeaturesForPredictionAsync_WithNewPlate_ReturnsDefaultFeatures()
        {
            // Arrange
            var plateNumber = "NEWPLATE";

            var input = new LicensePlateInput
            {
                LicensePlate = plateNumber,
                CameraId = 1,
                LastSeen = DateTime.UtcNow.AddHours(-2),
                VehicleType = "truck",
                VehicleColor = "black"
            };
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _extractor.ExtractFeaturesForPredictionAsync(input, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.LicensePlate.Should().Be(plateNumber);
            result.TotalVisits.Should().Be(1);
            result.HistoricalFrequency.Should().Be(0.1f);
            result.AverageTimeBetweenVisits.Should().Be(168f); // Weekly default
            result.VehicleTypeCode.Should().Be(2); // truck
            result.VehicleColorCode.Should().Be(2); // black
            result.TimeSinceLastSeen.Should().BeApproximately(2f, 0.1f);
        }

        [Test]
        public async Task ExtractFeaturesForPredictionAsync_CalculatesTimeBasedFeaturesCorrectly()
        {
            // Arrange
            var plateNumber = "TIME123";
            var testDateTime = new DateTime(2024, 6, 15, 18, 30, 0, DateTimeKind.Utc); // Saturday, June 15, 6:30 PM UTC
            var testEpoch = ((DateTimeOffset)testDateTime).ToUnixTimeMilliseconds();

            await UnitOfWork.PlateGroups.AddAsync(CreatePlateGroup(plateNumber, testEpoch));
            await UnitOfWork.SaveChangesAsync();

            var input = new LicensePlateInput
            {
                LicensePlate = plateNumber,
                CameraId = 1,
                LastSeen = testDateTime.AddHours(-1), // 5:30 PM UTC
                VehicleType = "car",
                VehicleColor = "white"
            };
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _extractor.ExtractFeaturesForPredictionAsync(input, cancellationToken);

            // Assert
            result.HourOfDay.Should().Be(18);
            result.DayOfWeek.Should().Be(6); // Saturday
            result.DayOfMonth.Should().Be(15);
            result.MonthOfYear.Should().Be(6);
            result.IsWeekend.Should().Be(1);
            result.IsBusinessHour.Should().Be(0); // Weekend, so not business hour
        }

        [Test]
        public async Task ExtractFeaturesForPredictionAsync_CalculatesBusinessHourCorrectly()
        {
            // Arrange
            var plateNumber = "BIZ123";
            var testDateTime = new DateTime(2024, 6, 17, 10, 0, 0); // Monday, June 17, 10:00 AM
            var testEpoch = ((DateTimeOffset)testDateTime).ToUnixTimeMilliseconds();

            await UnitOfWork.PlateGroups.AddAsync(CreatePlateGroup(plateNumber, testEpoch));
            await UnitOfWork.SaveChangesAsync();

            var input = new LicensePlateInput
            {
                LicensePlate = plateNumber,
                CameraId = 1,
                LastSeen = testDateTime.AddHours(-1),
                VehicleType = "car",
                VehicleColor = "white"
            };
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _extractor.ExtractFeaturesForPredictionAsync(input, cancellationToken);

            // Assert
            result.IsWeekend.Should().Be(0);
            result.IsBusinessHour.Should().Be(1); // Weekday, 10 AM
        }

        [TestCase("car", 1f)]
        [TestCase("truck", 2f)]
        [TestCase("suv", 3f)]
        [TestCase("van", 4f)]
        [TestCase("motorcycle", 5f)]
        [TestCase("bus", 6f)]
        [TestCase("unknown", 0f)]
        [TestCase(null, 0f)]
        public async Task ExtractFeaturesForPredictionAsync_EncodesVehicleTypeCorrectly(string vehicleType, float expectedCode)
        {
            // Arrange
            var plateNumber = $"VT{expectedCode}";
            await UnitOfWork.PlateGroups.AddAsync(CreatePlateGroup(plateNumber, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), vehicleType: vehicleType));
            await UnitOfWork.SaveChangesAsync();

            var input = new LicensePlateInput
            {
                LicensePlate = plateNumber,
                CameraId = 1,
                LastSeen = DateTime.UtcNow.AddHours(-1),
                VehicleType = vehicleType,
                VehicleColor = "white"
            };
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _extractor.ExtractFeaturesForPredictionAsync(input, cancellationToken);

            // Assert
            result.VehicleTypeCode.Should().Be(expectedCode);
        }

        [TestCase("white", 1f)]
        [TestCase("black", 2f)]
        [TestCase("silver", 3f)]
        [TestCase("gray", 4f)]
        [TestCase("red", 5f)]
        [TestCase("blue", 6f)]
        [TestCase("green", 7f)]
        [TestCase("yellow", 8f)]
        [TestCase("brown", 9f)]
        [TestCase("unknown", 0f)]
        [TestCase(null, 0f)]
        public async Task ExtractFeaturesForPredictionAsync_EncodesVehicleColorCorrectly(string vehicleColor, float expectedCode)
        {
            // Arrange
            var plateNumber = $"VC{expectedCode}";
            await UnitOfWork.PlateGroups.AddAsync(CreatePlateGroup(plateNumber, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), vehicleColor: vehicleColor));
            await UnitOfWork.SaveChangesAsync();

            var input = new LicensePlateInput
            {
                LicensePlate = plateNumber,
                CameraId = 1,
                LastSeen = DateTime.UtcNow.AddHours(-1),
                VehicleType = "car",
                VehicleColor = vehicleColor
            };
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _extractor.ExtractFeaturesForPredictionAsync(input, cancellationToken);

            // Assert
            result.VehicleColorCode.Should().Be(expectedCode);
        }

        [Test]
        public async Task ExtractFeaturesForPredictionAsync_CalculatesHistoricalFrequency()
        {
            // Arrange
            var plateNumber = "FREQ123";
            var baseTime = DateTimeOffset.UtcNow;

            // Create 3 sightings over 10 days
            await UnitOfWork.PlateGroups.AddAsync(CreatePlateGroup(plateNumber, baseTime.AddDays(-10).ToUnixTimeMilliseconds()));
            await UnitOfWork.PlateGroups.AddAsync(CreatePlateGroup(plateNumber, baseTime.AddDays(-5).ToUnixTimeMilliseconds()));
            await UnitOfWork.PlateGroups.AddAsync(CreatePlateGroup(plateNumber, baseTime.ToUnixTimeMilliseconds()));
            await UnitOfWork.SaveChangesAsync();

            var input = new LicensePlateInput
            {
                LicensePlate = plateNumber,
                CameraId = 1,
                LastSeen = DateTime.UtcNow.AddHours(-1),
                VehicleType = "car",
                VehicleColor = "white"
            };
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _extractor.ExtractFeaturesForPredictionAsync(input, cancellationToken);

            // Assert
            result.HistoricalFrequency.Should().BeApproximately(0.3f, 0.05f); // 3 sightings over 10 days
        }

        [Test]
        public async Task ExtractFeaturesForPredictionAsync_CalculatesAverageTimeBetweenVisits()
        {
            // Arrange
            var plateNumber = "AVG123";
            var baseTime = DateTimeOffset.UtcNow;

            // Create sightings with 5-hour intervals
            await UnitOfWork.PlateGroups.AddAsync(CreatePlateGroup(plateNumber, baseTime.AddHours(-10).ToUnixTimeMilliseconds()));
            await UnitOfWork.PlateGroups.AddAsync(CreatePlateGroup(plateNumber, baseTime.AddHours(-5).ToUnixTimeMilliseconds()));
            await UnitOfWork.PlateGroups.AddAsync(CreatePlateGroup(plateNumber, baseTime.ToUnixTimeMilliseconds()));
            await UnitOfWork.SaveChangesAsync();

            var input = new LicensePlateInput
            {
                LicensePlate = plateNumber,
                CameraId = 1,
                LastSeen = DateTime.UtcNow.AddHours(-1),
                VehicleType = "car",
                VehicleColor = "white"
            };
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _extractor.ExtractFeaturesForPredictionAsync(input, cancellationToken);

            // Assert
            result.AverageTimeBetweenVisits.Should().BeApproximately(5f, 0.1f); // Average of 5 and 5 hours
        }

        [Test]
        public async Task ExtractFeaturesForPredictionAsync_CalculatesSeasonalFactor()
        {
            // Arrange
            var plateNumber = "SEASON123";
            await UnitOfWork.PlateGroups.AddAsync(CreatePlateGroup(plateNumber, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
            await UnitOfWork.SaveChangesAsync();

            var input = new LicensePlateInput
            {
                LicensePlate = plateNumber,
                CameraId = 1,
                LastSeen = DateTime.UtcNow.AddHours(-1),
                VehicleType = "car",
                VehicleColor = "white"
            };
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _extractor.ExtractFeaturesForPredictionAsync(input, cancellationToken);

            // Assert
            result.SeasonalFactor.Should().BeInRange(0f, 1f);
        }

        [Test]
        public async Task ExtractTrainingDataAsync_WithEmptyPlateNumbers_FiltersOutEmptyPlates()
        {
            // Arrange
            await UnitOfWork.PlateGroups.AddAsync(CreatePlateGroup("", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
            await UnitOfWork.PlateGroups.AddAsync(CreatePlateGroup(null, DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeMilliseconds()));
            await UnitOfWork.SaveChangesAsync();

            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _extractor.ExtractTrainingDataAsync(10000, cancellationToken);

            // Assert
            result.Should().BeEmpty("empty or null plate numbers should be filtered out");
        }

        [Test]
        public async Task ExtractTrainingDataAsync_RespectsOrderByReceivedOnEpoch()
        {
            // Arrange
            var plateNumber = "ORDER123";
            var baseTime = DateTimeOffset.UtcNow;

            // Add in random order
            await UnitOfWork.PlateGroups.AddAsync(CreatePlateGroup(plateNumber, baseTime.AddHours(-2).ToUnixTimeMilliseconds()));
            await UnitOfWork.PlateGroups.AddAsync(CreatePlateGroup(plateNumber, baseTime.AddHours(-8).ToUnixTimeMilliseconds()));
            await UnitOfWork.PlateGroups.AddAsync(CreatePlateGroup(plateNumber, baseTime.AddHours(-5).ToUnixTimeMilliseconds()));
            await UnitOfWork.SaveChangesAsync();

            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _extractor.ExtractTrainingDataAsync(10000, cancellationToken);

            // Assert
            result.Should().HaveCount(2); // Two intervals between 3 sightings
            // Verify the intervals are calculated correctly (should be 3 hours and 3 hours)
            result.Should().AllSatisfy(td => td.HoursUntilNextSeen.Should().BeApproximately(3f, 0.1f));
        }

        private async Task<List<PlateGroup>> CreateTestPlateGroupsInDatabase(string plateNumber = "ABC123")
        {
            var baseTime = DateTimeOffset.UtcNow;
            var plateGroups = new List<PlateGroup>
            {
                CreatePlateGroup(plateNumber, baseTime.AddHours(-10).ToUnixTimeMilliseconds()),
                CreatePlateGroup(plateNumber, baseTime.AddHours(-8).ToUnixTimeMilliseconds()),
                CreatePlateGroup(plateNumber, baseTime.AddHours(-6).ToUnixTimeMilliseconds()),
                CreatePlateGroup(plateNumber, baseTime.AddHours(-4).ToUnixTimeMilliseconds()),
                CreatePlateGroup(plateNumber, baseTime.AddHours(-2).ToUnixTimeMilliseconds())
            };

            foreach (var plateGroup in plateGroups)
            {
                await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            }
            await UnitOfWork.SaveChangesAsync();

            return plateGroups;
        }

        private PlateGroup CreatePlateGroup(string plateNumber, long epochTime, string vehicleType = "car", string vehicleColor = "white")
        {
            return new PlateGroup
            {
                BestNumber = plateNumber,
                ReceivedOnEpoch = epochTime,
                OpenAlprCameraId = 1,
                VehicleType = vehicleType,
                VehicleColor = vehicleColor
            };
        }
    }
}