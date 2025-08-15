using AwesomeAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor;
using Tests.TestHelpers;

namespace Tests.WebhookProcessor
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class ImageRetrieverServiceTests : TestBase
    {
        private ImageRetrieverService _service;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _service = new ImageRetrieverService();
        }

        [TearDown]
        public override void TearDown()
        {
            _service?.Dispose();
            base.TearDown();
        }

        [Test]
        public void Constructor_InitializesService_Successfully()
        {
            // Arrange & Act
            using var service = new ImageRetrieverService();

            // Assert
            service.Should().NotBeNull();
            service.GetImageRequestsCount().Should().Be(0);
            service.GetCompressionRequestsCount().Should().Be(0);
        }

        [Test]
        public void AddImageRetrievalJob_WithValidUuid_AddsJobToQueue()
        {
            // Arrange
            var uuid = "test-uuid-123";

            // Act
            _service.AddImageRetrievalJob(uuid);

            // Assert
            _service.GetImageRequestsCount().Should().Be(1);
        }

        [Test]
        public void AddImageRetrievalJob_WithDuplicateUuid_DoesNotAddDuplicate()
        {
            // Arrange
            var uuid = "test-uuid-123";

            // Act
            _service.AddImageRetrievalJob(uuid);
            _service.AddImageRetrievalJob(uuid); // Add same UUID again

            // Assert
            _service.GetImageRequestsCount().Should().Be(1);
        }

        [Test]
        public void AddImageRetrievalJob_WithNullUuid_DoesNotAddJob()
        {
            // Act
            _service.AddImageRetrievalJob(null);

            // Assert
            _service.GetImageRequestsCount().Should().Be(0);
        }

        [Test]
        public void AddImageRetrievalJob_WithEmptyUuid_DoesNotAddJob()
        {
            // Act
            _service.AddImageRetrievalJob(string.Empty);

            // Assert
            _service.GetImageRequestsCount().Should().Be(0);
        }

        [Test]
        public void AddImageRetrievalJob_WithWhitespaceUuid_DoesNotAddJob()
        {
            // Act
            _service.AddImageRetrievalJob("   ");

            // Assert
            _service.GetImageRequestsCount().Should().Be(0);
        }

        [Test]
        public void AddImageRetrievalJob_AfterDisposed_DoesNotAddJob()
        {
            // Arrange
            _service.Dispose();

            // Act
            _service.AddImageRetrievalJob("test-uuid");

            // Assert
            _service.GetImageRequestsCount().Should().Be(0);
        }

        [Test]
        public void RemoveImageRequest_WithExistingUuid_RemovesFromTracking()
        {
            // Arrange
            var uuid = "test-uuid-123";
            _service.AddImageRetrievalJob(uuid);

            // Act
            _service.RemoveImageRequest(uuid);

            // Assert - Count may still be 1 since item is in channel, but it's removed from tracking
            // This is verified by being able to add the same UUID again
            _service.AddImageRetrievalJob(uuid);
            _service.GetImageRequestsCount().Should().Be(2); // Original + re-added
        }

        [Test]
        public void RemoveImageRequest_WithNonExistentUuid_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _service.RemoveImageRequest("non-existent"));
        }

        [Test]
        public void AddImageCompressionJob_WithParameter_AddsJobToQueue()
        {
            // Arrange
            var parameter = "test-parameter";

            // Act
            _service.AddImageCompressionJob(parameter);

            // Assert
            _service.GetCompressionRequestsCount().Should().Be(1);
        }

        [Test]
        public void AddImageCompressionJob_WithDuplicateParameter_DoesNotAddDuplicate()
        {
            // Arrange
            var parameter = "test-parameter";

            // Act
            _service.AddImageCompressionJob(parameter);
            _service.AddImageCompressionJob(parameter); // Add same parameter again

            // Assert
            _service.GetCompressionRequestsCount().Should().Be(1);
        }

        [Test]
        public void AddImageCompressionJob_WithNullParameter_DoesNotAddJob()
        {
            // Act
            _service.AddImageCompressionJob((string)null);

            // Assert
            _service.GetCompressionRequestsCount().Should().Be(0);
        }

        [Test]
        public void AddImageCompressionJob_WithEmptyParameter_DoesNotAddJob()
        {
            // Act
            _service.AddImageCompressionJob(string.Empty);

            // Assert
            _service.GetCompressionRequestsCount().Should().Be(0);
        }

        [Test]
        public void AddImageCompressionJob_WithWhitespaceParameter_DoesNotAddJob()
        {
            // Act
            _service.AddImageCompressionJob("   ");

            // Assert
            _service.GetCompressionRequestsCount().Should().Be(0);
        }

        [Test]
        public void AddImageCompressionJob_WithParameterAfterDisposed_DoesNotAddJob()
        {
            // Arrange
            _service.Dispose();

            // Act
            _service.AddImageCompressionJob("test-parameter");

            // Assert
            _service.GetCompressionRequestsCount().Should().Be(0);
        }

        [Test]
        public void AddImageCompressionJob_WithoutParameter_AddsAllImagesJob()
        {
            // Act
            _service.AddImageCompressionJob();

            // Assert
            _service.GetCompressionRequestsCount().Should().Be(1);
        }

        [Test]
        public void AddImageCompressionJob_WithoutParameterMultipleTimes_AddsMultipleJobs()
        {
            // Act
            _service.AddImageCompressionJob();
            _service.AddImageCompressionJob();

            // Assert
            _service.GetCompressionRequestsCount().Should().Be(2);
        }

        [Test]
        public void AddImageCompressionJob_WithoutParameterAfterDisposed_DoesNotAddJob()
        {
            // Arrange
            _service.Dispose();

            // Act
            _service.AddImageCompressionJob();

            // Assert
            _service.GetCompressionRequestsCount().Should().Be(0);
        }

        [Test]
        public async Task GetConsumingImageRequestsAsync_WithAddedRequests_ReturnsAllRequests()
        {
            // Arrange
            var uuid1 = "test-uuid-1";
            var uuid2 = "test-uuid-2";
            _service.AddImageRetrievalJob(uuid1);
            _service.AddImageRetrievalJob(uuid2);
            _service.CompleteImageRequests(); // Complete to stop the enumerable

            // Act
            var results = new List<string>();
            await foreach (var request in _service.GetConsumingImageRequestsAsync())
            {
                results.Add(request);
            }

            // Assert
            results.Should().HaveCount(2);
            results.Should().Contain(uuid1);
            results.Should().Contain(uuid2);
        }

        [Test]
        public async Task GetConsumingImageRequestsAsync_WithNoRequests_ReturnsEmpty()
        {
            // Arrange
            _service.CompleteImageRequests(); // Complete to stop the enumerable

            // Act
            var results = new List<string>();
            await foreach (var request in _service.GetConsumingImageRequestsAsync())
            {
                results.Add(request);
            }

            // Assert
            results.Should().BeEmpty();
        }

        [Test]
        public void GetConsumingImageRequestsAsync_WithCancellation_StopsEnumeration()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            _service.AddImageRetrievalJob("test-uuid");
            cts.CancelAfter(TimeSpan.FromMilliseconds(100));

            // Act & Assert
            var results = new List<string>();
            Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await foreach (var request in _service.GetConsumingImageRequestsAsync(cts.Token))
                {
                    results.Add(request);
                    await Task.Delay(200, cts.Token); // This will be cancelled
                }
            });
        }

        [Test]
        public async Task GetConsumingCompressionRequestsAsync_WithAddedRequests_ReturnsAllRequests()
        {
            // Arrange
            var param1 = "test-param-1";
            var param2 = "test-param-2";
            _service.AddImageCompressionJob(param1);
            _service.AddImageCompressionJob(param2);
            _service.AddImageCompressionJob(); // Add parameterless job
            _service.CompleteCompressionRequests(); // Complete to stop the enumerable

            // Act
            var results = new List<string>();
            await foreach (var request in _service.GetConsumingCompressionRequestsAsync())
            {
                results.Add(request);
            }

            // Assert
            results.Should().HaveCount(3);
            results.Should().Contain(param1);
            results.Should().Contain(param2);
            results.Should().Contain("allImages");
        }

        [Test]
        public async Task GetConsumingCompressionRequestsAsync_WithNoRequests_ReturnsEmpty()
        {
            // Arrange
            _service.CompleteCompressionRequests(); // Complete to stop the enumerable

            // Act
            var results = new List<string>();
            await foreach (var request in _service.GetConsumingCompressionRequestsAsync())
            {
                results.Add(request);
            }

            // Assert
            results.Should().BeEmpty();
        }

        [Test]
        public void GetConsumingCompressionRequestsAsync_WithCancellation_StopsEnumeration()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            _service.AddImageCompressionJob("test-param");
            cts.CancelAfter(TimeSpan.FromMilliseconds(100));

            // Act & Assert
            var results = new List<string>();
            Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await foreach (var request in _service.GetConsumingCompressionRequestsAsync(cts.Token))
                {
                    results.Add(request);
                    await Task.Delay(200, cts.Token); // This will be cancelled
                }
            });
        }

        [Test]
        public void CompleteImageRequests_CompletesChannel_Successfully()
        {
            // Arrange
            _service.AddImageRetrievalJob("test-uuid");

            // Act
            _service.CompleteImageRequests();

            // Assert
            // Completing doesn't affect count immediately, but prevents new items
            _service.GetImageRequestsCount().Should().Be(1);
            
            // Should not be able to add more after completion
            _service.AddImageRetrievalJob("another-uuid");
            _service.GetImageRequestsCount().Should().Be(1); // Still 1, new item not added
        }

        [Test]
        public void CompleteCompressionRequests_CompletesChannel_Successfully()
        {
            // Arrange
            _service.AddImageCompressionJob("test-param");

            // Act
            _service.CompleteCompressionRequests();

            // Assert
            // Completing doesn't affect count immediately, but prevents new items
            _service.GetCompressionRequestsCount().Should().Be(1);
            
            // Should not be able to add more after completion
            _service.AddImageCompressionJob("another-param");
            _service.GetCompressionRequestsCount().Should().Be(1); // Still 1, new item not added
        }

        [Test]
        public void Dispose_CompletesChannelsAndSetsDisposed()
        {
            // Arrange
            _service.AddImageRetrievalJob("test-uuid");
            _service.AddImageCompressionJob("test-param");

            // Act
            _service.Dispose();

            // Assert
            // Should not be able to add new items after disposal
            _service.AddImageRetrievalJob("new-uuid");
            _service.AddImageCompressionJob("new-param");
            _service.AddImageCompressionJob();

            // Counts should remain as they were before disposal
            _service.GetImageRequestsCount().Should().Be(1);
            _service.GetCompressionRequestsCount().Should().Be(1);
        }

        [Test]
        public void Dispose_CalledMultipleTimes_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                _service.Dispose();
                _service.Dispose();
                _service.Dispose();
            });
        }

        [Test]
        public void ThreadSafety_ConcurrentAddImageRetrievalJobs_HandlesCorrectly()
        {
            // Arrange
            const int threadCount = 10;
            const int jobsPerThread = 100;
            var tasks = new Task[threadCount];

            // Act
            for (int i = 0; i < threadCount; i++)
            {
                int threadId = i;
                tasks[i] = Task.Run(() =>
                {
                    for (int j = 0; j < jobsPerThread; j++)
                    {
                        _service.AddImageRetrievalJob($"uuid-{threadId}-{j}");
                    }
                });
            }

            Task.WaitAll(tasks);

            // Assert
            // All unique UUIDs should be added
            _service.GetImageRequestsCount().Should().Be(threadCount * jobsPerThread);
        }

        [Test]
        public void ThreadSafety_ConcurrentAddImageCompressionJobs_HandlesCorrectly()
        {
            // Arrange
            const int threadCount = 10;
            const int jobsPerThread = 100;
            var tasks = new Task[threadCount];

            // Act
            for (int i = 0; i < threadCount; i++)
            {
                int threadId = i;
                tasks[i] = Task.Run(() =>
                {
                    for (int j = 0; j < jobsPerThread; j++)
                    {
                        _service.AddImageCompressionJob($"param-{threadId}-{j}");
                    }
                });
            }

            Task.WaitAll(tasks);

            // Assert
            // All unique parameters should be added
            _service.GetCompressionRequestsCount().Should().Be(threadCount * jobsPerThread);
        }

        [Test]
        public void ThreadSafety_ConcurrentAddAndRemove_HandlesCorrectly()
        {
            // Arrange
            const int operationCount = 1000;
            var addTask = Task.Run(() =>
            {
                for (int i = 0; i < operationCount; i++)
                {
                    _service.AddImageRetrievalJob($"uuid-{i}");
                }
            });

            var removeTask = Task.Run(() =>
            {
                for (int i = 0; i < operationCount / 2; i++)
                {
                    _service.RemoveImageRequest($"uuid-{i}");
                }
            });

            // Act
            Task.WaitAll(addTask, removeTask);

            // Assert
            // Should handle concurrent operations without throwing
            _service.GetImageRequestsCount().Should().BeGreaterThan(0);
        }

        [Test]
        public async Task AsyncEnumerable_MultipleConsumers_CanConsumeSimultaneously()
        {
            // Arrange
            for (int i = 0; i < 10; i++)
            {
                _service.AddImageRetrievalJob($"uuid-{i}");
                _service.AddImageCompressionJob($"param-{i}");
            }
            _service.CompleteImageRequests();
            _service.CompleteCompressionRequests();

            // Act
            var imageTask = Task.Run(async () =>
            {
                var results = new List<string>();
                await foreach (var request in _service.GetConsumingImageRequestsAsync())
                {
                    results.Add(request);
                }
                return results;
            });

            var compressionTask = Task.Run(async () =>
            {
                var results = new List<string>();
                await foreach (var request in _service.GetConsumingCompressionRequestsAsync())
                {
                    results.Add(request);
                }
                return results;
            });

            var imageResults = await imageTask;
            var compressionResults = await compressionTask;

            // Assert
            imageResults.Should().HaveCount(10);
            compressionResults.Should().HaveCount(10);
        }
    }
}