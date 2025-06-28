using Microsoft.Extensions.Logging;
using NSubstitute;
using OpenAlprWebhookProcessor.Server.WebhookProcessor.OpenAlprWebsocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.WebhookProcessor
{
    internal class WebsocketClientOrganizerTests
    {
        private readonly ILogger<WebsocketClientOrganizer> _logger;

        private readonly WebsocketClientOrganizer _websocketClientOrganizer;

        public WebsocketClientOrganizerTests()
        {
            _logger = Substitute.For<ILogger<WebsocketClientOrganizer>>();
            _websocketClientOrganizer = new WebsocketClientOrganizer(_logger);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _websocketClientOrganizer.Dispose();
        }

        [Test]
        public void WebsocketClientConstructs()
        {
            var organizer = new WebsocketClientOrganizer(_logger);
            Assert.That(organizer, Is.Not.Null);
        }

        [Test]
        public async Task AddingNewWebsocketClient_ISuccessful()
        {
            var agentId = Guid.NewGuid().ToString();
            var websocketClient = Substitute.For<IOpenAlprWebsocketClient>();
            var addResult = await _websocketClientOrganizer.AddAgentAsync(
                agentId,
                websocketClient,
                CancellationToken.None);

            Assert.That(addResult.WasAdded, Is.True);
            Assert.That(addResult.WasUpdated, Is.False);
        }

        [Test]
        public async Task RemovingExistingWebsocketClient_IsSuccessful()
        {
            var agentId = Guid.NewGuid().ToString();
            var websocketClient = Substitute.For<IOpenAlprWebsocketClient>();
            await _websocketClientOrganizer.AddAgentAsync(
                agentId,
                websocketClient,
                CancellationToken.None);

            var removeResult = await _websocketClientOrganizer.RemoveAgentAsync(
                agentId,
                CancellationToken.None);

            Assert.That(removeResult, Is.True);
        }
    }
}
