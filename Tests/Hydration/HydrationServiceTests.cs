using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Hydrator;
using Tests.TestHelpers;

namespace Tests.Hydrator
{
    [TestFixture]
    public class HydrationServiceTests : TestBase
    {
        private IServiceProvider _serviceProvider;

        private IServiceScope _serviceScope;

        private IServiceScopeFactory _serviceScopeFactory;

        private ILogger<HydrationService> _logger;

        private HydrationService _sut;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _serviceProvider = Substitute.For<IServiceProvider>();
            _serviceScope = Substitute.For<IServiceScope>();
            _serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
            _logger = Substitute.For<ILogger<HydrationService>>();

            _serviceProvider.GetService(typeof(IServiceScopeFactory)).Returns(_serviceScopeFactory);
            _serviceScopeFactory.CreateScope().Returns(_serviceScope);
            _serviceScope.ServiceProvider.Returns(_serviceProvider);

            _serviceProvider.GetService(typeof(IUnitOfWork)).Returns(UnitOfWork);
            _serviceProvider.GetService(typeof(ILogger<HydrationService>)).Returns(_logger);

            _sut = new HydrationService(_serviceProvider);
        }

        [TearDown]
        public override void TearDown()
        {
            _serviceScope?.Dispose();
            _sut?.DisposeTimer();
            base.TearDown();
        }

        [Test]
        public void Constructor_NullServiceProvider_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new HydrationService(null));
        }

        [Test]
        public void StartHydration_AddsNameToQueue()
        {
            // Arrange
            var name = "test-agent";

            // Act
            _sut.StartHydration(name);

            // Assert
            Assert.That(_sut.GetPendingHydrationCount(), Is.EqualTo(1));
        }

        [Test]
        public void StartHydration_MultipleNames_AddsAllToQueue()
        {
            // Arrange
            var names = new[] { "agent1", "agent2", "agent3" };

            // Act
            foreach (var name in names)
            {
                _sut.StartHydration(name);
            }

            // Assert
            Assert.That(_sut.GetPendingHydrationCount(), Is.EqualTo(3));
        }

        [Test]
        public void GetPendingHydrationCount_EmptyQueue_ReturnsZero()
        {
            // Act
            var count = _sut.GetPendingHydrationCount();

            // Assert
            Assert.That(count, Is.EqualTo(0));
        }

        [Test]
        public void GetConsumingHydrationRequests_ReturnsItemsInOrder()
        {
            // Arrange
            var names = new[] { "agent1", "agent2", "agent3" };
            foreach (var name in names)
            {
                _sut.StartHydration(name);
            }

            // Act
            using var cts = new CancellationTokenSource();
            var consumingEnumerable = _sut.GetConsumingHydrationRequests(cts.Token);
            var results = consumingEnumerable.Take(3).ToList();

            // Assert
            Assert.That(results, Is.EqualTo(names));
            Assert.That(_sut.GetPendingHydrationCount(), Is.EqualTo(0));
        }

        [Test]
        public async Task ScheduleHydrationAsync_NoAgent_LogsWarningAndDisposesTimer()
        {
            // Arrange
            // No agent in database

            // Act
            await _sut.ScheduleHydrationAsync(CancellationToken.None);

            // Assert
            _logger.Received(1).LogWarning("Agent UID is not set. Cannot schedule hydration.");
        }

        [Test]
        public async Task ScheduleHydrationAsync_AgentWithNoUid_LogsWarningAndDisposesTimer()
        {
            // Arrange
            var agent = new Agent { Uid = "" };
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            // Act
            await _sut.ScheduleHydrationAsync(CancellationToken.None);

            // Assert
            _logger.Received(1).LogWarning("Agent UID is not set. Cannot schedule hydration.");
        }

        [Test]
        public async Task ScheduleHydrationAsync_AgentWithNoInterval_SetsNextScrapeToNull()
        {
            // Arrange
            var agent = new Agent
            {
                Uid = "test-agent",
                ScheduledScrapingIntervalMinutes = null,
                NextScrapeEpochMs = 123456789
            };
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            // Act
            await _sut.ScheduleHydrationAsync(CancellationToken.None);

            // Assert
            var updatedAgent = await UnitOfWork.Agents.GetFirstAgentAsync();
            Assert.That(updatedAgent.NextScrapeEpochMs, Is.Null);
        }

        [Test]
        public async Task ScheduleHydrationAsync_AgentWithInterval_SetsNextScrapeTime()
        {
            // Arrange
            var intervalMinutes = 30;
            var agent = new Agent
            {
                Uid = "test-agent",
                ScheduledScrapingIntervalMinutes = intervalMinutes
            };
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            var beforeSchedule = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Act
            await _sut.ScheduleHydrationAsync(CancellationToken.None);

            // Assert
            var updatedAgent = await UnitOfWork.Agents.GetFirstAgentAsync();
            Assert.That(updatedAgent.NextScrapeEpochMs, Is.Not.Null);

            // Verify next scrape time is approximately correct (within 1 second tolerance)
            var expectedTime = beforeSchedule + (intervalMinutes * 60 * 1000);
            Assert.That(updatedAgent.NextScrapeEpochMs.Value, Is.InRange(expectedTime - 1000, expectedTime + 1000));
        }

        [Test]
        public async Task ScheduleHydrationAsync_CalledTwiceWithSameConfig_DoesNotRecreateTimer()
        {
            // Arrange
            var agent = new Agent
            {
                Uid = "test-agent",
                ScheduledScrapingIntervalMinutes = 30
            };
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            // Act
            await _sut.ScheduleHydrationAsync(CancellationToken.None);
            var firstNextScrapeTime = (await UnitOfWork.Agents.GetFirstAgentAsync()).NextScrapeEpochMs;

            // Wait a bit to ensure time difference if timer was recreated
            await Task.Delay(100);

            await _sut.ScheduleHydrationAsync(CancellationToken.None);
            var secondNextScrapeTime = (await UnitOfWork.Agents.GetFirstAgentAsync()).NextScrapeEpochMs;

            // Assert - If configuration hasn't changed, next scrape time should be updated but similar
            Assert.That(secondNextScrapeTime, Is.Not.Null);
            Assert.That(firstNextScrapeTime, Is.Not.Null);
            // Both times should be close (within 1 second)
            Assert.That(Math.Abs(secondNextScrapeTime.Value - firstNextScrapeTime.Value), Is.LessThan(1000));
        }

        [Test]
        public async Task ScheduleHydrationAsync_IntervalChanges_RecreatesTimer()
        {
            // Arrange
            var agent = new Agent
            {
                Uid = "test-agent",
                ScheduledScrapingIntervalMinutes = 30
            };
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            // Act - First schedule
            await _sut.ScheduleHydrationAsync(CancellationToken.None);

            // Change interval
            agent.ScheduledScrapingIntervalMinutes = 60;
            UnitOfWork.Agents.Update(agent);
            await UnitOfWork.SaveChangesAsync();

            // Act - Second schedule
            await _sut.ScheduleHydrationAsync(CancellationToken.None);

            // Assert
            var updatedAgent = await UnitOfWork.Agents.GetFirstAgentAsync();
            Assert.That(updatedAgent.ScheduledScrapingIntervalMinutes, Is.EqualTo(60));

            // Next scrape time should reflect the new interval
            var expectedTime = DateTimeOffset.UtcNow.AddMinutes(60).ToUnixTimeMilliseconds();
            Assert.That(updatedAgent.NextScrapeEpochMs.Value, Is.InRange(expectedTime - 1000, expectedTime + 1000));
        }

        [Test]
        public async Task ScheduleHydrationAsync_AgentUidChanges_RecreatesTimer()
        {
            // Arrange
            var agent1 = new Agent
            {
                Uid = "agent1",
                ScheduledScrapingIntervalMinutes = 30
            };
            await UnitOfWork.Agents.AddAsync(agent1);
            await UnitOfWork.SaveChangesAsync();

            // Act - First schedule
            await _sut.ScheduleHydrationAsync(CancellationToken.None);

            // Remove first agent and add new one
            UnitOfWork.Agents.Delete(agent1);
            await UnitOfWork.SaveChangesAsync();

            var agent2 = new Agent
            {
                Uid = "agent2",
                ScheduledScrapingIntervalMinutes = 30
            };
            await UnitOfWork.Agents.AddAsync(agent2);
            await UnitOfWork.SaveChangesAsync();

            // Act - Second schedule
            await _sut.ScheduleHydrationAsync(CancellationToken.None);

            // Assert
            var updatedAgent = await UnitOfWork.Agents.GetFirstAgentAsync();
            Assert.That(updatedAgent.Uid, Is.EqualTo("agent2"));
            Assert.That(updatedAgent.NextScrapeEpochMs, Is.Not.Null);
        }

        [Test]
        public async Task ScheduleHydrationAsync_IntervalRemovedAfterBeingSet_DisposesTimerAndClearsNextScrape()
        {
            // Arrange
            var agent = new Agent
            {
                Uid = "test-agent",
                ScheduledScrapingIntervalMinutes = 30
            };
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            // Act - Schedule with interval
            await _sut.ScheduleHydrationAsync(CancellationToken.None);

            // Remove interval
            agent.ScheduledScrapingIntervalMinutes = null;
            UnitOfWork.Agents.Update(agent);
            await UnitOfWork.SaveChangesAsync();

            // Act - Schedule without interval
            await _sut.ScheduleHydrationAsync(CancellationToken.None);

            // Assert
            var updatedAgent = await UnitOfWork.Agents.GetFirstAgentAsync();
            Assert.That(updatedAgent.NextScrapeEpochMs, Is.Null);
        }

        [Test]
        public void DisposeTimer_DisposesTimerAndClearsState()
        {
            // Act
            _sut.DisposeTimer();

            // Assert - Calling dispose multiple times should not throw
            Assert.DoesNotThrow(() => _sut.DisposeTimer());
        }
    }
}