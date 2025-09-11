using AwesomeAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using Tests.TestHelpers;

namespace Tests.Data.Repositories
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class PlateGroupRepositoryTests : TestBase
    {
        [Test]
        public async Task GetPlateStatisticsAggregationAsync_PlateNumberInBestNumberOnly_ReturnsCorrectCount()
        {
            // Arrange
            var plateNumber = "ABC123";
            var now = DateTimeOffset.UtcNow;
            var last90DaysEpoch = now.AddDays(-90).ToUnixTimeMilliseconds();

            // Create plate groups where the plate number is only in BestNumber
            var plateGroup1 = TestDataFactory.CreateTestPlateGroup(plateNumber, now.AddDays(-30).ToUnixTimeMilliseconds());
            var plateGroup2 = TestDataFactory.CreateTestPlateGroup(plateNumber, now.AddDays(-60).ToUnixTimeMilliseconds());
            var plateGroup3 = TestDataFactory.CreateTestPlateGroup("DIFFERENT", now.AddDays(-10).ToUnixTimeMilliseconds());

            await UnitOfWork.PlateGroups.AddAsync(plateGroup1);
            await UnitOfWork.PlateGroups.AddAsync(plateGroup2);
            await UnitOfWork.PlateGroups.AddAsync(plateGroup3);
            await UnitOfWork.SaveChangesAsync();

            // Act
            var result = await UnitOfWork.PlateGroups.GetPlateStatisticsAggregationAsync(
                plateNumber, last90DaysEpoch, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(2);
            result.Last90DaysCount.Should().Be(2);
            result.MinEpoch.Should().Be(plateGroup2.ReceivedOnEpoch);
            result.MaxEpoch.Should().Be(plateGroup1.ReceivedOnEpoch);
        }

        [Test]
        public async Task GetPlateStatisticsAggregationAsync_PlateNumberInPossibleNumbersOnly_ReturnsCorrectCount()
        {
            // Arrange
            var plateNumber = "XYZ789";
            var now = DateTimeOffset.UtcNow;
            var last90DaysEpoch = now.AddDays(-90).ToUnixTimeMilliseconds();

            // Create plate groups where the plate number is only in PossibleNumbers
            var plateGroup1 = TestDataFactory.CreateTestPlateGroup("BEST123", now.AddDays(-20).ToUnixTimeMilliseconds());
            plateGroup1.PossibleNumbers = new List<PlateGroupPossibleNumbers>
            {
                new PlateGroupPossibleNumbers { Id = Guid.NewGuid(), PlateGroupId = plateGroup1.Id, Number = plateNumber }
            };

            var plateGroup2 = TestDataFactory.CreateTestPlateGroup("BEST456", now.AddDays(-50).ToUnixTimeMilliseconds());
            plateGroup2.PossibleNumbers = new List<PlateGroupPossibleNumbers>
            {
                new PlateGroupPossibleNumbers { Id = Guid.NewGuid(), PlateGroupId = plateGroup2.Id, Number = plateNumber }
            };

            var plateGroup3 = TestDataFactory.CreateTestPlateGroup("DIFFERENT", now.AddDays(-10).ToUnixTimeMilliseconds());

            await UnitOfWork.PlateGroups.AddAsync(plateGroup1);
            await UnitOfWork.PlateGroups.AddAsync(plateGroup2);
            await UnitOfWork.PlateGroups.AddAsync(plateGroup3);
            await UnitOfWork.SaveChangesAsync();

            // Act
            var result = await UnitOfWork.PlateGroups.GetPlateStatisticsAggregationAsync(
                plateNumber, last90DaysEpoch, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(2);
            result.Last90DaysCount.Should().Be(2);
            result.MinEpoch.Should().Be(plateGroup2.ReceivedOnEpoch);
            result.MaxEpoch.Should().Be(plateGroup1.ReceivedOnEpoch);
        }

        [Test]
        public async Task GetPlateStatisticsAggregationAsync_PlateNumberInBothBestAndPossible_CountsOnlyOnce()
        {
            // Arrange
            var plateNumber = "DUPLICATE123";
            var now = DateTimeOffset.UtcNow;
            var last90DaysEpoch = now.AddDays(-90).ToUnixTimeMilliseconds();

            // Create a plate group where the plate number is in BOTH BestNumber AND PossibleNumbers
            var plateGroup1 = TestDataFactory.CreateTestPlateGroup(plateNumber, now.AddDays(-30).ToUnixTimeMilliseconds());
            plateGroup1.PossibleNumbers = new List<PlateGroupPossibleNumbers>
            {
                new PlateGroupPossibleNumbers { Id = Guid.NewGuid(), PlateGroupId = plateGroup1.Id, Number = plateNumber },
                new PlateGroupPossibleNumbers { Id = Guid.NewGuid(), PlateGroupId = plateGroup1.Id, Number = "OTHER123" }
            };

            // Create another plate group with different plate number
            var plateGroup2 = TestDataFactory.CreateTestPlateGroup("DIFFERENT456", now.AddDays(-60).ToUnixTimeMilliseconds());

            await UnitOfWork.PlateGroups.AddAsync(plateGroup1);
            await UnitOfWork.PlateGroups.AddAsync(plateGroup2);
            await UnitOfWork.SaveChangesAsync();

            // Act
            var result = await UnitOfWork.PlateGroups.GetPlateStatisticsAggregationAsync(
                plateNumber, last90DaysEpoch, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(1); // Should count only once, not twice
            result.Last90DaysCount.Should().Be(1);
            result.MinEpoch.Should().Be(plateGroup1.ReceivedOnEpoch);
            result.MaxEpoch.Should().Be(plateGroup1.ReceivedOnEpoch);
        }

        [Test]
        public async Task GetPlateStatisticsAggregationAsync_MultiplePlateGroupsWithMixedMatches_ReturnsCorrectAggregation()
        {
            // Arrange
            var plateNumber = "MIXED123";
            var now = DateTimeOffset.UtcNow;
            var last90DaysEpoch = now.AddDays(-90).ToUnixTimeMilliseconds();

            // Plate group 1: BestNumber matches
            var plateGroup1 = TestDataFactory.CreateTestPlateGroup(plateNumber, now.AddDays(-10).ToUnixTimeMilliseconds());

            // Plate group 2: PossibleNumbers matches
            var plateGroup2 = TestDataFactory.CreateTestPlateGroup("BEST456", now.AddDays(-40).ToUnixTimeMilliseconds());
            plateGroup2.PossibleNumbers = new List<PlateGroupPossibleNumbers>
            {
                new PlateGroupPossibleNumbers { Id = Guid.NewGuid(), PlateGroupId = plateGroup2.Id, Number = plateNumber }
            };

            // Plate group 3: Both BestNumber and PossibleNumbers match (should count only once)
            var plateGroup3 = TestDataFactory.CreateTestPlateGroup(plateNumber, now.AddDays(-70).ToUnixTimeMilliseconds());
            plateGroup3.PossibleNumbers = new List<PlateGroupPossibleNumbers>
            {
                new PlateGroupPossibleNumbers { Id = Guid.NewGuid(), PlateGroupId = plateGroup3.Id, Number = plateNumber }
            };

            // Plate group 4: No match
            var plateGroup4 = TestDataFactory.CreateTestPlateGroup("NOMATCH789", now.AddDays(-20).ToUnixTimeMilliseconds());

            await UnitOfWork.PlateGroups.AddAsync(plateGroup1);
            await UnitOfWork.PlateGroups.AddAsync(plateGroup2);
            await UnitOfWork.PlateGroups.AddAsync(plateGroup3);
            await UnitOfWork.PlateGroups.AddAsync(plateGroup4);
            await UnitOfWork.SaveChangesAsync();

            // Act
            var result = await UnitOfWork.PlateGroups.GetPlateStatisticsAggregationAsync(
                plateNumber, last90DaysEpoch, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(3); // plateGroup1, plateGroup2, plateGroup3 (counted once)
            result.Last90DaysCount.Should().Be(3); // All are within 90 days
            result.MinEpoch.Should().Be(plateGroup3.ReceivedOnEpoch); // Oldest matching
            result.MaxEpoch.Should().Be(plateGroup1.ReceivedOnEpoch); // Newest matching
        }

        [Test]
        public async Task GetPlateStatisticsAggregationAsync_WithLast90DaysFiltering_ReturnsCorrectCounts()
        {
            // Arrange
            var plateNumber = "TIMETEST123";
            var now = DateTimeOffset.UtcNow;
            var last90DaysEpoch = now.AddDays(-90).ToUnixTimeMilliseconds();

            // Plate group 1: Within 90 days
            var plateGroup1 = TestDataFactory.CreateTestPlateGroup(plateNumber, now.AddDays(-30).ToUnixTimeMilliseconds());

            // Plate group 2: Within 90 days
            var plateGroup2 = TestDataFactory.CreateTestPlateGroup(plateNumber, now.AddDays(-60).ToUnixTimeMilliseconds());

            // Plate group 3: Outside 90 days (older)
            var plateGroup3 = TestDataFactory.CreateTestPlateGroup(plateNumber, now.AddDays(-120).ToUnixTimeMilliseconds());

            // Plate group 4: Outside 90 days (much older)
            var plateGroup4 = TestDataFactory.CreateTestPlateGroup(plateNumber, now.AddDays(-200).ToUnixTimeMilliseconds());

            await UnitOfWork.PlateGroups.AddAsync(plateGroup1);
            await UnitOfWork.PlateGroups.AddAsync(plateGroup2);
            await UnitOfWork.PlateGroups.AddAsync(plateGroup3);
            await UnitOfWork.PlateGroups.AddAsync(plateGroup4);
            await UnitOfWork.SaveChangesAsync();

            // Act
            var result = await UnitOfWork.PlateGroups.GetPlateStatisticsAggregationAsync(
                plateNumber, last90DaysEpoch, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(4); // All plate groups
            result.Last90DaysCount.Should().Be(2); // Only plateGroup1 and plateGroup2
            result.MinEpoch.Should().Be(plateGroup4.ReceivedOnEpoch); // Oldest overall
            result.MaxEpoch.Should().Be(plateGroup1.ReceivedOnEpoch); // Newest overall
        }

        [Test]
        public async Task GetPlateStatisticsAggregationAsync_NoMatchingPlates_ReturnsZeroAggregation()
        {
            // Arrange
            var plateNumber = "NOTFOUND123";
            var now = DateTimeOffset.UtcNow;
            var last90DaysEpoch = now.AddDays(-90).ToUnixTimeMilliseconds();

            // Create some plate groups that don't match
            var plateGroup1 = TestDataFactory.CreateTestPlateGroup("DIFFERENT1", now.AddDays(-30).ToUnixTimeMilliseconds());
            var plateGroup2 = TestDataFactory.CreateTestPlateGroup("DIFFERENT2", now.AddDays(-60).ToUnixTimeMilliseconds());

            await UnitOfWork.PlateGroups.AddAsync(plateGroup1);
            await UnitOfWork.PlateGroups.AddAsync(plateGroup2);
            await UnitOfWork.SaveChangesAsync();

            // Act
            var result = await UnitOfWork.PlateGroups.GetPlateStatisticsAggregationAsync(
                plateNumber, last90DaysEpoch, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(0);
            result.Last90DaysCount.Should().Be(0);
            result.MinEpoch.Should().Be(0);
            result.MaxEpoch.Should().Be(0);
        }

        [Test]
        public async Task GetPlateStatisticsAggregationAsync_EmptyDatabase_ReturnsZeroAggregation()
        {
            // Arrange
            var plateNumber = "EMPTY123";
            var now = DateTimeOffset.UtcNow;
            var last90DaysEpoch = now.AddDays(-90).ToUnixTimeMilliseconds();

            // Act (no data in database)
            var result = await UnitOfWork.PlateGroups.GetPlateStatisticsAggregationAsync(
                plateNumber, last90DaysEpoch, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(0);
            result.Last90DaysCount.Should().Be(0);
            result.MinEpoch.Should().Be(0);
            result.MaxEpoch.Should().Be(0);
        }
    }
}
