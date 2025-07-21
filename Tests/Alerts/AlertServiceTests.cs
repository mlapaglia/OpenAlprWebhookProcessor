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
        public void AddJob_NullRequest_HandlesGracefully()
        {
            // Act
            _sut.AddJob(null);

            // Assert
            Assert.That(_sut.GetPendingAlertsCount(), Is.EqualTo(1));

            // Verify we can retrieve the null without issues
            var alerts = _sut.GetConsumingAlerts(CancellationToken.None).Take(1).ToList();
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
        public void GetConsumingAlerts_PartialConsumption_LeavesRemainingItems()
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
            var consumedAlerts = _sut.GetConsumingAlerts(CancellationToken.None)
                .Take(2)
                .ToList();

            // Assert
            Assert.That(consumedAlerts.Count, Is.EqualTo(2));
            Assert.That(_sut.GetPendingAlertsCount(), Is.EqualTo(1));
        }

        [Test]
        public void GetConsumingAlerts_WithCancellation_StopsEnumeration()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            _sut.AddJob(new AlertUpdateRequest { PlateId = Guid.NewGuid() });
            _sut.AddJob(new AlertUpdateRequest { PlateId = Guid.NewGuid() });

            // Act
            var alerts = new List<AlertUpdateRequest>();
            var enumerable = _sut.GetConsumingAlerts(cts.Token);

            foreach (var alert in enumerable)
            {
                alerts.Add(alert);
                if (alerts.Count == 1)
                {
                    cts.Cancel();
                    break;
                }
            }

            // Assert
            Assert.That(alerts.Count, Is.EqualTo(1));
            Assert.That(_sut.GetPendingAlertsCount(), Is.EqualTo(1));
        }

        [Test]
        public async Task GetConsumingAlerts_ConcurrentAddAndConsume_HandlesCorrectly()
        {
            // Arrange
            var addCount = 100;
            var consumeCount = 0;
            var consumedIds = new ConcurrentBag<Guid>();

            // Act
            var consumeTask = Task.Run(() =>
            {
                foreach (var alert in _sut.GetConsumingAlerts(CancellationToken.None))
                {
                    consumedIds.Add(alert.PlateId);
                    Interlocked.Increment(ref consumeCount);
                    if (consumeCount >= addCount)
                        break;
                }
            });

            var addTask = Task.Run(() =>
            {
                for (int i = 0; i < addCount; i++)
                {
                    _sut.AddJob(new AlertUpdateRequest { PlateId = Guid.NewGuid() });
                    Task.Delay(1); // Small delay to simulate real-world scenario
                }
            });

            await Task.WhenAll(addTask, consumeTask);

            // Assert
            Assert.That(consumeCount, Is.EqualTo(addCount));
            Assert.That(consumedIds.Count, Is.EqualTo(addCount));
            Assert.That(_sut.GetPendingAlertsCount(), Is.EqualTo(0));
        }

        [Test]
        public void GetConsumingAlerts_EmptyQueue_BlocksUntilItemAdded()
        {
            // Arrange
            AlertUpdateRequest receivedAlert = null;
            var alertReceived = new ManualResetEventSlim(false);

            // Act
            var consumeTask = Task.Run(() =>
            {
                foreach (var alert in _sut.GetConsumingAlerts(CancellationToken.None))
                {
                    receivedAlert = alert;
                    alertReceived.Set();
                    break;
                }
            });

            // Give the consumer time to start blocking
            Thread.Sleep(100);

            var testAlert = new AlertUpdateRequest { PlateId = Guid.NewGuid() };
            _sut.AddJob(testAlert);

            // Assert
            Assert.That(alertReceived.Wait(TimeSpan.FromSeconds(5)), Is.True, "Alert was not received within timeout");
            Assert.That(receivedAlert, Is.Not.Null);
            Assert.That(receivedAlert.PlateId, Is.EqualTo(testAlert.PlateId));
        }

        [Test]
        public void GetPendingAlertsCount_AfterAddingAndConsuming_ReturnsCorrectCount()
        {
            // Arrange & Act
            _sut.AddJob(new AlertUpdateRequest { PlateId = Guid.NewGuid() });
            _sut.AddJob(new AlertUpdateRequest { PlateId = Guid.NewGuid() });
            _sut.AddJob(new AlertUpdateRequest { PlateId = Guid.NewGuid() });

            Assert.That(_sut.GetPendingAlertsCount(), Is.EqualTo(3));

            // Consume one
            _sut.GetConsumingAlerts(CancellationToken.None).Take(1).ToList();

            Assert.That(_sut.GetPendingAlertsCount(), Is.EqualTo(2));

            // Add two more
            _sut.AddJob(new AlertUpdateRequest { PlateId = Guid.NewGuid() });
            _sut.AddJob(new AlertUpdateRequest { PlateId = Guid.NewGuid() });

            Assert.That(_sut.GetPendingAlertsCount(), Is.EqualTo(4));

            // Consume all
            _sut.GetConsumingAlerts(CancellationToken.None).Take(4).ToList();

            Assert.That(_sut.GetPendingAlertsCount(), Is.EqualTo(0));
        }
    }
}