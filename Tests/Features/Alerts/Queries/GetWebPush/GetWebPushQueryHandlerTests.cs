using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Features.Alerts.Queries.GetWebPush;
using OpenAlprWebhookProcessor.Features.WebPushSubscriptions.VapidKeys;
using Tests.TestHelpers;

namespace Tests.Features.Alerts.Queries.GetWebPush
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class GetWebPushQueryHandlerTests : TestBase
    {
        private GetWebPushQueryHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new GetWebPushQueryHandler(UnitOfWork);
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public async Task Handle_NoWebPushSettingsExist_CreatesNewSettingsAndReturnsRequest()
        {
            // Arrange
            var query = new GetWebPushQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.PublicKey.Should().NotBeNullOrWhiteSpace();
            result.PrivateKey.Should().NotBeNullOrWhiteSpace();
            result.IsEnabled.Should().BeFalse(); // Default value
            result.SendEveryPlateEnabled.Should().BeFalse(); // Default value
            result.EmailAddress.Should().BeNull(); // Default value

            // Verify that settings were created in the database
            var savedSettings = await Context.WebPushSettings.FirstOrDefaultAsync();
            savedSettings.Should().NotBeNull();
            savedSettings.PublicKey.Should().Be(result.PublicKey);
            savedSettings.PrivateKey.Should().Be(result.PrivateKey);
        }

        [Test]
        public async Task Handle_WebPushSettingsExistWithValidKeys_ReturnsMappedRequest()
        {
            // Arrange
            var webPushSettings = CreateTestWebPushSettings(
                isEnabled: true,
                sendEveryPlateEnabled: true,
                subject: "test@example.com");

            Context.WebPushSettings.Add(webPushSettings);
            await Context.SaveChangesAsync();

            var query = new GetWebPushQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.IsEnabled.Should().Be(webPushSettings.IsEnabled);
            result.SendEveryPlateEnabled.Should().Be(webPushSettings.SendEveryPlateEnabled);
            result.EmailAddress.Should().Be(webPushSettings.Subject);
            result.PublicKey.Should().Be(webPushSettings.PublicKey);
            result.PrivateKey.Should().Be(webPushSettings.PrivateKey);
        }

        [Test]
        public async Task Handle_WebPushSettingsExistWithNullPublicKey_CreatesNewSettingsAndReturnsRequest()
        {
            // Arrange
            var webPushSettings = new WebPushSettings
            {
                Id = Guid.NewGuid(),
                IsEnabled = true,
                Subject = "test@example.com",
                PublicKey = null,
                PrivateKey = null,
                SendEveryPlateEnabled = false
            };

            Context.WebPushSettings.Add(webPushSettings);
            await Context.SaveChangesAsync();

            var query = new GetWebPushQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.PublicKey.Should().NotBeNullOrWhiteSpace();
            result.PrivateKey.Should().NotBeNullOrWhiteSpace();
            // The handler returns the newly created settings, which have default values
            result.IsEnabled.Should().BeFalse(); 
            result.SendEveryPlateEnabled.Should().BeFalse();
            result.EmailAddress.Should().BeNull();

            // Verify that new settings were created (should now have 2 records)
            var allSettings = await Context.WebPushSettings.ToListAsync();
            allSettings.Should().HaveCount(2);
            allSettings.Should().Contain(s => s.PublicKey == result.PublicKey);
        }

        [Test]
        public async Task Handle_WebPushSettingsExistWithEmptyPublicKey_CreatesNewSettingsAndReturnsRequest()
        {
            // Arrange
            var webPushSettings = new WebPushSettings
            {
                Id = Guid.NewGuid(),
                IsEnabled = true,
                Subject = "test@example.com",
                PublicKey = "",
                PrivateKey = "",
                SendEveryPlateEnabled = false
            };

            Context.WebPushSettings.Add(webPushSettings);
            await Context.SaveChangesAsync();

            var query = new GetWebPushQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.PublicKey.Should().NotBeNullOrWhiteSpace();
            result.PrivateKey.Should().NotBeNullOrWhiteSpace();

            // Verify that new settings were created
            var allSettings = await Context.WebPushSettings.ToListAsync();
            allSettings.Should().HaveCount(2);
            allSettings.Should().Contain(s => s.PublicKey == result.PublicKey);
        }

        [Test]
        public async Task Handle_WebPushSettingsExistWithWhitespacePublicKey_CreatesNewSettingsAndReturnsRequest()
        {
            // Arrange
            var webPushSettings = CreateTestWebPushSettings(
                isEnabled: true,
                subject: "test@example.com",
                publicKey: "   ",
                privateKey: "   ");

            Context.WebPushSettings.Add(webPushSettings);
            await Context.SaveChangesAsync();

            var query = new GetWebPushQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.PublicKey.Should().NotBeNullOrWhiteSpace();
            result.PrivateKey.Should().NotBeNullOrWhiteSpace();

            // Verify that new settings were created
            var allSettings = await Context.WebPushSettings.ToListAsync();
            allSettings.Should().HaveCount(2);
            allSettings.Should().Contain(s => s.PublicKey == result.PublicKey);
        }

        [Test]
        public async Task Handle_MultipleWebPushSettingsExist_ReturnsOneOfTheValidSettings()
        {
            // Arrange
            var firstSettings = CreateTestWebPushSettings(
                isEnabled: true,
                subject: "first@example.com");

            var secondSettings = CreateTestWebPushSettings(
                isEnabled: false,
                subject: "second@example.com");

            Context.WebPushSettings.AddRange(firstSettings, secondSettings);
            await Context.SaveChangesAsync();

            var query = new GetWebPushQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.PublicKey.Should().NotBeNullOrWhiteSpace();
            result.PrivateKey.Should().NotBeNullOrWhiteSpace();
            
            // The handler returns one of the existing settings (order not guaranteed)
            // It should match either the first or second settings
            var matchesFirst = result.IsEnabled == firstSettings.IsEnabled && 
                              result.EmailAddress == firstSettings.Subject &&
                              result.PublicKey == firstSettings.PublicKey;
            
            var matchesSecond = result.IsEnabled == secondSettings.IsEnabled && 
                               result.EmailAddress == secondSettings.Subject &&
                               result.PublicKey == secondSettings.PublicKey;
            
            (matchesFirst || matchesSecond).Should().BeTrue("Result should match one of the existing settings");
        }

        private static WebPushSettings CreateTestWebPushSettings(
            bool isEnabled = true,
            bool sendEveryPlateEnabled = false,
            string subject = "test@example.com",
            string publicKey = "GENERATE_KEYS",
            string privateKey = "GENERATE_KEYS")
        {
            // Generate valid VAPID keys if requested
            if (publicKey == "GENERATE_KEYS" || privateKey == "GENERATE_KEYS")
            {
                var vapidKeys = VapidKeyGenerator.GenerateVapidKeys();
                publicKey = publicKey == "GENERATE_KEYS" ? vapidKeys.PublicKey : publicKey;
                privateKey = privateKey == "GENERATE_KEYS" ? vapidKeys.PrivateKey : privateKey;
            }

            return new WebPushSettings
            {
                Id = Guid.NewGuid(),
                IsEnabled = isEnabled,
                SendEveryPlateEnabled = sendEveryPlateEnabled,
                Subject = subject,
                PublicKey = publicKey,
                PrivateKey = privateKey
            };
        }
    }
}