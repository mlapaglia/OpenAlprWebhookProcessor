using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Alerts;
using OpenAlprWebhookProcessor.Alerts.Pushover;
using OpenAlprWebhookProcessor.Alerts.WebPush;
using OpenAlprWebhookProcessor.Features.Alerts;
using OpenAlprWebhookProcessor.Features.Alerts.Commands.AddAlert;
using OpenAlprWebhookProcessor.Features.Alerts.Commands.TestPushover;
using OpenAlprWebhookProcessor.Features.Alerts.Commands.TestWebPush;
using OpenAlprWebhookProcessor.Features.Alerts.Commands.UpsertAlerts;
using OpenAlprWebhookProcessor.Features.Alerts.Commands.UpsertPushover;
using OpenAlprWebhookProcessor.Features.Alerts.Commands.UpsertWebPush;
using OpenAlprWebhookProcessor.Features.Alerts.Queries.GetAlerts;
using OpenAlprWebhookProcessor.Features.Alerts.Queries.GetPushover;
using OpenAlprWebhookProcessor.Features.Alerts.Queries.GetWebPush;
using Tests.TestHelpers;

namespace Tests.Controllers
{
    [TestFixture]
    public class AlertsControllerTests : TestBase
    {
        private AlertsController _controller;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _controller = new AlertsController(Mediator);
        }

        [Test]
        public async Task AddAlert_ValidAlert_ReturnsOk()
        {
            // Arrange
            var alert = TestDataFactory.CreateTestAlert();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _controller.AddAlert(alert, cancellationToken);

            // Assert
            AssertOkResult(result);
            await Mediator.Received(1).Send(Arg.Any<AddAlertCommand>(), cancellationToken);
        }

        [Test]
        public async Task AddAlert_CallsCorrectCommand()
        {
            // Arrange
            var alert = TestDataFactory.CreateTestAlert("TEST123", "Test Description");
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.AddAlert(alert, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<AddAlertCommand>(cmd => 
                    cmd.Alert.PlateNumber == "TEST123" && 
                    cmd.Alert.Description == "Test Description"), 
                cancellationToken);
        }

        [Test]
        public async Task UpsertAlerts_ValidAlerts_ReturnsOk()
        {
            // Arrange
            var alerts = new List<Alert>
            {
                TestDataFactory.CreateTestAlert("ALERT1"),
                TestDataFactory.CreateTestAlert("ALERT2")
            };
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _controller.UpsertAlerts(alerts, cancellationToken);

            // Assert
            AssertOkResult(result);
            await Mediator.Received(1).Send(Arg.Any<UpsertAlertsCommand>(), cancellationToken);
        }

        [Test]
        public async Task UpsertAlerts_CallsCorrectCommand()
        {
            // Arrange
            var alerts = new List<Alert>
            {
                TestDataFactory.CreateTestAlert("ALERT1"),
                TestDataFactory.CreateTestAlert("ALERT2")
            };
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.UpsertAlerts(alerts, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<UpsertAlertsCommand>(cmd => cmd.Alerts.Count == 2), 
                cancellationToken);
        }

        [Test]
        public async Task GetAlerts_ReturnsCorrectResult()
        {
            // Arrange
            var expectedAlerts = new List<Alert>
            {
                TestDataFactory.CreateTestAlert("ALERT1"),
                TestDataFactory.CreateTestAlert("ALERT2")
            };
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetAlertsQuery>(), cancellationToken)
                .Returns(expectedAlerts);

            // Act
            var result = await _controller.GetAlerts(cancellationToken);

            // Assert
            AssertOkResult(result);
            var alerts = GetControllerActionResult<List<Alert>>(result);
            alerts.Should().HaveCount(2);
            alerts[0].PlateNumber.Should().Be("ALERT1");
            alerts[1].PlateNumber.Should().Be("ALERT2");
        }

        [Test]
        public async Task UpsertPushover_ValidRequest_ReturnsOk()
        {
            // Arrange
            var pushoverRequest = new PushoverRequest
            {
                ApiToken = "test-token",
                UserKey = "test-key",
                IsEnabled = true,
                SendEveryPlateEnabled = false,
                SendPlatePreviewEnabled = true
            };
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _controller.UpsertPushover(pushoverRequest, cancellationToken);

            // Assert
            AssertOkResult(result);
            await Mediator.Received(1).Send(Arg.Any<UpsertPushoverCommand>(), cancellationToken);
        }

        [Test]
        public async Task UpsertPushover_CallsCorrectCommand()
        {
            // Arrange
            var pushoverRequest = new PushoverRequest
            {
                ApiToken = "test-token",
                UserKey = "test-key",
                IsEnabled = true
            };
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.UpsertPushover(pushoverRequest, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<UpsertPushoverCommand>(cmd => 
                    cmd.Request.ApiToken == "test-token" && 
                    cmd.Request.UserKey == "test-key"), 
                cancellationToken);
        }

        [Test]
        public async Task TestPushover_ReturnsOk()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _controller.TestPushover(cancellationToken);

            // Assert
            AssertOkResult(result);
            await Mediator.Received(1).Send(Arg.Any<TestPushoverCommand>(), cancellationToken);
        }

        [Test]
        public async Task GetPushover_ReturnsCorrectResult()
        {
            // Arrange
            var expectedRequest = new PushoverRequest
            {
                ApiToken = "test-token",
                UserKey = "test-key",
                IsEnabled = true
            };
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetPushoverQuery>(), cancellationToken)
                .Returns(expectedRequest);

            // Act
            var result = await _controller.GetPushover(cancellationToken);

            // Assert
            AssertOkResult(result);
            var pushoverRequest = GetControllerActionResult<PushoverRequest>(result);
            pushoverRequest.ApiToken.Should().Be("test-token");
            pushoverRequest.UserKey.Should().Be("test-key");
            pushoverRequest.IsEnabled.Should().BeTrue();
        }

        [Test]
        public async Task UpsertWebpush_ValidRequest_ReturnsOk()
        {
            // Arrange
            var webPushRequest = new WebPushRequest
            {
                EmailAddress = "test@example.com",
                PublicKey = "test-public-key",
                PrivateKey = "test-private-key",
                IsEnabled = true
            };
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _controller.UpsertWebpush(webPushRequest, cancellationToken);

            // Assert
            AssertOkResult(result);
            await Mediator.Received(1).Send(Arg.Any<UpsertWebPushCommand>(), cancellationToken);
        }

        [Test]
        public async Task UpsertWebpush_CallsCorrectCommand()
        {
            // Arrange
            var webPushRequest = new WebPushRequest
            {
                EmailAddress = "test@example.com",
                PublicKey = "test-public-key",
                PrivateKey = "test-private-key",
                IsEnabled = true
            };
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.UpsertWebpush(webPushRequest, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<UpsertWebPushCommand>(cmd => 
                    cmd.Request.EmailAddress == "test@example.com" && 
                    cmd.Request.PublicKey == "test-public-key"), 
                cancellationToken);
        }

        [Test]
        public async Task TestWebpush_ReturnsOk()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _controller.TestWebpush(cancellationToken);

            // Assert
            AssertOkResult(result);
            await Mediator.Received(1).Send(Arg.Any<TestWebPushCommand>(), cancellationToken);
        }

        [Test]
        public async Task GetWebpush_ReturnsCorrectResult()
        {
            // Arrange
            var expectedRequest = new WebPushRequest
            {
                EmailAddress = "test@example.com",
                PublicKey = "test-public-key",
                PrivateKey = "test-private-key",
                IsEnabled = true
            };
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetWebPushQuery>(), cancellationToken)
                .Returns(expectedRequest);

            // Act
            var result = await _controller.GetWebpush(cancellationToken);

            // Assert
            AssertOkResult(result);
            var webPushRequest = GetControllerActionResult<WebPushRequest>(result);
            webPushRequest.EmailAddress.Should().Be("test@example.com");
            webPushRequest.PublicKey.Should().Be("test-public-key");
            webPushRequest.IsEnabled.Should().BeTrue();
        }
    }
} 