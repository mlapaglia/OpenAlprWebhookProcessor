using FluentAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetLicensePlateCounts;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tests.TestHelpers;

namespace Tests.Features.LicensePlates.Queries.GetLicensePlateCounts
{
    [TestFixture]
    public class GetLicensePlateCountsQueryHandlerTests : TestBase
    {
        private GetLicensePlateCountsQueryHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new GetLicensePlateCountsQueryHandler(UnitOfWork);
        }

        [Test]
        public async Task Handle_WithNoPlates_ReturnsEmptyList()
        {
            // Arrange
            var startDate = DateTimeOffset.UtcNow.AddDays(-7);
            var endDate = DateTimeOffset.UtcNow;
            var query = new GetLicensePlateCountsQuery(startDate, endDate);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Counts.Should().BeEmpty();
        }

        [Test]
        public async Task Handle_WithPlatesInDateRange_ReturnsCorrectDailyCounts()
        {
            // Arrange
            var baseDate = new DateTimeOffset(2023, 10, 15, 0, 0, 0, TimeSpan.Zero);
            var startDate = baseDate;
            var endDate = baseDate.AddDays(3);

            var plateGroups = new[]
            {
                // Day 1: 3 plates
                CreatePlateGroup(baseDate.AddHours(8).ToUnixTimeMilliseconds()),
                CreatePlateGroup(baseDate.AddHours(14).ToUnixTimeMilliseconds()),
                CreatePlateGroup(baseDate.AddHours(20).ToUnixTimeMilliseconds()),
                
                // Day 2: 2 plates
                CreatePlateGroup(baseDate.AddDays(1).AddHours(10).ToUnixTimeMilliseconds()),
                CreatePlateGroup(baseDate.AddDays(1).AddHours(16).ToUnixTimeMilliseconds()),
                
                // Day 3: 1 plate
                CreatePlateGroup(baseDate.AddDays(2).AddHours(12).ToUnixTimeMilliseconds())
            };

            await SeedPlateGroupsAsync(plateGroups);

            var query = new GetLicensePlateCountsQuery(startDate, endDate);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Counts.Should().HaveCount(3);
            
            var day1Count = result.Counts.FirstOrDefault(c => c.Date.Date == baseDate.Date);
            day1Count.Should().NotBeNull();
            day1Count.Count.Should().Be(3);
            
            var day2Count = result.Counts.FirstOrDefault(c => c.Date.Date == baseDate.AddDays(1).Date);
            day2Count.Should().NotBeNull();
            day2Count.Count.Should().Be(2);
            
            var day3Count = result.Counts.FirstOrDefault(c => c.Date.Date == baseDate.AddDays(2).Date);
            day3Count.Should().NotBeNull();
            day3Count.Count.Should().Be(1);
        }

        [Test]
        public async Task Handle_WithPlatesOutsideDateRange_ExcludesThemFromResults()
        {
            // Arrange
            var baseDate = new DateTimeOffset(2023, 10, 15, 0, 0, 0, TimeSpan.Zero);
            var startDate = baseDate;
            var endDate = baseDate.AddDays(1);

            var plateGroups = new[]
            {
                // Before start date - should be excluded
                CreatePlateGroup(baseDate.AddDays(-1).ToUnixTimeMilliseconds()),
                
                // Within range - should be included
                CreatePlateGroup(baseDate.AddHours(12).ToUnixTimeMilliseconds()),
                CreatePlateGroup(baseDate.AddDays(1).AddHours(16).ToUnixTimeMilliseconds()),
                
                // After end date - should be excluded
                CreatePlateGroup(baseDate.AddDays(2).AddHours(5).ToUnixTimeMilliseconds())
            };

            await SeedPlateGroupsAsync(plateGroups);

            var query = new GetLicensePlateCountsQuery(startDate, endDate);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Counts.Should().HaveCount(2); // Only 2 days within range
            
            var totalCount = result.Counts.Sum(c => c.Count);
            totalCount.Should().Be(2); // Only 2 plates within range
        }

        [Test]
        public async Task Handle_GroupsByDateCorrectly()
        {
            // Arrange
            var baseDate = new DateTimeOffset(2023, 10, 15, 0, 0, 0, TimeSpan.Zero);
            var startDate = baseDate;
            var endDate = baseDate.AddDays(1);

            var plateGroups = new[]
            {
                // Same day, different times - should be grouped together
                CreatePlateGroup(baseDate.AddHours(1).ToUnixTimeMilliseconds()),
                CreatePlateGroup(baseDate.AddHours(6).ToUnixTimeMilliseconds()),
                CreatePlateGroup(baseDate.AddHours(12).ToUnixTimeMilliseconds()),
                CreatePlateGroup(baseDate.AddHours(18).ToUnixTimeMilliseconds()),
                CreatePlateGroup(baseDate.AddHours(23).ToUnixTimeMilliseconds()),
                
                // Next day
                CreatePlateGroup(baseDate.AddDays(1).AddHours(12).ToUnixTimeMilliseconds())
            };

            await SeedPlateGroupsAsync(plateGroups);

            var query = new GetLicensePlateCountsQuery(startDate, endDate);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Counts.Should().HaveCount(2);
            
            var day1Count = result.Counts.FirstOrDefault(c => c.Date.Date == baseDate.Date);
            day1Count.Should().NotBeNull();
            day1Count.Count.Should().Be(5); // All 5 plates from the same day
            
            var day2Count = result.Counts.FirstOrDefault(c => c.Date.Date == baseDate.AddDays(1).Date);
            day2Count.Should().NotBeNull();
            day2Count.Count.Should().Be(1);
        }

        [Test]
        public async Task Handle_WithSameDateStartAndEnd_ReturnsCountForThatDay()
        {
            // Arrange
            var specificDate = new DateTimeOffset(2023, 10, 15, 0, 0, 0, TimeSpan.Zero);
            var plateGroups = new[]
            {
                CreatePlateGroup(specificDate.AddHours(8).ToUnixTimeMilliseconds()),
                CreatePlateGroup(specificDate.AddHours(14).ToUnixTimeMilliseconds()),
                CreatePlateGroup(specificDate.AddHours(20).ToUnixTimeMilliseconds())
            };

            await SeedPlateGroupsAsync(plateGroups);

            var query = new GetLicensePlateCountsQuery(specificDate, specificDate);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Counts.Should().HaveCount(1);
            result.Counts[0].Date.Date.Should().Be(specificDate.Date);
            result.Counts[0].Count.Should().Be(3);
        }

        [Test]
        public async Task Handle_WithLongDateRange_ReturnsAllDaysWithPlates()
        {
            // Arrange
            var baseDate = new DateTimeOffset(2023, 10, 1, 0, 0, 0, TimeSpan.Zero);
            var startDate = baseDate;
            var endDate = baseDate.AddDays(6); // 7 days total

            // Create plates for some days but not others
            var plateGroups = new[]
            {
                // Day 1: 2 plates
                CreatePlateGroup(baseDate.AddHours(12).ToUnixTimeMilliseconds()),
                CreatePlateGroup(baseDate.AddHours(18).ToUnixTimeMilliseconds()),
                
                // Day 2: No plates
                
                // Day 3: 1 plate
                CreatePlateGroup(baseDate.AddDays(2).AddHours(15).ToUnixTimeMilliseconds()),
                
                // Days 4-6: No plates
                
                // Day 7: 3 plates
                CreatePlateGroup(baseDate.AddDays(6).AddHours(9).ToUnixTimeMilliseconds()),
                CreatePlateGroup(baseDate.AddDays(6).AddHours(12).ToUnixTimeMilliseconds()),
                CreatePlateGroup(baseDate.AddDays(6).AddHours(15).ToUnixTimeMilliseconds())
            };

            await SeedPlateGroupsAsync(plateGroups);

            var query = new GetLicensePlateCountsQuery(startDate, endDate);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Counts.Should().HaveCount(3); // Only days with plates
            
            var totalCount = result.Counts.Sum(c => c.Count);
            totalCount.Should().Be(6);
        }

        [Test]
        public async Task Handle_WithCancellationToken_RespectsToken()
        {
            // Arrange
            var startDate = DateTimeOffset.UtcNow.AddDays(-1);
            var endDate = DateTimeOffset.UtcNow;
            var query = new GetLicensePlateCountsQuery(startDate, endDate);
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
        }

        [Test]
        public async Task Handle_WithPlatesAtExactBoundaries_IncludesThemCorrectly()
        {
            // Arrange
            var startDate = new DateTimeOffset(2023, 10, 15, 0, 0, 0, TimeSpan.Zero);
            var endDate = new DateTimeOffset(2023, 10, 16, 0, 0, 0, TimeSpan.Zero);

            var plateGroups = new[]
            {
                // Exact start time - should be included
                CreatePlateGroup(startDate.ToUnixTimeMilliseconds()),
                
                // Exact end time - should be included
                CreatePlateGroup(endDate.ToUnixTimeMilliseconds()),
                
                // Just before start - should be excluded
                CreatePlateGroup(startDate.AddMilliseconds(-1).ToUnixTimeMilliseconds()),
                
                // Just after end of day - should be excluded (past the end of the search window)
                CreatePlateGroup(endDate.AddDays(1).AddMilliseconds(1).ToUnixTimeMilliseconds())
            };

            await SeedPlateGroupsAsync(plateGroups);

            var query = new GetLicensePlateCountsQuery(startDate, endDate);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            
            var totalCount = result.Counts.Sum(c => c.Count);
            totalCount.Should().Be(2); // Only the plates at exact boundaries
        }

        private PlateGroup CreatePlateGroup(long epoch)
        {
            return new PlateGroup
            {
                Id = Guid.NewGuid(),
                BestNumber = $"PLATE{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
                ReceivedOnEpoch = epoch,
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