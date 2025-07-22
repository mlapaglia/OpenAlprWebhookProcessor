using NUnit.Framework;
using OpenAlprWebhookProcessor.WebhookProcessor;

namespace Tests.WebhookProcessor
{
    [TestFixture]
    public class ImageRetrieverServiceTests
    {
        private ImageRetrieverService _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new ImageRetrieverService();
        }

        [TearDown]
        public void TearDown()
        {
            _sut?.Dispose();
        }

        [Test]
        public void AddImageRetrievalJob_WithValidUuid_AddsToQueue()
        {
            // Arrange
            var uuid = "test-uuid-123";

            // Act
            _sut.AddImageRetrievalJob(uuid);

            // Assert
            Assert.That(_sut.GetImageRequestsCount(), Is.EqualTo(1));
        }

        [Test]
        public void AddImageRetrievalJob_WithNullOrWhitespace_DoesNotAdd()
        {
            // Act & Assert
            _sut.AddImageRetrievalJob(null);
            Assert.That(_sut.GetImageRequestsCount(), Is.EqualTo(0));

            _sut.AddImageRetrievalJob("");
            Assert.That(_sut.GetImageRequestsCount(), Is.EqualTo(0));

            _sut.AddImageRetrievalJob("   ");
            Assert.That(_sut.GetImageRequestsCount(), Is.EqualTo(0));
        }

        [Test]
        public void AddImageRetrievalJob_WithDuplicateUuid_OnlyAddsOnce()
        {
            // Arrange
            var uuid = "duplicate-uuid";

            // Act
            _sut.AddImageRetrievalJob(uuid);
            _sut.AddImageRetrievalJob(uuid);
            _sut.AddImageRetrievalJob(uuid);

            // Assert
            Assert.That(_sut.GetImageRequestsCount(), Is.EqualTo(1));
        }

        [Test]
        public async Task GetConsumingImageRequestsAsync_ReturnsAddedItems()
        {
            // Arrange
            var uuids = new[] { "uuid1", "uuid2", "uuid3" };
            foreach (var uuid in uuids)
            {
                _sut.AddImageRetrievalJob(uuid);
            }
            _sut.CompleteImageRequests();

            // Act
            var results = new List<string>();
            await foreach (var request in _sut.GetConsumingImageRequestsAsync(CancellationToken.None))
            {
                results.Add(request);
            }

            // Assert
            Assert.That(results.Count, Is.EqualTo(3));
            Assert.That(results, Is.EquivalentTo(uuids));
        }

        [Test]
        public async Task GetConsumingImageRequestsAsync_UpdatesCount()
        {
            // Arrange
            _sut.AddImageRetrievalJob("uuid1");
            _sut.AddImageRetrievalJob("uuid2");
            Assert.That(_sut.GetImageRequestsCount(), Is.EqualTo(2));

            // Act - consume one item
            await foreach (var request in _sut.GetConsumingImageRequestsAsync(CancellationToken.None))
            {
                break; // Take only one
            }

            // Assert
            Assert.That(_sut.GetImageRequestsCount(), Is.EqualTo(1));
        }

        [Test]
        public void RemoveImageRequest_RemovesFromHashSet()
        {
            // Arrange
            var uuid = "test-uuid";
            _sut.AddImageRetrievalJob(uuid);

            // Act
            _sut.RemoveImageRequest(uuid);

            // Assert - should still be in channel but not in hashset
            // So duplicate adds should now work
            _sut.AddImageRetrievalJob(uuid);
            Assert.That(_sut.GetImageRequestsCount(), Is.EqualTo(2));
        }

        [Test]
        public void AddImageCompressionJob_WithParameter_AddsToQueue()
        {
            // Arrange
            var parameter = "test-param";

            // Act
            _sut.AddImageCompressionJob(parameter);

            // Assert
            Assert.That(_sut.GetCompressionRequestsCount(), Is.EqualTo(1));
        }

        [Test]
        public void AddImageCompressionJob_WithNullOrWhitespace_DoesNotAdd()
        {
            // Act & Assert
            _sut.AddImageCompressionJob(null);
            Assert.That(_sut.GetCompressionRequestsCount(), Is.EqualTo(0));

            _sut.AddImageCompressionJob("");
            Assert.That(_sut.GetCompressionRequestsCount(), Is.EqualTo(0));

            _sut.AddImageCompressionJob("   ");
            Assert.That(_sut.GetCompressionRequestsCount(), Is.EqualTo(0));
        }

        [Test]
        public void AddImageCompressionJob_WithDuplicateParameter_OnlyAddsOnce()
        {
            // Arrange
            var parameter = "duplicate-param";

            // Act
            _sut.AddImageCompressionJob(parameter);
            _sut.AddImageCompressionJob(parameter);
            _sut.AddImageCompressionJob(parameter);

            // Assert
            Assert.That(_sut.GetCompressionRequestsCount(), Is.EqualTo(1));
        }

        [Test]
        public void AddImageCompressionJob_WithoutParameter_AddsAllImages()
        {
            // Act
            _sut.AddImageCompressionJob();
            _sut.AddImageCompressionJob();

            // Assert
            Assert.That(_sut.GetCompressionRequestsCount(), Is.EqualTo(2)); // Should add multiple "allImages"
        }

        [Test]
        public async Task GetConsumingCompressionRequestsAsync_ReturnsAddedItems()
        {
            // Arrange
            _sut.AddImageCompressionJob("param1");
            _sut.AddImageCompressionJob("param2");
            _sut.AddImageCompressionJob(); // Adds "allImages"
            _sut.CompleteCompressionRequests();

            // Act
            var results = new List<string>();
            await foreach (var request in _sut.GetConsumingCompressionRequestsAsync(CancellationToken.None))
            {
                results.Add(request);
            }

            // Assert
            Assert.That(results.Count, Is.EqualTo(3));
            Assert.That(results, Contains.Item("param1"));
            Assert.That(results, Contains.Item("param2"));
            Assert.That(results, Contains.Item("allImages"));
        }

        [Test]
        public async Task GetConsumingCompressionRequestsAsync_UpdatesCount()
        {
            // Arrange
            _sut.AddImageCompressionJob("param1");
            _sut.AddImageCompressionJob();
            Assert.That(_sut.GetCompressionRequestsCount(), Is.EqualTo(2));

            // Act - consume one item
            await foreach (var request in _sut.GetConsumingCompressionRequestsAsync(CancellationToken.None))
            {
                break; // Take only one
            }

            // Assert
            Assert.That(_sut.GetCompressionRequestsCount(), Is.EqualTo(1));
        }

        [Test]
        public async Task ConcurrentAddAndConsume_ImageRequests_HandlesCorrectly()
        {
            // Arrange
            var addCount = 50;
            var consumedItems = new List<string>();
            var consumeCount = 0;

            // Act
            var consumeTask = Task.Run(async () =>
            {
                await foreach (var request in _sut.GetConsumingImageRequestsAsync(CancellationToken.None))
                {
                    consumedItems.Add(request);
                    var count = Interlocked.Increment(ref consumeCount);
                    if (count >= addCount)
                        break;
                }
            });

            var addTask = Task.Run(() =>
            {
                for (int i = 0; i < addCount; i++)
                {
                    _sut.AddImageRetrievalJob($"uuid-{i}");
                    Thread.Sleep(1); // Small delay
                }
            });

            await Task.WhenAll(addTask, consumeTask);

            // Assert
            Assert.That(consumeCount, Is.EqualTo(addCount));
            Assert.That(consumedItems.Count, Is.EqualTo(addCount));
            Assert.That(consumedItems.Distinct().Count(), Is.EqualTo(addCount)); // All unique
        }

        [Test]
        public async Task ConcurrentAddAndConsume_CompressionRequests_HandlesCorrectly()
        {
            // Arrange
            var addCount = 50;
            var consumedItems = new List<string>();
            var consumeCount = 0;

            // Act
            var consumeTask = Task.Run(async () =>
            {
                await foreach (var request in _sut.GetConsumingCompressionRequestsAsync(CancellationToken.None))
                {
                    consumedItems.Add(request);
                    var count = Interlocked.Increment(ref consumeCount);
                    if (count >= addCount)
                        break;
                }
            });

            var addTask = Task.Run(() =>
            {
                for (int i = 0; i < addCount; i++)
                {
                    _sut.AddImageCompressionJob($"param-{i}");
                    Thread.Sleep(1); // Small delay
                }
            });

            await Task.WhenAll(addTask, consumeTask);

            // Assert
            Assert.That(consumeCount, Is.EqualTo(addCount));
            Assert.That(consumedItems.Count, Is.EqualTo(addCount));
        }

        [Test]
        public void AddImageRetrievalJob_AfterDispose_DoesNotThrow()
        {
            // Arrange
            _sut.Dispose();

            // Act & Assert
            Assert.DoesNotThrow(() => _sut.AddImageRetrievalJob("uuid"));
            Assert.That(_sut.GetImageRequestsCount(), Is.EqualTo(0));
        }

        [Test]
        public void AddImageCompressionJob_AfterDispose_DoesNotThrow()
        {
            // Arrange
            _sut.Dispose();

            // Act & Assert
            Assert.DoesNotThrow(() => _sut.AddImageCompressionJob("param"));
            Assert.DoesNotThrow(() => _sut.AddImageCompressionJob());
            Assert.That(_sut.GetCompressionRequestsCount(), Is.EqualTo(0));
        }

        [Test]
        public async Task GetConsumingRequests_AfterComplete_ExitsEnumeration()
        {
            // Arrange
            _sut.AddImageRetrievalJob("uuid1");
            _sut.AddImageCompressionJob("param1");

            // Act
            _sut.CompleteImageRequests();
            _sut.CompleteCompressionRequests();

            var imageResults = new List<string>();
            await foreach (var request in _sut.GetConsumingImageRequestsAsync(CancellationToken.None))
            {
                imageResults.Add(request);
            }

            var compressionResults = new List<string>();
            await foreach (var request in _sut.GetConsumingCompressionRequestsAsync(CancellationToken.None))
            {
                compressionResults.Add(request);
            }

            // Assert
            Assert.That(imageResults.Count, Is.EqualTo(1));
            Assert.That(compressionResults.Count, Is.EqualTo(1));
        }


        [Test]
        public async Task EmptyQueue_BlocksUntilItemAdded()
        {
            // Arrange
            var itemReceived = new TaskCompletionSource<string>();

            // Act
            var consumeTask = Task.Run(async () =>
            {
                await foreach (var request in _sut.GetConsumingImageRequestsAsync(CancellationToken.None))
                {
                    itemReceived.SetResult(request);
                    break;
                }
            });

            // Give consumer time to start waiting
            await Task.Delay(100);

            var testUuid = "test-uuid";
            _sut.AddImageRetrievalJob(testUuid);

            // Assert
            var result = await itemReceived.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.That(result, Is.EqualTo(testUuid));
        }

        [Test]
        public void MultipleDispose_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                _sut.Dispose();
                _sut.Dispose();
                _sut.Dispose();
            });
        }
    }
}