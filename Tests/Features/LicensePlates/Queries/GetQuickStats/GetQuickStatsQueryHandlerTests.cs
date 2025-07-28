using AwesomeAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetQuickStats;
using Tests.TestHelpers;

namespace Tests.Features.LicensePlates.Queries.GetQuickStats
{
    [TestFixture]
    public class GetQuickStatsQueryHandlerTests : TestBase
    {
        private GetQuickStatsQueryHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new GetQuickStatsQueryHandler(UnitOfWork);
        }

        [Test]
        public async Task Handle_WithNoPlateGroups_ReturnsZeroStats()
        {
            // Arrange
            var query = new GetQuickStatsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.TodayCount.Should().Be(0);
            result.WeekCount.Should().Be(0);
            result.MonthCount.Should().Be(0);
            result.UniquePlatesThisWeek.Should().Be(0);
            result.ActiveCameras.Should().Be(0);
            result.AverageDailyPlates.Should().Be(0);
        }

        [Test]
        public async Task Handle_WithTodaysPlates_ReturnsTodayCount()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;
            var todayEpoch = new DateTimeOffset(today, TimeSpan.Zero).ToUnixTimeMilliseconds();
            
            await SeedPlateGroupsWithEpochAsync(new[]
            {
                todayEpoch + 3600000, // 1 hour into today
                todayEpoch + 7200000, // 2 hours into today
            });

            var query = new GetQuickStatsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.TodayCount.Should().Be(2);
            result.WeekCount.Should().Be(2);
            result.MonthCount.Should().Be(2);
        }

        [Test]
        public async Task Handle_WithWeekOldPlates_ReturnsWeekAndMonthCount()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var threeDaysAgoEpoch = now.AddDays(-3).ToUnixTimeMilliseconds();
            var sixDaysAgoEpoch = now.AddDays(-6).ToUnixTimeMilliseconds();
            var tenDaysAgoEpoch = now.AddDays(-10).ToUnixTimeMilliseconds();
            
            await SeedPlateGroupsWithEpochAsync(new[]
            {
                threeDaysAgoEpoch,
                sixDaysAgoEpoch,
                tenDaysAgoEpoch
            });

            var query = new GetQuickStatsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.TodayCount.Should().Be(0);
            result.WeekCount.Should().Be(2); // Only plates from 3 and 6 days ago
            result.MonthCount.Should().Be(3); // All plates within 30 days
        }

        [Test]
        public async Task Handle_WithUniqueAndDuplicatePlates_ReturnsCorrectUniqueCount()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var threeDaysAgoEpoch = now.AddDays(-3).ToUnixTimeMilliseconds();
            
            await SeedUniqueAndDuplicatePlatesAsync(threeDaysAgoEpoch);

            var query = new GetQuickStatsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.WeekCount.Should().Be(4); // Total plates
            result.UniquePlatesThisWeek.Should().Be(2); // Only 2 unique plate numbers
        }

        [Test]
        public async Task Handle_WithMultipleCameras_ReturnsActiveCameraCount()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var threeDaysAgoEpoch = now.AddDays(-3).ToUnixTimeMilliseconds();
            
            await SeedPlatesFromMultipleCamerasAsync(threeDaysAgoEpoch);

            var query = new GetQuickStatsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ActiveCameras.Should().Be(3); // Cameras 1, 2, and 3
        }

        [Test]
        public async Task Handle_WithZeroCameraId_ExcludesFromActiveCameraCount()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var threeDaysAgoEpoch = now.AddDays(-3).ToUnixTimeMilliseconds();
            
            var plateGroups = new[]
            {
                CreatePlateGroup("ABC123", threeDaysAgoEpoch, 1),
                CreatePlateGroup("DEF456", threeDaysAgoEpoch, 0), // Should be excluded
                CreatePlateGroup("GHI789", threeDaysAgoEpoch, 2)
            };

            await SeedPlateGroupsAsync(plateGroups);

            var query = new GetQuickStatsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ActiveCameras.Should().Be(2); // Only cameras 1 and 2
        }

        [Test]
        public async Task Handle_CalculatesAverageDailyPlatesCorrectly()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var epochs = new long[60]; // 60 plates over 30 days = average of 2 per day
            
            for (int i = 0; i < 60; i++)
            {
                epochs[i] = now.AddDays(-i % 30).ToUnixTimeMilliseconds();
            }
            
            await SeedPlateGroupsWithEpochAsync(epochs);

            var query = new GetQuickStatsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.MonthCount.Should().Be(60);
            result.AverageDailyPlates.Should().Be(2); // 60 plates / 30 days = 2
        }

        [Test]
        public async Task Handle_WithOldPlates_ExcludesFromCounts()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var fortyDaysAgoEpoch = now.AddDays(-40).ToUnixTimeMilliseconds();
            var fiftyDaysAgoEpoch = now.AddDays(-50).ToUnixTimeMilliseconds();
            
            await SeedPlateGroupsWithEpochAsync(new[]
            {
                fortyDaysAgoEpoch,
                fiftyDaysAgoEpoch
            });

            var query = new GetQuickStatsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.TodayCount.Should().Be(0);
            result.WeekCount.Should().Be(0);
            result.MonthCount.Should().Be(0);
            result.UniquePlatesThisWeek.Should().Be(0);
            result.ActiveCameras.Should().Be(0);
            result.AverageDailyPlates.Should().Be(0);
        }

        [Test]
        public async Task Handle_WithCancellationToken_RespectsToken()
        {
            // Arrange
            var query = new GetQuickStatsQuery();
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
        }

        private async Task SeedPlateGroupsWithEpochAsync(long[] epochs)
        {
            var plateGroups = new PlateGroup[epochs.Length];
            
            for (int i = 0; i < epochs.Length; i++)
            {
                plateGroups[i] = CreatePlateGroup($"PLATE{i:D3}", epochs[i], 1);
            }

            await SeedPlateGroupsAsync(plateGroups);
        }

        private async Task SeedUniqueAndDuplicatePlatesAsync(long epoch)
        {
            var plateGroups = new[]
            {
                CreatePlateGroup("ABC123", epoch, 1),
                CreatePlateGroup("ABC123", epoch + 1000, 1), // Duplicate plate
                CreatePlateGroup("XYZ789", epoch + 2000, 2),
                CreatePlateGroup("XYZ789", epoch + 3000, 2) // Duplicate plate
            };

            await SeedPlateGroupsAsync(plateGroups);
        }

        private async Task SeedPlatesFromMultipleCamerasAsync(long epoch)
        {
            var plateGroups = new[]
            {
                CreatePlateGroup("ABC123", epoch, 1),
                CreatePlateGroup("DEF456", epoch + 1000, 2),
                CreatePlateGroup("GHI789", epoch + 2000, 3),
                CreatePlateGroup("JKL012", epoch + 3000, 1) // Same camera as first
            };

            await SeedPlateGroupsAsync(plateGroups);
        }

        private PlateGroup CreatePlateGroup(string plateNumber, long epoch, int cameraId)
        {
            return new PlateGroup
            {
                BestNumber = plateNumber,
                ReceivedOnEpoch = epoch,
                OpenAlprCameraId = cameraId,
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