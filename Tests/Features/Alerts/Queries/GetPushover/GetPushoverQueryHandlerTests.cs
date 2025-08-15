using AwesomeAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Features.Alerts.Queries.GetPushover;
using Tests.TestHelpers;

namespace Tests.Features.Alerts.Queries.GetPushover
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class GetPushoverQueryHandlerTests : TestBase
    {
        private GetPushoverQueryHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new GetPushoverQueryHandler(UnitOfWork);
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public async Task Handle_NoPushoverClientExists_ReturnsEmptyPushoverRequest()
        {
            // Arrange
            var query = new GetPushoverQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.ApiToken.Should().BeNull();
            result.UserKey.Should().BeNull();
            result.IsEnabled.Should().BeFalse();
            result.SendPlatePreviewEnabled.Should().BeFalse();
            result.SendEveryPlateEnabled.Should().BeFalse();
        }

        [Test]
        public async Task Handle_PushoverClientExists_ReturnsMappedPushoverRequest()
        {
            // Arrange
            var pushoverClient = CreateTestPushoverClient();

            Context.PushoverAlertClients.Add(pushoverClient);
            await Context.SaveChangesAsync();

            var query = new GetPushoverQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.ApiToken.Should().Be(pushoverClient.ApiToken);
            result.UserKey.Should().Be(pushoverClient.UserKey);
            result.IsEnabled.Should().Be(pushoverClient.IsEnabled);
            result.SendPlatePreviewEnabled.Should().Be(pushoverClient.SendPlatePreview);
            result.SendEveryPlateEnabled.Should().Be(pushoverClient.SendEveryPlateEnabled);
        }

        [Test]
        public async Task Handle_MultiplePushoverClientsExist_ReturnsOneOfTheClients()
        {
            // Arrange
            var firstClient = CreateTestPushoverClient(
                apiToken: "first-token",
                userKey: "first-user",
                isEnabled: true);

            var secondClient = CreateTestPushoverClient(
                apiToken: "second-token",
                userKey: "second-user",
                isEnabled: false);

            Context.PushoverAlertClients.AddRange(firstClient, secondClient);
            await Context.SaveChangesAsync();

            var query = new GetPushoverQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            // GetFirstAsync() doesn't guarantee order, so we should accept either client
            result.ApiToken.Should().BeOneOf("first-token", "second-token");
            if (result.ApiToken == "first-token")
            {
                result.UserKey.Should().Be("first-user");
                result.IsEnabled.Should().BeTrue();
            }
            else
            {
                result.UserKey.Should().Be("second-user");
                result.IsEnabled.Should().BeFalse();
            }
        }

        [Test]
        public async Task Handle_DisabledPushoverClient_ReturnsDisabledRequest()
        {
            // Arrange
            var pushoverClient = CreateTestPushoverClient(isEnabled: false);

            Context.PushoverAlertClients.Add(pushoverClient);
            await Context.SaveChangesAsync();

            var query = new GetPushoverQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.IsEnabled.Should().BeFalse();
            result.ApiToken.Should().Be(pushoverClient.ApiToken);
            result.UserKey.Should().Be(pushoverClient.UserKey);
        }

        [Test]
        public async Task Handle_PushoverClientWithAllFeaturesEnabled_ReturnsMappedRequest()
        {
            // Arrange
            var pushoverClient = CreateTestPushoverClient(
                isEnabled: true,
                sendPlatePreview: true,
                sendEveryPlateEnabled: true);

            Context.PushoverAlertClients.Add(pushoverClient);
            await Context.SaveChangesAsync();

            var query = new GetPushoverQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.IsEnabled.Should().BeTrue();
            result.SendPlatePreviewEnabled.Should().BeTrue();
            result.SendEveryPlateEnabled.Should().BeTrue();
        }

        private static Pushover CreateTestPushoverClient(
            bool isEnabled = true,
            string apiToken = "test-api-token",
            string userKey = "test-user-key",
            bool sendPlatePreview = false,
            bool sendEveryPlateEnabled = false)
        {
            return new Pushover
            {
                Id = Guid.NewGuid(),
                IsEnabled = isEnabled,
                ApiToken = apiToken,
                UserKey = userKey,
                SendPlatePreview = sendPlatePreview,
                SendEveryPlateEnabled = sendEveryPlateEnabled
            };
        }
    }
}