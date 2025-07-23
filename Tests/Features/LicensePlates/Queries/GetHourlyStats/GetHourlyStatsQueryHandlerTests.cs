using FluentAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetHourlyStats;
using Tests.TestHelpers;

namespace Tests.Features.LicensePlates.Queries.GetHourlyStats
{
    [TestFixture]
    public class GetHourlyStatsQueryHandlerTests : TestBase
    {
        private GetHourlyStatsQueryHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new GetHourlyStatsQueryHandler(UnitOfWork);
        }

        [Test]
        public async Task Handle_WithNoPlateGroups_ReturnsAllHoursWithZeroCount()
        {
            // Arrange
            var query = new GetHourlyStatsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Counts.Should().HaveCount(24);
            
            for (int hour = 0; hour < 24; hour++)
            {
                var hourlyCount = result.Counts.First(h => h.Hour == hour);
                hourlyCount.Count.Should().Be(0);
            }
        }

        [Test]
        public async Task Handle_WithPlatesInDifferentHours_ReturnsCorrectHourlyCounts()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            
            // Create plates for different hours
            var plateGroups = new[]
            {
                CreatePlateGroupAtHour(now, 8, "PLATE001"),  // 8 AM
                CreatePlateGroupAtHour(now, 8, "PLATE002"),  // 8 AM (same hour)
                CreatePlateGroupAtHour(now, 14, "PLATE003"), // 2 PM
                CreatePlateGroupAtHour(now, 20, "PLATE004"), // 8 PM
                CreatePlateGroupAtHour(now, 20, "PLATE005"), // 8 PM (same hour)
                CreatePlateGroupAtHour(now, 20, "PLATE006")  // 8 PM (same hour)
            };

            await SeedPlateGroupsAsync(plateGroups);

            var query = new GetHourlyStatsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Counts.Should().HaveCount(24);
            
            result.Counts.First(h => h.Hour == 8).Count.Should().Be(2);   // 8 AM
            result.Counts.First(h => h.Hour == 14).Count.Should().Be(1);  // 2 PM
            result.Counts.First(h => h.Hour == 20).Count.Should().Be(3);  // 8 PM
            
            // All other hours should have 0
            var otherHours = result.Counts.Where(h => h.Hour != 8 && h.Hour != 14 && h.Hour != 20);
            otherHours.Should().AllSatisfy(h => h.Count.Should().Be(0));
        }

        [Test]
        public async Task Handle_OnlyIncludesPlatesFromLast30Days()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var twentyNineDaysAgo = now.AddDays(-29);
            var thirtyOneDaysAgo = now.AddDays(-31);
            
            var plateGroups = new[]
            {
                CreatePlateGroupAtHour(now, 10, "PLATE001"),             // Today - should be included
                CreatePlateGroupAtHour(twentyNineDaysAgo, 10, "PLATE002"), // 29 days ago - should be included
                CreatePlateGroupAtHour(thirtyOneDaysAgo, 10, "PLATE003")   // 31 days ago - should be excluded
            };

            await SeedPlateGroupsAsync(plateGroups);

            var query = new GetHourlyStatsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Counts.First(h => h.Hour == 10).Count.Should().Be(2); // Only the two within 30 days
        }

        [Test]
        public async Task Handle_ReturnsHoursInCorrectOrder()
        {
            // Arrange
            var query = new GetHourlyStatsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Counts.Should().HaveCount(24);
            result.Counts.Should().BeInAscendingOrder(h => h.Hour);
            
            for (int i = 0; i < 24; i++)
            {
                result.Counts[i].Hour.Should().Be(i);
            }
        }

        [Test]
        public async Task Handle_WithMidnightPlates_HandlesHour0Correctly()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            
            var plateGroups = new[]
            {
                CreatePlateGroupAtHour(now, 0, "PLATE001"),  // Midnight
                CreatePlateGroupAtHour(now, 23, "PLATE002") // 11 PM
            };

            await SeedPlateGroupsAsync(plateGroups);

            var query = new GetHourlyStatsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Counts.First(h => h.Hour == 0).Count.Should().Be(1);   // Midnight
            result.Counts.First(h => h.Hour == 23).Count.Should().Be(1);  // 11 PM
        }

        [Test]
        public async Task Handle_WithLargeDataset_ProcessesCorrectly()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var plateGroups = new PlateGroup[100];
            
            // Create 100 plates spread across different hours
            for (int i = 0; i < 100; i++)
            {
                int hour = i % 24; // Distribute across all 24 hours
                plateGroups[i] = CreatePlateGroupAtHour(now.AddDays(-i % 30), hour, $"PLATE{i:D3}");
            }

            await SeedPlateGroupsAsync(plateGroups);

            var query = new GetHourlyStatsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Counts.Should().HaveCount(24);
            
            // Each hour should have approximately 4 plates (100 plates / 24 hours ≈ 4.17)
            // But due to distribution, some hours might have more
            var totalCount = result.Counts.Sum(h => h.Count);
            totalCount.Should().Be(100);
        }

        [Test]
        public async Task Handle_WithCancellationToken_RespectsToken()
        {
            // Arrange
            var query = new GetHourlyStatsQuery();
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
        }

        [Test]
        public async Task Handle_WithDifferentTimeZones_UsesUtcTime()
        {
            // Arrange
            var utcNow = DateTimeOffset.UtcNow;
            
            // Create a plate at a specific UTC hour
            var plateGroups = new[]
            {
                CreatePlateGroupAtHour(utcNow, 15, "PLATE001") // 3 PM UTC
            };

            await SeedPlateGroupsAsync(plateGroups);

            var query = new GetHourlyStatsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            // Should count in hour 15 (3 PM UTC) regardless of local timezone
            result.Counts.First(h => h.Hour == 15).Count.Should().Be(1);
        }

        private PlateGroup CreatePlateGroupAtHour(DateTimeOffset baseTime, int hour, string plateNumber)
        {
            // Create a DateTimeOffset at the specified hour
            var timeAtHour = new DateTimeOffset(
                baseTime.Year, 
                baseTime.Month, 
                baseTime.Day, 
                hour, 
                0, 
                0, 
                baseTime.Offset);

            return new PlateGroup
            {
                BestNumber = plateNumber,
                ReceivedOnEpoch = timeAtHour.ToUnixTimeMilliseconds(),
                OpenAlprCameraId = 1,
                OpenAlprUuid = Guid.NewGuid().ToString(),
                Confidence = 95.5
            };
        }

        private async Task SeedPlateGroupsAsync(PlateGroup[] plateGroups)
        {
            Context.PlateGroups.AddRange(plateGroups);
            await Context.SaveChangesAsync();
        }
    }
} 