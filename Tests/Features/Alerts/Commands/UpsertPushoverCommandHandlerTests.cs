using AwesomeAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Features.Alerts;
using OpenAlprWebhookProcessor.Features.Alerts.Commands.UpsertPushover;
using Tests.TestHelpers;

namespace Tests.Features.Alerts.Commands
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class UpsertPushoverCommandHandlerTests : TestBase
    {
        private UpsertPushoverCommandHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new UpsertPushoverCommandHandler(UnitOfWork);
        }

        [Test]
        public async Task Handle_WithNoPushoverClient_CreatesNewClient()
        {
            // Arrange
            var request = new PushoverRequest
            {
                ApiToken = "test-api-token",
                UserKey = "test-user-key",
                IsEnabled = true,
                SendPlatePreviewEnabled = true,
                SendEveryPlateEnabled = false
            };
            var command = new UpsertPushoverCommand(request);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var pushoverClients = await UnitOfWork.PushoverAlertClients.GetAllAsync();
            pushoverClients.Should().HaveCount(1);

            var client = pushoverClients.First();
            client.ApiToken.Should().Be("test-api-token");
            client.UserKey.Should().Be("test-user-key");
            client.IsEnabled.Should().BeTrue();
            client.SendPlatePreview.Should().BeTrue();
            client.SendEveryPlateEnabled.Should().BeFalse();
        }

        [Test]
        public async Task Handle_WithExistingPushoverClient_UpdatesExistingClient()
        {
            // Arrange
            var existingClient = CreateTestPushoverClient(
                "original-api-token",
                "original-user-key",
                false,
                false,
                true);
            
            await UnitOfWork.PushoverAlertClients.AddAsync(existingClient);
            await UnitOfWork.SaveChangesAsync();

            var request = new PushoverRequest
            {
                ApiToken = "updated-api-token",
                UserKey = "updated-user-key",
                IsEnabled = true,
                SendPlatePreviewEnabled = true,
                SendEveryPlateEnabled = false
            };
            var command = new UpsertPushoverCommand(request);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var pushoverClients = await UnitOfWork.PushoverAlertClients.GetAllAsync();
            pushoverClients.Should().HaveCount(1);

            var client = pushoverClients.First();
            client.Id.Should().Be(existingClient.Id); // Should be the same client
            client.ApiToken.Should().Be("updated-api-token");
            client.UserKey.Should().Be("updated-user-key");
            client.IsEnabled.Should().BeTrue();
            client.SendPlatePreview.Should().BeTrue();
            client.SendEveryPlateEnabled.Should().BeFalse();
        }

        [Test]
        public async Task Handle_WithNullApiToken_UpdatesWithNullValue()
        {
            // Arrange
            var request = new PushoverRequest
            {
                ApiToken = null,
                UserKey = "test-user-key",
                IsEnabled = true,
                SendPlatePreviewEnabled = false,
                SendEveryPlateEnabled = true
            };
            var command = new UpsertPushoverCommand(request);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var pushoverClients = await UnitOfWork.PushoverAlertClients.GetAllAsync();
            pushoverClients.Should().HaveCount(1);

            var client = pushoverClients.First();
            client.ApiToken.Should().BeNull();
            client.UserKey.Should().Be("test-user-key");
            client.IsEnabled.Should().BeTrue();
            client.SendPlatePreview.Should().BeFalse();
            client.SendEveryPlateEnabled.Should().BeTrue();
        }

        [Test]
        public async Task Handle_WithNullUserKey_UpdatesWithNullValue()
        {
            // Arrange
            var request = new PushoverRequest
            {
                ApiToken = "test-api-token",
                UserKey = null,
                IsEnabled = false,
                SendPlatePreviewEnabled = true,
                SendEveryPlateEnabled = false
            };
            var command = new UpsertPushoverCommand(request);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var pushoverClients = await UnitOfWork.PushoverAlertClients.GetAllAsync();
            pushoverClients.Should().HaveCount(1);

            var client = pushoverClients.First();
            client.ApiToken.Should().Be("test-api-token");
            client.UserKey.Should().BeNull();
            client.IsEnabled.Should().BeFalse();
            client.SendPlatePreview.Should().BeTrue();
            client.SendEveryPlateEnabled.Should().BeFalse();
        }

        [Test]
        public async Task Handle_WithAllFalseFlags_UpdatesCorrectly()
        {
            // Arrange
            var request = new PushoverRequest
            {
                ApiToken = "test-api-token",
                UserKey = "test-user-key",
                IsEnabled = false,
                SendPlatePreviewEnabled = false,
                SendEveryPlateEnabled = false
            };
            var command = new UpsertPushoverCommand(request);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var pushoverClients = await UnitOfWork.PushoverAlertClients.GetAllAsync();
            pushoverClients.Should().HaveCount(1);

            var client = pushoverClients.First();
            client.IsEnabled.Should().BeFalse();
            client.SendPlatePreview.Should().BeFalse();
            client.SendEveryPlateEnabled.Should().BeFalse();
        }

        [Test]
        public async Task Handle_WithAllTrueFlags_UpdatesCorrectly()
        {
            // Arrange
            var request = new PushoverRequest
            {
                ApiToken = "test-api-token",
                UserKey = "test-user-key",
                IsEnabled = true,
                SendPlatePreviewEnabled = true,
                SendEveryPlateEnabled = true
            };
            var command = new UpsertPushoverCommand(request);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var pushoverClients = await UnitOfWork.PushoverAlertClients.GetAllAsync();
            pushoverClients.Should().HaveCount(1);

            var client = pushoverClients.First();
            client.IsEnabled.Should().BeTrue();
            client.SendPlatePreview.Should().BeTrue();
            client.SendEveryPlateEnabled.Should().BeTrue();
        }

        [Test]
        public async Task Handle_UpdateExistingClientMultipleTimes_MaintainsSingleClient()
        {
            // Arrange
            var existingClient = CreateTestPushoverClient(
                "original-api-token",
                "original-user-key");
            
            await UnitOfWork.PushoverAlertClients.AddAsync(existingClient);
            await UnitOfWork.SaveChangesAsync();

            var request1 = new PushoverRequest
            {
                ApiToken = "first-update",
                UserKey = "first-user",
                IsEnabled = true,
                SendPlatePreviewEnabled = false,
                SendEveryPlateEnabled = true
            };
            var command1 = new UpsertPushoverCommand(request1);

            var request2 = new PushoverRequest
            {
                ApiToken = "second-update",
                UserKey = "second-user",
                IsEnabled = false,
                SendPlatePreviewEnabled = true,
                SendEveryPlateEnabled = false
            };
            var command2 = new UpsertPushoverCommand(request2);

            // Act
            await _handler.Handle(command1, GetCancellationToken());
            await _handler.Handle(command2, GetCancellationToken());

            // Assert
            var pushoverClients = await UnitOfWork.PushoverAlertClients.GetAllAsync();
            pushoverClients.Should().HaveCount(1);

            var client = pushoverClients.First();
            client.Id.Should().Be(existingClient.Id); // Should be the same client
            client.ApiToken.Should().Be("second-update");
            client.UserKey.Should().Be("second-user");
            client.IsEnabled.Should().BeFalse();
            client.SendPlatePreview.Should().BeTrue();
            client.SendEveryPlateEnabled.Should().BeFalse();
        }

        [Test]
        public async Task Handle_WithEmptyStrings_UpdatesWithEmptyValues()
        {
            // Arrange
            var request = new PushoverRequest
            {
                ApiToken = "",
                UserKey = "",
                IsEnabled = true,
                SendPlatePreviewEnabled = true,
                SendEveryPlateEnabled = true
            };
            var command = new UpsertPushoverCommand(request);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var pushoverClients = await UnitOfWork.PushoverAlertClients.GetAllAsync();
            pushoverClients.Should().HaveCount(1);

            var client = pushoverClients.First();
            client.ApiToken.Should().Be("");
            client.UserKey.Should().Be("");
            client.IsEnabled.Should().BeTrue();
            client.SendPlatePreview.Should().BeTrue();
            client.SendEveryPlateEnabled.Should().BeTrue();
        }

        private static Pushover CreateTestPushoverClient(
            string apiToken = "test-api-token",
            string userKey = "test-user-key",
            bool isEnabled = true,
            bool sendPlatePreview = false,
            bool sendEveryPlateEnabled = false)
        {
            return new Pushover
            {
                Id = System.Guid.NewGuid(),
                ApiToken = apiToken,
                UserKey = userKey,
                IsEnabled = isEnabled,
                SendPlatePreview = sendPlatePreview,
                SendEveryPlateEnabled = sendEveryPlateEnabled
            };
        }
    }
}
