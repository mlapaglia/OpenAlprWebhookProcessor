using AwesomeAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Alerts.WebPush;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Features.Alerts.Commands.UpsertWebPush;
using Tests.TestHelpers;

namespace Tests.Features.Alerts.Commands
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class UpsertWebPushCommandHandlerTests : TestBase
    {
        private UpsertWebPushCommandHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new UpsertWebPushCommandHandler(UnitOfWork);
        }

        [Test]
        public async Task Handle_WithNoWebPushSettings_CreatesNewSettings()
        {
            // Arrange
            var request = new WebPushRequest
            {
                IsEnabled = true,
                SendEveryPlateEnabled = false,
                EmailAddress = "test@example.com",
                PublicKey = "test-public-key",
                PrivateKey = "test-private-key"
            };
            var command = new UpsertWebPushCommand(request);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var webPushSettings = await UnitOfWork.WebPushSettings.GetAllAsync();
            webPushSettings.Should().HaveCount(1);

            var settings = webPushSettings.First();
            settings.IsEnabled.Should().BeTrue();
            settings.SendEveryPlateEnabled.Should().BeFalse();
            settings.Subject.Should().Be("test@example.com");
        }

        [Test]
        public async Task Handle_WithExistingWebPushSettings_UpdatesExistingSettings()
        {
            // Arrange
            var existingSettings = CreateTestWebPushSettings(
                false,
                true,
                "original@example.com");
            
            await UnitOfWork.WebPushSettings.AddAsync(existingSettings);
            await UnitOfWork.SaveChangesAsync();

            var request = new WebPushRequest
            {
                IsEnabled = true,
                SendEveryPlateEnabled = false,
                EmailAddress = "updated@example.com",
                PublicKey = "updated-public-key",
                PrivateKey = "updated-private-key"
            };
            var command = new UpsertWebPushCommand(request);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var webPushSettings = await UnitOfWork.WebPushSettings.GetAllAsync();
            webPushSettings.Should().HaveCount(1);

            var settings = webPushSettings.First();
            settings.Id.Should().Be(existingSettings.Id); // Should be the same settings
            settings.IsEnabled.Should().BeTrue();
            settings.SendEveryPlateEnabled.Should().BeFalse();
            settings.Subject.Should().Be("updated@example.com");
        }

        [Test]
        public async Task Handle_WithNullEmailAddress_UpdatesWithNullValue()
        {
            // Arrange
            var request = new WebPushRequest
            {
                IsEnabled = true,
                SendEveryPlateEnabled = true,
                EmailAddress = null,
                PublicKey = "test-public-key",
                PrivateKey = "test-private-key"
            };
            var command = new UpsertWebPushCommand(request);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var webPushSettings = await UnitOfWork.WebPushSettings.GetAllAsync();
            webPushSettings.Should().HaveCount(1);

            var settings = webPushSettings.First();
            settings.IsEnabled.Should().BeTrue();
            settings.SendEveryPlateEnabled.Should().BeTrue();
            settings.Subject.Should().BeNull();
        }

        [Test]
        public async Task Handle_WithAllFalseFlags_UpdatesCorrectly()
        {
            // Arrange
            var request = new WebPushRequest
            {
                IsEnabled = false,
                SendEveryPlateEnabled = false,
                EmailAddress = "test@example.com",
                PublicKey = "test-public-key",
                PrivateKey = "test-private-key"
            };
            var command = new UpsertWebPushCommand(request);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var webPushSettings = await UnitOfWork.WebPushSettings.GetAllAsync();
            webPushSettings.Should().HaveCount(1);

            var settings = webPushSettings.First();
            settings.IsEnabled.Should().BeFalse();
            settings.SendEveryPlateEnabled.Should().BeFalse();
        }

        [Test]
        public async Task Handle_WithAllTrueFlags_UpdatesCorrectly()
        {
            // Arrange
            var request = new WebPushRequest
            {
                IsEnabled = true,
                SendEveryPlateEnabled = true,
                EmailAddress = "test@example.com",
                PublicKey = "test-public-key",
                PrivateKey = "test-private-key"
            };
            var command = new UpsertWebPushCommand(request);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var webPushSettings = await UnitOfWork.WebPushSettings.GetAllAsync();
            webPushSettings.Should().HaveCount(1);

            var settings = webPushSettings.First();
            settings.IsEnabled.Should().BeTrue();
            settings.SendEveryPlateEnabled.Should().BeTrue();
        }

        [Test]
        public async Task Handle_UpdateExistingSettingsMultipleTimes_MaintainsSingleSettings()
        {
            // Arrange
            var existingSettings = CreateTestWebPushSettings(
                false,
                false,
                "original@example.com");
            
            await UnitOfWork.WebPushSettings.AddAsync(existingSettings);
            await UnitOfWork.SaveChangesAsync();

            var request1 = new WebPushRequest
            {
                IsEnabled = true,
                SendEveryPlateEnabled = false,
                EmailAddress = "first@example.com"
            };
            var command1 = new UpsertWebPushCommand(request1);

            var request2 = new WebPushRequest
            {
                IsEnabled = false,
                SendEveryPlateEnabled = true,
                EmailAddress = "second@example.com"
            };
            var command2 = new UpsertWebPushCommand(request2);

            // Act
            await _handler.Handle(command1, GetCancellationToken());
            await _handler.Handle(command2, GetCancellationToken());

            // Assert
            var webPushSettings = await UnitOfWork.WebPushSettings.GetAllAsync();
            webPushSettings.Should().HaveCount(1);

            var settings = webPushSettings.First();
            settings.Id.Should().Be(existingSettings.Id); // Should be the same settings
            settings.IsEnabled.Should().BeFalse();
            settings.SendEveryPlateEnabled.Should().BeTrue();
            settings.Subject.Should().Be("second@example.com");
        }

        [Test]
        public async Task Handle_WithEmptyEmailAddress_UpdatesWithEmptyValue()
        {
            // Arrange
            var request = new WebPushRequest
            {
                IsEnabled = true,
                SendEveryPlateEnabled = true,
                EmailAddress = "",
                PublicKey = "test-public-key",
                PrivateKey = "test-private-key"
            };
            var command = new UpsertWebPushCommand(request);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var webPushSettings = await UnitOfWork.WebPushSettings.GetAllAsync();
            webPushSettings.Should().HaveCount(1);

            var settings = webPushSettings.First();
            settings.Subject.Should().Be("");
            settings.IsEnabled.Should().BeTrue();
            settings.SendEveryPlateEnabled.Should().BeTrue();
        }

        private static WebPushSettings CreateTestWebPushSettings(
            bool isEnabled = true,
            bool sendEveryPlateEnabled = false,
            string subject = "test@example.com",
            string publicKey = "test-public-key",
            string privateKey = "test-private-key")
        {
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
    }
}
