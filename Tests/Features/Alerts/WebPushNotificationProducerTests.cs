using AwesomeAssertions;
using Lib.Net.Http.WebPush;
using Lib.Net.Http.WebPush.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Features.Alerts;
using OpenAlprWebhookProcessor.Features.WebPushSubscriptions;
using OpenAlprWebhookProcessor.Features.WebPushSubscriptions.VapidKeys;
using Tests.TestHelpers;

namespace Tests.Features.Alerts
{
    [TestFixture]
    public class WebPushNotificationProducerTests : TestBase
    {
        private WebPushNotificationProducer _webPushProducer;
        private IWebPushSubscriptionsService _mockSubscriptionsService;
        private IPushServiceClientWrapper _mockPushClient;
        private IServiceProvider _serviceProvider;
        private ILogger<WebPushNotificationProducer> _mockLogger;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _mockSubscriptionsService = Substitute.For<IWebPushSubscriptionsService>();
            _mockPushClient = Substitute.For<IPushServiceClientWrapper>();
            _mockLogger = Substitute.For<ILogger<WebPushNotificationProducer>>();
            
            // Set up service provider
            var services = new ServiceCollection();
            services.AddScoped(provider => UnitOfWork);
            services.AddLogging();
            _serviceProvider = services.BuildServiceProvider();
            
            _webPushProducer = new WebPushNotificationProducer(
                _mockSubscriptionsService,
                _serviceProvider,
                _mockLogger,
                _mockPushClient);
        }

        [TearDown]
        public override void TearDown()
        {
            (_serviceProvider as IDisposable)?.Dispose();
            _webPushProducer.Dispose();
            base.TearDown();
        }

        [Test]
        public async Task SendAlertAsync_WithDisabledSettings_DoesNotSendNotifications()
        {
            // Arrange
            var webPushSettings = CreateTestWebPushSettings(isEnabled: false);
            await SeedWebPushSettingsAsync(webPushSettings);

            var alert = CreateTestAlertUpdateRequest();
            var cancellationToken = GetCancellationToken();

            // Act
            await _webPushProducer.SendAlertAsync(alert, cancellationToken);

            // Assert
            await _mockSubscriptionsService.DidNotReceive().GetAllAsync(Arg.Any<CancellationToken>());
            await _mockPushClient.DidNotReceive().RequestPushMessageDeliveryAsync(
                Arg.Any<PushSubscription>(), 
                Arg.Any<PushMessage>(), 
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task SendAlertAsync_WithNoSettings_DoesNotSendNotifications()
        {
            // Arrange
            var alert = CreateTestAlertUpdateRequest();
            var cancellationToken = GetCancellationToken();

            // Act
            await _webPushProducer.SendAlertAsync(alert, cancellationToken);

            // Assert
            await _mockSubscriptionsService.DidNotReceive().GetAllAsync(Arg.Any<CancellationToken>());
            await _mockPushClient.DidNotReceive().RequestPushMessageDeliveryAsync(
                Arg.Any<PushSubscription>(), 
                Arg.Any<PushMessage>(), 
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task SendAlertAsync_WithNonUrgentAlertAndEveryPlateDisabled_DoesNotSendNotifications()
        {
            // Arrange
            var webPushSettings = CreateTestWebPushSettings(
                isEnabled: true, 
                sendEveryPlateEnabled: false);
            await SeedWebPushSettingsAsync(webPushSettings);

            var alert = CreateTestAlertUpdateRequest(isUrgent: false);
            var cancellationToken = GetCancellationToken();

            // Act
            await _webPushProducer.SendAlertAsync(alert, cancellationToken);

            // Assert
            await _mockSubscriptionsService.DidNotReceive().GetAllAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task SendAlertAsync_WithUrgentAlert_SendsNotifications()
        {
            // Arrange
            var webPushSettings = CreateTestWebPushSettings(
                isEnabled: true,
                sendEveryPlateEnabled: false);
            await SeedWebPushSettingsAsync(webPushSettings);

            var subscriptions = new List<PushSubscription>
            {
                CreateTestPushSubscription("https://endpoint1.example.com"),
                CreateTestPushSubscription("https://endpoint2.example.com")
            };
            _mockSubscriptionsService.GetAllAsync(Arg.Any<CancellationToken>()).Returns(subscriptions);

            var alert = CreateTestAlertUpdateRequest(isUrgent: true);
            var cancellationToken = GetCancellationToken();

            // Act
            await _webPushProducer.SendAlertAsync(alert, cancellationToken);

            // Assert
            await _mockSubscriptionsService.Received(1).GetAllAsync(cancellationToken);
            await _mockPushClient.Received(2).RequestPushMessageDeliveryAsync(
                Arg.Any<PushSubscription>(), 
                Arg.Is<PushMessage>(pm => pm.Content.Contains(alert.PlateNumber)), 
                cancellationToken);
        }

        [Test]
        public async Task SendAlertAsync_WithEveryPlateEnabled_SendsNotifications()
        {
            // Arrange
            var webPushSettings = CreateTestWebPushSettings(
                isEnabled: true,
                sendEveryPlateEnabled: true);
            await SeedWebPushSettingsAsync(webPushSettings);

            var subscriptions = new List<PushSubscription>
            {
                CreateTestPushSubscription("https://endpoint1.example.com")
            };
            _mockSubscriptionsService.GetAllAsync(Arg.Any<CancellationToken>()).Returns(subscriptions);

            var alert = CreateTestAlertUpdateRequest(isUrgent: false);
            var cancellationToken = GetCancellationToken();

            // Act
            await _webPushProducer.SendAlertAsync(alert, cancellationToken);

            // Assert
            await _mockSubscriptionsService.Received(1).GetAllAsync(cancellationToken);
            await _mockPushClient.Received(1).RequestPushMessageDeliveryAsync(
                Arg.Any<PushSubscription>(), 
                Arg.Any<PushMessage>(), 
                cancellationToken);
        }

        [Test]
        public async Task SendAlertAsync_WithPushClientException_LogsErrorAndContinues()
        {
            // Arrange
            var webPushSettings = CreateTestWebPushSettings(isEnabled: true);
            await SeedWebPushSettingsAsync(webPushSettings);

            var subscriptions = new List<PushSubscription>
            {
                CreateTestPushSubscription("https://endpoint1.example.com"),
                CreateTestPushSubscription("https://endpoint2.example.com")
            };
            _mockSubscriptionsService.GetAllAsync(Arg.Any<CancellationToken>()).Returns(subscriptions);

            // Make the first call throw an exception, second should succeed
            _mockPushClient.RequestPushMessageDeliveryAsync(
                    Arg.Is<PushSubscription>(s => s.Endpoint == "https://endpoint1.example.com"),
                    Arg.Any<PushMessage>(),
                    Arg.Any<CancellationToken>())
                .ThrowsAsync(new InvalidOperationException("Push service error"));

            var alert = CreateTestAlertUpdateRequest(isUrgent: true);
            var cancellationToken = GetCancellationToken();

            // Act & Assert - Should not throw
            await _webPushProducer.SendAlertAsync(alert, cancellationToken);

            // Verify both subscriptions were attempted
            await _mockPushClient.Received(2).RequestPushMessageDeliveryAsync(
                Arg.Any<PushSubscription>(),
                Arg.Any<PushMessage>(),
                cancellationToken);
        }

        [Test]
        public async Task SendAlertAsync_WithValidAlert_CallsPushClientWithCorrectParameters()
        {
            // Arrange
            var webPushSettings = CreateTestWebPushSettings(isEnabled: true, sendEveryPlateEnabled: true);
            await SeedWebPushSettingsAsync(webPushSettings);

            var subscriptions = new List<PushSubscription>
            {
                CreateTestPushSubscription("https://endpoint1.example.com")
            };
            _mockSubscriptionsService.GetAllAsync(Arg.Any<CancellationToken>()).Returns(subscriptions);

            var alert = CreateTestAlertUpdateRequest(
                isUrgent: false, // Not urgent, but sendEveryPlateEnabled is true
                plateNumber: "ABC123",
                plateId: System.Guid.NewGuid(),
                plateJpegUrl: "/api/images/crop/test-uuid");
            var cancellationToken = GetCancellationToken();

            // Act
            await _webPushProducer.SendAlertAsync(alert, cancellationToken);

            // Assert
            await _mockPushClient.Received(1).RequestPushMessageDeliveryAsync(
                Arg.Is<PushSubscription>(s => s.Endpoint == "https://endpoint1.example.com"),
                Arg.Any<PushMessage>(),
                cancellationToken);
        }

        [Test]
        public async Task ShouldSendAllPlatesAsync_WithNoSettings_ReturnsFalse()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _webPushProducer.ShouldSendAllPlatesAsync(cancellationToken);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task ShouldSendAllPlatesAsync_WithSendEveryPlateEnabled_ReturnsTrue()
        {
            // Arrange
            var webPushSettings = CreateTestWebPushSettings(sendEveryPlateEnabled: true);
            await SeedWebPushSettingsAsync(webPushSettings);
            
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _webPushProducer.ShouldSendAllPlatesAsync(cancellationToken);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task ShouldSendAllPlatesAsync_WithSendEveryPlateDisabled_ReturnsFalse()
        {
            // Arrange
            var webPushSettings = CreateTestWebPushSettings(sendEveryPlateEnabled: false);
            await SeedWebPushSettingsAsync(webPushSettings);
            
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _webPushProducer.ShouldSendAllPlatesAsync(cancellationToken);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task VerifyCredentialsAsync_WithSettings_DoesNotThrow()
        {
            // Arrange
            var webPushSettings = CreateTestWebPushSettings();
            await SeedWebPushSettingsAsync(webPushSettings);
            
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            Assert.DoesNotThrowAsync(() => _webPushProducer.VerifyCredentialsAsync(cancellationToken));
        }

        [Test]
        public void VerifyCredentialsAsync_WithNoSettings_DoesNotThrow()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            Assert.DoesNotThrowAsync(() => _webPushProducer.VerifyCredentialsAsync(cancellationToken));
        }

        [Test]
        public async Task ExecuteAsync_WithValidSettings_SetsVapidAuthentication()
        {
            // Arrange
            var webPushSettings = CreateTestWebPushSettings(subject: "test@example.com");
            await SeedWebPushSettingsAsync(webPushSettings);

            var stoppingToken = GetCancellationToken();

            // Act
            await CallExecuteAsync(stoppingToken);

            // Assert
            _mockPushClient.Received().DefaultAuthentication = Arg.Any<VapidAuthentication>();
        }

        [Test]
        public async Task ExecuteAsync_WithMissingKeys_GeneratesNewVapidKeys()
        {
            // Arrange - No existing settings
            var stoppingToken = GetCancellationToken();

            // Act
            await CallExecuteAsync(stoppingToken);

            // Assert
            _mockPushClient.Received().DefaultAuthentication = Arg.Is<VapidAuthentication>(auth =>
                !string.IsNullOrEmpty(auth.PublicKey) &&
                !string.IsNullOrEmpty(auth.PrivateKey));

            // Verify keys were saved to database
            using var freshContext = ContextCreator.CreateContext();
            var savedSettings = await freshContext.WebPushSettings.FirstOrDefaultAsync();
            savedSettings.Should().NotBeNull();
            savedSettings.PublicKey.Should().NotBeNullOrEmpty();
            savedSettings.PrivateKey.Should().NotBeNullOrEmpty();
        }

        private static WebPushSettings CreateTestWebPushSettings(
            bool isEnabled = true,
            bool sendEveryPlateEnabled = false,
            string subject = "test@example.com",
            string publicKey = null,
            string privateKey = null)
        {
            // Generate valid VAPID keys if not provided
            if (publicKey == null || privateKey == null)
            {
                var vapidKeys = VapidKeyGenerator.GenerateVapidKeys();
                publicKey ??= vapidKeys.PublicKey;
                privateKey ??= vapidKeys.PrivateKey;
            }

            return new WebPushSettings
            {
                Id = System.Guid.NewGuid(),
                IsEnabled = isEnabled,
                SendEveryPlateEnabled = sendEveryPlateEnabled,
                Subject = subject,
                PublicKey = publicKey,
                PrivateKey = privateKey
            };
        }

        private async Task SeedWebPushSettingsAsync(WebPushSettings webPushSettings)
        {
            Context.WebPushSettings.Add(webPushSettings);
            await Context.SaveChangesAsync();
        }

        private static AlertUpdateRequest CreateTestAlertUpdateRequest(
            bool isUrgent = false,
            string plateNumber = "ABC123",
            System.Guid? plateId = null,
            string plateJpegUrl = "/api/images/crop/test-uuid")
        {
            return new AlertUpdateRequest
            {
                PlateId = plateId ?? System.Guid.NewGuid(),
                PlateNumber = plateNumber,
                Description = "Test alert description",
                IsUrgent = isUrgent,
                PlateJpegUrl = plateJpegUrl,
                ReceivedOn = System.DateTimeOffset.UtcNow
            };
        }

        private static PushSubscription CreateTestPushSubscription(string endpoint = "https://test.example.com")
        {
            return new PushSubscription
            {
                Endpoint = endpoint,
                Keys = new Dictionary<string, string>
                {
                    { "p256dh", "test-p256dh-key" },
                    { "auth", "test-auth-key" }
                }
            };
        }

        private async Task CallExecuteAsync(CancellationToken cancellationToken = default)
        {
            // Use reflection to access the protected ExecuteAsync method
            var method = typeof(WebPushNotificationProducer).GetMethod("ExecuteAsync", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (method != null)
            {
                var task = (Task)method.Invoke(_webPushProducer, new object[] { cancellationToken });
                await task.ConfigureAwait(false);
            }
        }
    }
} 