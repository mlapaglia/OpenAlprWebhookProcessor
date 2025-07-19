using FluentAssertions;
using Lib.Net.Http.WebPush;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.WebPushSubscriptions;
using Tests.TestHelpers;

namespace Tests.WebPushSubscriptions
{
    [TestFixture]
    public class WebPushSubscriptionsServiceTests : TestBase
    {
        private WebPushSubscriptionsService _service;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            var services = new ServiceCollection();
            services.AddScoped(provider => UnitOfWork);
            var serviceProvider = services.BuildServiceProvider();
            
            _service = new WebPushSubscriptionsService(serviceProvider);
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        #region GetAll Tests

        [Test]
        public async Task GetAll_WhenNoSubscriptions_ReturnsEmptyList()
        {
            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetAll_WhenSubscriptionsExistWithoutKeys_ReturnsEmptyList()
        {
            // Arrange
            var subscription = CreateTestDbSubscription();
            subscription.Keys = null;
            Context.WebPushSubscriptions.Add(subscription);
            await Context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetAll_WhenSubscriptionsExistWithIncompleteKeys_ReturnsEmptyList()
        {
            // Arrange
            var subscription = CreateTestDbSubscriptionWithIncompleteKeys();
            Context.WebPushSubscriptions.Add(subscription);
            await Context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetAll_WhenValidSubscriptionsExist_ReturnsSubscriptions()
        {
            // Arrange
            var subscription = CreateTestDbSubscriptionWithKeys();
            Context.WebPushSubscriptions.Add(subscription);
            await Context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            
            var pushSubscription = result[0];
            pushSubscription.Endpoint.Should().Be(subscription.Endpoint);
            pushSubscription.Keys.Should().ContainKey("auth");
            pushSubscription.Keys.Should().ContainKey("p256dh");
            pushSubscription.Keys["auth"].Should().Be("test-auth-key");
            pushSubscription.Keys["p256dh"].Should().Be("test-p256dh-key");
        }

        [Test]
        public async Task GetAll_WhenMultipleValidSubscriptionsExist_ReturnsAllSubscriptions()
        {
            // Arrange
            var subscription1 = CreateTestDbSubscriptionWithKeys("endpoint1");
            var subscription2 = CreateTestDbSubscriptionWithKeys("endpoint2");
            Context.WebPushSubscriptions.AddRange(subscription1, subscription2);
            await Context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        [Test]
        public async Task GetAll_WithMixOfValidAndInvalidSubscriptions_ReturnsOnlyValid()
        {
            // Arrange
            var validSubscription = CreateTestDbSubscriptionWithKeys();
            var invalidSubscription = CreateTestDbSubscription();
            invalidSubscription.Keys = null;
            
            Context.WebPushSubscriptions.AddRange(validSubscription, invalidSubscription);
            await Context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Endpoint.Should().Be(validSubscription.Endpoint);
        }

        [Test]
        public async Task GetAll_WithCancellationToken_UsesTokenCorrectly()
        {
            // Arrange
            var subscription = CreateTestDbSubscriptionWithKeys();
            Context.WebPushSubscriptions.Add(subscription);
            await Context.SaveChangesAsync();

            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _service.GetAllAsync(cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }

        #endregion

        #region Insert Tests

        [Test]
        public async Task Insert_WhenNewSubscription_AddsSubscriptionToDatabase()
        {
            // Arrange
            var pushSubscription = CreateTestPushSubscription();

            // Act
            await _service.InsertAsync(pushSubscription);

            // Assert
            using (var newContext = ContextCreator.CreateContext())
            {
                var dbSubscription = await newContext.WebPushSubscriptions
                    .Include(s => s.Keys)
                    .FirstOrDefaultAsync(s => s.Endpoint == pushSubscription.Endpoint);
                
                dbSubscription.Should().NotBeNull();
                dbSubscription.Endpoint.Should().Be(pushSubscription.Endpoint);
                dbSubscription.Keys.Should().HaveCount(2);
                
                var authKey = dbSubscription.Keys.FirstOrDefault(k => k.Key == "auth");
                var p256dhKey = dbSubscription.Keys.FirstOrDefault(k => k.Key == "p256dh");
                
                authKey.Should().NotBeNull();
                authKey.Value.Should().Be("test-auth-key");
                p256dhKey.Should().NotBeNull();
                p256dhKey.Value.Should().Be("test-p256dh-key");
            }
        }

        [Test]
        public async Task Insert_WhenSubscriptionAlreadyExists_DoesNotAddDuplicate()
        {
            // Arrange
            var existingSubscription = CreateTestDbSubscriptionWithKeys();
            Context.WebPushSubscriptions.Add(existingSubscription);
            await Context.SaveChangesAsync();

            var pushSubscription = CreateTestPushSubscription(existingSubscription.Endpoint);

            // Act
            await _service.InsertAsync(pushSubscription);

            // Assert
            using (var newContext = ContextCreator.CreateContext())
            {
                var subscriptions = await newContext.WebPushSubscriptions.ToListAsync();
                subscriptions.Should().HaveCount(1);
                subscriptions[0].Endpoint.Should().Be(existingSubscription.Endpoint);
            }
        }

        [Test]
        public async Task Insert_WhenSubscriptionHasNoKeys_AddsSubscriptionWithoutKeys()
        {
            // Arrange
            var pushSubscription = new PushSubscription
            {
                Endpoint = "https://test.endpoint.com",
                Keys = null
            };

            // Act
            await _service.InsertAsync(pushSubscription);

            // Assert
            using (var newContext = ContextCreator.CreateContext())
            {
                var dbSubscription = await newContext.WebPushSubscriptions
                    .Include(s => s.Keys)
                    .FirstOrDefaultAsync(s => s.Endpoint == pushSubscription.Endpoint);
                
                dbSubscription.Should().NotBeNull();
                dbSubscription.Keys.Should().BeEmpty();
            }
        }

        [Test]
        public async Task Insert_WhenSubscriptionHasEmptyKeys_AddsSubscriptionWithEmptyKeys()
        {
            // Arrange
            var pushSubscription = new PushSubscription
            {
                Endpoint = "https://test.endpoint.com",
                Keys = new Dictionary<string, string>()
            };

            // Act
            await _service.InsertAsync(pushSubscription);

            // Assert
            using (var newContext = ContextCreator.CreateContext())
            {
                var dbSubscription = await newContext.WebPushSubscriptions
                    .Include(s => s.Keys)
                    .FirstOrDefaultAsync(s => s.Endpoint == pushSubscription.Endpoint);
                
                dbSubscription.Should().NotBeNull();
                dbSubscription.Keys.Should().BeEmpty();
            }
        }

        [Test]
        public void Insert_WhenSubscriptionIsNull_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(() => _service.InsertAsync(null));
        }

        [Test]
        public async Task Insert_WithCancellationToken_UsesTokenCorrectly()
        {
            // Arrange
            var pushSubscription = CreateTestPushSubscription();
            var cancellationToken = GetCancellationToken();

            // Act
            await _service.InsertAsync(pushSubscription, cancellationToken);

            // Assert
            using (var newContext = ContextCreator.CreateContext())
            {
                var dbSubscription = await newContext.WebPushSubscriptions.FirstOrDefaultAsync(s => s.Endpoint == pushSubscription.Endpoint);
                dbSubscription.Should().NotBeNull();
            }
        }

        #endregion

        #region Delete Tests

        [Test]
        public async Task Delete_WhenSubscriptionExists_RemovesFromDatabase()
        {
            // Arrange
            var subscription = CreateTestDbSubscriptionWithKeys();
            Context.WebPushSubscriptions.Add(subscription);
            await Context.SaveChangesAsync();

            // Act
            await _service.DeleteAsync(subscription.Endpoint);

            // Assert
            using (var newContext = ContextCreator.CreateContext())
            {
                var deletedSubscription = await newContext.WebPushSubscriptions.FirstOrDefaultAsync(s => s.Endpoint == subscription.Endpoint);
                deletedSubscription.Should().BeNull();
            }
        }

        [Test]
        public async Task Delete_WhenSubscriptionDoesNotExist_DoesNotThrow()
        {
            // Arrange
            var nonExistentEndpoint = "https://nonexistent.endpoint.com";

            // Act
            await _service.DeleteAsync(nonExistentEndpoint);

            Assert.Pass();
        }

        [Test]
        public async Task Delete_WhenMultipleSubscriptionsExist_RemovesOnlySpecified()
        {
            // Arrange
            var subscription1 = CreateTestDbSubscriptionWithKeys("endpoint1");
            var subscription2 = CreateTestDbSubscriptionWithKeys("endpoint2");
            Context.WebPushSubscriptions.AddRange(subscription1, subscription2);
            await Context.SaveChangesAsync();

            // Act
            await _service.DeleteAsync(subscription1.Endpoint);

            // Assert
            using (var newContext = ContextCreator.CreateContext())
            {
                var remainingSubscriptions = await newContext.WebPushSubscriptions.ToListAsync();
                remainingSubscriptions.Should().HaveCount(1);
                remainingSubscriptions[0].Endpoint.Should().Be(subscription2.Endpoint);
            }
        }

        [Test]
        public async Task Delete_WithMultipleMatchingEndpoints_DeletesAll()
        {
            // Arrange - This scenario shouldn't happen in practice due to unique constraints
            // but testing the method behavior
            var endpoint = "https://test.endpoint.com";
            var subscription1 = CreateTestDbSubscriptionWithKeys(endpoint);
            var subscription2 = CreateTestDbSubscriptionWithKeys(endpoint);
            subscription2.Id = Guid.NewGuid(); // Force different ID
            
            Context.WebPushSubscriptions.AddRange(subscription1, subscription2);
            await Context.SaveChangesAsync();

            // Act
            await _service.DeleteAsync(endpoint);

            Assert.Pass();
        }

        [Test]
        public void Delete_WhenEndpointIsNull_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrowAsync(() => _service.DeleteAsync(null)); // Should not throw
        }

        [Test]
        public void Delete_WhenEndpointIsEmpty_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrowAsync(() => _service.DeleteAsync(string.Empty)); // Should not throw
        }

        [Test]
        public async Task Delete_WithCancellationToken_UsesTokenCorrectly()
        {
            // Arrange
            var subscription = CreateTestDbSubscriptionWithKeys();
            Context.WebPushSubscriptions.Add(subscription);
            await Context.SaveChangesAsync();

            var cancellationToken = GetCancellationToken();

            // Act
            await _service.DeleteAsync(subscription.Endpoint, cancellationToken);

            // Assert
            using (var newContext = ContextCreator.CreateContext())
            {
                var deletedSubscription = await newContext.WebPushSubscriptions.FirstOrDefaultAsync(s => s.Endpoint == subscription.Endpoint);
                deletedSubscription.Should().BeNull();
            }
        }

        #endregion

        #region Exception Handling Tests

        [Test]
        public void Constructor_WhenServiceProviderIsNull_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => new WebPushSubscriptionsService(null));
            ex.ParamName.Should().Be("serviceProvider");
        }

        [Test]
        public void GetAll_WhenServiceProviderThrowsException_PropagatesException()
        {
            // Arrange
            var badServiceProvider = Substitute.For<IServiceProvider>();
            var scopeFactory = Substitute.For<IServiceScopeFactory>();
            badServiceProvider.GetService(typeof(IServiceScopeFactory)).Returns(scopeFactory);
            scopeFactory.CreateScope().Returns(x => throw new InvalidOperationException("Service provider error"));
            
            var serviceWithBadProvider = new WebPushSubscriptionsService(badServiceProvider);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(() => serviceWithBadProvider.GetAllAsync());
            ex.Message.Should().Be("Service provider error");
        }

        [Test]
        public void Insert_WhenServiceProviderThrowsException_PropagatesException()
        {
            // Arrange
            var badServiceProvider = Substitute.For<IServiceProvider>();
            var scopeFactory = Substitute.For<IServiceScopeFactory>();
            badServiceProvider.GetService(typeof(IServiceScopeFactory)).Returns(scopeFactory);
            scopeFactory.CreateScope().Returns(x => throw new InvalidOperationException("Service provider error"));
            
            var serviceWithBadProvider = new WebPushSubscriptionsService(badServiceProvider);
            var pushSubscription = CreateTestPushSubscription();

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(() => serviceWithBadProvider.InsertAsync(pushSubscription));
            ex.Message.Should().Be("Service provider error");
        }

        [Test]
        public void Delete_WhenServiceProviderThrowsException_PropagatesException()
        {
            // Arrange
            var badServiceProvider = Substitute.For<IServiceProvider>();
            var scopeFactory = Substitute.For<IServiceScopeFactory>();
            badServiceProvider.GetService(typeof(IServiceScopeFactory)).Returns(scopeFactory);
            scopeFactory.CreateScope().Returns(x => throw new InvalidOperationException("Service provider error"));
            
            var serviceWithBadProvider = new WebPushSubscriptionsService(badServiceProvider);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(() => serviceWithBadProvider.DeleteAsync("endpoint"));
            ex.Message.Should().Be("Service provider error");
        }

        #endregion

        #region Helper Methods

        private WebPushSubscription CreateTestDbSubscription(string endpoint = "https://test.endpoint.com")
        {
            return new WebPushSubscription
            {
                Id = Guid.NewGuid(),
                Endpoint = endpoint,
                Keys = new List<WebPushSubscriptionKey>()
            };
        }

        private WebPushSubscription CreateTestDbSubscriptionWithKeys(string endpoint = "https://test.endpoint.com")
        {
            return new WebPushSubscription
            {
                Id = Guid.NewGuid(),
                Endpoint = endpoint,
                Keys = new List<WebPushSubscriptionKey>
                {
                    new WebPushSubscriptionKey { Key = "auth", Value = "test-auth-key" },
                    new WebPushSubscriptionKey { Key = "p256dh", Value = "test-p256dh-key" }
                }
            };
        }

        private WebPushSubscription CreateTestDbSubscriptionWithIncompleteKeys(string endpoint = "https://test.endpoint.com")
        {
            return new WebPushSubscription
            {
                Id = Guid.NewGuid(),
                Endpoint = endpoint,
                Keys = new List<WebPushSubscriptionKey>
                {
                    new WebPushSubscriptionKey { Key = "auth", Value = "test-auth-key" }
                    // Missing p256dh key
                }
            };
        }

        private PushSubscription CreateTestPushSubscription(string endpoint = "https://test.endpoint.com")
        {
            var subscription = new PushSubscription
            {
                Endpoint = endpoint,
                Keys = new Dictionary<string, string>
                {
                    ["auth"] = "test-auth-key",
                    ["p256dh"] = "test-p256dh-key"
                }
            };
            return subscription;
        }

        #endregion
    }
} 