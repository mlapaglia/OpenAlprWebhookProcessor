using AwesomeAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetStatistics;
using Tests.TestHelpers;

namespace Tests.Features.LicensePlates.Queries.GetStatistics
{
    [TestFixture]
    public class GetStatisticsQueryHandlerTests : TestBase
    {
        private GetStatisticsQueryHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new GetStatisticsQueryHandler(UnitOfWork);
        }

        [Test]
        public async Task Handle_WithNoPlates_ReturnsZeroStatistics()
        {
            // Arrange
            var query = new GetStatisticsQuery("ABC123");

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.TotalSeen.Should().Be(0);
            result.Last90Days.Should().Be(0);
            result.FirstSeen.Should().Be(default(DateTimeOffset));
            result.LastSeen.Should().Be(default(DateTimeOffset));
        }

        [Test]
        public async Task Handle_WithBestNumberMatches_ReturnsCorrectStatistics()
        {
            // Arrange
            var plateNumber = "ABC123";
            var now = DateTimeOffset.UtcNow;
            var thirtyDaysAgo = now.AddDays(-30);
            var oneHundredDaysAgo = now.AddDays(-100);
            
            await SeedPlateGroupsAsync(new[]
            {
                CreatePlateGroup(plateNumber, now.ToUnixTimeMilliseconds()),
                CreatePlateGroup(plateNumber, thirtyDaysAgo.ToUnixTimeMilliseconds()),
                CreatePlateGroup(plateNumber, oneHundredDaysAgo.ToUnixTimeMilliseconds())
            });

            var query = new GetStatisticsQuery(plateNumber);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.TotalSeen.Should().Be(3);
            result.Last90Days.Should().Be(2); // Only the recent two within 90 days
            result.FirstSeen.Should().BeCloseTo(oneHundredDaysAgo, TimeSpan.FromMinutes(1));
            result.LastSeen.Should().BeCloseTo(now, TimeSpan.FromMinutes(1));
        }

        [Test]
        public async Task Handle_WithPossibleNumberMatches_IncludesThemInStatistics()
        {
            // Arrange
            var plateNumber = "ABC123";
            var now = DateTimeOffset.UtcNow;
            var threeDaysAgo = now.AddDays(-3);
            
            // Create plate group with different best number but matching possible number
            var plateGroup = CreatePlateGroup("XYZ789", now.ToUnixTimeMilliseconds());
            var possibleNumber = new PlateGroupPossibleNumbers
            {
                PlateGroupId = plateGroup.Id,
                Number = plateNumber
            };
            plateGroup.PossibleNumbers = new List<PlateGroupPossibleNumbers> { possibleNumber };

            await SeedPlateGroupsAsync(new[] { plateGroup });

            var query = new GetStatisticsQuery(plateNumber);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.TotalSeen.Should().Be(1);
            result.Last90Days.Should().Be(1);
        }

        [Test]
        public async Task Handle_WithBothBestAndPossibleMatches_CombinesStatistics()
        {
            // Arrange
            var plateNumber = "ABC123";
            var now = DateTimeOffset.UtcNow;
            var tenDaysAgo = now.AddDays(-10);
            var twentyDaysAgo = now.AddDays(-20);
            var oneHundredDaysAgo = now.AddDays(-100);

            // Best number matches
            var bestMatchPlates = new[]
            {
                CreatePlateGroup(plateNumber, now.ToUnixTimeMilliseconds()),
                CreatePlateGroup(plateNumber, tenDaysAgo.ToUnixTimeMilliseconds()),
                CreatePlateGroup(plateNumber, oneHundredDaysAgo.ToUnixTimeMilliseconds())
            };

            // Possible number match
            var badPlateGroup = CreatePlateGroup("DEF456", tenDaysAgo.ToUnixTimeMilliseconds());

            var possibleNumber = new PlateGroupPossibleNumbers
            {
                PlateGroupId = badPlateGroup.Id,
                Number = plateNumber
            };

            badPlateGroup.PossibleNumbers = new List<PlateGroupPossibleNumbers> { possibleNumber };

            await SeedPlateGroupsAsync(new[] { bestMatchPlates[0], bestMatchPlates[1], bestMatchPlates[2], badPlateGroup });

            var query = new GetStatisticsQuery(plateNumber);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.TotalSeen.Should().Be(4); // 3 best matches + 1 possible match
            result.Last90Days.Should().Be(3); // Only the recent ones within 90 days
            result.FirstSeen.Should().BeCloseTo(oneHundredDaysAgo, TimeSpan.FromMinutes(1));
            result.LastSeen.Should().BeCloseTo(now, TimeSpan.FromMinutes(1));
        }

        [Test]
        public async Task Handle_WithOldPlatesOnly_ExcludesFromLast90Days()
        {
            // Arrange
            var plateNumber = "ABC123";
            var now = DateTimeOffset.UtcNow;
            var ninetyFiveDaysAgo = now.AddDays(-95);
            var oneHundredTwentyDaysAgo = now.AddDays(-120);
            
            await SeedPlateGroupsAsync(new[]
            {
                CreatePlateGroup(plateNumber, ninetyFiveDaysAgo.ToUnixTimeMilliseconds()),
                CreatePlateGroup(plateNumber, oneHundredTwentyDaysAgo.ToUnixTimeMilliseconds())
            });

            var query = new GetStatisticsQuery(plateNumber);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.TotalSeen.Should().Be(2);
            result.Last90Days.Should().Be(0); // None within last 90 days
            result.FirstSeen.Should().BeCloseTo(oneHundredTwentyDaysAgo, TimeSpan.FromMinutes(1));
            result.LastSeen.Should().BeCloseTo(ninetyFiveDaysAgo, TimeSpan.FromMinutes(1));
        }

        [Test]
        public async Task Handle_WithSinglePlate_ReturnsSameFirstAndLastSeen()
        {
            // Arrange
            var plateNumber = "ABC123";
            var now = DateTimeOffset.UtcNow;
            
            await SeedPlateGroupsAsync(new[]
            {
                CreatePlateGroup(plateNumber, now.ToUnixTimeMilliseconds())
            });

            var query = new GetStatisticsQuery(plateNumber);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.TotalSeen.Should().Be(1);
            result.Last90Days.Should().Be(1);
            result.FirstSeen.Should().BeCloseTo(now, TimeSpan.FromMinutes(1));
            result.LastSeen.Should().BeCloseTo(now, TimeSpan.FromMinutes(1));
        }

        [Test]
        public async Task Handle_WithDuplicateEpochs_CountsThemCorrectly()
        {
            // Arrange
            var plateNumber = "ABC123";
            var now = DateTimeOffset.UtcNow;
            var sameEpoch = now.ToUnixTimeMilliseconds();
            
            await SeedPlateGroupsAsync(new[]
            {
                CreatePlateGroup(plateNumber, sameEpoch),
                CreatePlateGroup(plateNumber, sameEpoch), // Same epoch
                CreatePlateGroup(plateNumber, sameEpoch)  // Same epoch
            });

            var query = new GetStatisticsQuery(plateNumber);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.TotalSeen.Should().Be(3); // All three should be counted
            result.Last90Days.Should().Be(3);
        }

        [Test]
        public async Task Handle_WithDifferentPlateNumbers_OnlyCountsMatchingPlate()
        {
            // Arrange
            var plateNumber = "ABC123";
            var now = DateTimeOffset.UtcNow;
            
            await SeedPlateGroupsAsync(new[]
            {
                CreatePlateGroup(plateNumber, now.ToUnixTimeMilliseconds()),
                CreatePlateGroup("XYZ789", now.ToUnixTimeMilliseconds()), // Different plate
                CreatePlateGroup("DEF456", now.ToUnixTimeMilliseconds())  // Different plate
            });

            var query = new GetStatisticsQuery(plateNumber);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.TotalSeen.Should().Be(1); // Only the matching plate
            result.Last90Days.Should().Be(1);
        }

        [Test]
        public async Task Handle_WithCancellationToken_RespectsToken()
        {
            // Arrange
            var query = new GetStatisticsQuery("ABC123");
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
        }

        [Test]
        public async Task Handle_SortsEpochsCorrectly()
        {
            // Arrange
            var plateNumber = "ABC123";
            var now = DateTimeOffset.UtcNow;
            var middleTime = now.AddDays(-10);
            var earliestTime = now.AddDays(-20);
            
            // Insert in random order to test sorting
            await SeedPlateGroupsAsync(new[]
            {
                CreatePlateGroup(plateNumber, middleTime.ToUnixTimeMilliseconds()),
                CreatePlateGroup(plateNumber, now.ToUnixTimeMilliseconds()),
                CreatePlateGroup(plateNumber, earliestTime.ToUnixTimeMilliseconds())
            });

            var query = new GetStatisticsQuery(plateNumber);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.FirstSeen.Should().BeCloseTo(earliestTime, TimeSpan.FromMinutes(1));
            result.LastSeen.Should().BeCloseTo(now, TimeSpan.FromMinutes(1));
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
                Confidence = 95.5,
                PossibleNumbers = new List<PlateGroupPossibleNumbers>()
            };
        }

        private async Task SeedPlateGroupsAsync(PlateGroup[] plateGroups)
        {
            Context.PlateGroups.AddRange(plateGroups);
            await Context.SaveChangesAsync();
        }

        private async Task SeedPossibleNumbersAsync(PlateGroupPossibleNumbers[] possibleNumbers)
        {
            Context.PlateGroupPossibleNumbers.AddRange(possibleNumbers);
            await Context.SaveChangesAsync();
        }
    }
} 