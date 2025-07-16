using FluentAssertions;
using Lib.Net.Http.WebPush;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.WebPushSubscriptions;
using OpenAlprWebhookProcessor.Features.WebPushSubscriptions.Commands.AddWebPushSubscription;
using OpenAlprWebhookProcessor.Features.WebPushSubscriptions.Commands.DeleteWebPushSubscription;
using OpenAlprWebhookProcessor.Features.WebPushSubscriptions.Queries.GetWebPushPublicKey;
using Tests.TestHelpers;

namespace Tests.Controllers
{
    [TestFixture]
    public class WebPushControllersTests : TestBase
    {
        private WebPushSubscriptionsController _subscriptionsController;
        private WebPushPublicKeyController _publicKeyController;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _subscriptionsController = new WebPushSubscriptionsController(Mediator);
            _publicKeyController = new WebPushPublicKeyController(Mediator);
        }

        #region WebPushSubscriptionsController Tests

        [Test]
        public async Task Post_ValidSubscription_ReturnsOk()
        {
            // Arrange
            var subscription = CreateTestPushSubscription();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _subscriptionsController.Post(subscription, cancellationToken);

            // Assert
            result.Should().BeOfType<OkResult>();
            await Mediator.Received(1).Send(
                Arg.Any<AddWebPushSubscriptionCommand>(), 
                cancellationToken);
        }

        [Test]
        public async Task Post_CallsCorrectCommand()
        {
            // Arrange
            var subscription = CreateTestPushSubscription();
            var cancellationToken = GetCancellationToken();

            // Act
            await _subscriptionsController.Post(subscription, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<AddWebPushSubscriptionCommand>(cmd => 
                    cmd.Subscription == subscription), 
                cancellationToken);
        }

        [Test]
        public async Task Post_NullSubscription_ReturnsOk()
        {
            // Arrange
            PushSubscription subscription = null;
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _subscriptionsController.Post(subscription, cancellationToken);

            // Assert
            result.Should().BeOfType<OkResult>();
            await Mediator.Received(1).Send(
                Arg.Is<AddWebPushSubscriptionCommand>(cmd => 
                    cmd.Subscription == null), 
                cancellationToken);
        }

        [Test]
        public async Task Delete_ValidEndpoint_ReturnsOk()
        {
            // Arrange
            var endpoint = "https://fcm.googleapis.com/fcm/send/test-subscription-id";
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _subscriptionsController.Delete(endpoint, cancellationToken);

            // Assert
            result.Should().BeOfType<OkResult>();
            await Mediator.Received(1).Send(
                Arg.Any<DeleteWebPushSubscriptionCommand>(), 
                cancellationToken);
        }

        [Test]
        public async Task Delete_CallsCorrectCommand()
        {
            // Arrange
            var endpoint = "https://fcm.googleapis.com/fcm/send/test-subscription-id";
            var cancellationToken = GetCancellationToken();

            // Act
            await _subscriptionsController.Delete(endpoint, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<DeleteWebPushSubscriptionCommand>(cmd => 
                    cmd.Endpoint == endpoint), 
                cancellationToken);
        }

        [Test]
        public async Task Delete_NullEndpoint_ReturnsOk()
        {
            // Arrange
            string endpoint = null;
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _subscriptionsController.Delete(endpoint, cancellationToken);

            // Assert
            result.Should().BeOfType<OkResult>();
            await Mediator.Received(1).Send(
                Arg.Is<DeleteWebPushSubscriptionCommand>(cmd => 
                    cmd.Endpoint == null), 
                cancellationToken);
        }

        [Test]
        public async Task Delete_EmptyEndpoint_ReturnsOk()
        {
            // Arrange
            var endpoint = "";
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _subscriptionsController.Delete(endpoint, cancellationToken);

            // Assert
            result.Should().BeOfType<OkResult>();
            await Mediator.Received(1).Send(
                Arg.Is<DeleteWebPushSubscriptionCommand>(cmd => 
                    cmd.Endpoint == ""), 
                cancellationToken);
        }

        [Test]
        public async Task Post_SubscriptionWithAllProperties_CallsCorrectCommand()
        {
            // Arrange
            var subscription = CreateTestPushSubscriptionWithAllProperties();
            var cancellationToken = GetCancellationToken();

            // Act
            await _subscriptionsController.Post(subscription, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<AddWebPushSubscriptionCommand>(cmd => 
                    cmd.Subscription.Endpoint == subscription.Endpoint &&
                    cmd.Subscription.Keys == subscription.Keys), 
                cancellationToken);
        }

        [Test]
        public async Task Delete_LongEndpointUrl_ReturnsOk()
        {
            // Arrange
            var endpoint = "https://very-long-endpoint-url.googleapis.com/fcm/send/very-long-subscription-id-with-many-characters-to-test-edge-cases";
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _subscriptionsController.Delete(endpoint, cancellationToken);

            // Assert
            result.Should().BeOfType<OkResult>();
            await Mediator.Received(1).Send(
                Arg.Is<DeleteWebPushSubscriptionCommand>(cmd => 
                    cmd.Endpoint == endpoint), 
                cancellationToken);
        }

        [Test]
        public async Task Post_MultipleSubscriptions_EachCallsCommand()
        {
            // Arrange
            var subscription1 = CreateTestPushSubscription();
            var subscription2 = CreateTestPushSubscription();
            var cancellationToken = GetCancellationToken();

            // Act
            await _subscriptionsController.Post(subscription1, cancellationToken);
            await _subscriptionsController.Post(subscription2, cancellationToken);

            // Assert
            await Mediator.Received(2).Send(
                Arg.Any<AddWebPushSubscriptionCommand>(), 
                cancellationToken);
        }

        [Test]
        public async Task Delete_MultipleEndpoints_EachCallsCommand()
        {
            // Arrange
            var endpoint1 = "https://fcm.googleapis.com/fcm/send/test-subscription-id-1";
            var endpoint2 = "https://fcm.googleapis.com/fcm/send/test-subscription-id-2";
            var cancellationToken = GetCancellationToken();

            // Act
            await _subscriptionsController.Delete(endpoint1, cancellationToken);
            await _subscriptionsController.Delete(endpoint2, cancellationToken);

            // Assert
            await Mediator.Received(2).Send(
                Arg.Any<DeleteWebPushSubscriptionCommand>(), 
                cancellationToken);
        }

        #endregion

        #region WebPushPublicKeyController Tests

        [Test]
        public async Task Get_ReturnsContentResultWithPublicKey()
        {
            // Arrange
            var expectedPublicKey = "BM8XWHRnIHNWXmCrmkgWrYQw1XkGlGNgKhxglLr_2QYThKV9QcvEGMNfMrAc2JbOzNNrqNhTKyJJCCgDG8r2j1k";
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetWebPushPublicKeyQuery>(), cancellationToken)
                .Returns(expectedPublicKey);

            // Act
            var result = await _publicKeyController.Get(cancellationToken);

            // Assert
            result.Should().BeOfType<ContentResult>();
            var contentResult = result as ContentResult;
            contentResult.Content.Should().Be(expectedPublicKey);
            contentResult.ContentType.Should().Be("text/plain");
        }

        [Test]
        public async Task Get_CallsCorrectQuery()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetWebPushPublicKeyQuery>(), cancellationToken)
                .Returns("test-public-key");

            // Act
            await _publicKeyController.Get(cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Any<GetWebPushPublicKeyQuery>(), 
                cancellationToken);
        }

        [Test]
        public async Task Get_NullPublicKey_ReturnsContentResultWithNull()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetWebPushPublicKeyQuery>(), cancellationToken)
                .Returns((string)null);

            // Act
            var result = await _publicKeyController.Get(cancellationToken);

            // Assert
            result.Should().BeOfType<ContentResult>();
            var contentResult = result as ContentResult;
            contentResult.Content.Should().BeNull();
            contentResult.ContentType.Should().Be("text/plain");
        }

        [Test]
        public async Task Get_EmptyPublicKey_ReturnsContentResultWithEmpty()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetWebPushPublicKeyQuery>(), cancellationToken)
                .Returns("");

            // Act
            var result = await _publicKeyController.Get(cancellationToken);

            // Assert
            result.Should().BeOfType<ContentResult>();
            var contentResult = result as ContentResult;
            contentResult.Content.Should().Be("");
            contentResult.ContentType.Should().Be("text/plain");
        }

        [Test]
        public async Task Get_ReturnsCorrectContentType()
        {
            // Arrange
            var publicKey = "test-public-key";
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetWebPushPublicKeyQuery>(), cancellationToken)
                .Returns(publicKey);

            // Act
            var result = await _publicKeyController.Get(cancellationToken);

            // Assert
            result.Should().BeOfType<ContentResult>();
            var contentResult = result as ContentResult;
            contentResult.ContentType.Should().Be("text/plain");
        }

        [Test]
        public async Task Get_MultipleCallsInSequence_EachCallsQuery()
        {
            // Arrange
            var publicKey = "test-public-key";
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetWebPushPublicKeyQuery>(), cancellationToken)
                .Returns(publicKey);

            // Act
            await _publicKeyController.Get(cancellationToken);
            await _publicKeyController.Get(cancellationToken);
            await _publicKeyController.Get(cancellationToken);

            // Assert
            await Mediator.Received(3).Send(
                Arg.Any<GetWebPushPublicKeyQuery>(), 
                cancellationToken);
        }

        [Test]
        public async Task Get_LongPublicKey_ReturnsContentResultWithLongKey()
        {
            // Arrange
            var longPublicKey = "BM8XWHRnIHNWXmCrmkgWrYQw1XkGlGNgKhxglLr_2QYThKV9QcvEGMNfMrAc2JbOzNNrqNhTKyJJCCgDG8r2j1k_additional_very_long_key_data_to_test_edge_cases";
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetWebPushPublicKeyQuery>(), cancellationToken)
                .Returns(longPublicKey);

            // Act
            var result = await _publicKeyController.Get(cancellationToken);

            // Assert
            result.Should().BeOfType<ContentResult>();
            var contentResult = result as ContentResult;
            contentResult.Content.Should().Be(longPublicKey);
            contentResult.ContentType.Should().Be("text/plain");
        }

        [Test]
        public async Task Get_PublicKeyWithSpecialCharacters_ReturnsContentResultWithSpecialChars()
        {
            // Arrange
            var publicKeyWithSpecialChars = "BM8XWHRnIHNWXmCrmkgWrYQw1XkGlGNgKhxglLr_2QYThKV9QcvEGMNfMrAc2JbOzNNrqNhTKyJJCCgDG8r2j1k+/=";
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetWebPushPublicKeyQuery>(), cancellationToken)
                .Returns(publicKeyWithSpecialChars);

            // Act
            var result = await _publicKeyController.Get(cancellationToken);

            // Assert
            result.Should().BeOfType<ContentResult>();
            var contentResult = result as ContentResult;
            contentResult.Content.Should().Be(publicKeyWithSpecialChars);
            contentResult.ContentType.Should().Be("text/plain");
        }

        #endregion

        #region Helper Methods

        private PushSubscription CreateTestPushSubscription()
        {
            return new PushSubscription
            {
                Endpoint = "https://fcm.googleapis.com/fcm/send/test-subscription-id"
            };
        }

        private PushSubscription CreateTestPushSubscriptionWithAllProperties()
        {
            return new PushSubscription
            {
                Endpoint = "https://fcm.googleapis.com/fcm/send/test-subscription-id",
                Keys = new Dictionary<string, string>
                {
                    { "p256dh", "BM8XWHRnIHNWXmCrmkgWrYQw1XkGlGNgKhxglLr_2QYThKV9QcvEGMNfMrAc2JbOzNNrqNhTKyJJCCgDG8r2j1k" },
                    { "auth", "Q_4A7g9QQXJjPHGgJIGQpw" }
                }
            };
        }

        #endregion
    }
} 