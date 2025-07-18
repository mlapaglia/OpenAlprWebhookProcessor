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
            var result = await _service.GetAllAsync(default);

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
            var result = await _service.GetAllAsync(default);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetAll_WhenSubscriptionsExistWithEmptyKeys_ReturnsEmptyList()
        {
            // Arrange
            var subscription = CreateTestDbSubscription();
            subscription.Keys = new List<WebPushSubscriptionKey>();
            Context.WebPushSubscriptions.Add(subscription);
            await Context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllAsync(default);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetAll_WhenSingleSubscriptionWithKeys_ReturnsSingleSubscription()
        {
            // Arrange
            var subscription = CreateTestDbSubscriptionWithKeys();
            Context.WebPushSubscriptions.Add(subscription);
            await Context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllAsync(default);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            
            var pushSubscription = result.First();
            pushSubscription.Endpoint.Should().Be(subscription.Endpoint);
            pushSubscription.Keys.Should().HaveCount(2);
            pushSubscription.Keys.Should().ContainKey("auth");
            pushSubscription.Keys.Should().ContainKey("p256dh");
            pushSubscription.Keys["auth"].Should().Be("test-auth-key");
            pushSubscription.Keys["p256dh"].Should().Be("test-p256dh-key");
        }

        [Test]
        public async Task GetAll_WhenMultipleSubscriptionsWithKeys_ReturnsAllSubscriptions()
        {
            // Arrange
            var subscription1 = CreateTestDbSubscriptionWithKeys("endpoint1");
            var subscription2 = CreateTestDbSubscriptionWithKeys("endpoint2");
            
            Context.WebPushSubscriptions.AddRange(subscription1, subscription2);
            await Context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllAsync(default);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            
            var endpoints = result.Select(s => s.Endpoint).ToList();
            endpoints.Should().Contain("endpoint1");
            endpoints.Should().Contain("endpoint2");
        }

        [Test]
        public async Task GetAll_WhenMixedSubscriptionsWithAndWithoutKeys_ReturnsOnlySubscriptionsWithKeys()
        {
            // Arrange
            var subscriptionWithKeys = CreateTestDbSubscriptionWithKeys("endpoint1");
            var subscriptionWithoutKeys = CreateTestDbSubscription("endpoint2");
            subscriptionWithoutKeys.Keys = null;
            
            Context.WebPushSubscriptions.AddRange(subscriptionWithKeys, subscriptionWithoutKeys);
            await Context.SaveChangesAsync(default);

            // Act
            var result = await _service.GetAllAsync(default);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().Endpoint.Should().Be("endpoint1");
        }

        [Test]
        public async Task GetAll_WhenSubscriptionHasIncompleteKeys_SkipsSubscription()
        {
            // Arrange
            var subscription = CreateTestDbSubscription();
            subscription.Keys = new List<WebPushSubscriptionKey>
            {
                new WebPushSubscriptionKey { Key = "auth", Value = "test-auth-key" }
                // Missing p256dh key
            };
            Context.WebPushSubscriptions.Add(subscription);
            await Context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllAsync(default);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region Insert Tests

        [Test]
        public async Task Insert_WhenNewSubscription_AddsSubscriptionToDatabaseAsync()
        {
            // Arrange
            var pushSubscription = CreateTestPushSubscription();

            // Act
            await _service.InsertAsync(pushSubscription, default);

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
        public async Task Insert_WhenSubscriptionAlreadyExists_DoesNotAddDuplicateAsync()
        {
            // Arrange
            var existingSubscription = CreateTestDbSubscriptionWithKeys();
            Context.WebPushSubscriptions.Add(existingSubscription);
            await Context.SaveChangesAsync();

            var pushSubscription = CreateTestPushSubscription(existingSubscription.Endpoint);

            // Act
            await _service.InsertAsync(pushSubscription, default);

            // Assert
            using (var newContext = ContextCreator.CreateContext())
            {
                var subscriptions = await newContext.WebPushSubscriptions.ToListAsync();
                subscriptions.Should().HaveCount(1);
                subscriptions.First().Endpoint.Should().Be(existingSubscription.Endpoint);
            }
        }

        [Test]
        public async Task Insert_WhenSubscriptionWithoutKeys_AddsSubscriptionWithEmptyKeysAsync()
        {
            // Arrange
            var pushSubscription = new PushSubscription
            {
                Endpoint = "https://test.endpoint.com"
            };

            // Act
            await _service.InsertAsync(pushSubscription, default);

            // Assert
            using (var newContext = ContextCreator.CreateContext())
            {
                var dbSubscription = await newContext.WebPushSubscriptions
                    .Include(s => s.Keys)
                    .FirstOrDefaultAsync(s => s.Endpoint == pushSubscription.Endpoint);
                
                dbSubscription.Should().NotBeNull();
                dbSubscription.Endpoint.Should().Be(pushSubscription.Endpoint);
                dbSubscription.Keys.Should().BeEmpty();
            }
        }

        [Test]
        public async Task Insert_WhenSubscriptionWithCustomKeys_AddsAllKeysAsync()
        {
            // Arrange
            var pushSubscription = new PushSubscription
            {
                Endpoint = "https://test.endpoint.com",
                Keys = new Dictionary<string, string>
                {
                    { "auth", "custom-auth-key" },
                    { "p256dh", "custom-p256dh-key" },
                    { "custom", "custom-value" }
                }
            };

            // Act
            await _service.InsertAsync(pushSubscription, default);

            // Assert
            using (var newContext = ContextCreator.CreateContext())
            {
                var dbSubscription = await newContext.WebPushSubscriptions
                    .Include(s => s.Keys)
                    .FirstOrDefaultAsync(s => s.Endpoint == pushSubscription.Endpoint);
                
                dbSubscription.Should().NotBeNull();
                dbSubscription.Keys.Should().HaveCount(3);
                
                var keys = dbSubscription.Keys.ToDictionary(k => k.Key, k => k.Value);
                keys["auth"].Should().Be("custom-auth-key");
                keys["p256dh"].Should().Be("custom-p256dh-key");
                keys["custom"].Should().Be("custom-value");
            }
        }

        [Test]
        public void Insert_WhenNullSubscription_ThrowsException()
        {
            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(() => _service.InsertAsync(null, default));
        }

        #endregion

        #region Delete Tests

        [Test]
        public async Task Delete_WhenSubscriptionExists_RemovesSubscriptionFromDatabase()
        {
            // Arrange
            var subscription = CreateTestDbSubscriptionWithKeys();
            Context.WebPushSubscriptions.Add(subscription);
            await Context.SaveChangesAsync();

            // Act
            await _service.DeleteAsync(subscription.Endpoint, default);

            // Assert
            using (var newContext = ContextCreator.CreateContext())
            {
                var deletedSubscription = await newContext.WebPushSubscriptions
                    .FirstOrDefaultAsync(s => s.Endpoint == subscription.Endpoint);
                deletedSubscription.Should().BeNull();
            }
        }

        [Test]
        public async Task Delete_WhenSubscriptionDoesNotExist_DoesNothing()
        {
            // Arrange
            var nonExistentEndpoint = "https://non-existent.endpoint.com";

            // Act
            await _service.DeleteAsync(nonExistentEndpoint, default);

            // Assert
            // Should not throw any exception
            using (var newContext = ContextCreator.CreateContext())
            {
                var subscriptions = await newContext.WebPushSubscriptions.ToListAsync();
                subscriptions.Should().BeEmpty();
            }
        }

        [Test]
        public async Task Delete_WhenSubscriptionExistsWithKeys_RemovesSubscriptionAndKeys()
        {
            // Arrange
            var subscription = CreateTestDbSubscriptionWithKeys();
            Context.WebPushSubscriptions.Add(subscription);
            await Context.SaveChangesAsync();

            using (var checkContext = ContextCreator.CreateContext())
            {
                var keyCount = await checkContext.WebPushSubscriptionKeys.CountAsync();
                keyCount.Should().Be(2);
            }

            // Act
            await _service.DeleteAsync(subscription.Endpoint, default);

            // Assert
            using (var newContext = ContextCreator.CreateContext())
            {
                var deletedSubscription = await newContext.WebPushSubscriptions
                    .FirstOrDefaultAsync(s => s.Endpoint == subscription.Endpoint);
                deletedSubscription.Should().BeNull();

                // Keys should be cascade deleted
                var remainingKeys = await newContext.WebPushSubscriptionKeys.ToListAsync();
                remainingKeys.Should().BeEmpty();
            }
        }

        [Test]
        public async Task Delete_WhenMultipleSubscriptionsExist_RemovesOnlySpecifiedSubscription()
        {
            // Arrange
            var subscription1 = CreateTestDbSubscriptionWithKeys("endpoint1");
            var subscription2 = CreateTestDbSubscriptionWithKeys("endpoint2");
            
            Context.WebPushSubscriptions.AddRange(subscription1, subscription2);
            await Context.SaveChangesAsync();

            // Act
            await _service.DeleteAsync("endpoint1", default);

            // Assert
            using (var newContext = ContextCreator.CreateContext())
            {
                var remainingSubscriptions = await newContext.WebPushSubscriptions.ToListAsync();
                remainingSubscriptions.Should().HaveCount(1);
                remainingSubscriptions.First().Endpoint.Should().Be("endpoint2");
            }
        }

        [Test]
        public async Task Delete_WhenNullEndpoint_DoesNothingAsync()
        {
            // Arrange
            var subscription = CreateTestDbSubscriptionWithKeys();
            Context.WebPushSubscriptions.Add(subscription);
            await Context.SaveChangesAsync();

            // Act
            await _service.DeleteAsync(null, default);

            // Assert
            using (var newContext = ContextCreator.CreateContext())
            {
                var subscriptions = await newContext.WebPushSubscriptions.ToListAsync();
                subscriptions.Should().HaveCount(1);
            }
        }

        [Test]
        public async Task Delete_WhenEmptyEndpoint_DoesNothingAsync()
        {
            // Arrange
            var subscription = CreateTestDbSubscriptionWithKeys();
            Context.WebPushSubscriptions.Add(subscription);
            await Context.SaveChangesAsync();

            // Act
            await _service.DeleteAsync(string.Empty, default);

            // Assert
            using (var newContext = ContextCreator.CreateContext())
            {
                var subscriptions = await newContext.WebPushSubscriptions.ToListAsync();
                subscriptions.Should().HaveCount(1);
            }
        }

        #endregion

        #region Service Provider Tests

        [Test]
        public void Constructor_WhenServiceProviderIsNull_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => new WebPushSubscriptionsService(null));
            ex.ParamName.Should().Be("serviceProvider");
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
            var ex = Assert.ThrowsAsync<InvalidOperationException>(() => serviceWithBadProvider.InsertAsync(pushSubscription, default));
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
            var ex = Assert.ThrowsAsync<InvalidOperationException>(() => serviceWithBadProvider.DeleteAsync("endpoint", default));
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
            var subscription = CreateTestDbSubscription(endpoint);
            subscription.Keys = new List<WebPushSubscriptionKey>
            {
                new WebPushSubscriptionKey
                {
                    Id = Guid.NewGuid(),
                    Key = "auth",
                    Value = "test-auth-key",
                    MobilePushSubscription = subscription
                },
                new WebPushSubscriptionKey
                {
                    Id = Guid.NewGuid(),
                    Key = "p256dh",
                    Value = "test-p256dh-key",
                    MobilePushSubscription = subscription
                }
            };
            
            // Set the foreign key reference
            foreach (var key in subscription.Keys)
            {
                key.MobilePushSubscriptionId = subscription.Id;
            }
            
            return subscription;
        }

        private PushSubscription CreateTestPushSubscription(string endpoint = "https://test.endpoint.com")
        {
            return new PushSubscription
            {
                Endpoint = endpoint,
                Keys = new Dictionary<string, string>
                {
                    { "auth", "test-auth-key" },
                    { "p256dh", "test-p256dh-key" }
                }
            };
        }

        #endregion
    }
} 