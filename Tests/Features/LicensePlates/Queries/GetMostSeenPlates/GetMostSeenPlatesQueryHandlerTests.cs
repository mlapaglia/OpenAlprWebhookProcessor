using AwesomeAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetMostSeenPlates;
using Tests.TestHelpers;

namespace Tests.Features.LicensePlates.Queries.GetMostSeenPlates
{
    [TestFixture]
    public class GetMostSeenPlatesQueryHandlerTests : TestBase
    {
        private GetMostSeenPlatesQueryHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new GetMostSeenPlatesQueryHandler(UnitOfWork);
        }

        [Test]
        public async Task Handle_WithNoPlates_ReturnsEmptyList()
        {
            // Arrange
            var query = new GetMostSeenPlatesQuery(null, null, 10);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Counts.Should().BeEmpty();
        }

        [Test]
        public async Task Handle_WithMultiplePlatesForSamePlateNumber_ReturnsCorrectCount()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var plateGroups = new[]
            {
                CreatePlateGroup("ABC123", now.AddDays(-1).ToUnixTimeMilliseconds()),
                CreatePlateGroup("ABC123", now.AddDays(-2).ToUnixTimeMilliseconds()),
                CreatePlateGroup("ABC123", now.AddDays(-3).ToUnixTimeMilliseconds()),
                CreatePlateGroup("XYZ789", now.AddDays(-1).ToUnixTimeMilliseconds()),
                CreatePlateGroup("XYZ789", now.AddDays(-2).ToUnixTimeMilliseconds())
            };

            await SeedPlateGroupsAsync(plateGroups);

            var query = new GetMostSeenPlatesQuery(null, null, 10);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Counts.Should().HaveCount(2);
            
            var abcCount = result.Counts.FirstOrDefault(c => c.PlateNumber == "ABC123");
            abcCount.Should().NotBeNull();
            abcCount.Count.Should().Be(3);
            
            var xyzCount = result.Counts.FirstOrDefault(c => c.PlateNumber == "XYZ789");
            xyzCount.Should().NotBeNull();
            xyzCount.Count.Should().Be(2);
        }

        [Test]
        public async Task Handle_ReturnsPlatesOrderedByCountDescending()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var plateGroups = new[]
            {
                // ABC123 appears 3 times
                CreatePlateGroup("ABC123", now.AddDays(-1).ToUnixTimeMilliseconds()),
                CreatePlateGroup("ABC123", now.AddDays(-2).ToUnixTimeMilliseconds()),
                CreatePlateGroup("ABC123", now.AddDays(-3).ToUnixTimeMilliseconds()),
                
                // XYZ789 appears 1 time
                CreatePlateGroup("XYZ789", now.AddDays(-1).ToUnixTimeMilliseconds()),
                
                // DEF456 appears 2 times
                CreatePlateGroup("DEF456", now.AddDays(-1).ToUnixTimeMilliseconds()),
                CreatePlateGroup("DEF456", now.AddDays(-2).ToUnixTimeMilliseconds())
            };

            await SeedPlateGroupsAsync(plateGroups);

            var query = new GetMostSeenPlatesQuery(null, null, 10);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Counts.Should().HaveCount(3);
            result.Counts[0].PlateNumber.Should().Be("ABC123");
            result.Counts[0].Count.Should().Be(3);
            result.Counts[1].PlateNumber.Should().Be("DEF456");
            result.Counts[1].Count.Should().Be(2);
            result.Counts[2].PlateNumber.Should().Be("XYZ789");
            result.Counts[2].Count.Should().Be(1);
        }

        [Test]
        public async Task Handle_WithLimitParameter_ReturnsOnlyTopResults()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var plateGroups = new[]
            {
                // Create plates for multiple plate numbers
                CreatePlateGroup("PLATE001", now.ToUnixTimeMilliseconds()),
                CreatePlateGroup("PLATE001", now.AddMinutes(-1).ToUnixTimeMilliseconds()),
                CreatePlateGroup("PLATE001", now.AddMinutes(-2).ToUnixTimeMilliseconds()),
                
                CreatePlateGroup("PLATE002", now.ToUnixTimeMilliseconds()),
                CreatePlateGroup("PLATE002", now.AddMinutes(-1).ToUnixTimeMilliseconds()),
                
                CreatePlateGroup("PLATE003", now.ToUnixTimeMilliseconds()),
                
                CreatePlateGroup("PLATE004", now.ToUnixTimeMilliseconds()),
                CreatePlateGroup("PLATE004", now.AddMinutes(-1).ToUnixTimeMilliseconds()),
                CreatePlateGroup("PLATE004", now.AddMinutes(-2).ToUnixTimeMilliseconds()),
                CreatePlateGroup("PLATE004", now.AddMinutes(-3).ToUnixTimeMilliseconds())
            };

            await SeedPlateGroupsAsync(plateGroups);

            var query = new GetMostSeenPlatesQuery(null, null, 2); // Limit to top 2

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Counts.Should().HaveCount(2);
            result.Counts[0].PlateNumber.Should().Be("PLATE004");
            result.Counts[0].Count.Should().Be(4);
            result.Counts[1].PlateNumber.Should().Be("PLATE001");
            result.Counts[1].Count.Should().Be(3);
        }

        [Test]
        public async Task Handle_WithStartDateFilter_OnlyIncludesPlatesAfterStartDate()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var startDate = now.AddDays(-5);
            
            var plateGroups = new[]
            {
                CreatePlateGroup("ABC123", now.AddDays(-2).ToUnixTimeMilliseconds()), // Within range
                CreatePlateGroup("ABC123", now.AddDays(-3).ToUnixTimeMilliseconds()), // Within range
                CreatePlateGroup("ABC123", now.AddDays(-7).ToUnixTimeMilliseconds()), // Before start date
                CreatePlateGroup("XYZ789", now.AddDays(-1).ToUnixTimeMilliseconds())  // Within range
            };

            await SeedPlateGroupsAsync(plateGroups);

            var query = new GetMostSeenPlatesQuery(startDate, null, 10);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Counts.Should().HaveCount(2);
            
            var abcCount = result.Counts.FirstOrDefault(c => c.PlateNumber == "ABC123");
            abcCount.Should().NotBeNull();
            abcCount.Count.Should().Be(2); // Only 2 within date range
            
            var xyzCount = result.Counts.FirstOrDefault(c => c.PlateNumber == "XYZ789");
            xyzCount.Should().NotBeNull();
            xyzCount.Count.Should().Be(1);
        }

        [Test]
        public async Task Handle_WithEndDateFilter_OnlyIncludesPlatesBeforeEndDate()
        {
            // Arrange
            var now = new DateTimeOffset(DateTimeOffset.UtcNow.Date, TimeSpan.Zero);
            var endDate = now.AddDays(-3);
            
            var plateGroups = new[]
            {
                CreatePlateGroup("ABC123", now.AddDays(-1).ToUnixTimeMilliseconds()), // After end date
                CreatePlateGroup("ABC123", now.AddDays(-4).ToUnixTimeMilliseconds()), // Before end date
                CreatePlateGroup("ABC123", now.AddDays(-5).ToUnixTimeMilliseconds()), // Before end date
                CreatePlateGroup("XYZ789", now.AddDays(-2).ToUnixTimeMilliseconds())  // After end date
            };

            await SeedPlateGroupsAsync(plateGroups);

            var query = new GetMostSeenPlatesQuery(null, endDate, 10);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Counts.Should().HaveCount(1);
            
            var abcCount = result.Counts.FirstOrDefault(c => c.PlateNumber == "ABC123");
            abcCount.Should().NotBeNull();
            abcCount.Count.Should().Be(2); // Only 2 within date range
        }

        [Test]
        public async Task Handle_WithBothStartAndEndDate_FiltersCorrectly()
        {
            // Arrange
            var baseDate = new DateTimeOffset(2023, 10, 15, 0, 0, 0, TimeSpan.Zero);
            var startDate = baseDate.AddDays(-7);
            var endDate = baseDate.AddDays(-3);
            
            var plateGroups = new[]
            {
                CreatePlateGroup("ABC123", baseDate.AddDays(-1).ToUnixTimeMilliseconds()),  // After range
                CreatePlateGroup("ABC123", baseDate.AddDays(-5).ToUnixTimeMilliseconds()),  // Within range
                CreatePlateGroup("ABC123", baseDate.AddDays(-6).ToUnixTimeMilliseconds()),  // Within range
                CreatePlateGroup("ABC123", baseDate.AddDays(-10).ToUnixTimeMilliseconds()), // Before range
                CreatePlateGroup("XYZ789", baseDate.AddDays(-4).ToUnixTimeMilliseconds())   // Within range
            };

            await SeedPlateGroupsAsync(plateGroups);

            var query = new GetMostSeenPlatesQuery(startDate, endDate, 10);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Counts.Should().HaveCount(2);
            
            var abcCount = result.Counts.FirstOrDefault(c => c.PlateNumber == "ABC123");
            abcCount.Should().NotBeNull();
            abcCount.Count.Should().Be(2); // Only 2 within date range
            
            var xyzCount = result.Counts.FirstOrDefault(c => c.PlateNumber == "XYZ789");
            xyzCount.Should().NotBeNull();
            xyzCount.Count.Should().Be(1);
        }

        [Test]
        public async Task Handle_WithDefaultDateRange_UsesLast30Days()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            
            var plateGroups = new[]
            {
                CreatePlateGroup("ABC123", now.AddDays(-10).ToUnixTimeMilliseconds()), // Within default 30 days
                CreatePlateGroup("ABC123", now.AddDays(-20).ToUnixTimeMilliseconds()), // Within default 30 days
                CreatePlateGroup("ABC123", now.AddDays(-40).ToUnixTimeMilliseconds())  // Outside default 30 days
            };

            await SeedPlateGroupsAsync(plateGroups);

            var query = new GetMostSeenPlatesQuery(null, null, 10); // No dates specified

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Counts.Should().HaveCount(1);
            
            var abcCount = result.Counts.FirstOrDefault(c => c.PlateNumber == "ABC123");
            abcCount.Should().NotBeNull();
            abcCount.Count.Should().Be(2); // Only 2 within default 30 days
        }

        [Test]
        public async Task Handle_WithCancellationToken_RespectsToken()
        {
            // Arrange
            var query = new GetMostSeenPlatesQuery(null, null, 10);
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
        }

        private PlateGroup CreatePlateGroup(string plateNumber, long epoch)
        {
            return new PlateGroup
            {
                Id = Guid.NewGuid(),
                BestNumber = plateNumber,
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