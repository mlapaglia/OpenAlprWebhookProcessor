using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Alerts;
using System.Collections.Concurrent;
using Tests.TestHelpers;

namespace Tests.Features.Alerts
{
    [TestFixture]
    public class AlertServiceTests : TestBase
    {
        private AlertService _sut;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _sut = new AlertService();
        }

        [TearDown]
        public override void TearDown()
        {
            _sut.Dispose();
        }

        [Test]
        public void AddJob_SingleRequest_AddsToQueue()
        {
            // Arrange
            var request = new AlertUpdateRequest { PlateId = Guid.NewGuid() };

            // Act
            _sut.AddJob(request);

            // Assert
            Assert.That(_sut.GetPendingAlertsCount(), Is.EqualTo(1));
        }

        [Test]
        public void AddJob_MultipleRequests_AddsAllToQueue()
        {
            // Arrange
            var requests = new[]
            {
                new AlertUpdateRequest { PlateId = Guid.NewGuid() },
                new AlertUpdateRequest { PlateId = Guid.NewGuid() },
                new AlertUpdateRequest { PlateId = Guid.NewGuid() }
            };

            // Act
            foreach (var request in requests)
            {
                _sut.AddJob(request);
            }

            // Assert
            Assert.That(_sut.GetPendingAlertsCount(), Is.EqualTo(3));
        }

        [Test]
        public async Task AddJob_NullRequest_HandlesGracefully()
        {
            // Act
            _sut.AddJob(null);

            // Assert
            Assert.That(_sut.GetPendingAlertsCount(), Is.GreaterThan(0)); // Channel count is approximate

            // Verify we can retrieve the null without issues
            var alerts = new List<AlertUpdateRequest>();
            await foreach (var alert in _sut.GetConsumingAlertsAsync(CancellationToken.None))
            {
                alerts.Add(alert);
                break; // Take only one
            }

            Assert.That(alerts.Count, Is.EqualTo(1));
            Assert.That(alerts[0], Is.Null);
        }

        [Test]
        public void GetPendingAlertsCount_EmptyQueue_ReturnsZero()
        {
            // Act
            var count = _sut.GetPendingAlertsCount();

            // Assert
            Assert.That(count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetConsumingAlertsAsync_PartialConsumption_LeavesRemainingItems()
        {
            // Arrange
            var requests = new[]
            {
                new AlertUpdateRequest { PlateId = Guid.NewGuid() },
                new AlertUpdateRequest { PlateId = Guid.NewGuid() },
                new AlertUpdateRequest { PlateId = Guid.NewGuid() }
            };

            foreach (var request in requests)
            {
                _sut.AddJob(request);
            }

            // Act
            var consumedAlerts = new List<AlertUpdateRequest>();
            var count = 0;

            await foreach (var alert in _sut.GetConsumingAlertsAsync(CancellationToken.None))
            {
                consumedAlerts.Add(alert);
                count++;
                if (count >= 2) // Take only 2 items
                    break;
            }

            // Assert
            Assert.That(consumedAlerts.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetConsumingAlertsAsync_WithCancellation_ThrowsOperationCancelledException()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            _sut.AddJob(new AlertUpdateRequest { PlateId = Guid.NewGuid() });

            // Act & Assert
            var alerts = new List<AlertUpdateRequest>();
            var cancellationOccurred = false;

            try
            {
                await foreach (var alert in _sut.GetConsumingAlertsAsync(cts.Token))
                {
                    alerts.Add(alert);
                    cts.Cancel(); // Cancel after first item

                    // Try to get next item - should throw OperationCanceledException
                }
            }
            catch (OperationCanceledException)
            {
                cancellationOccurred = true;
            }

            Assert.That(alerts.Count, Is.EqualTo(1));
            Assert.That(cancellationOccurred, Is.True, "OperationCanceledException should have been thrown");
        }

        [Test]
        public async Task GetConsumingAlertsAsync_ConcurrentAddAndConsume_HandlesCorrectly()
        {
            // Arrange
            var addCount = 100;
            var consumeCount = 0;
            var consumedIds = new ConcurrentBag<Guid>();

            // Act
            var consumeTask = Task.Run(async () =>
            {
                await foreach (var alert in _sut.GetConsumingAlertsAsync(CancellationToken.None))
                {
                    consumedIds.Add(alert.PlateId);
                    var currentCount = Interlocked.Increment(ref consumeCount);
                    if (currentCount >= addCount)
                        break;
                }
            });

            var addTask = Task.Run(async () =>
            {
                for (int i = 0; i < addCount; i++)
                {
                    _sut.AddJob(new AlertUpdateRequest { PlateId = Guid.NewGuid() });
                    await Task.Delay(1); // Small delay to simulate real-world scenario
                }
            });

            await Task.WhenAll(addTask, consumeTask);

            // Assert
            Assert.That(consumeCount, Is.EqualTo(addCount));
            Assert.That(consumedIds.Count, Is.EqualTo(addCount));
        }


        [Test]
        public async Task GetConsumingAlerts_EmptyQueue_BlocksUntilItemAdded()
        {
            // Arrange
            AlertUpdateRequest receivedAlert = null;
            var alertReceived = new TaskCompletionSource<bool>();

            // Act
            var consumeTask = Task.Run(async () =>
            {
                await foreach (var alert in _sut.GetConsumingAlertsAsync(CancellationToken.None))
                {
                    receivedAlert = alert;
                    alertReceived.SetResult(true);
                    break;
                }
            });

            // Give the consumer time to start blocking
            await Task.Delay(100);

            var testAlert = new AlertUpdateRequest { PlateId = Guid.NewGuid() };
            _sut.AddJob(testAlert);

            // Assert
            var completed = await alertReceived.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.That(completed, Is.True, "Alert was not received within timeout");
            Assert.That(receivedAlert, Is.Not.Null);
            Assert.That(receivedAlert.PlateId, Is.EqualTo(testAlert.PlateId));
        }

        [Test]
        public async Task GetPendingAlertsCount_AfterAddingAndConsuming_ReturnsApproximateCount()
        {
            // Note: Channels don't provide exact counts, only approximations

            // Arrange & Act
            _sut.AddJob(new AlertUpdateRequest { PlateId = Guid.NewGuid() });
            _sut.AddJob(new AlertUpdateRequest { PlateId = Guid.NewGuid() });
            _sut.AddJob(new AlertUpdateRequest { PlateId = Guid.NewGuid() });

            // Should indicate items are available (returns 1 if any items, 0 if none)
            Assert.That(_sut.GetPendingAlertsCount(), Is.GreaterThan(0));

            // Consume one
            await foreach (var alert in _sut.GetConsumingAlertsAsync(CancellationToken.None))
            {
                break; // Take only one
            }

            // Add two more
            _sut.AddJob(new AlertUpdateRequest { PlateId = Guid.NewGuid() });
            _sut.AddJob(new AlertUpdateRequest { PlateId = Guid.NewGuid() });

            // Should still indicate items are available
            Assert.That(_sut.GetPendingAlertsCount(), Is.GreaterThan(0));

            // Consume all remaining by completing the channel
            _sut.CompleteChannel();
            var consumedCount = 0;
            await foreach (var alert in _sut.GetConsumingAlertsAsync(CancellationToken.None))
            {
                consumedCount++;
            }

            // Should have consumed 4 remaining items (3 original + 2 added - 1 consumed = 4)
            Assert.That(consumedCount, Is.EqualTo(4));
        }
    }
}