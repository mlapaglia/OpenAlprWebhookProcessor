using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebsocket;
using Tests.TestHelpers;

namespace Tests.Features.Webhooks.WebhookProcessor.OpenAlprWebsocket
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class WebSocketClientOrganizerHostedServiceTests : TestBase
    {
        private WebsocketClientOrganizerHostedService _hostedService;
        private IWebsocketClientOrganizer _mockWebsocketClientOrganizer;
        private ILogger<WebsocketClientOrganizerHostedService> _mockLogger;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _mockWebsocketClientOrganizer = Substitute.For<IWebsocketClientOrganizer>();
            _mockLogger = Substitute.For<ILogger<WebsocketClientOrganizerHostedService>>();
            _hostedService = new WebsocketClientOrganizerHostedService(_mockWebsocketClientOrganizer, _mockLogger);
        }

        [TearDown]
        public override void TearDown()
        {
            _hostedService?.Dispose();
            base.TearDown();
        }

        [Test]
        public void Constructor_NullWebsocketClientOrganizer_ThrowsArgumentNullException()
        {
            // Act & Assert
            FluentActions.Invoking(() => new WebsocketClientOrganizerHostedService(null, _mockLogger))
                .Should().Throw<ArgumentNullException>()
                .WithParameterName("websocketClientOrganizer");
        }

        [Test]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            FluentActions.Invoking(() => new WebsocketClientOrganizerHostedService(_mockWebsocketClientOrganizer, null))
                .Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }












    }
}
